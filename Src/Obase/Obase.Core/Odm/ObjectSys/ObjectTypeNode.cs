/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树中代表对象类型的节点.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:31:28
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联树中代表对象类型的节点。
    /// </summary>
    public class ObjectTypeNode : AssociationTreeNode
    {
        /// <summary>
        ///     节点代表的对象元素的名称。
        /// </summary>
        private string _elementName;

        /// <summary>
        ///     父节点。
        /// </summary>
        protected AssociationTreeNode _parent;

        /// <summary>
        ///     创建ObjectTypeNode实例。
        /// </summary>
        /// <param name="objType">节点代表的对象类型。</param>
        /// <param name="elementName">节点代表的元素的名称。</param>
        internal ObjectTypeNode(ObjectType objType, string elementName = null) : base(objType)
        {
            _elementName = elementName;
        }

        /// <summary>
        ///     获取关联树节点代表的类型元素。
        /// </summary>
        public ReferenceElement Element => Parent?.RepresentedType.GetReferenceElement(ElementName);

        /// <summary>
        ///     获取节点代表的对象元素的名称。
        /// </summary>
        public string ElementName
        {
            get => _elementName;
            internal set => _elementName = value;
        }

        /// <summary>
        ///     获取上级节点。
        /// </summary>
        public override AssociationTreeNode Parent
        {
            get => _parent;
            internal set => _parent = value;
        }

        /// <summary>
        ///     获取根节点。
        /// </summary>
        public override AssociationTreeNode Root
        {
            get
            {
                //自己就是根节点
                if (_parent == null)
                    return this;
                return _parent.Root;
            }
        }
    }
}