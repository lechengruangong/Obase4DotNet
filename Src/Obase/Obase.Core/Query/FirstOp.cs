/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示First运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:52:05
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示First运算。
    /// </summary>
    public class FirstOp : FilterOp
    {
        /// <summary>
        ///     创建FirstOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试每个元素是否满足条件。</param>
        /// <param name="model"></param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值。</param>
        internal FirstOp(LambdaExpression predicate, ObjectDataModel model, bool returnDefault = false)
            : base(EQueryOpName.First, predicate, model, returnDefault)
        {
        }

        /// <summary>
        ///     创建FirstOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值。</param>
        internal FirstOp(Type sourceType, bool returnDefault = false)
            : base(EQueryOpName.First, sourceType, returnDefault)
        {
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}