/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：去重运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:23:52
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     去重运算执行器。
    ///     算法：
    ///     if(resultSql.Top > 0) AcceptResult();
    ///     resultSql.Distinct = true;
    /// </summary>
    public class DistinctExecutor : RopExecutor
    {
        /// <summary>
        ///     构造DistinctExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public DistinctExecutor(QueryOp queryOp, OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
        }

        /// <summary>
        ///     执行运算
        /// </summary>
        /// <param name="context">关系运算上下文</param>
        public override void Execute(RopContext context)
        {
            if (context.ResultSql.TakeNumber > 0)
                context.AcceptResult();
            context.ResultSql.Distinct = true;

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}