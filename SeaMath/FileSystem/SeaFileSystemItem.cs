using Gate.CLanguage;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Console;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;
using System.Text;
using static Gate.CLanguage.Types.BuiltIns.CTypeBinChar;

namespace Gate.SeaMath.FileSystem
{
   /// <summary>
   /// Represents a stream (of file or devuce) within a hierarchical sea file system.
   /// </summary>
   /// <remarks><see cref="SeaFileSystemItem"/> provides a base abstraction for files managed by a <see
   /// cref="SeaFileSystem"/>.  It exposes file metadata and supports operations common to files in a hierarchical
   /// structure.</remarks>
   public unsafe abstract class SeaFileSystemItem : HierarchicalItem
   {
      public enum InOutErrType
      {
         None = 0,
         StdIn = -1,
         StdOut = -2,
         StdErr = -3
      }

      public enum FSeekWhence
      {
         SEEK_SET = 0,
         SEEK_CUR = 1,
         SEEK_END = 2
      }

      private static int myFileCounter = 0;

      private byte[]? myEnqueuedNarrowChars = null;
      private int[]? myEnqueuedWideChars = null;
      private readonly List<int> myListAlias = new List<int>();

      protected SeaFileSystemItem(
         FileMode mode, FileAccess access, RtmDbgEngVirtCpuProcess? processBound, CompiledStdio compiledStdio)
      {
         Mode = mode;
         Access = access;
         ProcessBound = (processBound ?? RtmDbgEngVirtCpuThread.GetRunningThread()?.Process) as SeaMathProcess ?? throw new Crash();
         CompiledStdio = compiledStdio;
         CUniversalStdio = new CUniversalStdio(compiledStdio);
      }

      public CUniversalStdio CUniversalStdio { get; }

      public CompiledStdio? CompiledStdio { get; }

      public abstract InOutErrType InOutErr { get; }

      public abstract FileInfo? FileInfo { get; }

      public abstract Stream Stream { get; }

