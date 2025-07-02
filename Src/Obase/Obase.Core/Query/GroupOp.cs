/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Group运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:53:03
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Group运算。
    /// </summary>
    public class GroupOp : QueryOp
    {
        /// <summary>
        ///     相等比较器，用于测试两个分组鍵是否相等。
        /// </summary>
        private readonly IEqualityComparer _comparer;

        /// <summary>
        ///     组元素函数，用于从每个元素提取组元素。
        /// </summary>
        private readonly LambdaExpression _elementSelector;

        /// <summary>
        ///     鍵函数，用于从每个元素提取分组鍵。
        /// </summary>
        private readonly LambdaExpression _keySelector;

        /// <summary>
        ///     创建GroupOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="elementSelector">组元素函数，用于从每个元素提取组元素。</param>
        internal GroupOp(LambdaExpression keySelector, ObjectDataModel model, LambdaExpression elementSelector = null)
            : base(EQueryOpName.Group, keySelector.Parameters[0].Type)
        {
            _keySelector = keySelector;
            _elementSelector = elementSelector;
            _model = model;
        }

        /// <summary>
        ///     创建GroupOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵</param>
        /// <param name="comparer">比较器</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="elementSelector">组元素函数，用于从每个元素提取组元素。</param>
        internal GroupOp(LambdaExpression keySelector, IEqualityComparer comparer, ObjectDataModel model,
            LambdaExpression elementSelector = null)
            : this(keySelector, model, elementSelector)
        {
            _comparer = comparer;
        }

        /// <summary>
        ///     获取组元素函数，该函数用于从每个元素提取组元素。
        /// </summary>
        public LambdaExpression ElementSelector => _elementSelector;

        /// <summary>
        ///     获取组元素类型。
        /// </summary>
        public Type ElementType => ElementSelector?.ReturnType ?? SourceType;

        /// <summary>
        ///     获取鍵函数，该函数用于从每个元素提取分组鍵。
        /// </summary>
        public LambdaExpression KeySelector => _keySelector;

        /// <summary>
        ///     获取分组键类型。
        /// </summary>
        public Type KeyType => KeySelector.ReturnType;

        /// <summary>
        ///     获取用于测试两个分组鍵是否相等比较器，。
        /// </summary>
        public IEqualityComparer Comparer => _comparer;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => typeof(IGrouping<object, object>);

        /// <summary>
        ///     由基类重写 获取表达式参数
        /// </summary>
        /// <returns></returns>
        protected override Expression[] GetArguments()
        {
            var member = new MemberExpressionExtractor(new SubTreeEvaluator(KeySelector)).ExtractMember(KeySelector)
                .Distinct().ToArray();
            var result = new List<Expression>(member);
            if (ElementSelector != null)
            {
                member = new MemberExpressionExtractor(new SubTreeEvaluator(ElementSelector))
                    .ExtractMember(ElementSelector).Distinct().ToArray();
                result.AddRange(member);
            }

            return result.ToArray();
        }
    }
}