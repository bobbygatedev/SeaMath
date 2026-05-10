using Gate.CLanguage.Decl;
using Gate.CLanguage.Source;
using Gate.LangBase.Expressions.Nodes;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.CLanguage.Linker
{
   /// <summary>
   /// 
   /// </summary>
   public class CLinker
   {
      private readonly List<CSource> myListSource = new List<CSource>();

      public CLinker(CLinkerSettings linkerSettings) => Settings = linkerSettings;

      /// <summary>
      /// Array of libraries to link.
      /// </summary>
      public CLibrary[]? Libraries { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public CLinkerSettings Settings { get; }

      public CLinkerResult Link(CSource[] sources, MsgCollection messages)
      {
         var res = new CLinkerResult();

         myListSource.Clear();

         foreach (var src in sources)
         {
            myListSource.Add(src);

            if (!LinkSingleSource(src, messages, res))
            {
               messages.Add(new Msg(MsgType.fail, "Link Failed!"));
               res.IsSuccess = false;

               return res;
            }
         }

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="source"></param>
      /// <param name="messages"></param>
      /// <param name="result"></param>
      /// <returns></returns>
      public virtual bool LinkSingleSource(CSource source, MsgCollection messages, CLinkerResult result)
      {
         var lng = source.Language;
         var oth_srs = myListSource.ToArray();
         var vrs_2_lnk =
            source.AllGlobalDeclVars.
               Where(g => g.IsExternalLinkRequired || g.IsInternalLinkRequired).ToArray();

         //all definitions where not 
         foreach (var dcl in vrs_2_lnk)
         {
            if (!myTryToLink(dcl, oth_srs, messages, out var dcl_lnk, lng)) { return false; }
            else
            {
               dcl.Linkage = dcl_lnk;
               result.AddLinkedSymbol(dcl, dcl_lnk);
            }
         }

         if (source.Language == CLanguage.c)
         {
            //C only
            var imp_cal_ops = source.ImplicitCallOperators;

            foreach (var op in imp_cal_ops)
            {
               if (!myTryToLinkImplicitCall(op, oth_srs, messages, out var dcl_lnk)) { return false; }
               else
               {
                  var fnc_id = (ExprNodeOperandVariable)op.OperandNodes[0];

                  fnc_id.Decl = dcl_lnk;
                  result.AddLinkedSymbol(fnc_id, dcl_lnk);
               }
            }
         }

         myListSource.Add(source);

         return true;
      }

      protected virtual bool myTryToLinkImplicitCall(
         ExprNodeOperator callOperatorNode, CSource[] sourceOtherThanDecl, MsgCollection messages, out CDecl? linkedDeclaration)
      {
         var id_nod = (ExprNodeOperandVariable)callOperatorNode.OperandNodes[0];
         var cal_nds = ((SubExpr)callOperatorNode.OperandNodes[1]).FunctionArgumentsNodes;
         var src_fns = sourceOtherThanDecl.SelectMany(s => s.AllDescendant.OfType<CDeclFunction>()).ToArray();
         var lib_fns = (Libraries ?? []).SelectMany(l => l.Decls).OfType<CDeclFunction>().ToArray();
         var all_fns = src_fns.Concat(lib_fns).ToArray();

         var ass_fns = all_fns.Where(f => f.Identifier == id_nod.Identifier && myIsPaired(f, cal_nds)).ToArray();

         if (ass_fns.Length > 0)
         {
            if (ass_fns.Length == 1)
            {
               linkedDeclaration = ass_fns[0];

               return true;
            }
            else
            {
               var n_scr_dcr = ass_fns.Count(f => src_fns.Contains(f));

               if (n_scr_dcr == 1 || Settings.LinkerPolicy == CLinkerPolicy.dynamic)
               {
                  linkedDeclaration = ass_fns[0];

                  return true;
               }
               else
               {
                  linkedDeclaration = null;
                  messages.Add(new Msg(MsgType.fail, $"Implicit function {id_nod.Identifier} is ambigous!"));

                  return false;
               }
            }
         }
         else if (Settings.LinkerPolicy == CLinkerPolicy.dynamic)
         {
            //if link policy is dynamic link is not made and errors is raised during function call
            linkedDeclaration = null;

            return true;
         }
         else
         {
            linkedDeclaration = null;
            messages.Add(new Msg(MsgType.fail, $"Not a valid function linked for {callOperatorNode}"));

            return false;
         }
      }

      protected bool myTryToLink(
         CDecl declarationToLink, CSource[] sourceOtherThanDecl, MsgCollection messages, out CDecl? linkedDeclaration, CLanguage language)
      {
         if (declarationToLink is CDeclFunction dcl_fnc)
         {
            return myLinkFunction(dcl_fnc, sourceOtherThanDecl, messages, out linkedDeclaration, language);
         }
         else if (declarationToLink is CDeclVar dcl_var)
         {
            return myLinkVar(dcl_var, sourceOtherThanDecl, messages, out linkedDeclaration, language);
         }
         else
         {
            throw new Crash();
         }
      }

      private bool myLinkFunction(
         CDeclFunction funcToLink, CSource[] sourceOtherThanDecl, MsgCollection messages, out CDecl? linkedDeclaration, CLanguage language)
      {
         var is_str = Settings.LinkerPolicy == CLinkerPolicy.strict;

         if (language == CLanguage.c)
         {
            var lnk_fns = sourceOtherThanDecl.
               SelectMany(s => s.AllGlobalDefVars.OfType<CDeclFunction>()).
               Where(gv => funcToLink.Identifier == gv.Identifier).ToArray();

            if (lnk_fns.Length == 0)
            {
               lnk_fns = (Libraries ?? []).
                  SelectMany(l => l.Decls).OfType<CDeclFunction>().
                  Where(gv => funcToLink.Identifier == gv.Identifier).ToArray();

               //in case search is made in libraries first match is taken
               lnk_fns = lnk_fns.Length > 0 ? lnk_fns.Take(1).ToArray() : lnk_fns;
            }

            return myCheckAssociation(funcToLink, messages, out linkedDeclaration, lnk_fns);
         }
         else
         {
            throw new NotImplementedException("Cpp not implemented yet!");
         }
      }

      private bool myLinkVar(
         CDeclVar varToLink, CSource[] allSources, MsgCollection messages, out CDecl? linkedDeclaration, CLanguage language)
      {
         var is_str = Settings.LinkerPolicy == CLinkerPolicy.strict;
         var lnk_dcs = allSources.
            SelectMany(s => s.AllGlobalDefVars.OfType<CDeclVar>().Concat(s.Linkages)).
            Where(gv => gv?.Identifier == varToLink.Identifier).ToArray();

         ///if mode is strict type shall be compatible <see cref="CTypeAlias.IsSimilar(CTypeAlias)"/>
         if (is_str) { lnk_dcs = [.. lnk_dcs.Nn().Where(l => l.TypeAlias.IsSimilar(varToLink.TypeAlias))]; }

         //if link on source scripts fails search is repeated on libraries
         if (lnk_dcs.Length == 0)
         {
            lnk_dcs = Libraries?.
               SelectMany(l => l.Decls).OfType<CDeclVar>().
               Where(gv => gv.Identifier == varToLink.Identifier).ToArray();

            if (is_str) { lnk_dcs = [.. (lnk_dcs ?? []).Nn().Where(l => l.TypeAlias.IsSimilar(varToLink.TypeAlias))]; }
         }

         return myCheckAssociation(varToLink, messages, out linkedDeclaration, lnk_dcs);
      }

      private bool myCheckAssociation(
         CDecl declToLink, MsgCollection messages, out CDecl? linkedDeclaration, CDecl?[]? linkedDecls)
      {
         var is_str = Settings.LinkerPolicy == CLinkerPolicy.strict;

         switch (linkedDecls?.Length ?? 0)
         {
            case 0:
               linkedDeclaration = null;

               if (declToLink.IsInternalLinkRequired)
               {
                  //internal linkage 
                  linkedDeclaration = declToLink;

                  return true;
               }
               else if (declToLink.IsExternalLinkRequired)
               {
                  if (declToLink.UsingOperands.Length != 0)
                  {
                     messages.Add(CLinkerMessages.M001_IdentifierNotFound(
                        declToLink.TxtToken, declToLink?.Identifier ?? "", is_str ? MsgType.fail : MsgType.warning));

                     return !is_str;
                  }
                  else { return true; }//can't link but var is never used
               }
               else
               {
                  throw new Crash();
               }

            case 1:
               linkedDeclaration = linkedDecls?.First();

               //var is no extern and trying to link to var from another source
               //this is valid for visual studio and for not init globals such as 'int a;' 
               var is_no_ext = !declToLink.IsExternalLinkRequired && !ReferenceEquals(linkedDeclaration?.HeaderSource, declToLink.HeaderSource);
               if (is_no_ext && Settings.IsExternCompulsoryForVars)
               {
                  //in this case i try to link a simple declarat

                  messages.Add(CLinkerMessages.M002_MoreThanOneIdentifier(declToLink.TxtToken, declToLink.Identifier.ExtTrim()));
                  linkedDeclaration = null;

                  return false;
               }

               return true;

            default:
               if (is_str)
               {
                  messages.Add(CLinkerMessages.M002_MoreThanOneIdentifier(declToLink.TxtToken, declToLink.Identifier.ExtTrim()));
                  linkedDeclaration = null;

                  return false;
               }
               else
               {
                  linkedDeclaration = linkedDecls?.First();

                  return true;
               }
         }
      }

      private bool myIsPaired(CDeclFunction declFunction, ExprNode[] callNodes)
      {
         var has_va = declFunction.FunctionContainer?.HasVarArgs ?? false;
         var fnc_prs = declFunction.FunctionContainer?.Parameters ?? [];

         return 
            (!has_va || callNodes.Length >= fnc_prs.Length) &&
            (has_va || callNodes.Length == fnc_prs.Length);
      }
   }
}
