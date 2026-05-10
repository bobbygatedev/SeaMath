using Gate.Tools;
using Gate.Tools.Extensions;
using System.Data;
using static Gate.ToolsView.TextSearch.TextSearchParamRecord.LookInRecord;

namespace Gate.ToolsView.TextSearch
{
   public partial class TextSearchControl
   {
      public const string SELECTED_FILE_DIR_LABEL = "Selected File Dir";
      public const string CURR_DOC_LABEL = "Current Document";
      public const string ALL_OPEN_DOCS_LABEL = "All Open Documents";
      public const string SELECTED_TEXT_LABEL = "Selected Text";

      private abstract class InnerLookInItem
      {
         public delegate void OnLookItemChangedHandler(InnerLookInItem? lookInItem);

         protected InnerLookInItem(TextSearchControl parent) => Parent = parent;

         public class Fixed : InnerLookInItem
         {
            public Fixed(TextSearchMode searchMode, TextSearchControl parent) : base(parent) => SearchMode = searchMode;

            public override TextSearchMode SearchMode { get; }

            public override string DropDownText
            {
               get
               {
                  switch (SearchMode)
                  {
                     case TextSearchMode.current_doc_mode: return CURR_DOC_LABEL;
                     case TextSearchMode.find_in_files_all_open_docs: return ALL_OPEN_DOCS_LABEL;
                     case TextSearchMode.selected_file_dir: return $"{SELECTED_FILE_DIR_LABEL}{(SelectedFileDir != "" ? $"({SelectedFileDir})" : "")}";
                     case TextSearchMode.selected_text: return SELECTED_TEXT_LABEL;

                     default: throw new Crash();
                  }

               }
            }
         }

         public class DirList : InnerLookInItem
         {
            public DirList(string[] dirs, TextSearchControl parent) : base(parent) => Dirs = dirs;

            public string[] Dirs { get; }

            public override TextSearchMode SearchMode => TextSearchMode.find_in_files_dir_list;

            public override string DropDownText => string.Join(";", Dirs);

            public bool HasSameDirectory(DirList otherDirList)
            {
               var oth_drs = otherDirList.Dirs;
               var ful_1 = Dirs.Where(d => Directory.Exists(d)).Select(d => Path.GetFullPath(d)).OrderBy(d => d).ToArray();
               var ful_2 = oth_drs.Where(d => Directory.Exists(d)).Select(d => Path.GetFullPath(d)).OrderBy(d => d).ToArray();

               return ful_1.SequenceEqual(ful_2);
            }
         }

         public class Set
         {
            private InnerLookInItem? myCurrentLookItem;
            private List<DirList>? myListLookItemsDirList = null;

            public event OnLookItemChangedHandler? OnLookItemChanged;

            public Set(TextSearchControl parent)
            {
               Parent = parent;
               CtrlComboLookIn = Parent.CtrlComboLookIn;
               CtrlComboLookIn.DropDown += CtrlComboLookIn_DropDown;
               CtrlComboLookIn.Validated += CtrlComboLookIn_Validated;
               CtrlComboLookIn.KeyDown += CtrlComboLookIn_KeyDown;
               CtrlComboLookIn.SelectedIndexChanged += CtrlComboLookIn_SelectedIndexChanged;

               var src_mds = new TextSearchMode[] {
                  TextSearchMode.selected_text,
                  TextSearchMode.current_doc_mode,
                  TextSearchMode.find_in_files_all_open_docs,
                  TextSearchMode.selected_file_dir };

               LookItemsFixed = src_mds.Select(m => new Fixed(m, parent)).ToArray();
            }

            private void CtrlComboLookIn_SelectedIndexChanged(object? sender, EventArgs e) => myEvaluateText();

            public InnerLookInItem[] LookItemsFixed { get; }

            public InnerLookInItem[] LookItemsDirectory => (myListLookItemsDirList ?? new List<DirList>()).ToArray();

            public ComboBox CtrlComboLookIn { get; }

            public TextSearchControl Parent { get; }

            public TextSearchParamRecord? PpSearchParamRecord => Parent.PpSearchParamRecord;

            public InnerLookInItem[] LookItemsActive =>
               (IsSelectTextActive ? LookItemsFixed : LookItemsFixed.Where(i => i.SearchMode != TextSearchMode.selected_text)).
                  Concat(LookItemsDirectory).ToArray();

            /// <summary>
            /// Reads look in text and tries to compare to possible lookitems in combo box dropdowns
            /// </summary>
            /// <returns></returns>
            public InnerLookInItem? CurrentLookItem
            {
               get => myCurrentLookItem ?? (Parent.PpIsInDocMode
                         ? LookItemsFixed.First(i => i.SearchMode == TextSearchMode.current_doc_mode)
                         : LookItemsActive.First(i => i.SearchMode == TextSearchMode.find_in_files_all_open_docs));

               set
               {
                  myCurrentLookItem = value;
                  OnLookItemChanged?.Invoke(myCurrentLookItem);

                  if (myCurrentLookItem != null)
                  {
                     CtrlComboLookIn.Text = myCurrentLookItem.DropDownText;
                  }
               }
            }

            public bool IsSelectTextActive { get; set; } = false;

