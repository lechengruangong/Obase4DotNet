/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示ElementAt运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:45:29
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示ElementAt运算。
    /// </summary>
    public class ElementAtOp : QueryOp
    {
        /// <summary>
        ///     要检索的从零开始的元素索引。
        /// </summary>
        private readonly int _index;

        /// <summary>
        ///     指示当指定索引处无元素时是否返回默认值。
        /// </summary>
        private readonly bool _returnDefault;

        /// <summary>
        ///     创建ElementAtOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="index">要检索的从零开始的元素索引。</param>
        /// <param name="returnDefault">指示当指定索引处无元素时是否返回默认值。</param>
        internal ElementAtOp(Type sourceType, int index, bool returnDefault = false)
            : base(EQueryOpName.ElementAt, sourceType)
        {
            _index = index;
            _returnDefault = returnDefault;
        }

        /// <summary>
        ///     获取要检索的从零开始的元素索引。
        /// </summary>
        public int Index => _index;

        /// <summary>
        ///     获取一个值，该值指示当指定索引处无元素时是否返回默认值。
        /// </summary>
        public bool ReturnDefault => _returnDefault;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}