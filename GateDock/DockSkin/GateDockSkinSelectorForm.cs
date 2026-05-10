using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.ComponentModel;
using System.Data;
using static System.Windows.Forms.ListViewItem;

namespace Gate.Dock.DockSkin
{
   /// <summary>
   /// 
   /// </summary>
   public partial class GateDockSkinSelectorForm : Form, IGateDockCtrlWithSkin
   {
      private GateDockSkin? myBackUpSkin;
      private SkinChildCtrlDispacther? mySkinChildCtrlDispacther;
      private bool myIsAccepting = false;
      private bool myIsDefaultChanged = false;
      private GateDockMainForm? myMainForm;

      public GateDockSkinSelectorForm()
      {
         InitializeComponent();
         mySkinChildCtrlDispacther = new SkinChildCtrlDispacther(this);
      }

      public class SkinChildCtrlDispacther : GateDockSkinChildCtrlDispatcher
      {
         public SkinChildCtrlDispacther(GateDockSkinSelectorForm parent) : base(parent) { }
      }

      private static class InnerColorHelper
      {
         public static Color ComplementColor(Color color)
         {
            my_ColorToHSV(color, out double hue, out double sat, out double val);

            var new_val = val;
            var new_hue = hue;
            var new_sat = sat;

            if (hue > 60) { new_hue = hue + 180; }
            else if (val >= 0.4 && val <= 0.6) { new_sat = 1.0 - sat; }
            else { new_val = 1.0 - val; }

            return my_GetColorFromHSV(new_hue, new_sat, new_val);
         }

         private static void my_ColorToHSV(Color color, out double hue, out double saturation, out double value)
         {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
         }

         private static Color my_GetColorFromHSV(double hue, double saturation, double value)
         {
            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;

            var v = Convert.ToInt32(value);
            var p = Convert.ToInt32(value * (1 - saturation));
            var q = Convert.ToInt32(value * (1 - f * saturation));
            var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0) { return Color.FromArgb(255, v, t, p); }
            else if (hi == 1) { return Color.FromArgb(255, q, v, p); }
            else if (hi == 2) { return Color.FromArgb(255, p, v, t); }
            else if (hi == 3) { return Color.FromArgb(255, p, q, v); }
            else if (hi == 4) { return Color.FromArgb(255, t, p, v); }
            else { return Color.FromArgb(255, v, p, q); }
         }
      }

      public TxtStringConverter PpAppParamStringConverter { get; set; } = new TxtStringConverter.Default();

      public SkinChildCtrlDispacther? PpSkinChildCtrlDispacther
      {
         get => mySkinChildCtrlDispacther;
         set
         {
            if (value != null) { mySkinChildCtrlDispacther = value; }
            else { MessageBox.Show(string.Format("{0}.PpSkinChildCtrlDispacther can't be null", GetType().Name)); }
         }
      }

      public GateDockSkin? PpSkin
      {
         get => PpSkinChildCtrlDispacther?.Skin;
         set => PpSkinChildCtrlDispacther.NnOrCrash().Skin = value;
      }

      public AppParam.Scalar? PpSelectedAppParam =>
         CtrlListSkinParams.SelectedItems.Count > 0 ? CtrlListSkinParams.SelectedItems[0].Tag as AppParam.Scalar : null;

      public GateDockMainForm PpMainForm => myMainForm ?? throw new NullReferenceException();

      public GateDockSkin? PpSelectedSkin => CtrlListSkins.SelectedItems.Count > 0 ? CtrlListSkins.SelectedItems[0].Tag as GateDockSkin : null;

      public GateDockSkin[] PpSkinsAll => CtrlListSkins.Items.Cast<ListViewItem>().Select(l => l.Tag).OfType<GateDockSkin>().ToArray();

