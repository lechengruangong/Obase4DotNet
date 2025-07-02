/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置对象类型的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:02:49
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置对象类型的规范。
    /// </summary>
    public interface IObjectTypeConfigurator : IStructuralTypeConfigurator
    {
        /// <summary>
        ///     获取类型各元素上设置的行为触发器，注：相同的触发器只返回一个实例。
        /// </summary>
        List<IBehaviorTrigger> BehaviorTriggers { get; }

        /// <summary>
        ///     获取映射表
        /// </summary>
        string TargetTable { get; }

        /// <summary>
        ///     获取行为触发器触发的对象行为所涉及到的元素。
        /// </summary>
        /// <param name="trigger">指定的触发器实例。</param>
        ITypeElementConfigurator[] GetBehaviorElements(IBehaviorTrigger trigger);

        /// <summary>
        ///     设置并发冲突处理策略。
        /// </summary>
        /// <param name="strategy">冲突处理策略。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy strategy, bool overrided = true);

        /// <summary>
        ///     设置要包含在对象变更通知中的属性。
        /// </summary>
        /// <param name="noticeAttributes">要包含的属性的名称的集合。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasNoticeAttributes(string[] noticeAttributes, bool overrided = true);

        /// <summary>
        ///     设置一个值，该值指示对象创建时是否发送通知。
        /// </summary>
        /// <param name="notifyCreation">指示是否发送对象创建通知。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasNotifyCreation(bool notifyCreation, bool overrided = true);

        /// <summary>
        ///     设置一个值，该值指示对象删除时是否发送通知。
        /// </summary>
        /// <param name="notifyDeletion">指示是否发送对象删除通知。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasNotifyDeletion(bool notifyDeletion, bool overrided = true);

        /// <summary>
        ///     设置一个值，该值指示对象更新时是否发送通知。
        /// </summary>
        /// <param name="notifyUpdate">指示是否发送对象更新通知。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasNotifyUpdate(bool notifyUpdate, bool overrided = true);

        /// <summary>
        ///     设置版本标识属性集（版本键）。每调用一次本方法将追加一个版本标识属性。
        /// </summary>
        /// <param name="attribute">属性的名称。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasVersionAttribute(string attribute, bool overrided = true);

        /// <summary>
        ///     设置映射表。
        /// </summary>
        /// <param name="table">映射表的名称。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void ToTable(string table, bool overrided = true);
    }
}