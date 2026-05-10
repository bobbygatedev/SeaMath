namespace Gate.Tools.DesignPattern
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="OBJ"></typeparam>
   public abstract class Builder<OBJ>
   {
      private int myPartBuilderIndex = -1;

      public enum BuildCurrentStepResult
      {
         success = 0,
         failure = 1,
         finished = 2,
      }

      public interface IPartBuilder
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="obj"></param>
         /// <returns>False stands for failure</returns>
         bool Build(OBJ? obj);
      }

      public class PartBuilderByAction : IPartBuilder
      {
         public PartBuilderByAction(Func<OBJ?, bool> action) => Action = action;

         public Func<OBJ?, bool> Action { get; }

         public bool Build(OBJ? obj) => Action(obj);
      }

      public IPartBuilder? CurrentPartBuilder => 
         myPartBuilderIndex >= 0 && myPartBuilderIndex < PartBuilders.Length ? PartBuilders[myPartBuilderIndex] : null;

      public OBJ? Object { get; private set; } = default(OBJ);

      /// <summary>
      /// 
      /// </summary>
      /// <returns>If no value build has finish otw true/false stands for success fail </returns>
      public BuildCurrentStepResult BuildCurrentStep()
      {
         if (CurrentPartBuilder != null)
         {
            if (CurrentPartBuilder.Build(Object))
            {
               myPartBuilderIndex++;
            }
            else
            {
               return BuildCurrentStepResult.failure;
            }
         }

         return CurrentPartBuilder != null ? BuildCurrentStepResult.success : BuildCurrentStepResult.finished;
      }

      public PART? BuildToPart<PART>(bool isPartType2Perform) where PART : class, IPartBuilder
      {
         var nxt_prs = PartBuilders.Skip(myPartBuilderIndex).ToArray();
         var prt_bui = nxt_prs.OfType<PART>().FirstOrDefault();

         if (prt_bui != null)
         {
            if (isPartType2Perform)
            {
               while (CurrentPartBuilder == prt_bui) { BuildCurrentStep(); }
            }
            else
            {
               while (PartBuilders[myPartBuilderIndex + 1] != prt_bui) { BuildCurrentStep(); }
            }
         }

         return prt_bui;
      }

      /// <summary>
      /// Reset the object and build all its parts.
      /// </summary>
      /// <returns></returns>
      public OBJ? BuildObjectFromBeginning()
      {
         Reset();

         return CompleteBuilding();
      }

      /// <summary>
      /// Build all parts until end.
      /// </summary>
      /// <returns></returns>
      public virtual OBJ? CompleteBuilding()
      {
         BuildCurrentStepResult res;

         while ((res = BuildCurrentStep()) == BuildCurrentStepResult.success) { }

         return res == BuildCurrentStepResult.finished ? Object : default(OBJ);
      }

      public virtual OBJ Reset()
      {
         myPartBuilderIndex = 0;

         return Object = GetBlankObject();
      }

      public abstract OBJ GetBlankObject();

      public abstract IPartBuilder[] PartBuilders { get; }
   }
}
