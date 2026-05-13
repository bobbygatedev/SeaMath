using Gate.LangBase;

namespace Gate.CLanguage.Compiler
{
   /// <summary>
   /// 
   /// </summary>
   public enum CCompilerMsgId
   {
      inf_no_error = 0,

      no_matching_bracket = 1,
      end_of_file_reached = 2,
      unexpected_token = 3,
      expected_identifier = 4,
      expected_an_integer_value = 5,
      two_or_more_types_in_declation = 6,
      empty_enum_is_invalid = 7,

      redefined_identifier = 10,
      redeclared_enumerator,
      unexpected_in_this_scope,
      nested_function_call_not_supported,
      builtin_type_not_defined,
      not_an_in_expr,
      expected_constant_expression,
      expected_expression,
      expected_token,
      expected_compound,

      expected_compound_or_semicolon = 20,
      expected_if_or_else,

      [CompilerMessage(Message = "Can't concatenate different type of strings.")]
      cant_concatenate_strings = 22,

      [CompilerMessage(Message = "Invalid init type for string.")]
      invalid_string_init = 23,
      wrong_char_after_numeric_constant = 24,
      unexpected_char_during_tokenise = 25,
      too_many_digits_in_hex_constant = 26,
      invalid_suffix = 27,
      expected_hex_number = 28,
      [CompilerMessage(Message = "At least a digit for an hex constant.")]
      hex_few_digit = 29,
      expected_bin_number = 30,
      too_many_characters_in_constant = 31,
      not_a_valid_universal_character = 32,

      assign_to_constant = 40,
      cant_convert_to,
      no_operands_for_operator,
      member_rvalue_not_a_name,
      member_not_defined_in_class,
      member_lvalue_not_a_class,
      member_lvalue_not_a_class_pointer,
      addressof_from_bitfield,

      not_a_function = 50,
      too_many_function_params = 51,
      not_enough_function_params = 52,
      duplicated_storage_class = 53,
      duplicated_type_modifier = 54,
      invalid_built_in_specifier = 55,
      invalid_type_for_operator = 56,
      default_int_not_valid = 57,
      duplicated_type_id = 58,
      empty_struct = 59,

      type_name_or_expression_expected_for_sizeof = 60,
      typedef_in_struct = 61,
      empty_decl_specifier = 62,

      cant_cast_to = 70,
      conflicting_types = 71,
      incomplete_type_not_allowed = 72,
      init_var_size_array_not_allowed = 73,
      var_size_array_not_allowed = 74,
      incomplete_size_array_not_allowed = 75,
      incompatible_type_qualifiers = 76,
      incompatible_storage_class = 77,
      init_returning_void = 78,
      init_invalid_type = 79,

      expected_pointer_or_array = 80,
      expected_numeric_type = 81,

      too_many_initializers = 90,
      not_constant_init_in_global_scope = 91,
      invalid_scalar_init = 92,
      field_not_found = 93,
      empty_scalar_init = 94,
      invalid_struct_init = 95,

      /// <summary>
      /// Category pragma pack warning
      /// </summary>
      pragma_pack_show = 100,
      not_a_pragma_pack_to_pop = 101,
      invalid_pointer_algebric_operation = 102,

      /// <summary>
      /// category bit field
      /// </summary>
      not_an_integer_count_for_bit_field = 200,
      too_many_bit_for_field = 200,

      [CompilerMessage(Message = "break not inside valid cycle(for,switch,while,do-while).")]
      break_invalid = 301,

      [CompilerMessage(Message = "continue not inside valid cycle(for,while,do-while).")]
      continue_invalid = 302,

      [CompilerMessage(Message = "type of condition shall be a built-in pointer type.")]
      condition_type_invalid = 303,
   }
}
