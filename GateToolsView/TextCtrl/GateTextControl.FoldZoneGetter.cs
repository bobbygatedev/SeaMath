using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.ToolsView.TextCtrl
{
   public partial class GateTextControl
   {
      public abstract class FoldZoneGetter
      {
         public class ForCpp : FoldZoneGetter
         {
            protected unsafe override FoldZone myGetRootFoldZone(string fileBody)
            {
               var len = fileBody.Length;
               var cpp_roo = new FoldZone(0, fileBody, FoldZoneTypeEnum.text_root);
               var cpp_cur = cpp_roo;
               var is_str = false;

               fixed (char* fil_p = fileBody)
               {
                  for (var i = 0; i < len; i++)
                  {
                     if (is_str)
                     {
                        is_str = fil_p[i] != '"' || fil_p[i - 1] == '\\';
                        continue;
                     }

                     switch (fil_p[i])
                     {
                        case '\n':
                           if (cpp_cur.Type == FoldZoneTypeEnum.comment_single_line)
                           {
                              // cpp-style
                              var is_fol = false;

                              for (var j = i; j < len; j++)
                              {
                                 if (fil_p[j] == '/' && j < len - 1 && fil_p[j + 1] == '/')
                                 {
                                    i = j + 1;
                                    is_fol = true;//follows (cpp comment follows in next lines)
                                    break;
                                 }
                                 else if (!char.IsWhiteSpace(fil_p[j])) { break; }
                              }

                              if (is_fol) { break; }
                              else
                              {
                                 cpp_cur.To = i;
                                 cpp_cur = cpp_cur.ParentZone;
                              }
                           }

                           break;

                        case '/':
                           if ((
                              cpp_cur.Type == FoldZoneTypeEnum.text_root || cpp_cur.Type == FoldZoneTypeEnum.brace)
                              && i < len - 1)
                           {
                              if (fil_p[i + 1] == '/')
                              {
                                 cpp_cur.AddSubZone(cpp_cur = new FoldZone(i, fileBody, FoldZoneTypeEnum.comment_single_line));
                                 i++;
                              }
                              else if (fil_p[i + 1] == '*')
                              {
                                 cpp_cur.AddSubZone(cpp_cur = new FoldZone(i, fileBody, FoldZoneTypeEnum.comment_multi_line));
                                 i++;
                              }
                           }
                           else if (cpp_cur.Type == FoldZoneTypeEnum.comment_multi_line && fil_p[i - 1] == '*')
                           {
                              cpp_cur.To = i;
                              cpp_cur = cpp_cur.ParentZone;
                           }
                           break;

                        case '"':
                           is_str = cpp_cur.Type == FoldZoneTypeEnum.text_root;
                           break;

                        case '{':
                           if (cpp_cur.Type == FoldZoneTypeEnum.text_root || cpp_cur.Type == FoldZoneTypeEnum.brace)
                           {
                              cpp_cur.AddSubZone(cpp_cur = new FoldZone(i, fileBody, FoldZoneTypeEnum.brace));
                           }
                           break;

                        case '}':
                           if (cpp_cur.Type == FoldZoneTypeEnum.brace)
                           {
                              cpp_cur.To = i;
                              cpp_cur = cpp_cur.ParentZone;
                           }
                           else if (cpp_cur.Type == FoldZoneTypeEnum.text_root) { i = len - 1; }//error close elaboration
                           break;
                     }
                  }

                  //var b_txt = fileBody.ToCharArray();

                  var cmm_zns = cpp_roo.AllZones.Where(z => z.Type == FoldZoneTypeEnum.comment_single_line || z.Type == FoldZoneTypeEnum.comment_multi_line).ToArray();

                  //check brace section --> to class/function
                  foreach (var zon in cpp_cur.AllZones.Where(z => z.Type == FoldZoneTypeEnum.brace) ?? [])
                  {
                     var itn = new Interval(zon.ParentZone.From, zon.From - 1);

                     var cmm_arr = itn.Range.Select(i => cmm_zns.Any(z => z.Interval.Contains(i))).ToArray();

                     for (var i = zon.From - 1; i >= zon.ParentZone.From; i--)
                     {
                        var is_cmm = cmm_arr[i - itn.From];

                        if (!is_cmm && fil_p[i] == ')')
                        {
                           zon.Type = FoldZoneTypeEnum.function;
                        }
                        else if (!is_cmm && !char.IsWhiteSpace(fil_p[i]))
                        {
                           break;
                        }
                     }
                  }

                  return cpp_roo;
               }

               //var b_txt = fileBody.ToCharArray();

               //var cmm_zns = cpp_roo.AllZones.Where(z=>z.Type == FoldZoneTypeEnum.comment_single_line || z.Type == FoldZoneTypeEnum.comment_multi_line).ToArray();

               ////blanking comment
               //foreach (var zon in
               //   cpp_roo.AllZones.Where(z =>
               //      z.Type == FoldZoneTypeEnum.comment_multi_line || z.Type == FoldZoneTypeEnum.comment_single_line))
               //{
               //   for (var i = zon.From; i <= zon.To; i++) { b_txt[i] = ' '; }
               //}

               ////check brace section --> to class/function
               //foreach (var zon in cpp_cur.AllZones.Where(z => z.Type == FoldZoneTypeEnum.brace))
               //{
               //   for (var i = zon.From - 1; i >= zon.ParentZone.From; i--)
               //   {
               //      if (b_txt[i] == ')' { zon.Type = FoldZoneTypeEnum.function; }
               //      else if (!char.IsWhiteSpace(b_txt[i])) { break; }
               //   }
               //}

               //return cpp_roo;
            }
         }

         public class ForXml : FoldZoneGetter
         {
            private unsafe bool myMoveToNoSpace(char* ptr, ref int pos, int len)
            {
               pos++;

               for (; pos < len && char.IsWhiteSpace(ptr[pos]); pos++) ;

               return pos < len;
            }

            private enum Phase
            {
               none = 0,

               /// <summary>
               /// eg &lt;tag&gt;value&lt;/tag&gt;
               /// </summary>
               tag,

               /// <summary>
               /// Inside body of ending tag (ie &lt;/tag&gt;).
               /// </summary>
               tag_ending,

               /// <summary>
               /// Inside a tag.
               /// </summary>
               value,
               header,
            }

            protected unsafe override FoldZone myGetRootFoldZone(string fileBody)
            {
               var len = fileBody.Length;
               var xml_roo = new FoldZone(0, fileBody, FoldZoneTypeEnum.text_root);
               var xml_cur = xml_roo;
               var pha = Phase.none;

               fixed (char* fil_p = fileBody)
               {
                  for (var i = 0; i < len; i++)
                  {
                     switch (fil_p[i])
                     {
                        case '"':
                           var is_ok = false;

                           for (i++; i < len; i++)
                           {
                              if (fil_p[i] == '"')
                              {
                                 is_ok = true;
                                 break;
                              }
                           }

                           if (!is_ok)
                           {
                              i = len - 1;//cause exit
                              continue;
                           }
                           break;

                        case '<':
                           switch (pha)
                           {
                              case Phase.none:
                              case Phase.value:
                                 break;
                              default:
                                 i = len - 1;//cause exit
                                 continue;
                           }

                           if (!myMoveToNoSpace(fil_p, ref i, len))
                           {
                              i = len - 1;//cause exit
                              continue;
                           }

                           switch (fil_p[i])
                           {
                              case '?':
                                 pha = Phase.header;
                                 break;

                              case '/':
                                 if (xml_cur.Type != FoldZoneTypeEnum.brace)
                                 {
                                    i = len - 1;//cause exit
                                    continue;
                                 }
                                 else
                                 {
                                    pha = Phase.tag_ending;
                                    break;
                                 }

                              default:
                                 pha = Phase.tag;
                                 break;
                           }

                           break;

                        case '/':
                           if (pha == Phase.tag)
                           {
                              if (myMoveToNoSpace(fil_p, ref i, len) && fil_p[i] == '>')
                              {
                                 pha = Phase.none;
                                 //no value tag
                                 continue;
                              }
                              else
                              {
                                 i = len - 1;//cause exit
                                 continue;
                              }
                           }

                           break;

                        case '>':
                           switch (pha)
                           {
                              case Phase.tag:
                                 xml_cur.AddSubZone(xml_cur = new FoldZone(i, fileBody, FoldZoneTypeEnum.brace));
                                 pha = Phase.value;
                                 break;

                              case Phase.tag_ending:
                                 xml_cur.To = i;
                                 xml_cur = xml_cur.ParentZone;
                                 pha = Phase.none;
                                 break;

                              case Phase.header:
                                 pha = Phase.none;
                                 break;

                              default:
                                 i = len - 1;//cause exit
                                 continue;
                           }
                           break;
                     }
                  }
               }

               return xml_roo;
            }
         }

         protected abstract FoldZone myGetRootFoldZone(string fileBody);

         public FoldZone GetRootFoldZone(string fileBody)
         {
            var roo_zon = myGetRootFoldZone(fileBody);
            var len = fileBody.Length;

            foreach (var zon in roo_zon.AllZones.Where(z => z.To == int.MaxValue)) { zon.To = len - 1; }

            myUpdateRowCol(roo_zon);

            return roo_zon;
         }

         private unsafe void myUpdateRowCol(FoldZone rootZone)
         {
            if (rootZone.Type == FoldZoneTypeEnum.text_root)
            {
               var len = rootZone.FileBody.Length;
               var seg_arr = rootZone.AllZones.ToArray();
               var seg_len = seg_arr.Length;
               var lin = 1;
               var lin_off = 0;

               fixed (char* fil_p = rootZone.FileBody)
               {
                  for (var pos = 0; pos < len; pos++)
                  {
                     if (fil_p[pos] == '\n' || pos == len - 1)//new line or last char
                     {
                        for (var idx = 0; idx < seg_len; idx++)
                        {
                           var seg = seg_arr[idx];

                           if (seg.FromPos == null && seg.From <= pos) { seg.FromPos = new TxtPos(lin, seg.From - lin_off + 1); }
                        }

                        for (var idx = 0; idx < seg_len; idx++)
                        {
                           var seg = seg_arr[idx];

                           if (seg.ToPos == null && seg.To <= pos) { seg.ToPos = new TxtPos(lin, seg.To - lin_off + 1); }
                        }

                        lin++;
                        lin_off = pos + 1;
                     }
                  }
               }
            }
            else { throw new Crash(); }
         }
      }
   }
}


