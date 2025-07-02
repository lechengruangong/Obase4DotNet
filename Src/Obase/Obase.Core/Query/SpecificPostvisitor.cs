/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定查询运算的后置访问逻辑.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:42:18
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     代表特定后置访问逻辑的委托。
    /// </summary>
    /// <param name="queryOp">要访问的查询运算。</param>
    /// <param name="previousState">访问上一运算时产生的状态数据。</param>
    /// <param name="previsitState">前置访问产生的状态数据。</param>
    public delegate void Postvisit(QueryOp queryOp, object previousState, object previsitState);

    /// <summary>
    ///     特定查询运算的后置访问逻辑。
    /// </summary>
    internal class SpecificPostvisitor
    {
        /// <summary>
        ///     特定查询运算的名称。
        /// </summary>
        private EQueryOpName _name;

        /// <summary>
        ///     代表后置访问逻辑的委托。
        /// </summary>
        private Postvisit _postvisit;

        /// <summary>
        ///     获取特定查询运算的名称。
        ///     实施说明
        ///     附加可访问性为internal的set访问器。
        /// </summary>
        public EQueryOpName Name
        {
            get => _name;
            internal set => _name = value;
        }

        /// <summary>
        ///     获取代表后置访问逻辑的委托。
        ///     实施说明
        ///     附加可访问性为internal的set访问器。
        /// </summary>
        public Postvisit Postvisit
        {
            get => _postvisit;
            internal set => _postvisit = value;
        }

        /// <summary>
        ///     获取确定是否启用特定访问逻辑的断言函数，函数返回true时启用，否则不启用。未指定断言函数时始终启用。
        ///     实施说明
        ///     附加可访问性为internal的set访问器。
        /// </summary>
        public Func<QueryOp, ESpecialPredicate> Predicate { get; internal set; }
    }
}