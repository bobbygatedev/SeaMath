using Gate.CLanguage;
using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Statement;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace GateCLang.Statement
{
   /// <summary>
   /// goto label eg end in void main() { goto end; end: }
   /// </summary>
   public class CStatementGotoLabel : CStatementLabeled
   {
      public CStatementGotoLabel() { }

      /// <summary>
      /// Token interpreter for goto label
      /// </summary>
      public class TokenInterpreter : CTokenInterpreter
      {
         private readonly And myAnd = new IsCTokenType(CTokenType.identifier, true) & new Is(":", true);

         /// <summary>
         /// 
         /// </summary>
         public TokenInterpreter() { }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var res = myAnd.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               var lab = new CStatementGotoLabel();

               lab.Name = input[input.CurrIdx - 2].Content;
               lab.TxtToken = TxtTokenConst.FromTokenInterval(input[input.CurrIdx - 2], input[input.CurrIdx - 1]);

               if (output.TopItem is CStatementCompound cmp)
               {
                  cmp.AddStatements(lab);
               }
               else if (output.TopItem is CStatementConditional cnd)
               {
                  cnd.AddLabeledStatement(lab);
               }
               else
               {
                  throw new Crash();
               }
            }

            return res;
         }
      }

      public CStatementCompound? ParentCompound => ParentItem as CStatementCompound;

      public CItem? NextStatement
      {
         get
         {
            if (!Name.IsBlank())
            {
               var is_cmp = ParentItem is CStatementCompound;

               var lst = (ParentItem?.SubItems ?? []).ToList();
               var itm = lst.IndexOf(this);
               var nxt_itm = itm >= 0 ? lst.ElementAtOrDefault(itm + 1) as CItem : null;

               return nxt_itm != null || is_cmp ? nxt_itm :
                  throw new Gate.CLanguage.CLangException($"Not found associated item for label {Name}");
            }
            else
            {
               throw new Gate.CLanguage.CLangException($"Anonimous goto label");
            }
         }
      }

      public string? Name { get; set; }

      public override string? Descriptor => $"{Name}:";

      public override string? Rebuilt => $"{Name}:";

      public override bool HasAssociatedPragma => false;
   }
}
