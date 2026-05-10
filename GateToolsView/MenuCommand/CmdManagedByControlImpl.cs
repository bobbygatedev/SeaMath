namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// <br> Implementation for a command implemented inside a control responsible for </br>
   /// <br> - <see cref="Cmd.IsEnabled"/> and <see cref="Cmd.IsVisible"/> state </br>
   /// <br> - Notify any change of state  </br>
   /// <br> - Given the command action implementation <see cref="CmdManagedByControlImpl.ActionImpl"/> </br>
   /// </summary>
   public abstract class CmdManagedByControlImpl
   {
      public CmdManagedByControlImpl(IControlWithManagedCmds parent, string id)
      {
         Id = id;
         Parent = parent;
      }

      /// <summary>
      /// Implementation for menu cmd.
      /// </summary>
      public abstract void ActionImpl();

      public string Id { get; }

      public IControlWithManagedCmds Parent { get; }

      public bool IsActive => IsEnabled && IsVisible;

      public abstract bool IsEnabled { get; }

      public abstract bool IsVisible { get; }
   }
}
