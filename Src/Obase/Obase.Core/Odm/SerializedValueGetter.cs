/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化取值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-10-14 18:09:28
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     序列化取值器
    /// </summary>
    public class SerializedValueGetter : IValueGetter
    {
        /// <summary>
        ///     基础取值器
        /// </summary>
        private readonly IValueGetter _baseValueGetter;


        /// <summary>
        ///     序列化器
        /// </summary>
        private readonly ITextSerializer _serializer;

        /// <summary>
        ///     初始化序列化取值器
        /// </summary>
        /// <param name="baseValueGetter">基础取值器</param>
        /// <param name="serializer">序列化器</param>
        public SerializedValueGetter(IValueGetter baseValueGetter, ITextSerializer serializer)
        {
            _baseValueGetter = baseValueGetter;
            _serializer = serializer;
        }

        /// <summary>
        ///     基础取值器
        /// </summary>
        public IValueGetter BaseValueGetter => _baseValueGetter;

        /// <summary>
        ///     从指定对象取值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            //序列化
            return _serializer.Serialize(_baseValueGetter.GetValue(obj));
        }
    }
}