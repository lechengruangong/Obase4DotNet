/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：只进读取器接口,包含普通版本和泛型版本.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:07:20
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     只进读取器。
    ///     提供读取集合“只进”流的方法，并定义一种延迟执行机制对集合元素进行排序。
    /// </summary>
    public interface IForwardReader
    {
        /// <summary>
        ///     获取一个值，该值指示“只进”读取器是否可重置。
        /// </summary>
        bool Resetable { get; }

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        object Current { get; }

        /// <summary>
        ///     将读取器向前移动到下一个元素。
        ///     刚被初始化或重置时，读取器位于只进流的起始位置，即第一个元素之前；读取结束后位置流的结束位置，即最后一个元素之后。
        /// </summary>
        /// <returns>如果移动成功返回true，如果已位于最后一个元素返回false。</returns>
        bool Read();

        /// <summary>
        ///     关闭读取器。
        /// </summary>
        void Close();


        /// <summary>
        ///     使用指定的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader OrderBy(IComparer comparer = null);

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader OrderByDescending(IComparer comparer = null);

        /// <summary>
        ///     将读取器回退到只进流的起始位置（第一个元素之前）。
        ///     如果当前只进流不可重置则引发异常。
        /// </summary>
        /// <exception cref="Exception">当前读取器不支持重置操作。</exception>
        void Reset();

        /// <summary>
        ///     将读取器向前移动指定个数的元素。
        /// </summary>
        /// <returns>实际移动数。当流中当前位置之后的元素数少于请求数时，实际移动数将小于请求数。</returns>
        /// <param name="count">提升的元素个数。</param>
        int Skip(int count);

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        IForwardReader Take(int count);

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        /// <param name="resultCount">
        ///     实际读取到的元素个数。
        ///     当流中当前位置之后的元素数少于请求数时，实际读取数将小于请求数。
        /// </param>
        IForwardReader Take(int count, out int resultCount);
    }

    /// <summary>
    ///     泛型版本的读取集合“只进”流的方法，并定义一种延迟执行机制对集合元素进行排序。
    /// </summary>
    /// <typeparam name="T">集合的类型</typeparam>
    public interface IForwardReader<T> : IForwardReader
    {
        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        new T Current { get; }

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        new IForwardReader<T> Take(int count);

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        /// <param name="resultCount">
        ///     实际读取到的元素个数。
        ///     当流中当前位置之后的元素数少于请求数时，实际读取数将小于请求数。
        /// </param>
        new IForwardReader<T> Take(int count, out int resultCount);

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader<T> OrderBy(IComparer<T> comparer = null);

        /// <summary>
        ///     使用指定的排序键和默认的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <exception cref="Exception">排序键的类型没有默认比较器。</exception>
        /// <param name="keySelector">用于计算排序键的委托。</param>
        IOrderedReader<T> OrderBy<TKey>(Func<T, TKey> keySelector);

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader<T> OrderByDescending(IComparer<T> comparer = null);

        /// <summary>
        ///     使用指定的排序键和默认的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <exception cref="Exception">排序键的类型没有默认比较器。</exception>
        /// <param name="keySelector">用于计算排序键的委托。</param>
        IOrderedReader<T> OrderByDescending<TKey>(Func<T, TKey> keySelector);
    }
}