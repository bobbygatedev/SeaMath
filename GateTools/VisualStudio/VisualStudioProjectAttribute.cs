using Gate.Tools.Extensions;
using System;
using System.Xml.Linq;

namespace Gate.Tools.VisualStudio
{
   /// <summary>
   /// 
   /// </summary>
   public class VisualStudioProjectAttribute : Attribute
   {
      public enum FileType
      {
         /// <summary>
         /// Option is placed into project file (Project_Name.user)
         /// </summary>
         project = 0 ,

         /// <summary>
         /// Option is placed into user file (Project_Name.vcxproj.user)
         /// </summary>
         user = 1,
      }

      public string? ElementProperty { get; set; }

      public string? ElementContainer { get; set; }

      public string? Label { get; set; } = null;
      
      public FileType File { get; set; } = FileType.project;   

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="isToCreate"></param>
      /// <returns></returns>
      private XElement? myGetSubelement(XElement? element, bool isToCreate)
      {
         if (element?.Name.LocalName == ElementContainer && element?.GetAttributeVal("Label", false) == Label)
         {
            //element property parts walks xml sub-node by sub-node 'Node.SubNode'
            var ele_pro_prs = ElementProperty?.Split('.')??[];
            var ele = element;

            foreach (var par in ele_pro_prs)
            {
               var sub_ele = ele?.ElementNs(par);

               if (sub_ele == null)
               {
                  if (!isToCreate)
                  {
                     return null;
                  }
                  else
                  {
                     ele = ele?.AddElementNs(par);
                  }
               }
               else
               {
                  ele = sub_ele;
               }
            }

            return ele;
         }
         else { return null; }
      }

      public void SetElementValue(XElement? element, string? value)
      {
         var sub_ele = myGetSubelement(element, true);

         if (sub_ele != null)
         {
            if (value.IsBlank())
            {
               sub_ele.Remove();
            }
            else
            {
               sub_ele.Value = value.ExtTrim();
            }
         }
      }

      public string? GetElementValue(XElement element) => myGetSubelement(element, false)?.Value;
   }
}
