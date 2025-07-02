/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序规则接口,描述集合中元素的顺序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:10:07
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     排序规则，描述集合中元素的顺序。
    /// </summary>
    /// <typeparam name="T">待排序元素的类型</typeparam>
    public class ItemOrder<T>
    {
        /// <summary>
        ///     排序比较器。
        /// </summary>
        private readonly IComparer<T> _comparer;

        /// <summary>
        ///     指示是否使用降序。
        /// </summary>
        private readonly bool _descending;

        /// <summary>
        ///     用于生成排序键的委托。
        /// </summary>
        private readonly Func<T, object> _keySelector;

        /// <summary>
        ///     从序。
        /// </summary>
        private ItemOrder<T> _subOrder;

        /// <summary>
        ///     使用默认的比较器创建ItemOrder实例，并指示排序时是否使用降序。
        /// </summary>
        /// <param name="descending">是否反序</param>
        public ItemOrder(bool descending = false)
        {
            _descending = descending;
            _comparer = Comparer<T>.Default;
        }

        /// <summary>
        ///     使用指定的比较器创建ItemOrder实例，并指示排序时是否使用降序。
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <param name="descending">是否反序</param>
        public ItemOrder(IComparer<T> comparer, bool descending)
        {
            _comparer = comparer;
            _descending = descending;
        }

        /// <summary>
        ///     使用指定的排序键和默认的比较器创建ItemOrder实例，并指示排序时是否使用降序。
        /// </summary>
        /// <param name="keySelector">键选择器</param>
        /// <param name="descending">是否反序</param>
        public ItemOrder(Expression<Func<T, object>> keySelector, bool descending)
        {
            _keySelector = keySelector.Compile();
            _descending = descending;
        }

        /// <summary>
        ///     获取用于生成排序键的委托。
        /// </summary>
        public Func<T, object> KeySelector => _keySelector;

        /// <summary>
        ///     获取排序比较器。
        /// </summary>
        public IComparer<T> Comparer => _comparer;

        /// <summary>
        ///     获取一个值，该值指示是否使用降序。
        /// </summary>
        public bool Descending => _descending;

        /// <summary>
        ///     获取主序。
        /// </summary>
        public ItemOrder<T> MainOrder => this;

        /// <summary>
        ///     获取或设置从序。
        /// </summary>
        public ItemOrder<T> SubOrder
        {
            get => _subOrder;
            set => _subOrder = value;
        }
    }
}