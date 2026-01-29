/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字段设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:22:50
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     字段设值器，使用该设值器可以直接为表示元素的字段设置值。
    /// </summary>
    public class FieldValueSetter : ValueSetter
    {
        /// <summary>
        ///     要为其设值的字段。
        /// </summary>
        private readonly FieldInfo _fieldInfo;

        /// <summary>
        ///     创建FieldValueSetter实例。
        /// </summary>
        /// <param name="fieldInfo">要为其设值的字段。</param>
        public FieldValueSetter(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }

        /// <summary>
        ///     获取设值模式。
        ///     注：本属性总是返回Assignment。
        /// </summary>
        public override EValueSettingMode Mode => EValueSettingMode.Assignment;

        /// <summary>
        ///     为对象设值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        protected override void SetValueCore(object obj, object value)
        {
            var tValueType = _fieldInfo.FieldType;
            var isNullable = tValueType.IsGenericType;
            //目标对象和值对象空判断
            if (obj == null || value == null || value is DBNull) return;
            //nullable
            if (isNullable)
                tValueType = tValueType.GenericTypeArguments[0];
            //使用统一的转换方法将值转换为字段类型
            value = Utils.ConvertDbValue(value, tValueType);
            _fieldInfo.SetValue(obj, value);
        }
    }
}