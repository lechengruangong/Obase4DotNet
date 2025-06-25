/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：代表简单属性的节点.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:38:46
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     代表简单属性的节点。
    /// </summary>
    public class SimpleAttributeNode : AttributeTreeNode
    {
        /// <summary>
        ///     创建SimpleAttributeNode实例。
        /// </summary>
        /// <param name="attribute">节点代表的属性。</param>
        internal SimpleAttributeNode(Attribute attribute) : base(attribute)
        {
        }
    }
}