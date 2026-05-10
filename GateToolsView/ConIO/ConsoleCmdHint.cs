using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Properties;
using System.Text.RegularExpressions;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// 
   /// </summary>
   public class ConsoleCmdHint
   {
      public enum ImageType
      {
         none = 0,
         type = 1,
         var = 2,
         func = 3,
         typedef = 4,
         enum_label = 5,
         define = 6,
      }

      private ImageType myImageId;
      private Image? myImage;

      /// <summary>
      /// 
      /// </summary>
      public ConsoleCmdHint()
      {
      }

      public class Handler
      {
         public Handler(ConsoleCmdHint cmdHint)
         {
            CmdHint = cmdHint;
            Fields = myFindFields();
         }

         public class Field
         {
            private static readonly Regex myRegex = new Regex(@"\[([^\]]+)\]");

            public string? Name => Match?.Groups[1].Value;

            public ConsoleCmdHint? ParentHint { get; private set; }

            public Match? Match { get; private set; }

            public static Field? Find(ConsoleCmdHint cmdHint, ref int index)
            {
               var mat = myRegex.Match(cmdHint.HintFormat.ExtTrim(), index);

               if (mat.Success)
               {
                  index = mat.Index + mat.Length;

                  return new Field() { Match = mat, ParentHint = cmdHint };
               }
               else
               {
                  return null;
               }
            }
         }

         private Field[] myFindFields()
         {
            var txt = CmdHint.HintFormat;
            var idx = 0;
            var lst = new List<Field>();

            while (true)
            {
               var fld = Field.Find(CmdHint, ref idx);

               if (fld == null) { break; }
               else
               {
                  lst.Add(fld);
               }
            }

            return lst.ToArray();
         }

         public int GetIndexFilter(string line)
         {
            var fld = Fields.ElementAtOrDefault(FieldIndex);
            var idx = line.IndexOf(fld?.Match?.Value ?? "");

            return idx;
         }

         public ConsoleCmdHint CmdHint { get; }

         public Field[] Fields { get; }

         public ConsoleIo? ConsoleIo { get; private set; }

         public int StartLinePos { get; set; }

         public int FieldIndex { get; set; }
      }

      public ImageType ImageId
      {
         get => myImageId;

         set
         {
            myImageId = value;
            myImage = myGetImageForType(value);
         }
      }

      public Image? Image { get => myImage; }


      public string? HintId { get; set; }

      /// <summary>
      /// eg double sin(double x)
      /// </summary>
      public string? HintText { get; set; }

      /// <summary>
      /// eg sin([x])
      /// </summary>
      public string? HintFormat { get; set; }

      public override string ToString() => $"{ImageId} Id={HintId} Text={HintText} Format={HintFormat}";

      private static Image? myGetImageForType(ImageType imageId)
      {
         switch (imageId)
         {
            case ImageType.none: return null;
            case ImageType.type: return Resources.Structure;
            case ImageType.var: return Resources.Variable;
            case ImageType.func: return Resources.Method;
            case ImageType.typedef: return Resources.Typedef;
            case ImageType.enum_label: return Resources.Enumeration;
            case ImageType.define: return Resources.Define;

            default: throw new Crash($"Can't found image for {imageId}");
         }
      }

      public Handler GetHandler() => new Handler(this);
   }
}
