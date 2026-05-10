using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.Source;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.CLanguage
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CItem : HierarchicalItem
   {
      private CLanguage myInternalLanguage = CLanguage.c;

      private static long myObjCounter = 0;

      /// <summary>
      /// 
      /// </summary>
      public CItem()
      {
         GlobalId = ++myObjCounter;
         myAddSubItem(Attributes = new Collection<CAttribute>());
      }

      /// <summary>
      /// Unique object counter/id
      /// </summary>
      public long GlobalId { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string? Descriptor { get; }

      /// <summary>
      /// Rebuilt content of item as were in source code (eg 'int a = 3;')
      /// </summary>
      public abstract string? Rebuilt { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool HasAssociatedPragma { get; }

      /// <summary>
      /// 
      /// </summary>
      public CLanguage Language
      {
         get => this is CSource || HeaderSource == null ? myInternalLanguage : HeaderSource.Language;

         set => myInternalLanguage = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public virtual CSource? HeaderSource => ParentItemChain.FirstOrDefault(i => i is CSource) as CSource;

      /// <summary>
      /// 
      /// </summary>
      public Collection<CAttribute> Attributes { get; }
      /// <summary>
      /// Source code fragment as in preprocessed file.
      /// </summary>
      public virtual TxtToken? TxtToken { get; set; }

      /// <summary> 
      /// <br>:</br>
      /// <br> - me if I am a scope.</br>
      /// <br> - closest scopeItem in ParentItemChain which is a scope (ie a compound, a class/struct/union, or null).</br>
      /// </summary>
      public CScope? ContainingScope => ParentItemChain.OfType<CItemWithScopeSpace>().FirstOrDefault()?.Scope;

      /// <summary>
      /// 
      /// </summary>
      public new CItem? ParentItem => base.ParentItem as CItem;

      /// <summary>
      /// 
      /// </summary>
      public CPrePxDirectivePragma? AssociatedPragma
      {
         get
         {
            if (HasAssociatedPragma && HeaderSource != null && TxtToken != null)
            {
               var ppx_srx = HeaderSource.PrePxSource;
               var ln_idx = TxtToken.From?.Line ?? -1;
               var drs = ppx_srx?.DirectiveMap?.Where(kp => kp.Value is CPrePxDirectivePragma && kp.Key < ln_idx).ToArray() ?? [];

               if (drs.Length > 0)
               {
                  var cnd_kp = drs.Last();

                  for (var li = cnd_kp.Key + 1; li < ln_idx; li++)
                  {
                     if (HeaderSource.Store != null && !HeaderSource.Store[ln_idx].Content.IsBlank()) { return null; }
                  }

                  return cnd_kp.Value as CPrePxDirectivePragma;
               }
            }

            return null;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"{Descriptor}(Id={GlobalId})";
   }
}
