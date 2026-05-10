using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// <br>Encapsulates a text search item ( a file ),which can be of two types:</br> 
   /// <br>- Associated to a control <seealso cref="OpenTextControl"/> NOT null).</br>
   /// <br>- Associated to an existing file path (<seealso cref="OpenTextControl"/> null).</br>
   /// </summary>
   public class TextSearchFile
   {
      private Control? myOpenTextControl = null;

      /// <summary>
      /// Constructor for existing file path, but not open in application.
      /// </summary>
      /// <param name="filePath"></param>
      /// <param name="findInfrastructure"></param>
      public TextSearchFile(string filePath, TextSearchInfrastructure findInfrastructure)
      {
         FindInfrastructure = findInfrastructure;
         FilePath = filePath ?? "";
      }

      /// <summary>
      /// Constructor for file open in application.
      /// </summary>
      /// <param name="docTextCtrl"></param>
      /// <param name="findInfrastructure"></param>
      public TextSearchFile(Control docTextCtrl, TextSearchInfrastructure findInfrastructure)
      {
         FindInfrastructure = findInfrastructure;
         FilePath = findInfrastructure.TxtCtrlInteraction.GetOpenFile(docTextCtrl);
         OpenTextControl = docTextCtrl;
      }

      public Control? OpenTextControl
      {
         get
         {
            if (myOpenTextControl != null)
            {
               if (FindInfrastructure.AllOpenTextControls.Contains(myOpenTextControl)) { return myOpenTextControl; }
               else { myOpenTextControl = null; }
            }

            if (FilePath != "" && FindInfrastructure.AllOpenFiles.Select(f => f.ToLower()).Contains(FilePath.Nn().ToLower()))
            {
               myOpenTextControl = FindInfrastructure.AllOpenTextControls.First(
                  c => FindInfrastructure.TxtCtrlInteraction.GetOpenFile(c).Nn().ToLower() == FilePath.Nn().ToLower());
            }

            return myOpenTextControl;
         }

         private set => myOpenTextControl = value;
      }

      public string? FilePath { get; }

      public string? CurrentFileBody
      {
         get
         {
            if (OpenTextControl == null)
            {
               if (FilePath.IsBlank())
               {
                  return null;
               }
               else
               {
                  try
                  {
                     using (var sr = new StreamReader(FilePath.Nn())) { return sr.ReadToEnd(); }
                  }
                  catch (IOException) { return null; }
               }
            }
            else { return FindInfrastructure.TxtCtrlInteraction.GetFileContent(OpenTextControl); }
         }
      }

      public TextSearchInfrastructure FindInfrastructure { get; }

      public Control? OpenAndSelect()
      {
         if (OpenTextControl == null) { OpenTextControl = FindInfrastructure.TxtAppInteraction.OpenFile(FilePath.Nn()); }

         return FindInfrastructure.SelectedTextControl = OpenTextControl;
      }

      public override string ToString() => !FilePath.IsBlank() ? FilePath.Nn() : "NoPath";

      public override bool Equals(object? obj)
      {
         if (obj is TextSearchFile fil)
         {
            if (FilePath != "") { return FilePath == fil.FilePath; }
            else if (IsValid) { return OpenTextControl == fil.OpenTextControl; }
            else { throw new Crash(); }
         }

         return false;
      }

      public bool IsValid => FilePath != "" && File.Exists(FilePath) || OpenTextControl != null;

      public override int GetHashCode() => FilePath != "" ? FilePath.Nn().GetHashCode() : OpenTextControl?.GetHashCode() ?? int.MinValue;

      public static bool operator ==(TextSearchFile? f1, TextSearchFile? f2)
      {
         if (ReferenceEquals(f1, f2)) { return true; }
         else if (f1 as object == null) { return false; }
         else { return f1.Equals(f2); }
      }

      public static bool operator !=(TextSearchFile? f1, TextSearchFile? f2) => !(f1 == f2);
   }
}

