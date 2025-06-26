/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示All运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:09:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示All运算。
    /// </summary>
    public class AllOp : CriteriaContainOp
    {
        /// <summary>
        ///     创建AllOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="model"></param>
        internal AllOp(LambdaExpression predicate, ObjectDataModel model)
            : base(EQueryOpName.All, predicate, model)
        {
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => typeof(bool);
    }
}