/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除字段取值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:29:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;
using Obase.Core.Odm;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除字段取值器
    /// </summary>
    public class LogicDeletionFieldValueGetter : IValueGetter
    {
        /// <summary>
        ///     表示要取其值的字段。
        /// </summary>
        private readonly FieldInfo _fieldInfo;


        /// <summary>
        ///     目标的结构化类型
        /// </summary>
        private readonly StructuralType _structuralType;

        /// <summary>
        ///     创建FieldValueSetter实例。
        /// </summary>
        /// <param name="fieldInfo">要取其值的字段。</param>
        /// <param name="structuralType">结构化类型</param>
        public LogicDeletionFieldValueGetter(FieldInfo fieldInfo, StructuralType structuralType)
        {
            _fieldInfo = fieldInfo;
            _structuralType = structuralType;
        }

        /// <summary>
        ///     从指定对象取值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            var currentType = obj.GetType();
            var structuralType = _structuralType;
            //和自己相同 直接返回对象里保存的值
            if (structuralType.RebuildingType == currentType)
                return _fieldInfo.GetValue(obj);

            //向下寻找
            return GetDerivedValue(structuralType, currentType, obj);
        }

        /// <summary>
        ///     查找所有的子类型 以确定具体的值
        /// </summary>
        /// <param name="structuralType">结构化类型</param>
        /// <param name="currentType">当前运行时类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        private object GetDerivedValue(StructuralType structuralType, Type currentType, object obj)
        {
            foreach (var derivedType in structuralType.DerivedTypes)
            {
                if (derivedType.RebuildingType == currentType)
                    return derivedType.RebuildingType.GetField(_fieldInfo.Name).GetValue(obj);
                GetDerivedValue(derivedType, currentType, obj);
            }

            return false;
        }
    }
}