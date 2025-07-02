/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构体的字段设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:20:24
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     适用于结构体的字段设值器。
    /// </summary>
    public class StructFieldValueSetter : StructValueSetter
    {
        /// <summary>
        ///     要设置其值的字段。
        /// </summary>
        private readonly FieldInfo _fieldInfo;


        /// <summary>
        ///     创建FieldStructValueSetter实例。
        /// </summary>
        /// <param name="fieldInfo">要设置其值的字段。</param>
        internal StructFieldValueSetter(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }

        /// <summary>
        ///     获取设值模式。
        ///     注：本属性总是返回Assignment。
        /// </summary>
        public override EValueSettingMode Mode => EValueSettingMode.Assignment;

        /// <summary>
        ///     为结构体设值。
        /// </summary>
        /// <param name="structObj">目标结构体。</param>
        /// <param name="value">值对象</param>
        protected override void SetStructValue(ref object structObj, object value)
        {
            _fieldInfo.SetValue(structObj, value);
        }
    }
}