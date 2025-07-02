/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型转换运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:53:37
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     类型转换运算执行器。
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    public class CastExecutor<TResult> : OopExecutor
    {
        /// <summary>
        ///     构造CastExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public CastExecutor(CastOp op)
            : base(op)
        {
        }

        /// <summary>
        ///     执行运算
        /// </summary>
        /// <param name="context">运算上下文</param>
        public override void Execute(OopContext context)
        {
            //转换为TResult类型
            context.Result = ((IEnumerable)context.Result).Cast<TResult>();
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}