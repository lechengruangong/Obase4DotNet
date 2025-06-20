/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：谓词条件组合器,增加了逻辑或和逻辑与的扩展方法.                                                    
│　作   者：Obase开发团队                                              
│　版权所有：武汉乐程软工科技有限公司                                                 
│　创建时间：2025-6-20 11:29:31                            
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Obase.Core.Common
{
    /// <summary>
    ///     谓词条件组合器
    /// </summary>
    public static class PredicateCombiner
    {
        /// <summary>
        ///     逻辑或运算
        /// </summary>
        /// <returns>逻辑或运算后结果表达式</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression,
            Expression<Func<T, bool>> otherExpression)
        {
            return MergeLambda(expression, otherExpression, Expression.OrElse);
        }

        /// <summary>
        ///     逻辑与运算
        /// </summary>
        /// <returns>逻辑与运算后结果表达式</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression,
            Expression<Func<T, bool>> otherExpression)
        {
            return MergeLambda(expression, otherExpression, Expression.AndAlso);
        }

        /// <summary>
        ///     具体的合并方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first">左侧</param>
        /// <param name="second">右侧</param>
        /// <param name="mergeDlegeate">合并委托</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static Expression<Func<T, bool>> MergeLambda<T>(Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second, Func<Expression, Expression, BinaryExpression> mergeDlegeate)
        {
            if (first == null && second == null)
                throw new ArgumentNullException("合并表达参数异常,要合并的表达式均为空.", innerException: null);
            if ((first == null) ^ (second == null)) return first ?? second;

            var map = first.Parameters.Select((fPar, i) => new { fPar, sPar = second.Parameters[i] })
                .ToDictionary(p => p.sPar, p => p.fPar);
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<Func<T, bool>>(mergeDlegeate(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        ///     Lambda表达式参数重绑定器
        /// </summary>
        private class ParameterRebinder : ExpressionVisitor
        {
            /// <summary>
            ///     重绑定字典
            /// </summary>
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

            /// <summary>
            ///     初始化Lambda表达式参数重绑定器
            /// </summary>
            /// <param name="map"></param>
            private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            /// <summary>
            ///     替换参数
            /// </summary>
            /// <param name="map">重绑定字典</param>
            /// <param name="exp">要替换的表达式</param>
            /// <returns></returns>
            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,
                Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            /// <summary>
            ///     访问参数表达式
            /// </summary>
            /// <param name="node">参数表达式</param>
            /// <returns></returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (_map.TryGetValue(node, out var replacement))
                    node = replacement;
                return base.VisitParameter(node);
            }
        }
    }
}