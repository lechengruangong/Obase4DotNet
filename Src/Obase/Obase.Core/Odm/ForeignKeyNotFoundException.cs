/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：没有找到外键异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:21:20
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     没有找到外键异常
    /// </summary>
    public class ForeignKeyNotFoundException : Exception
    {
        /// <summary>
        ///     构造没有找到外键异常实例
        /// </summary>
        /// <param name="message"></param>
        public ForeignKeyNotFoundException(string message) : base(message)
        {
        }
    }
}
