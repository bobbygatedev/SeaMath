using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Xml.Linq;
using static Gate.Tools.AppParams.AppParam;

namespace Gate.Tools.AppParams
{
   public abstract partial class AppParamLoadSaver
   {
      /// <summary>
      /// 
      /// </summary>
      public class ByXDoc : AppParamLoadSaver
      {
         public ByXDoc() { }

         private static class InnerLoadHelper
         {
            public static bool LoadNotScalarParam(
               NotScalar notScalarParam,
               XElement element,
               TxtStringConverter stringConverter,
               MsgCollection msgs) =>
               myLoadNotScalar((dynamic)notScalarParam, element, stringConverter, msgs);

            /// <summary>
            /// <br> Record-like load check param sub-params sequentially:</br>
            /// <br> - if any required scalar sub_param is absent sub_param.Clear() is invoked.</br>
            /// <br> - for each sub-param retrieves corresponding xml sub-node and inflate it  </br>
            /// </summary>
            /// <param name="appParam"></param>
            /// <param name="node"></param>
            /// <param name="stringConverter"></param>
            /// <param name="msgs"></param>
            /// <returns></returns>
            private static bool myLoadParam2Param(
               AppParam appParam,
               XElement node,
               TxtStringConverter stringConverter,
               MsgCollection msgs)
            {
               var res = true;
               var sub_prs = appParam.SubItems.Cast<AppParam>().ToArray();
               var req_sca_prs = sub_prs.OfType<Scalar>().Where(s => s.IsRecordRequired).ToArray();
               var els = node.Elements().ToArray();

               foreach (var sub_par in sub_prs)
               {
                  if (sub_par is Scalar sca_par)
                  {
                     if (!myLoadScalar(sca_par, node, stringConverter, msgs))
                     {
                        appParam.Clear();//error clear all

                        res = false;
                     }
                  }
                  else if (sub_par is ArryRaw arr_sub)
                  {
                     var arr_els = els.Where(n => n.Name.LocalName == sub_par.ParamName).ToArray();

                     arr_sub.Clear();

                     foreach (var ele in arr_els)
                     {
                        var itm = arr_sub.MakeItem();

                        arr_sub.AddParamRaw(itm);

                        if (arr_sub.IsScalarArray)
                        {
                           if (!myLoadScalar(itm as Scalar ?? throw new Crash(), ele, stringConverter, msgs, "value"))
                           {
                              res = false;
                           }
                        }
                        else
                        {
                           if (!LoadNotScalarParam(
                              itm as NotScalar ?? throw new Crash(), ele, stringConverter, msgs))
                           {
                              res = false;
                           }
                        }
                     }
                  }
                  else
                  {
                     var par_ele = els.FirstOrDefault(n => n.Name.LocalName == sub_par.ParamName);

                     if (par_ele != null)
                     {
                        if (!LoadNotScalarParam((NotScalar)sub_par, par_ele, stringConverter, msgs))
                        {
                           appParam.Clear();//error clear all

                           res = false;
                        }
                     }
                  }
               }

               return res;
            }

            private static bool myLoadNotScalar(
               Record recordParam,
               XElement element,
               TxtStringConverter stringConverter,
               MsgCollection msgs)
            {
               var sub_prs = recordParam.SubParams;
               var req_sca_prs = sub_prs.OfType<Scalar>().Where(s => s.IsRecordRequired).ToArray();
               var ats = element.Attributes().ToArray();

               //check if any param is required and not is defined
               if (req_sca_prs.Any(s => !ats.Any(a => a.Name.LocalName == s.ParamName)))
               {
                  recordParam.Clear();//put record to its default value

                  var mis_req =
                     req_sca_prs.
                     Where(p => !ats.Any(n => n.Name.LocalName == p.ParamName)).
                     Select(p => p.ParamName).ToArray();

                  msgs.Add(new Msg(
                     MsgType.error,
                     $"Node {element.Name.LocalName} no have required scalar ({string.Join(",", mis_req)})"));

                  return false;
               }
               else
               {
                  return myLoadParam2Param(recordParam, element, stringConverter, msgs);
               }
            }

