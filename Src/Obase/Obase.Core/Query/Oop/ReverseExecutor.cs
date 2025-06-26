/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：反序运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:26:58
└──────────────────────────────────────────────────────────────┘
*/


using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     反序运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    public class ReverseExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     构造ReverseExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public ReverseExecutor(ReverseOp op)
            : base(op)
        {
        }

        /// <summary>
        ///     执行运算
        /// </summary>
        /// <param name="context">运算上下文</param>
        public override void Execute(OopContext context)
        {
            //反序处理
            context.Result = ((IEnumerable<TSource>)context.Result).Reverse();
            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}