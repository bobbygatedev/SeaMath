using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// 
   /// </summary>
   public class CStatementLoopFor : CStatementLoop, ICItemWithScopeSpace
   {
      private CExprStatement? myUpdate;
      private CExprStatement? myInitExpression;

      /// <summary>
      /// 
      /// </summary>
      public CStatementLoopFor() => Scope = new CScope(this);

      /// <summary>
      /// 
      /// </summary>
      public class TokenInterpret : TokenInterpretBase
      {
         private And myAnd;

         public TokenInterpret(CDeclInterpretFactory declInterpretFactory, CAttributesInterpret attributesInterpret, CExprStatementInterpreter exprInterpret)
            : base(declInterpretFactory, attributesInterpret, exprInterpret)
         {
            DeclInterpret = new CDeclInterpret(CDeclInterpretContext.local_var, DeclInterpretFactory, ExprInterpret, attributesInterpret);
            myAnd =
               new Is("for", true) & new Expect("(", true) &
               new InnerInitInterpreter(this) &
               new ConditionInterpreter(this) &
               new InnerUpdateInterpreter(this) &
               new ContentInterpret(this);

            //for & ( dcl/exp/none & ; & (built_in no_void) none & ; & exp & ) & compound or expression
         }

         /// <summary>
         /// Interprets and processes initialization expressions within a `for` loop construct. (eg for(int i=0; i<10; i++) ... )
         /// </summary>
         /// <remarks>This interpreter handles the initialization part of a `for` loop by evaluating
         /// expressions or declarations and updating the loop's initialization state accordingly. It uses a combination
         /// of expression interpretation and declaration interpretation to process the input tokens.</remarks>
         private class InnerInitInterpreter : CTokenInterpreter
         {
            private Or myOr;

            public InnerInitInterpreter(TokenInterpret forInterpret)
            {
               ForInterpret = forInterpret;
               myOr = forInterpret.DeclInterpret | new May(forInterpret.ExprInterpret) & new Expect(";", true);
            }

            public TokenInterpret ForInterpret { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var for_bdy = output.PeekOrCrash<CStatementLoopFor>();

               ForInterpret.ExprInterpret.OutputPreCondition = t => t?.Content == ";";

               var res = myOr.Perform(input, inData, ref output);
               var ds = (output.ScopeSpaceItem as CItem)?.SubItems.LastOrDefault() as CDeclSpecifiers;

               if (res == TxtElabResult.success)
               {
                  if (output.PeekOrCrash<CItem>() is CExprStatement exp)
                  {
                     output.PopOrCrash<CItem>();
                     for_bdy.InitExpression = exp;
                  }
               }

               return res;
            }
         }

         /// <summary>
         /// Update part of for cycle , eg in 'for(i=0;i<3;i++) i++ is update.
         /// </summary>
         private class InnerUpdateInterpreter : CTokenInterpreter
         {
            private readonly And myAnd;

            public InnerUpdateInterpreter(TokenInterpret forInterpret) =>
               myAnd = new May((ForInterpret = forInterpret).ExprInterpret) & new Expect(")", true);

            public TokenInterpret ForInterpret { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               ForInterpret.ExprInterpret.OutputPreCondition = t => t?.Content == ")";

               var res = myAnd.Perform(input, inData, ref output);

               if (res == TxtElabResult.success)
               {
                  var exp = output.PeekOrDefault<CExprStatement>();

                  if (exp != null)
                  {
                     var ali = exp.Expr.NnOrCrash().RootNode?.DeclType.ConvertOrCrash<CTypeAlias>();
                     var pri_ali = ali?.PrimitiveAlias;

                     output.PopOrCrash<CExprStatement>();
                     output.PeekOrCrash<CStatementLoopFor>().Update = exp;
                  }
               }

               return res;
            }
         }

         public CDeclInterpret DeclInterpret { get; private set; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cyc_for = new CStatementLoopFor();
            var itm_sco = output.ScopeSpaceItem.NnOrCrash();
            var idx = input.CurrIdx;

            if (input.MarkedText != "for") { return TxtElabResult.continue_searching; }

            if (!itm_sco.AddToScopeSpace(cyc_for, inData.ScopeHelper, inData.Messages)) { throw new Crash(); }

            var res = myNested(cyc_for, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res == TxtElabResult.success)
            {
               cyc_for.TxtToken = TxtTokenConst.FromTokenInterval(input[idx], input[input.CurrIdx - 1]);
            }
            else { itm_sco.RemoveFromScopeSpace(cyc_for); }

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CExprStatement? InitExpression
      {
         get => myInitExpression;
         set
         {
            if (myInitExpression != value)
            {
               myRemoveSubItem(myInitExpression);

               if ((myInitExpression = value) != null) { myAddSubItem(myInitExpression); }
            }
         }
      }

      /// <summary>
      /// <br> If <see cref="Initialisation"/> is a declarator (eg 'for(int i = 0;; i++);') associated <see cref="CDeclSpecifiers"/> </br>
      /// <br> in other cases (eg 'for(i = 0;; i++);','for(;; i++);') </br>
      /// </summary>
      public CDeclSpecifiers? InitSpecifiers
      {
         get => SubItems.OfType<CDeclSpecifiers>().FirstOrDefault();

         set
         {
            myRemoveSubItem(InitSpecifiers);
            myAddSubItem(value);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CItem? Initialisation => InitExpression as CItem ?? InitSpecifiers;

      /// <summary>
      /// 
      /// </summary>
      public CExprStatement? Update
      {
         get => myUpdate;
         set
         {
            if (myUpdate != value)
            {
               myRemoveSubItem(myUpdate);

               if ((myUpdate = value) != null) { myAddSubItem(myUpdate); }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt
      {
         get
         {
            if (Body is CStatementCompound cmp) { return $"{Descriptor}\n{cmp.Descriptor}"; }
            else if (Body is CExprStatement exp) { return $"{Descriptor} {exp.Descriptor};"; }
            else { return $"{Body?.Descriptor};"; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => $"for({Initialisation?.Descriptor};{StayConditionExpr?.Descriptor};{Update?.Descriptor}){Body?.Descriptor}";

      public CScope Scope { get; }

      public CDecl[] ScopeDecls =>
         Body is CStatementCompound cmp ? cmp.ScopeDecls : InitSpecifiers?.Decls ?? [];

      public bool AddDeclSpec(MsgCollection messages, CDeclSpecifiers declSpecifier, CScopeHelperBase? scopeHelper = null)
      {
         if (InitSpecifiers == null)
         {
            InitSpecifiers = declSpecifier.NnOrCrash();

            return true;
         }
         else
         {
            throw new Crash("Init specifiers already set");
         }
      }

      public bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages) =>
         AddDeclSpec(messages, item.ConvertOrCrash<CDeclSpecifiers>(), scopeHelper);

      public void RemoveFromScopeSpace(CItem item)
      {
         if (item == InitSpecifiers)
         {
            InitSpecifiers = null;
         }
         else
         {
            throw new Crash("Unexpected!");
         }
      }
   }
}
