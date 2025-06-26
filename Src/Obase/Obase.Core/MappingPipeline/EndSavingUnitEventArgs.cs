/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结束保存单元事件数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:01:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Saving;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     结束保存单元事件数据类
    /// </summary>
    public class EndSavingUnitEventArgs : MappingUnitEventArgs
    {
        /// <summary>
        ///     保存过程中发生的异常，如果执行成功则值为NULL。
        /// </summary>
        private readonly Exception _exception;


        /// <summary>
        ///     创建EndSavingUnitEventArgs实例，并指定尝试保存的映射单元和执行过程中发生的异常。
        /// </summary>
        /// <param name="mappingUnit">要保存的映射单元。</param>
        /// <param name="hostObjectStatus">映射单元主对象状态</param>
        /// <param name="exception">异常。</param>
        public EndSavingUnitEventArgs(MappingUnit mappingUnit, EObjectStatus hostObjectStatus,
            Exception exception = null) : base(mappingUnit, hostObjectStatus)
        {
            _exception = exception;
        }

        /// <summary>
        ///     获取保存过程中发生的异常，如果执行成功则值为NULL。
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        ///     获取一个值，该值指示保存操作是否执行失败。
        /// </summary>
        public bool Failed => _exception != null;
    }
}