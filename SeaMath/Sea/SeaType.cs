using Gate.CLanguage;
using Gate.CLanguage.Types;
using System;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaType : CTypePrimitive
   {
      public const string NAME = "sea";

      /// <summary>
      /// 
      /// </summary>
      private SeaType() : base(NAME) { }

      /// <summary>
      /// 
      /// </summary>
      public static SeaType Instance { get; } = new SeaType();

      /// <summary>
      /// 
      /// </summary>
      public override string TypeSpecifier => NAME;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsBuiltIn => true;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsConstant => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsClass => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsEnum => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsUserDefined => false;

      /// <summary>
      /// 
      /// </summary>
      public override string DescriptorGcc => Descriptor;

      /// <summary>
      /// 
      /// </summary>
      public override Type CSharpTypeForStorage => typeof(SeaTypeContent);

      /// <summary>
      /// 
      /// </summary>
      public override CTypeBuiltIn? BuiltIn => null;

      /// <summary>
      /// 64bit
      /// </summary>
      public override int SizeOf => 8;

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => NAME;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="scopeHelper"></param>
      /// <returns></returns>
      public override string GetSignature(CScopeHelperBase? scopeHelper) => NAME;
   }
}
