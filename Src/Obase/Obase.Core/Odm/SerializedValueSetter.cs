/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-10-14 18:09:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     序列化设值器
    /// </summary>
    public class SerializedValueSetter : IValueSetter
    {
        /// <summary>
        ///     基础设值器
        /// </summary>
        private readonly IValueSetter _baseValueSetter;


        /// <summary>
        ///     序列化器
        /// </summary>
        private readonly ITextSerializer _serializer;

        /// <summary>
        ///     反序列化后的类型
        /// </summary>
        private readonly Type _valueType;

        /// <summary>
        ///     初始化序列化设值器
        /// </summary>
        /// <param name="baseValueSetter">基础设值器</param>
        /// <param name="serializer">序列化器</param>
        /// <param name="valueType">反序列化后的类型</param>
        public SerializedValueSetter(IValueSetter baseValueSetter, ITextSerializer serializer, Type valueType)
        {
            _baseValueSetter = baseValueSetter;
            _serializer = serializer;
            _valueType = valueType;
        }

        /// <summary>
        ///     基础设值器
        /// </summary>
        public IValueSetter BaseValueSetter => _baseValueSetter;

        /// <summary>
        ///     获取设值模式。
        /// </summary>
        public EValueSettingMode Mode => _baseValueSetter.Mode;

        /// <summary>
        ///     为对象设值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        public void SetValue(object obj, object value)
        {
            //按照字符串处理
            var stringValue = value.ToString();
            //反序列化
            var realObj = _serializer.Deserialize(stringValue, _valueType);
            //设置值
            _baseValueSetter.SetValue(obj, realObj);
        }
    }
}