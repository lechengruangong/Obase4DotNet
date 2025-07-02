/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：最小堆,堆排序中用于表示排序节点的数据结构.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:41:28
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     表示一个最小堆 即一棵完全二叉树 且所有非叶结点的值均不大于其子女的值
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class MinHeap<T>
    {
        /// <summary>
        ///     默认的容量
        /// </summary>
        private const int DefaultCapacity = 5000;

        /// <summary>
        ///     比较器 用于比较堆内元素
        /// </summary>
        private readonly IComparer<T> _comparer;

        /// <summary>
        ///     元素的比较委托
        /// </summary>
        private readonly Comparison<T> _comparison;

        /// <summary>
        ///     堆内当前的元素数量
        /// </summary>
        private int _count;

        /// <summary>
        ///     堆元素集合
        /// </summary>
        private T[] _items;

        /// <summary>
        ///     构造一个最小堆 并指定容量
        /// </summary>
        /// <param name="capacity">最小堆的容量 默认值:5000</param>
        private MinHeap(int capacity)
        {
            if (capacity < 0) throw new IndexOutOfRangeException("堆容量不能小于0.");

            _items = new T[capacity];
        }

        /// <summary>
        ///     构造一个最小堆 并指定 堆内元素比较器 容量
        /// </summary>
        /// <param name="comparer">堆内元素比较器</param>
        /// <param name="capacity">容量</param>
        public MinHeap(IComparer<T> comparer, int capacity = DefaultCapacity) : this(capacity)
        {
            _comparer = comparer;
        }

        /// <summary>
        ///     构造一个最小堆 并指定 堆内元素比较委托 容量
        /// </summary>
        /// <param name="comparison">堆内元素比较委托</param>
        /// <param name="capacity">容量</param>
        public MinHeap(Comparison<T> comparison, int capacity = DefaultCapacity) : this(capacity)
        {
            _comparison = comparison;
        }

        /// <summary>
        ///     堆内当前的元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        ///     增加元素到堆 并从后往前依次对各结点为根的子树进行筛选 直至成为最小堆
        /// </summary>
        /// <param name="value">元素</param>
        /// <returns></returns>
        public bool Enqueue(T value)
        {
            //存储空间已满
            if (_count == _items.Length)
                //扩容至两倍
                ResizeItemStore(_items.Length * 2);

            _items[_count++] = value;
            var position = BubbleUp(_count - 1);

            return position == 0;
        }

        /// <summary>
        ///     从堆内取出元素 并从前往后依次对各结点为根的子树进行筛选 直至成为最小堆
        /// </summary>
        /// <param name="shrink">取出元素后是否尝试收缩空间 默认:是</param>
        /// <returns></returns>
        public T Dequeue(bool shrink = true)
        {
            if (_count == 0) throw new InvalidOperationException("已无元素可取出.");
            var result = _items[0];
            if (_count == 1)
            {
                _count = 0;
                _items[0] = default;
            }
            else
            {
                --_count;
                //取序列最后的元素放在堆顶
                _items[0] = _items[_count];
                _items[_count] = default;
                // 维护堆的结构
                BubbleDown();
            }

            //是非收缩空间
            if (shrink) ShrinkStore();
            return result;
        }

        /// <summary>
        ///     用适当的方式比较两个对象
        /// </summary>
        /// <param name="x">第一个元素</param>
        /// <param name="y">第二个元素</param>
        /// <returns></returns>
        private int Compare(T x, T y)
        {
            int result;
            if (_comparison != null)
                result = _comparison.Invoke(x, y);
            else if (_comparer != null)
                result = _comparer.Compare(x, y);
            else
                result = Comparer<T>.Default.Compare(x, y);

            return result;
        }

        /// <summary>
        ///     从前往后依次对各结点为根的子树进行筛选，使之成为堆，直到序列最后的节点
        /// </summary>
        private void BubbleDown()
        {
            var parent = 0;
            //从根节点开始 找到第一个子节点 为parent * 2 + 1 首次循环固定为2+1
            var leftChild = 2 + 1;
            while (leftChild < _count)
            {
                // 找到子节点中较小的那个
                var rightChild = leftChild + 1;
                var bestChild = rightChild < _count && Compare(_items[rightChild], _items[leftChild]) < 0
                    ? rightChild
                    : leftChild;
                if (Compare(_items[bestChild], _items[parent]) < 0)
                {
                    // 如果子节点小于父节点, 交换子节点和父节点
                    (_items[parent], _items[bestChild]) = (_items[bestChild], _items[parent]);
                    parent = bestChild;
                    leftChild = parent * 2 + 1;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     当元素增加时 重新调整堆内元素存储空间
        /// </summary>
        /// <param name="newSize">新大小</param>
        private void ResizeItemStore(int newSize)
        {
            //不需要扩容 直接返回
            if (_count < newSize || DefaultCapacity <= newSize) return;
            //扩容至指定的大小
            var temp = new T[newSize];
            Array.Copy(_items, 0, temp, 0, _count);
            _items = temp;
        }

        /// <summary>
        ///     收缩存储空间
        /// </summary>
        private void ShrinkStore()
        {
            // 如果容量不足一半以上，默认容量会下降。
            if (_items.Length > DefaultCapacity && _count < _items.Length >> 1)
            {
                var newSize = Math.Max(
                    DefaultCapacity, (_count / DefaultCapacity + 1) * DefaultCapacity);

                ResizeItemStore(newSize);
            }
        }

        /// <summary>
        ///     从后往前依次对各结点为根的子树进行筛选 使之成为堆 直到根结点
        /// </summary>
        /// <param name="startIndex">开始索引</param>
        /// <returns></returns>
        private int BubbleUp(int startIndex)
        {
            while (startIndex > 0)
            {
                //求出父节点Index
                var parent = (startIndex - 1) / 2;
                //如果子节点小于父节点，交换子节点和父节点
                if (Compare(_items[startIndex], _items[parent]) < 0)
                    //交换子节点和父节点
                    (_items[startIndex], _items[parent]) = (_items[parent], _items[startIndex]);
                else
                    break;

                startIndex = parent;
            }

            return startIndex;
        }
    }
}