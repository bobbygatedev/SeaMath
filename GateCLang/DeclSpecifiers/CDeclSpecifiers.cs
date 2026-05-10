using Gate.CLanguage.Decl;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using System.Text;

namespace Gate.CLanguage.DeclSpecifiers
{
   /// <summary>
   /// Represents a declaration specifiers + plus ints declators (eg 'int a,2;'), which can contain one ore more <see cref="CDecl"/>. 
   /// </summary>
   public class CDeclSpecifiers : CItem
   {
      /// <summary>
      /// 
      /// </summary>
      public CDeclSpecifiers() { }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => true;

      /// <summary>
      /// Gets the base type associated with this instance without transferring ownership.
      /// </summary>
      public CType? TypeBaseNotOwned { get; private set; }

      /// <summary>
      /// Gets the first owned subitem of type <see cref="CType"/>, or <see langword="null"/> if no such item exists.
      /// </summary>
      public CType? TypeBaseOwned => SubItems.OfType<CType>().FirstOrDefault();

      /// <summary>
      /// Type bound decl specifier eg 'int a,b;'=> int 
      /// </summary>
      public CType? TypeBase
      {
         get => TypeBaseOwned ?? TypeBaseNotOwned ?? DefaultInt;

         set
         {
            //make empty
            TypeBaseNotOwned = null;
            myRemoveSubItem(TypeBase);

            //built/typedef in not in hierarchy class/
            if (value != null && value.IsBuiltIn)
            {
               TypeBaseNotOwned = value;
            }
            else if (value is CTypeAlias ali)
            {
               TypeBaseNotOwned = ali.IsTypedef ?
                  ali :
                  throw new Gate.CLanguage.CLangException($"Type alias {value.Descriptor} not associated to typedef!");
            }
            else if (value is CTypeDecl td)
            {
               if (td.ParentItem == null)
               {
                  myAddSubItem(value);
               }
               else
               {
                  TypeBaseNotOwned = td;
               }
            }
            else if (value != null) { throw new Gate.Tools.ToolsException($"Not a valid type {value.Descriptor} "); }
         }
      }

      /// <summary>
      /// Default int type (set by application)
      /// </summary>
      public CTypeBuiltIn? DefaultInt { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsConstant => (TypeQualifiers & CTypeQualifiersFlags.@const) != 0;

      /// <summary>
      /// 
      /// </summary>
      public bool IsStatic => (StorageClass & CTypeStorageClass.@static) != 0;

      /// <summary>
      /// Whether it not contains any not anonimous declaration (eg 'struct {int f1;};').
      /// </summary>
      public bool IsAnonimous => Decls.Length == 0 || Decls.All(d => d.IsAnonimous);

      /// <summary>
      /// Empty declarator (not type nor declarations associated, nor storage class, nor qualifiers, not function specifiers)
      /// </summary>
      public bool IsEmpty =>
         Decls.Length == 0 &&
         TypeBase == null &&
         StorageClass == CTypeStorageClass.none &&
         TypeQualifiers == CTypeQualifiersFlags.none &&
         FunctionSpecifier == CFunctionSpecifier.none;

      /// <summary>
      /// 
      /// </summary>
      public CTypeStorageClass StorageClass { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CTypeQualifiersFlags TypeQualifiers { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CFunctionSpecifier FunctionSpecifier { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CDecl[] Decls => SubItems.OfType<CDecl>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public bool IsFunctionDef => Decls.Length == 1 && Decls[0].IsFunctionDef;

      /// <summary>
      /// 
      /// </summary>
      public string Signature
      {
         get
         {
            var sgn = "";

            if (IsStatic) { sgn += "static "; }
            if (IsConstant) { sgn += "const "; }

            return sgn.Trim();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt
      {
         get
         {
            if (Decls.Length == 0) { return CDeclDescriptorHelper.GetDescriptor(this, null, null, null, null, null); }//type only
            else if (IsFunctionDef)
            {
               var fnc = (CDeclFunction)Decls[0];
               var sb = new StringBuilder();

               sb.AppendLine(CDeclDescriptorHelper.GetDescriptor(
                  this, fnc.TypeAlias.TypeSubscriptSet, fnc.Identifier, null, fnc.FunctionContainer?.Parameters.ToArray(), null));
               sb.AppendLine(fnc?.Body?.Rebuilt);

               return sb.ToString();
            }
            else
            {
               var ini_dcl_lst = string.Join(",", Decls.Select(d =>
                  CDeclDescriptorHelper.GetDescriptor(
                     this,
                     d.TypeAlias.TypeSubscriptSet,
                     d.Identifier, d is CDeclVar dva ? dva.OwnedInit : null,
                     d is CDeclFunction dfn ? dfn.FunctionContainer?.Parameters.ToArray() : null,
                     null)));

               return ini_dcl_lst + ";";
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor
      {
         get
         {
            var dsc = CDeclDescriptorHelper.GetDescriptor(this, null, null, null, null, null);
            var dcs = Decls.Select(d =>
               CDeclDescriptorHelper.GetDescriptor(
                  null,
                  d.TypeAlias.TypeSubscriptSet,
                  d.Identifier,
                  d is CDeclVar dv ? dv.OwnedInit : null,
                  d.IsFunction ? ((CDeclFunction)d).FunctionContainer?.Parameters.ToArray() : null,
                  null));

            return $"DeclSpec: {dsc} {string.Join(",", dcs.Select(d => d.Trim()))};";
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="decl"></param>
      /// <returns></returns>
      public bool RemoveDecl(CDecl decl) => myRemoveSubItem(decl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="decl"></param>
      /// <param name="messages"></param>
      /// <param name="scopeHelper"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public bool AddDecl(CDecl decl, MsgCollection? messages = null, CScopeHelperBase? scopeHelper = null)
      {
         var is_exc = messages == null;

         messages = messages ?? new MsgCollection();
         scopeHelper = scopeHelper ?? ContainingScope?.Helper ?? HeaderSource?.ScopeHelper;

         if (decl.DeclSpecifiers == this) { return true; }//nothing to to
         else if (decl.DeclSpecifiers != null) { throw new Gate.Tools.ToolsException($"Already bound to other {GetType().Name}"); }
         else
         {
            myAddSubItem(decl);

            if (
               scopeHelper == null ||
               decl.Identifier.IsBlank() ||
               scopeHelper.CheckDecl(messages, decl, ContainingScope?.ItemWithScopeSpace ?? throw new Crash()))
            {
               return true;
            }
            else
            {
               myRemoveSubItem(decl);
               return is_exc ? throw new Gate.Tools.ToolsException(messages) : false;
            }
         }
      }
   }
}
