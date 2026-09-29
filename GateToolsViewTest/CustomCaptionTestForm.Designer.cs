namespace GateToolsViewTest
{
   partial class CustomCaptionTestForm
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
         gateDockWidgetCaption1 = new Gate.Dock.DockWidget.GateDockWidgetCaption();
         SuspendLayout();
         // 
         // gateDockWidgetCaption1
         // 
         gateDockWidgetCaption1.BackColor = Color.FromArgb(40, 40, 40);
         gateDockWidgetCaption1.Location = new Point(46, 61);
         gateDockWidgetCaption1.Margin = new Padding(4);
         gateDockWidgetCaption1.Name = "gateDockWidgetCaption1";
         gateDockWidgetCaption1.PpCmdMenuRef = null;
         gateDockWidgetCaption1.PpIsWidgetSelected = false;
         gateDockWidgetCaption1.PpSkin = null;
         gateDockWidgetCaption1.Size = new Size(566, 20);
         gateDockWidgetCaption1.TabIndex = 1;
         // 
         // CustomCaptionTestForm
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         BackColor = SystemColors.ActiveCaptionText;
         ClientSize = new Size(800, 450);
         Controls.Add(gateDockWidgetCaption1);
         Name = "CustomCaptionTestForm";
         Text = "Form1";
         ResumeLayout(false);
      }

      #endregion

      private Gate.Dock.DockWidget.GateDockWidgetCaption gateDockWidgetCaption1;
   }
}