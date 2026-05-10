using Gate.Tools.Text;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Gate.Tools.Extensions
{
   /// <summary>
   /// 
   /// </summary>
   public static class XmlExtender
   {
      /// <summary>
      /// Retrieves sub-element array of a xml node by name when xml has a namespace. 
      /// </summary>
      /// <param name="element">(Parent) xml element.</param>
      /// <param name="nameNoNamespace"></param>
      /// <param name="nameSpace">Name of namespace or null(in this case default namespace is used)</param>
      /// <returns></returns>
      public static XElement[] ElementsNs(this XElement element, string nameNoNamespace, string? nameSpace = null)
      {
         var nsp = (nameSpace != null) ? element.GetNamespaceOfPrefix(nameSpace) : element.Document?.Root?.Name.Namespace;

         return element.Elements((nsp ?? throw new Crash()) + nameNoNamespace).ToArray();
      }

      /// <summary>
      /// Retrieves single sub-element array of a xml node by name when xml has a namespace. 
      /// </summary>
      /// <param name="element">(Parent) xml element.</param>
      /// <param name="nameNoNamespace">Name to be searched without namespace (eg 'xnsp:name' shall be passed as 'name').</param>
      /// <param name="nameSpace">Name of namespace or null(in this case default namespace is used)</param>
      /// <returns></returns>
      public static XElement? ElementNs(this XElement element, string nameNoNamespace, string? nameSpace = null)
      {
         var nsp = (nameSpace != null) ? element.GetNamespaceOfPrefix(nameSpace) : element.Document?.Root?.Name.Namespace;

         return element.Element((nsp ?? throw new Crash()) + nameNoNamespace);
      }

      /// <summary>
      /// Saves xml using specific <paramref name="newLine"/>.
      /// </summary>
      /// <param name="xDocument"></param>
      /// <param name="fileInfo"></param>
      /// <param name="newLine"></param>
      public static void SaveNl(this XDocument xDocument, FileInfo fileInfo, string newLine)
      {
         var sto = new TxtStore(xDocument.ToString());

         sto.Settings.NewLine = newLine;
         sto.Save(fileInfo.FullName);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="node"></param>
      /// <param name="name"></param>
      /// <param name="isRequired"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public static string? GetAttributeVal(this XElement node, string name, bool isRequired)
      {
         var atr = node.Attribute(name);

         if (atr != null) { return atr.Value; }
         else if (!isRequired) { return null; }
         else { throw new Gate.Tools.ToolsException($"Attribute {name} not found!"); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="name"></param>
      public static void SetElementName(this XElement element, string name) => element.Name = XName.Get(name);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="name"></param>
      /// <param name="nameSpace"></param>
      public static void SetElementNameNs(this XElement element, string name, string? nameSpace = null) =>
         element.Name = (GetNamespace(element, nameSpace) ?? throw new Crash()) + name;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="nameSpace"></param>
      /// <returns></returns>
      public static XNamespace? GetNamespace(this XElement element, string? nameSpace = null) =>
         (nameSpace != null) ? element.GetNamespaceOfPrefix(nameSpace) : element.Document?.Root?.Name.Namespace;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="name"></param>
      /// <param name="attributes"></param>
      /// <returns></returns>
      public static XElement AddElement(this XElement element, string name, params (string name, string value)[] attributes)
      {
         var nod = new XElement(name);

         foreach (var at in attributes)
         {
            nod.Add(new XAttribute(at.name, at.value));
         }

         element.Add(nod);

         return nod;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="document"></param>
      /// <param name="name"></param>
      /// <param name="nameSpaceName"></param>
      /// <returns></returns>
      public static XElement GetNewElementsNs(this XDocument? document, string name, string? nameSpaceName = null)
      {
         var nsp = nameSpaceName == null ?
            document?.Root?.Name.Namespace :
            document?.Root?.GetNamespaceOfPrefix(nameSpaceName);

         return new XElement((nsp ?? throw new Crash()) + name);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="parent"></param>
      /// <param name="name"></param>
      /// <param name="nameSpaceName"></param>
      /// <param name="attributes"></param>
      /// <returns></returns>
      public static XElement AddElementNs(
         this XElement parent, string name, string nameSpaceName, params (string name, string value)[] attributes)
      {
         var ele = GetNewElementsNs(parent.Document, name, nameSpaceName);

         ele.AddAttributes(attributes);
         parent.Add(ele);

         return ele;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="parent"></param>
      /// <param name="name"></param>
      /// <param name="attributes"></param>
      /// <returns></returns>
      public static XElement AddElementNs(this XElement parent, string name, params (string name, string value)[] attributes)
      {
         var ele = GetNewElementsNs(parent.Document, name);

         foreach (var at in attributes)
         {
            ele.Add(new XAttribute(at.name, at.value));
         }

         parent.Add(ele);

         return ele;
      }

      public static XElement InsertAfterElementsNs(this XElement parent, string name, params (string name, string value)[] attributes) =>
         InsertAfterElementsNs(parent, name, null, attributes);

      public static XElement InsertBeforeElementsNs(this XElement parent, string name, params (string name, string value)[] attributes) =>
         InsertBeforeElementsNs(parent, name, null, attributes);

      public static XElement InsertAfterElementsNs(
         this XElement parent, string name, string? nameSpaceName, params (string name, string value)[] attributes)
      {
         var ele = GetNewElementsNs(parent.Document, name, nameSpaceName);

         foreach (var at in attributes)
         {
            ele.Add(new XAttribute(at.name, at.value));
         }

         parent.AddAfterSelf(ele);

         return ele;
      }

      public static XElement InsertBeforeElementsNs(
         this XElement parent, string name, string? nameSpaceName, params (string name, string value)[] attributes)
      {
         var ele = GetNewElementsNs(parent.Document, name, nameSpaceName);

         foreach (var at in attributes)
         {
            ele.Add(new XAttribute(at.name, at.value));
         }

         parent.AddBeforeSelf(ele);

         return ele;
      }


      public static XElement InsertAfterElements(this XElement parent, string name, params (string name, string value)[] attributes)
      {
         var ele = new XElement(name);

         foreach (var at in attributes)
         {
            ele.Add(new XAttribute(at.name, at.value));
         }

         parent.AddAfterSelf(ele);

         return ele;
      }

      public static XElement InsertBeforeElements(this XElement parent, string name, params (string name, string value)[] attributes)
      {
         var ele = new XElement(name);

         foreach (var at in attributes)
         {
            ele.Add(new XAttribute(at.name, at.value));
         }

         parent.AddBeforeSelf(ele);

         return ele;
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="attributes"></param>
      public static void AddAttributes(this XElement element, params (string name, string value)[] attributes)
      {
         foreach (var at in attributes.Where(a => a.value != null))
         {
            element.Add(new XAttribute(at.name, at.value));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="namespaceName"></param>
      /// <param name="attributes"></param>
      public static void AddAttributesNs(this XElement element, string? namespaceName = null, params (string name, string value)[] attributes)
      {
         var nsp = element.GetNamespace(namespaceName);

         foreach (var at in attributes.Where(a => a.value != null))
         {
            element.Add(new XAttribute((nsp ?? throw new Crash()) + at.name, at.value));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="attributeName"></param>
      /// <param name="attributeValue"></param>
      /// <param name="namespaceName"></param>
      public static void SetAttributeVal(this XElement element, string attributeName, string attributeValue, string? namespaceName = null)
      {
         var atr = element.Attributes().FirstOrDefault(a => a.Name.LocalName == attributeName);

         if (atr == null)
         {
            if (namespaceName.IsBlank())
            {
               element.AddAttributes((attributeName, attributeValue));
            }
            else
            {
               element.AddAttributesNs(namespaceName, (attributeName, attributeValue));
            }
         }
         else
         {
            atr.Value = attributeValue;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="document"></param>
      /// <param name="encoding"></param>
      /// <returns></returns>
      public static string GetIdentedText(this XDocument document, Encoding? encoding = null)
      {
         using (var ms = new MemoryStream())
         {
            using (var xml_wr = new XmlTextWriter(ms, encoding ?? Encoding.UTF8))
            {
               xml_wr.Formatting = Formatting.Indented;

               // Write the XML into a formatting XmlTextWriter
               document.WriteTo(xml_wr);
               xml_wr.Flush();
               ms.Flush();

               // Have to rewind the MemoryStream in order to read
               // its contents.
               ms.Position = 0;

               // Read MemoryStream contents into a StreamReader.
               using (var sr = new StreamReader(ms)) { return sr.ReadToEnd().Replace("xmlns=\"\"", ""); }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="doc"></param>
      /// <param name="path"></param>
      /// <param name="pathSeparator"></param>
      /// <returns></returns>
      public static XElement[] SelectElements(this XDocument doc, string path, char pathSeparator = '/') =>
         SelectElements(doc.Root ?? throw new Crash(), path, pathSeparator);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="nameExt"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public static XName GetXName(this XElement element, string nameExt)
      {
         var sps = nameExt.Trim().Split(':');

#pragma warning disable CS8604 // Possible null reference argument.
         switch (sps.Length)
         {
            case 1: return XName.Get(sps[0]);

            case 2: return sps[0] == "" ? element.GetNamespace() + sps[1] : element.GetNamespace(sps[0]) + sps[1];
#pragma warning restore CS8604 // Possible null reference argument.

            default: throw new Gate.Tools.ToolsException($"{nameExt} is not valid as xml extended name (eg 'name' ':name' 'nsp:name' are valid )");

         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="path"></param>
      /// <param name="pathSeparator"></param>
      /// <returns></returns>
      public static XElement[] SelectElements(this XElement element, string path, char pathSeparator = '/')
      {
         var spl = path.ExtTrim().
            Split(pathSeparator).
            Select(ps => ps.Trim()).
            Where(ps => !ps.IsBlank()).ToArray();
         var ele = element;
         var res = new[] { ele };

         foreach (var itm in spl)
         {
            var xn = element.GetXName(itm);
            var new_res = res.SelectMany(e => e.Elements(xn)).ToArray();

            if (new_res.Length == 0)
            {
               return new_res;
            }
            else
            {
               res = new_res;
            }
         }

         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="element"></param>
      /// <param name="path"></param>
      /// <param name="pathSeparator"></param>
      /// <returns></returns>
      public static XElement? SelectElement(this XElement element, string path, char pathSeparator = '/')
      {
         var spl = path.ExtTrim().
            Split(pathSeparator).
            Select(ps => ps.Trim()).
            Where(ps => !ps.IsBlank()).ToArray();

         if (spl.Length != 0)
         {
            var res = element;

            foreach (var itm in spl)
            {
               var xn = element.GetXName(itm);
               var new_res = res.Element(xn);

               if (new_res == null) { return null; }
               else { res = new_res; }
            }

            return res;
         }
         else { return element; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="doc"></param>
      /// <param name="path"></param>
      /// <param name="pathSeparator"></param>
      /// <returns></returns>
      public static XElement? SelectElement(this XDocument doc, string path, char pathSeparator = '/')
      {
         if (doc.Root == null) { return null; }
         else
         {
            var pts = path.ExtTrim().Split(pathSeparator).Select(ps => ps.Trim()).ToArray();

            if (pts.Length == 0) { return null; }
            else
            {
               var xn = GetXName(doc.Root, pts[0]);

               if (doc.Root.Name.Equals(xn))
               {
                  var pt1 = string.Join($"{pathSeparator}", pts.Skip(1));

                  return SelectElement(doc.Root, pt1, pathSeparator);
               }
               else
               {
                  return null;
               }
            }
         }
      }

      /// <summary>
      /// Equivalent of <see cref="SelectElement(XDocument, string, string)"/> if <paramref name="path"/> already exists.
      /// <br>otw any item in <paramref name="path"/> is created when necessary </br>
      /// </summary>
      /// <param name="document"></param>
      /// <param name="path"></param>
      /// <param name="pathSeparator"></param>
      /// <returns></returns>
      public static XElement? MakeElement(this XDocument document, string path, char pathSeparator = '/')
      {
         var pts = path.ExtTrim().
            Split(pathSeparator).
            Select(ps => ps.Trim()).
            Where(ps => !ps.IsBlank()).ToArray();

         if (pts.Length == 0) { return null; }

         if (document.Root == null)
         {
            var p0 = pts[0].Trim();
            var spl = p0.Split(':');

            if (spl.Length > 1)
            {
               throw new Gate.Tools.ToolsException("Not namespace can be indicated for root");
            }
            else
            {
               document.Add(new XElement(p0));

               return document.Root;
            }
         }
         else
         {
            return document.Root.MakeElement(path, pathSeparator);
         }
      }

      /// <summary>
      /// Equivalent of <see cref="SelectElement(XDocument, string, string)"/> if <paramref name="path"/> already exists.
      /// <br>otw any item in <paramref name="path"/> is created when necessary </br>
      /// </summary>
      /// <param name="document"></param>
      /// <param name="path"></param>
      /// <param name="pathSeparator"></param>
      /// <returns></returns>
      public static XElement MakeElement(this XElement element, string path, char pathSeparator = '/')
      {
         var pts = path.ExtTrim().Split(pathSeparator);
         var ele = element;

         foreach (var pth in pts.Select(p => p.Trim()))
         {
            var sub = ele.SelectElement(pth);

            if (sub == null)
            {
               var nam = ele.GetXName(pth);

               ele.Add(sub = new XElement(nam));
            }

            ele = sub;
         }

         return ele;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="document"></param>
      /// <param name="isDefault"></param>
      /// <param name="namespacePrefix"></param>
      /// <param name="namespaceUrl"></param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public static void AddNamespace(
         this XDocument document,
         bool isDefault = true,
         string? namespacePrefix = null,
         string namespaceUrl = "https://gatelib.com")
      {
         var roo = document.Root;

         if (document.Root == null)
         {
            throw new Gate.Tools.ToolsException("document shall have a root");
         }

         XNamespace nsp = namespaceUrl;

         if (namespacePrefix != null)
         {
            document.Root.Add(new XAttribute(XNamespace.Xmlns + namespacePrefix, nsp.NamespaceName));
         }
         else
         {
            document.Root.Add(new XAttribute("xmlns", nsp.NamespaceName));
         }

         if (isDefault)
         {
            document.Root.Name = nsp + document.Root.Name.LocalName;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="document"></param>
      /// <param name="xmlPath"></param>
      /// <param name="encoding"></param>
      /// <param name="isXmlDeclToOmit"></param>
      public static void SaveWithEncoding(
         this XDocument document, string xmlPath, Encoding encoding, bool isXmlDeclToOmit = false)
      {
         var xse = new XmlWriterSettings();

         xse.Encoding = encoding;
         xse.Indent = true;
         xse.OmitXmlDeclaration = isXmlDeclToOmit;
         xse.ConformanceLevel = ConformanceLevel.Document;

         new FileInfo(xmlPath).Directory?.Create();

         using (var xml_wri = XmlWriter.Create(xmlPath, xse))
         {
            document.Save(xml_wri);
         }
      }

      /// <summary>
      /// Retrieves child elements, child of childs and so on. 
      /// </summary>
      /// <param name="root"></param>
      /// <returns></returns>
      public static XElement[] GetAllSubElementsRecursively(this XElement root) =>
         new[] { root }.Concat(root.Elements().SelectMany(GetAllSubElementsRecursively)).ToArray();
   }
}
