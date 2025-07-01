/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字段条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:59:57
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     字段条件
    /// </summary>
    public class FieldCriteria : SimpleCriteria<Field>
    {
        /// <summary>
        ///     创建字段条件实例。
        /// </summary>
        /// <param name="leftField">左端字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="rightField">右端字段名</param>
        public FieldCriteria(string leftField, ERelationOperator relationoperator, string rightField) : base(leftField,
            relationoperator, new Field(rightField))
        {
        }

        /// <summary>
        ///     创建字段条件实例。
        /// </summary>
        /// <param name="leftSource">左端源名称</param>
        /// <param name="leftField">左端字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="rightSource">右端源名称</param>
        /// <param name="rightField">右端字段名</param>
        public FieldCriteria(string leftSource, string leftField, ERelationOperator relationoperator,
            string rightSource, string rightField) : base(leftSource, leftField, relationoperator,
            new Field(rightSource, rightField))
        {
        }

        /// <summary>
        ///     创建字段条件实例。
        /// </summary>
        /// <param name="leftField">左端字段</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="rightField">右端字段</param>
        public FieldCriteria(Field leftField, ERelationOperator relationoperator, Field rightField) : base(leftField,
            relationoperator, rightField)
        {
        }

        /// <summary>
        ///     创建字段条件实例。
        /// </summary>
        /// <param name="leftSource">左端源</param>
        /// <param name="leftField">左端字段名</param>
        /// <param name="relationoperator">关系运算符</param>
        /// <param name="rightSource">右端源</param>
        /// <param name="rightField">右端字段名</param>
        public FieldCriteria(ISource leftSource, string leftField, ERelationOperator relationoperator,
            ISource rightSource, string rightField) : base(new Field((MonomerSource)leftSource, leftField),
            relationoperator,
            new Field((MonomerSource)rightSource, rightField))
        {
        }
    }
}