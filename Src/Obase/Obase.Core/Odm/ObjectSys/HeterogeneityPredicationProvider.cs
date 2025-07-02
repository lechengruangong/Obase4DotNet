/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树异构断言提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:31:51
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联树异构断言提供程序。
    ///     异构断言是指判定关联树是否为异构的，只要存在一个节点，该节点与根节点某种特性上不相同，则认为该关联树是异构的，或称该关联树为异构关联树。所述“特性”称为“关注特
    ///     性”。
    ///     断言算法的基本思想是，先寄存根节点的关注特性，然后遍历其它节点，如果找到一个节点在关注特性上与根节点不同，则判定关联树为异构的。
    /// </summary>
    public abstract class HeterogeneityPredicationProvider : IEquatable<HeterogeneityPredicationProvider>
    {
        /// <summary>
        ///     判定当前实例与另一个实例是否相等，实现IEquatable的方法。
        /// </summary>
        /// <param name="other">关联树异构断言提供程序</param>
        /// <returns></returns>
        public abstract bool Equals(HeterogeneityPredicationProvider other);

        /// <summary>
        ///     比较当前节点与根节点在关注特性上的异同。
        /// </summary>
        /// <returns>如果相同返回true，否则返回false。</returns>
        /// <param name="currentNode">当前节点。</param>
        public abstract bool Compare(AssociationTreeNode currentNode);

        /// <summary>
        ///     判定当前实例与另一个实例是否相等，重写Object.Equals方法。
        /// </summary>
        /// <returns>
        ///     相等返回true，否则返回false。
        ///     给实施者的说明
        ///     对于异构断言提供程序而言，“相等”的含义是采用了相同的断言算法及参数（如果有），而不应关注是否为同一个提供程序实例。基于此含义，一个可行的实施方案是使用具体提供
        ///     程序类的完全限定名作为判定依据。
        /// </returns>
        /// <param name="other">关联树异构断言提供程序</param>
        public abstract override bool Equals(object other);

        /// <summary>
        ///     返回异构断言提供程序实例的Hash码，重写Object.GetHashCode方法。
        ///     给实施者的说明
        ///     对于异构断言提供程序而言，“相等”的含义是采用了相同的断言算法及参数（如果有），而不应关注是否为同一个提供程序实例。
        ///     因此，应当确保采用同一断言方案及参数的提供程序具有相同的Hash码。一个可行的实施方案是基于具体提供程序类的完全限定名生成Hash码。
        /// </summary>
        public abstract override int GetHashCode();

        /// <summary>
        ///     寄存根节点的关注特性。
        /// </summary>
        /// <param name="rootNode">根节点。</param>
        public abstract void RegisterRoot(AssociationTreeNode rootNode);

        /// <summary>
        ///     重写不等于（!=）运算符。
        /// </summary>
        /// <param name="instance1">第一个操作数。</param>
        /// <param name="instance2">第二个操作数。</param>
        public static bool operator !=(HeterogeneityPredicationProvider instance1,
            HeterogeneityPredicationProvider instance2)
        {
            return !(instance1 == instance2);
        }

        /// <summary>
        ///     重写等于（==）运算符。
        /// </summary>
        /// <param name="instance1">第一个操作数。</param>
        /// <param name="instance2">第二个操作数。</param>
        public static bool operator ==(HeterogeneityPredicationProvider instance1,
            HeterogeneityPredicationProvider instance2)
        {
            if (Equals(instance1, null) && Equals(instance2, null)) return true;
            return !Equals(instance1, null) && instance1.Equals(instance2);
        }
    }
}