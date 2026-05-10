using Gate.Tools.AppParams;
using Gate.ToolsView.AppParams;
using Gate.ToolsView.Extensions;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Gate.ToolsViewTest
{
   internal class AppParamRecordTest
   {

      class InnerRecord : AppParam.Record
      {
         public readonly Simple<int> Value1 = new Simple<int>();
         public readonly Simple<int> Value2 = new Simple<int>();

         public InnerRecord() : base("Record")
         {

         }
      }

      [STAThread]
      public static void Main(string[] args)
      {
         var ctr = new AppParamPageRecordControl();
         var frm = new Form();

         ctr.Dock = DockStyle.Fill;
         frm.Controls.Add(ctr);
         ctr.BackColor = Color.White;
         frm.Load += (s,e)=> {

            ctr.PpPageRecord = new InnerRecord();

            var lst = frm.MthGetNephews().ToList();

            lst.Insert(0, frm);

            foreach (var c in lst)
            {
               //Console.WriteLine($"{c.Name}.{c.GetType().Name} B:{c.Bounds}");
            }

         };
         frm.ShowDialog();
      }
   }
}
