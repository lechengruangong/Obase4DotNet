/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序冒泡不支持异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:27:30
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     排序冒泡不支持异常。引发该异常表明源不支持排序冒泡操作。
    /// </summary>
    public class OrderBubblingUnsuportedException : Exception
    {
        /// <summary>
        ///     要实施排序冒泡的源。
        /// </summary>
        private readonly ISource _source;

        /// <summary>
        ///     构造OrderBubblingUnsuportedException的新实例。
        /// </summary>
        /// <param name="source">要实施顺序冒泡的源。</param>
        public OrderBubblingUnsuportedException(ISource source)
        {
            _source = source;
        }

        /// <summary>
        ///     获取要实施排序冒泡的源。
        /// </summary>
        public ISource OrderSource => _source;

        /// <summary>
        ///     获取异常消息。重写Exception.Message。
        ///     消息格式：[Source.GetType().Name]类型的源不支持排序冒泡。
        /// </summary>
        public override string Message => $"{(Source != null ? Source.GetType().Name : "")}类型的源不支持排序冒泡";
    }
}