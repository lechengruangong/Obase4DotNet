/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Skip运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:15:08
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Skip运算。
    /// </summary>
    public class SkipOp : QueryOp
    {
        /// <summary>
        ///     要略过的个数。
        /// </summary>
        private readonly int _count;

        /// <summary>
        ///     创建SkipOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="count">要略过的个数。</param>
        internal SkipOp(Type sourceType, int count)
            : base(EQueryOpName.Skip, sourceType)
        {
            _count = count;
        }

        /// <summary>
        ///     获取要略过的个数。
        /// </summary>
        public int Count => _count;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}