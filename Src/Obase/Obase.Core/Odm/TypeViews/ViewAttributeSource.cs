/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图属性的源.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:18:12
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     表示视图属性的源。
    ///     属性源由扩展树节点（可为根节点）和节点上的一个属性（直接属性或子属性）构成，该属性是视图属性值的来源或来源之一。
    ///     如果视图属性的值来源于一个属性，该视图属性称为单源属性；
    ///     如果是由两个或多个属性经计算得来的，则称为多源属性。
    ///     为便于表述，若某视图属性VA的源为{节点N, 属性A}，称VA锚定于N（或称N为其锚点），绑定于A。如果VA为多源属性，它有多个锚点、多个绑定。
    /// </summary>
    public class ViewAttributeSource
    {
        /// <summary>
        ///     一个代表某属性的属性树节点，该属性与某一扩展树节点共同构成属性源。
        /// </summary>
        private readonly SimpleAttributeNode _attributeNode;

        /// <summary>
        ///     一个扩展树节点，与定位于其上的某一属性共同构成属性源。
        /// </summary>
        private readonly AssociationTreeNode _extensionNode;

        /// <summary>
        ///     表征属性源的表达式，简称源表达式。
        /// </summary>
        private readonly MemberExpression _representor;

        /// <summary>
        ///     属性源代理，即定义在基础视图或附加视图上的直观属性，它的值来自于原视图上的某一属性源（已分解到基础视图或附加视图），因而它可以代表该源参与属性绑定表达式的计算。
        /// </summary>
        private ViewAttribute _agent;

        /// <summary>
        ///     创建表示由指定锚点和属性构成的属性源的ViewAttributeSource实例。
        /// </summary>
        /// <param name="anchor">构成属性源的锚点。</param>
        /// <param name="attribute">构成属性源的属性。</param>
        /// <param name="representor">源表达式。</param>
        public ViewAttributeSource(AssociationTreeNode anchor, Attribute attribute, MemberExpression representor)
        {
            _representor = representor;
            _attributeNode = new SimpleAttributeNode(attribute);
            _extensionNode = anchor;
        }

        /// <summary>
        ///     创建表示由根节点作为锚点和指定属性构成的属性源的ViewAttributeSource实例。
        /// </summary>
        /// <param name="attribute">构成属性源的属性。</param>
        /// <param name="representor">源表达式。</param>
        public ViewAttributeSource(Attribute attribute, MemberExpression representor)
            : this(new ObjectTypeNode((ObjectType)attribute.HostType), attribute, representor)
        {
        }

        /// <summary>
        ///     属性源的代理属性，简称属性源代理。
        ///     属性源代理是定义在基础视图或附加视图上的直观属性，它的值来自于原视图上的某一属性源（已分解到基础视图或附加视图），因而它可以代表该源参与属性绑定表达式的计算。
        /// </summary>
        internal ViewAttribute Agent
        {
            get => _agent;
            set => _agent = value;
        }

        /// <summary>
        ///     获取一个代表某属性的属性树节点，该属性与某一扩展树节点共同构成属性源。
        /// </summary>
        public SimpleAttributeNode AttributeNode => _attributeNode;

        /// <summary>
        ///     获取一个扩展树节点，与定位于其上的某一属性共同构成属性源。
        /// </summary>
        public AssociationTreeNode ExtensionNode => _extensionNode;

        /// <summary>
        ///     获取表征属性源的表达式。
        /// </summary>
        public MemberExpression Representor => _representor;
    }
}