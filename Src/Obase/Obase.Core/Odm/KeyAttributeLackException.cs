/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：缺少键属性异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:20:48
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     缺少键属性异常
    /// </summary>
    public class KeyAttributeLackException : Exception
    {
        /// <summary>
        ///     构造缺少键属性异常实例
        /// </summary>
        /// <param name="message"></param>
        public KeyAttributeLackException(string message) : base(message)
        {
        }
    }
}