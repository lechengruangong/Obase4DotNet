/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字符条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:40:50
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     字符条件
    /// </summary>
    public class CharCriteria : SimpleCriteria<char>
    {
        /// <summary>
        ///     创建简单字符条件实例。
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public CharCriteria(string field, ERelationOperator relationoperator, char value) : base(field,
            relationoperator, value)
        {
        }

        /// <summary>
        ///     创建简单字符条件实例。
        /// </summary>
        /// <param name="source">源名称</param>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public CharCriteria(string source, string field, ERelationOperator relationoperator, char value) : base(source,
            field, relationoperator, value)
        {
        }
    }
}