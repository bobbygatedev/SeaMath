using Gate.Tools.AppParams;

namespace Gate.ToolsView.MenuCommand
{
   public class CustomNoDefaultShortCutAppParamRecord : AppParam.Record
   {
      public static CustomNoDefaultShortCutAppParamRecord FromCmd(Cmd cmd)
      {
         var ist = new CustomNoDefaultShortCutAppParamRecord();

         ist.ShortCut = cmd.ShortCut;
         ist.ShortCut2 = cmd.ShortCut2;
         ist.Id = cmd.Id;

         return ist;
      }

      /// <summary>
      /// 
      /// </summary>
      public Keys ShortCut { get => myShortCut.Value; set => myShortCut.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public Keys ShortCut2 { get => myShortCut2.Value; set => myShortCut2.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public string? Id { get => myId.Value; set => myId.Value = value; }

      private readonly Simple<Keys> myShortCut = new Simple<Keys>();
      private readonly Simple<Keys> myShortCut2 = new Simple<Keys>();
      private readonly Simple<string> myId = new Simple<string>();
   }
}

