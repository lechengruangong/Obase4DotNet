/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性树.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:19:17
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     属性树。
    ///     属性树是以一个复杂属性为根节点，以其类型的属性为子节点，层层嵌套，生成的树形结构。
    /// </summary>
    public class AttributeTree
    {
        /// <summary>
        ///     属性树的节点层级结构（根节点为当前子树的根）。
        /// </summary>
        private readonly AttributeTreeNode _node;

        /// <summary>
        ///     父级树
        /// </summary>
        private readonly AttributeTree _parent;


        /// <summary>
        ///     创建代表指定属性的AttributeTree实例。
        /// </summary>
        /// <param name="attribute">代表属性。</param>
        public AttributeTree(Attribute attribute)
        {
            //根据是否为复杂属性构造
            if (attribute is ComplexAttribute complexAttribute)
                _node = new ComplexAttributeNode(complexAttribute);
            else _node = new SimpleAttributeNode(attribute);
        }

        /// <summary>
        ///     使用指定的节点层级结构（根节点为树根）创建AttributeTree实例。
        /// </summary>
        /// <param name="treeNode">节点层级结构。</param>
        internal AttributeTree(AttributeTreeNode treeNode)
        {
            _node = treeNode;
            if (_node.Parent != null)
                _parent = new AttributeTree(_node.Parent);
        }


        /// <summary>
        ///     获取属性树代表的属性。
        /// </summary>
        public Attribute Attribute => _node.Attribute;

        /// <summary>
        ///     获取代表属性的名称。
        /// </summary>
        public string AttributeName
        {
            get
            {
                if (_node.Attribute is ViewAttribute viewAttribute)
                {
                    if (viewAttribute.Shadow != null) return ((MemberExpression)viewAttribute.Binding).Member.Name;
                    return viewAttribute.Name;
                }

                return _node.Attribute.Name;
            }
        }

        /// <summary>
        ///     获取代表属性的模型类型。
        /// </summary>
        public TypeBase AttributeType => _node.AttributeType;

        /// <summary>
        ///     获取属性树的节点层级结构（根节点为当前子树的根）。
        /// </summary>
        public AttributeTreeNode Node => _node;

        /// <summary>
        ///     获取所有子树。
        /// </summary>
        public AttributeTree[] SubTrees
        {
            get
            {
                if (IsComplex)
                {
                    var node = (ComplexAttributeNode)_node;
                    return node.Children?.Select(p => p.AsTree()).ToArray() ?? Array.Empty<AttributeTree>();
                }

                return Array.Empty<AttributeTree>();
            }
        }

        /// <summary>
        ///     获取属性树的父级。
        /// </summary>
        public AttributeTree Parent => _parent;

        /// <summary>
        ///     获取一个值，该值指示属性树代表的属性是否为复杂属性。
        /// </summary>
        public bool IsComplex => _node.Attribute.IsComplex;

        /// <summary>
        ///     为当前节点添加子树。如果已存在同名子树，不执行添加操作。
        /// </summary>
        /// <param name="subTree">要添加的子树。</param>
        /// <returns>返回刚添加的子树；如果存在同名子树，返回已存在的子树。</returns>
        public AttributeTree AddSubTree(AttributeTree subTree)
        {
            if (!(_node is ComplexAttributeNode complexAttribute)) throw new Exception("非代表复杂属性的节点，不能添加子树");
            //查询同名子树
            var subTreeChild = complexAttribute.GetChild(subTree.AttributeName);
            //判断同名子树是否存在
            if (subTreeChild == null) subTreeChild = complexAttribute.AddChild(subTree.Node);
            return subTreeChild.AsTree();
        }

        /// <summary>
        ///     获取代表指定属性的子树。
        /// </summary>
        /// <param name="attrName">属性名称。</param>
        public AttributeTree GetSubTree(string attrName)
        {
            if (_node is ComplexAttributeNode complexAttribute)
            {
                var subTreeChild = complexAttribute.GetChild(attrName);
                return subTreeChild?.AsTree();
            }

            return null;
        }

        /// <summary>
        ///     移除代表指定属性的子树，并返回该子树。
        /// </summary>
        /// <param name="attrName">属性名称。</param>
        public void RemoveSubTree(string attrName)
        {
            if (_node is ComplexAttributeNode complexAttribute) complexAttribute.RemoveChild(attrName);
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">访问者。</param>
        public void Accept(IAttributeTreeDownwardVisitor visitor)
        {
            Accept(visitor, null);
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">访问者。</param>
        public TResult Accept<TResult>(IAttributeTreeDownwardVisitor<TResult> visitor)
        {
            Accept(visitor, null);
            return visitor.Result;
        }

        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">访问者。</param>
        public void Accept(IAttributeTreeUpwardVisitor visitor)
        {
            Accept(visitor, null);
        }

        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">访问者。</param>
        public TResult Accept<TResult>(IAttributeTreeUpwardVisitor<TResult> visitor)
        {
            Accept(visitor, null);
            return visitor.Result;
        }

        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">访问者。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        private void Accept(IAttributeTreeUpwardVisitor visitor, object childState)
        {
            //执行前置访问
            visitor.Previsit(this, childState, out var outChildState, out var outPrevisitState);
            //向上遍历
            if (Parent != null)
                Parent.Accept(visitor, outChildState);
            //执行后置访问
            visitor.Postvisit(this, outChildState, outPrevisitState);
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">访问者。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        private void Accept(IAttributeTreeDownwardVisitor visitor, object parentState)
        {
            //执行前置访问
            visitor.Previsit(this, parentState, out var outParentState, out var outPrevisitState);
            //向下遍历
            foreach (var subTree in SubTrees ?? Array.Empty<AttributeTree>())
                subTree.Accept(visitor, outParentState);
            //执行后置访问
            visitor.Postvisit(this, outParentState, outPrevisitState);
        }
    }
}