using Gate.Tools.Message;
using Gate.Tools.Text.Elab;
using System.Collections;

namespace Gate.Tools.Text.TemplateExpand
{
   /// <summary>
   /// 
   /// </summary>
   public class TemplateExpander : HierarchicalItem
   {
      private bool myIs2Update = false;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="templateText"></param>
      /// <param name="messages"></param>
      public TemplateExpander(string? templateText, MsgCollection? messages = null)
      {
         Messages = messages ?? new MsgCollection();
         TemplateText = templateText;
         Symbols = new SymbolsType(this);
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      public TemplateExpander() : this(null, null) { }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="templateText"></param>
      public TemplateExpander(string templateText) : this(templateText, null) { }

      /// <summary>
      /// 
      /// </summary>
      public class SymbolsType : IEnumerable<TemplateExpanderSymbol>
      {
         public SymbolsType(TemplateExpander templateExpander) => TemplateExpander = templateExpander;

         public TemplateExpanderSymbol[] Items
         {
            get
            {
               TemplateExpander.myUpdate();

               return TemplateExpander.SubItems.OfType<TemplateExpanderSymbol>().ToArray();
            }
         }

         public TemplateExpanderSymbol? this[string id] => Items.FirstOrDefault(s => s.Id == id);

         public TemplateExpander TemplateExpander { get; }

         public IEnumerator<TemplateExpanderSymbol> GetEnumerator() => Items.ToList().GetEnumerator();

         IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
      }

      /// <summary>
      /// 
      /// </summary>
      public SymbolsType Symbols { get; }

      /// <summary>
      /// 
      /// </summary>
      public TemplateExpanderContent Content
      {
         get
         {
            var res = SubItems.OfType<TemplateExpanderContent>().FirstOrDefault();

            if (res == null)
            {
               myAddSubItem(res = new TemplateExpanderContentRecord());
            }

            return res;
         }
      }

      /// <summary>
      /// <br> If enabled any <see cref="TemplateExpanderContent"/> whose content is null can inherit content from parent</br>
      /// <br> eg  'LAB1  !ARRAY:! LAB1 !;!'  LAB1 inside array may use value set from parent LAB1 </br>
      /// </summary>
      public bool IsInheritanceEnabled { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? TemplateStore { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public MsgCollection Messages { get; }


      /// <summary>
      /// 
      /// </summary>
      public (string ini, string end) TokenBorders { get; set; } = ("[", "]");

      /// <summary>
      /// 
      /// </summary>
      public TemplateExpanderSymbolParserMain SymbolParser { get; set; } = new TemplateExpanderSymbolParserDefault();

      /// <summary>
      /// 
      /// </summary>
      public string? TemplateText
      {
         get => TemplateStore?.Content;

         set
         {
            TemplateStore = new TxtStore(value ?? "");
            myIs2Update = true;
            Clear();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsError => Messages.Any(m => m.MsgType == MsgType.fail || m.MsgType == MsgType.fatal || m.MsgType == MsgType.fail);

      /// <summary>
      /// 
      /// </summary>
      public string ExpandedText
      {
         get
         {
            var sto = TemplateStore?.GetCopy() ?? throw new Gate.Tools.ToolsException("Template store is null");

            //eg #[#[ => #[
            var sbs = Symbols.OfType<TemplateExpanderSymbolStartBorderReplace>().ToArray();

            var sbs_rps = sbs.Select(s => new TxtStoreReplacement(s.Token.Interval, new TxtTokenConst(TokenBorders.ini))).ToArray();

            var rps_tks = Content.SubContents.Select(c => c.ExpandedToken).ToArray();
            var cnt_tks = Content.SubContents.Select(c => c.Symbol.Token).ToArray();

            var rps =
               Enumerable.Range(0, rps_tks.Length).
               Select(i => new TxtStoreReplacement(cnt_tks[i].Interval, rps_tks[i])).
               ToArray();

            sto.Replace(sbs_rps.Concat(rps));

            return sto.ContentFinal;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public TemplateExpander GetCopy() => new TemplateExpander
      {
         TemplateText = TemplateText,
         TokenBorders = TokenBorders
      };

      public void Clear() => myRemoveSubItem(SubItems.OfType<TemplateExpanderContent>().FirstOrDefault());//clear content

      public void SetOrCrash(string field, object? value)
      {
         (Content[field]??throw new Crash($"Field {field} not defined")).Value = "LAB1_VALUE";
      }

      private void myUpdate()
      {
         if (myIs2Update)
         {
            myIs2Update = false;

            var mrk = new TxtMarker(TemplateStore ?? throw new Gate.Tools.ToolsException("Template store is null"));
            var in_dat = new TxtElabInData(Messages);
            var oup = new TemplateExpanderSymbolParserOutput();

            var prs = SymbolParser ?? throw new Crash("Symbol parser not set");

            prs.TokenBorders = TokenBorders;

            switch (prs.Perform(mrk, in_dat, ref oup))
            {
               case TxtElabResult.success:
                  myAddSubItemRange(oup?.ListSymbols ?? throw new Crash());
                  break;

               case TxtElabResult.failure:
               case TxtElabResult.failure_unrecoverable:
                  throw new Gate.Tools.ToolsException(Messages);

               case TxtElabResult.continue_searching:
               default:
                  throw new Crash();
            }
         }
      }

      public override string ToString() => TemplateStore?.Lines.First(l => l.Content.Trim() != "").Content ?? "";
   }
}
