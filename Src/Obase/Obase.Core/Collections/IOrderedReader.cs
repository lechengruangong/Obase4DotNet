/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序结果读取器接口,包含普通版本和泛型版本.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:08:07
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     排序结果读取器。
    ///     作为排序结果封装排序延迟执行逻辑，并提供对结果再次排序的方法。
    /// </summary>
    public interface IOrderedReader : IForwardReader
    {
        /// <summary>
        ///     对其执行排序操作的源序列。
        /// </summary>
        IForwardReader Source { get; }

        /// <summary>
        ///     使用指定的比较器，采用延迟执行策略对排序结果执行递进排序（升序）。
        /// </summary>
        /// <returns>代表排序结果的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader ThenBy(IComparer comparer = null);

        /// <summary>
        ///     使用指定的比较器，采用延迟执行策略对排序结果执行递进排序（降序）。
        /// </summary>
        /// <returns>代表排序结果的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader ThenByDescending(IComparer comparer = null);
    }

    /// <summary>
    ///     泛型版本的排序结果读取器。
    ///     作为泛型版本的排序结果封装排序延迟执行逻辑，并提供对结果再次排序的方法。
    ///     <typeparam name="T">集合元素类型</typeparam>
    /// </summary>
    public interface IOrderedReader<T> : IOrderedReader, IForwardReader<T>
    {
        /// <summary>
        ///     对其执行排序操作的源序列。
        /// </summary>
        new IForwardReader<T> Source { get; }

        /// <summary>
        ///     使用指定的比较器，采用延迟执行策略对排序结果执行递进排序（升序）。
        /// </summary>
        /// <returns>代表排序结果的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader<T> ThenBy(IComparer<T> comparer = null);

        /// <summary>
        ///     使用指定的排序键和默认的比较器，采用延迟执行策略对排序结果执行递进排序（升序）。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的只进读取器。</returns>
        /// <exception cref="Exception">当前类型没有默认比较器。</exception>
        /// <param name="keySelector">用于计算排序键的委托。</param>
        IOrderedReader<T> ThenBy<TKey>(Func<T, TKey> keySelector);


        /// <summary>
        ///     使用指定的比较器，采用延迟执行策略对排序结果执行递进排序（降序）。
        /// </summary>
        /// <returns>代表排序结果的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader<T> ThenByDescending(IComparer<T> comparer = null);

        /// <summary>
        ///     使用指定的排序键和默认的比较器，采用延迟执行策略对排序结果执行递进排序（降序）。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的只进读取器。</returns>
        /// <exception cref="Exception">当前类型没有默认比较器。</exception>
        /// <param name="keySelector">用于计算排序键的委托。</param>
        IOrderedReader<T> ThenByDescending<TKey>(Func<T, TKey> keySelector);
    }
}