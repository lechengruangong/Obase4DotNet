/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：选择类运算的补充运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:25:43
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     选择类运算（FirstOp, LastOp）的补充运算。
    /// </summary>
    public class FilteringComplementaryOp : QueryOp
    {
        /// <summary>
        ///     被补充的运算。
        /// </summary>
        private readonly QueryOp _complementedOp;

        /// <summary>
        ///     初始化FilteringComplementaryOp的新实例。
        /// </summary>
        /// <param name="complementedOp">被补充的运算。</param>
        /// <param name="model"></param>
        public FilteringComplementaryOp(QueryOp complementedOp, ObjectDataModel model)
            : base(complementedOp.Name, complementedOp.SourceType)
        {
            _complementedOp = complementedOp;
            _model = model;
        }

        /// <summary>
        ///     被补充的运算。
        /// </summary>
        public QueryOp ComplementedOp => _complementedOp;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => _complementedOp.ResultType;
    }
}