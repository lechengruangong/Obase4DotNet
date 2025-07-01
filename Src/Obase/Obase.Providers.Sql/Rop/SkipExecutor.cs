/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：略过运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:10:44
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     略过运算执行器。
    /// </summary>
    public class SkipExecutor : RopExecutor
    {
        /// <summary>
        ///     略过的数量。
        /// </summary>
        private readonly int _count;

        /// <summary>
        ///     构造SkipExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="count">要略过的数量。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public SkipExecutor(QueryOp queryOp, int count, OpExecutor<RopContext> next) : base(queryOp, next)
        {
            _count = count;
        }

        /// <summary>
        ///     执行映射
        /// </summary>
        /// <param name="context">对象运算上下文</param>
        public override void Execute(RopContext context)
        {
            switch (context.SourceType)
            {
                case EDataSource.SqlServer:
                {
                    //     算法：
                    //     对于SqlServer
                    //     if (joinMemo.Count > 0)
                    //     {
                    //     resultSql.Distinct = true;
                    //     AcceptResult();
                    //     }
                    //     if(resultSql.Order.Count == 0) resultSql.BubbleOrder();
                    //     FunctionExpression index = Expression.Function("row_number");
                    //     OverClause over = new OverClause(resultSql.Orders);
                    //     index.Over = over;
                    //     string alias = model[resultType].Name + “_rownumber”;
                    //     resultSql.投影集.Add(index, alias);
                    //     AcceptResult();
                    //     resultSql.条件 = { alias > 略过数量 };
                    //     对于Mysql
                    //     context.ResultSql.SkipNumber = _count;
                    if (context.ResultSql.Distinct)
                        context.AcceptResult();

                    if (context.ResultSql.Orders.Count == 0)
                    {
                        if (!context.ResultSql.Source.CanBubbleOrder)
                            throw new ArgumentException($"{context.ResultSql.Source.GetType().Name}执行Skip操作前要先执行排序操作.");

                        context.ResultSql.BubbleOrder();
                    }

                    //参照算法类注释
                    var index = Expression.Function("row_number");
                    var over = new OverClause(context.ResultSql.Orders.ToArray());
                    index.Over = over;
                    var alias = context.ResultModelType.Name + "_rownumber";
                    if (context.ResultSql.SelectionSet == null) context.ResultSql.SelectionSet = new SelectionSet();

                    context.ResultSql.SelectionSet.Add(index, alias);
                    context.AcceptResult();
                    context.ResultSql.Criteria = new NumericCriteria<int>(alias, ERelationOperator.GreaterThan, _count);
                    break;
                }
                case EDataSource.Oracle:
                case EDataSource.MySql:
                case EDataSource.PostgreSql:
                case EDataSource.Sqlite:
                {
                    context.ResultSql.SkipNumber = _count;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(context.SourceType), $"不支持的查询源{context.SourceType}");
            }

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}