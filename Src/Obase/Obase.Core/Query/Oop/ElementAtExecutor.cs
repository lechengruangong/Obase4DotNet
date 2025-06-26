/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：索引运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:05:32
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     索引运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型.</typeparam>
    public class ElementAtExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly ElementAtOp _op;

        /// <summary>
        ///     构造ElementAtExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public ElementAtExecutor(ElementAtOp op)
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
            //根据是否返回默认值来调用不同重载
            context.Result = _op.ReturnDefault
                ? ((IEnumerable<TSource>)context.Result).ElementAtOrDefault(_op.Index)
                : ((IEnumerable<TSource>)context.Result).ElementAt(_op.Index);
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}