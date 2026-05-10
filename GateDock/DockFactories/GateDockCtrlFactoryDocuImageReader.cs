using Gate.Dock.DockDocu;
using Gate.Dock.DockTab;
using Gate.ToolsView.MenuCommand;

namespace Gate.Dock.DockFactories
{
   public class GateDockCtrlFactoryDocuImageReader : GateDockDocuFactory
   {
      public override string CtrlGuid => "{FFC4C340-840B-41CD-A6DC-D8DB5B63A203}";

      public override string[] AssociatedExtensions => new string[] { ".bmp", ".jpeg", ".jpg" };

      public override string ContentDescriptor => "Image Files";

      public override string DocuTypeGuid => "{67B212A0-C30C-4CB0-A7B8-4F1924AB1A66}";

      protected override GateDockTabPageCtrl myMakeTabPageControl() => new GateDockDocuImageReaderCtrl();

      public override void AddExtraMenus(CmdContainer cmdContainer) { }
   }
}

