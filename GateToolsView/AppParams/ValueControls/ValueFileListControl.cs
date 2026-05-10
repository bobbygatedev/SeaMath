using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.AppParams.ValueControls;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature;
using static Gate.Tools.AppParams.AppParam;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.AppParams.ValueControls
{
   /// <summary>
   /// 
   /// </summary>
   [ValueControlAssociation(Id = ValueControlStandardId.file_list)]
   public partial class ValueFileListControl : UserControl, IValueControl
   {
      private Usage myUsage = Usage.Files;
      private ShallExistMode myShallExist = ShallExistMode.none;
      private readonly DragDropEnableFeature.CheckForFiles myFeatureDrop;

      public event OnValueChangeHandler? OnValueChange;

      public enum Usage
      {
         Files = 0,
         Directories
      }

      public enum ShallExistMode
      {
         none = 0,
         shall_exist = 1,
         shall_directory_exist = 2,
      }

      public ValueFileListControl()
      {
         InitializeComponent();

         myFeatureDrop = CtrlFeature.Add<DragDropEnableFeature.CheckForFiles>(CtrlListView);
         myFeatureDrop.OnFileSystemInfos += MyFeatureDrop_OnFileSystemInfos;
         CtrlListView.MthRowAdd();
      }

      [ValueControlAssociation(Id = ValueControlStandardId.dir_list)]
      public class Dirs : ValueFileListControl
      {
         public Dirs() => PpUsage = Usage.Directories;
      }

      /// <summary>
      /// 
      /// </summary>
      public Usage PpUsage
      {
         get => myUsage;

         set
         {
            myUsage = value;

            switch (myUsage)
            {
               case Usage.Files:
                  myFeatureDrop.CheckMode &= ~DragDropEnableFeature.CheckForFiles.CheckForFilesMode.is_dir;
                  myFeatureDrop.CheckMode |= DragDropEnableFeature.CheckForFiles.CheckForFilesMode.is_file;
                  break;

               case Usage.Directories:
                  myFeatureDrop.CheckMode &= ~DragDropEnableFeature.CheckForFiles.CheckForFilesMode.is_file;
                  myFeatureDrop.CheckMode |= DragDropEnableFeature.CheckForFiles.CheckForFilesMode.is_dir;
                  break;

               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// If not null path are represented as relative
      /// </summary>
      public DirectoryInfo? PpRelativeBasePath { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public ShallExistMode PpShallExist
      {
         get => myShallExist;

         set
         {
            myShallExist = value;

            switch (myShallExist)
            {
               case ShallExistMode.none:
                  myFeatureDrop.CheckMode &= ~DragDropEnableFeature.CheckForFiles.CheckForFilesMode.shall_exist;
                  break;

               case ShallExistMode.shall_directory_exist:
               case ShallExistMode.shall_exist:
                  myFeatureDrop.CheckMode |= DragDropEnableFeature.CheckForFiles.CheckForFilesMode.shall_exist;
                  break;

               default: throw new Crash();
            }
         }
      }

      /// <summary>
      /// Separated by comma or semicolon examples *.exe,*.dll or *.exe;*.dll
      /// </summary>
      public string? PpPattern { get; set; } = null;

      public string[] PpPaths
      {
         get => (CtrlListView.PpRows ?? []).Select(r => myGetRelativeHandled(r.Cells[0].Text.ExtTrim())).Where(t => !t.IsBlank()).ToArray();

         set
         {
            var pts = (value ?? []).Select(d => d.ExtTrim()).Where(d => !d.IsBlank()).ToArray();

            myDoSetPaths(pts);
            myActionOnValueChanged(this, PpPathsString);
         }
      }

      public string PpPathsString
      {
         get => string.Join(";", PpPaths);

         set => PpPaths =
            value.ExtTrim().Split(';').
            Select(d => d.ExtTrim()).
            Where(d => !d.IsBlank()).
            Select(d => myGetRelativeHandled(d)).ToArray();
      }

      public string ParamName { get => CtrlListView.PpColumns[0].Text; set => CtrlListView.PpColumns[0].Text = value; }

      public Scalar? PpScalar { get; private set; }

      object? IValueControl.ParamValue
      {
         get => PpPathsString;
         set => PpPathsString = value is string str ? str : "";
      }

      object? IValueControl.Tag { get => PpScalar; set => base.Tag = PpScalar = value as Scalar; }

      void IValueControl.ActionOnAppParamAssociationAction(AppParam param) => myActionOnAppParamAssociationAction(param);

      protected virtual void myActionOnAppParamAssociationAction(AppParam param) { }

      protected virtual void myActionOnValueChanged(ValueFileListControl sender, string dirsString) => OnValueChange?.Invoke(this, PpPathsString);

      public override Size GetPreferredSize(Size proposedSize) => new Size(50, 100);


      private void myDoSetPaths(string[] paths)
      {
         foreach (var row in CtrlListView.PpRows ?? []) { CtrlListView.MthRowsRemove(row); }

         foreach (var pth in paths.Where(p => myIsValidPath(p))) { CtrlListView.MthRowAdd().Cells[0].Text = myGetPathAbsolute(pth); }

         CtrlListView.MthRowAdd();
      }

      private string myGetRelativeHandled(string path)
      {
         if (PpRelativeBasePath != null)
         {
            if (Path.IsPathRooted(path))
            {
               var rel_pth = new RelativePath(PpRelativeBasePath, path);

               return rel_pth.RelativePathLinux;
            }
            else
            {
               return path;
            }
         }
         else
         {
            return path;
         }
      }

      private bool myIsValidPath(string path) => myIsValidPath(path, out _);

      private bool myIsPathPatternValid(string path)
      {
         if (PpPattern.IsBlank()) { return true; }
         else
         {
            var sp =
              PpPattern.Nn().Split([';', ','], StringSplitOptions.RemoveEmptyEntries).
              Select(d => d.ExtTrim()).
              Where(d => d != "").
              ToArray();

            return sp.Any(p => myIsValidPattern(path, p));
         }
      }

      private bool myIsValidPattern(string path, string pattern)
      {
         var ext = Path.GetExtension(path);

         pattern = pattern.ExtTrim();

         return pattern.StartsWith("*") ?
            pattern.Substring(1).ExtTrim().IsEqualNoContent(ext) :
            pattern.IsEqualNoContent(ext);
      }

      protected virtual bool myIsValidPath(string path, out string? pathSanitized)
      {
         var abs_pth = myGetPathAbsolute(path);

         if (PpPaths.Any(p => p.IsEqualNoContent(path)))
         {
            pathSanitized = null;

            return false;
         }

         if (!myIsPathPatternValid(abs_pth))
         {
            MessageBox.Show($"{abs_pth} doesn't match pattern '{PpPattern.ExtTrim()}'");
            pathSanitized = null;

            return false;
         }
         else
         {
            if (PpUsage == Usage.Directories)
            {
               try { pathSanitized = new DirectoryInfo(abs_pth).GetFullPathCase().FullName; }
               catch
               {
                  MessageBox.Show($"Invalid path format for '{abs_pth}'!");
                  pathSanitized = null;

                  return false;
               }

               switch (PpShallExist)
               {
                  case ShallExistMode.none: return true;

                  case ShallExistMode.shall_exist:
                  case ShallExistMode.shall_directory_exist:
                     if (Directory.Exists(abs_pth)) { return true; }
                     else
                     {
                        MessageBox.Show($"Directory {abs_pth} doesn't exist!");
                        pathSanitized = null;

                        return false;
                     }

                  default: throw new Crash();
               }
            }
            else if (PpUsage == Usage.Files)
            {
               var fif_snz = null as FileInfo;

               try
               {
                  fif_snz = new FileInfo(abs_pth).GetFullPathCase();
                  pathSanitized = fif_snz.FullName;
               }
               catch
               {
                  MessageBox.Show($"Invalid path format for '{abs_pth}'!");
                  pathSanitized = null;

                  return false;
               }

               switch (PpShallExist)
               {
                  case ShallExistMode.none: return true;

                  case ShallExistMode.shall_exist:
                     if (fif_snz.Exists) { return true; }
                     else
                     {
                        MessageBox.Show($"File {abs_pth} doesn't exist!");
                        pathSanitized = null;

                        return false;
                     }
                  case ShallExistMode.shall_directory_exist:
                     if (fif_snz.Exists || (fif_snz?.Directory?.Exists ?? false)) { return true; }
                     else
                     {
                        MessageBox.Show($"Neither File {abs_pth}, nor containing dir {new FileInfo(abs_pth).DirectoryName} exist!");
                        pathSanitized = null;

                        return false;
                     }

                  default: throw new Crash();
               }
            }
            else { throw new Crash(); }
         }
      }

      private string myGetPathAbsolute(string path)
      {
         if (PpRelativeBasePath != null && !Path.IsPathRooted(path))
         {
            var rel_pth = new RelativePath(PpRelativeBasePath);

            rel_pth.RelativePathLinux = path;

            return rel_pth.Absolute.ExtTrim();
         }
         else
         {
            return path;
         }
      }

      private void MyFeatureDrop_OnFileSystemInfos(FileSystemInfo[] fileSystemInfos)
      {
         foreach (var fil in fileSystemInfos)
         {
            if (myIsValidPath(fil.FullName, out var ns))
            {
               var cel = CtrlListView.PpRows.LastOrDefault()?.Cells.FirstOrDefault();

               if (cel != null)
               {
                  cel.Text = fil.FullName;
                  CtrlListView.MthRowAdd();
               }
            }
         }

         myActionOnValueChanged(this, PpPathsString);
      }

      private void CtrlListView_OnCellDoubleClick(CellType cell)
      {
         var edi = new EditorType.ByComboBox();

         edi.TextControl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
         edi.TextControl.AutoCompleteSource = PpUsage == Usage.Directories ? AutoCompleteSource.FileSystemDirectories : AutoCompleteSource.FileSystem;
         cell.Editor = edi;
         cell.Edit();
      }

      private void CtrlListView_OnCellTextUpdating(CellType cell, UpdateCellTextArgs args)
      {
         args.IsAccept = myIsValidPath(args.Text2Set.Nn(), out var pth_snz);
         args.Text2Set = pth_snz.ExtTrim();

         if (args.IsAccept)
         {
            if (cell.RowIdx == CtrlListView.PpRows.Length - 1)
            {
               CtrlListView.MthRowAdd();
            }
            else
            {
               CtrlListView.PpSelectedRowIdx++;
            }
         }
      }

      private void CtrlListView_OnCellTextUpdated(CellType cell, string newText) => myActionOnValueChanged(this, PpPathsString);

      private void CtrlListView_OnCellKeyDown(object? sender, CellType cell, KeyEventArgs e)
      {
         if (
            e.KeyCode == Keys.Delete &&
            cell?.Text != null &&
            MessageBox.Show($"Do you want to remove item {cell?.Text}", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
         {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            CtrlListView.MthRowsRemove(cell.Row);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            myActionOnValueChanged(this, PpPathsString);
         }
      }
   }
}
