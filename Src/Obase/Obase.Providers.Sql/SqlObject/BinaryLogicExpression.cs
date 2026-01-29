/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示二元逻辑运算的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:02:32
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示二元逻辑运算的表达式。
    /// </summary>
    public class BinaryLogicExpression : BinaryExpression
    {
        /// <summary>
        ///     创建BinaryLogicExpression的实例，并设置Left属性和Right属性的值。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public BinaryLogicExpression(Expression left, Expression right)
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
            //由于SQL Server不支持布尔类型字段作为条件，故需要将布尔类型字段转换为位类型字段进行处理
            if (sourceType == EDataSource.SqlServer) ReplaceBoolField();

            //操作数
            string operatorStr;
            //可以处理And Or
            switch (NodeType)
            {
                case EExpressionType.OrElse:
                    operatorStr = " OR ";
                    break;
                case EExpressionType.AndAlso:
                    operatorStr = " AND ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的二元逻辑运算表达式类型{NodeType}");
            }

            //判断左操作数
            if (Left is BinaryLogicExpression leftBinaryLogicExpression &&
                leftBinaryLogicExpression.NodeType == NodeType)
                return $"{Left.ToString(sourceType)}{operatorStr}({Right.ToString(sourceType)})";

            //判断右操作数
            if (Right is BinaryLogicExpression rightBinaryLogicExpression &&
                rightBinaryLogicExpression.NodeType == NodeType)
                return $"({Left.ToString(sourceType)}){operatorStr}{Right.ToString(sourceType)}";

            return $"({Left.ToString(sourceType)}){operatorStr}({Right.ToString(sourceType)})";
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
            //由于SQL Server不支持布尔类型字段作为条件，故需要将布尔类型字段转换为位类型字段进行处理
            if (sourceType == EDataSource.SqlServer) ReplaceBoolField();

            //操作数
            string operatorStr;
            //可以处理And Or
            switch (NodeType)
            {
                case EExpressionType.OrElse:
                    operatorStr = " OR ";
                    break;
                case EExpressionType.AndAlso:
                    operatorStr = " AND ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的二元逻辑运算表达式类型{NodeType}");
            }

            //每个部分的参数集合
            List<IDataParameter> leftSqlParameter;
            List<IDataParameter> rightSqlParameter;
            //字符串
            string resultStr;

            //判断左操作数
            if (Left is BinaryLogicExpression leftBinaryLogicExpression &&
                leftBinaryLogicExpression.NodeType == NodeType)
                resultStr =
                    $"{Left.ToString(sourceType, out leftSqlParameter, creator)}{operatorStr}({Right.ToString(sourceType, out rightSqlParameter, creator)})";
            //判断右操作数
            else if (Right is BinaryLogicExpression rightBinaryLogicExpression &&
                     rightBinaryLogicExpression.NodeType == NodeType)
                resultStr =
                    $"({Left.ToString(sourceType, out leftSqlParameter, creator)}){operatorStr}{Right.ToString(sourceType, out rightSqlParameter, creator)}";
            else
                resultStr =
                    $"({Left.ToString(sourceType, out leftSqlParameter, creator)}){operatorStr}({Right.ToString(sourceType, out rightSqlParameter, creator)})";

            //最终的集合
            sqlParameters = new List<IDataParameter>();
            sqlParameters.AddRange(leftSqlParameter);
            sqlParameters.AddRange(rightSqlParameter);

            return resultStr;
        }
    }
}