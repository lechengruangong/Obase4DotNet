/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：分组聚合运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:26:45
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;
using Expression = Obase.Providers.Sql.SqlObject.Expression;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     分组聚合运算执行器。
    /// </summary>
    public class GroupAggregationExecutor : RopExecutor
    {
        /// <summary>
        ///     作为分组依据的表达式。
        /// </summary>
        private readonly LambdaExpression _expression;

        /// <summary>
        ///     根据分组依据表达式翻译出的分组依据（Sql表达式）。
        /// </summary>
        private readonly Expression _groupBy;

        /// <summary>
        ///     构造GroupAggregationExecutor实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="expression">作为分组依据的表达式。</param>
        /// <param name="groupBy">根据分组依据表达式解析出的分组依据（SQL表达式）。</param>
        /// <param name="next">下一节</param>
        public GroupAggregationExecutor(QueryOp queryOp, LambdaExpression expression, Expression groupBy,
            OpExecutor<RopContext> next) :
            base(queryOp, next)
        {
            _groupBy = groupBy;
            _expression = expression;
        }

        /// <summary>
        ///     执行运算。
        /// </summary>
        /// <param name="context">管道上下文</param>
        public override void Execute(RopContext context)
        {
            //     算法：
            //     if (resultSql.Top > 0) AcceptResult();
            //     ExpandSource(依据表达式);
            //     SourceAliasRootSetter setter = new SourceAliasRootSetter(aliasRoot);
            //     依据表达式.Accept(setter)
            //     GroupBy group = new Group(分组依据);
            //     resultSql.GroupBy = group;

            if (context.ResultSql.TakeNumber > 0) context.AcceptResult();
            context.ExpandSource(_expression);
            var setter = new SourceAliasRootSetter(context.AliasRoot);
            _groupBy.Accept(setter);
            var group = new GroupBy(_groupBy);
            context.ResultSql.Orders = new List<Order>();
            context.ResultSql.GroupBy = group;
            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}