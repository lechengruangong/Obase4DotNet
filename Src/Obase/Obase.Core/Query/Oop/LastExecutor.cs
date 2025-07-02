/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Last索引运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:18:21
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     Last索引运算执行器。
    ///     类型参数：
    ///     TSource		源对象的类型
    ///     实施建议：
    ///     首先从上下文获取查询源，强制转换为IEnumerable{TSource}，然后调用相应的扩展方法（注意分情况选择不同的重载），最后将结果存储于上下文。
    /// </summary>
    public class LastExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly LastOp _op;


        /// <summary>
        ///     构造LastExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public LastExecutor(LastOp op)
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
            //条件部分已转移至管道构建时处理
            var result = (IEnumerable<TSource>)context.Result;
            context.Result = _op.ReturnDefault ? result.LastOrDefault() : result.Last();
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}