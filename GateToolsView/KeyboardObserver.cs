using Gma.System.MouseKeyHook;

namespace Gate.ToolsView
{
   /// <summary>
   /// 
   /// </summary>
   public class KeyboardObserver
   {
      public event KeyEventHandler? OnKeyDown;
      public event KeyEventHandler? OnKeyUp;

      private IKeyboardMouseEvents? myGlobalEvents;

      public KeyboardObserver()
      {
      }

      public Object? Sender { get; internal set; }


      public void Start()
      {
         myGlobalEvents = Hook.AppEvents();
         myGlobalEvents.KeyDown += myActionOnKeyDown;
         myGlobalEvents.KeyUp += myActionOnKeyUp;
      }

      public void Stop()
      {
         if (myGlobalEvents != null)
         {
            myGlobalEvents.KeyDown -= myActionOnKeyDown;
            myGlobalEvents.KeyUp -= myActionOnKeyUp;
            myGlobalEvents.Dispose();
            myGlobalEvents = null;
         }
      }

      protected virtual void myActionOnKeyDown(object? sender, KeyEventArgs e) => OnKeyDown?.Invoke(Sender ?? this, e);

      protected virtual void myActionOnKeyUp(object? sender, KeyEventArgs e) => OnKeyUp?.Invoke(Sender ?? this, e);
   }
}