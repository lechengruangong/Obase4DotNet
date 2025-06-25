/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：读取器的基础集合中找不到指定基键时引发的异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:36:48
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     在读取器的基础集合中找不到指定基键时引发的异常
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="T">值的类型</typeparam>
    public class KeyNotFoundException<TKey, T> : Exception
    {
        /// <summary>
        ///     缺失的基键。
        /// </summary>
        private readonly TKey _key;

        /// <summary>
        ///     引发该异常的键序基读取器。
        /// </summary>
        private readonly IKeySequenceBasedReader<TKey, T> _reader;

        /// <summary>
        ///     创建KeyNotFoundException异常。
        /// </summary>
        /// <param name="key">缺失的基键。</param>
        /// <param name="reader">引发该异常的键序基读取器。</param>
        public KeyNotFoundException(TKey key, IKeySequenceBasedReader<TKey, T> reader)
        {
            _key = key;
            _reader = reader;
        }

        /// <summary>
        ///     获取缺失的基键。
        /// </summary>
        public TKey Key => _key;

        /// <summary>
        ///     获取引发该异常的键序基读取器。
        /// </summary>
        public IKeySequenceBasedReader<TKey, T> Reader => _reader;
    }
}