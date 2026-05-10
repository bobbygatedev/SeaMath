namespace Gate.ToolsView.MenuCommand
{
   public class CmdManagedByComboBox : ComboBox, IControlWithManagedCmds
   {
      private OnCmdStateUpdateHandler? myOnCmdStateUpdate;
      private CmdManagedByControlImpl[] myCommands;

      public CmdManagedByComboBox() => myCommands = new CmdManagedByControlImpl[] {
            new InnerImpl.Copy(this),
            new InnerImpl.Cut(this),
            new InnerImpl.Paste(this),
         };
      private static class InnerImpl
      {
         public class Copy : CmdManagedByControlImpl
         {
            public Copy(CmdManagedByComboBox parent) : base(parent, CmdCommonlyUsedIds.COPY) { }

            public override bool IsEnabled => true;

            public override bool IsVisible => true;

            public override void ActionImpl()
            {
               var cmb = (ComboBox)Parent;

               if (cmb.SelectedIndex != -1)
               {
                  if (cmb?.SelectedItem is object obj)
                  {
                     Clipboard.SetDataObject(obj);
                  }
               }
               else if ((Parent as ComboBox)?.SelectedText is string str)
               {
                  Clipboard.SetText(str);
               }
            }
         }

         public class Cut : CmdManagedByControlImpl
         {
            public Cut(CmdManagedByComboBox parent) : base(parent, CmdCommonlyUsedIds.CUT) { }

            public override bool IsEnabled => true;

            public override bool IsVisible => true;

            public override void ActionImpl()
            {
               //todo
            }
         }

         public class Paste : CmdManagedByControlImpl
         {
            public Paste(CmdManagedByComboBox parent) : base(parent, CmdCommonlyUsedIds.PASTE) { }

            public override bool IsEnabled => true;

            public override bool IsVisible => true;

            public override void ActionImpl()
            {
               var cmb = (ComboBox)Parent;

               if (cmb.SelectedText != "")
               {
                  var txt = Clipboard.GetText();
               }

               //todo
            }
         }
      }

      CmdManagedByControlImpl[] IControlWithManagedCmds.CmdsImpl => myCommands;

      CmdManagedByControlImpl? IControlWithManagedCmds.this[string id] => myCommands.FirstOrDefault(c => c.Id == id);

      event OnCmdStateUpdateHandler IControlWithManagedCmds.OnCmdStateUpdate
      {
         add => myOnCmdStateUpdate += value;

         remove => myOnCmdStateUpdate -= value;
      }
   }
}
