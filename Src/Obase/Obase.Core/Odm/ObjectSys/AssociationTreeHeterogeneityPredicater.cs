/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树异构判断器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:01:28
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Query;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     作为一个关联树向下访问者断言关联树是否为异构的。
    ///     [定义]如果关联树中存在任意一个节点，其映射源与根节点映射源分属不同的存储服务，则称该关联树为异构的。
    ///     警告
    ///     如果关联树根为TypeViewNode，不会检测该视图是否为异构，直接依据其终极源（考虑视图嵌套情形）判定根节点的存储标记。
    ///     实施说明
    ///     采用显式接口实现，相关方法定义为私有。
    /// </summary>
    public class AssociationTreeHeterogeneityPredicater : IAssociationTreeDownwardVisitor<bool>
    {
        /// <summary>
        ///     关联树异构断言提供程序
        /// </summary>
        private readonly HeterogeneityPredicationProvider _provider;

        /// <summary>
        ///     指示关联树是否为异构的。
        /// </summary>
        private bool _heterogeneous;

        /// <summary>
        ///     构造作为一个关联树向下访问者断言关联树是否为异构的
        /// </summary>
        /// <param name="provider">异构断言提供者</param>
        public AssociationTreeHeterogeneityPredicater(HeterogeneityPredicationProvider provider)
        {
            _provider = provider == null ? new StorageHeterogeneityPredicationProvider() : provider;
        }

        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        public bool Result => _heterogeneous;

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        void IAssociationTreeDownwardVisitor.Postvisit(AssociationTree subTree, object parentState,
            object previsitState)
        {
            //nothing to do
        }

        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        bool IAssociationTreeDownwardVisitor.Previsit(AssociationTree subTree, object parentState,
            out object outParentState, out object outPrevisitState)
        {
            outParentState = null;
            outPrevisitState = null;
            //如果已经是异构的，则不再继续访问
            if (_heterogeneous)
                return false;
            //根节点 注册根节点
            if (subTree.IsRoot)
            {
                _provider.RegisterRoot(subTree.Node);
                return true;
            }

            //进行比较
            var result = _provider.Compare(subTree.Node);
            if (result) return true;

            _heterogeneous = true;
            return false;
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
            //nothing to do
        }
    }
}