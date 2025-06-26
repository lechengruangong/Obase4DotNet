/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：去重运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:04:10
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     去重运算执行器。
    /// </summary>
    public class DistinctExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly DistinctOp _op;

        /// <summary>
        ///     构造DistinctExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public DistinctExecutor(DistinctOp op)
            : base(op)
        {
            _op = op;
        }

        /// <summary>
        ///     执行运算
        /// </summary>
        /// <param name="context">运算上下文</param>
        public override void Execute(OopContext context)
        {
            IEqualityComparer<TSource> comparer = null;
            if (_op.Comparer is IEqualityComparer<TSource> cm)
                comparer = cm;
            //根据是否有比较器调用不同的重载
            context.Result = comparer != null
                ? ((IEnumerable<TSource>)context.Result).Distinct(comparer)
                : ((IEnumerable<TSource>)context.Result).Distinct();
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}