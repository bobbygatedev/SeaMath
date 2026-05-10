using Gate.Tools;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.MenuCommand
{
   public delegate void OnCmdStateUpdateHandler(IControlWithManagedCmds control);

   /// <summary>
   /// 
   /// </summary>
   public class CmdManagedByControlObserver
   {
      private readonly List<Default> myListDefault = new List<Default>();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="observableFocusedAllForms"></param>
      /// <param name="cmdContainer"></param>
      public CmdManagedByControlObserver(ControlObservableFocusedAllForms observableFocusedAllForms, CmdContainer cmdContainer)
      {
         CmdContainer = cmdContainer;
         ObservableFocusedAllForms = observableFocusedAllForms;
         ObservableFocusedAllForms.OnFocusedControlChanged += ObservableFocusedAllForms_OnFocusedControlChanged;
      }

      /// <summary>
      /// Indicates the state of a control id when focused control doesnt
      /// </summary>
      public class Default
      {
         public Default(string cmdId, bool isEnabled, bool isVisible)
         {
            CmdId = cmdId;
            IsEnabled = isEnabled;
            IsVisible = isVisible;
         }

         public string CmdId { get; }

         public bool IsEnabled { get; }

         public bool IsVisible { get; }
      }


      public void AddDefault(Default @default)
      {
         myListDefault.Add(@default);

         if (ActiveControlWithCommands != null && !ActiveControlWithCommands.CmdsImpl.Any(c => c.Id == @default.CmdId))
         {
            var cmd = CmdContainer.AllCmds[@default.CmdId];

            cmd.IsEnabled = @default.IsEnabled;
            cmd.IsVisible = @default.IsVisible;
         }
      }

      public IControlWithManagedCmds? ActiveControlWithCommands { get; private set; }

      private void ObservableFocusedAllForms_OnFocusedControlChanged(
         ControlObservableFocusedAllForms sender, Control? oldFocusedControl, Control? focusedControl)
      {
         var cut_pst_ctr =
            focusedControl as IControlWithManagedCmds ??
            focusedControl?.MthGetAnchestors().OfType<IControlWithManagedCmds>().FirstOrDefault();

         if (cut_pst_ctr != ActiveControlWithCommands)
         {
            myActionOnControlCommandChanged(cut_pst_ctr);
         }
      }

      public ControlObservableFocusedAllForms ObservableFocusedAllForms { get; }

      public CmdContainer CmdContainer { get; }

      protected virtual void myActionOnControlCommandChanged(IControlWithManagedCmds? activeControlWithCommands)
      {
         if (ActiveControlWithCommands != null)
         {
            ActiveControlWithCommands.OnCmdStateUpdate -= ActiveControlWithCommands_OnCommandControlChanged;
         }

         if (activeControlWithCommands != null)
         {
            activeControlWithCommands.OnCmdStateUpdate += ActiveControlWithCommands_OnCommandControlChanged;
         }

         ActiveControlWithCommands = activeControlWithCommands;
         myRefresh();
      }

      private void ActiveControlWithCommands_OnCommandControlChanged(IControlWithManagedCmds control) => myRefresh();

      private void myRefresh()
      {
         if (ActiveControlWithCommands != null)
         {
            foreach (var ctr_cmd in ActiveControlWithCommands.CmdsImpl)
            {
               var men_cmd = CmdContainer.AllCmds[ctr_cmd.Id.ToString()];

               if (men_cmd != null)
               {
                  men_cmd.IsEnabled = ctr_cmd.IsEnabled;
                  men_cmd.IsVisible = ctr_cmd.IsVisible;
               }
            }
         }

         //all defaults not in active control commands 
         var not_dfs = myListDefault.Where(d => 
            ActiveControlWithCommands == null || !ActiveControlWithCommands.CmdsImpl.Any(c => c.Id == d.CmdId)).ToArray();

         foreach (var def in not_dfs)
         {
            var cmd = CmdContainer.AllCmds[def.CmdId];
            
            cmd.IsEnabled = def.IsEnabled;
            cmd.IsVisible = def.IsVisible;
         }
      }

      public void DoCommandAction(string cmdId)
      {
         if (ActiveControlWithCommands?[cmdId]?.IsActive??false)
         {
            ActiveControlWithCommands?[cmdId]?.ActionImpl();
         }
      }
   }
}
