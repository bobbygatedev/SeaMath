using Gate.Tools;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// Displays a coloured border and a transparent center.
   /// </summary>
   public partial class ExtendedRectMarkerForm : Form
   {
      private InnerFormAssociation myFormAssociation;
      private CtrlFeatureBorder myFeatureBorder;

      public ExtendedRectMarkerForm()
      {
         this.AddFeature<FormFeatureAlwaysOnTop>();
         this.AddFeature<FormFeatureTransparent>();
         this.AddFeature<CtrlFeatureMessageTransparent>();

         InitializeComponent();
         SetStyle(ControlStyles.Selectable, false);
         FormBorderStyle = FormBorderStyle.None;

         myFeatureBorder = this.AddFeature<CtrlFeatureBorder>();
         myFeatureBorder.BorderColor = Color.Yellow;
         myFeatureBorder.BorderWidth = 5.0f;
         Refresh();
         myFormAssociation = new InnerFormAssociation(this);
      }

      private class InnerFormAssociation
      {
         private readonly ExtendedRectMarkerForm myMarkerForm;
         private Control? myAssociatedControl = null;
         private Form? myAssociatedParentForm = null;
         private Rectangle? myRelativeRectangle = null;

         public InnerFormAssociation(ExtendedRectMarkerForm parent) => myMarkerForm = parent;

         public Control? AssociatedControl
         {
            get => myAssociatedControl;
            set
            {
               if (myAssociatedControl != value)
               {
                  if (value != null && myAssociatedControl != null) { throw new Gate.Tools.ToolsException("Already associated control!"); }
                  else if (value != null)
                  {
                     myAssociatedControl = value;
                     myAssociatedParentForm = value?.MthGetParentForm();

                     if (myAssociatedParentForm != null)
                     {
                        myAssociatedParentForm.FormClosed += (s, e) => myMarkerForm.Close();
                        myAssociatedParentForm.Resize += MyAssociatedForm_Resize;
                        myAssociatedParentForm.Move += MyAssociatedForm_Move;
                     }

                     if (myAssociatedControl != myAssociatedParentForm)
                     {
                        myAssociatedControl.Resize += MyAssociatedControl_Resize;
                        myAssociatedControl.Move += MyAssociatedControl_Move;
                     }

                     myMarkerForm.Show();
                     myMarkerForm.Owner = myAssociatedParentForm;
                     myAlignToDisplayScreen();
                  }
                  else { myDeAssociate(); }
               }
            }
         }

         public Rectangle? RelativeRectangle
         {
            get => myRelativeRectangle;

            set
            {
               myRelativeRectangle = value;
               myAlignToDisplayScreen();
            }
         }

         private void myDeAssociate()
         {
            (myAssociatedParentForm ?? throw new Crash()).Resize -= MyAssociatedForm_Resize;
            myAssociatedParentForm.Move -= MyAssociatedForm_Move;

            if (myAssociatedControl != myAssociatedParentForm)
            {
               (myAssociatedControl ?? throw new Crash()).Resize -= MyAssociatedControl_Resize;
               myAssociatedControl.Move -= MyAssociatedControl_Move;
            }

            myAssociatedParentForm = null;
            myAssociatedControl = null;
         }

         private void myAlignToDisplayScreen()
         {
            if (myAssociatedControl != null)
            {
               var cnt_loc = myAssociatedControl.Location;
               var cnt_siz = myAssociatedControl.DisplayRectangle.Size;

               if (myRelativeRectangle.HasValue)
               {
                  cnt_loc = myRelativeRectangle.Value.Location;
                  cnt_siz = myRelativeRectangle.Value.Size;
               }

               myMarkerForm.Location = (myAssociatedControl?.Parent ?? throw new Crash()).PointToScreen(cnt_loc);
               myMarkerForm.Size = cnt_siz;
            }
         }

         private void MyAssociatedControl_Move(object? sender, EventArgs e) => myAlignToDisplayScreen();

         private void MyAssociatedControl_Resize(object? sender, EventArgs e) => myAlignToDisplayScreen();

         private void MyAssociatedForm_Move(object? sender, EventArgs e) => myAlignToDisplayScreen();

         private void MyAssociatedForm_Resize(object? sender, EventArgs e) => myAlignToDisplayScreen();
      }

      /// <summary>
      /// 
      /// </summary>
      public Control? PpAssociatedControl { get => myFormAssociation.AssociatedControl; set => myFormAssociation.AssociatedControl = value; }

      /// <summary>
      ///  the rectangle inside the associaed control
      /// </summary>
      public Rectangle? PpAssociatedControlRectangle { get => myFormAssociation.RelativeRectangle; set => myFormAssociation.RelativeRectangle = value; }

      /// <summary>
      /// 
      /// </summary>
      public Color PpBorderColor { get => myFeatureBorder.BorderColor; set => myFeatureBorder.BorderColor = value; }

      /// <summary>
      /// 
      /// </summary>
      public float PpBorderWidth { get => myFeatureBorder.BorderWidth; set => myFeatureBorder.BorderWidth = value; }

      protected override void OnClosed(EventArgs e)
      {
         PpAssociatedControl = null;

         base.OnClosed(e);
      }
   }
}
