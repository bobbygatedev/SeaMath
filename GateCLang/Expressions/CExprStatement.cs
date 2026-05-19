using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Statement;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.Tools.Text;

namespace Gate.CLanguage.Expressions
{
   /// <summary>
   /// Expression statement. Expression with side effects, e.g. function call or assignment.
   /// </summary>
   public class CExprStatement : CStatement, ICloneable
   {
      /// <summary>
      /// 
      /// </summary>
      public CExprStatement()
      {

      }

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => Expr?.Rebuilt;

      /// <summary>
      /// 
      /// </summary>
      public override string? Descriptor => Expr?.Descriptor;

      /// <summary>
      /// 
      /// </summary>
      public bool IsLiteralString => TxtToken is CTokenString;

      /// <summary>
      /// 
      /// </summary>
      public CTokenString? TokenString => TxtToken as CTokenString;

      /// <summary>
      /// 
      /// </summary>
      public Expr? Expr
      {
         get => SubItems.OfType<Expr>().FirstOrDefault();
         set
         {
            myRemoveSubItem(Expr);
            myAddSubItem(value);

            if (value != null)
            {
               var ord_nds = value.ExprNodes.Select(n => n.Token).OfType<CToken>().OrderBy(t => t.From?.StoreIdx).ToArray();

               switch (ord_nds.Length)
               {
                  case 0:
                     TxtToken = null;
                     break;

                  case 1:
                     TxtToken = ord_nds[0];
                     break;

                  default:
                     TxtToken = ord_nds.Length > 0 ?
                        TxtTokenConst.FromTokenInterval(ord_nds.First(), ord_nds.Last()) : null as TxtToken;
                     break;
               }

            }
            else
            {
               TxtToken = null;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int? ConstIntValue =>
         ConstValue?.DeclType is CTypeAlias ali &&
         ali.TypeBase is CTypeBuiltIn bin &&
         bin.RepresentedType == CTypeBuiltInRepresent.integer ?
            (int)(dynamic)(ConstValue?.CSharpObj ?? throw new Gate.LangBase.Runtime.RtmException()) : (int?)null;

      /// <summary>
      /// Expression constant value or null.
      /// </summary>
      public CRtmObj? ConstValue => (Expr?.ConstantValue) as CRtmObj;


      /// <summary>
      /// Type of the expression. 
      /// </summary>
      public CTypeAlias? Type => Expr?.RootNode?.DeclType as CTypeAlias;

      public object Clone() => GetCopy();

      public CExprStatement GetCopy()
      {
         var res = new CExprStatement();

         res.Expr = Expr?.Clone() as Expr;

         return res;
      }
   }
}
