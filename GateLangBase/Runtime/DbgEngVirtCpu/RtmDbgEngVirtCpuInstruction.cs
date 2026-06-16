using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class RtmDbgEngVirtCpuInstruction : HierarchicalItem, IRtmDbgEngPoint
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      protected RtmDbgEngVirtCpuInstruction(TxtToken? token, object? tag = null)
      {
         Token = token;
         Tag = tag;
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract string Name { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      public abstract void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy);

      /// <summary>
      /// 
      /// </summary>
      public IDeclFunction? DeclFunction => ParentItem as IDeclFunction;

      /// <summary>
      /// 
      /// </summary>
      public TxtToken? Token { get; set; }

      public int? Idx
      {
         get
         {
            var iss = DeclFunction?.Instructions;

            return iss != null ? iss.ToList().IndexOf(this) : null;
         }
      }

      public object? Tag { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"#{Idx}:{Name}({DeclFunction?.Identifier}): {Token} (Tag={Tag})";
   }
}
