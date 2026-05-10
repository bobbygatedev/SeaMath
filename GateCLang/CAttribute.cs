using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.CLanguage
{
   /// <summary>
   /// Encapsulates c declaration attributes like __declspec, __attribute__
   /// </summary>
   public abstract class CAttribute : CItem, IEquatable<CAttribute>
   {
      public class WinDeclSpec : CAttribute
      {
         public const string TAG = "__declspec";
         public const string TAG2 = "_declspec";

         public override string Tag => TAG;

         public override string? Descriptor => $"{Tag}({Content})";
      }

      public class GccAttribute : CAttribute
      {
         public readonly static string[] Tags = Enum.GetNames(typeof(TagEnum));

         public enum TagEnum
         {
            __attribute__,
            __attribute
         }

         public GccAttribute() { }

         public TagEnum TagId { get; set; }

         public override string Tag => TagId.ToString();

         public override string? Descriptor => $"{Tag}(({Content}))";

         public (string? name, string? arg)[] AttributeTuples
         {
            get
            {
               var lst = new List<(string?, string?)>();

               foreach (var str in ContentSplit)
               {
                  var mrk = new TxtMarker(new TxtStore(str));
                  var arg = mrk.GetMarkingVarNameMoveOver();

                  if (arg.IsBlank())
                  {
                     break;
                  }
                  else
                  {
                     if (mrk.IsMarkingAnySignMoveOver("("))
                     {
                        var std_idx = mrk.CurrIdx;

                        if (mrk.LookForAnySign(")"))
                        {
                           lst.Add((arg, mrk.Store.Content.Substring(std_idx, mrk.CurrIdx - std_idx).ExtTrim()));
                        }
                     }
                     else
                     {
                        lst.Add((arg, null));
                     }
                  }
               }

               return lst.ToArray();
            }
         }
      }

      public abstract string Tag { get; }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      public string? Content { get; set; }

      public string[] ContentSplit => Content.ExtTrim().Split(',').Select(i => i.ExtTrim()).ToArray();

      public override string? Rebuilt => Descriptor;

      public virtual bool Equals(CAttribute? other) => other != null && other.GetType() == GetType() && other.Tag == Tag && other.Content == Content;

      public override bool Equals(object? obj) => base.Equals(obj);

      public override int GetHashCode() => Descriptor?.GetHashCode() ?? -1;
   }
}
