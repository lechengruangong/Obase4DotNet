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
        ///     仅限字符串类型。字符数会换算为以位为单位的字段长度（字符数 * 8）存入属性的值长度，由存储结构映射据此生成列长度。
        ///     默认长度（未调用本方法时）：
        ///     字符串属性的值长度为0，表示“未指定长度、不限制长度”；存储结构映射遇到长度0时使用内置缺省值
        ///     （未设置字段长度的回退取值，注意不是255）：
        ///     SqlServer 为40个字符，生成 nvarchar(40)；MySql 为40个字符，生成 varchar(40)；
        ///     PostgreSql 为40个字符，生成 varchar(40)；Sqlite 不写长度，生成 TEXT。
        ///     调用本方法后，换算出的长度超过360位（即字符数超过45）时，SqlServer 生成 nvarchar(max)，
        ///     MySql/PostgreSql 生成 Text；不超过360位时按实际字符数生成列长度。
        /// </summary>
        /// <param name="maxcharNumber">最大字符数。传0等同于不限制长度（按上述缺省处理，不会被改写为255）</param>
        /// <param name="overrided">是否覆盖既有配置。为false时仅在此前未设置过最大字符数时才生效</param>
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