using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.TypeSpecifierInterpret.CTypeSpecifierInterpret;

namespace Gate.CLanguage.DeclInterpreter
{
   /// <summary>
   /// Interpreter for whole declaration body (eg 'int a,b=2','int f(void){ return 0; }')
   /// </summary>
   public class CDeclInterpret : CTokenInterpreter
   {
      /// <summary>
      /// 
      /// </summary>
      private readonly Lazy<And> myLazyInterpretComposer;

      /// <summary>
      /// Initializes a new instance of the <see cref="CDeclInterpret"/> class.
      /// </summary>
      /// <param name="context"></param>
      /// <param name="declInterpretFactory"></param>
      /// <param name="exprInterpret"></param>
      /// <param name="attributesInterpret"></param>
      public CDeclInterpret(
         CDeclInterpretContext context,
         CDeclInterpretFactory declInterpretFactory,
         CExprStatementInterpreter exprInterpret,
         CAttributesInterpret attributesInterpret)
      {
         Context = context;
         DeclInterpretFactory = declInterpretFactory;
         ExprInterpret = exprInterpret;
         AttributesInterpret = attributesInterpret;
         myLazyInterpretComposer = new Lazy<And>(new InnerInterpreterComposer(this).Compose);//get composed interpreter
      }

      /// <summary>
      /// 
      /// </summary>
      public class DeclBeginningTokenIdDataType { }

      /// <summary>
      /// Composes the inner interpreters based on the current context.
      /// </summary>
      private class InnerInterpreterComposer
      {
         public InnerInterpreterComposer(CDeclInterpret declInterpret)
         {
            DeclInterpret = declInterpret;
            Context = declInterpret.Context;
            DeclInterpretFactory = declInterpret.DeclInterpretFactory;
            ExprInterpret = declInterpret.ExprInterpret;
            AttributesInterpret = declInterpret.AttributesInterpret;
         }

         public CDeclInterpret DeclInterpret { get; }
         public CDeclInterpretContext Context { get; }
         public CDeclInterpretFactory DeclInterpretFactory { get; }
         public CExprStatementInterpreter ExprInterpret { get; }
         public CAttributesInterpret AttributesInterpret { get; }

         /// <summary>
         /// Interprets declaration specifiers (eg 'int' in 'int a;' or 'static int' in 'static int a;')
         /// </summary>
         public CDeclSpecifiersInterpret DeclSpecifiers =>
            new CDeclSpecifiersInterpret(
               DeclInterpretFactory.MakeDeclSpecifierInterprers(ExprInterpret, AttributesInterpret, UsageId.decl_specifier));


         /// <summary>
         /// Interprets array subscript and variable name (eg 'var[3]' in 'int var[3];')
         /// </summary>
         public CDeclInterpretArraySubscriptAndVarName SubscriptAndVarName => DeclInterpretFactory.MakeArraySubscriptAndVarNameInterpret(Context, ExprInterpret, AttributesInterpret);

         /// <summary>
         /// Interprets function definition (eg 'f(void){ return 0; }' in 'int f(void){ return 0; }')
         /// </summary>
         public CDeclInterpretPartFunction FunctionDefinition => DeclInterpretFactory.MakeDeclInterpretFunctionPart(true, ExprInterpret, AttributesInterpret);

         /// <summary>
         /// Interprets function declaration (eg 'f(void)' in 'int f(void);' or 'int a,f(void)')
         /// </summary>
         public CDeclInterpretPartFunction FunctionDeclaration => DeclInterpretFactory.MakeDeclInterpretFunctionPart(false, ExprInterpret, AttributesInterpret);

         /// <summary>
         /// Interprets semicolon(;) at the end of declaration
         /// </summary>
         public CDeclInterpreterSemicolonCheck Semicolon => new CDeclInterpreterSemicolonCheck();

         /// <summary>
         /// Interprets a SINGLE local and global variable declaration/definition (eg 'a=2' or 'b=3' in 'int a=2,b=3;')
         /// </summary>
         public CDeclInterpretPartVar LocalAndGlobalSingleVar => new CDeclInterpretPartVar(
            Context,
            AttributesInterpret,
            SubscriptAndVarName,
            DeclInterpretFactory.MakeInitializerInterpret(ExprInterpret),
            new CDeclInterpreterBitField(ExprInterpret),
            DeclInterpretFactory,
            ExprInterpret);


         /// <summary>
         /// Init declarator list interpreters (eg in 'int a,*b,c=2,f(int p)' 'a,*b,c=2,f(int p)' is init declarator list )
         /// </summary>
         public And InitDeclarationList => new May(new Lst(",", FunctionDeclaration | LocalAndGlobalSingleVar, false)) & Semicolon;//declaration (with ini list) eg int a = 3 , f(void) , b;

         /// <summary>
         /// Equal to <see cref="InitDeclarationList"/>, but <see cref="CDeclSpecifiers"/> shall have typedef storage class
         /// and declarations can't be have an init.
         /// </summary>
         public And TypedefInitDeclarationList => new InnerTypedefCheck() & InitDeclarationList;

