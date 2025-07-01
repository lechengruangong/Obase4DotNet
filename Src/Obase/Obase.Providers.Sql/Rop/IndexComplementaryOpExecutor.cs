/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：索引运算的补充运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:45:14
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Query;
using Obase.Core.Query.Oop;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     负责以对象运算方式执行索引运算（ElementAtOp）的补充运算。
    /// </summary>
    /// 算法：
    /// 从结果序列中取出第一个元素，返回该元素；如果结果序列为空，返回默认值或null。
    public class IndexComplementaryOpExecutor : OopExecutor
    {
        /// <summary>
        ///     被执行的运算。
        /// </summary>
        private readonly IndexComplementaryOp _executedOp;

        /// <summary>
        ///     初始化IndexComplementaryOpExecutor的新实例。
        /// </summary>
        /// <param name="executedOp">被执行的索引补充运算。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public IndexComplementaryOpExecutor(IndexComplementaryOp executedOp, OopExecutor next)
            : base(executedOp, next)
        {
            _executedOp = executedOp;
        }

        /// <summary>
        ///     执行操作
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(OopContext context)
        {
            var tor = context.Source.GetEnumerator();
            tor.MoveNext();
            if (_executedOp.ComplementedOp is ElementAtOp elementAtOp)
                if (!elementAtOp.ReturnDefault)
                    if (tor.Current == null)
                        throw new InvalidOperationException("序列不包含任何元素");
            context.Result = tor.Current;
            while (tor.MoveNext())
            {
                //取干净 防止连接未关闭
            }

            if (tor is IDisposable disposable) disposable.Dispose();

            (_next as OopExecutor)?.Execute(context);
        }
    }
}