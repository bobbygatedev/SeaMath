using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// Provides reference to notepad plus plus.
   /// </summary>
   public class NppRef
   {
      /// <summary>
      /// Contains the path to Notepad++ instance
      /// </summary>
      public const string NPP_PATH_ENV_VAR = "NOTEPAD_PP";

      /// <summary>
      /// Use FromEnvVar() instead!
      /// </summary>
      private NppRef() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="nppExePath"></param>
      public NppRef(string nppExePath) => NppExePath = nppExePath;

      /// <summary>
      /// 
      /// </summary>
      private class InnerTextOpener : ITextOpener
      {
         private NppRef myNppRef;

         public InnerTextOpener(NppRef aNotepadPlusPlusRef) => myNppRef = aNotepadPlusPlusRef;

         public void Open(Msg msg)
         {
            if (!msg.FilePath.IsBlank()) { myNppRef.Launch(msg); }
         }
      }

      /// <summary>
      /// <br> Returns an instance of 'NotepadPlusPlusRef' whose path is read from environment variable ''.</br>
      /// <br> If the env var is not defined ask for address and create the env var in the local machine. </br>
      /// </summary>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException">Not a valid path for notepad++ input.</exception>
      public static NppRef FromEnvVar() => FromEnvVar(NPP_PATH_ENV_VAR);

      /// <summary>
      /// <br> Returns an instance of 'NotepadPlusPlusRef' whose path is read from an environment variable.</br>
      /// <br> If the env var is not defined ask for address and create the env var in the local machine. </br>
      /// </summary>
      /// <param name="envVarName"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException">Not a valid path for notepad++ input.</exception>
      public static NppRef FromEnvVar(string envVarName)
      {
         var pth = System.Environment.GetEnvironmentVariable(envVarName);

         if (pth != null && pth != "" && File.Exists(pth)) { return new NppRef(pth); }
         else
         {
            var require_input_box = new InputBoxForm();

            require_input_box.MthSetValidator(new InputBoxForm.ValidatorForFilePath());

            if (require_input_box.ShowDialog() == DialogResult.OK)
            {
               //save on user var!
               Environment.SetEnvironmentVariable(envVarName, require_input_box.PpUserInputText, EnvironmentVariableTarget.Process);
               Environment.SetEnvironmentVariable(envVarName, require_input_box.PpUserInputText, EnvironmentVariableTarget.User);

               return new NppRef(require_input_box.PpUserInputText);
            }
            else { throw new Gate.Tools.ToolsException("Not a valid notepad++ path found!"); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string? NppExePath { get; private set; }

      /// <summary>
      /// Returns an instance of 'TextMessageListControl.ITextOpener' using this notepad++ path.
      /// </summary>
      /// <returns></returns>
      public ITextOpener GetTextOpener() => new InnerTextOpener(this);

      /// <summary>
      /// Returns Notepad++ cmdline for text message with file reference.
      /// </summary>
      /// <param name="msg">Text reference to be open.</param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException">Referenced path not defined or not existing in text ref!</exception>
      public string GetNppCmdLine(Msg msg)
      {
         if (msg?.FilePath == null || msg.FilePath.IsBlank()) { throw new Gate.Tools.ToolsException("Referenced path is not defined!"); }
         else if (!File.Exists(msg.FilePath)) { throw new Gate.Tools.ToolsException($"Text Reference File {msg.FilePath} doesn't exist!"); }
         else { return $@"""{NppExePath}"" ""{msg.FilePath}"" -n{msg.Token?.From?.Primitive?.Line} -c{msg.Token?.From?.Primitive?.Col}"; }
      }

      /// <summary>
      /// Returns Notepad++ cmdline for text file to open.
      /// </summary>
      /// <param name="fileToOpen"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException">Path not defined or not existing!</exception>
      public string GetNppCmdLine(string fileToOpen)
      {
         if (fileToOpen.ExtTrim() == "") { throw new Gate.Tools.ToolsException("File to open not defined!"); }
         else if (!File.Exists(fileToOpen)) { throw new Gate.Tools.ToolsException($"File {fileToOpen} doesn't exist!"); }
         else { return string.Format(@"""{0}"" ""{1}""", NppExePath, fileToOpen); }
      }

      /// <summary>
      /// Launches Notepad++ or open a text file window if Notepad++ has been already launched.
      /// </summary>
      /// <param name="msg">Reference to open.</param>
      /// <exception cref="Gate.Tools.ToolsException">Referenced path not defined or not existing in text ref!</exception>
      public void Launch(Msg msg) => myLaunch(GetNppCmdLine(msg));

      /// <summary>
      /// Returns path to Notepad++ executable.
      /// </summary>
      /// <returns></returns>
      public override string ToString() => NppExePath.Nn();

      private static void myLaunch(string cmdLine)
      {
         int idx = cmdLine.IndexOf(' ');

         if (idx != -1)
         {
            var psi = new ProcessStartInfo();

            psi.FileName = cmdLine.Substring(0, idx).Trim();
            psi.Arguments = cmdLine.Substring(idx + 1).Trim();

            Process.Start(psi);
         }
         else { throw new ToolsException(); }
      }
   }
}