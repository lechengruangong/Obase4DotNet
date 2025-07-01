/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示二元按位运算的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:00:16
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示二元按位运算的表达式。
    /// </summary>
    public class BinaryBitExpression : BinaryExpression
    {
        /// <summary>
        ///     创建BinaryBitExpression实例。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public BinaryBitExpression(Expression left, Expression right) : base(left, right)
        {
        }


        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            //可以处理 & | ^ ~ << >> 位运算
            switch (NodeType)
            {
                case EExpressionType.BitAnd:
                    return $"({Left.ToString(sourceType)}&({Right.ToString(sourceType)}))";
                case EExpressionType.BitOr:
                    return $"({Left.ToString(sourceType)}|({Right.ToString(sourceType)}))";
                case EExpressionType.BitXor:
                    return $"{Left.ToString(sourceType)}^({Right.ToString(sourceType)})";
                case EExpressionType.BitNot:
                    return $"{Left.ToString(sourceType)}~({Right.ToString(sourceType)})";
                case EExpressionType.LeftShift:
                    return $"{Left.ToString(sourceType)}<<({Right.ToString(sourceType)})";
                case EExpressionType.RightShift:
                    return $"{Left.ToString(sourceType)}>>({Right.ToString(sourceType)})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的位运算表达式类型{NodeType}");
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
            //每个部分的参数集合
            List<IDataParameter> leftSqlParameter;
            List<IDataParameter> rightSqlParameter;
            //字符串
            string resultStr;
            //可以处理 & | ^ ~ << >> 位运算
            switch (NodeType)
            {
                case EExpressionType.BitAnd:
                    resultStr =
                        $"({Left.ToString(sourceType, out leftSqlParameter, creator)}&({Right.ToString(sourceType, out rightSqlParameter, creator)}))";
                    break;
                case EExpressionType.BitOr:
                    resultStr =
                        $"({Left.ToString(sourceType, out leftSqlParameter, creator)}|({Right.ToString(sourceType, out rightSqlParameter, creator)}))";
                    break;
                case EExpressionType.BitXor:
                    resultStr =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)}^({Right.ToString(sourceType, out rightSqlParameter, creator)})";
                    break;
                case EExpressionType.BitNot:
                    resultStr =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)}~({Right.ToString(sourceType, out rightSqlParameter, creator)})";
                    break;
                case EExpressionType.LeftShift:
                    resultStr =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)}<<({Right.ToString(sourceType, out rightSqlParameter, creator)})";
                    break;
                case EExpressionType.RightShift:
                    resultStr =
                        $"{Left.ToString(sourceType, out leftSqlParameter, creator)}>>({Right.ToString(sourceType, out rightSqlParameter, creator)})";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的位运算表达式类型{NodeType}");
            }

            //最终的集合
            sqlParameters = new List<IDataParameter>();
            sqlParameters.AddRange(leftSqlParameter);
            sqlParameters.AddRange(rightSqlParameter);

            return resultStr;
        }
    }
}