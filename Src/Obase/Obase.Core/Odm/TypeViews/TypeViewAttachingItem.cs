/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：附加视图及其附加节点和附加引用.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:06:13
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     表示对视图实施极限分解后得到的附加视图及其附加节点和附加引用。
    /// </summary>
    public class TypeViewAttachingItem
    {
        /// <summary>
        ///     附加节点。附加节点是基础视图源扩展树上的一个节点，在分解前的扩展树中，它是附加视图源扩展树根的父节点。
        /// </summary>
        private readonly AssociationTreeNode _attachingNode;

        /// <summary>
        ///     附加引用。附加视图是基础视图上的一个引用，它锚定于附加节点，绑定到附加视图源扩展树根节点在分解前的扩展树中所代表的引用元素。
        /// </summary>
        private readonly ViewReference _attachingReference;

        /// <summary>
        ///     附加视图。
        /// </summary>
        private readonly TypeView _attachingView;

        /// <summary>
        ///     创建TypeViewAttachingItem实例。
        /// </summary>
        /// <param name="attachingView">附加视图。</param>
        /// <param name="attachingNode">附加节点。</param>
        /// <param name="attachingRef">附加引用。</param>
        public TypeViewAttachingItem(TypeView attachingView, AssociationTreeNode attachingNode,
            ViewReference attachingRef)
        {
            _attachingView = attachingView;
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
        public ViewReference AttachingReference => _attachingReference;

        /// <summary>
        ///     获取附加视图。
        /// </summary>
        public TypeView AttachingView => _attachingView;
    }
}