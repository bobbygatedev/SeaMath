using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.DeclSpecifiers
{
   /// <summary>
   /// Interpreter for C declaration specifiers (eg 'static const int', 'typedef struct MyStruct', etc)
   /// eg inside char v,*pv; -> 'char' is the decl specifier for both v and pv
   /// </summary>
   internal class CDeclSpecifiersInterpret : CTokenInterpreter
   {
      private readonly And myInterpreterAnd;

      /// <summary>
      /// Initializes a new instance of the <see cref="CDeclSpecifiersInterpret"/> class.
      /// </summary>
      /// <param name="subElabs"></param>
      internal CDeclSpecifiersInterpret(CTokenInterpreter[] subElabs) =>
         myInterpreterAnd = new InnerPreCheck() & new IterateWhileSuccess(new Or(subElabs), true);//at least one success   

      /// <summary>
      /// Represents an internal pre-check operation for token interpretation within the compiler process.
      /// </summary>
      /// <remarks>This class is a specialized implementation of <see cref="CTokenInterpreter"/> that
      /// performs pre-checks on tokens to determine their validity and classification within the context of compiler
      /// input data. It evaluates tokens based on their type, scope visibility, and other contextual rules, returning a
      /// result that guides further processing.</remarks>
      private class InnerPreCheck : CTokenInterpreter
      {
         /// <summary>
         /// 
         /// </summary>
         public InnerPreCheck() { }

         /// <summary>
         /// <br>Precondition testing(on first token of statement):</br>
         /// <br> - is ';' is empty declaration return <see cref="TxtElabResult.success"/> interpretation follows </br>
         /// <br> - is <see cref="CTokenType.keyword"/> <see cref="TxtElabResult.success"/> interpretation follows </br>
         /// <br> - is <see cref="CTokenType.identifier"/> following check is made on identifier:</br>
         /// <br>    - is a var/function name, we presumibly are on a expression and therefore returns <see cref="TxtElabResult.continue_searching"/></br>
         /// <br>    - is a typedef name returns <see cref="TxtElabResult.success"/> interpretation follows </br>
         /// <br>    -  otw its an unknown identifier </br>
         /// <br>       -  if it's followed by '(' or ';' and default-int is enabled may be a default-int declaration returns <see cref="TxtElabResult.success"/> interpretation follows </br>
         /// <br>       -  otw returns <see cref="TxtElabResult.continue_searching"/> (probably an error) </br>
         /// <br> - otherwise <see cref="TxtElabResult.continue_searching"/> interpretation is skipped </br>
         /// </summary>         
         /// <param name="input">The list of tokens to be analyzed. Must not be null and should contain a marked token.</param>
         /// <param name="inData">The compiler input data providing context for the analysis, such as scope and settings. Must not be null.</param>
         /// <param name="output">A reference to the output object where the results of the token interpretation are stored. Must not be
         /// null.</param>
         /// <returns>A <see cref="TxtElabResult"/> value indicating the result of the analysis: <list type="bullet">
         /// <item><description><see cref="TxtElabResult.success"/> if the analysis determines a valid
         /// result.</description></item> <item><description><see cref="TxtElabResult.continue_searching"/> if further
         /// analysis is required to determine the result.</description></item> </list></returns>
         /// <exception cref="Crash">Thrown if the marked token in the input is not of type <see cref="CToken"/>.</exception>
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (input.IsIn)
            {
               var ctk = input.MarkedToken as CToken ?? throw new Crash();

               switch (ctk.TokenType)
               {
                  case CTokenType.keyword: return TxtElabResult.success;

                  case CTokenType.identifier:
                     var tok = input.Peek<CToken>();

                     //identifier on current token
                     var id = tok?.Content;

                     //identifier is a previous declaration? (eg 'int a; ->a=2;)
                     var is_dcs =
                        inData.ScopeHelper.GetDeclStoragesFunctionVisible(output.ScopeSpaceItem ?? throw new Crash()).
                        Any(d => d.Identifier == id);

                     //maybe an expression
                     if (is_dcs) { return TxtElabResult.continue_searching; }

                     //identifer a typedef 
                     var tds_vis = inData.ScopeHelper.GetTypedefsFunctionVisible(output.ScopeSpaceItem).ToArray();
                     var is_tdf = tds_vis.Any(t => t.Identifier == id);

                     if (!is_tdf)
                     {
                        //if current token and default int is acceptable check if it's an implicit-int declaration
                        //otw check for expression
                        if (inData.Settings.IsDefaultIntAcceptable)
                        {
                           var nxt_tok = input.Peek(1)?.Content;

                           //if identifier has not been defined yet and next token is '(' or ';' then is a possible default-int declaration (eg x; f();)
                           if (nxt_tok != ";" && nxt_tok != "(") { return TxtElabResult.continue_searching; }
                        }
                        else
                        {
                           //in case default-int is not valid && identifier then may be an expression
                           return TxtElabResult.continue_searching;
                        }
                     }

                     return TxtElabResult.success;

                  default: return input.MarkedText == ";" ? TxtElabResult.success : TxtElabResult.continue_searching;
               }
            }
            else
            {
               return TxtElabResult.continue_searching;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var inp = input.Peek();
         var beg_idx = input.CurrIdx;

         var res = myInterpreterAnd.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var dcl_spc = output.PeekOrCrash<CDeclSpecifiers>();
            var str_bdy = output.PeekOrDefault<CTypeStructBody>(1);

            if (dcl_spc.TypeBase == null)
            {
               if (inData.Settings.IsDefaultIntAcceptable)
               {
                  //default int 
                  dcl_spc.DefaultInt =
                     inData.Settings.BuiltInSet?.FirstOrDefault(bi => bi.TypeSpecifier == "int") ??
                     throw new Crash("Implementation doesn't define int type");
               }
               else
               {
                  inData.Messages.Add(CCompilerMsgId.default_int_not_valid.GetError(inp));
               }
            }

            if (str_bdy != null)
            {
               //in C is not allowed to place a typedef inside a struct 
               //eg 'struct MyStruct { typedef int Int_t; };'
               if ((dcl_spc.StorageClass & CTypeStorageClass.typedef) == CTypeStorageClass.typedef)
               {
                  inData.Messages.Add(CCompilerMsgs.TypedefInStruct(inp));

                  return TxtElabResult.failure;
               }
            }

            if (input.CurrIdx > beg_idx) { dcl_spc.TxtToken = input.GetTokenFrom(beg_idx); }
         }

         return res;
      }

      public override string ToString() => GetType().Name;
   }
}
