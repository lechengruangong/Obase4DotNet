/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：作为排序结果的只进阅读器,实现有序的只进读取.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-20 11:37:02
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     作为排序结果的只进阅读器，实现延迟执行机制。
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    public class OrderedReader<T> : IOrderedReader<T>, IReversable<T>, IContains<T>, ICountable
    {
        /// <summary>
        ///     排序器
        /// </summary>
        private readonly IItemSorter<T> _sorter = new DefaultItemSorter<T>();

        /// <summary>
        ///     源序列。
        /// </summary>
        private readonly IForwardReader<T> _source;

        /// <summary>
        ///     固有顺序。
        /// </summary>
        private ItemOrder<T> _connaturalOrder;

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        private T _current;

        /// <summary>
        ///     延迟执行的顺序。
        /// </summary>
        private ItemOrder<T> _delayedOrder;

        /// <summary>
        ///     源序列的HugeSet包装
        /// </summary>
        private HugeSet<T> _hugeSetSource;

        /// <summary>
        ///     是否需要排序
        ///     当被构造未读取前 执行了OrderBy ThenBy重设了排序方式之后 需要在Read时排序
        /// </summary>
        private bool _needToSort = true;

        /// <summary>
        ///     作为排序结果的集合。
        /// </summary>
        private HugeSet<T> _resultSet;

        /// <summary>
        ///     使用已指明固有顺序的源序列、排序规则创建OrderedReader实例，该实例使用默认排序策略延迟执行排序操作。
        /// </summary>
        /// <param name="source">要对其进行排序的源序列。</param>
        /// <param name="connaturalOrder">源序列的固有顺序。</param>
        /// <param name="delayedOrder">待执行的排序规则。</param>
        public OrderedReader(IForwardReader<T> source, ItemOrder<T> connaturalOrder, ItemOrder<T> delayedOrder)
        {
            _source = source;
            _connaturalOrder = connaturalOrder;
            _delayedOrder = delayedOrder;

            //处理HugeSet包装的源序列
            SetHugeSetSource();
        }

        /// <summary>
        ///     使用指定的源序列、排序规则创建OrderedReader实例，该实例使用默认排序策略延迟执行排序操作。
        /// </summary>
        /// <param name="source">要对其进行排序的源序列。</param>
        /// <param name="delayedOrder">待执行的排序规则。</param>
        public OrderedReader(IForwardReader<T> source, ItemOrder<T> delayedOrder)
        {
            _source = source;
            _delayedOrder = delayedOrder;

            //处理HugeSet包装的源序列
            SetHugeSetSource();
        }

        /// <summary>
        ///     使用已指明固有顺序的源序列、排序规则创建OrderedReader实例，该实例使用指定的排序策略延迟执行排序操作。
        /// </summary>
        /// <param name="source">要对其进行排序的源序列。</param>
        /// <param name="connaturalOrder">源序列的固有顺序。</param>
        /// <param name="delayedOrder">待执行的排序规则。</param>
        /// <param name="sorter">按一定策略执行排序操作的排序器。</param>
        public OrderedReader(IForwardReader<T> source, ItemOrder<T> connaturalOrder, ItemOrder<T> delayedOrder,
            IItemSorter<T> sorter)
        {
            _source = source;
            _connaturalOrder = connaturalOrder;
            _delayedOrder = delayedOrder;
            _sorter = sorter;

            //处理HugeSet包装的源序列
            SetHugeSetSource();
        }

        /// <summary>
        ///     使用指定的源序列、排序规则创建OrderedReader实例，该实例使用指定的排序策略延迟执行排序操作。
        /// </summary>
        /// <param name="source">要对其进行排序的源序列。</param>
        /// <param name="delayedOrder">待执行的排序规则。</param>
        /// <param name="sorter">按一定策略执行排序操作的排序器。</param>
        public OrderedReader(IForwardReader<T> source, ItemOrder<T> delayedOrder, IItemSorter<T> sorter)
        {
            _source = source;
            _delayedOrder = delayedOrder;
            _sorter = sorter;
            //处理HugeSet包装的源序列
            SetHugeSetSource();
        }

        /// <summary>
        ///     是否包含元素
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return _hugeSetSource.Contains(item);
        }

        /// <summary>
        ///     获取一个值，该值指示集合或序列是否支持统计元素个数的操作。
        /// </summary>
        public bool CanCount => true;

        /// <summary>
        ///     获取元素个数。
        /// </summary>
        /// <exception cref="NotSupportedException">不支持统计元素个数操作。</exception>
        public long Count => _hugeSetSource.Count;

        /// <summary>
        ///     获取一个值，该值指示“只进”读取器是否可重置。
        /// </summary>
        public bool Resetable => _hugeSetSource.Resetable;

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        public T Current => _current;

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        object IForwardReader.Current => Current;

        /// <summary>
        ///     对其执行排序操作的源序列。
        /// </summary>
        public IForwardReader<T> Source => _hugeSetSource;


        /// <summary>
        ///     对其执行排序操作的源序列。
        /// </summary>
        IForwardReader IOrderedReader.Source => Source;

        /// <summary>
        ///     将读取器向前移动到下一个元素。
        ///     刚被初始化或重置时，读取器位于只进流的起始位置，即第一个元素之前；读取结束后位置流的结束位置，即最后一个元素之后。
        /// </summary>
        /// <returns>如果移动成功返回true，如果已位于最后一个元素返回false。</returns>
        public bool Read()
        {
            //如果需要排序
            if (_needToSort)
                //排序
                DoSort();

            var result = _resultSet.Read();
            _current = _resultSet.Current;
            return result;
        }

        /// <summary>
        ///     关闭读取器。
        /// </summary>
        public void Close()
        {
            _source.Close();
            _hugeSetSource.Close();
            _resultSet?.Close();
        }

        /// <summary>
        ///     OrderReader的OrderBy会重设主排序_connaturalOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader OrderBy(IComparer comparer = null)
        {
            _needToSort = true;
            _connaturalOrder = new ItemOrder<T>();

            return this;
        }

        /// <summary>
        ///     OrderReader的OrderBy会重设主排序_connaturalOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader OrderByDescending(IComparer comparer = null)
        {
            _needToSort = true;
            _connaturalOrder = new ItemOrder<T>(true);

            return this;
        }

        /// <summary>
        ///     将读取器回退到只进流的起始位置（第一个元素之前）。
        ///     如果当前只进流不可重置则引发异常。
        /// </summary>
        /// <exception cref="Exception">当前读取器不支持重置操作。</exception>
        public void Reset()
        {
            _resultSet?.Reset();
        }

        /// <summary>
        ///     将读取器向前移动指定个数的元素。
        /// </summary>
        /// <returns>实际移动数。当流中当前位置之后的元素数少于请求数时，实际移动数将小于请求数。</returns>
        /// <param name="count">提升的元素个数。</param>
        public int Skip(int count)
        {
            if (_needToSort) Sort();
            return _resultSet.Skip(count);
        }

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        public IForwardReader<T> Take(int count)
        {
            if (_needToSort) Sort();
            return _resultSet.Take(count);
        }

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        /// <param name="resultCount">
        ///     实际读取到的元素个数。
        ///     当流中当前位置之后的元素数少于请求数时，实际读取数将小于请求数。
        /// </param>
        public IForwardReader<T> Take(int count, out int resultCount)
        {
            if (_needToSort) Sort();
            return _resultSet.Take(count, out resultCount);
        }

        /// <summary>
        ///     OrderReader的OrderBy会重设主排序_connaturalOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader<T> OrderBy(IComparer<T> comparer = null)
        {
            _needToSort = true;
            _connaturalOrder = new ItemOrder<T>(comparer, false);

            return this;
        }

        /// <summary>
        ///     OrderReader的OrderBy会重设主排序_connaturalOrder
        /// </summary>
        /// <param name="keySelector">键选择器</param>
        /// <returns></returns>
        public IOrderedReader<T> OrderBy<TKey>(Func<T, TKey> keySelector)
        {
            _needToSort = true;
            _connaturalOrder = new ItemOrder<T>(arg => keySelector.Invoke(arg), false);

            return this;
        }

        /// <summary>
        ///     OrderReader的OrderBy会重设主排序_connaturalOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader<T> OrderByDescending(IComparer<T> comparer = null)
        {
            _needToSort = true;
            _connaturalOrder = new ItemOrder<T>(comparer, true);

            return this;
        }

        /// <summary>
        ///     OrderReader的OrderBy会重设主排序_connaturalOrder
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="keySelector">键选择器</param>
        /// <returns></returns>
        public IOrderedReader<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
        {
            _needToSort = true;
            _connaturalOrder = new ItemOrder<T>(arg => keySelector.Invoke(arg), true);

            return this;
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
        ///     调用ThenBy会重设延迟排序_delayedOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader<T> ThenBy(IComparer<T> comparer = null)
        {
            _needToSort = true;
            _delayedOrder = new ItemOrder<T>(comparer, false);

            return this;
        }

        /// <summary>
        ///     调用ThenBy会重设延迟排序_delayedOrder
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="keySelector">键选择器</param>
        /// <returns></returns>
        public IOrderedReader<T> ThenBy<TKey>(Func<T, TKey> keySelector)
        {
            _needToSort = true;
            _delayedOrder = new ItemOrder<T>(arg => keySelector.Invoke(arg), false);

            return this;
        }

        /// <summary>
        ///     调用ThenBy会重设延迟排序_delayedOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader<T> ThenByDescending(IComparer<T> comparer = null)
        {
            _needToSort = true;
            _delayedOrder = new ItemOrder<T>(comparer, true);

            return this;
        }

        /// <summary>
        ///     调用ThenBy会重设延迟排序_delayedOrder
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="keySelector">键选择器</param>
        /// <returns></returns>
        public IOrderedReader<T> ThenByDescending<TKey>(Func<T, TKey> keySelector)
        {
            _needToSort = true;
            _delayedOrder = new ItemOrder<T>(arg => keySelector.Invoke(arg), true);

            return this;
        }


        /// <summary>
        ///     调用ThenBy会重设延迟排序_delayedOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader ThenBy(IComparer comparer = null)
        {
            _needToSort = true;
            _delayedOrder = new ItemOrder<T>();

            return this;
        }

        /// <summary>
        ///     调用ThenBy会重设延迟排序_delayedOrder
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <returns></returns>
        public IOrderedReader ThenByDescending(IComparer comparer = null)
        {
            _needToSort = true;
            _delayedOrder = new ItemOrder<T>(true);

            return this;
        }

        /// <summary>
        ///     反序
        /// </summary>
        /// <returns></returns>
        public IForwardReader<T> Reverse()
        {
            if (_needToSort) Sort();
            return _resultSet.Reverse();
        }

        /// <summary>
        ///     为HugeSet包装的源序列设值
        /// </summary>
        private void SetHugeSetSource()
        {
            if (_source is HugeSet<T> hugeSet)
            {
                _hugeSetSource = hugeSet;
            }
            else
            {
                _hugeSetSource = new HugeSet<T>();
                _hugeSetSource.Append(_source);
            }
        }

        /// <summary>
        ///     排序
        /// </summary>
        private void Sort()
        {
            //准备集合
            _resultSet = new HugeSet<T>();
            _hugeSetSource.Reset();
            //调用排序器
            _sorter.Sort(_hugeSetSource, _connaturalOrder, _resultSet);

            _needToSort = false;
        }

        /// <summary>
        ///     真正执行排序
        /// </summary>
        private void DoSort()
        {
            //准备集合
            _resultSet = new HugeSet<T>();
            _hugeSetSource.Reset();

            //必须要有主序
            if (_connaturalOrder != null)
            {
                if (_hugeSetSource.Count > 65536)
                    MemorySort(_connaturalOrder);
                else
                    MergeHeapSort(_connaturalOrder);

                //有延迟排序 则再排一次
                if (_delayedOrder != null)
                {
                    if (_hugeSetSource.Count > 65536)
                        MemorySort(_delayedOrder);
                    else
                        MergeHeapSort(_delayedOrder);
                }
            }

            _needToSort = false;
        }

        /// <summary>
        ///     内存排序
        /// </summary>
        private void MemorySort(ItemOrder<T> order)
        {
            //构造一个容器
            var list = new List<T>();
            while (_hugeSetSource.Read()) list.Add(_hugeSetSource.Current);
            //排序
            if (order.KeySelector != null)
                list.Sort((x, y) => (int)order.KeySelector.Invoke(x) - (int)order.KeySelector.Invoke(y));
            else if (order.Comparer != null)
                list.Sort(order.Comparer);
            else
                list.Sort(Comparer<T>.Default);
            //处理结果
            if (order.Descending) list.Reverse();
            _resultSet.Append(list);
        }

        /// <summary>
        ///     基于文件的归并堆排序
        /// </summary>
        private void MergeHeapSort(ItemOrder<T> order)
        {
            //构造外存归并排序器
            MergeSortExecutor<T> mergeSorter;
            if (order.KeySelector != null)
                mergeSorter = new MergeSortExecutor<T>(
                    (x, y) => (int)order.KeySelector.Invoke(x) - (int)order.KeySelector.Invoke(y), order.Descending);
            else if (order.Comparer != null)
                mergeSorter = new MergeSortExecutor<T>(order.Comparer, order.Descending);
            else
                mergeSorter = new MergeSortExecutor<T>(Comparer<T>.Default, order.Descending);
            //放入待排序元素
            while (_hugeSetSource.Read()) mergeSorter.PutIn(_hugeSetSource.Current);
            //结束放入
            mergeSorter.EndPutIn();
            //读取结果
            var isSucess = true;
            while (isSucess)
            {
                var result = mergeSorter.TakeOut(out isSucess);
                if (isSucess) _resultSet.Append(result);
            }
        }
    }
}