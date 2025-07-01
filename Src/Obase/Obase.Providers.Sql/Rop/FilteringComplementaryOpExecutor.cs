/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：选择类运算的补充运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:26:05
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Query;
using Obase.Core.Query.Oop;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     负责以对象运算方式执行选择类运算（FirstOp, LastOp）的补充运算。
    /// </summary>
    public class FilteringComplementaryOpExecutor : OopExecutor
    {
        /// <summary>
        ///     被执行的运算。
        /// </summary>
        private readonly FilteringComplementaryOp _executedOp;

        /// <summary>
        ///     初始化FilteringComplementaryOpExecutor的新实例。
        /// </summary>
        /// <param name="executedOp">被执行的选择补充运算。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public FilteringComplementaryOpExecutor(FilteringComplementaryOp executedOp, OopExecutor next = null)
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
            var source = context.Source.GetEnumerator();
            switch (_executedOp.Name)
            {
                case EQueryOpName.First:
                    var firstOp = (FirstOp)_executedOp.ComplementedOp;
                    if (!source.MoveNext() && !firstOp.ReturnDefault)
                        throw new InvalidOperationException("序列不包含任何元素");
                    context.Result = source.Current;
                    while (source.MoveNext())
                    {
                        //取干净 防止连接未关闭
                    }

                    break;
                case EQueryOpName.Last:
                    var lastOp = (LastOp)_executedOp.ComplementedOp;
                    if (!source.MoveNext() && !lastOp.ReturnDefault)
                        throw new InvalidOperationException("序列不包含任何元素");
                    context.Result = source.Current;
                    while (source.MoveNext()) //用下一个元素替换
                        context.Result = source.Current;
                    break;
            }

            if (source is IDisposable disposable) disposable.Dispose();

            (_next as OopExecutor)?.Execute(context);
        }
    }
}