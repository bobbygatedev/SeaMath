namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      public abstract class NotScalar : AppParam
      {
         protected NotScalar(string? name, string? caption) : base(name, caption) { }
      }
   }
}

