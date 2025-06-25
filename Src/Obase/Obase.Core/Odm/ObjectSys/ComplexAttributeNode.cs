/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：代表复杂属性的节点.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:28:02
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     代表复杂属性的节点。
    /// </summary>
    public class ComplexAttributeNode : AttributeTreeNode
    {
        /// <summary>
        ///     子节点。（子节点较少，不使用哈希存储）
        /// </summary>
        private readonly IDictionary<string, AttributeTreeNode> _children;

        /// <summary>
        ///     创建ComplexAttributeNode实例。
        /// </summary>
        /// <param name="attribute">节点代表的属性。</param>
        internal ComplexAttributeNode(Attribute attribute) : base(attribute)
        {
            _children = new Dictionary<string, AttributeTreeNode>();
        }

        /// <summary>
        ///     获取所有子节点。
        /// </summary>
        public List<AttributeTreeNode> Children => _children?.Values.ToList();

        /// <summary>
        ///     添加子节点。
        /// </summary>
        /// <param name="child">要添加的子节点。</param>
        public AttributeTreeNode AddChild(AttributeTreeNode child)
        {
            child.Parent = this;
            _children[child.Attribute.Name] = child;
            return this;
        }

        /// <summary>
        ///     获取代表指定属性的子节点。
        /// </summary>
        /// <param name="attrName">属性名称。</param>
        public AttributeTreeNode GetChild(string attrName)
        {
            if (_children.TryGetValue(attrName, out var child))
                return child;
            return null;
        }


        /// <summary>
        ///     移除代表指定属性的子节点，并返回该节点。
        /// </summary>
        /// <param name="attrName">属性名称。</param>
        public AttributeTreeNode RemoveChild(string attrName)
        {
            AttributeTreeNode node = null;
            //如果不存在该属性，则返回null
            if (_children.ContainsKey(attrName))
            {
                node = _children[attrName];
                _children.Remove(attrName);
            }

            return node;
        }
    }
}