      public abstract bool IsEndOfStream { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract void PerformCloseOperation();

      public SeaFileSystem? FileSystem => ParentItem as SeaFileSystem;

      public FileMode Mode { get; }

      public FileAccess Access { get; }

      public SeaMathProcess ProcessBound { get; }

      /// <summary>
      /// Returns the next byte from the stream, or null if end of stream is reached.
      /// </summary>
      /// <returns></returns>
      public byte? Getc()
      {
         var nar_byt = myEnqueuedNarrowChars?.FirstOrDefault();

         if (nar_byt.HasValue)
         {
            myEnqueuedNarrowChars = myEnqueuedNarrowChars?.Skip(1).ToArray();

            return nar_byt;
         }
         else
         {
            var buf = new byte[1];
            var nb = Stream.Read(buf, 0, buf.Length);

            return nb < 1 ? null : (byte?)buf[0];
         }
      }

      public unsafe int? GetcW(SeaRtmStrategy rtmStrategy)
      {
         var wid_byt = myEnqueuedWideChars?.FirstOrDefault();

         if (wid_byt.HasValue)
         {
            myEnqueuedWideChars = myEnqueuedWideChars?.Skip(1).ToArray();

            if (myEnqueuedWideChars == null || myEnqueuedWideChars.Length == 0)
            {
               myEnqueuedWideChars = null;
            }

            return wid_byt;
         }
         else
         {
            var in_bys = GetNextInputNarrowChars(false, false);
            var str = rtmStrategy.Settings.NarrowCharEncoding.GetString(in_bys ?? []);

            if (str.Length > 0)
            {
               //wide string bytes
               var wnb = rtmStrategy.Settings.WideCharEncoding.GetBytes(str);

               //wide char length
               var wcl = rtmStrategy.Settings.WideCharEncoding.GetStringWideEncodingBytes();

               //wide char count
               var nwc = wnb.Length / wcl;

               //old length
               var old_len = myEnqueuedWideChars == null ? 0 : myEnqueuedWideChars.Length;

               //update enqueued wide chars
               myEnqueuedWideChars = myEnqueuedWideChars == null ?
                  new int[nwc] :
                  myEnqueuedWideChars.Concat(new int[nwc]).ToArray();

               for (var i = 0; i < nwc; i++)
               {
                  var w_ch = 0;

                  for (int j = 0; j < wcl; j++)
                  {
                     ((byte*)&w_ch)[j] = wnb[(i * wcl) + j];
                  }

                  myEnqueuedWideChars[old_len + i] = w_ch;
               }

               //recursive call to get the first wide char
               return GetcW(rtmStrategy);
            }
            else
            {
               return null;
            }
         }
      }

      public unsafe int ScanfWide(IntPtr format, SeaRtmStrategy rtmStrategy, object[] @params)
      {
         var frm_pos = 0;
         var cnt = 0;
         //sizeof of wide-char
         var nw = rtmStrategy.Settings.WideCharEncoding.GetStringWideEncodingBytes();

         var tmp_siz = 1024;
         var tmp = stackalloc int[tmp_siz];

         while (true)
         {
            var frm_str = myGetNextFormat(format, ref frm_pos, nw);

            if (frm_str != IntPtr.Zero)
            {
               var wid_chs = ReadNextInputWideChars(true, true, rtmStrategy);

               if (wid_chs != null)
               {
                  wid_chs = wid_chs.Append(0).ToArray();

                  if (wid_chs.Length + 1 > tmp_siz)
                  {
                     throw new Gate.LangBase.Runtime.RtmException($"wscanf internal buffer capacity exceeded!");
                  }

                  wid_chs.CopyWideCharsToBuffer(tmp, rtmStrategy.Settings.WideCharEncoding);

                  var ret = CUniversalStdio.WSscanf(
                     tmp, (void*)frm_str, rtmStrategy.Settings.WideCharEncoding, @params.Skip(cnt).ToArray());

                  if (ret == 0)
                  {
                     myReEnqueueWideChars(wid_chs.Take(wid_chs.Length - 1).ToArray());

                     return cnt;
                  }
                  else
                  {
                     cnt++;
                  }
               }
               else
               {
                  return cnt == 0 ? -1 : cnt;
               }
            }
            else { return cnt; }
         }
      }

      private void myReEnqueueWideChars(int[] wideChars) =>
         myEnqueuedWideChars = myEnqueuedWideChars == null ? wideChars : myEnqueuedWideChars.Concat(wideChars).ToArray();

      public unsafe int Scanf(IntPtr format, SeaRtmStrategy rtmStrategy, object[] @params)
      {
         var frm_pos = 0;
         var cnt = 0;

         if (CompiledStdio != null)
         {
            while (true)
            {
               var frm_str = myGetNextFormat(format, ref frm_pos);

               if (frm_str != IntPtr.Zero)
               {
                  var inp_bys = GetNextInputNarrowChars(true, true);

                  if (inp_bys != null)
                  {
                     //adds null terminator
                     inp_bys = inp_bys.Append((byte)0).ToArray();

                     fixed (byte* buffer = inp_bys)
                     {
                        var ret = CompiledStdio.DoSscanf((sbyte*)buffer, (sbyte*)frm_str, @params.Skip(cnt).ToArray());

                        if (ret == 0)
                        {
                           myReEnqueueNarrowChars(inp_bys.Take(inp_bys.Length - 1).ToArray());

                           return cnt;
                        }
                        else
                        {
                           cnt++;
                        }
                     }
                  }
                  else
                  {
                     return cnt == 0 ? -1 : cnt;
                  }
               }
               else { return cnt; }
            }
         }

         return -1;
      }

      public static T[]? myReadNextInputChars<T>(bool isForScanf, bool isContinue, Func<T?> Getc) where T : struct, IConvertible
      {
         var lst = new List<T>();
         var val = null as T?;

         if (isForScanf)
         {
            if (!isContinue) { throw new Crash(); }

            //moves to next non-newline char (scanf only)
            while (true)
            {
               val = Getc();

               if (val == null)
               {
                  return null;//Ctrl+D
               }
               else if (char.IsWhiteSpace(val.Value.ToChar(null)))
               {
                  return myReadNextInputChars(isForScanf, isContinue, Getc);//restarts
               }
               else
               {
                  break;
               }
            }
         }
         else
         {
            val = Getc();
         }

         while (true)
         {
            if (val == null)
            {
               if (lst.Count == 0)
               {
                  return null;//EOF
               }
               else
               {
                  return lst.ToArray();
               }
            }
            else
            {
               if (isForScanf)
               {
                  //ends search
                  if (char.IsWhiteSpace(val.Value.ToChar(null)))
                  {
                     return lst.ToArray();
                  }
                  else
                  {
                     lst.Add(val.Value);
                  }
               }
               else
               {
                  lst.Add(val.Value);

                  if (val.Value.ToChar(null) == '\n' && !isContinue)
                  {
                     return lst.ToArray();//ends search
                  }
               }
            }

            val = Getc();//re-read next char
         }
      }

      /// <summary>
      /// Reads the next sequence of input bytes up to and including the next whitespace character or end of input.
      /// </summary>
      /// <remarks>Consecutive newline characters are skipped. If the input begins with a newline, the
      /// method restarts and continues reading from the next non-newline character. The returned array will always
      /// include the first encountered whitespace character that terminates the sequence, unless the end of input is
      /// reached first.</remarks>
      /// <param name="isForScanf"></param>
      /// <param name="isContinue"></param>
      /// <returns></returns>
      public byte[]? GetNextInputNarrowChars(bool isForScanf, bool isContinue) =>
         myReadNextInputChars(isForScanf, isContinue, Getc);

      /// <summary>
      /// Reads from console input next sequence of input bytes up to and including the next whitespace character or end of input.
      /// </summary>
      /// <remarks>Returns an array of int[] even if the input is 16-bit wide characters.</remarks>
      /// <param name="isForScanf"></param>
      /// <param name="isContinue"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      public int[]? ReadNextInputWideChars(bool isForScanf, bool isContinue, SeaRtmStrategy rtmStrategy) =>
         myReadNextInputChars(isForScanf, isContinue, () => GetcW(rtmStrategy));

      public unsafe int Printf(SeaRtmStrategy rtmStrategy, sbyte* format, object[] @params)
      {
         try
         {
            if (CompiledStdio != null)
            {
               var buf = new byte[2048];

               fixed (byte* bp = buf)
               {
                  var res = CompiledStdio.DoSprintf((sbyte*)bp, buf.Length, format, @params);

                  Stream.Write(buf, 0, res);

                  return res;
               }
            }

            return -1;
         }
         catch (Exception exc) { throw new Gate.LangBase.Runtime.RtmException($"Memory error during printf: {exc.Message}"); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmStrategy"></param>
      /// <param name="format"></param>
      /// <param name="params"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public unsafe int PrintfW(SeaRtmStrategy rtmStrategy, void* format, params object[] @params)
      {
         try
         {
            var buf = new byte[2048];

            fixed (byte* bp = buf)
            {
               var sb = new StringBuilder();
               var res = CUniversalStdio.WSPrintf(sb, format, rtmStrategy.Settings.WideCharEncoding, @params);

               var nar_bys = myEncodeWideToNarrow(rtmStrategy, (IntPtr)bp);

               nar_bys = rtmStrategy.Settings.NarrowCharEncoding.GetBytes(sb.ToString());

               Stream.Write(nar_bys, 0, nar_bys.Length);

               return res;
            }
         }
         catch { throw new Gate.LangBase.Runtime.RtmException($"Memory error during sprintf"); }
      }

      public void Print(string message, SeaRtmStrategy rtmStrategy)
      {
         var bys = rtmStrategy.Settings.NarrowCharEncoding.GetBytes(message);

         Stream.Write(bys, 0, bys.Length);
      }

      public unsafe sbyte* Gets(sbyte* buffer, bool isNewLineToRemove = true)
      {
         var bys = GetNextInputNarrowChars(false, false);

         if (bys != null)
         {
            if (isNewLineToRemove)
            {
               bys = bys.RemoveNewLine() ?? [];
            }

            bys.CopyNarrowBytesToNullTerminated(buffer);

            return buffer;
         }
         else
         {
            return null;
         }
      }

      public unsafe RtmObj SeaGets(SeaRtmStrategy rtmStrategy, bool isNewLineToRemove = true)
      {
         var bys = GetNextInputNarrowChars(false, false);

         if (bys == null) { return new SeaTypeRtmObj(rtmStrategy.Allocator); }
         else
         {
            if (isNewLineToRemove)
            {
               bys = bys.RemoveNewLine() ?? [];
            }

            var typ = rtmStrategy.Settings?.BuiltInSet?["char"] ?? throw new Crash();
            var out_ali = CTypeAlias.Make(typ, bys.Length + 1);
            var out_arr = new CRtmObjArray(rtmStrategy, out_ali, [bys.Length + 1]);

            out_arr.StringEncoding = rtmStrategy.Settings.NarrowCharEncoding;
            bys.CopyNarrowBytesToNullTerminated((void*)(out_arr.Address ?? throw new Crash()));

            return out_arr;
         }
      }

      public unsafe RtmObj SeaWGets(SeaRtmStrategy rtmStrategy, bool isNewLineToRemove = true)
      {
         var wid_chs = ReadNextInputWideChars(false, false, rtmStrategy) ?? [];

         if (wid_chs == null) { return new SeaTypeRtmObj(rtmStrategy.Allocator); }
         else
         {
            if (isNewLineToRemove)
            {
               wid_chs = wid_chs.RemoveNewLine() ?? [];
            }

            var bs = rtmStrategy.Settings.BuiltInSet;
            var typ = bs?.OfType<WChar>().FirstOrDefaultUnique() ?? throw new Crash();
            var out_ali = CTypeAlias.Make(typ, wid_chs.Length + 1);
            var out_arr = new CRtmObjArray(rtmStrategy, out_ali, [wid_chs.Length + 1]);
            var ptr = out_arr.Address ?? throw new Crash();

            out_arr.StringEncoding = rtmStrategy.Settings.WideCharEncoding;
            wid_chs.CopyWideCharsToBuffer((void*)ptr, rtmStrategy.Settings.WideCharEncoding);

            return out_arr;
         }
      }

      public unsafe void* WGets(void* buffer, SeaRtmStrategy rtmStrategy, bool isNewLineToRemove = true)
      {
         var wid_chs = ReadNextInputWideChars(false, false, rtmStrategy);

         if (wid_chs != null)
         {
            if (isNewLineToRemove)
            {
               wid_chs = wid_chs.RemoveNewLine() ?? [];
            }

            wid_chs.CopyWideCharsToBuffer(buffer, rtmStrategy.Settings.WideCharEncoding);

            return buffer;
         }
         else
         {
            return null;
         }
      }

      public long DoFSeek(long offset, FSeekWhence whence)
      {
         var sj = null as SeekOrigin?;

         switch (whence)
         {
            case FSeekWhence.SEEK_SET:
               sj = SeekOrigin.Begin;
               break;

            case FSeekWhence.SEEK_CUR:
               sj = SeekOrigin.Current;
               break;

            case FSeekWhence.SEEK_END:
               sj = SeekOrigin.End;
               break;

            default: throw new Gate.LangBase.Runtime.RtmException($"Invalid whence value {whence}");
         }

         return Stream.Seek(offset, sj.Value);
      }

      public unsafe int Puts(sbyte* @string, Encoding narrowCharEncoding)
      {
         try
         {
            var len = GeneralizedString.GetNullTerminatedLength((IntPtr)@string, 1);
            var buf = new byte[len + 1];

            buf[len] = (byte)'\n';

            Stream.Write(buf, 0, buf.Length);

            return buf.Length;
         }
         catch { throw new Gate.LangBase.Runtime.RtmException($"Memory error during sprintf"); }
      }

      public unsafe int PutsWide(SeaRtmStrategy rtmStrategy, void* @string)
      {
         try
         {
            var len = ((IntPtr)@string).GetStringWideNtLen(rtmStrategy.Settings.WideCharEncoding);
            var bl = rtmStrategy.Settings.WideCharEncoding.GetStringWideEncodingBytes();
            var buf = new byte[len * bl];

            //append write line
            buf[len * bl] = (byte)'\n';

            Stream.Write(buf, 0, buf.Length);

            return buf.Length;
         }
         catch { throw new Gate.LangBase.Runtime.RtmException($"Memory error during sprintf"); }
      }

      public bool IsUnbound { get; set; } = false;

      private static unsafe byte[] myEncodeWideToNarrow(SeaRtmStrategy rtmStrategy, IntPtr wideBuffer)
      {
         //number of bytes per wide char
         var nb = rtmStrategy.Settings.WideCharEncoding.GetStringWideEncodingBytes();

         //number of unterminated wide chars
         var nl = GeneralizedString.GetNullTerminatedLength(wideBuffer, nb);

         //convert to string 
         var tmp_str = rtmStrategy.Settings.WideCharEncoding.GetString((byte*)wideBuffer, nl * nb);

         return rtmStrategy.Settings.WideCharEncoding.GetBytes(tmp_str.ToCharArray());
      }

      private void myReEnqueueNarrowChars(byte[] bytes) =>
         myEnqueuedNarrowChars = myEnqueuedNarrowChars == null ? bytes : myEnqueuedNarrowChars.Concat(bytes).ToArray();

      private static unsafe IntPtr myGetNextFormat(IntPtr format, ref int pos, int nb)
      {
         try
         {
            while (true)
            {
               var byt = myGetNextInt(format, ref pos, nb);

               if (byt == 0 || byt == '\n')
               {
                  return IntPtr.Zero;
               }
               else if (!char.IsWhiteSpace((char)byt))
               {
                  break;
               }
            }

            var ret = (IntPtr)((sbyte*)format + (pos - nb));

            while (true)
            {
               var byt = myGetNextInt(format, ref pos, nb);

               if (byt == 0 || char.IsWhiteSpace((char)byt))
               {
                  return ret;
               }
            }
         }
         catch (NullReferenceException)
         {
            throw new Gate.LangBase.Runtime.RtmException($"Null pointer");
         }
      }

      private static unsafe int myGetNextInt(IntPtr format, ref int pos, int nb)
      {
         var byt = 0;

         for (var i = 0; i < nb; i++)
         {
            *((byte*)&byt + i) = ((byte*)format)[pos++];
         }

         return byt;
      }

      private unsafe IntPtr myGetNextFormat(IntPtr format, ref int pos) => myGetNextFormat(format, ref pos, 1);

      /// <summary>
      /// 
      /// </summary>
      public int[] Aliases => myListAlias.ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fd"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      public int AddAlias(int? fd = null)
      {
         if (!fd.HasValue)
         {
            fd = ++myFileCounter;
         }

         myListAlias.Add(fd.Value);
         myCheckAlias();

         return fd.Value;
      }

      protected override void myActionOnParentSet(HierarchicalItem parentItem)
      {
         base.myActionOnParentSet(parentItem);

         myCheckAlias();
      }

      private void myCheckAlias()
      {
         if (
            FileSystem != null &&
            FileSystem.AllFiles.Where(f => f.ProcessBound != ProcessBound).SelectMany(f => f.Aliases).Intersect(Aliases).Any())
         {
            throw new Crash();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileId"></param>
      /// <exception cref="Crash"></exception>
      public void RemoveAlias(int fileId)
      {
         if (myListAlias.Contains(fileId))
         {
            myListAlias.Remove(fileId);

            if (myListAlias.Count == 0)
            {
               PerformCloseOperation();
            }
         }
         else
         {
            throw new Crash();
         }
      }

      public override string ToString() => $"{FileInfo} {InOutErr} {string.Join(",", Aliases)} {ProcessBound}";
   }
}
