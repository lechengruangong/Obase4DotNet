/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：聚合类运算基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:00:48
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     为聚合类运算提供基础实现。
    /// </summary>
    public abstract class AggregateOp : QueryOp
    {
        /// <summary>
        ///     断言函数，用于判定元素是否参与聚合。
        /// </summary>
        private readonly LambdaExpression _predicate;

        /// <summary>
        ///     创建AggregateOp实例。
        /// </summary>
        /// <param name="name">运算名。</param>
        /// <param name="predicate">断言函数，用于判定元素是否参与聚合。</param>
        /// <param name="model">模型</param>
        /// <param name="type">源类型</param>
        protected AggregateOp(EQueryOpName name, LambdaExpression predicate, ObjectDataModel model, Type type)
            : base(name, type)
        {
            _predicate = predicate;
            _model = model;
        }

        /// <summary>
        ///     创建AggregateOp实例。
        /// </summary>
        /// <param name="name">运算名称。</param>
        /// <param name="type">源类型。</param>
        protected AggregateOp(EQueryOpName name, Type type)
            : base(name, type)
        {
        }

        /// <summary>
        ///     获取断言函数，该函数用于判定元素是否参与聚合。
        /// </summary>
        public LambdaExpression Predicate => _predicate;
    }
}