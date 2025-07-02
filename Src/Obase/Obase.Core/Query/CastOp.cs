/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Cast运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:30:16
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Cast运算。
    /// </summary>
    public class CastOp : QueryOp
    {
        /// <summary>
        ///     转换目标类型。
        /// </summary>
        private readonly Type _resultType;

        /// <summary>
        ///     创建CastOp实例。
        /// </summary>
        /// <param name="resultType">转换目标类型。</param>
        /// <param name="sourceType">查询源类型。</param>
        internal CastOp(Type resultType, Type sourceType)
            : base(EQueryOpName.Cast, sourceType)
        {
            _resultType = resultType;
        }

        /// <summary>
        ///     获取转换目标类型。
        /// </summary>
        public override Type ResultType => _resultType;
    }
}