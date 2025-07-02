/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图属性建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:37:13
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     视图属性建造器。
    /// </summary>
    public class ViewAttributeBuilder : ViewElementBuilder
    {
        /// <summary>
        ///     类成员绑定的表达式
        /// </summary>
        private MemberExpression _expression;

        /// <summary>
        ///     创建ViewAttributeBuilder实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        public ViewAttributeBuilder(ObjectDataModel model)
            : base(model)
        {
        }

        /// <summary>
        ///     实例化类型元素，同时根据需要扩展视图源。
        /// </summary>
        /// <param name="member">与元素对应的类成员。</param>
        /// <param name="expression">类成员绑定的表达式。</param>
        /// <param name="sourceExtension">视图源扩展树。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public override void Instantiate(MemberInfo member, Expression expression, AssociationTree sourceExtension,
            ParameterBinding[] paraBindings = null)
        {
            var memberExps = new MemberExpressionExtractor(new SubTreeEvaluator(expression)).ExtractMember(expression);
            var sources = new List<ViewAttributeSource>();
            //提取成员表达式的关联树
            foreach (var memberExp in memberExps)
            {
                var assTail = memberExp.GrowAssociationTree(sourceExtension, _model, out AttributeTreeNode attrTail,
                    paraBindings);
                //用关联树创建视图属性源
                var source = new ViewAttributeSource(assTail, attrTail.Attribute, memberExp);
                sources.Add(source);
                _expression = memberExp;
            }

            var result = new ViewAttribute(member.Name, expression, sources.ToArray());
            _element = result;
        }

        /// <summary>
        ///     设置映射字段。
        /// </summary>
        /// <param name="member">与元素对应的类成员。</param>
        public override void SetTargetField(MemberInfo member)
        {
            var host = _model.GetStructuralType(_expression.Expression.Type);
            var attr = host.GetAttribute(_expression.Member.Name);
            //映射字段名与成员名相同
            if (_element is Attribute attribute)
                attribute.TargetField =
                    string.Equals(member.Name, attr.TargetField, StringComparison.CurrentCultureIgnoreCase)
                        ? member.Name
                        : attr.TargetField;
        }
    }
}