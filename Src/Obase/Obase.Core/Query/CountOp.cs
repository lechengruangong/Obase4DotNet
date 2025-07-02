/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Count运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:41:09
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Count运算。
    /// </summary>
    public class CountOp : AggregateOp
    {
        /// <summary>
        ///     创建CountOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于判定元素是否参与计数。</param>
        /// <param name="model">对象数据模型</param>
        internal CountOp(LambdaExpression predicate, ObjectDataModel model)
            : base(EQueryOpName.Count, predicate, model, predicate.Parameters[0].Type)
        {
        }

        /// <summary>
        ///     创建CountOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        internal CountOp(Type sourceType)
            : base(EQueryOpName.Count, sourceType)
        {
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => Predicate.ReturnType;
    }
}