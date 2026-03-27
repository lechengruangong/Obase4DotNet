/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：有模型的序列化设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 17:08:51
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using Obase.Core.Common;
using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     有模型的序列化设值器
    /// </summary>
    public class SerializedModelValueSetter : IValueSetter
    {
        /// <summary>
        ///     基础设值器
        /// </summary>
        private readonly IValueSetter _baseValueSetter;

        /// <summary>
        ///     所属的属性是否为多重的
        /// </summary>
        private readonly bool _isAttitudeMultiple;

        /// <summary>
        ///     序列化对象数据模型
        /// </summary>
        private readonly SerializationObjectDataModel _model;


        /// <summary>
        ///     序列化器
        /// </summary>
        private readonly ITextSerializer _serializer;

        /// <summary>
        ///     反序列化后的类型
        /// </summary>
        private readonly Type _valueType;

        /// <summary>
        ///     初始化有模型的序列化设值器
        /// </summary>
        /// <param name="baseValueSetter">基础设值器</param>
        /// <param name="serializer">序列化器</param>
        /// <param name="valueType">反序列化后的类型</param>
        /// <param name="model">序列化对象数据模型</param>
        /// <param name="isMultiple">所属的属性是否为多重的</param>
        public SerializedModelValueSetter(IValueSetter baseValueSetter, ITextSerializer serializer, Type valueType,
            SerializationObjectDataModel model, bool isMultiple)
        {
            _baseValueSetter = baseValueSetter;
            _serializer = serializer;
            _valueType = valueType;
            _model = model;
            _isAttitudeMultiple = isMultiple;
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
            //创建反序列化器
            var deSerializer = new SerializationObjectDataModelDeSerialzer(_model);
            //反序列化后的对象集合
            var objects = deSerializer.DeSerialize((SerializationDataTransferObjectWrapper)realObj);
            if (objects != null && objects.Any())
                //如果是多值的属性 直接设置 否则 设置首个
                _baseValueSetter.SetValue(obj, _isAttitudeMultiple ? objects : objects.FirstOrDefault());
        }
    }
}