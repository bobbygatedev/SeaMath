using Gate.ToolsView.AppParams.ValueControls;

namespace Gate.ToolsView.AppParams
{
   partial class AppParamOptionTreeRecordControl
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

      private void InitializeComponent()
      {
         this.CtrlValueRepoTree = new Gate.ToolsView.AppParams.ValueControls.ValueRepoTreeControl();
         this.SuspendLayout();
         // 
         // CtrlValueRepoTree
         // 
         this.CtrlValueRepoTree.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlValueRepoTree.Location = new System.Drawing.Point(0, 0);
         this.CtrlValueRepoTree.Name = "CtrlValueRepoTree";
         this.CtrlValueRepoTree.Size = new System.Drawing.Size(392, 506);
         this.CtrlValueRepoTree.TabIndex = 0;
         this.CtrlValueRepoTree.OnRepoNodeSelected += new Gate.ToolsView.AppParams.ValueControls.ValueRepoTreeControl.OnRepoNodeSelectedHandler(this.CtrlValueRepoTree_OnRepoNodeSelected);
         // 
         // AppParamOptionsTreeControl
         // 
         this.Controls.Add(this.CtrlValueRepoTree);
         this.Name = "AppParamOptionsTreeControl";
         this.Size = new System.Drawing.Size(392, 506);
         this.ResumeLayout(false);

      }

      #endregion

      private ValueRepoTreeControl CtrlValueRepoTree;
   }
}