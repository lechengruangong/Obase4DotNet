/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Join运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 12:00:26
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Join运算。
    /// </summary>
    public class JoinOp : QueryOp
    {
        /// <summary>
        ///     相等比较器，用于测试来自两个元素的联接鍵是否相等。
        /// </summary>
        private readonly IEqualityComparer _comparer;

        /// <summary>
        ///     联接鍵函数，用于从第二个序列的每个元素提取联接鍵。
        /// </summary>
        private readonly LambdaExpression _innerKeySelector;

        /// <summary>
        ///     获取要与第一个序列联接的序列。
        /// </summary>
        private readonly IEnumerable _innerSource;

        /// <summary>
        ///     联接鍵函数，用于从第一个序列的每个元素提取联接鍵。
        /// </summary>
        private readonly LambdaExpression _outerKeySelector;

        /// <summary>
        ///     结果投影函数，用于从两个匹配元素创建结果元素。
        /// </summary>
        private readonly LambdaExpression _resultSelector;

        /// <summary>
        ///     创建JoinOp实例。
        /// </summary>
        /// <param name="innerSource">要与第一个序列联接的序列。</param>
        /// <param name="outerKeySelector">联接鍵函数，用于从第一个序列的每个元素提取联接鍵。</param>
        /// <param name="innerKeySelector">联接鍵函数，用于从第二个序列的每个元素提取联接鍵。</param>
        /// <param name="resultSelector">结果投影函数，用于从两个匹配元素创建结果元素。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="comparer">相等比较器，用于测试来自两个元素的联接鍵是否相等。</param>
        /// 实施说明:
        /// outerKeySelector的第一个形参的类型为查询源类型。
        internal JoinOp(IEnumerable innerSource, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector,
            LambdaExpression resultSelector, ObjectDataModel model, IEqualityComparer comparer = null)
            : base(EQueryOpName.Join, outerKeySelector.Parameters[0].Type)
        {
            _innerSource = innerSource;
            _outerKeySelector = outerKeySelector;
            _innerKeySelector = innerKeySelector;
            _resultSelector = resultSelector;
            _comparer = comparer;
            _model = model;
        }

        /// <summary>
        ///     获取相等比较器，该比较器用于测试来自两个元素的联接鍵是否相等。
        /// </summary>
        public IEqualityComparer Comparer => _comparer;

        /// <summary>
        ///     获取一个联接鍵函数，该函数用于从第二个序列的每个元素提取联接鍵。
        /// </summary>
        public LambdaExpression InnerKeySelector => _innerKeySelector;

        /// <summary>
        ///     获取第二个序列联接鍵的类型。
        /// </summary>
        public Type InnerKeyType => _innerKeySelector.ReturnType;

        /// <summary>
        ///     要与第一个序列联接的序列。
        /// </summary>
        public IEnumerable InnerSource => _innerSource;

        /// <summary>
        ///     获取第二个序列元素的类型。
        /// </summary>
        public Type InnerType => _innerSource.GetType().GetGenericArguments()[0];

        /// <summary>
        ///     获取一个值，该值指示是否对第二个序列按其联接鍵分组、以组为单位与第一个序列联接。
        ///     实施说明
        ///     如果结果投影函数的第二个形参为IEnumerable，则返回true。
        /// </summary>
        public bool IsGrouping => _resultSelector.Parameters[1].Type.GetInterface("IEnumerable") != null;

        /// <summary>
        ///     获取一个联接鍵函数，该函数用于从第一个序列的每个元素提取联接鍵。
        /// </summary>
        public LambdaExpression OuterKeySelector => _outerKeySelector;

        /// <summary>
        ///     获取第一个序列联接鍵的类型。
        /// </summary>
        public Type OuterKeyType => _outerKeySelector.ReturnType;

        /// <summary>
        ///     获取第一个序列元素的类型。
        /// </summary>
        public Type OuterType => OuterKeySelector.Parameters[0].Type;

        /// <summary>
        ///     获取结果投影函数，该函数用于从两个匹配元素创建结果元素。
        /// </summary>
        public LambdaExpression ResultSelector => _resultSelector;

        /// <summary>
        ///     获取结果序列元素的类型。
        /// </summary>
        public override Type ResultType => _resultSelector.ReturnType;
    }
}