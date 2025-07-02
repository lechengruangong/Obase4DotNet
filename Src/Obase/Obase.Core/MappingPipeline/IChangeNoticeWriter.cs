/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：变更通知编写器接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 09:47:30
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     变更通知编写器接口
    /// </summary>
    public interface IChangeNoticeWriter
    {
        /// <summary>
        ///     编写通知的字符串形式
        /// </summary>
        /// <param name="serializeFunction">序列化方法</param>
        /// <returns></returns>
        string Write(Func<object, string> serializeFunction);

        /// <summary>
        ///     编写通知
        /// </summary>
        /// <returns></returns>
        ChangeNotice Write();
    }
}