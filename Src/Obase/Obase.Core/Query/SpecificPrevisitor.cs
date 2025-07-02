/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定查询运算的前置访问逻辑.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:40:56
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     代表特定前置访问逻辑的委托。
    /// </summary>
    /// <returns>如果要继续访问后续运算返回true，否则返回false。</returns>
    /// <param name="queryOp">要访问的查询运算。</param>
    /// <param name="previousState">访问前一运算时产生的状态数据。</param>
    /// <param name="outPreviousState">返回一个状态数据，在遍历到后一运算时该数据将被视为前序状态。</param>
    /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
    public delegate bool Previsit(QueryOp queryOp, object previousState, out object outPreviousState,
        out object outPrevisitState);

    /// <summary>
    ///     特定查询运算的前置访问逻辑。
    /// </summary>
    internal class SpecificPrevisitor
    {
        /// <summary>
        ///     特定查询运算的名称。
        /// </summary>
        private EQueryOpName _name;

        /// <summary>
        ///     代表前置访问逻辑的委托。
        /// </summary>
        private Previsit _previsit;

        /// <summary>
        ///     获取特定查询运算的名称。
        /// </summary>
        public EQueryOpName Name
        {
            get => _name;
            internal set => _name = value;
        }

        /// <summary>
        ///     获取代表前置访问逻辑的委托。
        ///     实施说明
        ///     附加可访问性为internal的set访问器。
        /// </summary>
        public Previsit Previsit
        {
            get => _previsit;
            internal set => _previsit = value;
        }

        /// <summary>
        ///     获取确定是否启用特定访问逻辑的断言函数，函数返回true时启用，否则不启用。未指定断言函数时始终启用。
        ///     实施说明
        ///     附加可访问性为internal的set访问器。
        /// </summary>
        public Func<QueryOp, ESpecialPredicate> Predicate { get; internal set; }
    }
}