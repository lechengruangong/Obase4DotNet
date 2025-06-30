/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于MemberInitExpression的视图构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:01:48
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     基于MemberInitExpression的视图构造器。
    /// </summary>
    public class MemberInitExpressionBasedBuilder : ITypeViewBuilder
    {
        /// <summary>
        ///     构造类型视图。
        /// </summary>
        /// <param name="viewExp">视图表达式。</param>
        /// <param name="source">视图源。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="sourcePara">视图表达式中代表视图源的形式参数。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public TypeView Build(Expression viewExp, StructuralType source, ObjectDataModel model,
            ParameterExpression sourcePara,
            params ParameterBinding[] paraBindings)
        {
            if (!(viewExp is MemberInitExpression initExp))
                throw new ArgumentException($"无法抽取MemberInitExpression的视图,表达式（{viewExp}）不合法。");
            //使用NewExpressionBasedBuilder构建NewExpression的视图
            var newExp = initExp.NewExpression;
            var typeView = new NewExpressionBasedBuilder().Build(newExp, source, model, sourcePara, paraBindings);
            //增加视图元素
            var adder = new ViewElementAdder(typeView, model);
            //处理绑定
            var bindings = initExp?.Bindings;
            foreach (var binding in bindings)
                if (binding is MemberAssignment bindingMember)
                    adder.AddElement(bindingMember.Member, bindingMember.Expression, paraBindings);
            return typeView;
        }
    }
}