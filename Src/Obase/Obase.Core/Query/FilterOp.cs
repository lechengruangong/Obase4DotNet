/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于断言函数对元素进行筛选的运算基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:51:04
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     为基于断言函数对元素进行筛选的运算提供基础实现。
    /// </summary>
    public abstract class FilterOp : QueryOp
    {
        /// <summary>
        ///     断言函数，用于测试每个元素是否满足条件。不指定表示恒为真，即任何元素都满足条件。
        /// </summary>
        private readonly LambdaExpression _predicate;

        /// <summary>
        ///     指示当未选中任何元素时是否返回默认值。
        /// </summary>
        private readonly bool _returnDefault;

        /// <summary>
        ///     创建FilterOp实例。
        /// </summary>
        /// <param name="name">运算名称。</param>
        /// <param name="predicate">对元素进行筛选的断言函数。</param>
        /// <param name="model"></param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值。</param>
        protected FilterOp(EQueryOpName name, LambdaExpression predicate, ObjectDataModel model,
            bool returnDefault = false)
            : this(name, predicate.Parameters[0].Type, returnDefault)
        {
            _predicate = predicate;
            _model = model;
        }

        /// <summary>
        ///     创建FilterOp实例。
        /// </summary>
        /// <param name="name">运算名称。</param>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值。</param>
        protected FilterOp(EQueryOpName name, Type sourceType, bool returnDefault = false)
            : base(name, sourceType)
        {
            _returnDefault = returnDefault;
        }

        /// <summary>
        ///     获取断言函数，该函数用于测试每个元素是否满足条件。不指定表示恒为真，即任何元素都满足条件。
        /// </summary>
        public LambdaExpression Predicate => _predicate;

        /// <summary>
        ///     获取一个值，该值指示未选中任何元素时是否返回默认值。
        /// </summary>
        public bool ReturnDefault => _returnDefault;

        /// <summary>
        ///     获取一个值该值指示断言函数是否将元素在序列中的索引作为（第二个）参数。
        /// </summary>
        public bool IndexReferred
        {
            get
            {
                //没有断言函数 返回false
                if (_predicate == null)
                    return false;
                //参数不足两个 返回false
                if (_predicate.Parameters.Count < 2)
                    return false;
                //判断第二个参数是不是整数
                return _predicate.Parameters[1].Type == typeof(int) || _predicate.Parameters[1].Type == typeof(short) ||
                       _predicate.Parameters[1].Type == typeof(long);
            }
        }
    }
}