      public void MthShow(GateDockMainForm mainForm)
      {
         myMainForm = mainForm.NnOrCrash();

         var ski = mainForm.PpSkin.NnOrCrash();
         var sk_fld = ski.SkinsFolder;

         myMainForm = mainForm;
         myBackUpSkin = (PpSkin = ski).GetCopy();
         myDoAddSkinToList(ski);

         foreach (var skf in Directory.EnumerateFiles(sk_fld, "*.xml").
            Where(s => Path.GetFileNameWithoutExtension(s).ToLower() != GateDockSkin.CURRENT.ToLower()))
         {
            var nam = Path.GetFileNameWithoutExtension(skf);
            var ski_cpy = ski.GetCopy();
            var mgs = new MsgCollection();

            try
            {
               ski_cpy.Name = Path.GetFileNameWithoutExtension(skf);
               ski_cpy.Load(mgs);

               //todo mgs add to log 

               myDoAddSkinToList(ski_cpy);
            }
            catch { }
         }

         CtrlListSkins.SelectedIndices.Clear();
         CtrlListSkins.SelectedIndices.Add(0);
         Show(mainForm);
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         if (!myIsAccepting && myIsDefaultChanged)
         {
            var res = MessageBox.Show(this, "Current skin has changed. Do you want to confirm?", "", MessageBoxButtons.YesNoCancel);

            switch (res)
            {
               case DialogResult.Yes: break;//confirm
               case DialogResult.No:
                  PpMainForm.PpSkin = myBackUpSkin;
                  break;

               case DialogResult.Cancel:
                  e.Cancel = true;
                  break;
            }
         }

         base.OnClosing(e);
      }

      private ListViewItem myDoAddSkinToList(GateDockSkin skin)
      {
         var lwi = CtrlListSkins.Items.Add(skin.Name);

         lwi.UseItemStyleForSubItems = false;
         lwi.Tag = skin;

         return lwi;
      }

      private void myDoOnSelectionChanged()
      {
         CtrlListSkinParams.Items.Clear();

         if (PpSelectedSkin != null)
         {
            var ski = PpSelectedSkin;

            foreach (var par in ski.Params.AllDescendant.OfType<AppParam.Scalar>())
            {
               var lwi = CtrlListSkinParams.Items.Add(new ListViewItem());

               lwi.UseItemStyleForSubItems = false;
               lwi.Text = par.ParamName;
               mySetParam(lwi.SubItems.Add(""), par);
               lwi.Tag = par;
            }

            CtrlListSkinParams.Columns[0].Width = -1;
            CtrlListSkinParams.Columns[1].Width = -1;
         }
      }

      private bool myDoEdit(ValueType value, AppParam.Scalar appParam) { throw new Crash(); }

      private bool myDoEdit(Color color, AppParam.Scalar appParam)
      {
         using (var dlg = new ColorDialog())
         {
            dlg.Color = color;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
               appParam.ObjValue = dlg.Color;
               myDoUpdateSkinParams(appParam);

               return true;
            }
         }

