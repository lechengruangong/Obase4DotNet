/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置属性的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:21:42
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置属性的规范。
    /// </summary>
    public interface IAttributeConfigurator : ITypeElementConfigurator
    {
        /// <summary>
        ///     设置修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="changeTrigger">修改触发器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasChangeTrigger(IBehaviorTrigger changeTrigger, bool overrided = true);

        /// <summary>
        ///     使用一个能触发属性修改的方法为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发属性修改的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasChangeTrigger(MethodInfo method, bool overrided = true);

        /// <summary>
        ///     使用一个能触发属性修改的属性访问器为属性创建Property-Set型修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发属性修改的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasChangeTrigger(PropertyInfo property, bool overrided = true);

        /// <summary>
        ///     使用一个能触发属性修改的属性访问器为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发属性修改的属性访问器。</param>
        /// <param name="triggerType">要创建的触发器类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasChangeTrigger(PropertyInfo property, EBehaviorTriggerType triggerType, bool overrided = true);

        /// <summary>
        ///     使用一个能触发属性修改的成员为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的触发器类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasChangeTrigger(string memberName, EBehaviorTriggerType triggerType, bool overrided = true);

        /// <summary>
        ///     使用与属性同名的成员为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的触发器类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasChangeTrigger(EBehaviorTriggerType triggerType, bool overrided = true);

        /// <summary>
        ///     使用与属性同名的属性访问器为属性创建Property-Set型修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasChangeTrigger(bool overrided = true);

        /// <summary>
        ///     设置属性的合并处理器。
        /// </summary>
        /// <param name="combiner">属性的合并处理器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasCombinationHandler(IAttributeCombinationHandler combiner, bool overrided = true);

        /// <summary>
        ///     设置与指定的属性合并处理策略对应的合并处理器。
        /// </summary>
        /// <param name="strategy">属性的合并处理策略。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasCombinationHandler(EAttributeCombinationHandlingStrategy strategy, bool overrided = true);

        /// <summary>
        ///     设置映射连接符。
        /// </summary>
        /// <param name="value">映射连接符</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasMappingConnectionChar(char value, bool overrided = true);

        /// <summary>
        ///     设置映射字段。
        /// </summary>
        /// <param name="field">映射字段</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void ToField(string field, bool overrided = true);

        /// <summary>
        ///     设置最大字符数
        /// </summary>
        /// <param name="maxcharNumber">最大字符数 只有1到255是有效的 如果设置为0 会被设置为255 如果超过255 会被设置为Text字段</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasMaxcharNumber(ushort maxcharNumber, bool overrided = true);

        /// <summary>
        ///     设置精度
        ///     只支持为映射类型decimal设置精度
        /// </summary>
        /// <param name="precision">以小数位数表示的精度，0表示小数点后没有位数。精度最大值28</param>
        /// <returns></returns>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasPrecision(byte precision, bool overrided = true);

        /// <summary>
        ///     设置是否可空
        /// </summary>
        /// <param name="value">指示是否可空。对于主键设置为可空是无效的</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasNullable(bool value, bool overrided = true);
    }
}