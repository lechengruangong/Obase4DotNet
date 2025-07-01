/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：提取运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:12:33
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     提取运算执行器。
    /// </summary>
    public class TakeExecutor : RopExecutor
    {
        /// <summary>
        ///     提取的数量。
        /// </summary>
        private readonly int _count;

        /// <summary>
        ///     构造TaskExecutor的新实例。
        /// </summary>
        /// <param name="queryOp"></param>
        /// <param name="count">要提取的数量。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public TakeExecutor(QueryOp queryOp, int count, OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
            _count = count;
        }

        /// <summary>
        ///     执行运算。
        /// </summary>
        /// <param name="context">运算上下文。</param>
        public override void Execute(RopContext context)
        {
            context.ResultSql.TakeNumber = _count;

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}