            private static bool myLoadNotScalar(
               ArryRaw arrayParam,
               XElement element,
               TxtStringConverter stringConverter,
               MsgCollection msgs)
            {
               var els = element.Elements().ToArray();
               var lst_fld = new List<AppParam>();

               if (arrayParam.ItemFieldType.IsSubclassOf(typeof(Scalar)))
               {
                  arrayParam.Clear();

                  var sca_par_cns = arrayParam.ItemFieldType.GetConstructor([]);

                  if (sca_par_cns == null) { throw new Crash($"{arrayParam.GetType()} shall have a default constructor"); }
                  else
                  {
                     var ats = element.Attributes().ToArray();
                     var prs = ats.Select(a =>
                     {
                        var sca_par = (Scalar)sca_par_cns.Invoke([]);

                        if (stringConverter.TryParse(a.Value, sca_par.ParamType, out var res, msgs))
                        {
                           sca_par.ObjValue = res;
                        }
                        else
                        {
                           sca_par.ObjValue = null;
                        }

                        return sca_par;
                     }).ToArray();

                     if (prs.All(v => v != null))
                     {
                        arrayParam.AddParamsRaw(prs);

                        return true;
                     }
                     else { return false; }
                  }
               }
               else
               {
                  foreach (var ele in els)
                  {
                     var par = arrayParam.MakeItem();

                     lst_fld.Add(par);

                     if (!LoadNotScalarParam((NotScalar)par, ele, stringConverter, msgs)) { return false; }
                  }
               }

               foreach (var fld in lst_fld) { arrayParam.AddParamRaw(fld); }

               return true;
            }

            /// <summary>
            /// Sets a scalar param inside a record/union if its found inside xml node
            /// </summary>
            /// <param name="scalarParam"></param>
            /// <param name="element"></param>
            /// <param name="stringConverter"></param>
            /// <param name="msgs"></param>
            /// <returns>true if value is correctly parser from repository or not is required</returns>
            private static bool myLoadScalar(
               Scalar scalarParam,
               XElement element,
               TxtStringConverter stringConverter,
               MsgCollection msgs,
               string? attributeName = null)
            {
               var atr_nam = attributeName ?? scalarParam.ParamName;
               var atr_par = element.GetAttributeVal(atr_nam.ExtTrim(), false);
               var res = !scalarParam.IsRecordRequired;

               //if scalar param of record/union is not required true is always returned
               if (atr_par != null)
               {
                  if (res = stringConverter.TryParse(atr_par, scalarParam.ParamType, out var obj, msgs))
                  {
                     scalarParam.ObjValue = obj;
                  }
                  else
                  {
                     msgs?.Add(new Msg(MsgType.error, $"During parsing of {scalarParam.ParamPath}"));
                     scalarParam.Clear();
                  }
               }

               return res;
            }

         }

         private static class InnerSaveHelper
         {
            /// <summary>
            /// Saves <see cref="Scalar"/> <paramref name="scalarParam"/> onto <paramref name="element"/>
            /// </summary>
            /// <param name="stringConverter"></param>
            /// <param name="element"></param>
            /// <param name="scalarParam"></param>
            /// <param name="paramName"></param>
            private static void mySaveScalar(
               TxtStringConverter stringConverter,
               XContainer element,
               Scalar scalarParam,
               string? paramName = null)
            {
               var str_val = scalarParam.ObjValue != null ? stringConverter.ToStr(scalarParam.ObjValue) : null;
               var par_nam = paramName ?? scalarParam.ParamName;

               if (!str_val.IsBlank() && str_val != null && par_nam != null)
               {
                  element.Add(new XAttribute(par_nam, str_val));
               }
            }

            /// <summary>
            /// Visitor for <see cref="SaveNotScalar(NotScalar, XContainer, TxtStringConverter)"/> for <see cref="NotScalar"/> of none of valid types (<see cref="Variant"/>,<see cref="ArryRaw"/>,<see cref="Record"/>,<see cref="Union"/>). Causes a Crash
            /// </summary>
            /// <param name="notScalar"></param>
            /// <param name="element"></param>
            /// <param name="stringConverter"></param>
            /// <exception cref="Crash"></exception>
            private static void mySaveNotScalarVisitor(NotScalar notScalar, XContainer element, TxtStringConverter stringConverter) => throw new Crash();

            /// <summary>
            /// Visitor for <see cref="SaveNotScalar(NotScalar, XContainer, TxtStringConverter)"/> for <see cref="ArryRaw"/> not scalar app-param.
            /// </summary>
            /// <param name="array"></param>
            /// <param name="element"></param>
            /// <param name="stringConverter"></param>
            /// <exception cref="Crash"></exception>
            private static void mySaveNotScalarVisitor(ArryRaw array, XContainer element, TxtStringConverter stringConverter)
            {
               if (array.IsScalarArray)
               {
                  for (int i = 0; i < array.ItemCount; i++)
                  {
                     var sc2 = array.Items[i] as Scalar ?? throw new Crash();
                     var sub_ele = new XElement(array.ParamName.ExtTrim());

                     element.Add(sub_ele);

                     mySaveScalar(stringConverter, sub_ele, sc2, "value");
                  }
               }
               else
               {
                  for (var i = 0; i < array.ItemCount; i++)
                  {
                     var ns2 = array.Items[i] as NotScalar ?? throw new Crash();

                     SaveNotScalar(ns2, element, stringConverter);
                  }
               }
            }

