namespace Gate.ToolsView.MenuCommand
{
   public class CmdManagedByTextBox : TextBox, IControlWithManagedCmds
   {
      private OnCmdStateUpdateHandler? myOnCmdStateUpdate;
      private CmdManagedByControlImpl[] myCommands;

      public CmdManagedByTextBox() => myCommands = [
            new InnerImpl.Copy(this),
            new InnerImpl.Cut(this),
            new InnerImpl.Paste(this),
         ];

      private static class InnerImpl
      {
         public class Copy : CmdManagedByControlImpl
         {
            public Copy(CmdManagedByTextBox parent) : base(parent, CmdCommonlyUsedIds.COPY) { }

            public override bool IsEnabled => true;

            public override bool IsVisible => true;

            public override void ActionImpl() => ((TextBox)Parent).Copy();
         }

         public class Cut : CmdManagedByControlImpl
         {
            public Cut(CmdManagedByTextBox parent) : base(parent, CmdCommonlyUsedIds.CUT) { }

            public override bool IsEnabled => !((TextBox)Parent).ReadOnly;

            public override bool IsVisible => true;

            public override void ActionImpl() => ((TextBox)Parent).Cut();
         }

         public class Paste : CmdManagedByControlImpl
         {
            public Paste(CmdManagedByTextBox parent) : base(parent, CmdCommonlyUsedIds.PASTE) { }

            public override bool IsEnabled => !((TextBox)Parent).ReadOnly;

            public override bool IsVisible => true;

            public override void ActionImpl() => ((TextBox)Parent).Paste();
         }
      }

      CmdManagedByControlImpl[] IControlWithManagedCmds.CmdsImpl => myCommands;

      CmdManagedByControlImpl? IControlWithManagedCmds.this[string id] => myCommands?.FirstOrDefault(c => c.Id == id);


      event OnCmdStateUpdateHandler IControlWithManagedCmds.OnCmdStateUpdate
      {
         add => myOnCmdStateUpdate += value;

         remove => myOnCmdStateUpdate -= value;
      }

      protected override void OnReadOnlyChanged(EventArgs e)
      {
         base.OnReadOnlyChanged(e);
         myOnCmdStateUpdate?.Invoke(this);
      }
   }
}
