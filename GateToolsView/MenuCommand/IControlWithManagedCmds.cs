namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// A control wanting to manage menu command (visibility,enable and action) shall implement this <see cref="IControlWithManagedCmds"/>
   /// </summary>
   public interface IControlWithManagedCmds
   {
      /// <summary>
      /// Control shall notify a state change in <see cref="Cmd.IsVisible"/> and <see cref="Cmd.IsEnabled"/>.
      /// </summary>
      event OnCmdStateUpdateHandler OnCmdStateUpdate;

      /// <summary>
      /// Array of managed command implementation.
      /// </summary>
      CmdManagedByControlImpl[] CmdsImpl { get; }

      /// <summary>
      /// Managed command implementation by id (shall return null if search fails)
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      CmdManagedByControlImpl? this[string id] { get; }
   }
}
