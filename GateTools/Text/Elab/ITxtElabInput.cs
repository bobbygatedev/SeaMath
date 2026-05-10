namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// Interface for text elaborator input.
   /// </summary>
   public interface ITxtElabInput 
   {
      /// <summary>
      /// Current id.
      /// </summary>
      int CurrIdx { get; set; }

      /// <summary>
      /// Whether the input is 
      /// </summary>
      bool IsIn { get; }
   }
}
