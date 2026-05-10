using Gate.Tools;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ExtendedTreeView : UserControl
   {
     
      private readonly InnerRoot myRoot;

      public ExtendedTreeView()
      {
         InitializeComponent();
         CtrlExtPanel.PpSingleControlContentSizeCalculator = myDoCalculateTreeSize;
         Refresh();
         myRoot = new InnerRoot(this);
      }

      private class InnerRoot : HierarchicalItem
      {
         public InnerRoot(ExtendedTreeView extendedTreeView) => ExtendedTreeView = extendedTreeView;

         public ExtendedTreeView ExtendedTreeView { get; }

         public void AddNode(NodeType node) => myAddSubItem(node);
      }

      public class NodeType : HierarchicalItem
      {
         private readonly TreeNode? myPhysicalNode;

         public NodeType(TreeNode treeNode)
         {
            myPhysicalNode = treeNode ?? throw new Crash();
            myPhysicalNode = new TreeNode();
         }

         public object? Tag { get; set; }

         public ExtendedTreeView? ExtendedTreeView => (Anchestor as InnerRoot)?.ExtendedTreeView;

         public string? Text
         {
            get => myPhysicalNode?.Text;

            set => (myPhysicalNode ?? throw new Crash()).Text = value;
         }

         public NodeType AddNode(string? text = null)
         {
            var tre_nod = new TreeNode();            
            var nod = new NodeType(tre_nod);

            ExtendedTreeView?.CtrlTreeView.BeginUpdate();
            nod.Text = text;  
            myPhysicalNode?.Nodes.Add(tre_nod);
            ExtendedTreeView?.CtrlTreeView.BeginUpdate();
            myAddSubItem(nod);

            return nod;
         }

         public void Remove()
         {
            myPhysicalNode?.Remove();
            ParentItem = null;
         }
      }

      public NodeType MthAddNode(string text, NodeType? parentNode = null)
      {
         if (parentNode == null)
         {
            var tre_nod = new TreeNode();
            var nod = new NodeType(tre_nod);

            CtrlTreeView.BeginUpdate();
            CtrlTreeView.Nodes.Add(tre_nod);

            myRoot.AddNode(nod);
            nod.Text = text;
            CtrlTreeView.EndUpdate();

            return nod;
         }
         else
         {
            var nod = parentNode.AddNode();

            nod.Text = text;

            return nod;
         }
      }

      public TreeNodeCollection PpNodes => CtrlTreeView.Nodes;

      public TreeNode[] PpAllNodes => myGetAllNodes(CtrlTreeView.Nodes);

      private Size myDoCalculateTreeSize(Control control)
      {
         var rc = new Rectangle();

         foreach (var nod in PpAllNodes) { rc = Rectangle.Union(rc, nod.Bounds); }

         return rc.Size;
      }

      private static TreeNode[] myGetAllNodes(TreeNodeCollection nodes)
      {
         var lst_nod = new List<TreeNode>();

         foreach (var nod in nodes.Cast<TreeNode>())
         {
            lst_nod.Add(nod);
            lst_nod.AddRange(myGetAllNodes(nod.Nodes));
         }

         return lst_nod.ToArray();
      }


      public void MthClear() => CtrlTreeView.Nodes.Clear();
   }
}
