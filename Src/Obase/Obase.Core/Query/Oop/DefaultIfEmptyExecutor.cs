/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：取默认值运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:01:10
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     取默认值运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型</typeparam>
    public class DefaultIfEmptyExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly DefaultIfEmptyOp _op;

        /// <summary>
        ///     构造DefaultIfEmptyExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public DefaultIfEmptyExecutor(DefaultIfEmptyOp op)
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
            //根据是否有默认值调用不同重载
            context.Result = _op.DefaultValue != null
                ? ((IEnumerable<TSource>)context.Result).DefaultIfEmpty((TSource)_op.DefaultValue)
                : ((IEnumerable<TSource>)context.Result).DefaultIfEmpty();
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}