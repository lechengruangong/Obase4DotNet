/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：“就地修改”管道接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:07:46
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     “就地修改”管道接口。
    ///     “就地修改”是指直接在数据库中修改符合条件的对象，而不是先将对象载入缓存、修改后再写回数据库。包含更改对象属性和删除对象。
    /// </summary>
    public interface IDirectlyChangingPipeline
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
        ///     为BeginDirectlyChanging事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<BeginDirectlyChangingEventArgs> BeginDirectlyChanging;

        /// <summary>
        ///     为EndDirectlyChanging事件附加或移除事件处理程序。
        /// </summary>
        event EventHandler<EndDirectlyChangingEventArgs> EndDirectlyChanging;
    }
}