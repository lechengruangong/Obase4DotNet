/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：只进读取器基础实现,提供只进读取器基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:25:24
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     为采用预读机制的只进读取器提供基础实现
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    public abstract class ForwardReader<T> : IForwardReader<T>
    {
        /// <summary>
        ///     存储预读取元素的缓冲区。
        /// </summary>
        protected readonly Queue<T> PrereadItems;

        /// <summary>
        ///     每次预读时最多读取的元素数。
        /// </summary>
        private int _prereadMax;

        /// <summary>
        ///     创建ForwardReader实例，并将预读上限数设置为默认值。
        /// </summary>
        protected ForwardReader() : this(256)
        {
        }

        /// <summary>
        ///     创建ForwardReader实例，并指定预读上限数。
        /// </summary>
        /// <param name="prereadMax">预读上限</param>
        private ForwardReader(int prereadMax)
        {
            _prereadMax = prereadMax;
            PrereadItems = new Queue<T>();
        }

        /// <summary>
        ///     每次预读时最多读取的元素数。
        /// </summary>
        public int PrereadMax
        {
            get => _prereadMax;
            set => _prereadMax = value;
        }

        /// <summary>
        ///     获取一个值，该值指示“只进”读取器是否可重置
        /// </summary>
        public abstract bool Resetable { get; }

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        public abstract T Current { get; }

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        object IForwardReader.Current => Current;

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        public abstract IForwardReader<T> Take(int count);

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        /// <param name="resultCount">
        ///     实际读取到的元素个数。
        ///     当流中当前位置之后的元素数少于请求数时，实际读取数将小于请求数。
        /// </param>
        public abstract IForwardReader<T> Take(int count, out int resultCount);


        /// <summary>
        ///     使用指定的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        public abstract IOrderedReader<T> OrderBy(IComparer<T> comparer = null);

        /// <summary>
        ///     使用指定的排序键和默认的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        public abstract IOrderedReader<T> OrderBy<TKey>(Func<T, TKey> keySelector);

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        public abstract IOrderedReader<T> OrderByDescending(IComparer<T> comparer = null);


        /// <summary>
        ///     使用指定的排序键和默认的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <exception cref="Exception">排序键的类型没有默认比较器。</exception>
        /// <param name="keySelector">用于计算排序键的委托。</param>
        public abstract IOrderedReader<T> OrderByDescending<TKey>(Func<T, TKey> keySelector);


        /// <summary>
        ///     将读取器向前移动到下一个元素。
        ///     刚被初始化或重置时，读取器位于只进流的起始位置，即第一个元素之前；读取结束后位置流的结束位置，即最后一个元素之后。
        /// </summary>
        /// <returns>如果移动成功返回true，如果已位于最后一个元素返回false。</returns>
        bool IForwardReader.Read()
        {
            return Read();
        }

        /// <summary>
        ///     关闭读取器。
        /// </summary>
        void IForwardReader.Close()
        {
            Close();
        }

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader IForwardReader.OrderBy(IComparer comparer)
        {
            return OrderBy(comparer);
        }

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        IOrderedReader IForwardReader.OrderByDescending(IComparer comparer)
        {
            return OrderByDescending(comparer);
        }

        /// <summary>
        ///     将读取器回退到只进流的起始位置（第一个元素之前）。
        ///     如果当前只进流不可重置则引发异常。
        /// </summary>
        /// <exception cref="Exception">当前读取器不支持重置操作。</exception>
        void IForwardReader.Reset()
        {
            Reset();
        }

        /// <summary>
        ///     将读取器向前移动指定个数的元素。
        /// </summary>
        /// <returns>实际移动数。当流中当前位置之后的元素数少于请求数时，实际移动数将小于请求数。</returns>
        /// <param name="count">提升的元素个数。</param>
        int IForwardReader.Skip(int count)
        {
            return Skip(count);
        }

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        IForwardReader IForwardReader.Take(int count)
        {
            return Take(count);
        }

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        /// <param name="resultCount">
        ///     实际读取到的元素个数。
        ///     当流中当前位置之后的元素数少于请求数时，实际读取数将小于请求数。
        /// </param>
        IForwardReader IForwardReader.Take(int count, out int resultCount)
        {
            return Take(count, out resultCount);
        }

        /// <summary>
        ///     关闭读取器。
        /// </summary>
        public void Close()
        {
            DoClosing();
        }


        /// <summary>
        ///     将读取器向前移动到下一个元素。
        ///     刚被初始化或重置时，读取器位于只进流的起始位置，即第一个元素之前；读取结束后位置流的结束位置，即最后一个元素之后。
        /// </summary>
        /// <returns>如果移动成功返回true，如果已位于最后一个元素返回false。</returns>
        public abstract bool Read();

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        public abstract IOrderedReader OrderByDescending(IComparer comparer);

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        public abstract IOrderedReader OrderBy(IComparer comparer);


        /// <summary>
        ///     将读取器向前移动指定个数的元素。
        /// </summary>
        /// <returns>
        ///     实际移动数。当流中当前位置之后的元素数少于请求数时，实际移动数将小于请求数。
        ///     实施注意
        ///     在调用由派生类实现的DoSkipping方法时要注意，由于采用了预读策略，读取器的当前位置与基础流的当前位置是不一致的。
        /// </returns>
        /// <param name="count">提升的元素个数。</param>
        public int Skip(int count)
        {
            //比预读的少
            if (PrereadItems.Count > count)
            {
                //直接跳掉若干个预读的元素
                var i = 0;
                while (i < count)
                {
                    PrereadItems.Dequeue();
                    i++;
                }

                //返回0
                return 0;
            }

            //比预读的多 预读的跳掉
            var sipCount = DoSkipping(count - PrereadItems.Count);

            PrereadItems.Clear();

            return sipCount;
        }

        /// <summary>
        ///     将读取器回退到只进流的起始位置（第一个元素之前）。
        ///     如果当前只进流不可重置则引发异常。
        /// </summary>
        /// <exception cref="Exception">当前读取器不支持重置操作。</exception>
        public void Reset()
        {
            if (!Resetable) throw new InvalidOperationException("当前只进流不可重置");

            DoReseting();
        }

        /// <summary>
        ///     枚举已预读到缓冲区的元素。
        /// </summary>
        protected IEnumerator<T> EnumeratePreread()
        {
            return PrereadItems.GetEnumerator();
        }

        /// <summary>
        ///     从集合只进流中预读指定数量的元素放入缓冲区。
        /// </summary>
        /// <returns>读取到的元素的集合，未读到任何元素返回null。当流中当前位置之后的元素数小于请求数时，实际读取到的元素会小于请求数。</returns>
        /// <param name="maxCount">最大读入数</param>
        protected abstract T[] Preread(int maxCount);

        /// <summary>
        ///     将读取器向前移动指定个数的元素。
        /// </summary>
        /// <returns>实际移动数。当流中当前位置之后的元素数少于请求数时，实际移动数将小于请求数。</returns>
        /// <param name="number">跳过个数</param>
        protected abstract int DoSkipping(int number);

        /// <summary>
        ///     将读取器回退到只进流的起始位置（第一个元素之前）。
        ///     在派生类中实现时，如果当前集合的只进流不可重置，则不执行任何操作。
        /// </summary>
        protected abstract void DoReseting();

        /// <summary>
        ///     关闭读取器。
        /// </summary>
        protected abstract void DoClosing();
    }
}