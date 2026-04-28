/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：有模型的序列化取值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 16:55:56
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Common;
using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     有模型的序列化取值器
    /// </summary>
    public class SerializedModelValueGetter : IValueGetter
    {
        /// <summary>
        ///     基础取值器
        /// </summary>
        private readonly IValueGetter _baseValueGetter;

        /// <summary>
        ///     序列化对象数据模型
        /// </summary>
        private readonly SerializationObjectDataModel _model;

        /// <summary>
        ///     序列化器
        /// </summary>
        private readonly ITextSerializer _serializer;

        /// <summary>
        ///     初始化有模型的序列化取值器
        /// </summary>
        /// <param name="baseValueGetter">基础取值器</param>
        /// <param name="serializer">序列化器</param>
        /// <param name="model">序列化对象数据模型</param>
        public SerializedModelValueGetter(IValueGetter baseValueGetter, ITextSerializer serializer,
            SerializationObjectDataModel model)
        {
            _baseValueGetter = baseValueGetter;
            _serializer = serializer;
            _model = model;
        }

        /// <summary>
        ///     从指定对象取值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            //取值
            var value = _baseValueGetter.GetValue(obj);
            //转换为列表
            var targets = Utils.GetObjectList(value);
            //创建序列化器
            var serializer = new SerializationObjectDataModelSerializer(_model);
            //序列化
            return _serializer.Serialize(serializer.Serialize(targets));
        }
    }
}