            public TextSearchMode SelectFixed
            {
               get => CurrentLookItem != null && CurrentLookItem.IsFixed ? CurrentLookItem.SearchMode : TextSearchMode.none;

               set
               {
                  var itm = LookItemsFixed.FirstOrDefault(i => i.SearchMode == value);

                  if (itm != null)
                  {
                     myFillCombo();
                     CtrlComboLookIn.SelectedItem = itm;
                  }
                  else { throw new Crash(); }
               }
            }

            public void Load()
            {
               if (Parent.PpSearchParamRecord != null)
               {
                  if (myListLookItemsDirList == null)
                  {
                     myListLookItemsDirList = new List<DirList>();

                     foreach (var dir_itm in Parent.PpSearchParamRecord.LookIn.DirItems.Where(d=>d != null))
                     {
                        myListLookItemsDirList.Add(new DirList(dir_itm.Dirs, Parent));
                     }
                  }

                  CtrlComboLookIn.Text = Parent.PpIsInDocMode ?
                     Parent.PpSearchParamRecord.LookIn.TextValueDoc.Value : Parent.PpSearchParamRecord.LookIn.TextValueFiles.Value;
               }
            }

            public void BeforeStartFind()
            {
               if (CtrlComboLookIn.Text.Trim() == "") { CtrlComboLookIn.Text = "*.*"; }

               if (PpSearchParamRecord != null)
               {
                  var lok_in_itm = CurrentLookItem;

                  if (lok_in_itm != null && lok_in_itm.SearchMode != TextSearchMode.selected_text)
                  {
                     if (Parent.PpIsInDocMode)
                     {
                        PpSearchParamRecord.LookIn.TextValueDoc.Value = lok_in_itm.DropDownText;
                     }
                     else
                     {
                        PpSearchParamRecord.LookIn.TextValueFiles.Value = lok_in_itm.DropDownText;
                     }
                  }
               }
            }

            private void myEvaluateText(string? comboText = null)
            {
               comboText = comboText ?? CtrlComboLookIn.Text;
               var cur_lok_itm = LookItemsFixed.FirstOrDefault(f => string.Compare(f.DropDownText, comboText.Trim(), true) == 0);

               if (cur_lok_itm == null)
               {
                  if (TryParseDirectoryList(comboText.Trim(), Parent, out var lok_in_itm))
                  {
                     lok_in_itm = lok_in_itm.NnOrCrash();
                     cur_lok_itm = lok_in_itm;

                     var lst = myListLookItemsDirList.NnOrCrash();

                     var oth = lst.FirstOrDefault(d => d.HasSameDirectory(lok_in_itm));

                     if (oth != null) { lst.Remove(oth); }
                     
                     lst.Insert(0, lok_in_itm);

                     if (Parent.PpSearchParamRecord != null)
                     {
                        Parent.PpSearchParamRecord.LookIn.DirItems = 
                           myListLookItemsDirList.NnOrCrash().Select(i => new DirItem(i.Dirs)).ToArray();
                     }
                  }
               }

               CurrentLookItem = cur_lok_itm;
            }

            private void myFillCombo()
            {
               CtrlComboLookIn.Items.Clear();
               CtrlComboLookIn.Items.AddRange(LookItemsActive);
            }

            private void CtrlComboLookIn_KeyDown(object? sender, KeyEventArgs e)
            {
               if (!e.Control && !e.Shift && !e.Alt && e.KeyCode == Keys.Enter)
               {
                  myEvaluateText(CtrlComboLookIn.Text);
               }
            }

            private void CtrlComboLookIn_Validated(object? sender, EventArgs e) => myEvaluateText(CtrlComboLookIn.Text);

            private void CtrlComboLookIn_DropDown(object? sender, EventArgs e) => myFillCombo();
         }

         public abstract TextSearchMode SearchMode { get; }

         public static TextSearchMode[] FixedModes =>[ 
            TextSearchMode.current_doc_mode,
            TextSearchMode.find_in_files_all_open_docs,
            TextSearchMode.selected_file_dir ,
            TextSearchMode.selected_text];

         public bool IsFixed => FixedModes.Contains(SearchMode);

         public TextSearchControl Parent { get; }

         public abstract string DropDownText { get; }

         public string? SelectedFileDir => Parent?.PpSearchInfrastructure?.SelectedFileDir;

         public static bool TryParseDirectoryList(string txt, TextSearchControl gateTextSearchControl, out DirList? lookInItemDirList)
         {
            if (myTryParseDirList(txt, out var dir_lst))
            {
               lookInItemDirList = new DirList(dir_lst ?? [], gateTextSearchControl);

               return true;
            }
            else
            {
               lookInItemDirList = null;

               return false;
            }
         }

         public override string ToString() => DropDownText;

         private static bool myIsValidPath(string path, bool allowRelativePaths = false)
         {
            var is_ok = true;

            try
            {
               var fullPath = Path.GetFullPath(path);

               if (allowRelativePaths) { is_ok = Path.IsPathRooted(path); }
               else
               {
                  var roo = Path.GetPathRoot(path);

                  is_ok = string.IsNullOrEmpty(roo.Nn().Trim(['\\', '/'])) == false;
               }
            }
            catch { is_ok = false; }

            return is_ok;
         }

         private static bool myTryParseDirList(string txt, out string[]? dirList)
         {
            if (txt.Split(';').Select(p => p.Trim()).All(p => myIsValidPath(p)))
            {
               dirList = txt.Split(';').Select(p => p.Trim()).Where(d => Directory.Exists(d)).ToArray();

               return true;
            }
            else
            {
               dirList = null;

               return false;
            }
         }
      }
   }
}
