/*
┌──────────────────────────────────────────────────────────────┐
│　描   述："删除"管道接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:05:20
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     "删除"管道接口。
    /// </summary>
    public interface IDeletingPipeline
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
        ///     为BeginDeleting事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler BeginDeleting;

        /// <summary>
        ///     为PostGenerateGroup事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler PostGenerateGroup;

        /// <summary>
        ///     为BeginDeletingGroup事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<BeginDeletingGroupEventArgs> BeginDeletingGroup;

        /// <summary>
        ///     为EndDeletingGroup事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<EndDeletingGroupEventArgs> EndDeletingGroup;

        /// <summary>
        ///     为EndDeleting事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler EndDeleting;
    }
}