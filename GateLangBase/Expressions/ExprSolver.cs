using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Reflection;

namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// <br>Abstract generic class for an expression solver</br>  
   /// <br>Don't use directly but use <see cref="WithParser"/> if you want to Parse an a text expression </br>
   /// <br> or <see cref="WithInterpret"/> if text has been parsed to a <see cref="TxtTokenList"/></br>
   /// </summary>
   /// <typeparam name="IN_DATA"></typeparam>
   public abstract class ExprSolver<IN_DATA> where IN_DATA : TxtElabInData, IExprSolverInData
   {
      private static readonly InnerAssemblyManager myInnerAssemblyManager = new();
      private Lazy<ExprNodePopulator<IN_DATA>> myLazyExprNodePopulator;
      private Lazy<Operator[]> myLazyOperators;

      /// <summary>
      /// Constructor.
      /// </summary>
      protected ExprSolver()
      {
         myLazyExprNodePopulator = new Lazy<ExprNodePopulator<IN_DATA>>(() => myMakeExprNodePopulator());
         myLazyOperators = new Lazy<Operator[]>(myMakeOperators);
      }

      /// <summary>
      /// 
      /// </summary>
      private class InnerAssemblyManager
      {
         private readonly List<Assembly> myListAssemblies = new List<Assembly>();
         private readonly List<Type> myListOperatorTypes = new List<Type>();

         public InnerAssemblyManager() { }

         public Type[] OperatorTypes
         {
            get
            {
               myUpdate();
               return myListOperatorTypes.ToArray();
            }
         }

         public Assembly[] Assemblies
         {
            get
            {
               myUpdate();

               return myListAssemblies.ToArray();
            }
         }

         private Type[] myGetTypes(Assembly assembly)
         {
            try
            {
               return assembly.GetTypes();
            }
            catch
            {
               return [];
            }
         }

         private void myUpdate()
         {
            var ass = AppDomain.CurrentDomain.GetAssemblies();

            ass = ass.Except(myListAssemblies).ToArray();

            var tps = ass.SelectMany(a => myGetTypes(a)).Where(t => t.IsSubclassOf(typeof(Operator)) && t.GetConstructor([]) != null).ToArray();

            myListAssemblies.AddRange(ass);
            myListOperatorTypes.AddRange(tps);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="INT_OUT">Interpreter outptu</typeparam>
      public abstract class WithInterpret<INT_OUT> : ExprSolver<IN_DATA> where INT_OUT : class, ICloneable, new()
      {
         private Lazy<ExprNodeInterpret<IN_DATA, INT_OUT>.Lst> myLazyNodeListInterpreter;

         public WithInterpret() => myLazyNodeListInterpreter = new Lazy<ExprNodeInterpret<IN_DATA, INT_OUT>.Lst>(() => myMakeListInterpreter());

         private TokenInterpreter<IN_DATA, ExprNodeOutput<INT_OUT>>.Lst myMakeListInterpreter()
         {
            var cns = new TxtElab<TxtTokenList, IN_DATA, ExprNodeOutput<INT_OUT>>?[] {
               myMakePreConditionInterpreter(),
               new ExprNodeInterpret<IN_DATA,INT_OUT>.Or(myMakeNodeInterpreters()),
               myMakePostConditionInterpreter() }.Nn().ToArray();

            var cmp = new ExprNodeInterpret<IN_DATA, INT_OUT>.And(cns.Where(c => c != null).ToArray());

            return new ExprNodeInterpret<IN_DATA, INT_OUT>.Lst(cmp);
         }

         protected abstract ExprNodeInterpret<IN_DATA, INT_OUT>? myMakePreConditionInterpreter();

         protected abstract ExprNodeInterpret<IN_DATA, INT_OUT>? myMakePostConditionInterpreter();

         protected abstract ExprNodeInterpret<IN_DATA, INT_OUT> myMakeConstantNodeInterpreter();

         protected abstract ExprNodeInterpret<IN_DATA, INT_OUT>[] myMakeAppSpecificNodeInterpreters();

         protected virtual ExprNodeOperandVariable.Interpreter<IN_DATA, INT_OUT> myMakeOperandNodeVariableInterpreter() =>
            new ExprNodeOperandVariable.Interpreter<IN_DATA, INT_OUT>.Standard();

         protected virtual ExprNodeInterpret<IN_DATA, INT_OUT>[] myMakeNodeInterpreters() =>
               myMakeAppSpecificNodeInterpreters().
               Append(myMakeConstantNodeInterpreter()).Concat(myMakeStandardNodeInterpreters()).ToArray();

         protected virtual ExprNodeInterpret<IN_DATA, INT_OUT>[] myMakeStandardNodeInterpreters() => [
               new ExprNodeOperatorPunctuator<IN_DATA>.Interpreter<INT_OUT>(myGetPunctuators(Operators)),
               myMakeOperandNodeVariableInterpreter(),
               new ExprNodeBracket<IN_DATA>.Interpreter<INT_OUT>(BracketPairs.Select(t=>(t.open,t.close)).ToArray())];

         /// <summary>
         /// 
         /// </summary>
         /// <param name="input"></param>
         /// <param name="inData"></param>
         /// <param name="visibleDecls">Declarations visible in scope of expression (last item is taken in case of homonymy).</param>
         /// <param name="interpreterOutput"></param>
         /// <param name="outputPreCondition"></param>
         /// <param name="expr"></param>
         /// <returns></returns>
         public virtual TxtElabResult InterpretTokens(
            TxtTokenList input,
            IN_DATA inData,
            IDecl[] visibleDecls,
            ref INT_OUT interpreterOutput,
            out Expr? expr,
            bool successIfAtEnd,
            BracketOutputHandler? bracketPreCondition = null,
            BracketOutputHandler? bracketPostCondition = null)
         {
            var nod_itr_out = new ExprNodeOutput<INT_OUT>();
            var bra_hlp = new BracketHelper(BracketPairs, successIfAtEnd, bracketPreCondition, bracketPostCondition);
            var beg_idx = input.CurrIdx;

            var bra_hlp_res = bra_hlp.Check(input, out var stk);

            var sub_tks = null as TxtTokenList;

            switch (bra_hlp_res)
            {
               case TxtElabResult.success:
                  sub_tks = new TxtTokenList(input.Skip(beg_idx).Take(input.CurrIdx - beg_idx));

                  if (sub_tks.Count() == 0)
                  {
                     expr = null;

                     return TxtElabResult.continue_searching;
                  }

                  break;

               case TxtElabResult.continue_searching:
                  expr = null;
                  return TxtElabResult.continue_searching;

               case TxtElabResult.failure:
               case TxtElabResult.failure_unrecoverable:
                  expr = null;

                  if (stk.Count == 0)
                  {
                     inData.Messages.Add(ExprSolverMessages.NoOpenBracket(input.MarkedToken));
                  }
                  else
                  {
                     inData.Messages.Add(
                        ExprSolverMessages.NotMatchingBracket(input?.MarkedToken, stk.Peek(), true));
                  }

                  return TxtElabResult.failure_unrecoverable;

               default: throw new Crash();
            }

            nod_itr_out.InterpreterOutput = interpreterOutput;

            var inr = myLazyNodeListInterpreter.Value;

            var res = inr.Perform(sub_tks, inData, ref nod_itr_out);

            expr = null;
            interpreterOutput = nod_itr_out?.InterpreterOutput ?? throw new Crash();

            if (res == TxtElabResult.success)
            {
               res = BuildExpression(nod_itr_out.ListProduct.ToArray(), inData, visibleDecls, out expr);
            }

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract class WithParser<INT_OUT> : ExprSolver<IN_DATA> where INT_OUT : class, ICloneable, new()
      {
         private Lazy<ExprNodeParser<IN_DATA, INT_OUT>.List> myLazyListParser;

         public WithParser()
         {
            var cns = new TxtElab<TxtMarker, IN_DATA, ExprNodeOutput<INT_OUT>>[] {
               myMakePreConditionParser(), new ExprNodeParser<IN_DATA,INT_OUT>.Or(myMakeExprNodeParsers()), myMakePostConditionParser() };

            var cmp = new ExprNodeParser<IN_DATA, INT_OUT>.And(cns.Where(c => c != null).ToArray());

            myLazyListParser = new Lazy<ExprNodeParser<IN_DATA, INT_OUT>.List>(() => new ExprNodeParser<IN_DATA, INT_OUT>.List(cmp));
         }

         protected abstract ExprNodeParser<IN_DATA, INT_OUT> myMakePreConditionParser();

         protected abstract ExprNodeParser<IN_DATA, INT_OUT> myMakePostConditionParser();

         protected abstract ExprNodeParser<IN_DATA, INT_OUT> myMakeConstantNodeParser();

         protected abstract ExprNodeParser<IN_DATA, INT_OUT>[] myMakeAppSpecificNodeParsers();

         protected virtual ExprNodeParser<IN_DATA, INT_OUT>[] myMakeExprNodeParsers() =>
            myMakeAppSpecificNodeParsers().Concat(myMakeStandardNodeParserers()).Append(myMakeConstantNodeParser()).ToArray();

         protected virtual ExprNodeParser<IN_DATA, INT_OUT>[] myMakeStandardNodeParserers() => [
               new ExprNodeOperatorPunctuator<IN_DATA>.Parser<INT_OUT>(myGetPunctuators(Operators)),
               myMakeIdentifierParser(),
               new ExprNodeBracket<IN_DATA>.Parser<INT_OUT>(BracketPairs.Select(t=>(t.open,t.close)).ToArray())];
         protected virtual ExprNodeOperandVariable.Parser<IN_DATA, INT_OUT> myMakeIdentifierParser() =>
            new ExprNodeOperandVariable.Parser<IN_DATA, INT_OUT>.Standard();

         /// <summary>
         /// 
         /// </summary>
         /// <param name="inMarker"></param>
         /// <param name="inData"></param>
         /// <param name="visibleDecls">Declarations visible in scope of expression (last item is taken in case of homonymy).</param>
         /// <param name="interpreterOutput"></param>
         /// <param name="expr"></param>
         /// <returns></returns>
         public virtual TxtElabResult ParseTokens(
            TxtMarker inMarker, IN_DATA inData, IDecl[] visibleDecls, ref INT_OUT interpreterOutput, out Expr? expr)
         {
            var nod_itr_out = new ExprNodeOutput<INT_OUT>();

            nod_itr_out.InterpreterOutput = interpreterOutput;

            var res = myLazyListParser.Value.Perform(inMarker, inData, ref nod_itr_out);

            expr = null;

            if (res == TxtElabResult.success)
            {
               res = BuildExpression(nod_itr_out?.ListProduct.ToArray() ?? [], inData, visibleDecls, out expr);
            }

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract (string open, string close, TxtElabResult missReturn)[] BracketPairs { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="operator"></param>
      /// <returns></returns>
      public abstract bool myIsOperatorTypeValid(Type @operatorType);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract ExprNodePopulator<IN_DATA> myMakeExprNodePopulator();

      /// <summary>
      /// 
      /// </summary>
      public Operator[] Operators => myLazyOperators.Value;

      /// <summary>
      /// 
      /// </summary>
      public ExprNodePopulator<IN_DATA> ExprNodePopulator => myLazyExprNodePopulator.Value;

      /// <summary>
      /// Elaborates expression tree and create expression (if not any error) version for languages not using explicit declaration (eg MATLAB)
      /// </summary>
      /// <param name="exprNodes"></param>
      /// <param name="inData"></param>
      /// <param name="expression"></param>
      /// <returns></returns>
      public TxtElabResult BuildExpression(ExprNode[] exprNodes, IN_DATA inData, out Expr? expression) => BuildExpression(exprNodes, inData, null, out expression);

      /// <summary>
      /// Builds an instance of <see cref="Expr"/> using expression nodes 
      /// </summary>
      /// <param name="exprNodes"></param>
      /// <param name="inData"></param>
      /// <param name="visibleDecls">Declarations visible in scope of expression (last item is taken in case of homonymy).</param>
      /// <param name="expr"></param>
      /// <returns></returns>
      public virtual TxtElabResult BuildExpression(ExprNode[] exprNodes, IN_DATA inData, IDecl[]? visibleDecls, out Expr? expr)
      {
         var sub_exp_crt_out = null as ExprNode[];
         var nod_tre_crt = new ExprNodeTreeCreator(Operators);
         var nod_tre_crt_out = null as ExprNode[];//node trees

         expr = null;

         var bra = exprNodes.FirstOrDefault(n => n is ExprNodeBracket);

         if (bra != null)
         {
            inData.Messages.Add(ExprSolverMessages.OpenBracketNotClosed(bra.Token ?? throw new Crash()));

            return TxtElabResult.failure;
         }

         //node tree creator
         if (!nod_tre_crt.Go(exprNodes, inData.Messages, inData.RtmStrategy, out nod_tre_crt_out)) { return TxtElabResult.failure; }

         var roo_nod = null as ExprNode;

         if (nod_tre_crt_out?.Length == 1) { roo_nod = nod_tre_crt_out[0]; }
         else { throw new Crash(); }

         var vrs = roo_nod.AllDescendant.OfType<ExprNodeOperandVariable>().ToArray();
         var tre_opr_nds = roo_nod.AllDescendant.OfType<ExprNodeOperator>().Reverse().ToArray();

         //populated
         if (!ExprNodePopulator.PopulateNodeOperandVariable(vrs, visibleDecls, inData)) { return TxtElabResult.failure; }
         if (!ExprNodePopulator.PopulateNodeOperator(tre_opr_nds, inData)) { return TxtElabResult.failure; }

         expr = new Expr(inData.RtmStrategy);
         expr.RootNode = roo_nod;

         return TxtElabResult.success;
      }

      /// <summary>
      /// Makes operator array by concat of basic operators (choosen by BasicOperatorFlags) and extra operator 
      /// </summary>
      /// <returns></returns>
      protected virtual Operator[] myMakeOperators()
      {
         var tps = myGetOperatorsWithAttribute().Where(t => myIsOperatorTypeValid(t)).ToArray();
         var cts = tps.Select(t => t.GetConstructor([]) ?? throw new Crash());
         var ops = cts.Select(c => c.Invoke([]) as Operator ?? throw new Crash()).ToArray();

         return [.. ops.Where(myIsOperatorValid)];
      }

      protected abstract bool myIsOperatorValid(Operator @operator);

      protected static Type[] myGetOperatorsWithAttribute() => myInnerAssemblyManager.OperatorTypes;

      protected static string[] myGetPunctuators(Operator[] Operators) =>
         Operators.Where(o => o is OperatorPunctuator).Cast<OperatorPunctuator>().Select(o => o.Punctuator).ToArray();
   }
}
