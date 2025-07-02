/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:25:52
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除注属性 用于指定哪个字段用于逻辑删除
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LogicDeletionAttribute : Attribute
    {
        /// <summary>
        ///     构造逻辑删除标记
        /// </summary>
        /// <param name="deletionField">指定哪个字段用于逻辑删除</param>
        public LogicDeletionAttribute(string deletionField)
        {
            DeletionField = deletionField?.Replace(" ", "");
        }

        /// <summary>
        ///     哪个字段用于逻辑删除
        /// </summary>
        public string DeletionField { get; }
    }
}