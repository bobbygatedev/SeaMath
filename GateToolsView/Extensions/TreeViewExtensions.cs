namespace Gate.ToolsView.Extensions
{
   /// <summary>
   /// 
   /// </summary>
   public static class TreeViewExtensions
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNode"></param>
      /// <returns></returns>
      public static TreeNode[] GetAllSiblingNodes(this TreeNode treeNode)
      {
         var nds = treeNode.Nodes.Cast<TreeNode>().ToArray();

         return nds.Concat(nds.SelectMany(n => GetAllSiblingNodes(n))).ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNodeCollection"></param>
      /// <returns></returns>
      public static TreeNode[] GetAllSiblingNodes(this TreeNodeCollection treeNodeCollection)
      {
         var nds = treeNodeCollection.Cast<TreeNode>().ToArray();

         return nds.Concat(nds.SelectMany(n => n.GetAllSiblingNodes())).ToArray();
      }

      /// <summary>
      /// <paramref name="node"/> sub nodes sort based on <see cref="TreeNode.Text"/> value.
      /// </summary>
      /// <param name="node">Input <see cref="TreeNodeCollection"/></param>
      public static void SortSubNodes(this TreeNode node) => node.Nodes.SortNodes();

      /// <summary>
      /// <paramref name="node"/> sub nodes sort based on <paramref name="treeNodeComparer"/>.
      /// </summary>
      /// <param name="node">Input <see cref="TreeNodeCollection"/></param>
      /// <param name="treeNodeComparer"><see cref="TreeNode"/> pair comparer.</param>
      public static void SortSubNodes(this TreeNode node, Func<TreeNode, TreeNode, int> treeNodeComparer) => node.Nodes.SortNodes(treeNodeComparer);

      /// <summary>
      /// <paramref name="treeNodeCollection"/> sort based on <see cref="TreeNode.Text"/> value.
      /// </summary>
      /// <param name="treeNodeCollection">Input <see cref="TreeNodeCollection"/></param>
      public static void SortNodes(this TreeNodeCollection treeNodeCollection) =>
         SortNodes(treeNodeCollection, (n1, n2) => string.Compare(n1.Text, n2.Text));

      /// <summary>
      /// <paramref name="treeNodeCollection"/> sort based on <paramref name="treeNodeComparer"/>.
      /// </summary>
      /// <param name="treeNodeCollection">Input <see cref="TreeNodeCollection"/></param>
      /// <param name="treeNodeComparer"><see cref="TreeNode"/> pair comparer.</param>
      public static void SortNodes(this TreeNodeCollection treeNodeCollection, Func<TreeNode, TreeNode, int> treeNodeComparer)
      {
         var nc = treeNodeCollection.Count;
         var lst_nds = treeNodeCollection.Cast<TreeNode>().ToList();

         //nc-1 iterations
         for (int i = 0; i < nc - 1; i++)
         {
            var c_i = i;

            //search the best
            for (var j = i + 1; j < nc; j++)
            {
               var cmp = treeNodeComparer(treeNodeCollection[c_i], treeNodeCollection[j]);

               if (cmp < 0) { c_i = j; }
            }

            if (c_i != i)
            {
               var nod = treeNodeCollection[c_i];

               treeNodeCollection.Remove(nod);
               treeNodeCollection.Insert(i, nod);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNodeCollection"></param>
      /// <returns></returns>
      public static TreeNode[] GetNodes(this TreeNodeCollection treeNodeCollection) => treeNodeCollection.Cast<TreeNode>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNodeCollection"></param>
      /// <returns></returns>
      public static TreeNode[] GetNodes(this TreeNode treeNode) => treeNode.Nodes.Cast<TreeNode>().ToArray();


      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNodeCollection"></param>
      /// <param name="predicate"></param>
      /// <returns></returns>
      public static TreeNode? FirstOrDefaultNode(this TreeNodeCollection treeNodeCollection, Func<TreeNode, bool> predicate) => 
         treeNodeCollection.GetNodes().FirstOrDefault(predicate);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNodeCollection"></param>
      /// <returns></returns>
      public static TreeNode? FirstOrDefaultNode(this TreeNodeCollection treeNodeCollection) => treeNodeCollection.GetNodes().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNode"></param>
      /// <param name="predicate"></param>
      /// <returns></returns>
      public static TreeNode? FirstOrDefaultNode(this TreeNode treeNode, Func<TreeNode, bool> predicate) => treeNode.Nodes.FirstOrDefaultNode(predicate);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="treeNode"></param>
      /// <returns></returns>
      public static TreeNode? FirstOrDefaultNode(this TreeNode treeNode) => treeNode.Nodes.FirstOrDefaultNode();
   }
}
