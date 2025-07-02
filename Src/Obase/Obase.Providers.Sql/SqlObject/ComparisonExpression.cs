/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示比较运算的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:48:06
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示比较运算的表达式。
    /// </summary>
    public class ComparisonExpression : BinaryExpression
    {
        /// <summary>
        ///     创建ComparisonExpression的实例，并设置Left属性和Right属性的值。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        internal ComparisonExpression(Expression left, Expression right)
            : base(left, right)
        {
        }

        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            var isNull = false;
            var valueStr = string.Empty;
            var constant = Right as ConstantExpression;
            if (constant != null)
            {
                if (constant.Value == null)
                {
                    isNull = true;
                }
                else
                {
                    //MySql中对于True和False不含单引号
                    //SqlServer中True和False必须包含单引号
                    //故分开处理
                    if ((constant.Value is bool && (sourceType == EDataSource.MySql ||
                                                    sourceType == EDataSource.Oracle ||
                                                    sourceType == EDataSource.Sqlite)) ||
                        sourceType == EDataSource.PostgreSql)
                        valueStr = $"{constant.ToString(sourceType)}";
                    else
                        valueStr = $"'{constant.ToString(sourceType)}'";
                }
            }
            else
            {
                var fieId = Right as FieldExpression;
                valueStr = $"({fieId?.ToString(sourceType)})";
            }

            switch (NodeType)
            {
                case EExpressionType.Equal:
                    if (isNull)
                        return $"{Left.ToString(sourceType)} IS NULL";
                    return $"{Left.ToString(sourceType)} = {valueStr}";
                case EExpressionType.NotEqual:
                    if (isNull)
                        return $"{Left.ToString(sourceType)} IS NOT NULL";
                    return $"{Left.ToString(sourceType)} <> {valueStr}";
                case EExpressionType.LessThan:
                    return $"{Left.ToString(sourceType)} < {valueStr}";
                case EExpressionType.LessThanOrEqual:
                    return $"{Left.ToString(sourceType)} <= {valueStr}";
                case EExpressionType.GreaterThan:
                    return $"{Left.ToString(sourceType)} > {valueStr}";
                case EExpressionType.GreaterThanOrEqual:
                    return $"{Left.ToString(sourceType)} >= {valueStr}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的比较运算表达式类型{NodeType}");
            }
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将表达式表示为字符串形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //参数列表
            sqlParameters = new List<IDataParameter>();

            var isLeftNull = false;
            var isRightNull = false;
            //比较表达式可能为空 为空时特殊翻译
            if (Left is ConstantExpression leftConstantExpression && leftConstantExpression.Value == null)
                isLeftNull = true;
            if (Right is ConstantExpression rightConstantExpression && rightConstantExpression.Value == null)
                isRightNull = true;

            //结果 左侧表达式参数
            string result;
            List<IDataParameter> leftSqlParameter;
            List<IDataParameter> rightSqlParameter;

            switch (NodeType)
            {
                case EExpressionType.Equal:
                    if (isLeftNull || isRightNull)
                    {
                        List<IDataParameter> sqlParameter;
                        if (isLeftNull)
                        {
                            result = $"{Right.ToString(sourceType, out sqlParameter, creator)} IS NULL";
                            sqlParameters.AddRange(sqlParameter);
                            break;
                        }

                        result = $"{Left.ToString(sourceType, out sqlParameter, creator)} IS NULL";
                        sqlParameters.AddRange(sqlParameter);
                    }
                    else
                    {
                        //如果是SqlServer数据源，且右侧是bool类型的常量，则需要特殊处理 用NOT代替
                        if (Right is ConstantExpression rightConstant && rightConstant.Value is bool rightBoolValue &&
                            sourceType == EDataSource.SqlServer)
                        {
                            if (Left is LikeExpression)
                            {
                                result =
                                    $"{(rightBoolValue ? "" : " NOT ")}{Left.ToString(sourceType, out leftSqlParameter, creator)}";
                                sqlParameters.AddRange(leftSqlParameter);
                                break;
                            }

                            if (Left is UnaryExpression unaryExpression &&
                                unaryExpression.NodeType == EExpressionType.Not)
                            {
                                result =
                                    $"Not {unaryExpression.Operand.ToString(sourceType, out leftSqlParameter, creator)} = {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                                sqlParameters.AddRange(leftSqlParameter);
                                sqlParameters.AddRange(rightSqlParameter);
                                break;
                            }
                        }

                        //如果是SqlServer数据源，且左侧是bool类型的常量，则需要特殊处理 用NOT代替
                        if (Left is ConstantExpression leftConstant && leftConstant.Value is bool leftBoolValue &&
                            sourceType == EDataSource.SqlServer)
                        {
                            if (Right is LikeExpression)
                            {
                                result =
                                    $"{(leftBoolValue ? "" : " NOT ")}{Right.ToString(sourceType, out rightSqlParameter, creator)}";
                                sqlParameters.AddRange(rightSqlParameter);
                                break;
                            }

                            if (Right is UnaryExpression unaryExpression &&
                                unaryExpression.NodeType == EExpressionType.Not)
                            {
                                result =
                                    $"Not {Left.ToString(sourceType, out leftSqlParameter, creator)} = {unaryExpression.Operand.ToString(sourceType, out rightSqlParameter, creator)}";
                                sqlParameters.AddRange(leftSqlParameter);
                                sqlParameters.AddRange(rightSqlParameter);
                                break;
                            }
                        }

                        result =
                            $"{Left.ToString(sourceType, out leftSqlParameter, creator)} = {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                        sqlParameters.AddRange(leftSqlParameter);
                        sqlParameters.AddRange(rightSqlParameter);
                    }

                    break;
                case EExpressionType.NotEqual:
                    if (isLeftNull || isRightNull)
                    {
                        List<IDataParameter> sqlParameter;
                        if (isLeftNull)
                        {
                            result = $"{Right.ToString(sourceType, out sqlParameter, creator)} IS NOT  NULL";
                            sqlParameters.AddRange(sqlParameter);
                            break;
                        }

                        result = $"{Left.ToString(sourceType, out sqlParameter, creator)} IS NOT NULL";
                        sqlParameters.AddRange(sqlParameter);
                    }
                    else
                    {
                        //如果是SqlServer数据源，且右侧是bool类型的常量，则需要特殊处理 用NOT代替
                        if (Right is ConstantExpression rightConstant && rightConstant.Value is bool rightBoolValue &&
                            sourceType == EDataSource.SqlServer)
                        {
                            if (Left is LikeExpression)
                            {
                                result =
                                    $"{(rightBoolValue ? " NOT " : "")}{Left.ToString(sourceType, out leftSqlParameter, creator)}";
                                sqlParameters.AddRange(leftSqlParameter);
                                break;
                            }

                            if (Left is UnaryExpression unaryExpression &&
                                unaryExpression.NodeType == EExpressionType.Not)
                            {
                                result =
                                    $"Not {unaryExpression.Operand.ToString(sourceType, out leftSqlParameter, creator)} <> {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                                sqlParameters.AddRange(leftSqlParameter);
                                sqlParameters.AddRange(rightSqlParameter);
                                break;
                            }
                        }

                        //如果是SqlServer数据源，且左侧是bool类型的常量，则需要特殊处理 用NOT代替
                        if (Left is ConstantExpression leftConstant && leftConstant.Value is bool leftBoolValue &&
                            sourceType == EDataSource.SqlServer)
                        {
                            if (Right is LikeExpression)
                            {
                                result =
                                    $"{(leftBoolValue ? " NOT " : "")}{Left.ToString(sourceType, out rightSqlParameter, creator)}";
                                sqlParameters.AddRange(rightSqlParameter);
                                break;
                            }

                            if (Right is UnaryExpression unaryExpression &&
                                unaryExpression.NodeType == EExpressionType.Not)
                            {
                                result =
                                    $"Not {Left.ToString(sourceType, out leftSqlParameter, creator)} <> {unaryExpression.Operand.ToString(sourceType, out rightSqlParameter, creator)}";
                                sqlParameters.AddRange(leftSqlParameter);
                                sqlParameters.AddRange(rightSqlParameter);
                                break;
                            }
                        }

                        result =
                            $"{Left.ToString(sourceType, out leftSqlParameter, creator)} <> {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                        sqlParameters.AddRange(leftSqlParameter);
                        sqlParameters.AddRange(rightSqlParameter);
                    }

                    break;
                case EExpressionType.LessThan:
                    result =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)} < {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                    sqlParameters.AddRange(leftSqlParameter);
                    sqlParameters.AddRange(rightSqlParameter);
                    break;
                case EExpressionType.LessThanOrEqual:
                    result =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)} <= {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                    sqlParameters.AddRange(leftSqlParameter);
                    sqlParameters.AddRange(rightSqlParameter);
                    break;
                case EExpressionType.GreaterThan:
                    result =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)} > {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                    sqlParameters.AddRange(leftSqlParameter);
                    sqlParameters.AddRange(rightSqlParameter);
                    break;
                case EExpressionType.GreaterThanOrEqual:
                    result =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)} >= {Right.ToString(sourceType, out rightSqlParameter, creator)}";
                    sqlParameters.AddRange(leftSqlParameter);
                    sqlParameters.AddRange(rightSqlParameter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的比较运算表达式类型{NodeType}");
            }

            return result;
        }
    }
}