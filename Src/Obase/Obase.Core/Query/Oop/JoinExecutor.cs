/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：联接运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:16:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     联接运算执行器。
    /// </summary>
    /// <typeparam name="TOuter">第一个序列中元素的类型</typeparam>
    /// <typeparam name="TInner">第二个序列中元素的类型</typeparam>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TResult">结果元素的类型</typeparam>
    public class JoinExecutor<TOuter, TInner, TKey, TResult> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly JoinOp _op;

        /// <summary>
        ///     构造JoinExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public JoinExecutor(JoinOp op)
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
            var result = ((IEnumerable<TOuter>)context.Result).ToList();
            var inner = (IEnumerable<TInner>)_op.InnerSource;
            var outerKeySelector = (Func<TOuter, TKey>)ExpressionDelegates.Current[_op.OuterKeySelector];
            var innerKeySelector = (Func<TInner, TKey>)ExpressionDelegates.Current[_op.InnerKeySelector];
            //从缓存中取到表达式后
            if (ExpressionDelegates.Current[_op.ResultSelector] is Func<TOuter, TInner, TResult> joinResultSelector)
            {
                //根据是否有比较器调用不同重载
                if (_op.Comparer != null)
                    context.Result = result.Join(inner, outerKeySelector, innerKeySelector, joinResultSelector,
                        (IEqualityComparer<TKey>)_op.Comparer);
                else
                    context.Result = result.Join(inner, outerKeySelector, innerKeySelector, joinResultSelector);
            }
            else if (ExpressionDelegates.Current[_op.ResultSelector] is Func<TOuter, IEnumerable<TInner>, TResult>
                     groupJoinResultSelector)
            {
                //根据是否有比较器调用不同重载
                if (_op.Comparer != null)
                    context.Result = result.GroupJoin(inner, outerKeySelector, innerKeySelector,
                        groupJoinResultSelector, (IEqualityComparer<TKey>)_op.Comparer);
                else
                    context.Result =
                        result.GroupJoin(inner, outerKeySelector, innerKeySelector, groupJoinResultSelector);
            }

            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}