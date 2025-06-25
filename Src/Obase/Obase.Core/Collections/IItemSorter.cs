/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序器接口,提供按一定策略实施排序的方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:11:46
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Collections
{
    /// <summary>
    ///     排序器，提供按一定策略实施排序的方法。
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public interface IItemSorter<T>
    {
        /// <summary>
        ///     执行排序。
        /// </summary>
        /// <param name="source">源序列。</param>
        /// <param name="rules">排序规则。</param>
        /// <param name="resultSet">结果集。</param>
        void Sort(IForwardReader<T> source, ItemOrder<T> rules, HugeSet<T> resultSet);
    }
}