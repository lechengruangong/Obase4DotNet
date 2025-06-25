/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树节点.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:28:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联树节点。
    /// </summary>
    public abstract class AssociationTreeNode
    {
        /// <summary>
        ///     子节点。（子字典较少，不使用哈希存储）
        /// </summary>
        private readonly Dictionary<string, ObjectTypeNode> _children;

        /// <summary>
        ///     节点所代表的类型。
        /// </summary>
        private readonly ReferringType _representedType;

        /// <summary>
        ///     关联树。{ 创建关联树后寄存，避免重复创建。}
        /// </summary>
        private AssociationTree _tree;

        /// <summary>
        ///     创建AssociationTreeNode实例。
        /// </summary>
        /// <param name="representedType">节点所代表的类型。</param>
        protected AssociationTreeNode(ReferringType representedType)
        {
            _children = new Dictionary<string, ObjectTypeNode>();
            _representedType = representedType;
        }

        /// <summary>
        ///     获取当前节点所代表的类型。
        /// </summary>
        public ReferringType RepresentedType => _representedType;

        /// <summary>
        ///     获取当前节点的所有子节点。
        /// </summary>
        public ObjectTypeNode[] Children => _children?.Values.ToArray() ?? Array.Empty<ObjectTypeNode>();

        /// <summary>
        ///     获取获取根节点。
        /// </summary>
        public abstract AssociationTreeNode Root { get; }


        /// <summary>
        ///     获取当前节点的父级节点。
        /// </summary>
        public abstract AssociationTreeNode Parent { get; internal set; }

        /// <summary>
        ///     获取一个值，该值指示当前节点是否为根节点。
        /// </summary>
        public bool IsRoot => Parent == null;


        /// <summary>
        ///     获取代表指定元素的子节点。
        /// </summary>
        /// <param name="elementName">要获取其对应子树的元素名称。</param>
        public ObjectTypeNode GetChild(string elementName)
        {
            if (_children.TryGetValue(elementName, out var child))
                return child;
            return null;
        }

        /// <summary>
        ///     为当前节点添加子节点，如果指定元素名称，则将子节点的元素名称强制更改为指定的名称。
        ///     说明：如果子节点的ElementName属性为空且未指定新名称，引发异常。
        ///     如果已存在同名子节点（以新名称为准），不执行添加操作。
        /// </summary>
        /// <exception cref="ArgumentException">被添加的关联树只能作为根节点。</exception>
        /// <param name="child">要添加的子节点。</param>
        /// <param name="elementName">子节点所代表的元素的名称。</param>
        /// <returns>返回刚添加的子节点；如果存在同名子节点，返回已存在的子节点。</returns>
        public ObjectTypeNode AddChild(ObjectTypeNode child, string elementName = null)
        {
            var key = elementName ?? child.ElementName;
            if (key == null)
                throw new ArgumentException("被添加的关联树只能作为根节点。");
            //不指定节点名称 将子节点的元素名称强制更改为指定的名称
            if (elementName != null) child.ElementName = elementName;
            if (_children.TryGetValue(key, out var child1))
            {
                //已有同名子节点
                child = child1;
            }
            else
            {
                //如果是投影到某一个已有的关联树节点而出现的同类型但不同名的
                var refferingTypeRepeated =
                    _children.Values.FirstOrDefault(p =>
                        p.RepresentedType == child.RepresentedType && p.Element is ViewReference);
                if (refferingTypeRepeated != null)
                {
                    child = _children[refferingTypeRepeated.ElementName];
                }
                else
                {
                    _children[key] = child;
                    //设置子节点的Parent
                    child.Parent = this;
                }
            }

            return child;
        }

        /// <summary>
        ///     为当前节点批量添加子节点。
        ///     说明：
        ///     如果子节点的ElementName属性为空，引发异常。如果已存在同名子节点，不执行添加操作。
        /// </summary>
        /// <exception cref="ArgumentException">被添加的关联树只能作为根节点。</exception>
        /// <param name="children">要添加的子节点。</param>
        public void AddChild(ObjectTypeNode[] children)
        {
            //循环处理所有子节点
            foreach (var child in children)
            {
                if (string.IsNullOrEmpty(child.ElementName))
                    throw new ArgumentException("子节点的ElementName属性为空且未指定新名称。");
                if (!_children.ContainsKey(child.ElementName))
                {
                    _children[child.ElementName] = child;
                    //设置子节点的Parent
                    child.Parent = this;
                }
            }
        }

        /// <summary>
        ///     移除代表指定元素的子节点，然后返回该节点。
        /// </summary>
        /// <param name="elementName">要获取其对应子树的元素名称。</param>
        public ObjectTypeNode RemoveChild(string elementName)
        {
            ObjectTypeNode node = null;
            //如果有符合的子节点
            if (_children.ContainsKey(elementName))
            {
                node = _children[elementName];
                _children.Remove(elementName);
            }

            return node;
        }

        /// <summary>
        ///     将节点视为一棵关联树。
        ///     实施注意：创建关联树后寄存，避免重复创建。
        /// </summary>
        public AssociationTree AsTree()
        {
            return _tree ?? (_tree = new AssociationTree(this));
        }

        /// <summary>
        ///     克隆关联树节点得到一个孤立节点，即不引用父节点和子节点。
        /// </summary>
        public AssociationTreeNode CloneAlone()
        {
            //克隆
            AssociationTreeNode clone = null;
            //分类
            if (this is ObjectTypeNode objectTypeNode)
                clone = new ObjectTypeNode((ObjectType)objectTypeNode.RepresentedType, objectTypeNode.ElementName);
            else if (this is TypeViewNode typeViewNode)
                clone = new TypeViewNode((TypeView)typeViewNode.RepresentedType);
            return clone;
        }

        /// <summary>
        ///     检测当前节点是否有代表指定元素的子节点。
        /// </summary>
        /// <param name="elementName">子节点代表元素的名称。</param>
        public bool HasChild(string elementName)
        {
            return _children.ContainsKey(elementName);
        }
    }
}