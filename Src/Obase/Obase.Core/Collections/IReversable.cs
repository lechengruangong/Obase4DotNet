/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：反序接口,提供反序方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:06:44
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Collections
{
    /// <summary>
    ///     反序接口
    /// </summary>
    public interface IReversable<T>
    {
        /// <summary>
        ///     创建一个从当前巨量集合中反序读取元素的只进读取器。
        /// </summary>
        /// <returns>生成的只进读取器。</returns>
        IForwardReader<T> Reverse();
    }
}