/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置引用元素的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:59:00
└──────────────────────────────────────────────────────────────┘
*/


using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置引用元素的规范。
    /// </summary>
    public interface IReferenceElementConfigurator : ITypeElementConfigurator
    {
        /// <summary>
        ///     是否已启用延迟加载。
        /// </summary>
        bool EnableLazyLoading { get; }

        /// <summary>
        ///     设置是否支持延迟加载。
        /// </summary>
        /// <param name="enableLazyLoading"></param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasEnableLazyLoading(bool enableLazyLoading, bool overrided = true);

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        /// <param name="loadingPriority">加载优先级。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingPriority(int loadingPriority, bool overrided = true);

        /// <summary>
        ///     设置加载触发器。
        ///     每次调用本方法将追加一个加载触发器。
        /// </summary>
        /// <param name="loadingTrigger">加载触发器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingTrigger(IBehaviorTrigger loadingTrigger, bool overrided = true);

        /// <summary>
        ///     使用一个能触发引用加载的方法为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发引用加载的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingTrigger(MethodInfo method, bool overrided = true);

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingTrigger(PropertyInfo property, bool overrided = true);

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingTrigger(PropertyInfo property, EBehaviorTriggerType triggerType, bool overrided = true);

        /// <summary>
        ///     使用一个能触发引用加载的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingTrigger(string memberName, EBehaviorTriggerType triggerType, bool overrided = true);

        /// <summary>
        ///     使用与引用元素同名的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingTrigger(EBehaviorTriggerType triggerType, bool overrided = true);

        /// <summary>
        ///     使用与引用元素同名的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLoadingTrigger(bool overrided = true);
    }
}