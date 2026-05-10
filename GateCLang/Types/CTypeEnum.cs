using Gate.CLanguage.Compiler;
using Gate.CLanguage.Types.BuiltIns;
using Gate.Tools;
using Gate.Tools.Message;
using System.Text;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeEnum : CTypeUserDefined
   {
      public CTypeEnum(string? name = null) : base(name) { }

      public override string? TypeSpecifier => Identifier != null && Identifier.Trim() != "" ? $"enum {Identifier}" : null;

      public override bool IsBuiltIn => false;

      public override CTypeUserDefined[]? SubTypes => null;

      public CTypeEnumLabel[] Labels => SubItems.OfType<CTypeEnumLabel>().ToArray();

      public override CTypeUserTag Kind => CTypeUserTag.@enum;

      public override int SizeOf => 4;

      public bool AddLabel(CTypeEnumLabel label, CCompilerSettings settings, MsgCollection messages)
      {
         if (Labels.Any(c => c.Identifier == label.Identifier))
         {
            messages.Add(CCompilerMsgs.RedeclaredEnumerator(label, Labels.First(c => c.Identifier == label.Identifier)));

            return false;
         }
         else
         {
            var cs = ContainingScope?.ItemWithScopeSpace;
            var old_dcl = cs?.ScopeDecls.Where(d => !d.IsAnonimous).FirstOrDefault(c => label.Identifier == c.Identifier);

            if (cs != null && old_dcl != null)
            {
               (label.AssociatedDecl??throw new Crash()).TxtToken = label.TxtToken;
               messages.Add(CCompilerMsgs.RedefindedIdentifier(label.AssociatedDecl, old_dcl));

               return false;
            }

            myAddSubItem(label);
            myRecalculateIntValues();

            return true;
         }
      }

      public bool RemoveLabel(CTypeEnumLabel label, CCompilerSettings settings, MsgCollection messages)
      {
         if (Labels.Contains(label))
         {
            myRecalculateIntValues();

            return myRemoveSubItem(label);
         }
         else { return false; }
      }

      private void myRecalculateIntValues()
      {
         var val = 0;

         foreach (var lab in Labels)
         {
            if (lab.LabelExpr == null) { lab.IntValue = val++; }
            else
            {
               if (lab.LabelExpr != null)
               {
                  val = lab.LabelExpr.ConstIntValue.HasValue ?
                     lab.LabelExpr.ConstIntValue.Value + 1 :
                     throw new Gate.CLanguage.CLangException($"Not an integer value");
               }
            }
         }
      }

      public override bool IsClass => false;

      public override bool IsEnum => true;

      public override bool IsConstant => false;

      /// <summary>
      /// Underlying built-in int type ie int in C, in C++ is int by default and any other integer type if specified in decl (eg 'enum X : long { a };')
      /// </summary>
      public CTypeBinInt? UnderlyingIntType { get; set; }

      public override string DescriptorGcc => throw new NotImplementedException();

      public override string Descriptor
      {
         get
         {
            switch (Labels.Length)
            {
               case 0: return string.Format("{0}{{err no constants}}", TypeSpecifier);
               case 1: return TypeSpecifier + "{" + Labels.First().Descriptor + "}";
               default: return TypeSpecifier + "{" + Labels.First().Descriptor + "(..) }";
            }
         }
      }

      public override Type? CSharpTypeForStorage => UnderlyingIntType?.CSharpTypeForStorage;

      public override CTypeBuiltIn? BuiltIn => UnderlyingIntType;

      public override string Rebuilt
      {
         get
         {
            var sb = new StringBuilder();

            sb.AppendLine(TypeSpecifier);
            sb.AppendLine("{");

            foreach (var cnt in Labels) { sb.AppendLine($"   {(cnt != Labels.Last() ? cnt.Rebuilt + "," : cnt.Rebuilt)}"); }

            sb.AppendLine("};");

            return sb.ToString();
         }
      }
   }
}
