/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Contains运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:40:13
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Contains运算。
    /// </summary>
    public class ContainsOp : QueryOp
    {
        /// <summary>
        ///     相等比较器，用于测试序列中的元素与要查找的元素是否相等。
        /// </summary>
        private readonly IEqualityComparer _comparer;

        /// <summary>
        ///     要在序列中查找的元素。
        /// </summary>
        private readonly object _item;

        /// <summary>
        ///     创建ContainsOp实例。
        /// </summary>
        /// <param name="item">要在序列中查找的元素。</param>
        /// <param name="comparer">相等比较器，用于测试序列中的元素与要查找的元素是否相等。</param>
        /// <param name="sourceType">源类型</param>
        internal ContainsOp(object item, Type sourceType, IEqualityComparer comparer = null)
            : base(EQueryOpName.Contains, sourceType)
        {
            _item = item;
            _comparer = comparer;
        }

        /// <summary>
        ///     获取相等比较器，该比较器用于测试序列中的元素与要查找的元素是否相等。
        /// </summary>
        public IEqualityComparer Comparer => _comparer;

        /// <summary>
        ///     获取要在序列中查找的元素。
        /// </summary>
        public object Item => _item;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => typeof(bool);
    }
}