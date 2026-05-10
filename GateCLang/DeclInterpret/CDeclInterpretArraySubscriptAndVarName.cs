using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// Interpret array subscript + var name block (eg '*var[2]' populates <see cref="CDecl.Identifier"/> and <see cref="CTypeSubscriptSet"/>).
   /// </summary>
   public class CDeclInterpretArraySubscriptAndVarName : CTokenInterpreter
   {
      private readonly static InnerParCountDataTag myParCountDataTag = new InnerParCountDataTag();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="context"></param>
      /// <param name="attributeInter"></param>
      /// <param name="exprInterpret"></param>
      public CDeclInterpretArraySubscriptAndVarName(
         CDeclInterpretContext context, CAttributesInterpret attributeInter, CExprStatementInterpreter? exprInterpret)
      {
         Context = context;
         AttributeInterpreter = attributeInter;
         ExprInterpret = exprInterpret;
      }

      /// <summary>
      /// 
      /// </summary>
      private class InnerParCountDataTag { }

      /// <summary>
      /// 
      /// </summary>
      public class PointerSubscript : CTokenInterpreter
      {
         private Or myComposed;

         public PointerSubscript(CAttributesInterpret? attributeInter, CDeclInterpretContext context) => myComposed = attributeInter != null ?
            attributeInter | (new Pointer(context) & new IterateWhileSuccess(new Qualifiers() | attributeInter)) :
            new Or(new Pointer(context) & new IterateWhileSuccess(new Qualifiers()));

         private class Pointer : CTokenInterpreter
         {
            public Pointer(CDeclInterpretContext context) => Context = context;

            public CDeclInterpretContext Context { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               if (input.MarkedText == "*")
               {
                  var ali = (Context == CDeclInterpretContext.expr_type_name ?
                     output.PeekOrDefault<CTypeAlias>() : output.PeekOrDefault<CDecl>()?.TypeAlias) ?? throw new Crash();

                  var scr = CTypeSubscript.MakePointer();

                  scr.TxtToken = input.Dequeue();

                  if (Context == CDeclInterpretContext.function_id)
                  {
                     ali?.FunctionContainer?.TypeAliasReturned?.TypeSubscriptSet.
                        AddSubscript(scr, (int)(inData.AppData[myParCountDataTag] ?? int.MinValue));
                  }
                  else
                  {
                     ali.TypeSubscriptSet.AddSubscript(scr, (int)(inData.AppData[myParCountDataTag] ?? int.MinValue));
                  }

                  return TxtElabResult.success;
               }
               else { return TxtElabResult.continue_searching; }
            }
         }

         private class Qualifiers : CTokenInterpreter
         {
            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               if (
                  input.IsIn &&
                  input.Peek<CToken>()?.TokenType == CTokenType.keyword &&
                  Enum.TryParse<CTypeQualifiersFlags>(input.Peek()?.Content, out var typ_qal))
               {
                  var dcl = output.PeekOrCrash<CDecl>();

                  input.Dequeue();
                  dcl.TypeAlias.TypeSubscriptSet.Subscripts.Last().TypeQualifiers |= typ_qal;

                  return TxtElabResult.success;
               }
               else { return TxtElabResult.continue_searching; }
            }
         }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) =>
            myComposed.Perform(input, inData, ref output);
      }

      /// <summary>
      /// 
      /// </summary>
      public class ArraySubscript : CTokenInterpreter
      {
         private Or myOr;

         public ArraySubscript(CExprStatementInterpreter? exprInterpret, CDeclInterpretContext context) =>
            myOr = new InnerIncomplete(context) | new And(new Is("[", true),
               new InnerArraySize(exprInterpret ?? throw new Crash()), new Expect("]", true));

         private class InnerIncomplete : CTokenInterpreter
         {
            private And myAnd = new Is("[", true) & new Is("]", false);

            public InnerIncomplete(CDeclInterpretContext context)
            {
               switch (Context = context)
               {
                  case CDeclInterpretContext.local_var:
                  case CDeclInterpretContext.global_var:
                  case CDeclInterpretContext.function_decl_param:
                  case CDeclInterpretContext.function_def_param:
                  case CDeclInterpretContext.cclass_field:
                  case CDeclInterpretContext.console_var:
                     break;

                  default: throw new Crash();
               }
            }

            public CDeclInterpretContext Context { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var beg_idx = input.CurrIdx;
               var res = myAnd.Perform(input, inData, ref output);

               if (res == TxtElabResult.success)
               {
                  if (Context == CDeclInterpretContext.cclass_field)
                  {
                     inData.Messages.Add(CCompilerMsgs.IncompleteSizeArrayNotAllowed(input.GetTokenFrom(beg_idx)));
                     return TxtElabResult.failure;
                  }

                  var dcl = output.PeekOrCrash<CDecl>();
                  var sub_scr = new CTypeSubscript();

                  sub_scr.Kind = CTypeSubscriptKind.array;
                  sub_scr.TxtToken = TxtTokenConst.FromTokenInterval(input[beg_idx], input.MarkedToken ?? throw new Crash());
                  input.CurrIdx++;
                  dcl.TypeAlias.TypeSubscriptSet.AddSubscript(sub_scr, (int)(inData.AppData[myParCountDataTag] ?? int.MinValue));
               }

               return res;
            }
         }

         private class InnerArraySize : CTokenInterpreter
         {
            private CExprStatementInterpreter myExprInterpret;

            public InnerArraySize(CExprStatementInterpreter exprInterpret) => myExprInterpret = exprInterpret;

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var beg_idx = input.CurrIdx;

               myExprInterpret.OutputPreCondition = t => t?.Content == "]";

               var res = myExprInterpret.Perform(input, inData, ref output);

               if (res == TxtElabResult.success)
               {
                  var c_exp = output.PopOrCrash<CExprStatement>();
                  var dcl = output.PeekOrCrash<CDecl>();

                  //is constant expression
                  var is_cst = c_exp.Expr?.IsConstant ?? false;

                  //is local variable (parameter or local var)
                  var is_loc = output.ScopeSpaceItem?.Scope.IsLocal ?? false;

                  if (is_cst || inData.Settings.AreVarArrayValid && is_loc)
                  {
                     var sub_scr = new CTypeSubscript();

                     sub_scr.ArraySizeExpr = c_exp;
                     sub_scr.TxtToken = TxtTokenConst.FromTokenInterval(input[beg_idx - 1], input.MarkedToken ?? throw new Crash());
                     dcl.TypeAlias.TypeSubscriptSet.AddSubscript(sub_scr, (int)(inData.AppData[myParCountDataTag] ?? int.MinValue));
                  }
                  else
                  {
                     inData.Messages.Add(CCompilerMsgId.expected_constant_expression.GetError(c_exp.TxtToken));

                     return TxtElabResult.failure;
                  }
               }

               return res;
            }
         }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) =>
            myOr.Perform(input, inData, ref output);
      }

      private class ParenthesisOpen : CTokenInterpreter
      {
         public ParenthesisOpen() { }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var par_cnt = (int)(inData.AppData[myParCountDataTag] ?? int.MinValue);

            if (input.MarkedText == "(")
            {
               inData.AppData[myParCountDataTag] = ++par_cnt;
               input.Dequeue();

               return TxtElabResult.success;
            }
            else { return TxtElabResult.continue_searching; }
         }
      }

      private class ParenthesisClose : CTokenInterpreter
      {
         public ParenthesisClose() { }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var par_cnt = (int)(inData.AppData[myParCountDataTag] ?? int.MinValue);

            if (input.MarkedText == ")" && par_cnt > 0)
            {
               inData.AppData[myParCountDataTag] = --par_cnt;
               input.Dequeue();

               return TxtElabResult.success;
            }
            else { return TxtElabResult.continue_searching; }
         }
      }

      private class InnerFunction : CTokenInterpreter
      {
         private readonly And myAnd;

         public InnerFunction(CAttributesInterpret attributeInter)
         {
            AttributeInter = attributeInter;
            myAnd = new And(
               new IterateWhileSuccess(new PointerSubscript(attributeInter, CDeclInterpretContext.function_id)),
               new IterateWhileSuccess(
                  //use local var for function pointer 
                  new ParenthesisOpen() | new PointerSubscript(attributeInter, CDeclInterpretContext.local_var)),
               new IdSetInterpreter(IdSetInterpreter.ModeType.optional_continue),
               new IterateWhileSuccess(new ParenthesisClose()));
         }

         public CAttributesInterpret AttributeInter { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var itm = output.TopItem as CDecl ?? throw new Crash();

            // necessary for not decl function declares (eg typedef)
            if (!(itm is CDeclFunction))
            {
               itm.TypeAlias.SetAsFunction(true);
            }

            var res = myAnd.Perform(input, inData, ref output);

            if (res != TxtElabResult.success && !(itm is CDeclFunction))
            {
               // necessary for not decl function declares (eg typedef)
               itm.TypeAlias.SetAsFunction(false);
            }

            return res;
         }
      }

      /// <summary>
      /// For local/global var or function decl/def parameter
      /// </summary>
      private class InnerParamOrVariable : CTokenInterpreter
      {
         private readonly And myAnd;

         public InnerParamOrVariable(
            CAttributesInterpret? attributeInter, CExprStatementInterpreter? exprInterpret, CDeclInterpretContext context)
         {
            switch (Context = context)
            {
               case CDeclInterpretContext.function_id:
                  myAnd = new And(
                     new IterateWhileSuccess(new ParenthesisOpen() | new PointerSubscript(attributeInter, Context)),
                     new IdSetInterpreter(IdSetInterpreter.ModeType.optional_continue),
                     new IterateWhileSuccess(new ParenthesisClose()));
                  break;

               case CDeclInterpretContext.console_var:
               case CDeclInterpretContext.local_var:
               case CDeclInterpretContext.global_var:
               case CDeclInterpretContext.function_decl_param:
               case CDeclInterpretContext.function_def_param:
               case CDeclInterpretContext.cclass_field:
                  //a function declaration may be empty like 'int f(int,int)'
                  myAnd = new And(
                     new IterateWhileSuccess(new ParenthesisOpen() | new PointerSubscript(attributeInter, Context)),
                     new IdSetInterpreter(IdSetInterpreter.ModeType.optional_success),
                     new IterateWhileSuccess(new ParenthesisClose() | new ArraySubscript(exprInterpret, context)));
                  break;

               case CDeclInterpretContext.class_method:
               case CDeclInterpretContext.expr_type_name:
               default:
                  throw new Crash();
            }
         }

         public CDeclInterpretContext Context { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var res = myAnd.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               var end_par_cnt = (int)(inData.AppData[myParCountDataTag] ?? int.MinValue);

               if (end_par_cnt > 0)
               {
                  var ope = input.Take(input.CurrIdx).LastOrDefault(i => i?.Content == "(") ?? throw new Crash();

                  if (input.IsIn)
                  {
                     inData.Messages.Add(CCompilerMsgs.EndOfFileReachedOpenBracket(ope, true));

                     return TxtElabResult.failure_unrecoverable;
                  }
               }

               inData.AppData[myParCountDataTag] = null;

               if (Context != CDeclInterpretContext.function_decl_param && Context != CDeclInterpretContext.function_def_param)
               {
                  var var = output.PeekOrCrash<CDecl>();

                  if (var.IsAnonimous)
                  {
                     /// identifier of a variable can't be null, 
                     /// in case no subscript is declared <see cref="Gate.CLanguage.DeclSpecifiers.CDeclSpecifiers"/> is with no Decls
                     /// eg 'int;' is ok but 'int*;' is error
                     /// function parameters
                     if (end_par_cnt != 0 || var.TypeAlias.TypeSubscriptSet.SubscriptCount != 0)
                     {
                        inData.Messages.Add(CCompilerMsgId.expected_identifier.GetError(
                           input.GetTokenFrom((int)(inData.AppData[CDeclInterpret.BeginningTokenIdData] ?? int.MinValue))));

                        return TxtElabResult.failure;
                     }
                     else { return TxtElabResult.continue_searching; }
                  }

               }
            }

            return res;
         }
      }

      private class InnerTypeName : CTokenInterpreter
      {
         private Lazy<IterateWhileSuccess> myCompose;

         public InnerTypeName(CAttributesInterpret? attributeInter) =>
            myCompose = new Lazy<IterateWhileSuccess>(() =>
               new IterateWhileSuccess(new ParenthesisUpdate() | new PointerSubscript(attributeInter, CDeclInterpretContext.expr_type_name)));

         private class ParenthesisUpdate : CTokenInterpreter
         {
            public ParenthesisUpdate()
            {

            }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var par_cnt = (int)(inData.AppData[myParCountDataTag] ?? int.MinValue);

               switch (input.MarkedText)
               {
                  case "(":
                     inData.AppData[myParCountDataTag] = ++par_cnt;
                     input.Dequeue();

                     return TxtElabResult.success;

                  case ")":
                     if (par_cnt <= 0) { return TxtElabResult.continue_searching; }
                     else
                     {
                        inData.AppData[myParCountDataTag] = --par_cnt;
                        input.Dequeue();

                        return TxtElabResult.success;
                     }

                  default: return TxtElabResult.continue_searching;
               }
            }
         }
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var res = myCompose.Value.Perform(input, inData, ref output);

            var end_par_cnt = (int)(inData.AppData[myParCountDataTag] ?? int.MinValue);

            if (end_par_cnt > 0)
            {
               var ope = input.Take(input.CurrIdx).LastOrDefault(i => i?.Content == "(") ?? throw new Crash();

               inData.Messages.Add(CCompilerMsgs.EndOfFileReachedOpenBracket(ope, true));

               return TxtElabResult.failure_unrecoverable;
            }

            inData.AppData[myParCountDataTag] = null;

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpretContext Context { get; }

      /// <summary>
      /// 
      /// </summary>
      public CAttributesInterpret AttributeInterpreter { get; }

      /// <summary>
      /// 
      /// </summary>
      public CExprStatementInterpreter? ExprInterpret { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var res = TxtElabResult.success;
         var atr = inData.Settings.AreAttributeInDeclSApecifierOnly ? AttributeInterpreter : null;

         inData.AppData[myParCountDataTag] = 0;

         switch (Context)
         {
            case CDeclInterpretContext.console_var:
            case CDeclInterpretContext.local_var:
            case CDeclInterpretContext.global_var:
            case CDeclInterpretContext.function_decl_param:
            case CDeclInterpretContext.function_def_param:
            case CDeclInterpretContext.cclass_field:
               var var_inr = new InnerParamOrVariable(atr, ExprInterpret, Context);

               res = var_inr.Perform(input, inData, ref output);
               break;

            case CDeclInterpretContext.expr_type_name:
               var cm2 = new InnerTypeName(atr);

               res = cm2.Perform(input, inData, ref output);
               break;
            case CDeclInterpretContext.function_id:
               var inn_f = new InnerFunction(AttributeInterpreter);

               res = inn_f.Perform(input, inData, ref output);
               break;

            default: throw new Crash();
         }

         inData.AppData[myParCountDataTag] = null;

         return res;
      }
   }
}