            /// <summary>
            /// Visitor for <see cref="SaveNotScalar(NotScalar, XContainer, TxtStringConverter)"/> for <see cref="Articulated"/> not scalar app-param.
            /// </summary>
            /// <param name="record"></param>
            /// <param name="element"></param>
            /// <param name="stringConverter"></param>
            private static void mySaveNotScalarVisitor(Record record, XContainer element, TxtStringConverter stringConverter)
            {
               /// in case of parameter  <see cref="Arry{P}"/> parameter name of record/union corresponds to 
               /// array param name 
               var par_nam = record.ParentItem is ArryRaw ? record.ParentParam?.ParamName : record.ParamName;
               var art_ele = new XElement(par_nam ?? throw new Gate.Tools.ToolsException("Param Name not found"));

               element.Add(art_ele);

               foreach (var sub_par in record.SubParams)
               {
                  mySave(stringConverter, art_ele, sub_par);
               }
            }

            /// <summary>
            /// Save a generic a scalar/not_scalar <see cref="AppParam"/> onto an <see cref="XElement"/>.
            /// </summary>
            /// <param name="stringConverter"></param>
            /// <param name="element"><see cref="XElement"/> where save data to</param>
            /// <param name="appParam"><see cref="AppParam"/> where data to save are located.</param>
            /// <exception cref="Crash"></exception>
            private static void mySave(TxtStringConverter stringConverter, XElement element, AppParam appParam)
            {
               switch (appParam)
               {
                  case Scalar sca:
                     mySaveScalar(stringConverter, element, sca);
                     break;

                  case NotScalar ns:
                     SaveNotScalar(ns, element, stringConverter);
                     break;

                  default: throw new Crash($"Type of {appParam.ParamName}({appParam.GetType().Name}) not valid!");
               }
            }

            /// <summary>
            /// Saves a <see cref="NotScalar"/> <see cref="AppParam"/> onto <paramref name="element"/>
            /// </summary>
            /// <param name="notScalar"></param>
            /// <param name="element"></param>
            /// <param name="stringConverter"></param>
            public static void SaveNotScalar(NotScalar notScalar, XContainer element, TxtStringConverter stringConverter) =>
               mySaveNotScalarVisitor((dynamic)notScalar, element, stringConverter);
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="appParamContainer"></param>
         /// <param name="stringConverter"></param>
         /// <param name="path"></param>
         /// <returns>True if successfully False if any error.</returns>
         public override bool Load(AppParamContainer appParamContainer, TxtStringConverter stringConverter, string path)
         {
            if (File.Exists(path))
            {
               try
               {
                  var xml_txt = myGetXmlText(path);
                  var doc = XDocument.Parse(xml_txt);

                  appParamContainer.ClearContent();

                  var roo = appParamContainer.Params;
                  var roo_ele = doc.SelectElement(roo.ParamPath.ExtTrim());

                  if (roo_ele != null)
                  {
                     return InnerLoadHelper.LoadNotScalarParam(roo, roo_ele, stringConverter, appParamContainer.InternalMessages);
                  }
                  else
                  {
                     appParamContainer.InternalMessages.Add(
                        new Msg(MsgType.error, $"{path} should have {roo.ParamPath} as root xml node!", null, null));
                  }
               }
               catch (Exception exc)
               {
                  appParamContainer.InternalMessages.Add(new Msg(MsgType.fail, $"Failed to load {path}", null, null));
                  appParamContainer.InternalMessages.Add(new Msg(MsgType.info, $"Reason: {exc.Message}", null, null));
               }
            }
            else
            {
               appParamContainer.InternalMessages.Add(new Msg(MsgType.error, $"{path} does not exist.", null, null));
            }

            return false;
         }

         private static string myGetXmlText(string path)
         {
            var res = "";
            var act = new Action(() => res = File.ReadAllText(path));

            act.IoActionTimeout(3.0);

            return res;
         }

         public override bool Save(AppParamContainer appParamContainer, TxtStringConverter stringConverter, string path)
         {
            try
            {
               var doc = new XDocument();
               var not_sca_prs = appParamContainer.Params.AllParams.OfType<NotScalar>().ToArray();

               var roo_rec = appParamContainer.Params;

               InnerSaveHelper.SaveNotScalar(roo_rec, doc, stringConverter);

               //not scalar params
               var dir = Directory.GetParent(path)?.FullName;

               if (dir != null && !Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

               lock (this)
               {
                  for (var i = 0; i < 3; i++)
                  {
                     try
                     {
                        doc.Save(path);
                        break;
                     }
                     catch { Thread.Sleep(50); }
                  }
               }

               return true;
            }
            catch (Exception exc)
            {
               appParamContainer.InternalMessages.Add(new Msg(
                  MsgType.error, $"Failed to save '{appParamContainer.FilePath}' reason {exc.Message}"));
            }

            return false;
         }
      }
   }

}
