/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：筛选运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:15:11
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     筛选运算执行器。
    /// </summary>
    public class WhereExecutor : RopExecutor
    {
        /// <summary>
        ///     根据条件表达式解析出的筛选条件。
        /// </summary>
        private readonly ICriteria _criteria;

        /// <summary>
        ///     条件表达式。
        /// </summary>
        private readonly LambdaExpression _expression;

        /// <summary>
        ///     构造WhereExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="criteria">根据条件表达式解析出的筛选条件。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public WhereExecutor(QueryOp queryOp, ICriteria criteria, OpExecutor<RopContext> next = null) : base(queryOp,
            next)
        {
            _criteria = criteria;
            if (queryOp is FilterOp filterOp) _expression = filterOp.Predicate;

            if (queryOp is AggregateOp aggregateOp) _expression = aggregateOp.Predicate;
        }

        /// <summary>
        ///     构造WhereExecutor的新实例。
        /// </summary>
        /// <param name="expression">条件表达式。</param>
        /// <param name="criteria">根据条件表达式解析出的筛选条件。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public WhereExecutor(LambdaExpression expression, ICriteria criteria, OpExecutor<RopContext> next = null) :
            base(null, next)
        {
            _expression = expression;
            _criteria = criteria;
        }

        /// <summary>
        ///     执行映射
        /// </summary>
        /// <param name="context">关系运算上下文</param>
        public override void Execute(RopContext context)
        {
            //     算法：
            //     if(_expression.Parameters.Count == 2)
            //     AddIndexColumn();
            //     if (resultSql.Top > 0) AcceptResult();
            //     ExpandSource(条件表达式);
            //     SourceAliasRootSetter setter = new SourceAliasRootSetter(aliasRoot);
            //     条件.GuideExpressionVisitor(setter);
            //     resultSql.条件 = resultSql.条件 且 条件;

            if (_expression?.Parameters?.Count == 2) context.AddIndexColumn();
            if (context.ResultSql.TakeNumber > 0) context.AcceptResult();
            if (_expression != null)
                context.ExpandSource(_expression);
            var setter = new SourceAliasRootSetter(context.AliasRoot);
            _criteria.GuideExpressionVisitor(setter);
            //组成新的复杂条件
            context.ResultSql.Criteria = context.ResultSql.Criteria == null
                ? _criteria
                : new ComplexCriteria(context.ResultSql.Criteria, _criteria, ELogicalOperator.And);

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}