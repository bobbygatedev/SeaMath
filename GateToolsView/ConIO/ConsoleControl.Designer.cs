namespace Gate.ToolsView.ConIO
{
   partial class ConsoleControl
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
         this.CtrlTableLayout = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlScrollBar = new Gate.ToolsView.Extended.ExtendedScrollBar();
         this.CtrlTimer = new System.Windows.Forms.Timer(this.components);
         this.CtrlTableLayout.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlTableLayout
         // 
         this.CtrlTableLayout.ColumnCount = 2;
         this.CtrlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
         this.CtrlTableLayout.Controls.Add(this.CtrlScrollBar, 1, 0);
         this.CtrlTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayout.Name = "CtrlTableLayout";
         this.CtrlTableLayout.RowCount = 1;
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
         this.CtrlTableLayout.Size = new System.Drawing.Size(539, 272);
         this.CtrlTableLayout.TabIndex = 1;
         // 
         // CtrlScrollBar
         // 
         this.CtrlScrollBar.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlScrollBar.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlScrollBar.Enabled = false;
         this.CtrlScrollBar.Location = new System.Drawing.Point(527, 3);
         this.CtrlScrollBar.Name = "CtrlScrollBar";
         this.CtrlScrollBar.PpGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlScrollBar.PpMouseWheelControl = this;
         this.CtrlScrollBar.PpWheelSensitivityMultiplier = 4;
         this.CtrlScrollBar.Size = new System.Drawing.Size(9, 266);
         this.CtrlScrollBar.TabIndex = 1;
         this.CtrlScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.CtrlScrollBar_Scroll);
         // 
         // CtrlTimer
         // 
         this.CtrlTimer.Enabled = true;
         this.CtrlTimer.Tick += new System.EventHandler(this.CtrlTimer_Tick);
         // 
         // ConsoleControl3
         // 
         this.Controls.Add(this.CtrlTableLayout);
         this.DoubleBuffered = true;
         this.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.Name = "ConsoleControl3";
         this.Size = new System.Drawing.Size(539, 272);
         this.CtrlTableLayout.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private TableLayoutPanel CtrlTableLayout;
      private Extended.ExtendedScrollBar CtrlScrollBar;
      private System.Windows.Forms.Timer CtrlTimer;
   }
}