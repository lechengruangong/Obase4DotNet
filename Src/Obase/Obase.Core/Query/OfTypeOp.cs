/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示OfType运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 12:08:48
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示OfType运算。
    /// </summary>
    public class OfTypeOp : QueryOp
    {
        /// <summary>
        ///     作为筛选依据的类型。
        /// </summary>
        private readonly Type _resultType;

        /// <summary>
        ///     创建OfType实例。
        /// </summary>
        /// <param name="resultType">作为筛选依据的类型。</param>
        /// <param name="sourceType">查询源类型。</param>
        internal OfTypeOp(Type resultType, Type sourceType)
            : base(EQueryOpName.OfType, sourceType)
        {
            _resultType = resultType;
        }

        /// <summary>
        ///     获取作为筛选依据的类型。
        /// </summary>
        public override Type ResultType => _resultType;
    }
}