using Gate.Tools;
using System.Collections;

namespace Gate.ToolsView.MenuCommand
{
   public class CmdCollection : IEnumerable<Cmd>
   {
      private readonly Func<Cmd[]> myObserver;

      public CmdCollection(Func<Cmd[]> observer) => myObserver = observer;

      public Cmd[] Items => myObserver.Invoke();

      public Cmd this[int index] => Items[index];

      public Cmd this[string? id] => Items.FirstOrDefault(i => i.Id == id) ?? throw new Crash();

      public bool TryGetCmd(string? id, out Cmd? cmd)
      {
         cmd = Items.FirstOrDefault(i => i.Id == id);

         return cmd != null;
      }

      public IEnumerator<Cmd> GetEnumerator() => Items.ToList().GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
   }
}
