/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：数值条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:25:41
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     数值条件
    /// </summary>
    public class NumericCriteria<TNumeric> : SimpleCriteria<TNumeric>
    {
        /// <summary>
        ///     创建简单数值条件实例。
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public NumericCriteria(string field, ERelationOperator relationoperator, TNumeric value) : base(field,
            relationoperator, value)
        {
        }

        /// <summary>
        ///     创建简单数值条件实例。
        /// </summary>
        /// <param name="source">源名称</param>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public NumericCriteria(string source, string field, ERelationOperator relationoperator, TNumeric value) : base(
            source, field, relationoperator, value)
        {
        }
    }
}