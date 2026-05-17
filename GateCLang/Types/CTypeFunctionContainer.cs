using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools;
using Gate.Tools.Message;
using System.Collections;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// Container for function type attributes (parameter list, return type).
   /// </summary>
   public class CTypeFunctionContainer : CItemWithScopeSpace, IEnumerable<CDeclVar>, IBlock
   {
      private bool myHasVoidParamters = true;

      /// <summary>
      /// 
      /// </summary>
      public CTypeFunctionContainer() => myAddSubItem(new CTypeAlias());

      /// <summary>
      /// Type subcript for Return Type (pointer only).
      /// </summary>
      public CTypeSubscriptSet? ReturnTypeSubscript => TypeAliasReturned?.TypeSubscriptSet;

      /// <summary>
      /// <see cref="CTypeAlias"/> for Return Type (pointer only).
      /// </summary>
      public CTypeAlias? TypeAliasReturned => SubItems.OfType<CTypeAlias>().FirstOrDefault();

      /// <summary>
      /// <see cref="CTypeAlias"/> of declaring <see cref="CDecl"/> (typdef,var,function).
      /// </summary>
      public CTypeAlias? TypeAliasDeclaration => ParentItem as CTypeAlias;

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      public int Count => this.Count();

      public override string Descriptor => IsVoid ? "(void)" : $"({string.Join(",", this.Select(p => p.Descriptor))})";

      public override string Rebuilt => Descriptor;

      /// <summary>
      /// <br> Whether parameter is (eg 'int f(void)' accept 0 parameters), </br>
      /// <br> whose meaning in C differs from 0 parameters (eg 'int f()': accept any parameters, but they are not visible to body).</br>
      /// </summary>
      public bool HasVoidParameters
      {
         get => myHasVoidParamters;

         set
         {
            if (myHasVoidParamters = value) { myRemoveSubItemRange(Parameters); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool HasVarArgs { get; set; }

      public CDeclVar[] Parameters =>
         IsVoid ? ([]) : SubItems.OfType<CDeclSpecifiers>().SelectMany(ds => ds.Decls.OfType<CDeclVar>()).ToArray();

      public CDeclVar this[int index] => IsVoid ? throw new Gate.CLanguage.CLangException($"void parameter set.") : Parameters[index];

      public override CDecl[] ScopeDecls => myGetScopeDeclsDefault(Scope);

      /// <summary>
      /// 
      /// </summary>
      public CDeclFunction? BoundFunction => ParentItemChain.OfType<CDeclFunction>().FirstOrDefault();

      /// <summary>
      /// Is of type 'void f(void)' differs from 'void f()'.
      /// </summary>
      public bool IsVoid
      {
         get
         {
            var dss = SubItems.OfType<CDeclSpecifiers>().ToArray();
            var dcs = dss.SelectMany(ds => ds.Decls).ToArray();

            return dcs.Length == 1 && dcs[0].TypeAlias.TypeSpecifier == "void";
         }
      }

      public bool IsEmpty => SubItems.OfType<CDeclSpecifiers>().Count() == 0;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="params"></param>
      public void AddParameter(CScopeHelperBase? scopeHelper = null, params CDeclVar[] @params)
      {
         scopeHelper = scopeHelper ?? CScopeHelperBase.GetDefault(Language);

         foreach (var par in @params)
         {
            if ((par.DeclSpecifiers ?? throw new Crash()).Decls.Length != 1)
            {
               throw new Gate.CLanguage.CLangException("Decl specifier for function parameter must have just one child CDeclVar!");
            }
            else if (IsVoid) { throw new Gate.CLanguage.CLangException($"Can't add parameter to 'void' function!"); }
            else
            {
               var oth = this.Where(p => !p.IsAnonimous).FirstOrDefault(p => p.Identifier == par.Identifier);

               if (oth != null)
               {
                  var mgs = new MsgCollection();

                  mgs.Add(CCompilerMsgs.RedefindedIdentifier(par, oth));
                  throw new Gate.Tools.ToolsException(mgs);
               }
               else { myAddSubItem(par.DeclSpecifiers); }
            }
         }
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="param"></param>
      /// <returns></returns>
      public bool RemoveParameter(CDeclVar param) => myRemoveSubItem(param.DeclSpecifiers);

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
              AddDeclSpec(messages, dcl_spc, scopeHelper) : throw new Gate.Tools.ToolsException($"{item.GetType().Name} not allowed here!");

      IEnumerator IEnumerable.GetEnumerator() => Parameters.GetEnumerator();

      public IEnumerator<CDeclVar> GetEnumerator() => Parameters.ToList().GetEnumerator();
   }
}
