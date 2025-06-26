/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示SkipWhile运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:50:47
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示SkipWhile运算。
    /// </summary>
    public class SkipWhileOp : FilterOp
    {
        /// <summary>
        ///     创建SkipWhileOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试每个元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        internal SkipWhileOp(LambdaExpression predicate, ObjectDataModel model)
            : base(EQueryOpName.SkipWhile, predicate, model)
        {
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}