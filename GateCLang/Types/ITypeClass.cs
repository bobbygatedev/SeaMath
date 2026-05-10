using Gate.CLanguage.Decl;
using Gate.LangBase;
using System;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// Interface for <see cref="CTypeStruct"/>(ie struct/union) <see cref="Cpp.CppTypeClass"/> (ie struct/union/class)
   /// </summary>
   public interface ITypeClass : IWithIdentifier
   {  
      /// <summary>
      /// Array of all fields of class.
      /// </summary>
      CDeclClassField[] Fields { get; }

      /// <summary>
      /// Built-in integer fields declared as bitfield are gathered into groups other fields remains stand-alone.
      /// </summary>
      CTypeClassFieldGroup[] FieldGroups { get; }

      /// <summary>
      /// struct/union/class
      /// </summary>
      CTypeUserTag Kind { get; }
      
      /// <summary>
      /// All members (fields + methods).
      /// </summary>
      CDecl[] Members { get; }
      
      /// <summary>
      /// eg 'struct MyStruct'
      /// </summary>
      string TypeSpecifier { get; }

      /// <summary>
      /// 
      /// </summary>
      Type? CSharpTypeForStorage { get; }

      /// <summary>
      /// 
      /// </summary>
      ITypeClassBody Body { get; }

      /// <summary>
      /// 
      /// </summary>
      int Alignement { get; }
   }
}