/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关系运算执行器基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:56:06
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     为关系运算执行器提供基础实现。
    /// </summary>
    public abstract class RopExecutor : OpExecutor<RopContext>
    {
        /// <summary>
        ///     构造OpExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        protected RopExecutor(QueryOp queryOp, OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
        }
    }
}