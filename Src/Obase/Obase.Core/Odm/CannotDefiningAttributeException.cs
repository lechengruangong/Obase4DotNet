/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：无法定义属性异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 09:55:55
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     无法定义属性异常
    /// </summary>
    public class CannotDefiningAttributeException : Exception
    {
        /// <summary>
        ///     构造无法定义属性异常实例
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CannotDefiningAttributeException(string message, Exception innerException) : base(message,
            innerException)
        {
        }
    }
}