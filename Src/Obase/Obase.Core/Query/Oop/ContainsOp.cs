/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Contains测定运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:56:50
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     Contains测定运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    public class ContainsExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly ContainsOp _op;

        /// <summary>
        ///     构造ContainsExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public ContainsExecutor(ContainsOp op)
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
            if (context.Result is IEnumerable<TSource> source)
            {
                var value = (TSource)_op.Item;
                IEqualityComparer<TSource> comparer = null;
                if (_op.Comparer is IEqualityComparer<TSource> cm)
                    comparer = cm;
                //根据是否有比较器调用重载
                context.Result = comparer != null ? source.Contains(value, comparer) : source.Contains(value);
            }
            else
            {
                context.Result = Convert.ToInt32(context.Result) > 0;
            }

            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}