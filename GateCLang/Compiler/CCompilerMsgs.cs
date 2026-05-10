using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.PrePx.Directives.PragmaKinds;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.CLanguage.Compiler
{
   /// <summary>
   /// 
   /// </summary>
   public static class CCompilerMsgs
   {
      private static CompilerMessagesTools myMsgTools = new CompilerMessagesTools("C");

      public static Msg[] NoMatchingBracket(TxtToken? wrongToken, TxtToken? openToken, bool isFatal)
      {
         var m1 = myMakMsg(
            isFatal ? MsgType.error : MsgType.error,
            CCompilerMsgId.no_matching_bracket,
            wrongToken);

         var m2 = myMakMsg2(
            MsgType.info,
            CCompilerMsgId.no_matching_bracket,
            openToken,
            $"      Open token was '{{{openToken?.Content}}}'.");

         return [m1, m2];
      }

      /// <summary>
      /// Returns EndOfFileReached unrecoverable error when a open bracket is not closed.
      /// </summary>
      /// <param name="lastToken"></param>
      /// <param name="openBracketToken"></param>
      /// <returns></returns>
      public static Msg EndOfFileReachedOpenBracket(TxtToken? openBracketToken, bool isFatal) =>
         myMakMsg2(
            isFatal ? MsgType.error : MsgType.error,
            CCompilerMsgId.end_of_file_reached,
            openBracketToken,
            $"End of file reached trying to close '{openBracketToken?.Content}'.");

      /// <summary>
      /// Returns a generic EndOfFileReached unrecoverable error.
      /// </summary>
      /// <param name="lastToken"></param>
      /// <returns></returns>
      public static Msg EndOfFileReached(TxtToken? lastToken) =>
         myMakErr(CCompilerMsgId.end_of_file_reached, lastToken);

      public static Msg EndOfFileReached(TxtToken? lastToken, string searchingWhat) =>
         myMakMsg2(
            MsgType.fatal,
            CCompilerMsgId.end_of_file_reached,
            lastToken,
            $"End of file reached when searching for {searchingWhat}.");

      public static Msg UnexpectedToken(TxtToken? token) =>
         myMakMsg2(
           MsgType.fatal,
           CCompilerMsgId.unexpected_token,
           token,
           $"Unexpected token '{token?.Content}'");

      public static Msg[] RedefindedIdentifier<CITEM>(CITEM itemRedeclared, CITEM itemPrevious) where CITEM : CItem, IWithIdentifier =>
         [
            myMakErr2(CCompilerMsgId.redefined_identifier , itemRedeclared.TxtToken ,
               $"Redefinition of identifier '{itemRedeclared.Identifier}'."),
            myMakInf($"             Previous declaration of '{itemPrevious.Identifier}' was here." ,
               CCompilerMsgId.redefined_identifier , itemPrevious.TxtToken )];

      public static Msg[] RedeclaredEnumerator(CTypeEnumLabel labelRedeclared, CTypeEnumLabel labelPrevious) => [
            myMakErr(  CCompilerMsgId.redeclared_enumerator , labelRedeclared.TxtToken , $" {labelRedeclared.Identifier}"),
            myMakInf(
               $"             Previous definition of '{labelPrevious.Identifier}' was here." ,
               CCompilerMsgId.redeclared_enumerator ,
               labelPrevious.TxtToken )];

      public static Msg BuiltInTypeNotDefined(string builtInName, TxtToken? token) =>
         myMakErr2(
            CCompilerMsgId.builtin_type_not_defined,
            token,
            $"Built-in type '{builtInName}' is not defined in this standard.");

      public static Msg ExpectedToken(TxtToken? token, string expToken) =>
         myMakMsg2(MsgType.error, CCompilerMsgId.expected_token, token, $"'{expToken}' was expected.");

      public static Msg WrongCharAfterNumericConstant(TxtToken charToken) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.wrong_char_after_numeric_constant,
            charToken,
            $"Wrong char '{charToken.Content}' after numeric constant.");

      public static Msg UnexpectedCharDuringTokenise(TxtToken charToken) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.unexpected_char_during_tokenise,
            charToken,
            $"Unexpected char '{charToken.Content}' during tokenise.");

      public static Msg TooManyDigitsInIntConstant(TxtToken intConstantToken) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.too_many_digits_in_hex_constant,
            intConstantToken,
            $"Too many digits in int constant '{intConstantToken.Content}'.");

      public static Msg InvalidSuffix(TxtToken suffixToken) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.invalid_suffix,
            suffixToken,
            $"Invalid Suffix '{suffixToken.Content}'.");

      public static Msg NotAValidUniversalCharacter(TxtTokenConst token, MsgType msgType = MsgType.error) =>
         myMakMsg2(
            msgType,
            CCompilerMsgId.not_a_valid_universal_character,
            token,
            $"{token} is not a valid universal character");

      public static Msg AssignToConst(TxtToken? token) =>
         myMakErr(CCompilerMsgId.assign_to_constant, token);

      public static Msg CantConvertTo(TxtToken? token, string targetType) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.cant_convert_to,
            token,
            $"Can't convert '{token?.Content}' to '{targetType}'.");

      public static Msg NotOperandsForOperator(TxtToken? token) => 
         myMakErr2(
            CCompilerMsgId.no_operands_for_operator,
            token,
            $"No input operands for operator '{token?.Content}'.");

      public static Msg MemberNotExists(TxtToken? token, string? clsName) => 
         myMakErr2(
            CCompilerMsgId.member_not_defined_in_class,
            token,
            $"A member named '{token}' not defined in class '{clsName}'.");

      public static Msg MemberLValueNotAClass(TxtToken? token) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.member_lvalue_not_a_class,
            token,
            $"Member operator L-Value '{token?.Content}' doesn't represent a class.");

      public static Msg MemberLValueNotAClassPointer(TxtToken? token) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.member_lvalue_not_a_class_pointer,
            token,
            $"Member operator L-Value '{token?.Content}' doesn't represent a class pointer.");

      public static Msg Not_A_Function(TxtToken? token) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.not_a_function,
            token,
            $"Can't call '{token?.Content}' which is neither a function nor a function pointer.");

      public static Msg Not_Too_Many_Params_For_Function(TxtToken? token) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.too_many_function_params,
            token,
            $"Too many input params for function '{token?.Content}'.");

      public static Msg Not_Enough_Params_For_Function(TxtToken? token) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.not_enough_function_params,
            token,
            $"Not enough input params for function '{token?.Content}'.");

      public static Msg DuplicatedStorageClass(TxtToken? token) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.duplicated_storage_class,
            token,
            $"Double Storage class '{token?.Content}' in declaration.");

      public static Msg InvalidBuiltInType(CToken token, string wrongBuiltIn) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.invalid_built_in_specifier,
            token,
            $"Invalid built-in type specifier '{wrongBuiltIn}'.");

      public static Msg[] DuplicatedTypeIdentifier(CTypeUserDefined type, CTypeUserDefined otherType)
      {
         var m1 = myMakMsg2(
            MsgType.error,
            CCompilerMsgId.duplicated_type_id,
            type.TxtToken,
            $"Duplicated type identfier '{type.Identifier}'.");

         var m2 = myMakMsg2(MsgType.info,
            CCompilerMsgId.duplicated_type_id,
            otherType.TxtToken,
             $"                                        previous instance of '{type.Identifier}'.");

         return [m1, m2];
      }

      public static Msg EmptyStruct(TxtToken? token) => myMakErr(CCompilerMsgId.empty_struct, token);

      public static Msg TypedefInStruct(TxtToken? token) => myMakErr(CCompilerMsgId.typedef_in_struct, token);

      public static Msg EmptyDeclSpecifier(TxtToken? token) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.empty_decl_specifier,
            token,
            $"Empty declaration specifier not allowed here.");

      public static Msg CantCastTo(CTypeAlias lType, CTypeAlias rType) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.cant_cast_to,
            rType.TxtToken,
            $"Can't cast {rType.Descriptor} to {rType.Descriptor}");

      public static Msg[] ConflictingTypes(CDecl decl, CDecl oldDecl) => [
         myMakErr( CCompilerMsgId.conflicting_types, decl.TxtToken, $" for '{decl.Descriptor}'") ,
         myMakInf(
            $" previous declared as '{oldDecl.Descriptor}'",
            CCompilerMsgId.conflicting_types,
            oldDecl.TxtToken)];

      public static Msg IncompleteTypeNotAllowed(CTypeIncomplete typeIncomplete) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.incomplete_type_not_allowed,
            typeIncomplete.TxtToken,
            $"Incomplete type '{typeIncomplete.TxtToken?.Content}' not allowed.");

      public static Msg IncompleteSizeArrayNotAllowed(TxtToken? token) =>
         myMakErr(CCompilerMsgId.incomplete_size_array_not_allowed, token);

      public static Msg[] IncompatibleTypeQualifiers(CDeclSpecifiers declSpecifier, CDeclSpecifiers oldDeclSpecifier) =>
         [
            myMakErr( CCompilerMsgId.incompatible_type_qualifiers, declSpecifier.TxtToken, $" for '{declSpecifier.Descriptor}'"),
            myMakInf($" previous declared as '{oldDeclSpecifier.Descriptor}'" , CCompilerMsgId.incompatible_type_qualifiers, oldDeclSpecifier?.TxtToken)];

      public static Msg[] IncompatibleStorageClass(CDeclSpecifiers declSpecifier, CDeclSpecifiers oldDeclSpecifier) => [
            myMakErr( CCompilerMsgId.incompatible_storage_class, declSpecifier.TxtToken, $" for '{declSpecifier.Descriptor}'"),
            myMakInf(
               $" previous declared as '{oldDeclSpecifier.Descriptor}'" ,
               CCompilerMsgId.incompatible_storage_class,
               oldDeclSpecifier.TxtToken)];

      public static Msg InitInvalidType(CExprStatement expression) =>
         myMakErr2(
            CCompilerMsgId.init_invalid_type,
            expression.TxtToken,
            $"Invalid init fot type {expression.Type?.PrimitiveAlias.Descriptor}");

      public static Msg ExpectedPointerOrArray(TxtToken? token) =>
         myMakErr(CCompilerMsgId.expected_pointer_or_array, token);

      public static Msg ExpectedNumericType(TxtToken? token) =>
         myMakErr(CCompilerMsgId.expected_numeric_type, token);

      public static Msg TooManyInitializers(TxtToken? token, MsgType msgType) =>
         myMakMsg(msgType, CCompilerMsgId.too_many_initializers, token);

      public static Msg InvalidScalarInit(TxtToken? token) =>
         myMakErr(CCompilerMsgId.invalid_scalar_init, token);

      public static Msg FieldNotFound(CInitialisation initStructField, ITypeClass typeClass) =>
         myMakMsg2(
            MsgType.error,
            CCompilerMsgId.field_not_found,
            initStructField.TxtToken,
            $"Field '{initStructField.StructFieldName}' not found in class {typeClass.Identifier}.");

      public static Msg EmptyArrayInit(TxtToken? token) => myMakErr(CCompilerMsgId.empty_scalar_init, token);

      public static Msg InvalidStructInit(TxtToken? token) => myMakErr(CCompilerMsgId.invalid_struct_init, token);

      public static Msg PragmaShow(CPragmaKindPack pragmaKindPack, int currentPack) =>
         myMakMsg(MsgType.info, CCompilerMsgId.pragma_pack_show, pragmaKindPack.TxtToken, $" = {currentPack}");

      public static Msg NotPackToPop(CPragmaKindPack pragmaKindPack) =>
         myMakMsg(MsgType.warning, CCompilerMsgId.not_a_pragma_pack_to_pop, pragmaKindPack.TxtToken);

      public static Msg InvalidPointerAlgebricOperation(TxtToken? token) =>
         myMakErr(CCompilerMsgId.invalid_pointer_algebric_operation, token);

      public static Msg BitFieldNotAnInteger(TxtToken? token) =>
         myMakErr(CCompilerMsgId.not_an_integer_count_for_bit_field, token);

      public static Msg BitFieldToomManyBits(TxtToken? token) =>
         myMakErr(CCompilerMsgId.too_many_bit_for_field, token);

      public static Msg GetError(this CCompilerMsgId msgId, TxtToken? token, string? msgSuffix = null) =>
         myMsgTools.MakeMsg(MsgType.error, msgId, token, msgSuffix);

      public static Msg GetWarning(this CCompilerMsgId msgId, TxtToken? token, string? msgSuffix = null) =>
         myMsgTools.MakeMsg(MsgType.warning, msgId, token, msgSuffix);

      private static Msg myMakErr(CCompilerMsgId msgId, TxtToken? token, string? msgSuffix = null) =>
         myMsgTools.MakeMsg(MsgType.error, msgId, token, msgSuffix);

      private static Msg myMakInf(string infoMsg, CCompilerMsgId msgId, TxtToken? token) => 
         myMsgTools.MakeMsg2(MsgType.warning, msgId, token, infoMsg);

      private static Msg myMakMsg2(MsgType msgType, CCompilerMsgId msgId, TxtToken? token, string msgContent) =>
         myMsgTools.MakeMsg2(msgType, msgId, token, msgContent);

      private static Msg myMakErr2(CCompilerMsgId msgId, TxtToken? token, string msgContent) =>
         myMakMsg2(MsgType.error, msgId, token, msgContent);

      private static Msg myMakMsg(MsgType msgType, CCompilerMsgId msgId, TxtToken? token, string? msgSuffix = null) =>
         myMsgTools.MakeMsg(msgType, msgId, token, msgSuffix);
   }
}
