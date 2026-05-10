using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TypeSpecifierInterpret
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeSpecifierInterpret : CTokenInterpreter
   {
      public enum UsageId
      {
         decl_specifier = 0,
         type_name = 1,
      }

      private readonly And myAnd;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="usage"></param>
      /// <param name="declInterpretFactory"></param>
      /// <param name="exprInterpret"></param>
      /// <param name="attributesInterpret"></param>
      public CTypeSpecifierInterpret(
         UsageId usage,
         CDeclInterpretFactory declInterpretFactory,
         CExprStatementInterpreter exprInterpret,
         CAttributesInterpret attributesInterpret)
      {
         ExprInterpret = exprInterpret;
         Usage = usage;
         DeclInterpretFactory = declInterpretFactory;
         AttributesInterpret = attributesInterpret;
         myAnd = new Or(MakeInterpreters()) & new InnerSpecifierIdCheck(usage);
      }

      private class InnerSpecifierIdCheck : CTokenInterpreter
      {
         public InnerSpecifierIdCheck(UsageId usage) => Usage = usage;

         public UsageId Usage { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            switch (Usage)
            {
               case UsageId.decl_specifier:
                  var dcl_spc = output.PeekOrDefault<CDeclSpecifiers>() ?? throw new Crash();

                  if (dcl_spc.TypeBase is CTypeUserDefined usr_typ)
                  {
                     if (!inData.ScopeHelper.CheckUserDefType(inData.Messages, usr_typ, output.ScopeSpaceItem))
                     {
                        return TxtElabResult.failure;
                     }
                  }

                  return TxtElabResult.success;

               case UsageId.type_name:
                  var typ_ali = output.PeekOrDefault<CTypeAlias>() ?? throw new Crash();

                  if (typ_ali.TypeBase is CTypeUserDefined usr_typ_1)
                  {
                     if (!inData.ScopeHelper.CheckUserDefType(inData.Messages, usr_typ_1, output.ScopeSpaceItem))
                     {
                        return TxtElabResult.failure;
                     }
                  }

                  return TxtElabResult.success;

               default: throw new Crash();
            }
         }
      }

      public UsageId Usage { get; }

      public CDeclInterpretFactory DeclInterpretFactory { get; }

      public CAttributesInterpret AttributesInterpret { get; }

      public CExprStatementInterpreter ExprInterpret { get; }

      protected virtual CTypeSpecifierInterpreterStructUnion[] myMakeInterpretersStructUnion() => new[] {
         new CTypeSpecifierInterpreterStructUnion(CTypeUserTag.@struct,AttributesInterpret,ExprInterpret,Usage, DeclInterpretFactory) ,
         new CTypeSpecifierInterpreterStructUnion(CTypeUserTag.union,AttributesInterpret,ExprInterpret, Usage, DeclInterpretFactory)};

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CTypeSpecifierInterpretBuiltIn myMakeInterpreterTypeBuiltIn() => new CTypeSpecifierInterpretBuiltIn(Usage);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CTypeSpecifierInterpretEnum myMakeInterpreterEnum() => new CTypeSpecifierInterpretEnum(AttributesInterpret, ExprInterpret, Usage);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual CTypeSpecifierInterpretTypedef myMakeInterpreterTypedef() => new CTypeSpecifierInterpretTypedef(Usage);

      public virtual CTokenInterpreter[] MakeInterpreters() => new CTokenInterpreter[] {
            myMakeInterpreterTypeBuiltIn() ,
            myMakeInterpreterEnum(),
            myMakeInterpreterTypedef()}.Concat(myMakeInterpretersStructUnion()).ToArray();

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) => myAnd.Perform(input, inData, ref output);

      /// <summary>
      /// <br>Get type base associated to output stack-top if any:</br>
      /// <br> - If <paramref name="usage"/> is <see cref="UsageId.decl_specifier"/> returns <see cref="CDeclSpecifiers.TypeBase"/> </br>
      /// <br> - If <paramref name="usage"/> is <see cref="UsageId.type_name"/> returns <see cref="CTypeAlias.TypeBase"/> </br>
      /// </summary>
      /// <param name="usage"></param>
      /// <param name="output"></param>
      /// <param name="offset"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public static CType? GetOutputTypeBase(UsageId usage, CTokenInterpreterOutput output, int offset = 0)
      {
         switch (usage)
         {
            case UsageId.decl_specifier: return output.PeekOrCrash<CDeclSpecifiers>(offset).TypeBase;

            case UsageId.type_name: return output.PeekOrCrash<CTypeAlias>(offset).TypeBase;

            default: throw new Crash();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="usage"></param>
      /// <param name="output"></param>
      /// <param name="type"></param>
      /// <param name="offset"></param>
      /// <exception cref="Crash"></exception>
      public static void SetOutputTypeBase(UsageId usage, CTokenInterpreterOutput output, CType? type, int offset = 0)
      {
         switch (usage)
         {
            case UsageId.decl_specifier:
               output.PeekOrCrash<CDeclSpecifiers>(offset).TypeBase = type;
               break;

            case UsageId.type_name:
               output.PeekOrCrash<CTypeAlias>(offset).TypeBase = type;
               break;

            default: throw new Crash();
         }
      }

      public override string ToString() => $"{GetType().Name}({Usage})";
   }
}
