namespace Gate.Tools
{
   /// <summary>
   /// HierarchicalItem with IDisposable pattern. 
   /// Dispose will not affect hierarchy, but only free managed resources of item itself (if any).
   /// </summary>
   public abstract class HierarchicalItemWithFinalizer : HierarchicalItem, IDisposable
   {
      private bool myIsDisposed;

      protected HierarchicalItemWithFinalizer()
      {
         
      }

      protected virtual void Dispose(bool isDisposing)
      {
         if (!myIsDisposed)
         {
            if (isDisposing)
            {
               myFreeManaged();
            }

            myFreeUnmanaged();
            myIsDisposed = true;
         }
      }

      protected abstract void myFreeUnmanaged();
      protected abstract void myFreeManaged();

      ~HierarchicalItemWithFinalizer()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(isDisposing: false);
      }

      public void Dispose()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(isDisposing: true);
         GC.SuppressFinalize(this);
      }
   }
}
