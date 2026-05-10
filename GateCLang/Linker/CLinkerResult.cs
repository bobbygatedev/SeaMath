using Gate.CLanguage.Decl;
using Gate.CLanguage.Source;
using Gate.LangBase.Expressions.Nodes;

namespace Gate.CLanguage.Linker
{
   /// <summary>
   /// 
   /// </summary>
   public class CLinkerResult
   {
      private readonly List<(CDecl? linkedSymbol, ExprNodeOperandVariable? functionImplicitId, CDecl? symbolLinkedTo)> myListLinks =
         new List<(CDecl? linkedSymbol, ExprNodeOperandVariable? functionImplicitId, CDecl? symbolLinkedTo)>();

      public CLinkerResult()
      {
            
      }

      public bool IsSuccess { get; set; } = true;

      public void AddLinkedSymbol(CDecl linkedSymbol, CDecl? symbolLinkedTo) => 
         myListLinks.Add((linkedSymbol, null, symbolLinkedTo));

      public void AddLinkedSymbol(ExprNodeOperandVariable functionImplicitId, CDecl? symbolLinkedTo) => 
         myListLinks.Add((null, functionImplicitId, symbolLinkedTo));

      public (CDecl? linkedSymbol, ExprNodeOperandVariable? functionImplicitId, CDecl? symbolLinkedTo)[] Links => myListLinks.ToArray();

      public CSource[] LinkedSources =>
         myListLinks.SelectMany(l => l.symbolLinkedTo?.ParentItemChain?.OfType<CSource>() ?? []).Distinct().ToArray();

      public CLibrary[] LinkedLibraries =>
         myListLinks.SelectMany(l => l.symbolLinkedTo?.ParentItemChain?.OfType<CLibrary>() ?? []).Distinct().ToArray();
   }
}
