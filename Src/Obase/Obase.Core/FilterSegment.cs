/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：映射筛选器片段.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:11:27
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core
{
    /// <summary>
    ///     表示映射筛选器片段。
    ///     映射筛选器可以看成是由一个片段序列进行连续逻辑“与”运算生成的，每个片段是一个最简单的筛选条件，即指定域（依据域）的值为指定值（参考值）。
    /// </summary>
    public class FilterSegment
    {
        /// <summary>
        ///     片段所属的筛选器
        /// </summary>
        private readonly MappingFilter _owner;

        /// <summary>
        ///     一个委托，代表映射筛选器片段制作完成时回调的方法。
        ///     该方法的第一个参数表示筛选器的依据域，第二个字段表示参考值。
        /// </summary>
        private readonly Action<string, object> _segmentReady;

        /// <summary>
        ///     依据域，即作为筛选依据的域。
        /// </summary>
        private string _field;

        /// <summary>
        ///     参考值，即当依据域的值为该值时即判定满足条件。
        /// </summary>
        private object _referenceValue;

        /// <summary>
        ///     创建FilterSegment实例。
        /// </summary>
        /// <param name="owner">片段所属的筛选器。</param>
        /// <param name="segmentReady">一个委托，代表映射筛选器片段制作完成时回调的方法。</param>
        public FilterSegment(MappingFilter owner, Action<string, object> segmentReady)
        {
            _owner = owner;
            _segmentReady = segmentReady;
        }

        /// <summary>
        ///     设置筛选片段的依据域。
        /// </summary>
        /// <returns>当前筛选片段。</returns>
        /// <param name="field">字段</param>
        internal FilterSegment SetField(string field)
        {
            _field = field;
            return this;
        }

        /// <summary>
        ///     设置筛选片段的参考值。
        /// </summary>
        /// <returns>当前片段所属的筛选器。</returns>
        /// <param name="value">值</param>
        internal MappingFilter SetReferenceValue(object value)
        {
            _referenceValue = value;
            //设置了值之后 视作完成
            _segmentReady(_field, _referenceValue);
            return _owner;
        }
    }
}