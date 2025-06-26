/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：提供发送变更通知的方法接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:03:25
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     提供发送变更通知的方法
    /// </summary>
    public interface IChangeNoticeSender
    {
        /// <summary>
        ///     发送变更通知
        /// </summary>
        /// <param name="notice">变更通知</param>
        void Send(ChangeNotice notice);
    }
}