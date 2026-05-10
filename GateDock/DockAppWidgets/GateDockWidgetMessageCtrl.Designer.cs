namespace Gate.Dock.DockAppWidgets
{
   partial class GateDockWidgetMessageCtrl
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
         this.SuspendLayout();
         // 
         // GateDockWidgetMessageCtrl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.Name = "GateDockWidgetMessageCtrl";
         this.OnChangingMainForm += new Gate.Dock.DockWidget.GateDockWidgetCtrl.OnChanginMainFormHandler(this.GateDockWidgetMessageCtrl_OnChangingMainForm);
         this.ResumeLayout(false);

      } 

      #endregion
   }
}