/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：开始保存单元事件数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:49:30
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Saving;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     开始保存单元事件数据类
    /// </summary>
    public class BeginSavingUnitEventArgs : MappingUnitEventArgs
    {
        /// <summary>
        ///     创建BeginSavingUnitEventArgs实例，并指定要保存的映射单元。
        /// </summary>
        /// <param name="mappingUnit">映射单元。</param>
        /// <param name="hostObjectStatus">映射单元主对象状态</param>
        public BeginSavingUnitEventArgs(MappingUnit mappingUnit, EObjectStatus hostObjectStatus) : base(mappingUnit,
            hostObjectStatus)
        {
        }
    }
}