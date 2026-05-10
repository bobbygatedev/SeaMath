using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public class Cmd : ICmdWithId, ICmdWithCaption
   {
      private string? myCaption = null;
      private Image? myImage = null;
      private bool myIsEnabled = true;
      private bool myIsVisible = true;
      private Action<Cmd>? myAction;
      private bool? myIsChecked;
      private readonly List<ICmdControlAssociation> myListAssociation = new List<ICmdControlAssociation>();

      public delegate void OnChangeHandler(Cmd sender);

      public event OnChangeHandler? OnVisibleChange;

      public event OnChangeHandler? OnEnabledChange;


      /// <summary>
      /// 
      /// </summary>
      /// <param name="id"></param>
      /// <param name="caption"></param>
      public Cmd(string? id = null, string? caption = null, Keys shortCut = Keys.None, Keys shortCut2 = Keys.None)
      {
         Id = GetUniqueId(id);
         DefaultCaption = Caption = caption;
         ShortCut = shortCut;
         ShortCut2 = shortCut2;
      }

      /// <summary>
      /// Since command are multiple instance object, they are needed to be encapsulated in a slot class in order to implement hierarchy. 
      /// </summary>
      public class Slot : HierarchicalItem, ICmdMenuItem
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="cmd"></param>
         /// <param name="isDefault"></param>
         public Slot(Cmd cmd, bool isDefault)
         {
            Cmd = cmd;
            IsDefault = isDefault;
         }

         /// <summary>
         /// 
         /// </summary>
         public Cmd Cmd { get; }

         /// <summary>
         /// 
         /// </summary>
         public bool IsDefault { get; }

         /// <summary>
         /// 
         /// </summary>
         public string Id => Cmd.Id;

         public override string ToString() => $"Slot to {Cmd}";
      }

      /// <summary>
      /// 
      /// </summary>
      public string? DefaultCaption { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public ICmdControlAssociation[] Associations => myListAssociation.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public string Id { get; }

      public string? Caption
      {
         get => myCaption;

         set
         {
            myCaption = value;

            foreach (var ass in myListAssociation) { ass.SetCmdCaption(myCaption); }
         }
      }

      public Keys ShortCut
      {
         get => ShortCutPair.Key1;
         set
         {
            ShortCutPair.Key1 = value;

            foreach (var ass in myListAssociation) { ass.SetShortCutPair(ShortCutPair.Key1, ShortCutPair.Key2); }
         }
      }

      public Keys ShortCut2
      {
         get => ShortCutPair.Key2;
         set
         {
            ShortCutPair.Key2 = value;

            foreach (var ass in myListAssociation) { ass.SetShortCutPair(ShortCutPair.Key1, ShortCutPair.Key2); }
         }
      }

      public ShortCutPair ShortCutPair { get; private set; } = new ShortCutPair();

      public string ShortCutText => ShortCutPair.Text;

      public Image? Image
      {
         get => myImage;

         set
         {
            myImage = value;

            foreach (var ass in myListAssociation) { ass.SetImage(value); }
         }
      }

      public Action<Cmd>? Action
      {
         get => myAction;
         set
         {
            if (myAction != null)
            {
               foreach (var ass in myListAssociation) { ass.SetAction(null); }
            }

            myAction = value;

            foreach (var ass in myListAssociation) { ass.SetAction(value); }
         }
      }

      public bool IsEnabled
      {
         get => myIsEnabled;
         set
         {
            myIsEnabled = value;
            OnEnabledChange?.Invoke(this);

            foreach (var ass in myListAssociation) { ass.IsEnabled = value; }
         }
      }

      public bool IsVisible
      {
         get => myIsVisible;
         set
         {
            myIsVisible = value;
            OnVisibleChange?.Invoke(this);

            foreach (var ass in myListAssociation) { ass.IsVisible = value; }
         }
      }

      public bool IsShortCutVisible
      {
         get
         {
            var is_vis = Associations.Any(a => a.IsVisible) || IsVisible;
            var is_ena = Associations.Any(a => a.IsEnabled) || IsEnabled;

            return is_vis && is_ena;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool? IsChecked
      {
         get => myIsChecked;
         set
         {
            myIsChecked = value;

            foreach (var ass in Associations) { ass.IsChecked = value; }
         }
      }

      /// <summary>
      /// True, since a command is always created inside application.
      /// </summary>
      public bool IsDefault => true;

      public void AddAssociation(ICmdControlAssociation cmdControlAssociation)
      {
         cmdControlAssociation.IsEnabled = IsEnabled;
         cmdControlAssociation.IsVisible = IsVisible;
         cmdControlAssociation.IsChecked = IsChecked;
         cmdControlAssociation.SetAction(Action);
         cmdControlAssociation.SetCmdCaption(Caption);
         cmdControlAssociation.SetImage(Image);
         cmdControlAssociation.SetShortCutPair(ShortCut, ShortCut2);
         myListAssociation.Add(cmdControlAssociation);
      }

      public void RemoveAssociation(ICmdControlAssociation cmdControlAssociation) => myListAssociation.Remove(cmdControlAssociation);

      /// <summary>
      /// Create an action, whose effect is synchornize check with simple boolean param.
      /// </summary>
      /// <param name="boolAppParam"></param>
      public void AssociateBooleanParamToCheck(AppParam.Simple<bool> boolAppParam)
      {
         IsChecked = boolAppParam.Value;
         boolAppParam.OnAnyChange += _ => IsChecked = boolAppParam.Value;
         Action = _ => boolAppParam.Value = !boolAppParam.Value;
      }

      /// <summary>
      /// 
      /// </summary>
      public void InvokeAction() => Action?.Invoke(this);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => Id;

      /// <summary>
      /// Returns string.Trim() or null if string is blank (null or string.Trim() == "").
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static string? GetSanitizedString(string? @string) => !@string.IsBlank() ? @string.ExtTrim() : null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public static string GetUniqueId(string? id)
      {
         var san_id = GetSanitizedString(id);

         return san_id == null ? Guid.NewGuid().ToString() : san_id;
      }
   }
}
