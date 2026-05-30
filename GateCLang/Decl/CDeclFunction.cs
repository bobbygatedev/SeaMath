using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Linker;
using Gate.CLanguage.Statement;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.CLanguage.Decl
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclFunction : CDeclStorage, IDeclFunction
   {
      public enum KindType
      {
         ordinary = 0,
         library,
         init,
         cleanup
      }

      /// <summary>
      /// 
      /// </summary>
      public CDeclFunction(bool hasBody, KindType kind, CDeclSpecifiers? declSpecifiers)
      {
         //setting type base permanently
         declSpecifiers = declSpecifiers ?? new CDeclSpecifiers();
         declSpecifiers.AddDecl(this);
         TypeAlias.SetAsFunction(true);

         if (hasBody) { myAddSubItem(new CStatementCompound()); }
         Kind = kind;
      }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsTypedef => false;

      /// <summary>
      /// Is equal to return type
      /// </summary>
      public override CType? TypeBase => TypeAlias.TypeBase;

      /// <summary>
      /// 
      /// </summary>
      public CTypeFunctionContainer? FunctionContainer => TypeAlias.FunctionContainer;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsDefinition => Body != null;

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => throw new NotImplementedException();//todo

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor =>
         $"{FunctionContainer?.TypeAliasReturned?.TypeSpecifier} " +
         $"{Identifier}{FunctionContainer?.Descriptor}{(Body != null ? "{...}" : ";")}";

      /// <summary>
      /// If is a functiomn definition is not null
      /// </summary>
      public CStatementCompound? Body => SubItems.OfType<CStatementCompound>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public string? AlternateLinkName { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsExternalLinkRequired => Body == null && DeclSpecifiers != null && !DeclSpecifiers.IsStatic;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsInternalLinkRequired => !(Anchestor is CLibrary) && Body == null;

      public RtmDbgEngVirtCpuInstruction[] InstructionsNoFrame
      {
         get
         {
            var iss = Instructions;

            if (
               iss.FirstOrDefault() is RtmDbgEngVirtCpuInstructionPushFunctionFrame && 
               iss.LastOrDefault() is RtmDbgEngVirtCpuInstructionFramePop)
            {
               return iss.Skip(1).SkipLast(1).ToArray();
            }
            else
            {
               throw new RtmException(
                  "Function frame push and pop instructions are expected at the beginning and at the end of the instruction sequence.");
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstruction[] Instructions
      {
         get => SubItems.OfType<RtmDbgEngVirtCpuInstruction>().ToArray();
         set
         {
            var iss = value ?? [];

            myRemoveSubItemRange(Instructions);

            if (iss.FirstOrDefault() is RtmDbgEngVirtCpuInstructionPushFunctionFrame)
            {
               if (iss.LastOrDefault() is not RtmDbgEngVirtCpuInstructionFramePop)
               {
                  throw new Crash("Function frame pop instruction missing at the end of the instruction sequence.");
               }
            }
            else
            {
               //in case function frame has not been added
               iss = 
                  new[] { new RtmDbgEngVirtCpuInstructionPushFunctionFrame() }.
                  Concat(iss).
                  Append(new RtmDbgEngVirtCpuInstructionFramePop()).ToArray();
            }

            myAddSubItemRange(iss);
         }
      }

      public CAttribute[] AttributesAll => Attributes.Concat(DeclSpecifiers?.Attributes ?? []).Distinct().ToArray();

      IDecl[] IDeclFunction.Parameters => (FunctionContainer ?? []).ToArray();

      IDeclType? IDeclFunction.ReturnType => FunctionContainer?.TypeAliasReturned;

      TxtToken? IDeclFunction.BodyToken => Body?.TxtToken;

      bool IDeclFunction.HasVarArgs => FunctionContainer?.HasVarArgs ?? false;

      public KindType Kind { get; }
   }
}
