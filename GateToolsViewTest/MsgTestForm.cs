using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.ToolsViewTest
{
   internal partial class MsgTestForm : Form
   {
      private int myMsgCounter = 0;
      private static MsgType myMsgType = MsgType.info;
      private readonly TxtStore myStore = new TxtStore();

      public MsgTestForm()
      {
         InitializeComponent();

         var tmp = Path.GetTempFileName();

         for (int i = 0; i < 20; i++)
         {
            myStore.AddLines($"Line {i + 1}");
         }

         CtrlMsgList?.MthSetNppAsTextOpener(@"c:\erik\Install\Notepad++\notepad++.exe");

         myStore.Save(tmp);
      }

      private void CtrlTimer_Tick(object? sender, EventArgs e)
      {
         var lst = Enum.GetValues(typeof(MsgType)).Cast<MsgType>().ToList();
         var id = lst.IndexOf(myMsgType);
         var ln_id = id % myStore.LineCount + 1;

         if (++id >= lst.Count)
         {
            id = 0;
         }

         CtrlMsgList.MthAddMsg(
            new Msg(
               myMsgType,
               $"Type={myMsgType}({++myMsgCounter})",
               ln_id,
               TxtTokenConst.FromFromLen(myStore, new TxtPos(ln_id, 1), 4)));

         myMsgType = lst[id];
      }



      static void Main()
      {
         /*
                this.CtrlImageList.Images.SetKeyName(0, "xCtrlBtnMinimize.Image.png");
         this.CtrlImageList.Images.SetKeyName(1, "CtrlBtnMaximizeToggle.Image.png");
         this.CtrlImageList.Images.SetKeyName(2, "CtrlBtnClose.Image.png");
   
          */


         return;//tododo
         var frm = new MsgTestForm();

         frm.ShowDialog();
      }

      private void MsgTestForm_FormClosed(object? sender, FormClosedEventArgs e)
      {
         myStore.FileInfo.Delete();
      }
   }
}
