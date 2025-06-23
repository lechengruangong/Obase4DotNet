/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：键序基读取器接口,沿键序列读取各个键对应的值.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:36:00
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     键序基读取器，沿键序列读取各个键对应的值。
    ///     该键序列中的键称为基键。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="T">值的类型</typeparam>
    public interface IKeySequenceBasedReader<TKey, T> : IForwardReader<T>
    {
        /// <summary>
        ///     获取基键序列。
        /// </summary>
        IForwardReader<TKey> Keys { get; }

        /// <summary>
        ///     将读取器向前移动到下一个元素。
        ///     刚被初始化或重置时，读取器位于只进流的起始位置，即第一个元素之前；读取结束后位置流的结束位置，即最后一个元素之后。
        /// </summary>
        /// <returns>如果移动成功返回true，如果已位于最后一个元素返回false。</returns>
        /// <exception cref="KeyNotFoundException">读取器的基础集合中找不到当前基键。</exception>
        /// <param name="key">移动后当前位置的基键。</param>
        bool Read(out TKey key);

        /// <summary>
        ///     检查是否所有基键都能在基础集合中找到。
        /// </summary>
        /// <returns>如果所有基键都能在基础集合中找到，则返回true；否则返回false。</returns>
        bool ExistanceCheck();

        /// <summary>
        ///     获取基础集合中缺失的基键。
        /// </summary>
        IForwardReader<TKey> GetAbsent();
    }
}
