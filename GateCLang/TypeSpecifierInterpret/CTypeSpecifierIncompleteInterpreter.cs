using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.TypeSpecifierInterpret
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSpecifierIncompleteInterpreter : CTokenInterpreter
   {
      private IdSetInterpreter myIdSetInterpreter = new IdSetInterpreter(IdSetInterpreter.ModeType.required);

      public CTypeSpecifierIncompleteInterpreter(CTypeUserTag incompleteType, UsageId usage)
      {
         IncompleteType = incompleteType;
         Usage = usage;
      }

      public CTypeUserTag IncompleteType { get; }

      public UsageId Usage { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         output.Push(new CTypeIncomplete(IncompleteType));

         var res = myIdSetInterpreter.Perform(input, inData, ref output);

         if (res != TxtElabResult.success) { output.PopOrCrash<CTypeIncomplete>(); }
         else
         {
            var typ_inc = output.PopOrCrash<CTypeIncomplete>();
            
            SetOutputTypeBase(Usage, output, typ_inc);
         }

         return res;
      }

      public override string ToString() => $"{GetType().Name}({IncompleteType})";
   }
}
