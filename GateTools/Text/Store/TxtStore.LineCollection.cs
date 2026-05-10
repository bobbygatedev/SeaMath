using System;
using System.Collections;
using System.Collections.Generic;

namespace Gate.Tools.Text
{
   public partial class TxtStore
   {
      /// <summary>
      /// Line collection optimized for memory occupation. 
      /// </summary>
      public abstract class LineCollection : IEnumerable<LineToken>
      {
         protected delegate void OnResetHandler();

         protected event OnResetHandler? OnReset;

         protected LineCollection() { }

         private class InnerEnumerator : IEnumerator<LineToken>
         {
            private readonly LineCollection myParent;
            private int? myCurrentIndex;

            public InnerEnumerator(LineCollection parent)
            {
               myParent = parent;
               myCurrentIndex = -1;
               myParent.OnReset += MyParent_OnReset;
            }

            private void MyParent_OnReset() => myCurrentIndex = null;

            public LineToken Current => myCurrentIndex.HasValue ?
               myParent.myGet(myCurrentIndex.Value) : throw new InvalidOperationException("Reset enumerator!");

            object IEnumerator.Current => Current;

            public void Dispose() => myCurrentIndex = null;

            public bool MoveNext() => myCurrentIndex.HasValue ?
               ++myCurrentIndex < myParent.Count : throw new InvalidOperationException("Reset enumerator!");

            public void Reset()
            {
               myParent.myReset();
               myCurrentIndex = null;
            }
         }


         protected void myReset()
         {
            myResetAction();
            OnReset?.Invoke();
         }

         /// <summary>
         /// 
         /// </summary>
         protected abstract void myResetAction();

         /// <summary>
         /// Returns line by index (0-) creating line object if necessary
         /// </summary>
         /// <param name="lineIdx0"></param>
         /// <returns></returns>
         protected abstract LineToken myGet(int lineIdx0);

         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         protected abstract int myGetCount();

         public LineToken this[int lineIdx0] => myGet(lineIdx0);

         public int Count => myGetCount();

         public LineToken LastLine => myGet(myGetCount() - 1);

         /// <summary>
         /// When true <see cref="LineToken"/> is saved inside memory, which increases speed but even memory occupation.
         /// </summary>
         public bool IsLineToSave { get; set; } = true;

         public IEnumerator<LineToken> GetEnumerator() => new InnerEnumerator(this);

         IEnumerator IEnumerable.GetEnumerator() => new InnerEnumerator(this);

         /// <summary>
         /// Discards both line reference and line indices in order to save memory.
         /// </summary>
         public abstract void DiscardLineMemory();
      }
   }
}
