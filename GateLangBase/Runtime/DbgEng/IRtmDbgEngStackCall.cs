using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// Interface for a <see cref="IRtmDbgEng"/> call read only (is not able to simulate a stack but just read it eg from gdb)
   /// </summary>
   public interface IRtmDbgEngStackCall
   {
      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngPoint? DbgPointCurrent { get; }

      /// <summary>
      /// <br> <see cref="RtmObj"/> instance in this frame (from stack function stack frame to next one)</br>
      /// <br> NOTICE anonimous object are included</br>
      /// </summary>
      RtmObj[] ObjInStackOnly { get; }

      /// <summary>
      /// <br> Comprise <see cref="ObjInStackOnly"/> and all static objects in nested function(s).</br>
      /// <br> eg void func(int p1) { void nested(void) { static int v1 } } </br>
      /// <br> in this case <see cref="ObjInStackOnly"/> contains p1, while <see cref="ObjLocalsAll"/> contains p1,v1  </br>
      /// <br> NOTICE anonimous object are omiited</br>
      /// </summary>
      RtmObj[] ObjLocalsAll { get; }

      /// <summary>
      /// <see cref="ObjLocalsAll"/> + globals + static objects from the current function module.
      /// </summary>
      RtmObj[] ObjAll { get; }

      /// <summary>
      /// 
      /// </summary>
      string? FunctionName { get; }

      /// <summary>
      /// 
      /// </summary>
      string? FunctionInfo{ get; }
   }
}