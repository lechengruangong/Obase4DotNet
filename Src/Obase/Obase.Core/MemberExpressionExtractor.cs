/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：成员表达式提取器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 15:01:14
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Obase.Core
{
    /// <summary>
    ///     成员表达式提取器。
    /// </summary>
    /// 实施说明:
    /// 在递归过程中，每次实际执行提取算法前先对目标表达式求值，然后从求值结果表达式中提取。
    /// 也就是说，提取算法总是作用于表达式的求值结果，而不是表达式本身。
    /// 参见顺序图“子树求值算法”。
    public class MemberExpressionExtractor : ExpressionVisitor
    {
        /// <summary>
        ///     子树求值器。
        /// </summary>
        private readonly SubTreeEvaluator _subTreeEvaluator;

        /// <summary>
        ///     提取出的成员表达式
        /// </summary>
        private List<MemberExpression> _memberExpressions;

        /// <summary>
        ///     创建MemberExpressionExtractor实例。
        /// </summary>
        /// <param name="subTreeEvaluator">子树求值器。</param>
        public MemberExpressionExtractor(SubTreeEvaluator subTreeEvaluator)
        {
            _subTreeEvaluator = subTreeEvaluator;
        }

        /// <summary>
        ///     获取提取出的成员表达式。
        /// </summary>
        public MemberExpression[] MemberExpressions => _memberExpressions.ToArray();

        /// <summary>
        ///     要指定表达式中提取成员表达式。
        /// </summary>
        /// <param name="expression">要从中提取成员表达式的表达式。</param>
        [Obsolete("之后用表达式访问器重写")]
        public List<MemberExpression> ExtractMember(Expression expression)
        {
            var exp = _subTreeEvaluator.Evaluate(expression);
            _memberExpressions = new List<MemberExpression>();
            var temp = new List<MemberExpression>();
            //目前可以处理以下类型的表达式
            //成员访问 自操作 如+a -a 方法调用 Lambda 新建对象 取反 转换 二元逻辑
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    temp.Add(exp as MemberExpression);
                    break;
                case ExpressionType.Invoke:
                    temp.AddRange(ExtractMember((exp as InvocationExpression)?.Expression));
                    var arguments = (exp as InvocationExpression)?.Arguments;
                    if (arguments != null)
                        foreach (var item in arguments)
                            temp.AddRange(ExtractMember(item));
                    break;
                case ExpressionType.UnaryPlus:
                    temp.AddRange(ExtractMember((exp as UnaryExpression)?.Operand));
                    break;
                case ExpressionType.Call:
                {
                    if (exp is MethodCallExpression methodCallExpression)
                    {
                        if (methodCallExpression.Method.Name == "Contains")
                        {
                            if (methodCallExpression.Object is MemberExpression)
                                temp.AddRange(ExtractMember(methodCallExpression.Object));
                            if (methodCallExpression.Arguments[0] is MemberExpression)
                                temp.AddRange(ExtractMember(methodCallExpression.Arguments[0]));
                        }
                        else
                        {
                            temp.AddRange(ExtractMember(methodCallExpression?.Object ??
                                                        methodCallExpression?.Arguments[0]));
                        }
                    }
                }
                    break;
                case ExpressionType.Lambda:
                    temp.AddRange(ExtractMember((exp as LambdaExpression)?.Body));
                    break;
                case ExpressionType.New:
                    if (exp is NewExpression newEx)
                        foreach (var item in newEx.Arguments)
                            temp.AddRange(ExtractMember(item));
                    break;
                case ExpressionType.Not:
                    if (exp is UnaryExpression unary) temp.AddRange(ExtractMember(unary.Operand));
                    break;
                case ExpressionType.Convert:
                    if (exp is UnaryExpression unaryC) temp.AddRange(ExtractMember(unaryC.Operand));
                    break;
                default:
                {
                    if (exp is BinaryExpression binary)
                    {
                        temp.AddRange(ExtractMember(binary.Left));
                        temp.AddRange(ExtractMember(binary.Right));
                    }

                    break;
                }
            }

            _memberExpressions.AddRange(temp);
            return _memberExpressions.Distinct().ToList();
        }
    }
}