/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：布尔条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:57:18
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     布尔条件
    /// </summary>
    public class BoolCriteria : SimpleCriteria<bool>
    {
        /// <summary>
        ///     创建简单布尔条件实例。
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public BoolCriteria(string field, ERelationOperator relationoperator, bool value) : base(field,
            relationoperator, value)
        {
        }

        /// <summary>
        ///     创建简单布尔条件实例。
        /// </summary>
        /// <param name="source">源名称</param>
        /// <param name="field">字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="value">参考值</param>
        public BoolCriteria(string source, string field, ERelationOperator relationoperator, bool value) : base(source,
            field, relationoperator, value)
        {
        }
    }
}