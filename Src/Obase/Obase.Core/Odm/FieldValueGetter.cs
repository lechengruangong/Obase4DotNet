/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字段取值器,可以直接获取表示属性的字段的值.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:58:54
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     字段取值器，使用该取值器可以直接获取表示属性的字段的值。
    /// </summary>
    public class FieldValueGetter : IValueGetter
    {
        /// <summary>
        ///     表示要取其值的字段。
        /// </summary>
        private readonly FieldInfo _fieldInfo;

        /// <summary>
        ///     创建FieldValueSetter实例。
        /// </summary>
        /// <param name="fieldInfo">要取其值的字段。</param>
        public FieldValueGetter(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }

        /// <summary>
        ///     从指定对象取值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            return _fieldInfo.GetValue(obj);
        }
    }
}