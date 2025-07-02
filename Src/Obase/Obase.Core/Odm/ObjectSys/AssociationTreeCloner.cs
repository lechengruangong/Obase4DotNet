/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树复制器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:46:54
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联树复制器,作为一个关联树向下访问者执行复制关联树的操作
    /// </summary>
    public class AssociationTreeCloner : IAssociationTreeDownwardVisitor<AssociationTreeNode>
    {
        /// <summary>
        ///     遍历关联树的结果。
        /// </summary>
        private AssociationTreeNode _result;

        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        public AssociationTreeNode Result => _result;

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
        /// <param name="out_parentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(AssociationTree subTree, object parentState, out object out_parentState,
            out object outPrevisitState)
        {
            var clone = subTree.Node.CloneAlone();
            _result = clone;

            out_parentState = clone;
            outPrevisitState = null;
            return true;
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
        }
    }
}