namespace Gate.Tools.DesignPattern
{
   /// <summary>
   /// Base class with finalizer and IDisposable pattern.
   /// </summary>
   public abstract class BaseClassWithFinalizer : IDisposable
   {
      /// <summary>
      /// 
      /// </summary>
      ~BaseClassWithFinalizer() => Dispose(false);

      /// <summary>
      /// Frees managed resources. Called by Dispose(true).
      /// </summary>
      protected abstract void myFreeManaged();

      /// <summary>
      /// Releases unmanaged resources held by the derived class.
      /// </summary>
      /// <remarks>Derived classes must implement this method to free any unmanaged resources they own. This
      /// method is typically called by the Dispose pattern to ensure proper cleanup of native resources. It should not
      /// reference managed objects, as they may have already been finalized.</remarks>
      protected abstract void myFreeUnmanaged();

      /// <summary>
      /// Indicates whether the object has been disposed.
      /// </summary>
      public bool IsDisposed { get; private set; } = false;

      /// <summary>
      /// Public implementation of Dispose pattern callable by consumers.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool isDisposing)
      {
         if (!IsDisposed)
         {
            if (isDisposing) { myFreeManaged(); }

            myFreeUnmanaged();
            IsDisposed = true;
         }
      }
   }
}
