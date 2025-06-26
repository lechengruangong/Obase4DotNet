/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：“保存”管道接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:16:51
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     “保存”管道接口。
    /// </summary>
    public interface ISavingPipeline
    {
        /// <summary>
        ///     为PreExecuteSql事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<PreExecuteCommandEventArgs> PreExecuteCommand;

        /// <summary>
        ///     为PostExecuteSql事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<PostExecuteCommandEventArgs> PostExecuteCommand;

        /// <summary>
        ///     为BeginSaving事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler BeginSaving;

        /// <summary>
        ///     为PostGenerateQueue事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler PostGenerateQueue;

        /// <summary>
        ///     为BeginSavingUnit事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<BeginSavingUnitEventArgs> BeginSavingUnit;

        /// <summary>
        ///     为EndSavingUnit事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<EndSavingUnitEventArgs> EndSavingUnit;

        /// <summary>
        ///     为EndSaving事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler EndSaving;
    }
}