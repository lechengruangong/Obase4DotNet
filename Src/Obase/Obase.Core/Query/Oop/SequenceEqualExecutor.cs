/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列相等测定运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:32:29
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     序列相等测定运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    public class SequenceEqualExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly SequenceEqualOp _op;

        /// <summary>
        ///     构造SequenceEqualExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public SequenceEqualExecutor(SequenceEqualOp op)
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
            var result = ((IEnumerable<TSource>)context.Result).ToList();
            var second = (IEnumerable<TSource>)_op.Other;
            IEqualityComparer<TSource> comparer = null;
            if (_op.Comparer is IEqualityComparer<TSource> cm)
                comparer = cm;
            //根据是否有比较器调用不同重载
            context.Result = comparer != null ? result.SequenceEqual(second, comparer) : result.SequenceEqual(second);
            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}