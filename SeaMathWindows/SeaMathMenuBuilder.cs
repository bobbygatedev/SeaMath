using Gate.ToolsView.MenuCommand;
using System.Drawing;

namespace Gate.SeaMath
{
   /// <summary>
   /// Import all <see cref="SeaMathCmd"/> <see cref="SeaMathCmd.MenuCaption"/> 
   /// </summary>
   public class SeaMathMenuBuilder : CmdMenuBuilder
   {
      public const string ID = "SeaMath.GatePad.Menu";

      public SeaMathMenuBuilder(SeaMathSession session) : base(ID, "SeaMath") => Session = session;

      public SeaMathSession Session { get; }

      protected override void myCustomInit(CmdMenu cmdMenu) { }

      protected override Image? myGetCmdImage(Cmd cmd) => null;
   }
}
