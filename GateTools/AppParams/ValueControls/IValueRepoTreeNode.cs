namespace Gate.Tools.AppParams.ValueControls
{
   /// <summary>
   /// 
   /// </summary>
   public interface IValueRepoTreeNode
   {
      string Caption { get; set; }

      IValueRepoTreeNode AddChild(string caption);

      object? Tag { get; set; }
   }
}
