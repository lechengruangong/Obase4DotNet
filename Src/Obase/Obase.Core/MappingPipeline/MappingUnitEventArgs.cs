/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：与映射单元相关的事件的数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:47:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Saving;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     与映射单元相关的事件的数据类。
    /// </summary>
    public abstract class MappingUnitEventArgs : EventArgs
    {
        /// <summary>
        ///     映射单元主对象状态
        /// </summary>
        private readonly EObjectStatus _hostObjectStatus;

        /// <summary>
        ///     映射单元。
        /// </summary>
        private readonly MappingUnit _mappingUnit;


        /// <summary>
        ///     创建MappingUnitEventArgs实例，并指定映射单元。
        /// </summary>
        /// <param name="mappingUnit">映射单元。</param>
        /// <param name="hostObjectStatus">映射单元主对象状态</param>
        protected MappingUnitEventArgs(MappingUnit mappingUnit, EObjectStatus hostObjectStatus)
        {
            _mappingUnit = mappingUnit;
            _hostObjectStatus = hostObjectStatus;
        }

        /// <summary>
        ///     获取映射单元。
        /// </summary>
        public MappingUnit MappingUnit => _mappingUnit;

        /// <summary>
        ///     映射单元主对象状态
        /// </summary>
        public EObjectStatus HostObjectStatus => _hostObjectStatus;
    }
}