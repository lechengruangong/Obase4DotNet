/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构查询分段执行器规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:12:16
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构查询分段执行器规范，提供执行异构查询分解所得片段的方案，执行该方案所得结果即为异构查询的结果。
    /// </summary>
    public interface IHeterogQuerySegmentallyExecutor
    {
        /// <summary>
        ///     执行异构查询分解所得的片段。
        /// </summary>
        /// <param name="segments">对异构查询实施分解产生的片段。</param>
        /// <param name="heterogQueryProvider">异构查询提供程序，用于执行从异构运算中分解出的附加查询。</param>
        /// <param name="attachObject">用于将对象附加到对象上下文的委托。</param>
        /// <param name="attachRoot">指示是否附加根对象。</param>
        object Execute(HeterogQuerySegments segments, HeterogQueryProvider heterogQueryProvider,
            AttachObject attachObject, bool attachRoot = true);
    }
}