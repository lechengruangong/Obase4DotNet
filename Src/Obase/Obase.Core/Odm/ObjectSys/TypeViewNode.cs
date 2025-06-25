/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：代表类型视图的节点.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:39:45
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联树中代表类型视图的节点。
    /// </summary>
    public class TypeViewNode : AssociationTreeNode
    {
        /// <summary>
        ///     创建TypeViewNode实例。
        /// </summary>
        /// <param name="viewType">节点代表的类型视图。</param>
        internal TypeViewNode(TypeView viewType) : base(viewType)
        {
        }

        /// <summary>
        ///     获取上级节点。
        /// </summary>
        public override AssociationTreeNode Parent
        {
            get => null;
            internal set { }
        }

        /// <summary>
        ///     获取根节点。
        /// </summary>
        public override AssociationTreeNode Root => this;
    }
}