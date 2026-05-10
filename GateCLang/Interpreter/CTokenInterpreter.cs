using Gate.CLanguage.Compiler;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Interpreter
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CTokenInterpreter : TokenInterpreter<CCompilerInData, CTokenInterpreterOutput>
   {
      /// <summary>
      /// 
      /// </summary>
      public enum NestedMode
      {
         /// <summary>
         /// Nested interpreter is called once in case of  of <see cref="TxtElabResult.continue_searching"/> continue is returned.
         /// </summary>
         once_continue = 0,

         /// <summary>
         /// Nested interpreter is called once in case of <see cref="TxtElabResult.continue_searching"/> <see cref="CCompilerMsgs.CCompilerMsgId.c005_unexpected_token"/> is raised
         /// </summary>
         once_not_continue = 1,
      }

      public delegate bool OutputEndHandler(TxtTokenList input);

      /// <summary>
      /// 
      /// </summary>
      private static SkipInstructionInterpret mySkipInstruction = new SkipInstructionInterpret();

      protected TxtElabResult myNestedIterate(
         CItem item,
         TxtTokenList input,
         CCompilerInData inData,
         ref CTokenInterpreterOutput output,
         TxtElab<TxtTokenList, CCompilerInData, CTokenInterpreterOutput> nestedInterpret,
         OutputEndHandler outputEndHandler)
      {
         var cln_out = output.Clone() as CTokenInterpreterOutput ?? throw new Crash();
         var has_itm_add = false;
         var itr_res = TxtElabResult.success;

         if (item != null && !output.ItemsOnStack.Contains(item))
         {
            output.Push(item);
            has_itm_add = true;
         }

         while (true)
         {
            if (outputEndHandler(input))
            {
               //in this case exits from nested
               myExitNested(item, output, has_itm_add);

               return itr_res;
            }

            var beg_idx = input.CurrIdx;
            var res = nestedInterpret.Perform(input, inData, ref output);

            switch (res)
            {
               case TxtElabResult.success: continue;

               case TxtElabResult.continue_searching:
                  {
                     if (input.IsIn)
                     {
                        input.CurrIdx = beg_idx;
                        inData.Messages.Add(CCompilerMsgs.UnexpectedToken(input.MarkedToken));

                        if (mySkipInstruction.Perform(input, inData, ref output) == TxtElabResult.failure)
                        {
                           itr_res = TxtElabResult.failure;
                           continue;
                        }
                        else { return TxtElabResult.failure_unrecoverable; }
                     }
                     else
                     {
                        inData.Messages.Add(CCompilerMsgs.EndOfFileReached(input?.MarkedToken));

                        //in this case unexpected token
                        myExitNested(item, output, has_itm_add);

                        return TxtElabResult.failure_unrecoverable;
                     }
                  }

               case TxtElabResult.failure_unrecoverable:
                  input.CurrIdx = input.Count();//moves to end
                  return res;

               case TxtElabResult.failure:
                  if (input.IsIn)
                  {
                     itr_res = TxtElabResult.failure;
                     continue;
                  }
                  else { return TxtElabResult.failure_unrecoverable; }

               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="item"></param>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <param name="nestedInterpret"></param>
      /// <param name="nestedMode"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      protected TxtElabResult myNested(
         CItem? item,
         TxtTokenList input,
         CCompilerInData inData,
         ref CTokenInterpreterOutput output,
         TxtElab<TxtTokenList, CCompilerInData, CTokenInterpreterOutput> nestedInterpret,
         NestedMode nestedMode)
      {
         var cln_out = output.Clone() as CTokenInterpreterOutput ?? throw new Crash();
         var has_itm_add = false;

         if (item != null && !output.ItemsOnStack.Contains(item))
         {
            output.Push(item);
            has_itm_add = true;
         }

         var beg_idx = input.CurrIdx;

         var res = nestedInterpret.Perform(input, inData, ref output);

         switch (res)
         {
            case TxtElabResult.success:
               myExitNested(item, output, has_itm_add);

               return TxtElabResult.success;

            case TxtElabResult.continue_searching:
               if (nestedMode == NestedMode.once_not_continue)
               {
                  if (input.IsIn)
                  {
                     input.CurrIdx = beg_idx;
                     inData.Messages.Add(CCompilerMsgs.UnexpectedToken(input.MarkedToken));

                     return mySkipInstruction.Perform(input, inData, ref output);
                  }
                  else
                  {
                     inData.Messages.Add(CCompilerMsgs.EndOfFileReached(input.MarkedToken));

                     return TxtElabResult.failure_unrecoverable;
                  }
               }
               else if (nestedMode == NestedMode.once_continue)
               {
                  output = cln_out;

                  return TxtElabResult.continue_searching;
               }
               else { throw new Crash(); }

            case TxtElabResult.failure_unrecoverable:
               input.CurrIdx = input.Count();//moves to end
               return res;

            case TxtElabResult.failure:
               if (input.IsIn)
               {
                  myExitNested(item, output, has_itm_add);
                  input.CurrIdx = beg_idx;

                  return mySkipInstruction.Perform(input, inData, ref output);
               }
               else { return TxtElabResult.failure_unrecoverable; }

            default: throw new Crash();
         }
      }

      private static void myExitNested(CItem? item, CTokenInterpreterOutput output, bool hasItemAdded)
      {
         ///pops everything above itemWithScope
         if (output.ItemsOnStack.Contains(item))
         {
            output.GetAllItemsAbove<CItem>(item, true);

            if (hasItemAdded && output.PopOrCrash<CItem>() != item) { throw new Crash(); }
         }
         else { throw new Crash($"Can't delete {item}"); }
      }

      /// <summary>
      /// 
      /// </summary>
      public class SkipInstructionInterpret : CTokenInterpreter
      {
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
            //block is considered as a function/compund in this cases:
            //begins with "{"
            //a sequence ")", "{" function compound beginning is detected 
            var is_fnc = input.MarkedText == "{";

            var bh = new BracketHelper(
               [("{", "}", TxtElabResult.failure), ("[", "]", TxtElabResult.failure), ("(", ")", TxtElabResult.failure)], false, null, (i, s) =>
            {
               if (!is_fnc && i.MarkedText == "{" && i[i.CurrIdx - 1].Content == ")") { is_fnc = true; }

               return s.Count == 0 && (is_fnc || i.MarkedText == ";");
            });

            var res = bh.Check(input, out _);

            switch (res)
            {
               case TxtElabResult.success:
                  input.CurrIdx++;
                  return TxtElabResult.failure;//failure is assered but compile continues

               case TxtElabResult.continue_searching:
               case TxtElabResult.failure:
                  return TxtElabResult.failure_unrecoverable;

               case TxtElabResult.failure_unrecoverable:
               default:
                  throw new Crash();
            }
         }
      }

      public class IdSetInterpreter : CTokenInterpreter
      {
         public enum ModeType
         {
            /// <summary>
            /// If id is not found <see cref="Gate.CLanguage.Compiler.CCompilerMsgs.CCompilerMsgId.c006_expected_identifier"/> is raised!
            /// </summary>
            required = 0,

            /// <summary>
            /// If id not found returns <see cref="TxtElabResult.success"/>
            /// </summary>
            optional_success = 1,

            /// <summary>
            /// If id not found returns <see cref="TxtElabResult.continue_searching"/>
            /// </summary>
            optional_continue = 2,
         }

         public IdSetInterpreter(ModeType mode) => Mode = mode;

         public ModeType Mode { get; }

         public override TxtElabResult Perform(TxtTokenList inputQueue, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (inputQueue.Peek<CToken>()?.TokenType == CTokenType.identifier)
            {
               var id_typ =
                     output.TopItem as IWithIdentifierSettable ??
                     throw new Crash($"{output.TopItem?.Descriptor} not {typeof(IWithIdentifierSettable).Name}");

               id_typ.Identifier = inputQueue.Dequeue()?.Content ?? "";

               return TxtElabResult.success;
            }
            else
            {
               switch (Mode)
               {
                  case ModeType.required:
                     return new Expect(CTokenType.identifier, false).PerformNoOutput(inputQueue, inData);

                  case ModeType.optional_success: return TxtElabResult.success;
                  case ModeType.optional_continue: return TxtElabResult.continue_searching;
                  default: throw new Crash();
               }
            }
            //precondition
         }

         public override string ToString() => $"{GetType().Name}({Mode})";
      }

      public class Expect : CTokenInterpreter
      {
         public Expect(string what, bool isMove)
         {
            What = what;
            IsMove = isMove;
            TokenType = CTokenType.unspecified;
         }

         public Expect(CTokenType tokenType, bool isMove)
         {
            What = null;
            IsMove = isMove;
            TokenType = tokenType;
         }

         public CTokenType TokenType { get; private set; }

         public bool IsMove { get; private set; }

         public string? What { get; private set; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var is_ok = false;
            var err_wht = What.ExtTrim();

            if (What == null)
            {
               is_ok = input.IsIn && input.Peek<CToken>()?.TokenType == TokenType;
               err_wht = TokenType.ToString();
            }
            else
            {
               is_ok = input.MarkedText == What;
            }

            if (is_ok)
            {
               if (IsMove) { input.CurrIdx++; }

               return TxtElabResult.success;
            }
            else if (input.Peek() == null)
            {
               inData.Messages.Add(CCompilerMsgs.EndOfFileReached(input.Last(), err_wht));

               return TxtElabResult.failure_unrecoverable;
            }
            else
            {
               inData.Messages.Add(CCompilerMsgs.ExpectedToken(input.Peek(), err_wht));

               return TxtElabResult.failure;
            }
         }

         public override string ToString() => $"{GetType().Name}:{What}";
      }

      public class IsCTokenType : CTokenInterpreter
      {
         public IsCTokenType(CTokenType ctokenType, bool isMove)
         {
            CTokenType = ctokenType;
            IsMove = isMove;
         }

         public bool IsMove { get; private set; }

         public CTokenType CTokenType { get; private set; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var condition = new Condition(tl => tl.Peek<CToken>()?.TokenType == CTokenType, IsMove);

            return condition.Perform(input, inData, ref output);
         }
      }

      public override string ToString() => GetType().Name;
   }
}
