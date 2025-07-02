/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：附加树及其附加节点和附加引用.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:56:07
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     表示附加树及其附加节点和附加引用。
    /// </summary>
    public class AssociationTreeAttachingItem
    {
        /// <summary>
        ///     附加节点。附加节点是基础树上的一个节点，在分解前的树中，它是附加树根的父节点。
        /// </summary>
        private readonly AssociationTreeNode _attachingNode;

        /// <summary>
        ///     附加引用。附加引用是附加节点代表类型的一个引用元素，在分解前的树中，它是附加树根节点代表的引用元素。
        /// </summary>
        private readonly ReferenceElement _attachingReference;

        /// <summary>
        ///     附加树。
        /// </summary>
        private readonly AssociationTree _attachingTree;

        /// <summary>
        ///     创建AssociationTreeAttachingItem实例。
        /// </summary>
        /// <param name="attachingTree">附加树。</param>
        /// <param name="attachingNode">附加节点。</param>
        /// <param name="attachingRef">附加引用。</param>
        public AssociationTreeAttachingItem(AssociationTree attachingTree, AssociationTreeNode attachingNode,
            ReferenceElement attachingRef)
        {
            _attachingTree = attachingTree;
            _attachingNode = attachingNode;
            _attachingReference = attachingRef;
        }

        /// <summary>
        ///     获取附加节点。
        /// </summary>
        public AssociationTreeNode AttachingNode => _attachingNode;

        /// <summary>
        ///     获取附加引用。
        /// </summary>
        public ReferenceElement AttachingReference => _attachingReference;

        /// <summary>
        ///     获取附加树。
        /// </summary>
        public AssociationTree AttachingTree => _attachingTree;
    }
}