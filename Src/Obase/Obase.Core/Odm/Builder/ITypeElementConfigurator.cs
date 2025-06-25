/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置类型元素的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:24:17
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置类型元素的规范
    /// </summary>
    public interface ITypeElementConfigurator
    {
        /// <summary>
        ///     为元素配置项设置一个扩展配置器。
        /// </summary>
        /// <param name="configType">扩展配置器的类型，须继承自ElementExtensionConfiguration。</param>
        ElementExtensionConfiguration HasExtension(Type configType);

        /// <summary>
        ///     为元素配置项设置一个扩展配置器
        /// </summary>
        /// <typeparam name="TExtensionConfiguration">扩展配置器的类型，须继承自ElementExtensionConfiguration。</typeparam>
        /// <returns></returns>
        ElementExtensionConfiguration HasExtension<TExtensionConfiguration>()
            where TExtensionConfiguration : ElementExtensionConfiguration, new();

        /// <summary>
        ///     为类型元素设置取值器。
        /// </summary>
        /// <param name="valueGetter">取值器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueGetter(IValueGetter valueGetter, bool overrided = true);

        /// <summary>
        ///     使用一个能够获取类型元素值的方法为类型元素创建取值器。
        ///     如果该方法的返回值类型与元素的IsMultiple属性不匹配，则引发异常。
        ///     实施建议：
        ///     调用MethodInfo类的CreateDelegate方法创建代表该方法的委托，然后创建委托取值器。
        /// </summary>
        /// <param name="method">获取元素值的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueGetter(MethodInfo method, bool overrided = true);

        /// <summary>
        ///     使用一个能够获取类型元素值的属性访问器为类型元素创建取值器。
        ///     如果该属性访问器的返回值类型与元素的IsMultiple属性不匹配，则引发异常。
        ///     实施建议：
        ///     首先取出该属性访问器的Get方法，然后调用MethodInfo类的CreateDelegate方法创建代表该方法的委托，最后创建委托取值器。
        /// </summary>
        /// <param name="property">获取元素值的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueGetter(PropertyInfo property, bool overrided = true);

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建取值器。
        ///     如果字段的数据类型与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueGetter(FieldInfo field, bool overrided = true);

        /// <summary>
        ///     使用指定的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueGetter(string memberName, MemberTypes memberType, bool overrided = true);

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="memberType">同名成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueGetter(MemberTypes memberType, bool overrided = true);

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueGetter(bool overrided = true);

        /// <summary>
        ///     为类型元素设置设值器。
        /// </summary>
        /// <param name="valueSetter">设值器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueSetter(IValueSetter valueSetter, bool overrided = true);

        /// <summary>
        ///     使用一个能够为类型元素设值的方法为类型元素创建设值器。
        ///     实施说明
        ///     检测方法的DeclaringType，如果为引用类型，使用MethodInfo.CreateDelegate方法创建Action{TStructural,
        ///     TElement}委托；如果是结构体，使用Emit创建SetValue{TStructural, TElement}委托。
        ///     使用上述委托，调用ValueSetter的Create方法创建设值器。
        /// </summary>
        /// <param name="method">为类型元素设值的方法。</param>
        /// <param name="mode">设值模式。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueSetter(MethodInfo method, EValueSettingMode mode, bool overrided = true);

        /// <summary>
        ///     使用指定的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueSetter(string memberName, MemberTypes memberType, bool overrided = true);

        /// <summary>
        ///     使用一个能够为类型元素设值的Property为类型元素创建设值器。
        ///     实施说明
        ///     取出Property的Set方法，然后调用HasValueSetter(methodInfo, mode)方法。
        /// </summary>
        /// <param name="property">为类型元素设值的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueSetter(PropertyInfo property, bool overrided = true);

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberType">成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueSetter(MemberTypes memberType, bool overrided = true);

        /// <summary>
        ///     为集合类型的元素创建设值器，该设值器可以向集合添加或移除元素。
        ///     实施说明
        ///     检测方法的DeclaringType，如果为引用类型，使用MethodInfo.CreateDelegate方法创建Action{TStructural,
        ///     TElement>委托；如果是结构体，使用Emit创建SetValue{TStructural, TElement}委托。
        ///     使用上述委托，实例化CollectionValueSetter。
        /// </summary>
        /// <param name="appendingMethod">添加集合项的方法。</param>
        /// <param name="removingMethod">移除集合项的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueSetter(MethodInfo appendingMethod, MethodInfo removingMethod, bool overrided = true);

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建设值器。
        /// </summary>
        void HasValueSetter(bool overrided = true);

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建设值器。
        ///     实施说明
        ///     使用ValueSetter类的Create方法创建设值器。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasValueSetter(FieldInfo field, bool overrided = true);

        /// <summary>
        ///     进入当前元素所属类型的配置项。
        /// </summary>
        IStructuralTypeConfigurator Upward();
    }
}