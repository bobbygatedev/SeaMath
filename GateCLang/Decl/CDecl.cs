using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Linker;
using Gate.CLanguage.Source;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Decl
{
   /// <summary>
   /// Represents a var/function declaration (eg int a; int f(int);).
   /// </summary>
   public abstract class CDecl : CItem, IMayBeDefinition, IDecl, IWithLinkSignature, IWithIdentifierSettable
   {
      /// <summary>
      /// 
      /// </summary>
      public CDecl() => myAddSubItem(TypeAlias = new CTypeAlias());

      /// <summary>
      /// 
      /// </summary>
      public abstract CType? TypeBase { get; }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// 
      /// </summary>
      public ExprDeclVisibility Visibility
      {
         get
         {
            var has_sta = StorageClass == CTypeStorageClass.@static;

            if (IsGlobal)
            {
               return has_sta ? ExprDeclVisibility.global_static : ExprDeclVisibility.global_extern;
            }
            else if (has_sta) { return ExprDeclVisibility.local_static; }
            else { return ExprDeclVisibility.local; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CTypeAlias TypeAlias { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsTypedef { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsDefinition { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsExternalLinkRequired { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsInternalLinkRequired { get; }

      /// <summary>
      /// <see cref="ExprNodeOperand"/> using me.
      /// </summary>
      public ExprNodeOperand[] UsingOperands => HeaderSource != null ?
         HeaderSource.AllDescendant.OfType<ExprNodeOperandVariable>().Where(v => v.Decl == this).ToArray() :
         new ExprNodeOperand[0];

      /// <summary>
      /// <br> A <see cref="CDecl"/> not being a <see cref="IsDefinition"/> shall be assigned a linkage by <see cref="CLinker"/> </br>
      /// <br> Linkage can be internal or external according to <see cref="IsInternalLinkRequired"/>/<see cref="IsExternalLinkRequired"/></br>
      /// </summary>
      public CDecl? Linkage { get; set; }

      /// <summary>
      /// Signature (string descritor whose satisfactory describes declaration)
      /// </summary>
      public string? Signature => GetSignature(CScopeHelperBase.GetDefault(Language));

      /// <summary>
      /// 
      /// </summary>
      public string? Identifier { get; set; } = null;

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => Identifier.IsBlank();

      /// <summary>
      /// 
      /// </summary>
      public bool IsConstant => DeclSpecifiers != null && DeclSpecifiers.IsConstant;

      /// <summary>
      /// 
      /// </summary>
      public CTypeStorageClass StorageClass
      {
         get => DeclSpecifiers != null ? DeclSpecifiers.StorageClass : 0;
         set => (DeclSpecifiers ?? throw new Crash()).StorageClass = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public CDeclSpecifiers? DeclSpecifiers => ParentItem as CDeclSpecifiers;

      /// <summary>
      /// True if this is either a declaration or a definition body.
      /// </summary>
      public bool IsFunction => TypeAlias != null && TypeAlias.IsFunction;

      /// <summary>
      /// True if this is a function definition (eg  'int a(int){ return 0}').
      /// </summary>
      public bool IsFunctionDef => IsFunction && ((CDeclFunction)this).Body != null;

      /// <summary>
      /// True if this is a function declaration (eg  'int a(int);').
      /// </summary>
      public bool IsFunctionDecl => IsFunction && ((CDeclFunction)this).Body == null;

      /// <summary>
      /// 
      /// </summary>
      public bool IsGlobal => DeclSpecifiers?.ParentItem is CSource || DeclSpecifiers?.ParentItem is CLibrary;

      /// <summary>
      /// 
      /// </summary>
      public bool IsScalar => TypeAlias != null && TypeAlias.IsScalar;

      /// <summary>
      /// 
      /// </summary>
      IDeclType? IDecl.DeclType => TypeAlias?.PrimitiveAlias;

      /// <summary>
      /// 
      /// </summary>
      IDecl? IDecl.Linkage => Linkage;

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngVirtPseudoExeItem? IDecl.ExeItem => HeaderSource;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="scopeHelper"></param>
      /// <returns></returns>
      public string? GetSignature(CScopeHelperBase? scopeHelper) => scopeHelper?.GetDeclSignature(this);
   }
}