         return false;
      }

      private bool myDoEdit(Font font, AppParam.Scalar appParamScalar)
      {
         using (var dlg = new FontDialog())
         {
            dlg.Font = font;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
               appParamScalar.ObjValue = dlg.Font;
               myDoUpdateSkinParams(appParamScalar);

               return true;
            }
         }

         return false;
      }

      private void myDoUpdateSkinParams(AppParam.Scalar appParam)
      {
         mySetParam(CtrlListSkinParams.Items.Cast<ListViewItem>().First(i => i.Tag == appParam).SubItems[1], appParam);
         CtrlListSkinParams.Columns[1].Width = -1;
      }

      private void mySetParam(ListViewSubItem subItem, AppParam.Scalar appParam) =>
         mySetParamValue(subItem, appParam, (dynamic)appParam.ObjValue.NnOrCrash());

      private void mySetParamValue(ListViewSubItem subItem, AppParam.Scalar paramScalar, Font font)
      {
         var ski = PpSelectedSkin;

         if (ski != null)
         {
            subItem.BackColor = ski.Params.BackContentColor.Value;
            subItem.ForeColor = ski.Params.ForeColor.Value;
            subItem.Font = font;
            subItem.Text = PpAppParamStringConverter.ToStr(paramScalar.ObjValue);
         }
      }

      private void mySetParamValue(ListViewSubItem subItem, AppParam.Scalar paramScalar, Color color)
      {
         var ski = PpSelectedSkin;
         
         if (ski != null)
         {
            subItem.BackColor = color;
            subItem.ForeColor = InnerColorHelper.ComplementColor(color);
            subItem.Font = ski.Params.ControlsFont.Value;
            subItem.Text = PpAppParamStringConverter.ToStr(paramScalar.ObjValue);
         }
      }

      private void mySetParamValue(ListViewSubItem subItem, AppParam.Scalar appParam, object dummy) => throw new Crash();

      private string myGetNewName()
      {
         for (int idx = 1; ; idx++)
         {
            var nam = "NewSkin" + idx;

            if (PpSkinsAll.All(s => s.Name.ToLower() != nam.ToLower())) { return nam; }
         }
      }

      private bool myDoIsNameExisting(string name) =>
         Directory.EnumerateFiles(PpMainForm.PpSkin?.SkinsFolder ?? "", "*.xml").
         Select(p => Path.GetFileNameWithoutExtension(p).ToLower()).Any(f => f == name.ToLower());

      private void CtrlButtonOk_Click(object? sender, EventArgs e)
      {
         myIsAccepting = true;
         Close();
      }

      private void CtrlButtonRollBack_Click(object? sender, EventArgs e)
      {
         if (PpMainForm != null)
         {
            var is_cur_ski = PpSelectedSkin == PpMainForm.PpSkin;

            PpMainForm.PpSkin = myBackUpSkin;

            if (is_cur_ski)
            {
               var idx = 0;

               foreach (var par in PpMainForm?.PpSkin?.Params.AllDescendant.OfType<AppParam.Scalar>() ?? [])
               {
                  var lwi = CtrlListSkinParams.Items[idx++];

                  mySetParam(lwi.SubItems[1], par);
                  lwi.Tag = par;
               }
            }
         }
      }

      private void CtrlButtonDefault_Click(object? sender, EventArgs e)
      {
         if (!(PpSelectedSkin?.IsCurrent ?? false))
         {
            var cpy = PpSelectedSkin?.GetCopy();

            PpMainForm.PpSkin = cpy;
            PpMainForm.PpSkin?.Save();
            myIsDefaultChanged = true;

            foreach (var lwi in CtrlListSkins.Items.Cast<ListViewItem>())
            {
               if ((lwi.Tag as GateDockSkin)?.IsCurrent ?? false)
               {
                  lwi.Tag = PpMainForm.PpSkin;
                  CtrlListSkins.SelectedIndices.Clear();
                  CtrlListSkins.SelectedIndices.Add(lwi.Index);
                  break;
               }
            }
         }
      }

      private void CtrlListSkinProperties_DoubleClick(object? sender, MouseEventArgs e)
      {
         if (PpSelectedAppParam != null)
         {
            var is_cng = myDoEdit((dynamic)PpSelectedAppParam.ObjValue.NnOrCrash(), PpSelectedAppParam);

            if (is_cng && PpSelectedSkin?.IsCurrent) { myIsDefaultChanged = true; }
         }
      }

      private void CtrlButtonSaveAs_Click(object? sender, EventArgs e)
      {
         var new_ski = PpSelectedSkin?.GetCopy();

         if (new_ski != null)
         {
            new_ski.Name = myGetNewName();

            var new_lwi = myDoAddSkinToList(new_ski);

            new_lwi.BeginEdit();
         }
      }

      private void CtrlListSkins_AfterLabelEdit(object? sender, LabelEditEventArgs e)
      {
         var lwi = CtrlListSkins.Items[e.Item];
         var ski = lwi.Tag as GateDockSkin ?? throw new Crash();
         var nam_pro = e.Label.ExtTrim();//name proposed

         if (lwi.Text.IsBlank()) { e.CancelEdit = true; }
         else if (myDoIsNameExisting(nam_pro))
         {
            lwi.ToolTipText = $"Name {nam_pro} already existing!";
            e.CancelEdit = true;
         }
         else
         {
            ski.Name = nam_pro;

            if (!ski.Save()) { e.CancelEdit = true; }
         }
      }

      private void CtrlListSkins_SelectedIndexChanged(object? sender, EventArgs e) => myDoOnSelectionChanged();
   }
}
