namespace Gate.Tools.Message
{
   /// <summary>
   /// Implement a collection of text messages.
   /// </summary>
   public class MsgCollection : HierarchicalItem, IEnumerable<Msg>
   {
      private bool myIs2PlotOnConsole;

      public delegate void OnMsg2DisplayAddedHandler(Msg[] msgs);
      public delegate void OnSubMsgsAddedHandler(Msg[] msgs, MsgCollection subCollection);
      public delegate void OnSubCollectionClearHandler(MsgCollection subCollection);

      /// <summary>
      /// 
      /// </summary>
      public event OnMsg2DisplayAddedHandler? OnMsg2DisplayAdded;

      /// <summary>
      /// 
      /// </summary>
      public event OnSubMsgsAddedHandler? OnSubMsgAdded;

      /// <summary>
      /// 
      /// </summary>
      public event Action? OnClear;

      /// <summary>
      /// 
      /// </summary>
      public event OnSubCollectionClearHandler? OnSubCollectionClear;

      /// <summary>
      /// Constructor.
      /// </summary>
      public MsgCollection() { }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="messages">Text message to init.</param>
      public MsgCollection(params Msg[] messages) => Add(messages);

      /// <summary>
      /// 
      /// </summary>
      public bool Is2PlotOnConsole
      {
         get => myIs2PlotOnConsole;

         set
         {
            if (myIs2PlotOnConsole != value)
            {
               if (myIs2PlotOnConsole = value) { OnMsg2DisplayAdded += MsgCollection_OnMsg2DisplayAdded; }
               else { OnMsg2DisplayAdded -= MsgCollection_OnMsg2DisplayAdded; }
            }
         }
      }

      /// <summary>
      /// Messages to display: <see cref="Msg.FullMessage"/> is not empty (trim() != "").
      /// </summary>
      public Msg[] MessagesToDisplay => MessagesAll.Where(m => m.Is2Display).ToArray();

      /// <summary>
      /// Equivalent to <see cref="MessagesToDisplay"/> length.
      /// </summary>
      public int Count => MessagesToDisplay.Length;

      /// <summary>
      /// All messages (including sub-messages).
      /// </summary>
      public Msg[] MessagesAll => AllDescendant.OfType<Msg>().ToArray();

      /// <summary>
      /// Report of collection containing the info about error/warning/success 
      /// </summary>
      public string Resume
      {
         get
         {
            var n_suc = MessagesAll.Count(m => m.MsgType == MsgType.success);
            var n_fai = MessagesAll.Count(m => m.MsgType == MsgType.fail);
            var n_up2 = MessagesAll.Count(m => m.MsgType == MsgType.uptodate);

            return $"Elaboration {(n_fai == 0 ? "successfull" : "failed")}({n_suc} succeeded , {n_fai} failed , {n_up2} up-to-date).";
         }
      }

      /// <summary>
      /// Messages, whose this collection is sub-message container or null if this collection has not a message as a parent.
      /// </summary>
      public Msg? ParentMsg => ParentItem as Msg;

      /// <summary>
      /// Anchestor collection, where the parent sequence is alwasy collection-msg-collection or this if parent is not a message.
      /// </summary>
      public MsgCollection? AnchestorCollection => ParentMsg != null ? ParentMsg?.ParentCollection?.AnchestorCollection : this;

      /// <summary>
      /// Message by index.
      /// </summary>
      /// <param name="index">Index to be get.</param>
      /// <returns></returns>
      /// <exception cref="System.ArgumentOutOfRangeException"></exception>
      public Msg this[int index] => this.ElementAt(index);

      /// <summary>
      /// Adds a text message to collection.
      /// </summary>
      /// <param name="messages"></param>
      public void Add(params Msg[] messages)
      {
         if (messages.All(m => m.ParentItem == null))
         {
            myAddSubItemRange(messages);

            if (messages.Any(m => m.Is2Display)) { OnMsg2DisplayAdded?.Invoke(messages.Where(m => m.Is2Display).ToArray()); }

            if (AnchestorCollection != this && AnchestorCollection?.OnSubMsgAdded != null)
            {
               AnchestorCollection.OnSubMsgAdded.Invoke(messages, this);
            }
         }
      }

      /// <summary>
      /// Clears the collection.
      /// </summary>
      public void Clear()
      {
         myRemoveSubItemRange(this);
         OnClear?.Invoke();

         if (AnchestorCollection != this && AnchestorCollection?.OnSubCollectionClear != null)
         {
            AnchestorCollection.OnSubCollectionClear.Invoke(this);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgs"></param>
      public void Remove(params Msg[] msgs) => myRemoveSubItemRange(msgs);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"{Count} messages({Resume})!";

      /// <summary>
      /// 
      /// </summary>
      public void PlotOnConsole()
      {
         foreach (var msg in this) { msg.PlotOnConsole(); }

         Console.WriteLine(Resume);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public IEnumerator<Msg> GetEnumerator() => MessagesToDisplay.ToList().GetEnumerator();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => MessagesToDisplay.GetEnumerator();

      private void MsgCollection_OnMsg2DisplayAdded(Msg[] msgs)
      {
         foreach (var msg in msgs) { msg.PlotOnConsole(); }
      }
   }
}
