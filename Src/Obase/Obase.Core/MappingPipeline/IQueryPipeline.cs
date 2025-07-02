/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询管道接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:13:47
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     查询管道接口。
    /// </summary>
    public interface IQueryPipeline
    {
        /// <summary>
        ///     为PreExecuteSql事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<QueryEventArgs> PreExecuteCommand;

        /// <summary>
        ///     为PostExecuteSql事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<QueryEventArgs> PostExecuteCommand;

        /// <summary>
        ///     为BeginQuery事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<QueryEventArgs> BeginQuery;

        /// <summary>
        ///     为EndQuery事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<QueryEventArgs> EndQuery;
    }
}