         /// <summary>
         /// Interprets function parameters (eg 'int a, float b' in 'void func(int a, float b)')
         /// </summary>
         public CDeclInterpretPartVar FunctionParameters =>
            new CDeclInterpretPartVar(Context, AttributesInterpret, SubscriptAndVarName, null, null, DeclInterpretFactory, ExprInterpret);

         /// <summary>
         /// Composes the declaration interpreter based on the current context.
         /// </summary>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         public And Compose()
         {
            switch (Context)
            {
               case CDeclInterpretContext.global_var:
               case CDeclInterpretContext.local_var:
                  return DeclSpecifiers & (FunctionDefinition | TypedefInitDeclarationList | InitDeclarationList);

               case CDeclInterpretContext.console_var: return DeclSpecifiers & (TypedefInitDeclarationList | InitDeclarationList);

               case CDeclInterpretContext.function_decl_param:
               case CDeclInterpretContext.function_def_param:
                  return DeclSpecifiers & FunctionParameters;

               case CDeclInterpretContext.cclass_field:
                  //struct member: interpreter for single declarator (eg '(int) a[5]') 
                  return DeclSpecifiers & (new InnerEmptyDeclSpecifier() | new May(new Lst(",", LocalAndGlobalSingleVar, false)) & Semicolon);

               case CDeclInterpretContext.expr_type_name:
               case CDeclInterpretContext.function_id:
               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// Checks if the declaration specifier is a typedef
      /// </summary>
      private class InnerTypedefCheck : TokenInterpreter<CCompilerInData, CTokenInterpreterOutput>
      {
         public InnerTypedefCheck() { }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) => 
            output.PeekOrCrash<CDeclSpecifiers>().StorageClass == CTypeStorageClass.typedef ? 
               TxtElabResult.success : TxtElabResult.continue_searching;
      }

      /// <summary>
      /// Checks for empty declarator specifier
      /// </summary>
      private class InnerEmptyDeclSpecifier : TokenInterpreter<CCompilerInData, CTokenInterpreterOutput>
      {
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var dcl_spc = output.PeekOrCrash<CDeclSpecifiers>();
            var typ_pri = new CTypeAlias(dcl_spc.TypeBase).PrimitiveAlias;

            if (input.MarkedText == ";" && !(typ_pri.TypeBase is ITypeClass) && !inData.Settings.AreEmptyDeclSpecifierInStructAccepted)
            {
               inData.Messages.Add(CCompilerMsgs.EmptyDeclSpecifier(input.MarkedToken));

               return TxtElabResult.failure;
            }
            else { return TxtElabResult.continue_searching; }//executes declarator list interpreter
         }
      }

      public static DeclBeginningTokenIdDataType BeginningTokenIdData { get; private set; } = new DeclBeginningTokenIdDataType();

      public CDeclInterpretContext Context { get; }

      public CExprStatementInterpreter ExprInterpret { get; }

      public CAttributesInterpret AttributesInterpret { get; }

      public CDeclInterpretFactory DeclInterpretFactory { get; }

      /// <summary>
      /// Processes the input token list to perform a declaration analysis and updates the output accordingly.
      /// </summary>
      /// <remarks>This method performs a declaration analysis by examining the input tokens and determining
      /// whether  they represent a valid declaration or an expression. If a declaration is successfully processed,  it
      /// is added to the scope space. Otherwise, the method ensures that any partially added declarations  are removed
      /// to maintain consistency.</remarks>
      /// <param name="input">The list of tokens to be analyzed. The current index of the token list is used as the starting point for
      /// processing.</param>
      /// <param name="inData">The compiler input data containing contextual information such as scope helpers and application data.</param>
      /// <param name="output">A reference to the output object that will be updated with the results of the analysis, including scope space
      /// modifications.</param>
      /// <returns>A <see cref="TxtElabResult"/> value indicating the result of the operation.  Returns <see
      /// cref="TxtElabResult.success"/> if the analysis is successful,  or <see
      /// cref="TxtElabResult.continue_searching"/> if further processing is required.</returns>
      /// <exception cref="Crash">Thrown if adding the declaration specifiers to the scope space fails unexpectedly.</exception>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var beg_idx = input.CurrIdx;
         var dcl_spc = new CDeclSpecifiers();
         var itm_sco = output.ScopeSpaceItem ?? throw new Crash();

         inData.AppData[BeginningTokenIdData] = input.CurrIdx;

         if (!itm_sco.AddToScopeSpace(dcl_spc, inData.ScopeHelper, inData.Messages)) { throw new Crash(); }

         var res = myNested(dcl_spc, input, inData, ref output, myLazyInterpretComposer.Value, NestedMode.once_continue);

         if (res == TxtElabResult.success) { dcl_spc.TxtToken = input.GetTokenFrom(beg_idx); }
         else { itm_sco.RemoveFromScopeSpace(dcl_spc); }

         return res;
      }

      public override string ToString() => $"{GetType().Name}({Context})";
   }
}
