using Gate.Tools;
using Gate.ToolsView.BaseControls;
using Gate.ToolsView.Extended;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Gate.ToolsViewTest
{
   internal class ExtendedListEditorTest
   {
      public class InnerTestClass : IExtendedListEditorItem
      {
         private string? myStringProperty;
         private bool myBoolProperty;
         private string? myDropDown;

         [ExtendedListEditor(ShownName = "String")]
         public string? StringProperty { get => myStringProperty; set => myStringProperty = value; }

         [ExtendedListEditor(ShownName = "Bool")]
         public bool BoolProperty { get => myBoolProperty; set => myBoolProperty = value; }

         [ExtendedListEditor(ShownName = "DropDown", AlternativeProperty = "DropDownAlternative")]
         public string? DropDown { get => myDropDown; set => myDropDown = value; }

         public string[] DropDownAlternative => new[] { "Item1", "Item2", "Item3" };

         [ExtendedListEditor(ShownName = "DirRel")]
         public RelativePath? DirRelProp { get; set; }

         public bool IsEditable => true;
      }

      [STAThread]
      static void Main()
      {
         var ctr = new ExtendedListEditor();
         var frm = new Form();

         ctr.PpItemType = typeof(InnerTestClass);
         ctr.Dock = DockStyle.Fill;

         for (int i = 1; i <= 3; i++)
         {
            var tmp = new InnerTestClass();

            tmp.StringProperty = "String" + i;
            tmp.DirRelProp = new RelativePath(RelativePath.OptionsType.dir, new DirectoryInfo(@"c:\temp"), $@"c:\temp\dir{i}");
            tmp.BoolProperty = i % 2 == 0;
            ctr.MthAddNewArrayItems(tmp);
         }

         frm.Controls.Add(ctr);
         frm.ShowDialog();

         var x = (ctr?.PpItems ?? []).Cast<InnerTestClass>().ToArray();

         foreach (var y in x)
         {
            Console.WriteLine(y.StringProperty);
            Console.WriteLine(y.BoolProperty);
            Console.WriteLine(y.DirRelProp);
            Console.WriteLine(y.DropDown);
         }
      }
   }
}
