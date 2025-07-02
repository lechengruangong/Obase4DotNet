/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：反序运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:53:05
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     反序运算执行器。
    ///     算法：
    ///     if(resultSql.Top > 0) AcceptResult();
    ///     resultSql.Reverse();
    /// </summary>
    public class ReverseExecutor : RopExecutor
    {
        /// <summary>
        ///     构造ReverseExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public ReverseExecutor(QueryOp queryOp, OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
        }

        /// <summary>
        ///     执行反序运算
        /// </summary>
        /// <param name="context">关系运算上下文</param>
        public override void Execute(RopContext context)
        {
            if (context.ResultSql.TakeNumber > 0) context.AcceptResult();
            //设置反序
            context.ResultSql.Reverse();

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}