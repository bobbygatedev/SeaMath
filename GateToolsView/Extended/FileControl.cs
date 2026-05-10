using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class FileControl : UserControl
   {
      private RelativePath? myRelPath;
      private RelativePath? myRelPathOriginal;

      public delegate void OnValueAcceptHandler(object? sender, RelativePath? relativePath);

      public event OnValueAcceptHandler? OnValueAccept;

      public FileControl()
      {
         InitializeComponent();

         this.AddFeature<DragDropEnableFeature>();
      }

      private class InnerLayout : LayoutEngine
      {
         public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
         {
            var ctr = (FileControl)container;

            ctr.CtrlTextBox.Location = new Point();
            ctr.CtrlTextBox.Width = ctr.Width - ctr.Height;
            ctr.CtrlTextBox.Height = ctr.Height;
            ctr.CtrlButtonSearch.Location = new Point(ctr.CtrlTextBox.Width, 0);
            ctr.CtrlButtonSearch.Width = ctr.CtrlButtonSearch.Height = ctr.Height;

            return true;
         }
      }

      public override LayoutEngine LayoutEngine => new InnerLayout();

      public RelativePath? PpRelativePath
      {
         get => myRelPath;

         set
         {
            if (myRelPath != null)
            {
               myRelPath.OnChangeAbsolute -= MyRelPath_OnChangeAbsolute;
            }

            myRelPath = value;

            if (myRelPath != null)
            {
               myRelPath.OnChangeAbsolute += MyRelPath_OnChangeAbsolute;
               myRelPathOriginal = new RelativePath(myRelPath.Options, myRelPath.BaseDirInfo, myRelPath.Absolute);
            }
            else
            {
               myRelPathOriginal = null;
            }

            CtrlTextBox.Text = myRelPath != null ? myRelPath.Absolute.ExtTrim() : "";
         }
      }

      protected override void OnDragDrop(DragEventArgs drgevent)
      {
         var dat = drgevent.Data?.GetAny();

         if (myRelPath?.Options == RelativePath.OptionsType.file && dat is FileInfo fil)
         {
            myDoValidate(fil.FullName);
         }
         else if (myRelPath?.Options == RelativePath.OptionsType.dir && dat is DirectoryInfo dir)
         {
            myDoValidate(dir.FullName);
         }

         base.OnDragDrop(drgevent);
      }

      private void myDoValidate(string? text = null)
      {
         var txt = text ?? CtrlTextBox.Text;

         if (myRelPath?.Options == RelativePath.OptionsType.file)
         {
            try
            {
               var fif = new FileInfo(txt);

               if (fif.Exists)
               {
                  myRelPath = new RelativePath(RelativePath.OptionsType.file, myRelPath.BaseDirInfo, fif.FullName);
               }
               else
               {
                  PpRelativePath = myRelPath;
               }
            }
            catch
            {
               PpRelativePath = myRelPath;
            }
         }
         else if (myRelPath?.Options == RelativePath.OptionsType.dir)
         {
            try
            {
               var dif = new DirectoryInfo(txt);

               if (dif.Exists)
               {
                  myRelPath = new RelativePath(RelativePath.OptionsType.dir, myRelPath.BaseDirInfo, dif.FullName);
               }
               else
               {
                  PpRelativePath = myRelPath;
               }
            }
            catch
            {
               PpRelativePath = myRelPath;
            }
         }
         else
         {
            throw new Crash();
         }
      }

      private void MyRelPath_OnChangeAbsolute(RelativePath path) => CtrlTextBox.Text = path.Absolute.ExtTrim();

      private void FileControl_Load(object? sender, EventArgs e) => PerformLayout();

      private void CtrlButtonSearch_Click(object? sender, EventArgs e)
      {
         if (myRelPath != null)
         {
            if (myRelPath.Options == RelativePath.OptionsType.file)
            {
               var fil_dlg = new OpenFileDialog();

               fil_dlg.FileName = myRelPath.Absolute;

               if (fil_dlg.ShowDialog() == DialogResult.OK && File.Exists(fil_dlg.FileName))
               {
                  myRelPath.Absolute = fil_dlg.FileName;
               }
            }
            else if (myRelPath.Options == RelativePath.OptionsType.dir)
            {
               var dir_dlg = new FolderBrowserDialog();

               dir_dlg.SelectedPath = myRelPath?.Absolute ?? "";
               dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;

               if (dir_dlg.ShowDialog() == DialogResult.OK && Directory.Exists(dir_dlg.SelectedPath))
               {
                  (myRelPath ?? throw new Crash()).Absolute = dir_dlg.SelectedPath;
                  CtrlTextBox.Text = myRelPath.RelativePathLinux;
               }
            }
         }
      }

      private void CtrlTextBox_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
      {
         if (myRelPath != null)
         {
            myDoValidate();
         }
      }

      private void CtrlTextBox_KeyUp(object? sender, KeyEventArgs e)
      {
         if (!e.Control && !e.Shift && !e.Alt)
         {
            switch (e.KeyCode)
            {
               case Keys.Enter:
                  myDoValidate();
                  OnValueAccept?.Invoke(this, PpRelativePath);
                  break;

               case Keys.Escape:
                  PpRelativePath = myRelPathOriginal;
                  break;
            }
         }
      }
   }
}
