/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Single运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:48:07
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Single运算。
    /// </summary>
    public class SingleOp : CriteriaContainOp
    {
        /// <summary>
        ///     指示不满足条件时是否返回默认值。
        /// </summary>
        private readonly bool _returnDefault;

        /// <summary>
        ///     创建SingleOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="returnDefault">指示不满足条件时是否返回默认值。</param>
        internal SingleOp(LambdaExpression predicate, ObjectDataModel model, bool returnDefault = false)
            : base(EQueryOpName.Single, predicate, model)
        {
            _returnDefault = returnDefault;
        }

        /// <summary>
        ///     创建SingleOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="returnDefault">指示不满足条件时是否返回默认值。</param>
        internal SingleOp(Type sourceType, bool returnDefault = false)
            : base(EQueryOpName.Single, sourceType)
        {
            _returnDefault = returnDefault;
        }

        /// <summary>
        ///     获取一个值，该值指示不满足条件时是否返回默认值。
        /// </summary>
        public bool ReturnDefault => _returnDefault;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}