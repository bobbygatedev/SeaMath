using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class TextSearchInfrastructure
   {
      public const int MIN_MAX_FIND_ITEMS = 500;

      private int myMaxFindItems = 10000;
      private InnerFindSimpleState myFindSimpleState;
      private Lazy<ITextSearchAppInteraction> myLazyTxtAppInteraction;
      private Lazy<ITextSearchCtrlInteraction> myLazyTxtCtrlInteraction;
      private ITextSearchFindReplaceToolWin? myFindReplaceToolWin = null;

      public TextSearchInfrastructure()
      {
         myFindSimpleState = new InnerFindSimpleState(this);
         myLazyTxtAppInteraction = new Lazy<ITextSearchAppInteraction>(myMakeTxtAppInteraction);
         myLazyTxtCtrlInteraction = new Lazy<ITextSearchCtrlInteraction>(myMakeTxtCtrlInteraction);
      }

      /// <summary>
      /// Uses <seealso cref="TextSearchFindReplaceToolWin"/> as toolwin.
      /// </summary>
      public abstract class Standard : TextSearchInfrastructure
      {
         protected override ITextSearchFindReplaceToolWin myMakeFindReplaceToolWin() => new TextSearchFindReplaceToolWin();

         /// <summary>
         /// 
         /// </summary>
         public abstract TextSearchParamRecord SearchParamRecord { get; }

         protected override ITextSearchFindTask myMakeFindTask(TextSearchFindTaskType findTaskType) => new TextSearchFindTaskStandard(findTaskType);
      }

      private class InnerFindSimpleState
      {
         private TextSearchMode myMode;

         public InnerFindSimpleState(TextSearchInfrastructure findInfrastructure) => FindInfrastructure = findInfrastructure;

         public TextSearchInfrastructure FindInfrastructure { get; }

         public TextSearchMode Mode
         {
            get => myMode == TextSearchMode.none ? TextSearchMode.current_doc_mode : myMode;
            set => myMode = value;
         }

         public (Control control, TxtToken? selection)? SelectionTextActive { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public TextSearchFlags Flags { get; set; }

         public string Txt { get; set; } = "";

         public string[]? Directories { get; internal set; }

         public string? Pattern { get; internal set; }

         public bool IsFromReplace { get; internal set; } = false;

         public bool IsThereAToken(TextSearchFile findFile) =>
            FindInfrastructure.FindStrategy.FindAll(Txt, Flags, findFile.CurrentFileBody.Nn(), null).Length > 0;

         public void Reset()
         {
            Directories = [];
            Pattern = "*";
            Flags = TextSearchFlags.None;
            Mode = TextSearchMode.none;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract ITextSearchCtrlInteraction myMakeTxtCtrlInteraction();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract ITextSearchAppInteraction myMakeTxtAppInteraction();

      /// <summary>
      /// 
      /// </summary>
      public TextSearchFlags FindSimpleSearchFlags { get => myFindSimpleState.Flags; set => myFindSimpleState.Flags = value; }

      /// <summary>
      /// 
      /// </summary>
      public ITextSearchAppInteraction TxtAppInteraction => myLazyTxtAppInteraction.Value;

      /// <summary>
      /// 
      /// </summary>
      public ITextSearchCtrlInteraction TxtCtrlInteraction => myLazyTxtCtrlInteraction.Value;

      /// <summary>
      /// 
      /// </summary>
      public Control[] AllOpenTextControls => TxtAppInteraction.AllOpenTextControls;

      /// <summary>
      /// Current dir ie containing dir of currently selected file.
      /// </summary>
      public string SelectedFileDir
      {
         get
         {
            if (SelectedTextControl != null)
            {
               var ope_fil = TxtCtrlInteraction.GetOpenFile(SelectedTextControl);

               return !ope_fil.IsBlank() ? Directory.GetParent(ope_fil.Nn())?.FullName ?? "" : "";
            }
            else
            {
               return "";
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int MaxFindItems { get => myMaxFindItems; set => myMaxFindItems = Math.Max(MIN_MAX_FIND_ITEMS, value); }

      public string[] AllOpenFiles =>
         AllOpenTextControls.Select(c => TxtCtrlInteraction.GetOpenFile(c)).Nn().Where(t => !t.IsBlank()).ToArray();

      public virtual ITextSearchFindStategy FindStrategy => new TextSearchFindStategyDefault();

      public Control? SelectedTextControl { get => TxtAppInteraction.SelectedTextControl; set => TxtAppInteraction.SelectedTextControl = value; }

      protected abstract ITextSearchFindReplaceToolWin myMakeFindReplaceToolWin();

      public ITextSearchFindReplaceToolWin FindReplaceToolWin
      {
         get
         {
            if (myFindReplaceToolWin == null)
            {
               myFindReplaceToolWin = myMakeFindReplaceToolWin();
               myFindReplaceToolWin.PpFindInfrastructure = this;
            }

            return myFindReplaceToolWin;
         }
      }

      public void LaunchFindWinTool(TextSearchFindReplaceToolWinModeFlags launchModeFlags)
      {
         var sel_txt = SelectedTextControl != null && TxtCtrlInteraction.GetSelection(SelectedTextControl) != null ?
            TxtCtrlInteraction.GetSelection(SelectedTextControl)?.Content : null;

         if (sel_txt != null && sel_txt.Split('\n').Length > 1) { sel_txt = null; }

         FindReplaceToolWin.MthStart(
            TxtAppInteraction.GetParentForm().NnOrCrash(),
            sel_txt.Nn(),
            (launchModeFlags & TextSearchFindReplaceToolWinModeFlags.replace) != 0,
            (launchModeFlags & TextSearchFindReplaceToolWinModeFlags.doc) != 0);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="searchText"></param>
      /// <param name="userReplaceText">Replace searchText as input by user</param>
      /// <param name="searchFlags"></param>
      /// <param name="searchMode"></param>
      /// <param name="directories"></param>
      /// <param name="pattern"></param>
      /// <returns></returns>
      public bool ReplaceNext(
         string searchText,
         string userReplaceText,
         TextSearchFlags searchFlags,
         TextSearchMode searchMode,
         string[] directories,
         string pattern)
      {
         if (FindNext(searchText, searchFlags, searchMode, directories, pattern, true))
         {
            TxtCtrlInteraction.ReplaceSelected(
               FindStrategy.GetReplaceText(userReplaceText, searchFlags), TxtAppInteraction.SelectedTextControl.NnOrCrash());

            return true;
         }
         else
         {
            return false;
         }
      }

      /// <summary>
      /// Continues a search set by Find
      /// </summary>
      /// <param name="isPrevious">If has value changes direction of search(next/previous)</param>
      /// <param name="searchFlags">If has value changes search flags</param>
      /// <returns></returns>
      public virtual bool ContinueFind(bool? isPrevious = null, TextSearchFlags? searchFlags = null)
      {
         try
         {
            var flg = myFindSimpleState.Flags;

            if (isPrevious.HasValue)
            {
               if (isPrevious.Value) { flg |= TextSearchFlags.Backward; }
               else { flg &= ~TextSearchFlags.Backward; }
            }

            if (searchFlags.HasValue) { flg = searchFlags.Value; }

            if (myFindSimpleState.Mode == TextSearchMode.current_doc_mode || myFindSimpleState.Mode == TextSearchMode.selected_text)
            {
               return SelectedTextControl != null && myFindInDoc(myFindSimpleState.Txt, flg, SelectedTextControl, false);
            }
            else
            {
               //multi file search
               var lst_fls = myGetOrderFileList(
                  myGetSearchFiles(myFindSimpleState.Mode, myFindSimpleState.Directories ?? [], myFindSimpleState.Pattern.Nn()), flg);

               if (
                  SelectedTextControl != null &&
                  lst_fls.Any(f => f.OpenTextControl == SelectedTextControl) &&
                  myFindInDoc(myFindSimpleState.Txt, flg, SelectedTextControl, false))
               {
                  return true;
               }
               else
               {
                  var sel_fil =
                     lst_fls.
                     Where(f => f.OpenTextControl != SelectedTextControl).
                     FirstOrDefault(f => myFindSimpleState.IsThereAToken(f));

                  return sel_fil != null ? myFindInDoc(myFindSimpleState.Txt, flg, sel_fil.OpenAndSelect().NnOrCrash(), true) : false;
               }
            }
         }
         finally
         {
            myFindSimpleState.IsFromReplace = false;
         }
      }

      protected abstract ITextSearchFindTask myMakeFindTask(TextSearchFindTaskType findTaskType);

      public ITextSearchFindTask ReplaceAll(
         string searchText,
         string userReplaceText,
         TextSearchFlags searchFlags,
         TextSearchMode searchMode,
         string[] directories,
         string pattern)
      {
         var act = new Action(() => myReplaceAllBody(searchText, userReplaceText, searchFlags, searchMode, directories, pattern));
         var tsk = myMakeFindTask(TextSearchFindTaskType.replace);

         tsk.Start(act);

         return tsk;
      }
      /// <summary>
      /// 
      /// </summary>
      /// <param name="searchText"></param>
      /// <param name="userReplaceText"></param>
      /// <param name="searchFlags"></param>
      /// <param name="searchMode"></param>
      /// <param name="directories"></param>
      /// <param name="pattern"></param>
      /// <exception cref="Crash"></exception>
      protected virtual void myReplaceAllBody(
         string searchText,
         string userReplaceText,
         TextSearchFlags searchFlags,
         TextSearchMode searchMode,
         string[] directories,
         string pattern)
      {
         var ope_ctr = TxtAppInteraction.SelectedTextControl;
         var src_fls = myGetSearchFiles(searchMode, directories, pattern);

         if (searchMode == TextSearchMode.current_doc_mode || searchMode == TextSearchMode.selected_text)
         {
            if (ope_ctr != null)
            {
               var src_fil = src_fls.First();
               var rep_txt = FindStrategy.GetReplaceText(userReplaceText, searchFlags);
               var fnd_tks = null as TxtStore.Sector[];

               if (searchMode == TextSearchMode.selected_text)
               {
                  var sel_tok = TxtCtrlInteraction.GetSelection(ope_ctr);

                  fnd_tks = FindStrategy.ReplaceCurrentDoc(searchText, rep_txt, searchFlags, (src_fil?.CurrentFileBody).Nn(), sel_tok);
               }
               else
               {
                  var cur_pos = myGetTextPosForReplace(searchFlags, ope_ctr, TxtCtrlInteraction.GetTextPos(ope_ctr));

                  fnd_tks = FindStrategy.ReplaceCurrentDoc(searchText, rep_txt, searchFlags, (src_fil?.CurrentFileBody).Nn(), cur_pos);
               }

               if (fnd_tks.Length > 0)
               {
                  var fil = fnd_tks[0].Store;

                  TxtCtrlInteraction.ResetText((src_fil?.OpenTextControl).NnOrCrash(), fil.Content);
               }
            }
         }
         else
         {
            TxtAppInteraction.OnStartFindInFiles(searchText, searchFlags, searchMode, directories, pattern);

            foreach (var src_fil in src_fls)
            {
               //find in files is always wrap-around
               var rep_txt = FindStrategy.GetReplaceText(userReplaceText, searchFlags);
               var rep_tks = FindStrategy.ReplaceAll(searchText, rep_txt, searchFlags, src_fil.CurrentFileBody.Nn());

               if (rep_tks?.Length > 0)
               {
                  var rep_fil = rep_tks[0].Store;

                  if (src_fil.OpenTextControl != null) { TxtCtrlInteraction.ResetText(src_fil.OpenTextControl, rep_fil.Content); }
                  else if ((searchFlags & TextSearchFlags.OpenWhenReplace) != 0)
                  {
                     TxtAppInteraction.OpenFile(src_fil.FilePath.Nn());
                     TxtCtrlInteraction.ResetText(src_fil.OpenTextControl.NnOrCrash(), rep_fil.Content);
                  }
                  else
                  {
                     //var fil = rep_tks[0].SourceFrom.Store;

                     try { rep_fil.Save(src_fil.FilePath); }
                     catch (System.IO.IOException) { }
                     catch (Exception exc) { throw new Crash(exc); }
                  }

                  TxtAppInteraction.OnAddFindTokens(rep_tks.Select(t => new TextSearchToken(src_fil, t)).ToArray());
               }
            }

            TxtAppInteraction.OnEndFindInFiles();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="searchText"></param>
      /// <param name="searchFlags"></param>
      /// <param name="searchMode"></param>
      /// <param name="directories"></param>
      /// <param name="pattern"></param>
      /// <returns>All find token or null if no find token is found.</returns>
      protected virtual void myFindAllBody(
         string searchText, TextSearchFlags searchFlags, TextSearchMode searchMode, string[] directories, string pattern)
      {
         var lst_fnd = new List<TextSearchToken>();

         var tsk = new Task(() =>
         {
            //get all files where search is made
            var fls = myGetSearchFiles(searchMode, directories, pattern);

            TxtAppInteraction.OnStartFindInFiles(searchText, searchFlags, searchMode, directories, pattern);

            var cnt = 0;
            var fla = searchFlags & ~TextSearchFlags.Backward | TextSearchFlags.WrapAround;//forward and full 

            foreach (var fil in fls)
            {
               var tks = FindStrategy.FindAll(searchText, fla, fil.CurrentFileBody.Nn(), null);
               if ((cnt = cnt + tks.Length) <= MaxFindItems)
               {
                  TxtAppInteraction.OnAddFindTokens(tks.Select(t => new TextSearchToken(fil, t)).ToArray());
               }
               else { break; }
            }

            TxtAppInteraction.OnEndFindInFiles();
         });

         tsk.Start();

         while (!tsk.IsCompleted) { Application.DoEvents(); }
      }

      public ITextSearchFindTask FindAll(
         string searchText, TextSearchFlags searchFlags, TextSearchMode searchMode, string[] directories, string pattern)
      {
         var act = new Action(() => myFindAllBody(searchText, searchFlags, searchMode, directories, pattern));
         var tsk = myMakeFindTask(TextSearchFindTaskType.find);

         tsk.Start(act);

         return tsk;
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="searchText"></param>
      /// <param name="searchFlags"></param>
      /// <param name="searchMode"></param>
      /// <param name="directories"></param>
      /// <param name="pattern"></param>
      /// <param name="isFromReplace"></param>
      /// <returns></returns>
      public virtual bool FindNext(
         string searchText, TextSearchFlags searchFlags, TextSearchMode searchMode, string[] directories, string pattern, bool isFromReplace)
      {
         myFindSimpleState.Reset();
         myFindSimpleState.Mode = searchMode;
         myFindSimpleState.Flags = searchFlags;
         myFindSimpleState.Directories = directories;
         myFindSimpleState.Pattern = pattern;
         myFindSimpleState.Txt = searchText;
         myFindSimpleState.IsFromReplace = isFromReplace;

         return ContinueFind();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="searchMode"></param>
      /// <param name="directories"></param>
      /// <param name="pattern"></param>
      /// <returns></returns>
      protected virtual TextSearchFile[] myGetSearchFiles(TextSearchMode searchMode, string[] directories, string pattern)
      {
         var fls = null as TextSearchFile[];

         switch (searchMode)
         {
            case TextSearchMode.current_doc_mode:
            case TextSearchMode.selected_text:
               return SelectedTextControl != null ?
                  [new TextSearchFile(SelectedTextControl, this)] :
                  [];

            case TextSearchMode.find_in_files_all_open_docs: return AllOpenTextControls.Select(c => new TextSearchFile(c, this)).ToArray();

            case TextSearchMode.selected_file_dir:
               return myGetFilesInDirectory(SelectedFileDir, pattern).
                  Select(f => new TextSearchFile(f, this)).OrderByDescending(f => f.FilePath).Reverse().ToArray();

            case TextSearchMode.find_in_files_dir_list:
               return myGetFilesInDirectoryList(directories, pattern).Select(f => new TextSearchFile(f, this)).
                  OrderByDescending(f => f.FilePath).Reverse().ToArray();

            default: throw new Crash();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="searchText"></param>
      /// <param name="searchFlags"></param>
      /// <param name="docTextCtrl"></param>
      /// <param name="isRewind">Restarts doc search</param>
      /// <returns></returns>
      protected bool myFindInDoc(string searchText, TextSearchFlags searchFlags, Control docTextCtrl, bool isRewind)
      {
         var sel_tok = null as TxtToken;
         var txt_cnt = TxtCtrlInteraction.GetFileContent(docTextCtrl);
         var txt_pos = isRewind ? null : TxtCtrlInteraction.GetTextPos(docTextCtrl);

         if (myFindSimpleState.Mode == TextSearchMode.selected_text)
         {
            sel_tok = TxtCtrlInteraction.GetMultilineSelectionToken(docTextCtrl);

            if (sel_tok == null)
            {
               sel_tok = TxtCtrlInteraction.GetSelection(docTextCtrl);

               if (sel_tok != null) { TxtCtrlInteraction.SetMultilineSelectionToken(docTextCtrl, sel_tok); }
               else { return false; }
            }
         }
         else
         {
            TxtCtrlInteraction.SetMultilineSelectionToken(docTextCtrl, null);
         }

         if (myFindSimpleState.Mode == TextSearchMode.current_doc_mode)
         {
            if (myFindSimpleState.IsFromReplace)
            {
               //selected token
               txt_pos = myGetTextPosForReplace(searchFlags, docTextCtrl, txt_pos);
            }
            else if ((searchFlags & TextSearchFlags.Backward) != 0)
            {
               var sel = TxtCtrlInteraction.GetSelection(docTextCtrl);

               if (sel != null)
               {
                  txt_pos = new TxtPos(sel.From?.Line ?? throw new Crash(), sel.From.Col, txt_pos?.Store);
               }
            }
         }

         var fnd_tok = FindStrategy.FindNext(searchText, searchFlags, txt_cnt.Nn(), txt_pos, sel_tok);

         if (fnd_tok != null)
         {
            TxtCtrlInteraction.SelectToken(fnd_tok, docTextCtrl);

            return true;
         }

         return false;
      }

      /// <summary>
      /// When doing a replace operation in current document mode, adjust the text position
      /// to start searching from the beginning of the current selection if it's before the cursor.
      /// This ensures we don't miss matches when doing multiple replacements.
      /// </summary>
      /// <param name="searchFlags"></param>
      /// <param name="docTextCtrl"></param>
      /// <param name="txtPos"></param>
      /// <returns></returns>
      private TxtPos? myGetTextPosForReplace(TextSearchFlags searchFlags, Control docTextCtrl, TxtPos? txtPos)
      {
         var sel = TxtCtrlInteraction.GetSelection(docTextCtrl);

         if (sel != null)
         {
            var is_bak = (searchFlags & TextSearchFlags.Backward) != 0;

            if (is_bak && sel?.To?.CompareTo(txtPos) > 0)
            {
               txtPos = new TxtPos(sel.To.Line, sel.To.Col, txtPos?.Store);
            }

            if (!is_bak && sel?.From?.CompareTo(txtPos) < 0)
            {
               txtPos = new TxtPos(sel.From.Line, sel.From.Col, txtPos?.Store);
            }
         }

         return txtPos;
      }

      private List<TextSearchFile> myGetOrderFileList(TextSearchFile[] files, TextSearchFlags flags)
      {
         var fls = (flags & TextSearchFlags.Backward) != 0 ? files.Reverse() : files;
         var idx = SelectedTextControl != null ? fls.Select(f => f.OpenTextControl).ToList().IndexOf(SelectedTextControl) : -1;
         var fls_1 = fls.Skip(idx).ToArray();
         var fls_2 = fls.Take(idx).ToArray();

         return fls_1.Concat(fls_2).ToList();
      }

      private static string[] myGetFilesInDirectoryList(string[] directories, string pattern) =>
         directories.SelectMany(d => myGetFilesInDirectory(d, pattern)).ToArray();

      private static string[] myGetFilesInDirectory(string directory, string pattern)
      {
         var drs = new string[0];
         var fls = new string[0];

         try
         {
            drs = Directory.EnumerateDirectories(directory).ToArray();
         }
         catch (System.ArgumentException) { }
         catch (System.Security.SecurityException) { }
         catch (System.UnauthorizedAccessException) { }
         catch (System.IO.DirectoryNotFoundException) { }

         try
         {
            var sp_ptr = pattern.Split(',', ';');

            fls = sp_ptr.SelectMany(p => Directory.EnumerateFiles(directory, p)).OrderBy(f => f).ToArray();
         }
         catch (System.ArgumentException) { }
         catch (System.Security.SecurityException) { }
         catch (System.UnauthorizedAccessException) { }
         catch (System.IO.DirectoryNotFoundException) { }

         return fls.Concat(drs.SelectMany(d => myGetFilesInDirectory(d, pattern))).ToArray();
      }
   }
}
