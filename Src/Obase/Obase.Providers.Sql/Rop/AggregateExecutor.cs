/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：聚合运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:55:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     聚合运算执行器。
    ///     算法：
    ///     resultSql.Aggregation = 聚合名称;
    ///     SetResultType(聚合结果类型);
    /// </summary>
    public class AggregateExecutor : RopExecutor
    {
        /// <summary>
        ///     聚合类型。
        /// </summary>
        private readonly EAggregationFunction _aggregationType;

        /// <summary>
        ///     聚合结果的类型。
        /// </summary>
        private readonly Type _resultType;

        /// <summary>
        ///     构造AggregateExecutor的新实例。
        /// </summary>
        /// <param name="queryOp"></param>
        /// <param name="aggregationType">聚合类型。</param>
        /// <param name="resultType">聚合结果的类型。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public AggregateExecutor(QueryOp queryOp, EAggregationFunction aggregationType, Type resultType,
            OpExecutor<RopContext> next = null)
            : base(queryOp, next)
        {
            _aggregationType = aggregationType;
            _resultType = resultType;
        }

        /// <summary>
        ///     聚合类型。
        /// </summary>
        public EAggregationFunction AggregationType => _aggregationType;

        /// <summary>
        ///     获取聚合结果的类型。
        /// </summary>
        public Type ResultType => _resultType;

        /// <summary>
        ///     执行运算。
        /// </summary>
        /// <param name="context">运算上下文。</param>
        public override void Execute(RopContext context)
        {
            context.ResultSql.Aggregation = _aggregationType;
            //设置结果类型为聚合后的类型
            context.SetResultType(PrimitiveType.FromType(_resultType), false, _next is RopTerminator);

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}