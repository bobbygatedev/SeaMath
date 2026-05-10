namespace Gate.Tools
{
   /// <summary>
   /// Interface for overriding the default exception show handler (which is console output) of ToolsException. 
   /// The implementation of this interface should be in a separate assembly, and it will be automatically detected and used by ToolsException.
   /// </summary>
   public interface IToolsExceptionShowHandlerModifier
   {
      /// <summary>
      /// Retrieves the current exception show handler that overrides the default behavior, if one is set.
      /// </summary>
      /// <returns>A <see cref="ToolExceptionShowHandler"/> delegate representing the overridden exception show handler, or
      /// <c>null</c> if no override is set.</returns>
      ToolExceptionShowHandler GetOverridenShowHandler();
   }
}