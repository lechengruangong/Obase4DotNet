/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图复杂属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:18:39
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     视图复杂属性。
    ///     视图复杂属性来源于源（或源扩展）的一个复杂属性。
    /// </summary>
    public class ViewComplexAttribute : ComplexAttribute, ITypeViewElement
    {
        /// <summary>
        ///     复杂属性的锚（或称锚点）。
        ///     锚点是源扩展树上的一个节点，视图复杂属性即来源于该节点代表类型的某个复杂属性。
        /// </summary>
        private readonly AssociationTreeNode _anchor;

        /// <summary>
        ///     复杂属性绑定。
        ///     绑定是一个属性树节点，该节点所代表的复杂属性即是视图复杂属性的来源。
        /// </summary>
        private readonly ComplexAttributeNode _binding;

        /// <summary>
        ///     影子元素。
        /// </summary>
        private ViewComplexAttribute _shadow;

        /// <summary>
        ///     创建ViewComplexAttribute实例。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="anchor">复杂属性锚。</param>
        /// <param name="binding">复杂属性绑定。</param>
        public ViewComplexAttribute(string name, AssociationTreeNode anchor, AttributeTreeNode binding)
            : base(anchor.RepresentedType.ClrType, name, new ComplexType(anchor.RepresentedType.ClrType))
        {
            _anchor = anchor;
            _binding = new ComplexAttributeNode(binding.Attribute);
        }

        /// <summary>
        ///     实例化ViewComplexAttribute实例，该实例表示的视图复杂属性锚定于源扩展树根节点。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="binding">复杂属性绑定。</param>
        public ViewComplexAttribute(string name, AttributeTreeNode binding)
            : base(binding.Attribute.HostType.ClrType, name, new ComplexType(binding.Attribute.HostType.ClrType))
        {
            _anchor = new ObjectTypeNode(new EntityType(binding.Attribute.HostType.ClrType));
            _binding = new ComplexAttributeNode(binding.Attribute);
        }

        /// <summary>
        ///     获取或设置视图复杂属性的影子元素。
        /// </summary>
        internal ViewComplexAttribute Shadow
        {
            get => _shadow;
            set => _shadow = value;
        }

        /// <summary>
        ///     获取复杂属性的锚（或称锚点）。
        /// </summary>
        public AssociationTreeNode Anchor => _anchor;

        /// <summary>
        ///     获取复杂属性绑定。
        /// </summary>
        public ComplexAttributeNode Binding => _binding;

        /// <summary>
        ///     影子属性
        /// </summary>
        ITypeViewElement ITypeViewElement.Shadow
        {
            get => _shadow;
            set => _shadow = (ViewComplexAttribute)value;
        }


        /// <summary>
        ///     生成在视图表达式中定义复杂属性的表达式，它规定了属性的锚点和绑定。
        /// </summary>
        /// <param name="sourcePara">源参数表达式</param>
        /// <param name="flatteningParaGetter">获取关联树平展点的委托</param>
        /// <returns> 定义当前复杂属性的表达式。</returns>
        /// 实施说明:
        /// 使用AssociationExpressionGenerator和AttributeExpressionGenerator生成表达式。
        public Expression GenerateExpression(ParameterExpression sourcePara,
            Func<AssociationTreeNode, ParameterExpression> flatteningParaGetter)
        {
            var typeExpGenerator = new AssociationExpressionGenerator(sourcePara, flatteningParaGetter);
            var hostExp = _anchor.AsTree().Accept(typeExpGenerator);
            //使用属性树表达式生成器生成
            var generator = new AttributeExpressionGenerator(hostExp);
            _binding.AsTree().Accept(generator);
            return generator.Result;
        }
    }
}