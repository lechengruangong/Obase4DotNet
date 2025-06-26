/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：包含运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:12:32
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     包含运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    /// <typeparam name="TResult">目标引用的类型。</typeparam>
    public class IncludeExecutor<TSource, TResult> : OopExecutor
    {
        /// <summary>
        ///     构造IncludeExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的包含运算。</param>
        public IncludeExecutor(IncludeOp op)
            : base(op)
        {
        }


        /// <summary>
        ///     执行运算。
        /// </summary>
        /// <param name="context">运算上下文。</param>
        public override void Execute(OopContext context)
        {
            //包含运算实际上不需要做任何运算
            if (context.Result is IEnumerable<TSource> source) context.Result = source;
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}