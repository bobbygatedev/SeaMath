using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Source
{
   /// <summary>
   /// 
   /// </summary>
   public class CSourceInterpreter : CTokenInterpreter
   {
      private readonly Or myOr;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subInterpreters"></param>
      public CSourceInterpreter(CTokenInterpreter[] subInterpreters) => myOr = new Or(SubInterpreters = subInterpreters);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var src = output.PeekOrDefault<CSource>();

         if (src == null) { output.Push(src = new CSource()); }

         src.ScopeHelper = inData.ScopeHelper;
         src.Settings = inData.Settings;

         var res = myNestedIterate(src, input, inData, ref output, myOr, inp => !inp.IsIn);

         if (res == TxtElabResult.success)
         {
            var vrs_prs = src.AllDescendant.OfType<CDeclVar>().Where(v => v.IsGlobal).ToArray();
            var var_gls = vrs_prs.Where(v => v.IsGlobal).ToArray();
            var vrs_glo_ins = var_gls.SelectMany(g =>
               inData.FunctionInstructionTranslator.GetInstructions(g)).ToArray();

            var dcl_fnc = new CDeclFunction(true, CDeclFunction.KindType.init, null);

            dcl_fnc.Identifier = $"{src.Name}.Init";
            dcl_fnc.Instructions = vrs_glo_ins;
            src.InitDeclFunction = dcl_fnc;
         }

         return res;
      }

      public CTokenInterpreter[] SubInterpreters { get; private set; }
   }
}
