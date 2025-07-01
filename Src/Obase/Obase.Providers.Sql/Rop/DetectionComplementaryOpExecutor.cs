/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：执行测定类运算的补充运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:23:16
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Query;
using Obase.Core.Query.Oop;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     负责以对象运算方式执行测定类运算（AllOp, AnyOp, ContainsOp, SingleOp）的补充运算。
    ///     算法
    ///     对于All测定，结果值为0则测定成功，结果值大于0则测定失败；
    ///     对于其它测定，结果值为0则测定失败，结果值大于0则测定成功。
    /// </summary>
    public class DetectionComplementaryOpExecutor : OopExecutor
    {
        /// <summary>
        ///     被执行的运算。
        /// </summary>
        private readonly DetectionComplementaryOp _executedOp;

        /// <summary>
        ///     初始化DetectionComplementaryOpExecutor的新实例。
        /// </summary>
        /// <param name="executedOp">被执行的测定补充运算。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public DetectionComplementaryOpExecutor(DetectionComplementaryOp executedOp, OopExecutor next = null)
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
            switch (_executedOp.Name)
            {
                case EQueryOpName.All:
                {
                    var count = Convert.ToInt32(context.Result);
                    context.Result = count <= 0;
                }

                    break;
                case EQueryOpName.Any:
                case EQueryOpName.Contains:
                {
                    var count = Convert.ToInt32(context.Result);
                    context.Result = count > 0;
                }
                    break;
                case EQueryOpName.Single:
                {
                    var enu = context.Source.GetEnumerator();
                    enu.MoveNext();
                    var result = enu.Current;
                    if (enu.MoveNext())
                    {
                        while (enu.MoveNext())
                        {
                            //取干净 防止连接未关闭
                        }

                        throw new InvalidOperationException("Sequence contains more than one matching element");
                    }

                    if (enu is IDisposable disposable) disposable.Dispose();

                    context.Result = result;
                }

                    break;
                default:
                    throw new AggregateException(
                        $"{_executedOp.Name}不适用于负责以对象运算方式执行测定类运算（AllOp, AnyOp, ContainsOp, SingleOp）的补充运算");
            }

            (_next as OopExecutor)?.Execute(context);
        }
    }
}