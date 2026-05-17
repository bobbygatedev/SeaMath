using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.PrePx;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.CLanguage.Source
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CSource : CItemWithScopeSpace, IBlock, IRtmDbgEngVirtCpuPseudoSource
   {
      public const int DEFAULT_PACK = 4;
      private bool myIsDirty;
      private TxtStore? myStorePrimitive;

      /// <summary>
      /// Constructor.
      /// </summary>
      public CSource() { }

      /// <summary>
      /// 
      /// </summary>
      public DateTime DateTimeLastWrite { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public CCompilerSettings? Settings { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => string.Join("\n", SubItems.OfType<CItem>().Select(i => i.Rebuilt));

      /// <summary>
      /// 
      /// </summary>
      public string Identifier => FileInfo != null ? Path.GetFileNameWithoutExtension(FileInfo.FullName) : "NoNameCSource";

      /// <summary>
      /// 
      /// </summary>
      public FileInfo? FileInfo => StorePrimitive?.FileInfo;

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? StorePrimitive
      {
         get => myStorePrimitive;

         set
         {
            myStorePrimitive = value;

            if (myStorePrimitive?.FileInfo != null)
            {
               DateTimeLastWrite = myStorePrimitive.FileInfo.GetLastWriteTime();
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? Store { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => Identifier;

      /// <summary>
      /// 
      /// </summary>
      public string Name => FileInfo?.Name ?? "ANONIMOUS";

      /// <summary>
      /// 
      /// </summary>
      public CPrePxSource? PrePxSource
      {
         get => SubItems.OfType<CPrePxSource>().FirstOrDefault();
         set
         {
            if (value != PrePxSource)
            {
               myRemoveSubItem(PrePxSource);

               if (value != null)
               {
                  myAddSubItem(value);
                  StorePrimitive = value.PrimitiveStore;
                  Store = value.ToCompileStore;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CScopeHelper? ScopeHelper { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CDecl[] AllDecls => AllDescendant.OfType<CDecl>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CDecl[] AllGlobals => SubItems.OfType<CDeclSpecifiers>().SelectMany(ds => ds.Decls).Where(d => !d.IsAnonimous).ToArray();

      /// <summary>
      /// All globals beeing definitions (are not external).
      /// </summary>
      public CDecl[] AllGlobalDefVars => AllGlobals.Where(g => g.IsDefinition).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CDecl[] AllGlobalDeclVars => AllGlobals.Where(g => !g.IsDefinition).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CDeclFunction[] AllFunctions => AllDescendant.OfType<CDeclFunction>().ToArray();

      /// <summary>
      /// All variables persistants : 
      /// <see cref="AllGlobalDefVars"/> + <see cref="Linkages"/> + all function static variables 
      /// </summary>
      public CDeclVar[] PersistantVariables =>
         AllGlobalDefVars.
            Concat(Linkages).
            Concat(
               AllDescendant.OfType<CDeclVar>().
               Where(v => v.Visibility == ExprDeclVisibility.local_static)).
               OfType<CDeclVar>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public override CDecl[] ScopeDecls => myGetScopeDeclsDefault(Scope);

      /// <summary>
      /// 
      /// </summary>
      public ExprNodeOperator[] ImplicitCallOperators
      {
         get
         {
            var all_exs = AllDescendant.OfType<CExprStatement>().ToArray();

            var all_exs_cal_nds = all_exs.
               SelectMany(ce => ce.AllDescendant.OfType<ExprNodeOperator>().
               Where(
                  no => no.Operator?.GetType() == typeof(OperatorCall) &&
                  no.OperandNodes.Length > 0 &&
                  no.OperandNodes[0] is ExprNodeOperandVariable id &&
                  id.Decl == null)).ToArray();

            return all_exs_cal_nds;
         }
      }

      public bool IsAnonimous => Identifier.IsBlank();

      /// <summary>
      /// <br>Array of int [1,..) indicating the value of pack line-2-line</br>
      /// <br>Pack changes become effective after '#pragma pack' occurence</br>
      /// <br>so that first item is equals to base pack as in <see cref="CCompilerSettings.Pack"/> </br>
      /// <br>eg base pack = 4</br>
      /// <br>'#pragma pack(1)'</br>
      /// <br>'#pragma pack(2)'</br>
      /// <br>''</br>
      /// <br>PackMap[]={4,2,1}</br>
      /// </summary>
      public int[]? PackMap { get; set; }

      /// <summary>
      /// Base pack of source corresponds to <see cref="PackMap"/>[0] or <see cref="DEFAULT_PACK"/> if it is null.
      /// </summary>
      public int BasePack => PackMap != null && PackMap.Length > 0 ? PackMap[0] : DEFAULT_PACK;

      /// <summary>
      /// <br>Array of line idx's (1-) where pragma can place when <see cref="CCompilerSettings.ArePragmaEverywhere"/> is false.</br>
      /// <br>ie line of file scope or compounds outside of any statement body </br>
      /// <br>eg 'int\n #pragma pack(1)\na;' is not acceptable since inside a declaration </br>
      /// <br>'#pragma pack(1)\n int a;' and 'void main(int) {\n' #pragma pack(1)\n}' are acceptable.</br>
      /// </summary>
      public int[]? PragmaLineIds { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public CDeclFunction? InitDeclFunction { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CDecl?[] Linkages =>
         AllGlobalDeclVars.Select(g => g.Linkage).Nn().Distinct(new EqualityComparerByRef<CDecl>()).ToArray() ?? [];

      /// <summary>
      /// 
      /// </summary>
      public IDeclFunction[] Functions => AllGlobalDefVars.OfType<IDeclFunction>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public bool IsDirty
      {
         get => myIsDirty || FileInfo == null || !FileInfo.Exists || FileInfo.GetLastWriteTime() > DateTimeLastWrite;

         set => myIsDirty = value;
      }

      /// <summary>
      /// Root <see cref="CDewclSpecifiers"/> of source.
      /// </summary>
      public CDeclSpecifiers[] RootItems => SubItems.OfType<CDeclSpecifiers>().ToArray();

      public IDeclType[] Types => SubItems.OfType<CDeclSpecifiers>().SelectMany(ds => ds.Decls).Where(d => !d.IsAnonimous).OfType<CDeclTypedef>().ToArray();

      IDeclFunction? IRtmDbgEngVirtPseudoExeItem.CleanupDeclFunction => null;

      IDeclFunction? IRtmDbgEngVirtPseudoExeItem.InitDeclFunction => InitDeclFunction;

      IDecl[] IRtmDbgEngVirtPseudoExeItem.PersistantVariables => PersistantVariables;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="item"></param>
      /// <param name="scopeHelper"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public override bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages) =>
         item is CDeclSpecifiers dcl_spc ?
            AddDeclSpec(messages, dcl_spc, scopeHelper) :
            throw new Gate.Tools.ToolsException($"{item.GetType().Name} not allowed here!");

      public override string ToString() => $"CSource '{Name.ExtTrim()}'(Id={GlobalId})";
   }
}
