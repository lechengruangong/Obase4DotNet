/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：联接分组压缩运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:41:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     联接分组压缩运算执行器。
    /// </summary>
    /// <typeparam name="TFirst">第一个序列中元素的类型</typeparam>
    /// <typeparam name="TSecond">第二个序列中元素的类型</typeparam>
    /// <typeparam name="TResult">结果序列的元素的类型</typeparam>
    public class ZipExecutor<TFirst, TSecond, TResult> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly ZipOp _op;

        /// <summary>
        ///     构造ZipExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public ZipExecutor(ZipOp op)
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
            if (_op.ResultSelector != null)
            {
                var first = ((IEnumerable<TFirst>)context.Result).ToList();
                context.Result = first.Zip((IEnumerable<TSecond>)_op.Second,
                    (Func<TFirst, TSecond, TResult>)ExpressionDelegates.Current[_op.ResultSelector]);
            }
            else
            {
                //此处无法直接调用扩展方法 改为自行实现
                var first = ((IEnumerable<TFirst>)context.Result).ToList();
                var second = (IEnumerable<TSecond>)_op.Second;
                var result = new List<(TFirst, TSecond)>();
                //根据条件连接 并且构造(TFirst, TSecond)类型的匿名对象
                using (var e1 = first.GetEnumerator())
                using (var e2 = second.GetEnumerator())
                {
                    while (e1.MoveNext() && e2.MoveNext()) result.Add((e1.Current, e2.Current));
                }

                //设置结果
                context.Result = result;
            }

            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}