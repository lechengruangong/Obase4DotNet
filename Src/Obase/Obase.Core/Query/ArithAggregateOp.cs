/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示ArithAggregate运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:17:45
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示ArithAggregate运算。
    /// </summary>
    public class ArithAggregateOp : AggregateOp
    {
        /// <summary>
        ///     算术聚合运算符。
        /// </summary>
        private readonly EAggregationOperator _operator;

        /// <summary>
        ///     投影函数，应用于每个元素然后以投影结果参与聚合。不指定投影函数则聚合元素自身。
        /// </summary>
        private readonly LambdaExpression _selector;

        /// <summary>
        ///     创建ArithAggregateOp实例。
        /// </summary>
        /// <param name="operator">算术聚合运算符。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="selector">投影函数，应用于每个元素然后以投影结果参与聚合。不指定投影函数则聚合元素自身。</param>
        internal ArithAggregateOp(EAggregationOperator @operator, ObjectDataModel model,
            LambdaExpression selector = null)
            : base(EQueryOpName.ArithAggregate, selector, model, selector?.Parameters[0].Type)
        {
            _operator = @operator;
            _selector = selector;
        }

        /// <summary>
        ///     创建ArithAggregateOp实例
        /// </summary>
        /// <param name="operator">算术聚合运算符</param>
        /// <param name="sourceType">源类型</param>
        internal ArithAggregateOp(EAggregationOperator @operator, Type sourceType)
            : base(EQueryOpName.ArithAggregate, sourceType)
        {
            _operator = @operator;
        }

        /// <summary>
        ///     获取算术聚合运算符。
        /// </summary>
        public EAggregationOperator Operator => _operator;

        /// <summary>
        ///     获取聚合结果类型。
        /// </summary>
        public override Type ResultType => _selector?.ReturnType;

        /// <summary>
        ///     获取投影函数，该函数应用于每个元素然后以投影结果参与聚合。不指定投影函数则聚合元素自身。
        /// </summary>
        public LambdaExpression Selector => _selector;
    }
}