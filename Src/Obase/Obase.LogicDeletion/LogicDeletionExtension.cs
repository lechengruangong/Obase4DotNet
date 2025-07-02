/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除扩展.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:26:29
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除扩展
    /// </summary>
    public class LogicDeletionExtension : TypeExtension
    {
        /// <summary>
        ///     删除标记的映射字段
        /// </summary>
        private string _deletionField;

        /// <summary>
        ///     逻辑删除标记的属性的名称
        /// </summary>
        private string _deletionMark;

        /// <summary>
        ///     逻辑删除标记的属性的名称
        /// </summary>
        public string DeletionMark
        {
            get => _deletionMark;
            set => _deletionMark = value;
        }

        /// <summary>
        ///     删除标记的映射字段
        /// </summary>
        public string DeletionField
        {
            get => _deletionField;
            set => _deletionField = value;
        }
    }
}