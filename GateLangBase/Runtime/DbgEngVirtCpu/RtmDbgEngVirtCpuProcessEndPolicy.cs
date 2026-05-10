namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Defines policy for ending process when one of its threads ends.
   /// </summary>
   public enum RtmDbgEngVirtCpuProcessEndPolicy
   {
      /// <summary>
      /// End the process when the main thread ends.
      /// </summary>
      end_process_on_main_thread_end = 0,

      /// <summary>
      /// End the process when all threads end.
      /// </summary>
      end_process_on_all_threads_end = 1,
   }
}
