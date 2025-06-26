/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：测试序列元素是否满足指定条件的运算基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:05:48
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     为测试序列元素是否满足指定条件的运算提供基础实现。
    /// </summary>
    public abstract class CriteriaContainOp : QueryOp
    {
        /// <summary>
        ///     断言函数，用于测试元素是否满足条件。
        /// </summary>
        private readonly LambdaExpression _predicate;

        /// <summary>
        ///     创建CriteriaContainOp实例。
        /// </summary>
        /// <param name="name">运算名称。</param>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="model"></param>
        protected CriteriaContainOp(EQueryOpName name, LambdaExpression predicate, ObjectDataModel model)
            : base(name, predicate.Parameters[0].Type)
        {
            _predicate = predicate;
            _model = model;
        }

        /// <summary>
        ///     创建CriteriaContainOp实例
        /// </summary>
        /// <param name="name">运算名称</param>
        /// <param name="type">源类型</param>
        protected CriteriaContainOp(EQueryOpName name, Type type) : base(name, type)
        {
            _predicate = null;
        }

        /// <summary>
        ///     获取断言函数，该函数用于测试元素是否满足条件。
        /// </summary>
        public LambdaExpression Predicate => _predicate;
    }
}