using System;

namespace Gate.ToolsView.TextSearch
{
   public delegate void OnTextSearchFindTaskFinishedHandler(bool isAborted);

   public interface ITextSearchFindTask
   {
      event OnTextSearchFindTaskFinishedHandler OnTextSearchFindTaskFinished;

      void Start(Action action);

      void Abort();

      TextSearchFindTaskType FindTaskType { get; }

      bool IsFinished { get; }
   }
}
