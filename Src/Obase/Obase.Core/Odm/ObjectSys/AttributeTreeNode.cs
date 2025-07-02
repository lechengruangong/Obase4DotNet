/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性树的节点.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:20:42
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     属性树节点。
    /// </summary>
    public abstract class AttributeTreeNode
    {
        /// <summary>
        ///     节点所代表的属性。
        /// </summary>
        private readonly Attribute _attribute;

        /// <summary>
        ///     属性树。{ 创建属性树后寄存，避免重复创建。}
        /// </summary>
        private AttributeTree _node;

        /// <summary>
        ///     父节点。
        /// </summary>
        private ComplexAttributeNode _parent;

        /// <summary>
        ///     构造属性树节点实例。
        /// </summary>
        /// <param name="attribute">节点代表的属性</param>
        protected AttributeTreeNode(Attribute attribute)
        {
            _attribute = attribute;
        }

        /// <summary>
        ///     获取节点代表的属性。
        /// </summary>
        public Attribute Attribute => _attribute;

        /// <summary>
        ///     获取节点代表属性的名称。
        /// </summary>
        public string AttributeName => _attribute.Name;

        /// <summary>
        ///     获取父节点。
        /// </summary>
        public ComplexAttributeNode Parent
        {
            get => _parent;
            internal set => _parent = value;
        }

        /// <summary>
        ///     获取节点代表的属性的模型类型。
        /// </summary>
        public TypeBase AttributeType => _attribute.HostType.Model?.GetType(_attribute.DataType);

        /// <summary>
        ///     将节点视为一棵属性树。
        ///     实施注意：创建属性树后寄存，避免重复创建。
        /// </summary>
        public AttributeTree AsTree()
        {
            return _node ?? (_node = new AttributeTree(this));
        }

        /// <summary>
        ///     重写是否相等
        /// </summary>
        /// <param name="obj">另一个属性树节点</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            //转换为AttributeTreeNode
            var attrTreenode = obj as AttributeTreeNode;
            if (attrTreenode == null)
                return false;
            //引用是否相等
            if (ReferenceEquals(attrTreenode, this)) return true;

            var parent = Parent;
            //有父级 比较父级
            if (parent != null)
                //父级不相等
                if (!ReferenceEquals(parent, Parent))
                    return false;
            //比较属性
            return Attribute.Equals(attrTreenode.Attribute);
        }

        /// <summary>
        ///     重写获取Hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _attribute != null ? _attribute.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_node != null ? _node.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_parent != null ? _parent.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}