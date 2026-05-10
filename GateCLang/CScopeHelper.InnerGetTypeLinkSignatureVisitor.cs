using Gate.CLanguage.Types;
using Gate.Tools;
using System.Text;

namespace Gate.CLanguage
{
   public partial class CScopeHelper
   {
      private class InnerGetTypeLinkSignatureVisitor
      {
         public string GetTypeLinkSignature(CType type) => myGetTypeLinkSignature((dynamic)type);

         protected string myGetTypeLinkSignature(CType type) => throw new Crash("Not defined signature");

         protected virtual string myGetTypeLinkSignature(CTypeBuiltIn typeBuiltIn)
         {
            var nb = typeBuiltIn.SizeOf * 8;

            switch (typeBuiltIn.RepresentedType)
            {
               case CTypeBuiltInRepresent.@void: return "$void";
               case CTypeBuiltInRepresent.integer: return typeBuiltIn.IsUnsigned ? $"$ui{nb}" : $"$i{nb}";
               case CTypeBuiltInRepresent.floating_point: return $"$f{nb}";
               case CTypeBuiltInRepresent.complex_float: return $"$c{nb}";
               case CTypeBuiltInRepresent.boolean: return $"$b";
               default: throw new Gate.Tools.ToolsException("");
            }
         }

         protected virtual string myGetTypeLinkSignature(CTypeEnum typeEnum) => "int";

         protected virtual string myGetTypeLinkSignature(CTypeStruct typeClass) => typeClass.Identifier == null ? $"anonimous {typeClass.Kind}{typeClass.InstanceCounter}" : typeClass.TypeSpecifier;

         protected virtual string? myGetTypeLinkSignature(CTypeIncomplete incomplete) => 
            incomplete.IncompleteKind != CTypeUserTag.@enum ?
            (incomplete.CompleteType != null ? GetTypeLinkSignature(incomplete.CompleteType ?? throw new Crash()) : null) : "int";

         protected virtual string myGetTypeLinkSignature(CTypeAlias typeAlias)
         {
            var prm_ali = typeAlias.PrimitiveAlias;
            var sb = new StringBuilder();

            sb.Append($"{prm_ali?.TypeBase?.Signature} ");
            sb.Append($"{prm_ali?.TypeSubscriptSet?.Signature} ");

            if (prm_ali?.FunctionContainer != null)
            {
               sb.Append($"({string.Join(",", prm_ali.FunctionContainer.Select(p => myGetTypeLinkSignature(p.TypeAlias)))})");
            }

            return myOptimizeSpaces(sb.ToString());
         }
      }
   }
}
