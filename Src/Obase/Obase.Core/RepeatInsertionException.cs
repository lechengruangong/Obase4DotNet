/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：重复插入异常,当插入相同主键的记录时引发此异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:47:00
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core
{
    /// <summary>
    ///     表示重复插入异常。当插入相同主键的记录时引发此异常。
    /// </summary>
    public class RepeatInsertionException : Exception
    {
        /// <summary>
        ///     当前数据源是否不支持此异常的处理模式
        /// </summary>
        private readonly bool _isUnSupported;

        /// <summary>
        ///     不支持的原因
        /// </summary>
        private string _unSupportMessage;

        /// <summary>
        ///     创建RepeatInsertionException实例。
        /// </summary>
        public RepeatInsertionException(bool isUnSupported) : base("插入了重复的记录。")
        {
            _isUnSupported = isUnSupported;
        }

        /// <summary>
        ///     当前数据源是否不支持此异常的处理模式
        /// </summary>
        public bool IsUnSupported => _isUnSupported;

        /// <summary>
        ///     不支持的原因
        /// </summary>
        public string UnSupportMessage
        {
            get => _unSupportMessage;
            set => _unSupportMessage = value;
        }
    }
}