using Gate.CLanguage.Source;
using Gate.LangBase;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.SeaMath
{
   public static class SeaMathMessages
   {
      private static CompilerMessagesTools myMsgTools = new CompilerMessagesTools("SEA");

      public enum IdEnum
      {
         sea000_inf_no_error = 0,

         /// <summary>
         /// 
         /// </summary>
         sea001_err_no_entry_point_found = 1,

         /// <summary>
         /// sea can't appear along with others type specifiers (eg 'sea int' is not allowed).
         /// </summary>
         sea002_no_extra_specifiers_with_sea = 2,

         /// <summary>
         /// sea variable declarator can't be initialised (expression shall be used)
         /// </summary>
         sea003_no_sea_initialisation = 3,

         /// <summary>
         /// Can't assign a rtm array initer to pure C object (eg int a;  a = { 1 , 2 }; )
         /// </summary>
         sea004_no_rtm_array_to_c_object = 4,

         /// <summary>
         /// sea type can be pure scalar only (sea v[2], sea* p are prohibited.
         /// </summary>
         sea005_sea_to_be_pure_scalar = 5,
      }

      public static Msg M001_No_EntryPointFound(CSource sourceFile)
      {
         var frg = TxtTokenConst.FromFromLen(sourceFile.StorePrimitive ?? throw new Crash(), 0, 1);

         return myMsgTools.MakeMsg(MsgType.error, IdEnum.sea001_err_no_entry_point_found, frg);
      }

      public static Msg M002_NoExtraSpecifiersWithSea(TxtToken? token) =>
         myMsgTools.MakeMsg(MsgType.error, IdEnum.sea002_no_extra_specifiers_with_sea, token);

      public static Msg M003_NoSeaInitialisation(TxtToken? token) =>
        myMsgTools.MakeMsg(MsgType.error, IdEnum.sea003_no_sea_initialisation, token);

      public static Msg M004_NoRtmArrayToCObject(TxtToken? token) =>
         myMsgTools.MakeMsg(MsgType.error, IdEnum.sea004_no_rtm_array_to_c_object, token);

      public static Msg M005_SeaToBePureScalar(TxtToken? token) =>
         myMsgTools.MakeMsg(MsgType.error, IdEnum.sea005_sea_to_be_pure_scalar, token);
   }
}
