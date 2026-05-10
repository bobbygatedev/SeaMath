using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Runtime.Object
{
   /// <summary>
   /// Object in runtime, can be variable, constant, reference, function, module.
   /// </summary>
   public abstract class RtmObj : HierarchicalItemWithFinalizer, IRtmDbgEngVirtCpuStackItem
   {
      private readonly IDeclType? myDeclType;

      private static long myGlobalCounter = 0;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declType"></param>
      protected RtmObj(IDeclType? declType)
      {
         GlobalId = ++myGlobalCounter;
         myDeclType = declType;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="decl"></param>
      public RtmObj(IDecl? decl)
      {
         GlobalId = ++myGlobalCounter;
         Decl = decl;
      }

      /// <summary>
      /// 
      /// </summary>
      public long GlobalId { get; }

      /// <summary>
      /// Address of object (when implemented) 
      /// </summary>
      public abstract IntPtr? Address { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract RtmFormat? CurrentFormat { get; }

      /// <summary>
      /// <br> <see cref="Decl"/>.Identifier </br>
      /// <br> Can be overriden by setting <see cref="VarNameOverriden"/> </br>
      /// </summary>
      public string? VarName => VarNameOverriden ?? Decl?.Identifier;

      /// <summary>
      /// Override value for <see cref="VarName"/> 
      /// </summary>
      public string? VarNameOverriden { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsConstant { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public IDecl? Decl { get; }

      /// <summary>
      /// 
      /// </summary>
      public IDeclType? DeclType => Decl != null ? Decl.DeclType : myDeclType;

      /// <summary>
      /// Effective object used eg for function call.
      /// </summary>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Setting value not implemented or irregular.</exception>
      public abstract ValueType? CSharpObj { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="runTimeFormat"></param>
      /// <returns></returns>
      public virtual string? GetDisplayValue(RtmFormat? runTimeFormat = null) => runTimeFormat?.Format(this);

      /// <summary>
      /// Descriptor of content + type of content(variable,const,ref).
      /// </summary>
      public virtual string? DisplayValue => CurrentFormat?.Format(this);

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuRtmModule? Module => ParentItem as RtmDbgEngVirtCpuRtmModule;

      /// <summary>
      /// Make <paramref name="objects"/> unique using <see cref="RtmObj.GlobalId"/> more recent (greater id) 
      /// are choosen among <paramref name="objects"/> not blank.
      /// </summary>
      /// <param name="objects"></param>
      /// <returns></returns>
      public static RtmObj[] GetUniqueById(IEnumerable<RtmObj> objects) => 
         objects.Where(o => !o.VarName.IsBlank()).OrderBy(o => o.GlobalId).GroupBy(o => o.VarName).Select(g => g.Last()).ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      protected virtual void myExtraInit(IRtmDbgEngStackRO stack) { }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"{VarName} = {DisplayValue}{(Address.HasValue ? $"(ptr=0x{Address.Value.ToInt64():x})" : "")} GlobalId={GlobalId}";
   }
}

