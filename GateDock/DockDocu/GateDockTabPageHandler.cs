using Gate.Dock.DockFactories;
using Gate.Dock.DockTab;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extensions;
using static Gate.Dock.DockDocu.GateDockDocuCloseForm;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// Provides an abstract base class for handling document-related operations within a tabbed interface.
   /// </summary>
   /// <remarks>This class defines the core functionality for managing documents, including opening, saving,
   /// and closing document tabs. It also provides mechanisms for interacting with document factories and handling user
   /// interactions such as undo/redo operations. Derived classes should implement the abstract members to provide
   /// specific behavior for document handling.</remarks>
   public abstract class GateDockTabPageHandler
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="GateDockTabPageHandler"/> class.
      /// </summary>
      /// <remarks>This constructor is protected and intended to be used by derived classes.</remarks>
      protected GateDockTabPageHandler() { }

      /// <summary>
      /// Gets the default factory for creating documentation objects specific to gate dock operations.
      /// </summary>
      public abstract GateDockDocuFactory? DefaultDocuFactory { get; }

      /// <summary>
      /// Gets the collection of document factories associated with the gate dock.
      /// </summary>
      public abstract GateDockDocuFactory[] DocuFactories { get; }

      /// <summary>
      /// Gets or sets the directory path of the last accessed file.
      /// </summary>
      public abstract string? LastFileDir { get; set; }

      /// <summary>
      /// Gets a file filter string that combines the filters associated with all document factories and includes a
      /// default "All Files" filter.
      /// </summary>
      /// <remarks>This property aggregates the file filters from all available document factories and
      /// appends a generic "All Files" filter at the end. It is useful for scenarios where a unified filter string is
      /// needed for file selection dialogs.</remarks>
      public string AllDocuFactoriesFileFilter => string.Join("|", DocuFactories.Select(f => f.AssociatedFilter).Concat(new string[] { "All Files(*.*)|*.*" }));

      /// <summary>
      /// Prompts the user to confirm closing the specified tab pages, handling both document and non-document tabs.
      /// </summary>
      /// <remarks>This method distinguishes between document tabs (implementing <see
      /// cref="IGateDockDocu"/>) and non-document tabs. Non-document tabs are handled separately, while document tabs
      /// are processed to determine if they need to be saved before closing. If the user cancels the operation, no tabs
      /// are closed.</remarks>
      /// <param name="mainForm">The main form that manages the tab pages.</param>
      /// <param name="tabPages">The tab pages to be considered for closing. Can include both document and non-document tabs.</param>
      /// <returns>A <see cref="SaveResultEnum"/> value indicating the result of the close operation: <see
      /// cref="SaveResultEnum.save"/>, <see cref="SaveResultEnum.dontSave"/>, or <see cref="SaveResultEnum.cancel"/>.</returns>
      public SaveResultEnum AskForClose(GateDockMainForm mainForm, params GateDockTabPageCtrl[] tabPages)
      {
         var tab_pgs_doc = tabPages.OfType<IGateDockDocu>().ToArray();
         var tab_pgs_no_doc = tabPages.Except(tab_pgs_doc.Cast<GateDockTabPageCtrl>()).ToArray();

         myActionOnTabPageNoDocClose(mainForm, tab_pgs_no_doc);

         var res = DocuClosing(mainForm, tab_pgs_doc, true);

         if (res != SaveResultEnum.cancel)
         {
            foreach (var ctr in tab_pgs_doc.Cast<GateDockTabPageCtrl>()) { mainForm.MthTabPageForceClose(ctr); }
         }

         return res;
      }

      /// <summary>
      /// Launches a file open dialog and processes the selected file if it exists.
      /// </summary>
      /// <remarks>If a valid file path is selected, the method attempts to open the file.  If the file does
      /// not exist, a <see cref="Crash"/> exception is thrown.</remarks>
      /// <param name="mainForm">The main form instance that serves as the parent for the dialog.</param>
      /// <exception cref="Crash">Thrown if the selected file path does not exist.</exception>
      public void LaunchOpenDialog(GateDockMainForm mainForm)
      {
         var pth = myDoOpenDocuDialog(mainForm);

         if (pth != null)
         {
            if (File.Exists(pth)) { OpenPath(mainForm, pth); }
            else { throw new Crash(); }
         }
      }

      /// <summary>
      /// Opens a document at the specified path and displays it in a new or existing tab within the main form.
      /// </summary>
      /// <remarks>If the document is already open, the existing tab page is reused. If the file extension
      /// is not recognized, the default document factory is used to create the tab page control. The method ensures
      /// that the main form's current tab is set to the newly opened or reused document tab.</remarks>
      /// <param name="mainForm">The main form that hosts the tab control where the document will be displayed. Cannot be <see
      /// langword="null"/>.</param>
      /// <param name="path">The file path of the document to open. Must be a valid file path and cannot be <see langword="null"/> or
      /// empty.</param>
      /// <param name="tabCtrl">An optional tab control to which the document tab will be added. If <see langword="null"/>, the document will
      /// be added to the main form's default tab control.</param>
      /// <returns>A <see cref="GateDockTabPageCtrl"/> instance representing the tab page control for the opened document.
      /// Returns an existing tab page if the document is already open; otherwise, creates and returns a new one.</returns>
      /// <exception cref="Crash">Thrown if an unexpected error occurs while opening the document or adding it to the tab control.</exception>
      public GateDockTabPageCtrl? OpenPath(GateDockMainForm mainForm, string path, GateDockTabCtrl? tabCtrl = null)
      {
         var doc_ctr = GetOpenPath(mainForm, path);

         mainForm.MthInvoke(new Action(() =>
         {
            try
            {
               if (doc_ctr == null)
               {
                  var ext = Path.GetExtension(path);
                  var fac = DocuFactories.FirstOrDefault(f => f.IsExtensionContained(ext)) ?? DefaultDocuFactory ?? throw new Crash();

                  doc_ctr = fac.MakeTabPageControl(mainForm);
                  LastFileDir = Path.GetDirectoryName(path);
                  ((IGateDockDocu)doc_ctr).MthOpenFile(path);
                  mainForm.MthTabPageAdd(doc_ctr, tabCtrl);
               }
               else if (tabCtrl != null) { mainForm.MthTabPageAdd(doc_ctr, tabCtrl); }

               mainForm.PpTabPageCurrent = doc_ctr;
            }
            catch (Exception exc) { throw new Crash(exc); }
         }));

         return doc_ctr;
      }

      /// <summary>
      /// Undoes the last action performed in the currently selected document.
      /// </summary>
      /// <remarks>This method attempts to undo the last action in the document currently selected within
      /// the provided <paramref name="mainForm"/>. If the active document does not support undo operations, this method
      /// has no effect.</remarks>
      /// <param name="mainForm">The main form containing the currently active document. This parameter cannot be null.</param>
      public void UndoSelected(GateDockMainForm mainForm)
      {
         if (mainForm.PpTabPageCurrent is IGateDockDocu txt_ctr) { txt_ctr.MthUndo(); }
      }

      /// <summary>
      /// Redoes the last undone action in the currently selected document.
      /// </summary>
      /// <remarks>This method attempts to redo the last undone action in the document currently selected in
      /// the provided <paramref name="mainForm"/>. If the active document does not support redo operations, this method
      /// has no effect.</remarks>
      /// <param name="mainForm">The main form containing the currently active document.</param>
      public void RedoSelected(GateDockMainForm mainForm)
      {
         if (mainForm.PpTabPageCurrent is IGateDockDocu txt_ctr) { txt_ctr.MthRedo(); }
      }

      /// <summary>
      /// Saves all modified documents associated with the specified main form.
      /// </summary>
      /// <remarks>This method identifies all documents within the provided <paramref name="mainForm"/> that
      /// implement <see cref="IGateDockDocu"/> and have been marked as modified. It then saves these
      /// documents.</remarks>
      /// <param name="mainForm">The main form containing the documents to be saved. This parameter cannot be <see langword="null"/>.</param>
      public void SaveAll(GateDockMainForm mainForm) => mySaveDocus(
         mainForm, mainForm.PpTabPagesAll.OfType<IGateDockDocu>().Where(d => d.PpIsModified).ToArray());

      /// <summary>
      /// Retrieves the open tab page control associated with the specified path.
      /// </summary>
      /// <param name="mainForm">The main form containing the collection of tab pages to search.</param>
      /// <param name="path">The path to match against the document paths of the tab pages. The comparison is case-insensitive.</param>
      /// <returns>The <see cref="GateDockTabPageCtrl"/> instance corresponding to the specified path,  or <see langword="null"/>
      /// if no matching tab page is found.</returns>
      public GateDockTabPageCtrl? GetOpenPath(GateDockMainForm mainForm, string path) =>
         mainForm.PpTabPagesAll.OfType<IGateDockDocu>().FirstOrDefault(d => d.PpDocuPath.Nn().ToLower() == path.ToLower()) as GateDockTabPageCtrl;

      /// <summary>
      /// Saves the current document associated with the specified main form.
      /// </summary>
      /// <remarks>This method checks if the current tab page in the provided main form implements the <see
      /// cref="IGateDockDocu"/> interface. If so, it saves the document represented by the current tab page.</remarks>
      /// <param name="mainForm">The main form containing the current document to be saved. This parameter cannot be null.</param>
      public void SaveCurrentDoc(GateDockMainForm mainForm)
      {
         if (mainForm.PpTabPageCurrent is IGateDockDocu)
         {
            mySaveDocus(mainForm, new IGateDockDocu[] { (IGateDockDocu)mainForm.PpTabPageCurrent });
         }
      }

      /// <summary>
      /// Saves the currently selected document in the specified main form.
      /// </summary>
      /// <remarks>This method attempts to save the document currently selected in the provided main form. 
      /// If the selected document implements <see cref="IGateDockDocu"/>, the save operation is performed;  otherwise,
      /// the method returns <see langword="false"/>.</remarks>
      /// <param name="mainForm">The main form containing the document to be saved. This parameter cannot be null.</param>
      /// <returns><see langword="true"/> if the currently selected document was successfully saved;  otherwise, <see
      /// langword="false"/> if no valid document is selected.</returns>
      public bool SaveAsSelected(GateDockMainForm mainForm)
      {
         if (mainForm.PpTabPageCurrent is IGateDockDocu doc_ctr)
         {
            mySaveDocuAs(mainForm, doc_ctr);

            return true;
         }
         else
         {
            return false;
         }
      }

      /// <summary>
      /// Creates a new document and adds it to the specified main form's tab control.
      /// </summary>
      /// <remarks>This method initializes a new document using the specified or default document factory, 
      /// populates it, and adds it as a new tab page to the main form.</remarks>
      /// <param name="mainForm">The main form to which the new document will be added. This parameter cannot be null.</param>
      /// <param name="docuFactory">An optional factory used to create the document. If not provided, a default factory is used.</param>
      public virtual void FileNew(GateDockMainForm mainForm, GateDockDocuFactory? docuFactory = null)
      {
         var doc_fac = docuFactory ?? DefaultDocuFactory ?? throw new Crash();
         var doc_ctr = doc_fac.MakeTabPageControl(mainForm);

         myPopulateDocu(mainForm, doc_ctr, doc_fac);
         mainForm.MthTabPageAdd(doc_ctr);
      }

      /// <summary>
      /// Handles the closing process for documents, determining whether any unsaved changes need to be addressed.
      /// </summary>
      /// <remarks>This method evaluates the provided documents to determine if any have unsaved changes or
      /// require saving due to missing file paths. If such documents are found, a save dialog is displayed to the user.
      /// Otherwise, the method returns a default save result.</remarks>
      /// <param name="mainForm">The main form of the application, used to display dialogs or interact with the user interface.</param>
      /// <param name="docusWithContent">An array of documents to evaluate for unsaved changes or missing file paths.</param>
      /// <param name="areFilesWithNoPathToSave">A value indicating whether documents without a file path should be considered for saving if they contain
      /// content.</param>
      /// <returns>A <see cref="SaveResultEnum"/> value indicating the result of the save operation.  Returns <see
      /// cref="SaveResultEnum.save"/> if no documents require saving, or the result of the save dialog if saving is
      /// needed.</returns>
      public virtual SaveResultEnum DocuClosing(GateDockMainForm mainForm, IGateDockDocu[] docusWithContent, bool areFilesWithNoPathToSave)
      {
         var dcs_sav = docusWithContent.Where(c => c.PpDocuPath != "" ? c.PpIsModified : (areFilesWithNoPathToSave && c.PpIsDocuNotEmpty)).ToArray();

         return dcs_sav.Length > 0 ? mySaveGroupDialog(mainForm, dcs_sav) : SaveResultEnum.save;
      }

      /// <summary>
      /// Displays an open file dialog to allow the user to select a file and returns the selected file path.
      /// </summary>
      /// <remarks>The dialog initializes with the last accessed directory and filters files based on the
      /// supported formats. If the selected file does not exist, an error message is displayed, and <see
      /// langword="null"/> is returned.</remarks>
      /// <param name="mainForm">The main form of the application, used as the owner of the dialog.</param>
      /// <returns>The full path of the selected file if the user selects a valid file; otherwise, <see langword="null"/>.</returns>
      protected virtual string? myDoOpenDocuDialog(GateDockMainForm mainForm)
      {
         using (var dlg = new OpenFileDialog())
         {
            dlg.InitialDirectory = LastFileDir;
            dlg.Filter = AllDocuFactoriesFileFilter;
            dlg.FilterIndex = DocuFactories.Length + 1;//index starting from 1, last idx Len+1 (ie *.*)
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
               //Get the path of specified file
               var pth = dlg.FileName;

               if (File.Exists(pth)) { return pth; }
               else { MessageBox.Show(string.Format("{0} doesn't exist!", pth)); }
            }
         }

         return null;
      }

      /// <summary>
      /// Displays a Save File dialog to allow the user to save the specified document to a file.
      /// </summary>
      /// <remarks>If the user selects a file and confirms the dialog, the document is saved to the
      /// specified file path.  If the selected file extension matches the current document factory, the document's path
      /// is updated,  and its modified state is reset. Otherwise, a new tab page is created for the document using the
      /// appropriate factory,  and the document is reopened in the new tab.  The method also updates the last used
      /// directory for file operations and ensures the document is saved using the  appropriate factory for the
      /// selected file type.</remarks>
      /// <param name="mainForm">The main form of the application, used to manage document and tab interactions.</param>
      /// <param name="docu">The document to be saved, which must implement the <see cref="IGateDockDocu"/> interface.</param>
      protected virtual void mySaveDocuAs(GateDockMainForm mainForm, IGateDockDocu docu)
      {
         using (var dlg = new SaveFileDialog())
         {
            var tab_pag = (GateDockTabPageCtrl)docu;

            dlg.InitialDirectory = LastFileDir;
            dlg.Filter = AllDocuFactoriesFileFilter;

            //index starting from 1, last idx Len+1 (ie *.*)
            dlg.FilterIndex = DocuFactories.ToList().IndexOf((tab_pag.PpFactory as GateDockDocuFactory).NnOrCrash());
            dlg.RestoreDirectory = true;

            if (docu.PpDocuPath != null && File.Exists(docu.PpDocuPath)) { dlg.FileName = docu.PpDocuPath; }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
               var new_fac = DocuFactories.FirstOrDefault(f =>
                  f.IsExtensionContained(Path.GetExtension(dlg.FileName))) ?? DefaultDocuFactory ?? throw new Crash();

               if (new_fac.CtrlGuid == tab_pag.PpFactory.NnOrCrash().CtrlGuid)
               {
                  docu.PpDocuPath = dlg.FileName;
                  docu.PpIsModified = false;
                  tab_pag.PpFactory = new_fac;
               }
               else
               {
                  var new_tab_pag = new_fac.MakeTabPageControl(mainForm);
                  var tab = tab_pag.PpParentTab;

                  myPopulateDocu(mainForm, new_tab_pag, new_fac);
                  ((IGateDockDocu)new_tab_pag).PpDocuPath = dlg.FileName;
                  mainForm.MthDocuReopen(tab_pag, new_tab_pag);
                  mainForm.PpTabPageCurrent = new_tab_pag;
               }

               LastFileDir = Path.GetDirectoryName(dlg.FileName);
               docu.MthSaveFile(dlg.FileName);
            }
         }
      }

      /// <summary>
      /// Performs an action to close specified tab pages in the main form if their associated factories allow closure.
      /// </summary>
      /// <remarks>For each tab page in the provided array, this method checks if the associated factory
      /// allows the tab page to be closed by invoking <see cref="GateDockTabPageCtrl.PpFactory.AskForClose"/>. If the
      /// factory permits closure, the tab page is forcibly closed using <see
      /// cref="GateDockMainForm.MthTabPageForceClose"/>.</remarks>
      /// <param name="mainForm">The main form containing the tab pages to be processed.</param>
      /// <param name="tabPages">An array of tab pages to evaluate for closure. Only tab pages with a non-null factory  (<see
      /// cref="GateDockTabPageCtrl.PpFactory"/>) are considered.</param>
      protected virtual void myActionOnTabPageNoDocClose(GateDockMainForm mainForm, GateDockTabPageCtrl[] tabPages)
      {
         foreach (var tab_pag in tabPages.Where(p => p.PpFactory != null))
         {
            if (tab_pag.PpFactory.NnOrCrash().AskForClose())
            {
               mainForm.MthTabPageForceClose(tab_pag);
            }
         }
      }

      /// <summary>
      /// Displays a dialog to prompt the user for actions on a group of documents and processes the user's choice.
      /// </summary>
      /// <remarks>If the user selects <see cref="SaveResultEnum.save"/>, the method will invoke the save
      /// operation for the provided documents.</remarks>
      /// <param name="mainForm">The main form that serves as the owner of the dialog.</param>
      /// <param name="docsToSave">An array of documents to be saved or processed based on the user's selection.</param>
      /// <returns>A <see cref="SaveResultEnum"/> value indicating the user's choice:  <see cref="SaveResultEnum.save"/> if the
      /// documents should be saved,  <see cref="SaveResultEnum.discard"/> if the documents should be discarded,  or
      /// <see cref="SaveResultEnum.cancel"/> if the operation was canceled.</returns>
      protected virtual SaveResultEnum mySaveGroupDialog(GateDockMainForm mainForm, IGateDockDocu[] docsToSave)
      {
         var frm = new GateDockDocuCloseForm();

         frm.PpDocs = docsToSave;
         frm.ShowDialog(mainForm);

         if (frm.PpSaveResult == SaveResultEnum.save)
         {
            mySaveDocus(mainForm, docsToSave);
         }

         return frm.PpSaveResult;
      }

      /// <summary>
      /// Generates a unique name for a new file that does not conflict with existing document names.
      /// </summary>
      /// <param name="mainForm">The main form containing the collection of documents to check for name conflicts.</param>
      /// <returns>A unique string in the format "NewFileX", where X is an integer starting from 1 and incremented until a name
      /// is found that does not conflict with existing document names.</returns>
      /// <exception cref="Crash">Thrown if no unique name can be generated, which is highly unlikely under normal circumstances.</exception>
      protected virtual string myGetUniqueName(GateDockMainForm mainForm)
      {
         var fil_nms = mainForm.PpTabPagesAll.OfType<IGateDockDocu>().Select(t => t.PpDocuName).ToArray();

         for (int i = 1; i < int.MaxValue; i++)
         {
            var nam = "NewFile" + i;

            if (fil_nms.Nn().All(n => n.ToLower() != nam.ToLower())) { return nam; }
         }

         throw new Crash();//nearly impossible
      }

      private void mySaveDocus(GateDockMainForm mainForm, IGateDockDocu[] selectedControls)
      {
         var sas_cts = selectedControls.Where(c => c.PpIsDocuNotEmpty && c.PpDocuPath == "").ToArray();
         var sav_cts = selectedControls.Where(c => c.PpIsDocuNotEmpty && c.PpDocuPath != "" && c.PpIsModified).ToArray();

         foreach (var ctr in sav_cts) { ctr.MthSaveFile(ctr.PpDocuPath.Nn()); }

         foreach (var ctr in sas_cts) { mySaveDocuAs(mainForm, ctr); }
      }

      /// <summary>
      /// Populate control with main properties
      /// </summary>
      /// <param name="mainForm"></param>
      /// <param name="tabPage"></param>
      /// <param name="docuFactory"></param>
      private void myPopulateDocu(GateDockMainForm mainForm, GateDockTabPageCtrl tabPage, GateDockDocuFactory docuFactory)
      {
         tabPage.PpFactory = docuFactory;
         tabPage.PpSkin = mainForm.PpSkin;
         ((IGateDockDocu)tabPage).PpIsReadOnly = false;
         ((IGateDockDocu)tabPage).PpDocuName = myGetUniqueName(mainForm);
      }
   }
}
