/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的元素排序器,实现排序器接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:14:25
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Collections
{
    /// <summary>
    ///     默认的元素排序器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DefaultItemSorter<T> : IItemSorter<T>
    {
        /// <summary>
        ///     执行排序。
        /// </summary>
        /// <param name="source">源序列。</param>
        /// <param name="rules">排序规则。</param>
        /// <param name="resultSet">结果集。</param>
        public void Sort(IForwardReader<T> source, ItemOrder<T> rules, HugeSet<T> resultSet)
        {
            //是正序还是倒序
            //分别使用只进读取器的排序方法处理 并且追加到结果集中
            if (!rules.Descending)
            {
                var orderedReader = rules.KeySelector != null
                    ? source.OrderBy(rules.Comparer).ThenBy(rules.KeySelector)
                    : source.OrderBy(rules.Comparer);
                resultSet.Append(orderedReader);
            }
            else
            {
                var orderedReader = rules.KeySelector != null
                    ? source.OrderByDescending(rules.Comparer).ThenByDescending(rules.KeySelector)
                    : source.OrderByDescending(rules.Comparer);
                resultSet.Append(orderedReader);
            }
        }
    }
}
