/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为IN条件提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:17:50
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     为IN条件提供基础实现。
    ///     IN条件是指运算符为IN或NOT IN的条件。
    /// </summary>
    public abstract class InCriteria<TItem> : SimpleCriteria<IEnumerable<TItem>>
    {
        /// <summary>
        ///     关系运算符
        /// </summary>
        private ERelationOperator _operator;

        /// <summary>
        ///     构造IN条件
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="relationoperator">操作符</param>
        /// <param name="value">值集合</param>
        protected InCriteria(string field, ERelationOperator relationoperator, IEnumerable<TItem> value) : base(field,
            relationoperator, value)
        {
        }

        /// <summary>
        ///     IN条件
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="field">字段</param>
        /// <param name="relationoperator">操作符</param>
        /// <param name="value">值集合</param>
        protected InCriteria(string source, string field, ERelationOperator relationoperator,
            IEnumerable<TItem> value) :
            base(source, field, relationoperator, value)
        {
        }

        /// <summary>
        ///     获取或设置关系运算符。
        ///     只允许设置IN或NOT IN两种运算符，如果设置其它运行符将引发异常。
        /// </summary>
        public override ERelationOperator Operator
        {
            get => _operator;
            set
            {
                if (value != ERelationOperator.In && value != ERelationOperator.NotIn)
                    throw new ArgumentOutOfRangeException(nameof(value), $"不支持的运算类型{value}");
                _operator = value;
            }
        }
    }
}