/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：可计数接口,提供统计集合或序列中元素个数的机制.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:33:42
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     提供统计集合或序列中元素个数的机制。
    /// </summary>
    public interface ICountable
    {
        /// <summary>
        ///     获取一个值，该值指示集合或序列是否支持统计元素个数的操作。
        /// </summary>
        bool CanCount { get; }

        /// <summary>
        ///     获取元素个数。
        /// </summary>
        /// <exception cref="NotSupportedException">不支持统计元素个数操作。</exception>
        long Count { get; }
    }
}
