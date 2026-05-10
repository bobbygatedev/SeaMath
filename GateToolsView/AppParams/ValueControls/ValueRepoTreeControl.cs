using Gate.Tools;
using Gate.Tools.AppParams.ValueControls;

namespace Gate.ToolsView.AppParams.ValueControls
{
   /// <summary>
   /// Tree control for repository 
   /// </summary>
   public partial class ValueRepoTreeControl : UserControl
   {
      public delegate void OnRepoNodeSelectedHandler(object? sender, IValueRepoTreeNode? repoTreeNode);

      public event OnRepoNodeSelectedHandler? OnRepoNodeSelected;

      public ValueRepoTreeControl()
      {
         InitializeComponent();
         
         CtrlImageList.Images.Add(Properties.Resources.ArrowForTree);
      }

      private class InnerNodeImpl : IValueRepoTreeNode
      {
         private readonly ValueRepoTreeControl myParent;

         public InnerNodeImpl(ValueRepoTreeControl parent, TreeNode treeNode)
         {
            myParent = parent;
            TreeNode = treeNode;
            TreeNode.Tag = this;
         }

         public TreeNode TreeNode { get; }

         public string Caption { get => TreeNode.Text; set => TreeNode.Text = value; }

         public object? Tag { get; set; }

         public IValueRepoTreeNode AddChild(string caption) => myParent.MthAddNode(caption);
      }

      /// <summary>
      /// Adds a node to tree. 
      /// </summary>
      /// <param name="caption">Name of the created node(appears as node text).</param>
      /// <param name="parentNode">Parent node (if null added to root)</param>
      /// <returns></returns>
      public IValueRepoTreeNode MthAddNode(string? caption, IValueRepoTreeNode? parentNode = null)
      {
         if (parentNode == null)
         {
            var tre_nod = CtrlTreeView.Nodes.Add(caption);

            myDoRefreshImages();

            return new InnerNodeImpl(this, tre_nod);
         }
         else
         {
            var par_tre_nod = myDoGetNode(parentNode) ?? throw new Crash();
            var chi_tri_nod = par_tre_nod.Nodes.Add(caption);

            myDoRefreshImages();

            return new InnerNodeImpl(this, chi_tri_nod);
         }
      }

      public void MthClear() => CtrlTreeView.Nodes.Clear();

      private void myDoRefreshImages()
      {
         foreach (var nod in myDoGetAllNodes())
         {
            nod.ImageIndex = nod.SelectedImageIndex = nod.StateImageIndex = nod.Nodes.Count > 0 ? 0 : 1;
         }
      }

      private TreeNode? myDoGetNode(IValueRepoTreeNode valueRepoNode)
      {
         var nds = CtrlTreeView.Nodes.Cast<TreeNode>().ToArray();

         var nod = nds.FirstOrDefault(n => n.Tag == valueRepoNode);

         if (nod != null) { return nod; }
         else
         {
            var rep_nds = nds.Select(n => (IValueRepoTreeNode)n.Tag).ToArray();

            foreach (var nd in rep_nds)
            {
               var tre_nod = myDoGetNode(nd);

               if (tre_nod != null) { return tre_nod; }
            }

            return null;
         }
      }

      private TreeNode[] myDoGetAllNodes(TreeNode? treeNode = null)
      {
         var lst = new List<TreeNode>();

         if (treeNode == null)
         {
            foreach (var nod in CtrlTreeView.Nodes.Cast<TreeNode>()) { lst.AddRange(myDoGetAllNodes(nod)); }
         }
         else
         {
            lst.Add(treeNode);
            lst.AddRange(treeNode.Nodes.Cast<TreeNode>().SelectMany(n => myDoGetAllNodes(n)));
         }

         return lst.ToArray();
      }

      private void CtrlTreeView_BeforeSelect(object? sender, TreeViewCancelEventArgs e) =>
         OnRepoNodeSelected?.Invoke(this, e.Node?.Tag as IValueRepoTreeNode);
   }
}
