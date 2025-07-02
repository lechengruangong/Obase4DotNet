/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示针对表达式树的访问者.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:59:22
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示针对表达式树的访问者。
    /// </summary>
    public abstract class ExpressionVisitor
    {
        /// <summary>
        ///     将要访问的表达式调度到此类中更专用的访问方法之一。
        /// </summary>
        /// <returns>Expression 如果修改了该表达式或任何子表达式，则为修改后的表达式；否则返回原始表达式。</returns>
        /// <param name="expression">要访问的表达式。</param>
        public virtual Expression Visit(Expression expression)
        {
            if (expression == null)
                return null;
            switch (expression.NodeType)
            {
                case EExpressionType.Add:
                case EExpressionType.Subtract:
                case EExpressionType.Multiply:
                case EExpressionType.Divide:
                case EExpressionType.Power:
                case EExpressionType.Equal:
                case EExpressionType.NotEqual:
                case EExpressionType.LessThan:
                case EExpressionType.LessThanOrEqual:
                case EExpressionType.GreaterThan:
                case EExpressionType.GreaterThanOrEqual:
                case EExpressionType.Like:
                case EExpressionType.In:
                case EExpressionType.NotIn:
                case EExpressionType.AndAlso:
                case EExpressionType.BitAnd:
                case EExpressionType.BitOr:
                case EExpressionType.BitXor:
                case EExpressionType.OrElse:
                case EExpressionType.LeftShift:
                case EExpressionType.RightShift:
                    return VisitBinary(expression as BinaryExpression);
                case EExpressionType.BitNot:
                case EExpressionType.Not:
                    return VisitNot(expression as UnaryExpression);
                case EExpressionType.Function:
                    return VisitFunction(expression as FunctionExpression);
                case EExpressionType.Constant:
                    return VisitConstant(expression as ConstantExpression);
                case EExpressionType.Field:
                    return VisitField(expression as FieldExpression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(expression.NodeType),
                        $"未知的表达式类型{expression.NodeType}.");
            }
        }

        /// <summary>
        ///     访问常量表达式。
        /// </summary>
        /// <returns>Expression 如果修改了该表达式或任何子表达式，则为修改后的表达式；否则返回原始表达式。</returns>
        /// <param name="constant">要访问的表达式。</param>
        protected virtual Expression VisitConstant(ConstantExpression constant)
        {
            return constant;
        }

        /// <summary>
        ///     访问字段表达式。
        /// </summary>
        /// <returns>Expression 如果修改了该表达式或任何子表达式，则为修改后的表达式；否则返回原始表达式。</returns>
        /// <param name="field">要访问的表达式。</param>
        protected virtual Expression VisitField(FieldExpression field)
        {
            return field;
        }

        /// <summary>
        ///     访问逻辑取反表达式。
        /// </summary>
        /// <returns>Expression 如果修改了该表达式或任何子表达式，则为修改后的表达式；否则返回原始表达式。</returns>
        /// <param name="not">要访问的表达式。</param>
        protected virtual Expression VisitNot(UnaryExpression not)
        {
            var exp = Visit(not.Operand);
            if (exp != not.Operand)
                return Expression.Not(exp);
            return not;
        }

        /// <summary>
        ///     访问函数表达式。
        /// </summary>
        /// <returns>Expression 如果修改了该表达式或任何子表达式，则为修改后的表达式；否则返回原始表达式。</returns>
        /// <param name="func">要访问的表达式。</param>
        protected virtual Expression VisitFunction(FunctionExpression func)
        {
            var isModify = false;
            var arguments = new Expression[func.Arguments.Length];
            for (var i = 0; i < func.Arguments.Length; i++)
            {
                var arg = Visit(func.Arguments[i]);
                if (arg != func.Arguments[i])
                    isModify = true;
                arguments[i] = arg;
            }

            if (isModify) return Expression.Function(func.FunctionName, arguments);
            return func;
        }

        /// <summary>
        ///     访问二元表达式。
        /// </summary>
        /// <returns>Expression 如果修改了该表达式或任何子表达式，则为修改后的表达式；否则返回原始表达式。</returns>
        /// <param name="binary">要访问的表达式。</param>
        protected virtual Expression VisitBinary(BinaryExpression binary)
        {
            var left = Visit(binary?.Left);
            var right = Visit(binary?.Right);
            if (left != binary?.Left || right != binary?.Right)
                switch (binary?.NodeType)
                {
                    case EExpressionType.Add:
                        return Expression.Add(left, right);
                    case EExpressionType.Subtract:
                        return Expression.Subtract(left, right);
                    case EExpressionType.Multiply:
                        return Expression.Multiply(left, right);
                    case EExpressionType.Divide:
                        return Expression.Devide(left, right);
                    case EExpressionType.Power:
                        return Expression.Power(left, right);
                    case EExpressionType.Equal:
                        return Expression.Equal(left, right);
                    case EExpressionType.NotEqual:
                        return Expression.NotEqual(left, right);
                    case EExpressionType.LessThan:
                        return Expression.LessThan(left, right);
                    case EExpressionType.LessThanOrEqual:
                        return Expression.LessThanOrEqual(left, right);
                    case EExpressionType.GreaterThan:
                        return Expression.GreaterThan(left, right);
                    case EExpressionType.GreaterThanOrEqual:
                        return Expression.GreaterThanOrEqual(left, right);
                    case EExpressionType.AndAlso:
                        return Expression.AndAlse(left, right);
                    case EExpressionType.OrElse:
                        return Expression.OrElse(left, right);
                    case EExpressionType.Like:
                        return Expression.Like(left, ((LikeExpression)binary).Pattern);
                    case EExpressionType.In:
                        return Expression.In(left, ((InExpression)binary).ValueSet);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(binary.NodeType),
                            $"未知的二元表达式类型{binary?.NodeType}.");
                }

            return binary;
        }
    }
}