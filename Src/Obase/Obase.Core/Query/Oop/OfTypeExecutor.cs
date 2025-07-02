/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型筛选运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:20:29
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     类型筛选运算执行器。
    /// </summary>
    /// <typeparam name="TResult">源对象的类型</typeparam>
    public class OfTypeExecutor<TResult> : OopExecutor
    {
        /// <summary>
        ///     构造OfTypeExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public OfTypeExecutor(OfTypeOp op)
            : base(op)
        {
        }

        /// <summary>
        ///     执行运算
        /// </summary>
        /// <param name="context">运算上下文</param>
        public override void Execute(OopContext context)
        {
            context.Result = ((IEnumerable)context.Result).OfType<TResult>();
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}