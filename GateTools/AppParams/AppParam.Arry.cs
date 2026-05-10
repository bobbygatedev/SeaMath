namespace Gate.Tools.AppParams
{
   public abstract partial class AppParam
   {
      public class Arry<P> : ArryRaw where P : AppParam, new()
      {
         public Arry() : base(null, null) { }

         public Arry(string name, string? caption = null) : base(name, caption) { }

         public new P this[int index] => (P)SubItems[index];

         public new P[] Items => SubItems.Cast<P>().ToArray();

         public override AppParam MakeItem() => new P();

         public override Type ItemFieldType => typeof(P);

         public P InsertParam(P param, int index) => (P)base.InsertParamRaw(param, index);

         public P AddParam(P param) => InsertParam(param, SubItems.Length);

         public P[] AddParams(params P[] @params)
         {
            foreach (var par in @params) { InsertParam(par, SubItems.Length); }

            return @params.ToArray();
         }

         public bool RemoveParams(params P[] @params) => base.RemoveParamsRaw(@params);

         public override void CopyTo(AppParam other)
         {
            if (other is Arry<P> oth && oth.GetType() == GetType())
            {
               oth.Clear();

               foreach (var itm in Items)
               {
                  var cpy = new P();

                  itm.CopyTo(cpy);
                  oth.AddParam(cpy);
               }
            }
            else
            {
               throw new Gate.Tools.ToolsException($"Can't copy from {GetType().Name} to {other.GetType().Name}");
            }
         }
      }
   }
}

