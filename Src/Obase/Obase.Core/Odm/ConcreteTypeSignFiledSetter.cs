/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：具体类型区别属性的字段设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:00:24
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     具体类型区别属性的字段设值器
    /// </summary>
    public class ConcreteTypeSignFiledSetter : IValueSetter
    {
        /// <summary>
        ///     定义的字段名称
        /// </summary>
        private readonly string _fieldName;

        /// <summary>
        ///     具体类型区别属性的字段设值器
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        public ConcreteTypeSignFiledSetter(string fieldName)
        {
            _fieldName = fieldName;
        }

        /// <summary>
        ///     获取设值模式。
        /// </summary>
        public EValueSettingMode Mode => EValueSettingMode.Assignment;

        /// <summary>
        ///     为对象设值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        public void SetValue(object obj, object value)
        {
            if (obj != null)
            {
                //获取字段信息
                var fieldInfo = obj.GetType().GetField(_fieldName);
                if (fieldInfo != null)
                {
                    var tValueType = fieldInfo.FieldType;
                    var isNullable = tValueType.IsGenericType;
                    //目标对象和值对象空判断
                    if (value == null || value is DBNull) return;
                    //nullable
                    if (isNullable)
                        tValueType = tValueType.GenericTypeArguments[0];
                    //统一转换值类型
                    value = Utils.ConvertDbValue(value, tValueType);
                    fieldInfo.SetValue(obj, value);
                }
            }
        }
    }
}