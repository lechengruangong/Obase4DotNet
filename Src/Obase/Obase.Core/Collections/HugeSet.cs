/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：巨量集合,为数据特别庞大的元素集提供存储与访问机制.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:11:07
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     巨量集合，为数据特别庞大的元素集提供存储与访问机制。
    ///     HugeSet提供两个存储区，内存存储区和后备存储区。新加入的元素将被放入内存存储区，该存储区用完后自动将元素转存至后备存储区，并且清空内存存储区以接受新的元素
    ///     HugeSet实现IForwardReader，调用方可以使用此接口访问集合的”只进“流。
    ///     从后备存储区读取元素实现了“预读”机制，一次性从后备存储区批量读取元素放入内存缓冲区，从而减少IO操作。
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    public class HugeSet<T> : ForwardReader<T>, IContains<T>, IReversable<T>, IDisposable
    {
        /// <summary>
        ///     后备存储提供程序
        /// </summary>
        private readonly IBackupStorageProvider<T> _backupStorage;

        /// <summary>
        ///     内存存储区容量。
        /// </summary>
        private readonly int _memoryCapacity;

        /// <summary>
        ///     内存存储区。
        /// </summary>
        private readonly Queue<T> _memorySet;

        /// <summary>
        ///     总个数
        /// </summary>
        private int _count;

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        private T _current;

        /// <summary>
        ///     内存存储区当前元素数。
        /// </summary>
        private int _memoryCount;

        /// <summary>
        ///     读取的索引 从-1开始
        /// </summary>
        private int _readIndex = -1;

        /// <summary>
        ///     使用默认的后备存储提供程序创建HugeSet实例，同时设置内存存储区容量为默认值。
        /// </summary>
        public HugeSet()
        {
            _backupStorage = new FileStorageProvider<T>();
            _memorySet = new Queue<T>();
            _memoryCount = 0;
            _memoryCapacity = 65536;
        }

        /// <summary>
        ///     使用默认的后备存储提供程序创建HugeSet实例，同时指定内存存储区容量。
        /// </summary>
        /// <param name="memoryCapacity">内存存储区容量。</param>
        public HugeSet(int memoryCapacity)
        {
            _backupStorage = new FileStorageProvider<T>();
            _memorySet = new Queue<T>();
            _memoryCount = 0;
            _memoryCapacity = memoryCapacity;
        }

        /// <summary>
        ///     使用指定的后备存储提供程序创建HugeSet实例，同时设置内存存储区容量为默认值。
        /// </summary>
        /// <param name="backupStorage">后备存储提供程序。</param>
        public HugeSet(IBackupStorageProvider<T> backupStorage)
        {
            _memorySet = new Queue<T>();
            _memoryCount = 0;
            _memoryCapacity = 65536;
            _backupStorage = backupStorage;
        }

        /// <summary>
        ///     使用指定的后备存储提供程序创建HugeSet实例，同时指定内存存储区容量。
        /// </summary>
        /// <param name="memoryCapacity">内存存储区容量。</param>
        /// <param name="backupStorage">后备存储提供程序。</param>
        public HugeSet(int memoryCapacity, IBackupStorageProvider<T> backupStorage)
        {
            _memorySet = new Queue<T>();
            _memoryCount = 0;
            _memoryCapacity = memoryCapacity;
            _backupStorage = backupStorage;
        }

        /// <summary>
        ///     获取集合中元素的个数。
        /// </summary>
        public long Count => _count;

        /// <summary>
        ///     获取一个值，该值指示“只进”读取器是否可重置
        /// </summary>
        public override bool Resetable => true;

        /// <summary>
        ///     获取读取器当前位置的元素。
        ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
        /// </summary>
        public override T Current => _current;

        /// <summary>
        ///     是否包含元素
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            if (_memorySet.Contains(item)) return true;

            return _backupStorage.Contains(item);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     创建一个从当前巨量集合中反序读取元素的只进读取器。
        /// </summary>
        /// <returns>生成的只进读取器。</returns>
        public IForwardReader<T> Reverse()
        {
            return new ReverselyReader<T>(this);
        }

        /// <summary>
        ///     向集合添加一个元素。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        public void Append(object item)
        {
            if (item is T t)
                Append(t);
            else
                throw new ArgumentException($"item参数类型不是{typeof(T)}");
        }

        /// <summary>
        ///     向集合添加一个元素。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        public void Append(T item)
        {
            //有余量
            if (_memoryCount < _memoryCapacity)
            {
                _memorySet.Enqueue(item);
                _memoryCount++;
            }
            else
            {
                var backUp = new List<T>();
                while (_memorySet.Count > 0) backUp.Add(_memorySet.Dequeue());
                //存入后备
                _backupStorage.Append(backUp);
                //清空内存区
                _memorySet.Clear();
                _memorySet.Enqueue(item);
                _memoryCount = 1;
            }

            _count++;
        }

        /// <summary>
        ///     向集合批量添加元素。
        /// </summary>
        /// <param name="items">要添加的元素集。</param>
        public void Append(IEnumerable items)
        {
            foreach (var item in items)
                if (item is T t)
                    Append(t);
                else
                    throw new ArgumentException($"item参数类型不是{typeof(T)}");
        }

        /// <summary>
        ///     向集合批量添加元素。
        /// </summary>
        /// <param name="item">要添加的元素集。</param>
        public void Append(IEnumerable<T> item)
        {
            foreach (var i in item) Append(i);
        }

        /// <summary>
        ///     向集合批量添加元素。
        /// </summary>
        /// <param name="item">要添加的元素集。</param>
        public void Append(IForwardReader item)
        {
            //挨个读出来
            while (item.Read()) Append(item.Current);
        }


        /// <summary>
        ///     向集合批量添加元素。
        /// </summary>
        /// <param name="item">要添加的元素集。</param>
        public void Append(IForwardReader<T> item)
        {
            //挨个读出来
            while (item.Read()) Append(item.Current);
        }

        /// <summary>
        ///     释放非托管资源
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            //暂时没有可释放的
        }

        /// <summary>
        ///     将读取器向前移动到下一个元素。
        ///     刚被初始化或重置时，读取器位于只进流的起始位置，即第一个元素之前；读取结束后位置流的结束位置，即最后一个元素之后。
        /// </summary>
        /// <returns>如果移动成功返回true，如果已位于最后一个元素返回false。</returns>
        public override bool Read()
        {
            //内存区无元素 返回false
            if (_memorySet.Count <= 0) return false;
            //当前索引在内存区内
            if (_memorySet.Count > _readIndex + 1)
            {
                _readIndex++;
                _current = _memorySet.ToArray()[_readIndex];
                return true;
            }

            var basckReadResult = _backupStorage.Read(1);
            if (basckReadResult != null)
            {
                //读取成功 移动索引
                _readIndex++;
                _current = basckReadResult[0];
                return true;
            }

            //返回false
            _current = default;
            return false;
        }

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        public override IForwardReader<T> Take(int count)
        {
            var result = new HugeSet<T>();
            //读取指定个数
            var i = 0;
            while (i < count && Read())
            {
                result.Append(Current);
                i++;
            }

            return result;
        }

        /// <summary>
        ///     从当前位置读取指定个数的元素。
        /// </summary>
        /// <param name="count">要读取的元素个数。</param>
        /// <param name="resultCount">
        ///     实际读取到的元素个数。
        ///     当流中当前位置之后的元素数少于请求数时，实际读取数将小于请求数。
        /// </param>
        public override IForwardReader<T> Take(int count, out int resultCount)
        {
            var result = new HugeSet<T>();
            //读取指定个数
            var i = 0;
            while (Read() && i < count)
            {
                result.Append(Current);
                i++;
            }

            //实际读取的数目
            resultCount = i;

            return result;
        }

        /// <summary>
        ///     从集合只进流中预读指定数量的元素放入缓冲区。
        /// </summary>
        /// <returns>读取到的元素的集合，未读到任何元素返回null。当流中当前位置之后的元素数小于请求数时，实际读取到的元素会小于请求数。</returns>
        /// <param name="maxCount"></param>
        protected override T[] Preread(int maxCount)
        {
            //最大可预读数
            if (maxCount > PrereadMax) maxCount = PrereadMax;

            var result = new List<T>();
            var i = 0;
            while (i < maxCount)
                if (Read())
                {
                    //读入缓冲区
                    PrereadItems.Enqueue(_current);
                    //返回值
                    result.Add(_current);
                    i++;
                }
                else
                {
                    break;
                }

            return result.ToArray();
        }

        /// <summary>
        ///     将读取器向前移动指定个数的元素。
        /// </summary>
        /// <returns>实际移动数。当流中当前位置之后的元素数少于请求数时，实际移动数将小于请求数。</returns>
        /// <param name="number"></param>
        protected override int DoSkipping(int number)
        {
            var realNumber = number;
            if (_readIndex + number > _count - 1) realNumber = _count - _readIndex - 1;
            _readIndex += realNumber;
            return realNumber;
        }

        /// <summary>
        ///     将读取器回退到只进流的起始位置（第一个元素之前）。
        ///     在派生类中实现时，如果当前集合的只进流不可重置，则不执行任何操作。
        /// </summary>
        protected override void DoReseting()
        {
            _readIndex = -1;
            _backupStorage.Reset();
        }

        /// <summary>
        ///     关闭读取器。
        /// </summary>
        protected override void DoClosing()
        {
            //暂时没有需要关闭的
        }

        /// <summary>
        ///     使用指定的排序键和默认的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <exception cref="Exception">排序键的类型没有默认比较器。</exception>
        /// <param name="keySelector">用于计算排序键的委托。</param>
        public override IOrderedReader<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
        {
            return new OrderedReader<T>(this, new ItemOrder<T>(arg => keySelector.Invoke(arg), true),
                delayedOrder: null);
        }

        /// <summary>
        ///     使用指定的排序键和默认的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        ///     类型参数
        ///     TKey
        ///     排序键的类型
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        public override IOrderedReader<T> OrderBy(IComparer<T> comparer = null)
        {
            return new OrderedReader<T>(this, new ItemOrder<T>(comparer, false), delayedOrder: null);
        }

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="keySelector">排序比较器。</param>
        public override IOrderedReader<T> OrderBy<TKey>(Func<T, TKey> keySelector)
        {
            return new OrderedReader<T>(this, new ItemOrder<T>(arg => keySelector.Invoke(arg), false),
                delayedOrder: null);
        }

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        public override IOrderedReader<T> OrderByDescending(IComparer<T> comparer = null)
        {
            return new OrderedReader<T>(this, new ItemOrder<T>(comparer, true), delayedOrder: null);
        }

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        public override IOrderedReader OrderByDescending(IComparer comparer)
        {
            return new OrderedReader<T>(this, new ItemOrder<T>(), delayedOrder: null);
        }

        /// <summary>
        ///     使用指定的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
        /// </summary>
        /// <returns>代表排序结果的新的只进读取器。</returns>
        /// <param name="comparer">排序比较器。</param>
        public override IOrderedReader OrderBy(IComparer comparer)
        {
            return new OrderedReader<T>(this, new ItemOrder<T>(true), delayedOrder: null);
        }

        /// <summary>
        ///     巨量集合的反序读取器，用于从后往前读取指定巨量集合的元素。
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        private class ReverselyReader<TItem> : ForwardReader<TItem>, ICountable
        {
            /// <summary>
            ///     用于读取的集合
            /// </summary>
            private readonly HugeSet<TItem> _hugeSet;
            //实施建议：
            //（1）实现预读取机制（以ForwardReader<T> 作为基类）；
            //（2）首先从后备区反序读取元素（ReverseRead方法），然后从内存区反序读取元素。

            /// <summary>
            ///     获取读取器当前位置的元素。
            ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
            /// </summary>
            private TItem _current;

            /// <summary>
            ///     倒序读取索引
            /// </summary>
            private int _reverselyReadIndex;

            /// <summary>
            ///     构造一个巨量集合的反序读取器
            /// </summary>
            /// <param name="hugeSet"></param>
            public ReverselyReader(HugeSet<TItem> hugeSet)
            {
                _hugeSet = hugeSet;
                _hugeSet._backupStorage.ReverselyReset();
                _reverselyReadIndex = _hugeSet._memorySet.Count;
            }

            /// <summary>
            ///     获取一个值，该值指示“只进”读取器是否可重置
            /// </summary>
            public override bool Resetable => _hugeSet.Resetable;

            /// <summary>
            ///     获取读取器当前位置的元素。
            ///     当读取器位于“只进”流的起始位置或结束位置时，返回null。
            /// </summary>
            public override TItem Current => _current;

            /// <summary>
            ///     获取一个值，该值指示集合或序列是否支持统计元素个数的操作。
            /// </summary>
            public bool CanCount => true;

            /// <summary>
            ///     获取元素个数。
            /// </summary>
            /// <exception cref="NotSupportedException">不支持统计元素个数操作。</exception>
            public long Count => _hugeSet.Count;


            /// <summary>
            ///     将读取器向前移动到下一个元素。
            ///     刚被初始化或重置时，读取器位于只进流的起始位置，即第一个元素之前；读取结束后位置流的结束位置，即最后一个元素之后。
            /// </summary>
            /// <returns>如果移动成功返回true，如果已位于最后一个元素返回false。</returns>
            public override bool Read()
            {
                //先从后备区读取
                var backResult = _hugeSet._backupStorage.ReverselyRead(1);
                if (backResult != null)
                {
                    _current = backResult[0];
                    return true;
                }

                //读取内存区
                if (_reverselyReadIndex > 0)
                {
                    _reverselyReadIndex--;
                    _current = _hugeSet._memorySet.ToArray()[_reverselyReadIndex];
                    return true;
                }

                //读完了
                _current = default;
                return false;
            }

            /// <summary>
            ///     从当前位置读取指定个数的元素。
            /// </summary>
            /// <param name="count">要读取的元素个数。</param>
            public override IForwardReader<TItem> Take(int count)
            {
                var result = new HugeSet<TItem>();
                //读取指定个数
                var i = 0;
                while (Read() && i < count)
                {
                    result.Append(Current);
                    i++;
                }

                return result;
            }

            /// <summary>
            ///     从当前位置读取指定个数的元素。
            /// </summary>
            /// <param name="count">要读取的元素个数。</param>
            /// <param name="resultCount">
            ///     实际读取到的元素个数。
            ///     当流中当前位置之后的元素数少于请求数时，实际读取数将小于请求数。
            /// </param>
            public override IForwardReader<TItem> Take(int count, out int resultCount)
            {
                var result = new HugeSet<TItem>();
                //读取指定个数
                var i = 0;
                while (Read() && i < count)
                {
                    result.Append(Current);
                    i++;
                }

                //实际读取的数目
                resultCount = i;

                return result;
            }

            /// <summary>
            ///     从集合只进流中预读指定数量的元素放入缓冲区。
            /// </summary>
            /// <returns>读取到的元素的集合，未读到任何元素返回null。当流中当前位置之后的元素数小于请求数时，实际读取到的元素会小于请求数。</returns>
            /// <param name="maxCount"></param>
            protected override TItem[] Preread(int maxCount)
            {
                //最大可预读数
                if (maxCount > PrereadMax) maxCount = PrereadMax;

                var result = new List<TItem>();
                var i = 0;
                while (i < maxCount)
                    if (Read())
                    {
                        //读入缓冲区
                        PrereadItems.Enqueue(_current);
                        //返回值
                        result.Add(_current);
                        i++;
                    }
                    else
                    {
                        break;
                    }

                return result.ToArray();
            }

            /// <summary>
            ///     将读取器向前移动指定个数的元素。
            /// </summary>
            /// <returns>实际移动数。当流中当前位置之后的元素数少于请求数时，实际移动数将小于请求数。</returns>
            /// <param name="number"></param>
            protected override int DoSkipping(int number)
            {
                var realNumber = number;
                if (_reverselyReadIndex - number > -1) realNumber = _hugeSet._memorySet.Count - _reverselyReadIndex;
                _reverselyReadIndex -= realNumber;
                return realNumber;
            }

            /// <summary>
            ///     将读取器回退到只进流的起始位置（第一个元素之前）。
            ///     在派生类中实现时，如果当前集合的只进流不可重置，则不执行任何操作。
            /// </summary>
            protected override void DoReseting()
            {
                _hugeSet.Reset();
            }

            /// <summary>
            ///     关闭读取器。
            /// </summary>
            protected override void DoClosing()
            {
                //没有需要特殊关闭的
            }

            /// <summary>
            ///     使用指定的排序键和默认的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
            ///     类型参数
            ///     TKey
            ///     排序键的类型
            /// </summary>
            /// <returns>代表排序结果的新的只进读取器。</returns>
            /// <exception cref="Exception">排序键的类型没有默认比较器。</exception>
            /// <param name="keySelector">用于计算排序键的委托。</param>
            public override IOrderedReader<TItem> OrderByDescending<TKey>(Func<TItem, TKey> keySelector)
            {
                return new OrderedReader<TItem>(this, new ItemOrder<TItem>(arg => keySelector.Invoke(arg), true),
                    delayedOrder: null);
            }

            /// <summary>
            ///     使用指定的排序键和默认的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
            ///     类型参数
            ///     TKey
            ///     排序键的类型
            /// </summary>
            /// <returns>代表排序结果的新的只进读取器。</returns>
            public override IOrderedReader<TItem> OrderBy(IComparer<TItem> comparer = null)
            {
                return new OrderedReader<TItem>(this, new ItemOrder<TItem>(comparer, false), delayedOrder: null);
            }

            /// <summary>
            ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
            /// </summary>
            /// <returns>代表排序结果的新的只进读取器。</returns>
            /// <param name="keySelector">排序比较器。</param>
            public override IOrderedReader<TItem> OrderBy<TKey>(Func<TItem, TKey> keySelector)
            {
                return new OrderedReader<TItem>(this, new ItemOrder<TItem>(arg => keySelector.Invoke(arg), false),
                    delayedOrder: null);
            }

            /// <summary>
            ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
            /// </summary>
            /// <returns>代表排序结果的新的只进读取器。</returns>
            /// <param name="comparer">排序比较器。</param>
            public override IOrderedReader<TItem> OrderByDescending(IComparer<TItem> comparer = null)
            {
                return new OrderedReader<TItem>(this, new ItemOrder<TItem>(comparer, true), delayedOrder: null);
            }

            /// <summary>
            ///     使用指定的比较器生成一个代表排序（降序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
            /// </summary>
            /// <returns>代表排序结果的新的只进读取器。</returns>
            /// <param name="comparer">排序比较器。</param>
            public override IOrderedReader OrderByDescending(IComparer comparer)
            {
                return new OrderedReader<TItem>(this, new ItemOrder<TItem>(), delayedOrder: null);
            }

            /// <summary>
            ///     使用指定的比较器生成一个代表排序（升序）结果的只进读取器，实际的排序操作将在结果流被首次读取时执行。
            /// </summary>
            /// <returns>代表排序结果的新的只进读取器。</returns>
            /// <param name="comparer">排序比较器。</param>
            public override IOrderedReader OrderBy(IComparer comparer)
            {
                return new OrderedReader<TItem>(this, new ItemOrder<TItem>(true), delayedOrder: null);
            }
        }
    }
}