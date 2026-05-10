using Gate.CLanguage.Decl;
using Gate.CLanguage.Initialisation;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// Space of source code containing statements (ie Declarations,Expression, Other statements (if,while,goto,..)
   /// </summary>
   public class CCompound : CStatement
   {
      public CCompound() => myAddSubItem(new CBlockCompound());

      public CBlockCompound Block => SubItems.OfType<CBlockCompound>().First();

      public override string Rebuilt => Block.Rebuilt;

      public override string Descriptor
      {
         get
         {
            var sub_items = Block.SubItems.OfType<CItem>().ToArray();

            if (SubItems.Count() == 0) { return "{}"; }
            else if (SubItems.Count() == 1) { return "{" + sub_items.First().Descriptor + "}"; }
            else { return "{" + sub_items.First().Descriptor + "(..)}"; }
         }
      }

      public CItem[] ExecutableLines => AllDescendant.Where(
         d => d is CDeclVar || (d is CStatement && !(d.ParentItem is CInitialisation))).OfType<CItem>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CDeclFunction? ParentFunction => ParentItem as CDeclFunction;

      /// <summary>
      /// 
      /// </summary>
      public CCycle? ParentCycle => ParentItem as CCycle;

      /// <summary>
      /// 
      /// </summary>
      public CCycleFor? ParentCycleFor => ParentItem as CCycleFor;
   }
}
