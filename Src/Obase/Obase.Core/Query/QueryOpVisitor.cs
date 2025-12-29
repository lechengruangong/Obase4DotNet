/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询链访问者.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:33:26
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Query
{
    /// <summary>
    ///     定义在遍历查询链过程中访问查询运算的规范，并提供基础实现。
    /// </summary>
    public abstract class QueryOpVisitor
    {
        /// <summary>
        ///     特定查询运算的后置访问逻辑。
        /// </summary>
        private Dictionary<EQueryOpName, SpecificPostVisitor> _specificPostvisitors;

        /// <summary>
        ///     特定查询运算的前置访问逻辑。
        /// </summary>
        private Dictionary<EQueryOpName, SpecificPreVisitor> _specificPrevisitors;

        /// <summary>
        ///     后置访问，即在访问后续运算后执行操作。
        /// </summary>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(QueryOp queryOp, object previousState, object previsitState)
        {
            //获取缓存的后置访问
            var specific = _specificPostvisitors != null &&
                           _specificPostvisitors.TryGetValue(queryOp.Name, out var specificPostvisitor)
                ? specificPostvisitor
                : null;
            //如果没有特定的后置访问逻辑，则执行通用后置访问逻辑
            if (specific?.Predicate == null)
            {
                PostvisitGenerally(queryOp, previousState, previsitState);
            }
            else
            {
                //如果有特定的后置访问逻辑，则根据断言函数的结果决定是否执行特定逻辑
                var predicateRe = specific.Predicate(queryOp);
                if (predicateRe == ESpecialPredicate.PreExecute)
                    specific.Postvisit(queryOp, previousState, previsitState);
                if (predicateRe != ESpecialPredicate.Substitute)
                    PostvisitGenerally(queryOp, previousState, previsitState);
                if (predicateRe == ESpecialPredicate.PostExecute)
                    specific.Postvisit(queryOp, previousState, previsitState);
            }
        }

        /// <summary>
        ///     执行通用后置访问逻辑。
        /// </summary>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        protected abstract bool PostvisitGenerally(QueryOp queryOp, object previousState, object previsitState);

        /// <summary>
        ///     前置访问，即在访问后续运算前执行操作。
        /// </summary>
        /// <returns>如果要继续访问后续运算，返回true；否则返回false。</returns>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="outPreviousState">返回一个状态数据，在访问下一运算时该数据将被视为前序状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(QueryOp queryOp, object previousState, out object outPreviousState,
            out object outPrevisitState)
        {
            var result = false;
            outPreviousState = outPrevisitState = null;
            //获取缓存的前置访问
            var specific = _specificPrevisitors != null &&
                           _specificPrevisitors.TryGetValue(queryOp.Name, out var specificPrevisitor)
                ? specificPrevisitor
                : null;
            //如果没有特定的前置访问逻辑，则执行通用前置访问逻辑
            if (specific?.Predicate == null)
            {
                result = PrevisitGenerally(queryOp, previousState, out outPreviousState, out outPrevisitState);
            }
            else
            {
                //如果有特定的前置访问逻辑，则根据断言函数的结果决定是否执行特定逻辑
                var predicateRe = specific.Predicate(queryOp);
                if (predicateRe == ESpecialPredicate.PreExecute)
                    result |= specific.Previsit(queryOp, previousState, out outPreviousState, out outPrevisitState);
                if (predicateRe != ESpecialPredicate.Substitute)
                    result |= PrevisitGenerally(queryOp, previousState, out outPreviousState, out outPrevisitState);
                if (predicateRe == ESpecialPredicate.PostExecute || predicateRe == ESpecialPredicate.Substitute)
                    result |= specific.Previsit(queryOp, previousState, out outPreviousState, out outPrevisitState);
            }

            return result;
        }

        /// <summary>
        ///     执行通用前置访问逻辑。
        /// </summary>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="outPreviousState">返回一个状态数据，在遍历到下一运算时该数据将被视为前序状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        protected abstract bool PrevisitGenerally(QueryOp queryOp, object previousState, out object outPreviousState,
            out object outPrevisitState);

        /// <summary>
        ///     为指定的查询运算设置特定的前置访问逻辑。
        /// </summary>
        /// <param name="name">运算的名称。</param>
        /// <param name="previsit">代表前置访问逻辑的委托。</param>
        /// <param name="predicate">断言是否启用特定访问逻辑的函数。</param>
        protected void Specify(EQueryOpName name, Previsit previsit, Func<QueryOp, ESpecialPredicate> predicate = null)
        {
            //注册特定前置访问逻辑
            if (_specificPrevisitors == null)
                _specificPrevisitors = new Dictionary<EQueryOpName, SpecificPreVisitor>();
            _specificPrevisitors[name] = new SpecificPreVisitor
                { Name = name, Previsit = previsit, Predicate = predicate };
        }

        /// <summary>
        ///     为指定的查询运算设置特定的后置访问逻辑。
        /// </summary>
        /// <param name="name">运算名称。</param>
        /// <param name="postvisit">代表后置访问逻辑的委托。</param>
        /// <param name="predicate">断言是否启用特定访问逻辑的函数。</param>
        protected void Specify(EQueryOpName name, Postvisit postvisit,
            Func<QueryOp, ESpecialPredicate> predicate = null)
        {
            //注册特定后置访问逻辑
            if (_specificPostvisitors == null)
                _specificPostvisitors = new Dictionary<EQueryOpName, SpecificPostVisitor>();
            _specificPostvisitors[name] = new SpecificPostVisitor
                { Name = name, Postvisit = postvisit, Predicate = predicate };
        }
    }


    /// <summary>
    ///     定义在遍历查询链过程中访问查询运算的规范，并提供基础实现。
    /// </summary>
    /// <typeparam name="TResult">访问操作返回值类型</typeparam>
    public abstract class QueryOpVisitor<TResult> : QueryOpVisitor
    {
        /// <summary>
        ///     访问操作的结果。
        /// </summary>
        protected TResult _result;

        /// <summary>
        ///     获取访问操作的结果。
        /// </summary>
        public virtual TResult Result => _result;
    }


    /// <summary>
    ///     定义在遍历查询链过程中访问查询运算的规范，并提供基础实现。
    /// </summary>
    /// <typeparam name="TResult">访问操作返回值类型。</typeparam>
    /// <typeparam name="TOut">访问操作输出参数的类型。</typeparam>
    public abstract class QueryOpVisitorWithOutArgs<TResult, TOut> : QueryOpVisitor<TResult>
    {
        /// <summary>
        ///     访问操作的输出参数值。
        /// </summary>
        protected internal TOut _outArgument;

        /// <summary>
        ///     获取访问操作的输出参数值。
        /// </summary>
        public TOut OutArgument => _outArgument;
    }

    /// <summary>
    ///     定义在遍历查询链过程中访问查询运算的规范，并提供基础实现。
    ///     类型参数
    ///     TArg
    ///     访问操作参数的类型。
    ///     TResult
    ///     访问操作返回值类型。
    /// </summary>
    public abstract class QueryOpVisitorWithArgs<TArg, TResult> : QueryOpVisitor<TResult>
    {
        /// <summary>
        ///     获取或设置访问操作参数。
        /// </summary>
        internal abstract TArg Argument { get; set; }
    }
}