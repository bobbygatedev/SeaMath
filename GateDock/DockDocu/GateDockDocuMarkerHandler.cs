using Gate.Dock.DockApp;
using Gate.Dock.DockTab;
using Gate.Dock.Extensions;
using Gate.Tools;
using Gate.Tools.DesignPattern;
using Gate.Tools.Extensions;
using Gate.Tools.Multithread;
using Gate.Tools.Text;
using Gate.ToolsView.Extensions;
using System.Runtime.ConstrainedExecution;
using static Gate.Tools.Text.TxtLineComparer.SectionType;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// Handler class for bookmarks and breakpoints.
   /// protocol:
   /// A bookmark/breakpoint is updated and the file is saved bookmark/breakpoint is saved on state
   /// A bookmark/breakpoint is updated and the file is NOT saved bookmark/breakpoint is saved on 
   ///    <see cref="Bookmarks"/> <see cref="Breakpoints"/>list only
   /// A file is saved all its bookmark/breakpoint are saved on state
   /// a file is updated from extern bookmark/breakpoint are updated based on 
   ///    <see cref="Gate.Tools.Text.TxtLineComparer.Compare(string, string)"/> then the state saved
   /// bookmark/breakpoint of file not having a path (not saved) are always saved 
   /// </summary>
   public class GateDockDocuMarkerHandler : BaseClassWithFinalizer
   {
      public event OnBreakpointsChangedHandler? OnBreakpointsChanged;
      public event OnBoomarksChangedHandler? OnBookmarksChanged;

      private List<string> myListBookmarkOrderedGuids = new List<string>();
      private GateDockDocuMarkerBookmark? myBookmarkCurrent;
      private Dictionary<IGateDockDocuText, GateDockDocuMarkerBookmark[]> myDictBookmarks =
         new Dictionary<IGateDockDocuText, GateDockDocuMarkerBookmark[]>();

      public GateDockDocuMarkerHandler(GateDockApp app)
      {
         (App = app).MainForm.OnTabPageOpen += MainForm_OnTabPageOpen;
         app.MainForm.OnTabPageClosing += MainForm_OnTabPageClosing;
      }

      public IGateDockDocuText[] AllDocus => App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public GateDockDocuMarkerBookmark? BookmarkCurrent
      {
         get => myBookmarkCurrent;

         private set
         {
            if (value != null)
            {
               if (Bookmarks.Contains(value))
               {
                  var doc =
                     AllDocus.
                     FirstOrDefault(d =>
                        (d.PpBookmarks ?? []).Any(b => b.Guid == value.Guid));

                  if (doc == null)
                  {
                     if (!value.IsBookmarkPathExisting)
                     {
                        myBookmarkCurrent = null;
                        return;
                     }
                     else
                     {
                        doc =
                           App.MainForm.PpDocuHandler.OpenPath(
                              App.MainForm, (myBookmarkCurrent?.BookmarkPath).NnOrCrash()) as IGateDockDocuText ??
                           throw new Crash();
                     }
                  }

                  App.MainForm.PpTabPageCurrent = (GateDockTabPageCtrl)doc;
                  doc.PpCurrLine = (value?.Line).NnOrCrash();
                  doc.PpCurrCol = 1;
                  myBookmarkCurrent = value;
               }
               else
               {
                  myBookmarkCurrent = Bookmarks.FirstOrDefault();
               }
            }
         }
      }

      /// <summary>
      /// Current breakpoint array includes:
      /// 1) in all open windows breakpoint markers associated to true saved file 
      /// 2) all other state-saved breakpoints associated file not in 1
      /// </summary>
      public GateDockDocuMarkerBreakpoint[] Breakpoints
      {
         get
         {
            var sav_bks = BreakpointsStateFiltered;
            var fls = sav_bks.
               Select(b => b.BreakpointPath).
               Where(p => Path.IsPathRooted(p)).
               Distinct().
               ToArray();

            //existing documents whose path is not of any saved breakpoint
            var doc_no_fls = AllDocus.
               Where(d =>
                  !d.PpDocuPath.IsBlank() &&
                  File.Exists(d.PpDocuPath) &&
                  d.IsSaved &&
                  !fls.Any(f => myGetTextDocu(f) == d)).ToArray();
            var lst = new List<GateDockDocuMarkerBreakpoint>();

            foreach (var fil in fls)
            {
               var doc = myGetTextDocu(fil);

               //if file is associated to doc window and is saved its breakpoint are added
               if (doc != null && doc.IsSaved)
               {
                  lst.AddRange(doc.PpBreakpoints ?? []);
               }
               else
               {
                  //otw are added saved breakpoints
                  lst.AddRange(sav_bks.Where(b => b.BreakpointPath.IsEqualNoContent(fil)));
               }
            }

            //added breakpoint 
            lst.AddRange(
               doc_no_fls.
               Where(d => !d.PpDocuPath.IsBlank() && d.IsSaved).
               SelectMany(d => d.PpBreakpoints ?? []));

            return lst.ToArray();
         }
      }

      public GateDockDocuMarkerBreakpoint[] Breakpoints2Save
      {
         get
         {
            var sav_bks = BreakpointsStateFiltered;
            var fls = sav_bks.Select(b => b.BreakpointPath).Distinct().ToArray();

            //documents whose path is not of any breakpoint
            var doc_no_fls = AllDocus.Where(d => !fls.Any(f => myGetTextDocu(f) == d)).ToArray();
            var lst = new List<GateDockDocuMarkerBreakpoint>();

            /// nearly equal to <see cref="Breakpoints"/> but breakpoint come from document breakpoints
            /// just if doc is <see cref="IGateDockDocuText.IsSaved"/>
            foreach (var fil in fls)
            {
               var doc = myGetTextDocu(fil);

               if (doc != null &&
                  (doc.IsSaved && File.Exists(doc.PpDocuPath) || doc.PpDocuPath.IsBlank()))
               {
                  lst.AddRange(doc.PpBreakpoints ?? []);
               }
               else if (Path.IsPathRooted(fil))
               {
                  lst.AddRange(sav_bks.Where(b => b.BreakpointPath.IsEqualNoContent(fil)));
               }
            }

            lst.AddRange(doc_no_fls.SelectMany(d => d.PpBreakpoints ?? []));

            return lst.ToArray();
         }
      }

      public GateDockDocuMarkerBookmark[] BookmarksStateFiltered =>
         App.StateContainer.Params.Bookmarks.Items.Where(i => myFilterPath(i.BookmarkPath)).ToArray();

      public GateDockDocuMarkerBreakpoint[] BreakpointsStateFiltered =>
         App.StateContainer.Params.Breakpoints.Items.Where(i => myFilterPath(i.BreakpointPath)).ToArray();

      public GateDockDocuMarkerBookmark[] Bookmarks
      {
         get
         {
            var bms = BookmarksStateFiltered;
            var fls = bms.Select(b => b.BookmarkPath).Distinct().ToArray();

            //documents whose path is not of any breakpoint
            var doc_no_fls = AllDocus.Where(d => !fls.Any(f => myGetTextDocu(f) == d)).ToArray();
            var lst = new List<GateDockDocuMarkerBookmark>();

            foreach (var fil in fls)
            {
               var doc = myGetTextDocu(fil);

               if (doc != null)
               {
                  lst.AddRange(doc.PpBookmarks ?? []);
               }
               else if (Path.IsPathRooted(fil))
               {
                  lst.AddRange(bms.Where(b => b.BookmarkPath.IsEqualNoContent(fil)));
               }
            }

            lst.AddRange(doc_no_fls.SelectMany(d => d.PpBookmarks ?? []));

            return myGetOrderedBookmarks(lst.ToArray());
         }
      }

      public GateDockDocuMarkerBookmark[] Bookmarks2Save
      {
         get
         {
            var bms = BookmarksStateFiltered;
            var fls = bms.Select(b => b.BookmarkPath).Distinct().ToArray();

            //documents whose path is not of any breakpoint
            var doc_no_fls = AllDocus.Where(d => !fls.Any(f => myGetTextDocu(f) == d)).ToArray();
            var lst = new List<GateDockDocuMarkerBookmark>();

            /// nearly equal to <see cref="Bookmarks"/> but breakpoint came from docu breakpoints
            /// just if doc is <see cref="IGateDockDocuText.IsSaved"/>
            foreach (var fil in fls)
            {
               var doc = myGetTextDocu(fil);

               if (
                  doc != null &&
                  (doc.IsSaved && File.Exists(doc.PpDocuPath) || doc.PpDocuPath.IsBlank()))
               {
                  lst.AddRange(doc.PpBookmarks ?? []);
               }
               else if (Path.IsPathRooted(fil))
               {
                  //check if fil is a full path (eg c:\temp) or a simple name (eg NewFile1)
                  //in latter case a no saved text window has been closed without being saved
                  lst.AddRange(bms.Where(b => b.BookmarkPath.IsEqualNoContent(fil)));
               }
            }

            lst.AddRange(doc_no_fls.SelectMany(d => d.PpBookmarks ?? []));

            return myGetOrderedBookmarks(lst.ToArray());
         }
      }

      public GateDockApp App { get; }

      public void BookmarkToggle()
      {
         if (App.MainForm.PpTabPageCurrent is IGateDockDocuText txt_ctr)
         {
            var bkm = txt_ctr.MthToggleBookmark();

            if (bkm != null)
            {
               myListBookmarkOrderedGuids.Add(bkm.Guid.NnOrCrash());
            }

            mySaveBookmarks();
         }
      }

      public void BookmarkClearAll()
      {
         foreach (var txt_ctr in App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>())
         {
            txt_ctr.PpBookmarks = null;
         }

         App.StateContainer.Params.Bookmarks.Clear();
      }

      public void BookmarkMoveToNext()
      {
         if (BookmarkCurrent == null) { BookmarkCurrent = Bookmarks.FirstOrDefault(); }
         else
         {
            if (Bookmarks.Length > 0)
            {
               var idx = Bookmarks.ToList().IndexOf(BookmarkCurrent) + 1;

               if (idx == Bookmarks.Length) { idx = 0; }

               BookmarkCurrent = Bookmarks[idx];
            }
            else
            {
               BookmarkCurrent = null;
            }
         }
      }

      public void BookmarkMoveToPrevious()
      {
         if (BookmarkCurrent == null) { BookmarkCurrent = Bookmarks.FirstOrDefault(); }
         else
         {
            if (Bookmarks.Length > 0)
            {
               var idx = Bookmarks.ToList().IndexOf(BookmarkCurrent) - 1;

               if (idx == -1) { idx = Bookmarks.Length - 1; }

               BookmarkCurrent = Bookmarks[idx];
            }
            else
            {
               BookmarkCurrent = null;
            }
         }
      }

      public void BreakpointToggle()
      {
         if (App.MainForm.PpTabPageCurrent is IGateDockDocuText txt_ctr)
         {
            txt_ctr.MthToggleBreakpoint();
            mySaveBreakpoints();
         }
      }

      public void BreakpointDeleteAll()
      {
         foreach (var txt_ctr in App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>())
         {
            txt_ctr.PpBreakpoints = null;
         }

         App.StateContainer.Params.Breakpoints.Clear();
         OnBreakpointsChanged?.Invoke(this, Breakpoints);
      }

      public void Load() => myListBookmarkOrderedGuids =
            App.StateContainer.Params.Bookmarks.Items.
            Select(i => i.Guid.NnOrCrash()).
            Distinct().
            ToList();

      public void ActionOnClosing()
      {
         mySaveBookmarks();
         mySaveBreakpoints();
      }

      private static GateDockDocuMarkerBookmark[] myCloneBookmarks(IGateDockDocuText doc)
      {
         var res = null as GateDockDocuMarkerBookmark[];
         var ctr = doc.ConvertOrCrash<Control>();

         ctr.MthInvoke(() => res = myCloneBookmarks(doc.PpBookmarks ?? []));

         return res.NnOrCrash();
      }

      private static GateDockDocuMarkerBookmark[] myCloneBookmarks(IEnumerable<GateDockDocuMarkerBookmark> bookmarks) =>
         bookmarks.Select(b => (GateDockDocuMarkerBookmark)b.MakeInstance(true)).ToArray();

      protected override void myFreeManaged() { }

      protected override void myFreeUnmanaged() { }

      private void mySaveBreakpoints()
      {
         var b2s = Breakpoints2Save;

         App.StateContainer.Params.Breakpoints.Clear();
         App.StateContainer.Params.Breakpoints.AddParams(b2s);
      }

      private bool myIsSectionConfirmBookmarks(TxtLineComparer.SectionType? section) =>
         section?.Type == TypeEnum.equal || myIsSimpleSectionReplace(section);

      private bool myIsSimpleSectionReplace(TxtLineComparer.SectionType? section) =>
         section?.Type == TypeEnum.replace &&
         section.LineIntervalNew0.Length == 1 &&
         section.LineIntervalOld0.Length == 1 &&
         myIsSimpleLineChange(section.LinesOld[0], section.LinesNew[0]);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="lineOld"></param>
      /// <param name="lineNew"></param>
      /// <returns></returns>
      private bool myIsSimpleLineChange(string lineOld, string lineNew)
      {
         var ran = Enumerable.Range(1, Math.Min(lineOld.Length, lineNew.Length)).ToArray();
         var beg = ran.TakeWhile(i => lineOld[i - 1] == lineNew[i - 1]).MaxOrDefault();
         var end = ran.TakeWhile(i => lineOld[lineOld.Length - i] == lineNew[lineNew.Length - i]).MaxOrDefault();

         return beg > 0 || end < 0;
      }

      private GateDockDocuMarkerBookmark[] myGetOrderedBookmarks(GateDockDocuMarkerBookmark[] bookmarks)
      {
         var bok_grs = bookmarks.
            Where(g => !g.Guid.IsBlank()).
            GroupBy(b => b.Guid.NnOrCrash()).
            ToDictionary(g => g.Key, g => g.First());

         var bmk_ord =
            myListBookmarkOrderedGuids.
            Select(g => bok_grs.TryGetValue(g, out var v) ? v : null).
            Nn().ToArray();
         var bmk_oth = bookmarks.Except(bmk_ord).ToArray();
         var res = bmk_ord.Concat(bmk_oth).ToArray();

         myListBookmarkOrderedGuids = res.Select(i => i.Guid.NnOrCrash()).ToList();

         return res;
      }

      private IGateDockDocuText? myGetTextDocu(string? fileName) =>
         AllDocus.FirstOrDefault(d => d.PpDocuPath.ExtTrim().IsEqualNoContent(fileName)) ??
         AllDocus.FirstOrDefault(d => d.PpDocuName.ExtTrim().IsEqualNoContent(fileName));

      private bool myFilterPath(string? path)
      {
         if (Path.IsPathRooted(path))
         {
            return File.Exists(path);
         }
         else
         {
            return AllDocus.Any(d => d.GetMarkerPath() == path);
         }
      }

      private void mySaveBookmarks()
      {
         var b2s = Bookmarks2Save;

         App.StateContainer.Params.Bookmarks.Clear();
         App.StateContainer.Params.Bookmarks.AddParams(b2s);
      }

      private void MainForm_OnTabPageOpen(object? sender, GateDockTabPageCtrl? tabPage)
      {
         if (tabPage is IGateDockDocuText doc)
         {
            var mrk_pth = doc.GetMarkerPath();

            doc.PpBreakpoints =
               App.StateContainer.Params.Breakpoints.Items.
               Where(p => p.BreakpointPath == mrk_pth).
               Select(b => (GateDockDocuMarkerBreakpoint)b.MakeInstance(true)).ToArray();
            doc.PpBookmarks =
               myCloneBookmarks(
                  App.StateContainer.Params.Bookmarks.Items.
                  Where(p => p.BookmarkPath == mrk_pth));
            doc.OnBreakpointsChanged += TextControl_OnBreakpointsChanged;
            doc.OnBookmarksChanged += TextControl_OnBoomarksChanged;
            doc.OnSave += Txt_ctr_OnSave;
            doc.OnReopen += Doc_OnReopen;
            doc.OnBeforeReopen += Doc_OnBeforeReopen;
         }
      }

      private void Doc_OnBeforeReopen(object? sender, string oldContent, string newContent)
      {
         var doc = sender.ConvertOrCrash<IGateDockDocuText>();

         myDictBookmarks[doc] = myCloneBookmarks(doc);
      }

      private void Doc_OnReopen(object? sender, TxtLineComparer lineComparer)
      {
         var doc = sender.ConvertOrCrash<IGateDockDocuText>();
         var bks = myDictBookmarks.TryGetValue(doc, out var b) ? b : myCloneBookmarks(doc);

         myDictBookmarks.Remove(doc);

         var lst_bks = new List<GateDockDocuMarkerBookmark>();

         foreach (var bok in bks)
         {
            var sec =
               lineComparer.Sections.
               FirstOrDefault(s => s.LineIntervalOld1.Contains(bok.Line)).NnOrCrash();

            //check in which bookmark line are placed after change
            //just bookmarks in unmodified section are confirmed
            if (myIsSectionConfirmBookmarks(sec))
            {
               //confirm bookmark
               bok.Line = sec.LineIntervalNew1.From + (bok.Line - sec.LineIntervalOld1.From);
               lst_bks.Add(bok);
            }
         }

         doc.PpBookmarks = lst_bks.ToArray();
         mySaveBookmarks();
      }

      private void Txt_ctr_OnSave(object? sender, string path)
      {
         if (sender is IGateDockDocuText txt_ctr)
         {
            mySaveBookmarks();
            mySaveBreakpoints();
         }
      }

      private void MainForm_OnTabPageClosing(object? sender, GateDockTabPageCtrl? tabPage)
      {
         if (tabPage is IGateDockDocuText doc)
         {
            doc.OnBreakpointsChanged -= TextControl_OnBreakpointsChanged;
            doc.OnBookmarksChanged -= TextControl_OnBoomarksChanged;
            doc.OnSave -= Txt_ctr_OnSave;
            doc.OnReopen -= Doc_OnReopen;
            doc.OnBeforeReopen -= Doc_OnBeforeReopen;

            OnBreakpointsChanged?.Invoke(this, Breakpoints);
         }
      }

      private void TextControl_OnBreakpointsChanged(object? sender, GateDockDocuMarkerBreakpoint[]? breakpoints) =>
         OnBreakpointsChanged?.Invoke(this, Breakpoints);

      private void TextControl_OnBoomarksChanged(object? sender, GateDockDocuMarkerBookmark[]? bookmarks) => 
         OnBookmarksChanged?.Invoke(this, Bookmarks);
   }
}