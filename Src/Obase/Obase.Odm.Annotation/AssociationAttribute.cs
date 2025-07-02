/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：显式关联型标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:13:57
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     显式关联型标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssociationAttribute : Attribute
    {
        /// <summary>
        ///     初始化关联型标注属性
        /// </summary>
        /// <param name="tableName">表名</param>
        public AssociationAttribute(string tableName = null)
        {
            TableName = tableName?.Replace(" ", "");
        }

        /// <summary>
        ///     表名
        /// </summary>
        public string TableName { get; }
    }
}