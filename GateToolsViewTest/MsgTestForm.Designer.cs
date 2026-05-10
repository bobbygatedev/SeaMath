namespace Gate.ToolsViewTest
{
   partial class MsgTestForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
     

      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         this.CtrlMsgList = new Gate.ToolsView.Extended.MsgListControl();
         this.CtrlTimer = new System.Windows.Forms.Timer(this.components);
         this.SuspendLayout();
         // 
         // CtrlMsgList
         // 
         this.CtrlMsgList.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlMsgList.Location = new System.Drawing.Point(0, 0);
         this.CtrlMsgList.Margin = new System.Windows.Forms.Padding(4);
         this.CtrlMsgList.Name = "CtrlMsgList";
         this.CtrlMsgList.PpIsInForegroundIfError = true;
         this.CtrlMsgList.PpSubMessageTab = 3;
         this.CtrlMsgList.PpTextOpener = null;
         this.CtrlMsgList.Size = new System.Drawing.Size(656, 348);
         this.CtrlMsgList.TabIndex = 0;
         // 
         // CtrlTimer
         // 
         this.CtrlTimer.Enabled = true;
         this.CtrlTimer.Interval = 1000;
         this.CtrlTimer.Tick += new System.EventHandler(this.CtrlTimer_Tick);
         // 
         // MsgTestForm
         // 
         this.ClientSize = new System.Drawing.Size(656, 348);
         this.Controls.Add(this.CtrlMsgList);
         this.Name = "MsgTestForm";
         this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MsgTestForm_FormClosed);
         this.ResumeLayout(false);

      } 

      #endregion

      private ToolsView.Extended.MsgListControl CtrlMsgList;
      private System.Windows.Forms.Timer CtrlTimer;
   }
}