/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示无查询参数的查询.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:00:48
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示无查询参数的查询
    /// </summary>
    public class EveryOp : QueryOp
    {
        /// <summary>
        ///     创建QueryOp的新实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="model">模型</param>
        internal EveryOp(Type sourceType, ObjectDataModel model) : base(EQueryOpName.Non, sourceType)
        {
            _model = model;
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}