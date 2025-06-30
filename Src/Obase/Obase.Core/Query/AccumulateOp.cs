/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Accumulate运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:04:43
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Accumulate运算。
    /// </summary>
    public class AccumulateOp : AggregateOp
    {
        /// <summary>
        ///     累加函数。
        /// </summary>
        private readonly LambdaExpression _accumulator;

        /// <summary>
        ///     结果函数，用于将累加器的最终值转换为结果值。
        /// </summary>
        private readonly LambdaExpression _resultSelector;

        /// <summary>
        ///     种子值。
        /// </summary>
        private readonly object _seed;

        /// <summary>
        ///     创建AccumulateOp实例。
        ///     实施说明
        ///     累加器表达式的第二个形参的类型为查询源类型。
        /// </summary>
        /// <param name="accumulator">累加函数。</param>
        /// <param name="model">对象数据模型</param>
        internal AccumulateOp(LambdaExpression accumulator, ObjectDataModel model)
            : base(EQueryOpName.Accumulate, accumulator, model, accumulator.Parameters[1].Type)
        {
            _accumulator = accumulator;
        }

        /// <summary>
        ///     创建AccumulateOp实例。
        /// </summary>
        /// <param name="accumulator">累加函数。</param>
        /// <param name="seed">种子值。</param>
        /// <param name="model">对象数据模型</param>
        /// 实施说明:
        /// 累加器表达式的第二个形参的类型为查询源类型。
        internal AccumulateOp(LambdaExpression accumulator, object seed, ObjectDataModel model)
            : this(accumulator, model)
        {
            _accumulator = accumulator;
            _seed = seed;
        }

        /// <summary>
        ///     创建AccumulateOp实例。
        /// </summary>
        /// <param name="accumulator">累加函数。</param>
        /// <param name="resultSelector">结果函数。</param>
        /// <param name="model">对象数据模型</param>
        /// 实施说明
        /// 累加器表达式的第二个形参的类型为查询源类型。
        internal AccumulateOp(LambdaExpression accumulator, LambdaExpression resultSelector, ObjectDataModel model)
            : this(accumulator, model)
        {
            _resultSelector = resultSelector;
        }

        /// <summary>
        ///     创建AccumulateOp实例。
        /// </summary>
        /// <param name="accumulator">累加函数。</param>
        /// <param name="seed">种子值。</param>
        /// <param name="resultSelector">结果函数。</param>
        /// <param name="model">对象数据模型</param>
        /// 实施说明:
        /// 累加器表达式的第二个形参的类型为查询源类型。
        internal AccumulateOp(LambdaExpression accumulator, object seed, LambdaExpression resultSelector,
            ObjectDataModel model)
            : this(accumulator, seed, model)
        {
            _resultSelector = resultSelector;
        }

        /// <summary>
        ///     获取累加函数。
        /// </summary>
        public LambdaExpression Accumulator => _accumulator;

        /// <summary>
        ///     获取结果函数，该函数用于将累加器的最终值转换为结果值。
        /// </summary>
        public LambdaExpression ResultSelector => _resultSelector;

        /// <summary>
        ///     获取结果值类型。
        /// </summary>
        public override Type ResultType => _resultSelector?.ReturnType;

        /// <summary>
        ///     获取种子值。
        /// </summary>
        public object Seed => _seed;

        /// <summary>
        ///     获取种子值类型。
        /// </summary>
        public Type SeedType => _seed?.GetType();
    }
}