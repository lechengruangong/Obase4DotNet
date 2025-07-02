/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举形参指代.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:28:02
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     枚举形参指代 表明该形参指代的内容
    /// </summary>
    public enum EParameterReferring : byte
    {
        /// <summary>
        ///     查询源中的单个对象或值。
        /// </summary>
        Single,

        /// <summary>
        ///     查询源序列。
        /// </summary>
        Sequence,

        /// <summary>
        ///     查询源中对象或值的索引号。
        /// </summary>
        Index
    }
}