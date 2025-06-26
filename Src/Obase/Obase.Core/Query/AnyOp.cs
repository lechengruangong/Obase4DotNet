/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Any运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:11:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Any运算。
    /// </summary>
    public class AnyOp : CriteriaContainOp
    {
        /// <summary>
        ///     创建AnyOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="model"></param>
        internal AnyOp(LambdaExpression predicate, ObjectDataModel model)
            : base(EQueryOpName.Any, predicate, model)
        {
        }

        /// <summary>
        ///     创建AnyOp实例
        /// </summary>
        /// <param name="sourceType">查询源类型</param>
        internal AnyOp(Type sourceType) : base(EQueryOpName.Any, sourceType)
        {
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => typeof(bool);
    }
}