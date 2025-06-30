/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Update语句未更新任何记录时引发的异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:03:49
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     表示Update语句未更新任何记录时引发的异常。
    /// </summary>
    public class NothingUpdatedException : Exception
    {
        /// <summary>
        ///     创建NothingUpdatedException实例。
        /// </summary>
        public NothingUpdatedException() : base("未更新任何记录。")
        {
        }
    }
}