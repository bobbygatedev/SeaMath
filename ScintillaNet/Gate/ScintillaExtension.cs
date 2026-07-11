using Gate.Tools;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ScintillaNET.Gate
{
   /// <summary>
   /// In order to minimize change on 3rd party sw scintilla extension class has been introduced.
   /// </summary>
   public class ScintillaExtension : Scintilla
   {
      /// <summary>
      /// Not present in newer version of scintila.net
      /// </summary>
      public const int SCI_SETILEXER = 4033;

      private static bool myIsStarted = false;
      private static nint myDllHandler = nint.Zero;

      private Lexer myLexer = Lexer.Null;

      public delegate IntPtr CreateLexerHandler(IntPtr lexerName);

      public delegate IntPtr LexerNameFromIDHandler(int lexerId);

      private static CreateLexerHandler? myCreateLexerHandler;
      private static LexerNameFromIDHandler? myLexerNameFromIDHandler;

      public ScintillaExtension()
      {

      }

      public static IntPtr CreateLexer(string lexerName)
      {
         var p = Marshal.StringToHGlobalAnsi(lexerName);
         var res = (myCreateLexerHandler ?? throw new Crash())(p);

         Marshal.FreeHGlobal(p);

         return res;
      }

      public static string? LexerNameFromID(int lexerId) => myLexerNameFromIDHandler != null ?
         Marshal.PtrToStringAnsi(myLexerNameFromIDHandler(lexerId)) : null;

      public new Lexer Lexer
      {
         get => myLexer;
         set
         {
            myLexer = value;

            if (value != Lexer.Null)
            {
               var lex_nam = LexerNameFromID((int)(myLexer = value));

               if (lex_nam != null)
               {
                  var lex = CreateLexer(lex_nam);

                  DirectMessage(SCI_SETILEXER, IntPtr.Zero, lex);
               }
            }
         }
      }

      public new Encoding Encoding => base.Encoding;

      public static bool CheckDll()
      {
         if (!myIsStarted)
         {
            myIsStarted = true;
            SetModulePath(SciLexerDllPath);
            myDllHandler = NativeMethods.LoadLibrary(SciLexerDllPath);
         }

         return myDllHandler != nint.Zero;
      }

      /// <summary>
      /// 
      /// </summary>
      protected override CreateParams CreateParams
      {
         get
         {
            if (!CheckDll()) { return new CreateParams(); }

            try
            {
               myCreateLexerHandler = Marshal.GetDelegateForFunctionPointer(
                   myGetProcAddress(myDllHandler, "CreateLexer"),
                   typeof(CreateLexerHandler)) as CreateLexerHandler ?? throw new Crash();

               myLexerNameFromIDHandler = Marshal.GetDelegateForFunctionPointer(
                  myGetProcAddress(myDllHandler, "LexerNameFromID"),
                   typeof(LexerNameFromIDHandler)) as LexerNameFromIDHandler ?? throw new Crash();
            }
            catch (Exception exc)
            {
               throw new Crash($"Failed to get function for dll {SciLexerDllPath}", exc);
            }

            return base.CreateParams;
         }
      }

      private nint myGetProcAddress(nint hnd, string lpProcName)
      {
         var add = NativeMethods.GetProcAddress(new HandleRef(this, hnd), lpProcName);

         if (add == IntPtr.Zero)
         {
            var err = Marshal.GetLastWin32Error();

            throw new Win32Exception(err, $"GetProcAddress failed for '{lpProcName}' in '{hnd}'");
         }
         else
         {
            return add;
         }
      }

      public static string SciLexerDllPath
      {
         get
         {
            var nam = Assembly.GetCallingAssembly()?.Location ?? throw new Crash();
            var dir = new FileInfo(nam).Directory ?? throw new Crash();

            return dir.Exists ? Path.Combine(dir.FullName, "Scilexer.dll") : throw new Crash();
         }
      }

      /// <summary>
      /// This mask the bug.
      /// </summary>
      /// <param name="path"></param>
      /// <param name="encoding"></param>
      public void OpenFile(string path, Encoding? encoding = null)
      {
         ReadOnly = false;

         using (var sr = new StreamReader(path, encoding ?? Encoding.Default)) { Text = sr.ReadToEnd(); }

         Lines.RebuildLineData();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="lineIdx">Line index (1-line_count) </param>
      public void ClearAnnotationOnLine(int lineIdx) => DirectMessage(NativeMethods.SCI_ANNOTATIONSETTEXT, new IntPtr(lineIdx - 1), IntPtr.Zero);

      public unsafe new string SelectedText
      {
         get
         {
            var len = DirectMessage(NativeMethods.SCI_GETSELTEXT).ToInt32();

            if (len <= 0) { return ""; }

            var bytes = new byte[len + 1];

            fixed (byte* bp = bytes)
            {
               DirectMessage(NativeMethods.SCI_GETSELTEXT, IntPtr.Zero, new IntPtr(bp));

               var enc = Encoding;
               var str = new string((sbyte*)bp, 0, len, enc);

               return str;
            }
         }
      }
   }
}
