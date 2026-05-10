using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.PrePx.Directives.Macro
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CPrePxDirectiveMacro : CPrePxDirective, IWithIdentifierSettable, IPrePxDirectiveDefUndef, IDecl
   {
      private static Regex myRegexForShrink = new Regex(@"\s{2,}", RegexOptions.Compiled);
      private string[]? myArguments = null;

      /// <summary>
      /// 
      /// </summary>
      public const string VA_ARGS = "__VA_ARGS__";

      /// <summary>
      /// 
      /// </summary>
      public const string DIRECTIVE_NAME = "define";

      /// <summary>
      /// 
      /// </summary>
      public CPrePxDirectiveMacro() { }

      /// <summary>
      /// 
      /// </summary>
      public override string DirectiveName => DIRECTIVE_NAME;

      /// <summary>
      /// 
      /// </summary>
      public bool HasArguments
      {
         get => myArguments != null;
         set
         {
            if (value)
            {
               if (myArguments == null) { myArguments = []; }
            }
            else { myArguments = null; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string[]? Args
      {
         get => myArguments?.ToArray();
         set => myArguments = value != null ? myCheckParams(value) : null;
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsVariadic => Args != null && Args.Length > 0 && Args.Last() == "...";

      public ContentMapType? ContentMap { get; set; }

      public override string Descriptor
      {
         get
         {
            var sb = new StringBuilder("#define ");

            if (DirectiveName != null && DirectiveName.Trim() != "") { sb.Append(Identifier); }

            if (HasArguments) { sb.Append($" ({string.Join(",", myArguments ?? throw new Crash())})"); }

            if (ContentToken != null)
            {
               var shr_prs = myShrink(ContentToken.Content);

               if (shr_prs != "") { sb.Append($" {shr_prs}"); }
            }

            return sb.ToString();
         }
      }

      public override string ToString() =>
         $"#define {Identifier}{(HasArguments ? $"({string.Join(",", Args ?? [])})" : "")} {(ContentToken != null ? ContentToken.Content : "")}";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="store"></param>
      /// <param name="currLineIdx"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public static CPrePxDirectiveMacro[] GetMacroSet(TxtStore store, int currLineIdx)
      {
         var def_und_arr = store.OwnedSectors.
            Where(s => (s.From?.Line ?? int.MaxValue) < currLineIdx).
            Select(s => s.Tag).
            OfType<IPrePxDirectiveDefUndef>().ToArray();

         return myDefUndefMakeDictionary(def_und_arr);
      }

      public static CPrePxDirectiveMacro[] GetMacroSet(CPrePxDirectiveMap? directiveMap, int currLineIdx)
      {
         var def_und_arr = directiveMap?.
            Where(kp => kp.Key < currLineIdx).
            Select(kp => kp.Value).
            OfType<IPrePxDirectiveDefUndef>().ToArray() ?? [];

         return myDefUndefMakeDictionary(def_und_arr);
      }

      private static CPrePxDirectiveMacro[] myDefUndefMakeDictionary(IPrePxDirectiveDefUndef[] defUndefArray)
      {
         var dct = new Dictionary<string, CPrePxDirectiveMacro>();

         foreach (var def_und in defUndefArray)
         {
            if (def_und is CPrePxDirectiveMacro mcr) { dct[mcr.Identifier ?? ""] = mcr; }
            else if (def_und is CPrePxDirectiveUndef und) { dct.Remove(und.Identifier ?? throw new Crash()); }
            else { throw new Crash(); }
         }

         return dct.Values.ToArray();
      }

      string? IPrePxDirectiveDefUndef.Id { get => Identifier; }

      bool IPrePxDirectiveDefUndef.IsDefine => true;

      TxtToken? IDecl.TxtToken { get => TxtToken; }

      bool IDecl.IsFunction => false;

      bool IDecl.IsConstant => false;

      IDeclType? IDecl.DeclType => null;

      string? IWithIdentifier.Identifier => Identifier;

      public ExprDeclVisibility Visibility => ExprDeclVisibility.global_extern;

      public IDecl? Linkage => null;

      public IRtmDbgEngVirtPseudoExeItem? ExeItem => HeaderSource;

      private string myShrink(string @string) => myRegexForShrink.Replace(@string, " ").Trim();

      private string[] myCheckParams(string[] @params)
      {
         var rgx_for_var_nam = TxtMarker.RegexForVarName;
         var prs = @params.Select(p => p == null ? "" : p.Trim()).ToArray();

         foreach (var par in prs)
         {
            var is_var_name = rgx_for_var_nam.Match(par).Length == par.Length;

            if (par.Trim() == "") { throw new Gate.Tools.ToolsException("Macro param shall not be null or empty"); }
            else if (par == "..." && par != prs.Last())
            {
               throw new Gate.Tools.ToolsException("Optional params (...) shall be last parameter!");
            }
            else if (!is_var_name && par != "...")
            {
               throw new Gate.Tools.ToolsException($"{par.Trim()} is not a valid macro param name!");
            }
         }

         return @params.ToArray();
      }
   }
}
