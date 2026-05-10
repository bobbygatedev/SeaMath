using Gate.Tools.Arry;
using Gate.Tools.Message;

namespace Gate.Tools.Watch
{
   /// <summary>
   /// Wrappes an expression wrapped inside a <see cref="WatchControl"/> (or any other compatibe with <see cref="WatchExpr.Factory"/>/<see cref="IWatchExprManager"/> inter
   /// </summary>
   public sealed class WatchExpr : HierarchicalItem, IDisposable
   {
      private ChildMatrixType? myChildMatrix;

      public delegate void OnChangeValueHandler(WatchExpr watchExpr);

      public event OnChangeValueHandler? OnChangeValue;

      private WatchExpr(string expression, FactoryType factory, IWatchExprManager parser, SubExprType? subExpr)
      {
         Expression = expression;
         Factory = factory;
         WatchManager = parser;
         SubExpr = subExpr;
      }

      ~WatchExpr() { myDispose(); }

      /// <summary>
      /// Factory acts as <see cref="WatchExpr"/> factory, dispatch expression and run state change events.
      /// </summary>
      public class FactoryType : HierarchicalItem
      {
         private bool myIsRun = true;

         public delegate void OnRunChangeHandler(FactoryType factory, bool isRun);
         public delegate void OnAnyExprChangeHandler(WatchExpr watchExpr);

         public event OnRunChangeHandler? OnRunChange;
         public event OnAnyExprChangeHandler? OnAnyExprChange;

         public FactoryType(IWatchExprManager exprManager) => ExprManager = exprManager;

         public WatchExpr[] WatchExprs => SubItems.Cast<WatchExpr>().ToArray();

         public bool IsRun
         {
            get => myIsRun;

            set
            {
               if (myIsRun != value)
               {
                  if (!value)
                  {
                     //re-parse
                     foreach (var wat_exp in WatchExprs) { wat_exp.TryParse(); }
                  }

                  myIsRun = value;
                  OnRunChange?.Invoke(this, IsRun);
               }
            }
         }

         public IWatchExprManager ExprManager { get; }

         public WatchExpr Make(string expression, IWatchExprManager exprManager, SubExprType? childSubExpr = null)
         {
            var wat_exp = new WatchExpr(expression, this, exprManager, childSubExpr);

            myAddSubItem(wat_exp);
            wat_exp.OnChangeValue += Wat_exp_OnChangeValue;

            return wat_exp;
         }

         private void Wat_exp_OnChangeValue(WatchExpr watchExpr) => OnAnyExprChange?.Invoke(watchExpr);
      }

      /// <summary>
      /// 
      /// </summary>
      public class SubExprType
      {
         public SubExprType(string? structLabel, int[] indexInParent, WatchExpr parentExpr)
         {
            StructLabel = structLabel;
            IndexInParent = indexInParent.ToArray();
            ParentExpr = parentExpr;
         }

         public string? StructLabel { get; }

         public int[] IndexInParent { get; }

         /// <summary>
         ///  parent expr if this belongs to an array/struct
         /// </summary>
         public WatchExpr ParentExpr { get; }
      }

      /// <summary>
      /// 
      /// </summary>
      public class ChildMatrixType
      {
         private readonly WatchExpr myParentExpr;

         public ChildMatrixType(WatchExpr parentExpr, ArrayMultidimensional<WatchExpr> matrix)
         {
            myParentExpr = parentExpr;
            Matrix = matrix;
         }

         public ArrayMultidimensional<WatchExpr> Matrix { get; }

         public WatchExpr? this[string structLabel]
         {
            get
            {
               if (StructLabels != null)
               {
                  var idx = StructLabels.Select(l => l.fieldName).ToList().IndexOf(structLabel);

                  return idx != -1 ? Matrix?[idx] : null;
               }
               else { return null; }
            }
         }

         public (string? fieldName, string? fieldType)[]? StructLabels =>
            Matrix != null && Matrix.All(se => se?.Value?.SubExpr?.StructLabel != null) ?
            Matrix.Select(se => (se?.Value?.SubExpr?.StructLabel, se?.Value?.TypeStr)).ToArray() : null;
      }

      /// <summary>
      /// 
      /// </summary>
      public IWatchExprManager WatchManager { get; }

      /// <summary>
      /// 
      /// </summary>
      public FactoryType Factory { get; }

      /// <summary>
      /// Is expression read only?
      /// </summary>
      public bool IsReadOnly { get; private set; }

      /// <summary>
      /// Input expression (as input by user in watch) 
      /// </summary>
      public string Expression { get; private set; }

      /// <summary>
      /// Type displayed name.
      /// </summary>
      public string? TypeStr => Value != null ? WatchManager.GetTypeString(Value) : null;

      /// <summary>
      /// 
      /// </summary>
      public object? Value { get; private set; }

      /// <summary>
      /// Displayed value after eval or null if eval fails.
      /// </summary>
      public string? ValueStr => Value != null ? WatchManager.GetObjectString(Value) : null;

      /// <summary>
      ///  error message to be displayed in case of evaluation fail.
      /// </summary>
      public Msg? ErrorMessage { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public SubExprType? SubExpr { get; set; }

      public ChildMatrixType? ChildMatrix
      {
         get
         {
            if (myChildMatrix == null && Value != null)
            {
               var sub_mat = WatchManager.GetChildMatrixArray(this);

               if (sub_mat != null)
               {
                  myChildMatrix = new ChildMatrixType(this, sub_mat);
               }
            }

            return myChildMatrix;
         }
      }

      /// <summary>
      /// Is there no error?
      /// </summary>
      public bool IsOk => ErrorMessage == null && Value != null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="newValue"></param>
      /// <returns></returns>
      public string? UpdateValue(string newValue)
      {
         if (WatchManager.Update(this, newValue, out var val))
         {
            Value = val;
            ErrorMessage = null;
            myChildMatrix = null;
            OnChangeValue?.Invoke(this);

            return ValueStr;
         }
         else { return null; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="value"></param>
      /// <param name="isReadOnly"></param>
      /// <returns></returns>
      public string? Init(object value, bool isReadOnly)
      {
         Value = value;
         myChildMatrix = null;
         IsReadOnly = isReadOnly;

         return ValueStr;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="expression"></param>
      /// <returns></returns>
      public bool TryParse()
      {
         if (WatchManager.TryParse(Expression, out var val, out var is_rdo, out var err_msg))
         {
            Value = val;
            IsReadOnly = is_rdo;
            ErrorMessage = null;
         }
         else
         {
            Value = null;
            IsReadOnly = true;
            ErrorMessage = err_msg;
         }

         myChildMatrix = null;//causes 

         return IsOk;
      }

      public override string ToString() => IsOk ? $"{Expression}={ValueStr}({TypeStr})" : $"Error: {ErrorMessage?.FullMessage}";

      public void Dispose() => myDispose();

      private void myDispose()
      {
         if (Factory != null)
         {
            myReplaceParent(null);
         }
      }
   }
}
