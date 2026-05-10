using Gate.Tools;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public class FileControlEditor : ExtendedColumnListViewControl.EditorType.ByControl<FileControl>
   {
      public FileControlEditor() { }


      protected override void myActionOnEmbed(FileControl control, ExtendedColumnListViewControl.CellType cell)
      {
         try
         {
            control.OnValueAccept += Control_OnValueAccept;
            control.PpRelativePath = cell.Tag as RelativePath;
         }
         catch
         {
            control.PpRelativePath = null;
         }
      }

      private void Control_OnValueAccept(object? sender, RelativePath? relativePath) => CellEmbeddeded?.UnembedTempControl();

      protected override void myActionOnUnembed(FileControl control, ExtendedColumnListViewControl.CellType cell)
      {
         control.OnValueAccept -= Control_OnValueAccept;

         if (control.PpRelativePath != null)
         {
            cell.Tag = control.PpRelativePath;
            cell.Text = control.PpRelativePath.RelativePathLinux;
         }
         else
         {
            cell.Tag = null;
            cell.Text = "";
         }
      }

      protected override void myDoSetText(FileControl textBoxControl, string text) { }

      protected override string myGetCellText(FileControl textBoxControl) => throw new Crash();
   }



}
