/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联叶子节点收集器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:39:03
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联叶子节点收集器
    /// </summary>
    public class AssociationLeafNodeCollector : IAssociationTreeDownwardVisitor<AssociationTreeNode[]>
    {
        /// <summary>
        ///     收集结果
        /// </summary>
        private readonly List<AssociationTreeNode> _result = new List<AssociationTreeNode>();

        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        public AssociationTreeNode[] Result => _result.ToArray();

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AssociationTree subTree, object parentState, object previsitState)
        {
        }

        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(AssociationTree subTree, object parentState, out object outParentState,
            out object outPrevisitState)
        {
            outParentState = outPrevisitState = null;
            //有子节点则不收集 而是继续遍历
            if (subTree.Node.Children.Length > 0)
                return true;
            _result.Add(subTree.Node);
            return false;
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
        }
    }
}