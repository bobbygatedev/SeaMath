using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools;

namespace Gate.SeaMath
{
   public delegate void OnBreakpointListChangeHandler(object? sender, RtmDbgEngBreakpoint[] breakpoints);
   public delegate void OnStartPathChangeHandler(object? sender, FileInfo? startInfo);

   /// <summary>
   /// 
   /// </summary>
   public abstract class SeaMathDocManager : HierarchicalItem
   {
      public event OnBreakpointListChangeHandler? OnBreakpointListChange;
      public event OnStartPathChangeHandler? OnStartPathChanged;

      private FileInfo? myStartInfo;
      private RtmDbgEngBreakpoint[]? myBreakpoints;

      /// <summary>
      /// 
      /// </summary>
      protected SeaMathDocManager() { }

      /// <summary>
      /// 
      /// </summary>
      public abstract FileInfo[] AllOpenFiles { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract void ToggleBreakpoint();

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngBreakpoint[] Breakpoints
      {
         get => myBreakpoints ?? new RtmDbgEngBreakpoint[0];

         protected set
         {
            myBreakpoints = value;
            OnBreakpointListChange?.Invoke(this, Breakpoints);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public FileInfo? StartInfo
      {
         get => myStartInfo;

         protected set
         {
            myStartInfo = value;
            OnStartPathChanged?.Invoke(this, myStartInfo);
         }
      }
   }
}