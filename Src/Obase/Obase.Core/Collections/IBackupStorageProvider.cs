/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：后备存储提供程序接口,提供与后备区相关的方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:15:42
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     后备存储提供程序
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    public interface IBackupStorageProvider<T>
    {
        /// <summary>
        ///     向后备存储区添加元素。
        /// </summary>
        /// <param name="item">元素</param>
        void Append(IEnumerable<T> item);

        /// <summary>
        ///     从后备存储区当前位置读取指定个数的元素。
        /// </summary>
        /// <returns>读取到的元素的集合，未读取到任何元素返回null。当后备存储区中当前位置之后的元素数少于请求数时，实际读取到的元素数会小于请求数。</returns>
        /// <param name="count">要读取的元素个数。</param>
        T[] Read(int count);

        /// <summary>
        ///     将后备存储区中的位置移动到存储区开始处，即第一个元素之前。
        /// </summary>
        void Reset();

        /// <summary>
        ///     从后备存储区当前位置反向读取（从后往前）指定个数的元素。
        /// </summary>
        /// <returns>读取到的元素的集合，未读取到任何元素返回null。当后备存储区中当前位置之前的元素数少于请求数时，实际读取到的元素数会小于请求数。</returns>
        /// <param name="count">要读取的元素个数。</param>
        T[] ReverselyRead(int count);

        /// <summary>
        ///     将后备存储区中的位置移动到存储区末尾，即最后一个元素之后。
        /// </summary>
        void ReverselyReset();

        /// <summary>
        ///     检测后备存储区中是否存在指定的元素。
        /// </summary>
        /// <returns>如果存在返回true，否则返回false。</returns>
        /// <param name="item">元素</param>
        bool Contains(T item);
    }
}