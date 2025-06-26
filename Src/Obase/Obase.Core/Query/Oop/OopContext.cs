/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象运算上下文.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:23:47
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     对象运算上下文。
    /// </summary>
    public class OopContext
    {
        /// <summary>
        ///     查询源对象序列，简称查询源。
        /// </summary>
        private readonly IEnumerable _source;

        /// <summary>
        ///     对象运算结果。
        /// </summary>
        private object _result;

        /// <summary>
        ///     构造OopContext的新实例。
        /// </summary>
        /// <param name="objects">作为查询源的对象序列</param>
        public OopContext(IEnumerable objects)
        {
            _result = objects;
            _source = objects;
        }

        /// <summary>
        ///     构造OopContext的新实例。
        /// </summary>
        /// <param name="knownResult">当前已知的运算结果。</param>
        public OopContext(object knownResult)
        {
            _result = knownResult;
        }

        /// <summary>
        ///     获取查询源对象序列，简称查询源。
        /// </summary>
        public IEnumerable Source => _source;

        /// <summary>
        ///     获取或设置查询结果。
        ///     注：设置结果时，如果结果值为枚举数，则将Source设置为该枚举数以作为下一个运算的源。
        /// </summary>
        public object Result
        {
            get => _result;
            set => _result = value;
        }
    }
}