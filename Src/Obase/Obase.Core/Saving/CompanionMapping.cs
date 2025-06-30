/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示参与伴随映射的关联对象及其状态.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:09:38
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     表示参与伴随映射的关联对象及其状态。
    /// </summary>
    public class CompanionMapping
    {
        /// <summary>
        ///     创建CompanionMapping实例。
        /// </summary>
        /// <param name="associationObj">伴随关联对象。</param>
        /// <param name="status">伴随关联对象的状态。</param>
        public CompanionMapping(object associationObj, EObjectStatus status)
        {
            AssociationObject = associationObj;
            Status = status;
        }

        /// <summary>
        ///     获取伴随关联对象。
        /// </summary>
        public object AssociationObject { get; }

        /// <summary>
        ///     获取伴随关联对象的状态。
        /// </summary>
        public EObjectStatus Status { get; }
    }
}