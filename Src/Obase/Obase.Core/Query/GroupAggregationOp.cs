/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Group聚合运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:54:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Group（聚合）运算。
    /// </summary>
    public class GroupAggregationOp : GroupOp
    {
        /// <summary>
        ///     聚合投影函数，用于对每个组生成聚合值。
        /// </summary>
        private readonly LambdaExpression _resultSelector;

        /// <summary>
        ///     创建GroupAggregationOp实例。
        /// </summary>
        /// <param name="resultSelector">聚合投影函数，用于对每个组生成聚合值。</param>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="elementSelector">组元素函数，用于从每个元素提取组元素。</param>
        internal GroupAggregationOp(LambdaExpression resultSelector, LambdaExpression keySelector,
            ObjectDataModel model,
            LambdaExpression elementSelector = null)
            : base(keySelector, model, elementSelector)
        {
            _resultSelector = resultSelector;
        }

        /// <summary>
        ///     创建GroupAggregationOp实例。
        /// </summary>
        /// <param name="resultSelector">聚合投影函数，用于对每个组生成聚合值。</param>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵。</param>
        /// <param name="comparer">相等比较器，用于测试两个分组鍵是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="elementSelector">组元素函数，用于从每个元素提取组元素。</param>
        internal GroupAggregationOp(LambdaExpression resultSelector, LambdaExpression keySelector,
            IEqualityComparer comparer, ObjectDataModel model, LambdaExpression elementSelector = null)
            : base(keySelector, comparer, model, elementSelector)
        {
            _resultSelector = resultSelector;
        }

        /// <summary>
        ///     获取一个值，该值表示是否为实例化聚合。
        ///     实例化聚合是指聚合投影函数的Body为New或MemberInit表达式。
        /// </summary>
        public bool IsNew
        {
            get
            {
                var type = _resultSelector.Body.NodeType;
                return type == ExpressionType.New || type == ExpressionType.MemberInit;
            }
        }

        /// <summary>
        ///     获取聚合投影函数，该函数用于对每个组生成聚合值。
        /// </summary>
        public LambdaExpression ResultSelector => _resultSelector;

        /// <summary>
        ///     获取聚合结果类型。
        /// </summary>
        public override Type ResultType => _resultSelector.ReturnType;
    }
}