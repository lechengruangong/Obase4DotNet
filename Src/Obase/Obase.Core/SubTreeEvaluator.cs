/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：子树求值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:07:07
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Obase.Core
{
    /// <summary>
    ///     子树求值器，用于对表达式中可求值的子树进行计算求值，以简化表达式。
    ///     表达式树可求值，当且仅当：
    ///     （1）该表达式非参数表达式，即其NodeType不等于Parameter；
    ///     （2）各子树均可求值。
    /// </summary>
    public class SubTreeEvaluator
    {
        /// <summary>
        ///     可求值子树的候选集，构成嵌套关系的两个子树可共存于候选集。
        /// </summary>
        private readonly HashSet<Expression> _candidates;

        /// <summary>
        ///     构造SubTreeEvaluator的新实例。
        /// </summary>
        /// <param name="wholeTree">将要对其各级子树尝试求值的整棵表达式树。</param>
        public SubTreeEvaluator(Expression wholeTree)
        {
            _candidates = new Nominator().Nominate(wholeTree);
        }

        /// <summary>
        ///     尝试对指定的表达式求值。如果该表达式可求值则计算其值，并返回以其结果为值的常量表达式，否则返回表达式本身。
        ///     建议调用方从表达式树根节点开始沿叶子方向逐级尝试求值，这样可做到对构成父子关系的多个表达式一次性完成求值。
        /// </summary>
        /// <param name="subTree">要尝试求值的子树。</param>
        public Expression Evaluate(Expression subTree)
        {
            //静态表达式 无需求值 本身就有值
            if (subTree.NodeType == ExpressionType.Constant) return subTree;
            //已求值过 从候选集返回子树
            if (!_candidates.Contains(subTree)) return subTree;
            //编译 求值 组成静态表达式
            var lambda = Expression.Lambda(subTree);
            var fn = lambda.Compile();
            var value = fn.DynamicInvoke(null);
            return Expression.Constant(value, subTree.Type);
        }

        /// <summary>
        ///     可求值表达式提取者，负责对指定表达式树的各级子树进行评估，将可求值子树提名为候选者。
        /// </summary>
        private class Nominator : ExpressionVisitor
        {
            /// <summary>
            ///     候选集
            /// </summary>
            private HashSet<Expression> _candidates;

            /// <summary>
            ///     是否可以被求值
            /// </summary>
            private bool _cannotBeEvaluated;

            /// <summary>
            ///     访问指定的表达式。重写ExpressionVisitor.Visit。
            /// </summary>
            /// <param name="expression">要访问的表达式。</param>
            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    //分别处理各个子树 Quote 和 Parameter 不能求值
                    //如果最终所有子树都不能求值 那么整个表达式不能求值
                    var savedCannotBeEvaluated = _cannotBeEvaluated;
                    _cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (_cannotBeEvaluated)
                    {
                        if (expression.NodeType == ExpressionType.Quote)
                        {
                            _candidates.Add(expression);
                            _cannotBeEvaluated = false;
                        }
                    }
                    else
                    {
                        if (expression.NodeType != ExpressionType.Parameter)
                        {
                            _candidates.Add(expression);
                            _cannotBeEvaluated = false;
                        }
                        else
                        {
                            _cannotBeEvaluated = true;
                        }
                    }

                    //求或
                    _cannotBeEvaluated |= savedCannotBeEvaluated;
                }

                return expression;
            }

            /// <summary>
            ///     从指定的表达式树中提名可求值子树。注：对构造父子关系的多个表达式分别提名。
            /// </summary>
            /// <param name="wholeTree">对其子树进行提名的整棵表达式树。</param>
            public HashSet<Expression> Nominate(Expression wholeTree)
            {
                _candidates = new HashSet<Expression>();
                Visit(wholeTree);
                return _candidates;
            }
        }
    }
}