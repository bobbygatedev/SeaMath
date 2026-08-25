using Gate.Dock.DockApp;
using Gate.Dock.DockTab;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// Handler class for bookmarks and breakpoints.
   /// </summary>
   public class GateDockDocuMarkerHandler
   {
      public event OnBreakpointsChangedHandler? OnBreakpointsChanged;
      public event OnBoomarksChangedHandler? OnBoomarksChanged;

      private GateDockDocuMarkerBookmark? myBookmarkCurrent;

      public GateDockDocuMarkerHandler(GateDockApp app)
      {
         app.MainForm.OnTabPageOpen += MainForm_OnTabPageOpen;
         app.MainForm.OnTabPageClosing += MainForm_OnTabPageClosing;
         App = app;
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockDocuMarkerBookmark? BookmarkCurrent
      {
         get => myBookmarkCurrent;

         private set
         {
            var old_gus = Bookmarks.Select(b => b.Guid).ToArray();
            var old_bks = Bookmarks.Cast<GateDockDocuMarkerBookmark?>().ToArray();
            var bok_idx = value != null ? Bookmarks.ToList().IndexOf(value) : -1;
            var all_doc_txt_bms = App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>().SelectMany(d => Bookmarks).ToArray();

            for (int i = 0; i < old_bks.Length; i++)
            {
               if (!all_doc_txt_bms.Contains(old_bks[i]) || !File.Exists(old_bks[i]?.BookmarkPath)) { old_bks[i] = null; }
            }

            var cnt = 0;

            for (int i = bok_idx; cnt < old_bks?.Length; i++, cnt++)
            {
               i = i % old_bks.Length;

               if (old_bks?[i] != null)
               {
                  myBookmarkCurrent = old_bks[i];

                  var txt_ctr = App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>().
                     FirstOrDefault(d => d.PpBookmarks?.Contains(myBookmarkCurrent) ?? false);

                  if (txt_ctr == null)
                  {
                     if (File.Exists(myBookmarkCurrent?.BookmarkPath))
                     {
                        txt_ctr =
                           App.MainForm.PpDocuHandler.OpenPath(App.MainForm, myBookmarkCurrent.BookmarkPath) as IGateDockDocuText ??
                           throw new Crash();
                     }
                     else { break; }
                  }

                  App.MainForm.PpTabPageCurrent = (GateDockTabPageCtrl)txt_ctr;
                  txt_ctr.PpCurrLine = old_bks?[i]?.Line ?? throw new Crash();
                  txt_ctr.PpCurrCol = 1;

                  return;
               }
            }

            myBookmarkCurrent = null;
         }
      }

      public GateDockDocuMarkerBreakpoint[] Breakpoints
      {
         get => App.StateContainer.Params.Breakpoints.Items.ToArray();

         private set
         {
            App.StateContainer.Params.Breakpoints.Clear();

            foreach (var bok in value ?? []) { App.StateContainer.Params.Breakpoints.AddParam(bok); }
         }
      }

      public GateDockDocuMarkerBookmark[] Bookmarks
      {
         get => App.StateContainer.Params.Bookmarks.Items.ToArray();

         private set
         {
            App.StateContainer.Params.Bookmarks.Clear();

            foreach (var bok in value ?? []) { App.StateContainer.Params.Bookmarks.AddParam(bok); }
         }
      }

      public GateDockApp App { get; }

      public void BookmarkToggle()
      {
         if (App.MainForm.PpTabPageCurrent is IGateDockDocuText txt_ctr) { txt_ctr.MthToggleBookmark(); }
      }

      public void BookmarkClearAll()
      {
         foreach (var txt_ctr in App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>()) { txt_ctr.PpBookmarks = null; }

         Bookmarks = [];
      }

      public void BookmarkMoveToNext()
      {
         if (BookmarkCurrent == null) { BookmarkCurrent = Bookmarks?.FirstOrDefault(); }
         else
         {
            if (Bookmarks?.Length > 0)
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
         if (BookmarkCurrent == null) { BookmarkCurrent = Bookmarks?.FirstOrDefault(); }
         else
         {
            if (Bookmarks?.Length > 0)
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
         if (App.MainForm.PpTabPageCurrent is IGateDockDocuText txt_ctr) { txt_ctr.MthToggleBreakpoint(); }
      }

      public void BreakpointDeleteAll()
      {
         foreach (var txt_ctr in App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>()) { txt_ctr.PpBreakpoints = null; }

         Breakpoints = [];
         OnBreakpointsChanged?.Invoke(this, Breakpoints);
      }

      public void Load() => myCheckExistance();

      public void ActionOnClosing()
      {
         myCheckExistance();

         //tododo
         //foreach (var txt_ctr in App.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>()) { myDocuTextSaveMarkers(txt_ctr); }
      }

      public static string? GetMarkerPath(IGateDockDocuText docuText) =>
         docuText.PpDocuPath.IsBlank() ? docuText.PpDocuName : docuText.PpDocuPath;

      public static bool IsMarkerPathExisting(string markerPath, GateDockApp app) => Path.IsPathRooted(markerPath) ?
         File.Exists(markerPath) :
         app.MainForm.PpTabPagesAll.OfType<IGateDockDocuText>().Any(t => t.PpDocuName == markerPath);

      private void myCheckExistance()
      {
         Bookmarks = Bookmarks.Where(b => IsMarkerPathExisting(b.BookmarkPath.ExtTrim(), App)).ToArray();
         Breakpoints = Breakpoints.Where(b => IsMarkerPathExisting(b.BreakpointPath.ExtTrim(), App)).ToArray();
      }

      private void myDocuTextSaveMarkers(IGateDockDocuText docuText)
      {
         var mrk_pth = GetMarkerPath(docuText);
         var brk_frs = Breakpoints.FirstOrDefault(b => b.BreakpointPath == mrk_pth);
         var lst_brk = Breakpoints.ToList();

         if (brk_frs != null)
         {
            var idx = lst_brk.IndexOf(brk_frs);

            lst_brk.RemoveAll(b => b.BreakpointPath == mrk_pth);
            lst_brk.InsertRange(idx, docuText.PpBreakpoints ?? []);
         }
         else { lst_brk.AddRange(docuText.PpBreakpoints ?? []); }

         Breakpoints = lst_brk.ToArray();

         var bok_frs = Bookmarks.FirstOrDefault(b => b.BookmarkPath == mrk_pth);
         var lst_bok = Bookmarks.ToList();

         if (bok_frs != null)
         {
            var idx = lst_bok.IndexOf(bok_frs);

            lst_bok.RemoveAll(b => b.BookmarkPath == mrk_pth);
            lst_bok.AddRange(docuText.PpBookmarks ?? []);
         }
         else
         {
            lst_bok.AddRange(docuText.PpBookmarks ?? []);
         }

         Bookmarks = lst_bok.ToArray();
      }

      private void MainForm_OnTabPageOpen(object? sender, GateDockTabPageCtrl? tabPage)
      {
         if (tabPage is IGateDockDocuText txt_ctr)
         {
            var mrk_pth = GetMarkerPath(txt_ctr);

            txt_ctr.PpBreakpoints = (Breakpoints ?? []).Where(p => p.BreakpointPath == mrk_pth).ToArray();
            txt_ctr.PpBookmarks = (Bookmarks ?? []).Where(p => p.BookmarkPath == mrk_pth).ToArray();
            txt_ctr.OnBreakpointsChanged += TextControl_OnBreakpointsChanged;
            txt_ctr.OnBoomarksChanged += TextControl_OnBoomarksChanged;
            txt_ctr.OnSave += Txt_ctr_OnSave;
         }
      }

      private void Txt_ctr_OnSave(object? sender, string path)
      {
         if (sender is IGateDockDocuText txt_ctr)
         {
            myDocuTextSaveMarkers(txt_ctr);
         }
      }

      private void MainForm_OnTabPageClosing(object? sender, GateDockTabPageCtrl? tabPage)
      {
         if (tabPage is IGateDockDocuText txt_ctr)
         {
            txt_ctr.OnBreakpointsChanged -= TextControl_OnBreakpointsChanged;
            txt_ctr.OnBoomarksChanged -= TextControl_OnBoomarksChanged;
            txt_ctr.OnSave -= Txt_ctr_OnSave;

            //tododo save on doc save
            //myDocuTextSaveMarkers(txt_ctr);
            OnBreakpointsChanged?.Invoke(this, Breakpoints);
         }
      }

      private void TextControl_OnBreakpointsChanged(object? sender, GateDockDocuMarkerBreakpoint[]? breakpoints)
      {
         //this causes update of prop 'Breakpoints'
         //myDocuTextSaveMarkers(sender as GateDockDocuTextCtrl ?? throw new Crash()); //tododo
         OnBreakpointsChanged?.Invoke(this, Breakpoints);
      }

      private void TextControl_OnBoomarksChanged(object? sender, GateDockDocuMarkerBookmark[]? bookmarks)
      {
         //this causes update of prop 'Bookmarks'
         //myDocuTextSaveMarkers(sender as GateDockDocuTextCtrl ?? throw new Crash()); //tododo
         OnBoomarksChanged?.Invoke(this, Bookmarks);
      }
   }
}