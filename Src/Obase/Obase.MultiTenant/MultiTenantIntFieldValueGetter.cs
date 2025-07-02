/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户int类型主键字段取值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:58:01
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;
using Obase.Core.Odm;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户int类型主键字段取值器
    /// </summary>
    public class MultiTenantIntFieldValueGetter : IValueGetter
    {
        /// <summary>
        ///     表示要取其值的字段。
        /// </summary>
        private readonly FieldInfo _fieldInfo;

        /// <summary>
        ///     宿主上下文类
        /// </summary>
        private readonly Type _hostContextType;

        /// <summary>
        ///     目标类型
        /// </summary>
        private readonly Type _targetType;

        /// <summary>
        ///     初始化多租户int类型主键字段取值器
        /// </summary>
        /// <param name="fieldInfo">字段</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="hostContextType">宿主上下文类型</param>
        public MultiTenantIntFieldValueGetter(FieldInfo fieldInfo, Type targetType, Type hostContextType)
        {
            _fieldInfo = fieldInfo;
            _targetType = targetType;
            _hostContextType = hostContextType;
        }

        /// <summary>
        ///     从指定对象取值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            if (obj.GetType() == _targetType) return _fieldInfo.GetValue(obj);

            var value = Extensions.GetTenantId(_hostContextType);

            return Convert.ToInt32(value.ToString());
        }
    }
}