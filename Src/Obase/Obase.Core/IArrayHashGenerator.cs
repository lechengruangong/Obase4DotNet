/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标志数组哈希生成器,提供为标识数组生成哈希代码的方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:46:54
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core
{
    /// <summary>
    ///     标志数组哈希生成器
    ///     提供为标识数组生成哈希代码的方法
    /// </summary>
    public interface IArrayHashGenerator
    {
        /// <summary>
        ///     生成哈希代码。
        /// </summary>
        /// <param name="members">标识成员序列。</param>
        int Generator(object[] members);
    }
}