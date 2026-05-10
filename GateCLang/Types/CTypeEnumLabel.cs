using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Types.BuiltIns;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// Encapsulates an enumeration label eg 'enum { enu_label = 5 , }
   /// </summary>
   public class CTypeEnumLabel : CItem, IWithIdentifierSettable
   {
      private int? myInternalLabelValue = null;

      public CTypeEnumLabel(CTypeBuiltIn? typeBuiltIn = null)
      {
         typeBuiltIn = typeBuiltIn ?? new CTypeBinInt.Int("int", 4);
         var dva = new CDeclVar();
         var dsp = new CDeclSpecifiers();

         dsp.AddDecl(dva);
         dsp.StorageClass = CTypeStorageClass.@static;
         dsp.TypeQualifiers = CTypeQualifiersFlags.@const;
         dsp.TypeBase = typeBuiltIn;

         myAddSubItem(dsp);
      }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => true;

      /// <summary>
      /// 
      /// </summary>
      public string? Identifier { get => AssociatedDecl?.Identifier; set => (AssociatedDecl??throw new Crash()).Identifier = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => Identifier.ExtTrim() == "";

      /// <summary>
      /// 
      /// </summary>
      public int IntValue
      {
         get
         {
            if (LabelExpr != null)
            {
               if (LabelExpr.ConstIntValue != null) { return LabelExpr.ConstIntValue.Value; }
               else { throw new Gate.Tools.ToolsException("Label expression to be an integer onstant."); }
            }
            else if (myInternalLabelValue != null) { return myInternalLabelValue.Value; }
            else { throw new Gate.Tools.ToolsException("Label Value not defined."); }
         }

         set => myInternalLabelValue = value;
      }

      public override string Rebuilt => $"{Identifier} = {IntValue}";

      public CDeclVar? AssociatedDecl => SubItems.OfType<CDeclSpecifiers>().First().Decls[0] as CDeclVar;

      public CExprStatement? LabelExpr { get; set; }

      public override string Descriptor
      {
         get
         {
            var id = Identifier ?? "";

            return LabelExpr != null ? $"{id}={IntValue}" : $"{id}(={IntValue})";
         }
      }
   }
}
