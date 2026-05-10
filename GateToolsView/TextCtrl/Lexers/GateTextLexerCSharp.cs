namespace Gate.ToolsView.TextCtrl.Lexers
{
   public class GateTextLexerCSharp : GateTextLexerCpp
   {
      public override string Name => "C#";

      public override string[] Extensions => new string[] { ".cs" };

      public override string KeyWords0 => "abstract inline as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while var partial";

      public override string KeyWords1 => "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void ";
   }
}
