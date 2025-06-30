/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型视图构造器工厂.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:33:49
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     类型视图构造器工厂，根据视图表达式创建特定的构造器实例。
    /// </summary>
    public class TypeViewBuilderFactory
    {
        /// <summary>
        ///     创建类型视图构造器实例。
        /// </summary>
        /// <param name="viewExp">视图表达式。</param>
        public ITypeViewBuilder Create(Expression viewExp)
        {
            if (viewExp is LambdaExpression lambda)
                switch (lambda.Body.NodeType)
                {
                    case ExpressionType.New:
                        return new NewExpressionBasedBuilder();
                    case ExpressionType.MemberInit:
                        return new MemberInitExpressionBasedBuilder();
                }

            throw new ArgumentException($"创建类型视图构造器实例时表达式不合法({viewExp})");
        }
    }
}