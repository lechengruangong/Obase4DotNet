/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：日期时间条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:51:00
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     日期时间条件
    /// </summary>
    public class DateTimeCriteria : SimpleCriteria<DateTime>
    {
        /// <summary>
        ///     创建简单日期时间条件实例。
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public DateTimeCriteria(string field, ERelationOperator relationoperator, DateTime value) : base(field,
            relationoperator, value)
        {
        }

        /// <summary>
        ///     创建简单日期时间条件实例。
        /// </summary>
        /// <param name="source">源名称</param>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public DateTimeCriteria(string source, string field, ERelationOperator relationoperator, DateTime value) : base(
            source, field, relationoperator, value)
        {
        }
    }
}