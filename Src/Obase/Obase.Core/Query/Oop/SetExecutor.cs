/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：集运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:33:32
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     集运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    public class SetExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly SetOp _op;

        /// <summary>
        ///     构造UnionExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public SetExecutor(SetOp op)
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
            var result = ((IEnumerable<TSource>)context.Result).ToList();
            var second = (IEnumerable<TSource>)_op.Other;
            //处理比较器
            IEqualityComparer<TSource> comparer = null;
            if (_op.Comparer is IEqualityComparer<TSource> cm)
                comparer = cm;
            switch (_op.Operator)
            {
                // 求并集（不去重）。
                case ESetOperator.Concat:
                    context.Result = result.Concat(second);
                    break;
                // 求交集。
                case ESetOperator.Interact:
                    context.Result = comparer != null ? result.Intersect(second, comparer) : result.Intersect(second);
                    break;
                // 求差集。
                case ESetOperator.Except:
                    context.Result = comparer != null ? result.Except(second, comparer) : result.Except(second);
                    break;
                //  求并集（去重）。
                case ESetOperator.Union:
                    context.Result = comparer != null ? result.Union(second, comparer) : result.Union(second);
                    break;
            }

            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}