namespace Gate.ToolsView.TextSearch
{
   public class TextSearchFindTaskStandard : ITextSearchFindTask
   {
      private Thread? myThreadFind;

      public event OnTextSearchFindTaskFinishedHandler? OnTextSearchFindTaskFinished;

      public TextSearchFindTaskStandard(TextSearchFindTaskType findTaskType) => FindTaskType = findTaskType;

      public TextSearchFindTaskType FindTaskType { get; }

      public bool IsFinished { get; private set; } = false;

      public void Abort() => myThreadFind?.Interrupt();

      public void Start(Action action)
      {
         var is_abo = false;

         myThreadFind = new Thread(() =>
         {
            try { action(); }
            catch (ThreadInterruptedException) { is_abo = true; }
            finally
            {
               IsFinished = true;
               OnTextSearchFindTaskFinished?.Invoke(is_abo);
            }
         });

         myThreadFind.Start();
      }
   }
}
