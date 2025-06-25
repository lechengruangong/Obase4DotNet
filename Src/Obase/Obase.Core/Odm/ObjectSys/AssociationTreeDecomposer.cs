/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树极限分解器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:00:37
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     作为一个关联树向下访问者对关联树实施极限分解，访问操作以是否克隆附加树为输入参数，以基础树为返回值，以附加项集合为输出参数。
    ///     [定义]从关联树中移除任意一棵子树，把该子树视为另一棵关联树，这一过程则为关联树分解。
    ///     移除子树后剩余部分仍然是一棵关联树，称为基础树，该子树称为附加树。
    ///     附加树的根节点在原树中的父节点称为该附加树的附加节点，在原树中代表的引用元素称为该附加树的附加引用。
    ///     [定义]如果一个分解方案对一棵关联树实施一次或连续实施多次分解，使得基础树是同构的而且其包含尽可能多的节点，则称该分解方案为关联树的极限分解。
    ///     警告
    ///     如果关联树根为TypeViewNode，不会检测该视图是否为异构，直接依据其终极源（考虑视图嵌套情形）判定根节点的存储标记。
    ///     警告
    ///     不会事先检查关联树是否为异构的，如果关联树不是异构的，将克隆整棵树作为基础树。建议执行分解操作前先确保关联树是异构的。
    /// </summary>
    /// 实施说明:
    /// （1）采用显式接口实现，相关方法定义为私有。
    /// （2）返回附加项时，如果调用方要求克隆，使用AssociationCloner执行克隆操作，参见活动图“获取附加树”。
    /// （3）寄存已生成的附加项，避免重复操作。
    public class AssociationTreeDecomposer : IParameterizedAssociationTreeDownwardVisitor<bool, AssociationTree,
        AssociationTreeAttachingItem[]>
    {
        /// <summary>
        ///     实施极限分解得到的附加树（未复制）的根节点。
        /// </summary>
        private readonly List<AssociationTreeAttachingItem> _attachingItems = new List<AssociationTreeAttachingItem>();

        /// <summary>
        ///     关联树异构断言提供程序
        /// </summary>
        private readonly HeterogeneityPredicationProvider _provider;

        /// <summary>
        ///     实施极限分解得到的基础树。
        /// </summary>
        private AssociationTreeNode _baseTree;

        /// <summary>
        ///     指示是否复制附加树，即依次克隆附加树的节点生成一棵新树。
        /// </summary>
        private bool _cloningAttachingTree;

        /// <summary>
        ///     作为一个关联树向下访问者对关联树实施极限分解
        /// </summary>
        /// <param name="provider"></param>
        public AssociationTreeDecomposer(HeterogeneityPredicationProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        ///     获取输出参数的值。
        /// </summary>
        public AssociationTreeAttachingItem[] OutArgument =>
            _attachingItems.Count <= 0 ? null : _attachingItems.ToArray();

        /// <summary>
        ///     为即将开始的遍历操作设置参数。
        /// </summary>
        /// <param name="argument">参数值。</param>
        public void SetArgument(bool argument)
        {
            _cloningAttachingTree = argument;
        }

        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        public AssociationTree Result => _baseTree.AsTree();

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        void IAssociationTreeDownwardVisitor.Postvisit(AssociationTree subTree, object parentState,
            object previsitState)
        {
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
            outParentState = outPrevisitState = null;
            if (parentState == null)
            {
                var clone = subTree.Node.CloneAlone();
                //克隆根节点。
                _baseTree = clone;
                //寄存根节点关注特性
                _provider.RegisterRoot(subTree.Root.Node);
                //克隆根节点。
                outParentState = clone;
                return true;
            }

            var result = _provider.Compare(subTree.Node);
            if (!result)
            {
                //创建并寄存附加项。
                NewAttachingItem(subTree, (AssociationTreeNode)parentState);
                return false;
            }

            //克隆当前节点。
            outParentState = subTree.Node.CloneAlone();
            //添加为子节点。
            ((AssociationTreeNode)parentState).AddChild((ObjectTypeNode)outParentState);
            return true;
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
        }

        /// <summary>
        ///     创建并寄存附加项。
        /// </summary>
        /// <param name="attachingTree">附加关联树树（未复制）。</param>
        /// <param name="attachingNode">附加节点。</param>
        private void NewAttachingItem(AssociationTree attachingTree, AssociationTreeNode attachingNode)
        {
            //获取附加引用
            var attchingRefence = attachingNode.RepresentedType.GetReferenceElement(attachingTree.Element.Name);
            AssociationTreeAttachingItem attachingItem;
            if (_cloningAttachingTree)
            {
                var cloneAttachingTree = attachingTree.Accept(new AssociationTreeCloner()).AsTree();
                //创建附加项
                attachingItem = new AssociationTreeAttachingItem(cloneAttachingTree, attachingNode, attchingRefence);
            }
            else
            {
                //创建附加项
                attachingItem = new AssociationTreeAttachingItem(attachingTree, attachingNode, attchingRefence);
            }

            _attachingItems.Add(attachingItem);
            //确保引用键
            attchingRefence.GetReferringKey();
            //确保参考键
            attchingRefence.GetReferredKey();
        }
    }
}