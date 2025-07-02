/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户的字段设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:56:50
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;
using Obase.Core.Common;
using Obase.Core.Odm;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户的字段设值器
    /// </summary>
    public class MultiTenantFieldValueSetter : IValueSetter
    {
        /// <summary>
        ///     表示要设值的字段。
        /// </summary>
        private readonly FieldInfo _fieldInfo;


        /// <summary>
        ///     目标的结构化类型
        /// </summary>
        private readonly StructuralType _structuralType;

        /// <summary>
        ///     多租户的字段设值器
        /// </summary>
        /// <param name="fieldInfo">字段</param>
        /// <param name="structuralType">结构化类型</param>
        public MultiTenantFieldValueSetter(FieldInfo fieldInfo, StructuralType structuralType)
        {
            _fieldInfo = fieldInfo;
            _structuralType = structuralType;
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
            var currentType = obj.GetType();
            if (_structuralType.RebuildingType == currentType)
                SetValueCore(_fieldInfo, obj, value);
            else
                SetDerivedValue(_structuralType, currentType, obj, value);
        }

        /// <summary>
        ///     查找所有的子类型 以确定具体的值
        /// </summary>
        /// <param name="structuralType">结构化类型</param>
        /// <param name="currentType">当前运行时类型</param>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private void SetDerivedValue(StructuralType structuralType, Type currentType, object obj, object value)
        {
            foreach (var derivedType in structuralType.DerivedTypes)
            {
                if (derivedType.RebuildingType == currentType)
                {
                    SetValueCore(derivedType.RebuildingType.GetField(_fieldInfo.Name), obj, value);
                    return;
                }

                SetDerivedValue(derivedType, currentType, obj, value);
            }
        }

        /// <summary>
        ///     实际设值
        /// </summary>
        /// <param name="fieldInfo">字段</param>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        private void SetValueCore(FieldInfo fieldInfo, object obj, object value)
        {
            var tValueType = fieldInfo.FieldType;
            var isNullable = tValueType.IsGenericType;
            //目标对象和值对象空判断
            if (value == null || value is DBNull)
                return;
            //nullable
            if (isNullable)
                tValueType = tValueType.GenericTypeArguments[0];
            value = Utils.ConvertDbValue(value, tValueType);
            fieldInfo.SetValue(obj, value);
        }
    }
}