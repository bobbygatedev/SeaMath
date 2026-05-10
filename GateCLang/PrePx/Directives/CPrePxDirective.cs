using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.CLanguage.PrePx.Directives
{
   public abstract class CPrePxDirective : CPrePxProduct, ICloneable
   {
      /// <summary>
      /// 
      /// </summary>
      protected CPrePxDirective() { }

      /// <summary>
      /// Name of directive (eg define,ifdef,endif,..).
      /// </summary>
      public abstract string DirectiveName { get; }

      /// <summary>
      /// 
      /// </summary>
      public TxtToken? ContentToken { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public virtual object Clone()
      {
         var typ = GetType();
         var prs = typ.GetProperties().Where(p => p.GetSetMethod() != null).ToArray();
         var ist = typ.InstanciateOrCrash();

         foreach (var pr in prs)
         {
            var get_val = pr.GetValue(this, []);

            pr.SetValue(ist, get_val, []);
         }

         return ist;
      }
   }
}
