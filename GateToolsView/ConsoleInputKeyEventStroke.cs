using Gate.Tools.DesignPattern;
using Gate.Tools.Extensions;
using Gate.Tools.Multithread;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// Observe for key event OnKeyUp OnKeyDown 
   /// </summary>
   public class ConsoleInputKeyEventStroke : BaseClassWithFinalizer
   {
      /// <summary>
      /// 
      /// </summary>
      public enum EventType
      {
         none = 0,
         cancel = 1,
         left = 2,
         right = 3,
         up = 4,
         down = 5,
         tab = 6,
         valid_char = 7,
         end = 8,
         home = 9,
         tab_shift = 10,

         /// <summary>
         /// Ctrl+C
         /// </summary>
         abort_control_c = 11,

         /// <summary>
         /// Ctrl+D used for aborting from scanf getchar ..
         /// </summary>
         abort_control_d = 12,

         backspace = 13,

         enter = 14,
      }

      public const int QUEUE_CAPACITY = 128;

      public delegate void OnConsumeHandler(EventType eventType, char? extraChar);

      private OnConsumeHandler? myConsumeHandler;
      private QueueSafeThread<(EventType, char)> myQueueEvent = new QueueSafeThread<(EventType, char)>(QUEUE_CAPACITY, "ConsoleInputKeyEventStroke");
      private bool myIsActive = false;

      public ConsoleInputKeyEventStroke(Control controlKeyEventGenerator)
      {
         ControlKeyEventGenerator = controlKeyEventGenerator;
         //ensure all keys are handled by ControlKeyEventGenerator_KeyDown
         ControlKeyEventGenerator.PreviewKeyDown += ControlKeyEventGenerator_PreviewKeyDown;
         ControlKeyEventGenerator.KeyDown += SynthetizeKeyDown;
         ControlKeyEventGenerator.KeyPress += SynthetizeKeyPress;
      }


      private class InnerCombination
      {
         [Flags]
         public enum ComboType
         {
            none = 0,
            shift = 0x1,
            control = 0x2,
            alt = 0x4,
         }

         private static readonly InnerCombination[] myCombos = new[] {
            new InnerCombination(Keys.Enter,EventType.enter),
            new InnerCombination(Keys.Back,EventType.backspace),
            new InnerCombination(Keys.Delete,EventType.cancel),
            new InnerCombination(Keys.Left,EventType.left),
            new InnerCombination(Keys.Right,EventType.right),
            new InnerCombination(Keys.Up,EventType.up),
            new InnerCombination(Keys.Down,EventType.down),
            new InnerCombination(Keys.Tab,EventType.tab),
            new InnerCombination(Keys.Tab, EventType.tab_shift , ComboType.shift),
            new InnerCombination(Keys.End,EventType.end),
            new InnerCombination(Keys.Home,EventType.home),
            new InnerCombination(Keys.C , EventType.abort_control_c , ComboType.control),
            new InnerCombination(Keys.D , EventType.abort_control_d , ComboType.control),
           };

         private InnerCombination(Keys keyCode, EventType eventType, ComboType combo = ComboType.none)
         {
            KeyCode = keyCode;
            EventType = eventType;
            Combo = combo;
         }

         public Keys KeyCode { get; }
         public EventType EventType { get; }
         public ComboType Combo { get; }

         public static void Action(ConsoleInputKeyEventStroke consoleInputKeyEventStroke, KeyEventArgs e)
         {
            var act = myCombos.FirstOrDefault(c => c.KeyCode == e.KeyCode && c.myCheckCSA(e));

            if (act != null)
            {
               consoleInputKeyEventStroke.myQueueEvent.Produce(new[] { (act.EventType, (char)e.KeyValue) });
               e.SuppressKeyPress = true;//avoid further processing
            }
         }

         public override string ToString() => $"{EventType},{Combo}";

         private bool myCheckCSA(KeyEventArgs e)
         {
            var is_s = Combo.HasFlag(ComboType.shift);
            var is_c = Combo.HasFlag(ComboType.control);
            var is_a = Combo.HasFlag(ComboType.alt);

            return is_s == e.Shift && is_c == e.Control && is_a == e.Alt;
         }

      }

      public OnConsumeHandler? OnConsume
      {
         get
         {
            var res = null as OnConsumeHandler;

            Interlocked.Exchange(ref res, myConsumeHandler);

            return res;
         }

         set
         {
            var res = null as OnConsumeHandler;
            var old_val = Interlocked.CompareExchange(ref myConsumeHandler, value, null);

            if (old_val != null && value != null) { throw new Gate.Tools.ToolsException($"OnConsume handler already set!"); }
            else if (myConsumeHandler != null)
            {
               myQueueEvent.Consumer = myConsumer;
               myConsumeHandler = value;
            }
            else
            {
               myQueueEvent.Consumer = null;
               myConsumeHandler = null;
            }
         }
      }

      public Control ControlKeyEventGenerator { get; }

      public void Clear() => myQueueEvent.Clear();

      public bool IsActive
      {
         get => myIsActive;

         set
         {
            if (myIsActive != value)
            {
               if (!(myIsActive = value))
               {
                  myQueueEvent.Clear();
               }
            }
         }
      }

      protected override void myFreeManaged() => myQueueEvent.Dispose();

      protected override void myFreeUnmanaged() { }

      private void myConsumer(QueueSafeThread<(EventType, char)> queueSafe, (EventType, char)[] consumedData)
      {
         if (IsActive)
         {
            var hnd = null as OnConsumeHandler;

            Interlocked.Exchange(ref hnd, myConsumeHandler);

            foreach (var dat in consumedData) { hnd?.Invoke(dat.Item1, dat.Item2); }
         }
      }

      /// <summary>
      /// In this way up,down,left,right are forwarded to <see cref="SynthetizeKeyDown(object, KeyEventArgs)"/>
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ControlKeyEventGenerator_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e) => e.IsInputKey =
         e.Control && e.KeyCode == Keys.C && !e.Shift && !e.Alt ||
         e.KeyData == Keys.Tab ||
         e.KeyData == Keys.Left ||
         e.KeyData == Keys.Right ||
         e.KeyData == Keys.Up ||
         e.KeyData == Keys.Down;

      public void SynthetizeKeyPress(object? sender, KeyPressEventArgs e)
      {
         var is_c = char.IsControl(e.KeyChar);

         if (!is_c)
         {
            myQueueEvent.Produce([(EventType.valid_char, e.KeyChar)]);
         }

         e.Handled = true;//avoid further processing
      }

      

      public void PasteString(string txt)
      {
         var lns = txt.SplitLines();

         for (int i = 0; i < lns.Length; i++)
         {
            myQueueEvent.Produce(
               lns[i].
               Where(c => myIsValidKeyPress(new KeyPressEventArgs(c))).
               Select(c => (EventType.valid_char, c)).
               ToArray());

            if (i < lns.Length -1)
            {
               myQueueEvent.Produce([(EventType.enter, (char)0)]);
            }
         }
      }

      public void SynthetizeKeyDown(object? sender, KeyEventArgs e) => InnerCombination.Action(this, e);

      private static bool myIsValidKeyPress(KeyPressEventArgs e) => !char.IsControl(e.KeyChar) || e.KeyChar == '\r' || e.KeyChar == 8;
   }
}
