/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：实体型标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:21:36
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     实体型标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        ///     初始化实体型标注属性
        /// </summary>
        /// <param name="tableName">表名 无设置即为类名</param>
        /// <param name="keyAttributes">主键集合</param>
        /// <param name="isSelfIncrease">是否主键自增</param>
        public EntityAttribute(string tableName = null, bool isSelfIncrease = true, params string[] keyAttributes)
        {
            if (keyAttributes == null || keyAttributes.Length == 0)
                throw new ArgumentException("至少要指定一个主键");
            KeyAttributes = keyAttributes;
            IsSelfIncrease = isSelfIncrease;
            TableName = tableName?.Replace(" ", "");
        }

        /// <summary>
        ///     主键集合
        /// </summary>
        public string[] KeyAttributes { get; }

        /// <summary>
        ///     是否主键自增
        /// </summary>
        public bool IsSelfIncrease { get; }

        /// <summary>
        ///     表名
        /// </summary>
        public string TableName { get; }
    }
}