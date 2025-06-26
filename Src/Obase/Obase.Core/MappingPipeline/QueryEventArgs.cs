/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询事件数据.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:25:19
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     查询事件数据
    /// </summary>
    public class QueryEventArgs : EventArgs
    {
        /// <summary>
        ///     查询上下文。
        /// </summary>
        private readonly QueryContext _context;

        /// <summary>
        ///     构造查询事件数据
        /// </summary>
        /// <param name="context"></param>
        public QueryEventArgs(QueryContext context)
        {
            _context = context;
        }

        /// <summary>
        ///     查询上下文。
        /// </summary>
        public QueryContext Context => _context;
    }
}