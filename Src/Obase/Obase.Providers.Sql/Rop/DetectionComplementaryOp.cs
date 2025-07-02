/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：测定类运算的补充运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:18:11
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     测定类运算（AllOp, AnyOp, ContainsOp, SingleOp）的补充运算。
    /// </summary>
    public class DetectionComplementaryOp : QueryOp
    {
        /// <summary>
        ///     被补充的运算。
        /// </summary>
        private readonly QueryOp _complementedOp;

        /// <summary>
        ///     初始化DetectionComplementaryOp的新实例。
        /// </summary>
        /// <param name="complementedOp">被补充的运算。</param>
        /// <param name="model">对象数据模型</param>
        public DetectionComplementaryOp(QueryOp complementedOp, ObjectDataModel model)
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