/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Where运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:53:47
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Where运算。
    /// </summary>
    public class WhereOp : FilterOp
    {
        /// <summary>
        ///     分解出来的或因子
        /// </summary>
        private OrFactor[] _orFactors;

        /// <summary>
        ///     创建WhereOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试每个元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        internal WhereOp(LambdaExpression predicate, ObjectDataModel model)
            : base(EQueryOpName.Where, predicate, model)
        {
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;


        /// <summary>
        ///     执行或因子分解。
        ///     实施说明
        ///     寄存分解结果，避免重复分解。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        public OrFactor[] Decompose(ObjectDataModel model)
        {
            //没有断言函数 不分解
            if (Predicate == null)
                return null;

            if (_orFactors == null)
            {
                var referringType = model.GetReferringType(SourceType);
                var flattener = new CriteriaFlattener(referringType, Predicate.Parameters[0]);
                //直接访问内容 参数绑定已传入
                _orFactors = flattener.Flatt(Predicate.Body);
            }

            return _orFactors;
        }

        /// <summary>
        ///     由基类重写 获取表达式参数
        /// </summary>
        /// <returns></returns>
        protected override Expression[] GetArguments()
        {
            if (Predicate == null)
                return Array.Empty<Expression>();
            //抽取成员表达式
            var member = new MemberExpressionExtractor(new SubTreeEvaluator(Predicate)).ExtractMember(Predicate)
                .Distinct().ToArray();
            var result = new List<Expression>(member);
            return result.ToArray();
        }

        /// <summary>
        ///     作为一个表达式访问者对表达式表示的筛选条件实施平展。
        /// </summary>
        private class CriteriaFlattener : ExpressionVisitor
        {
            /// <summary>
            ///     主引类型
            /// </summary>
            private readonly ReferringType _referringType;

            /// <summary>
            ///     在函数中的
            /// </summary>
            private readonly ParameterExpression _sourceParameter;


            /// <summary>
            ///     存放当前层级分解出来的或因子
            /// </summary>
            private readonly Stack<OrFactor> _tempFactors = new Stack<OrFactor>();

            /// <summary>
            ///     构造一个条件平展器 将判断函数条件进行平展
            /// </summary>
            /// <param name="referringType">注音类型</param>
            /// <param name="sourceParameter">源表达式</param>
            public CriteriaFlattener(ReferringType referringType, ParameterExpression sourceParameter)
            {
                _referringType = referringType;
                _sourceParameter = sourceParameter;
            }

            /// <summary>
            ///     平展方法
            /// </summary>
            /// <returns></returns>
            public OrFactor[] Flatt(Expression expression)
            {
                expression.Accept(this);
                return _tempFactors.Reverse().ToArray();
            }

            /// <summary>
            ///     是否是逻辑运算
            /// </summary>
            /// <param name="type">表达式类型</param>
            /// <returns></returns>
            private bool IsLogicOp(ExpressionType type)
            {
                return type == ExpressionType.Not || type == ExpressionType.AndAlso || type == ExpressionType.OrElse;
            }

            /// <summary>
            ///     将被访问的表达式调度到访问特定类型节点的方法。
            /// </summary>
            /// <param name="expression">表达式</param>
            public override Expression Visit(Expression expression)
            {
                var nodeType = expression.NodeType;

                //是逻辑运算 且返回值是布尔值
                if (IsLogicOp(nodeType) && expression.Type == typeof(bool))
                    return nodeType == ExpressionType.Not
                        ? VisitUnary((UnaryExpression)expression)
                        : VisitBinary((BinaryExpression)expression);

                //不是一元或二元表达式 直接放入临时栈
                _tempFactors.Push(new OrFactor(new[] { expression }, _referringType, _sourceParameter));
                return expression;
            }

            /// <summary>
            ///     访问二元表达式。
            /// </summary>
            /// <param name="expression">被访问的表达式。</param>
            protected override Expression VisitBinary(BinaryExpression expression)
            {
                Visit(expression.Left);
                Visit(expression.Right);

                //是与 组合
                if (expression.NodeType == ExpressionType.AndAlso)
                {
                    var left = _tempFactors.Pop();
                    var right = _tempFactors.Pop();
                    var result = left.And(right);
                    _tempFactors.Push(result);
                }

                //是或 无需处理
                if (expression.NodeType == ExpressionType.OrElse)
                {
                    //ignore
                }

                return expression;
            }

            /// <summary>
            ///     访问一元表达式。
            /// </summary>
            /// <param name="expression">被访问的表达式。</param>
            protected override Expression VisitUnary(UnaryExpression expression)
            {
                //处理非运算 变为挨个表达式取反 翻转与或
                if (expression.NodeType == ExpressionType.Not)
                {
                    var current = _tempFactors.Count;
                    Visit(expression.Operand);
                    current = _tempFactors.Count - current;

                    //存放需要改为与的表达式
                    var tempList = new List<OrFactor>();
                    for (var i = 0; i < current; i++)
                    {
                        var needRevers = _tempFactors.Pop();
                        foreach (var exp in needRevers.Items)
                            tempList.Add(new OrFactor(new Expression[] { Expression.Not(exp) }, _referringType,
                                _sourceParameter));
                    }

                    //求最终与
                    var result = tempList.First();
                    for (var i = 1; i < tempList.Count; i++) result.And(tempList[i]);
                    _tempFactors.Push(result);
                }
                else
                {
                    _tempFactors.Push(new OrFactor(new Expression[] { expression }, _referringType, _sourceParameter));
                }

                return expression;
            }
        }
    }
}