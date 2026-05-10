using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.PrePx.Directives.PragmaKinds;
using Gate.CLanguage.PrePx.Stages;
using Gate.LangBase;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.CLanguage.PrePx
{
   /// <summary>
   /// Source code after preprocessing.
   /// </summary>
   public class CPrePxSource : CItem, IWithIdentifier
   {
      private CPrePxOutput? myPrePxOutput;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="primitiveStore"></param>
      /// <param name="isHeader"></param>
      public CPrePxSource(TxtStore primitiveStore, bool isHeader)
      {
         PrimitiveStore = primitiveStore.IsPrimitive ? primitiveStore : throw new Crash("Not a primitive store!");
         IsHeader = isHeader;
      }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => ToCompileStore?.Content;

      /// <summary>
      /// 
      /// </summary>
      public CPrePxProduct[] PrePxProducts
      {
         get => SubItems.OfType<CPrePxProduct>().ToArray();
         set
         {
            myRemoveSubItemRange(PrePxProducts);
            myAddSubItemRange(value);
         }
      }

      /// <summary>
      /// List of includes which are effectively included by the source
      /// </summary>
      public CPrePxSource[] IncludeSources => SubItems.OfType<CPrePxSource>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? PrimitiveStore { get; }

      /// <summary>
      /// 
      /// </summary>
      public CPrePxOutput? PrePxOutput
      {
         get => myPrePxOutput;

         set
         {
            myPrePxOutput = value;

            if (ToCompileStore != null)
            {
               var cmp_shr_lns = ToCompileStore.Lines.Where(l => l.Content.Trim() != "").ToArray();

               ToCompileStoreShrinked = new TxtStore();
               ToCompileStoreShrinked.AddLines(cmp_shr_lns.Select(l => l.Content).ToArray());
            }
         }
      }

      /// <summary>
      /// Final store ready to compile (ie AfterStage4Expanded where preprocessor token lines have been removed). 
      /// </summary>
      public TxtStore? ToCompileStore => PrePxOutput?[PrePxOutput.Stages.Last()];

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? ToCompileStoreShrinked { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? BeforeMacroExpansion => PrePxOutput?.GetStoreOfStage<CPrePxStage33IfDefCheck>();

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? AfterLineSplicingStore => PrePxOutput?.GetStoreOfStage<CPrePxStage2LinesSplicing>();

      /// <summary>
      /// 
      /// </summary>
      public bool HasPragmaOnce => PrePxProducts.OfType<CPrePxDirectivePragma>().Any(p => p.ContentToken?.Content.Trim() == CPragmaKindOnce.NAME);

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => throw new NotImplementedException();

      /// <summary>
      /// 
      /// </summary>
      public string? Identifier => PrimitiveStore != null ? Path.GetFileNameWithoutExtension(PrimitiveStore.FileInfo?.FullName) : null;

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => Identifier.IsBlank();

      /// <summary>
      /// 
      /// </summary>
      public bool IsHeader { get; }

      /// <summary>
      /// Dictionary of map of directive after <seealso cref="CPrePxStage4DoDirectiveAction"/> stage.
      /// </summary>
      public CPrePxDirectiveMap? DirectiveMap { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CPrePxSource? ContainingSource => ParentItem as CPrePxSource;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="includeSource"></param>
      public void AddIncludeSource(CPrePxSource includeSource) => myAddSubItem(includeSource);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => PrimitiveStore?.FileInfo?.FullName??"";
   }
}
