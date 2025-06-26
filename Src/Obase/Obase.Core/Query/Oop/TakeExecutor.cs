/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：提取运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:39:13
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     提取运算执行器。
    ///     类型参数：
    ///     TSource		源对象的类型
    ///     实施建议：
    ///     首先从上下文获取查询源，强制转换为IEnumerable{TSource}，然后调用相应的扩展方法（注意分情况选择不同的重载），最后将结果存储于上下文。
    /// </summary>
    public class TakeExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly TakeOp _op;

        /// <summary>
        ///     构造TakeExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public TakeExecutor(TakeOp op)
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
            //提取
            context.Result = ((IEnumerable<TSource>)context.Result).Take(_op.Count);
            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}