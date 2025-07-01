/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示一元运算的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:51:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示一元运算的表达式，具体可以表示Increment（递减）、Decrement（递增）、Negate（算术取反）、UnaryPlus（一元加法）、Not（逻辑求反）、BitNot（按位取反）六种运算。
    /// </summary>
    public class UnaryExpression : Expression
    {
        /// <summary>
        ///     操作数。
        /// </summary>
        private readonly Expression _operand;


        /// <summary>
        ///     创建UnaryExpression的实例，并设置Operand属性的值。
        /// </summary>
        /// <param name="operand">操作数。</param>
        internal UnaryExpression(Expression operand)
        {
            _operand = operand;
        }

        /// <summary>
        ///     获取操作数。
        /// </summary>
        public Expression Operand => _operand;

        /// <summary>
        ///     判定具体类型的表达式对象是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected override bool ConcreteEquals(Expression other)
        {
            var notOther = other as UnaryExpression;
            if (notOther != null && Operand == notOther.Operand)
                return true;
            return false;
        }

        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public override string ToString(EDataSource sourceType)
        {
            switch (NodeType)
            {
                case EExpressionType.Increment:
                    return $"({Operand.ToString(sourceType)}+1)";
                case EExpressionType.Decrement:
                    return $"({Operand.ToString(sourceType)}-1)";
                case EExpressionType.Negate:
                    return $"(-{Operand.ToString(sourceType)})";
                case EExpressionType.UnaryPlus:
                    return $"(+{Operand.ToString(sourceType)})";
                case EExpressionType.Not:
                    return $"(!{Operand.ToString(sourceType)})";
                case EExpressionType.BitNot:
                    return $"(~{Operand.ToString(sourceType)})";
                default: throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的一元运算表达式类型{NodeType}");
            }
        }


        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将表达式表示为字符串形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            switch (NodeType)
            {
                case EExpressionType.Increment:
                    return $"({Operand.ToString(sourceType, out sqlParameters, creator)}+1)";
                case EExpressionType.Decrement:
                    return $"({Operand.ToString(sourceType, out sqlParameters, creator)}-1)";
                case EExpressionType.Negate:
                    return $"(-{Operand.ToString(sourceType, out sqlParameters, creator)})";
                case EExpressionType.UnaryPlus:
                    return $"(+{Operand.ToString(sourceType, out sqlParameters, creator)})";
                case EExpressionType.Not:
                {
                    if (sourceType == EDataSource.SqlServer || sourceType == EDataSource.Sqlite ||
                        sourceType == EDataSource.PostgreSql)
                    {
                        if (Operand is FieldExpression)
                        {
                            var exp = Equal(Operand, new ConstantExpression(true));
                            return $"not {exp.ToString(sourceType, out sqlParameters, creator)}";
                        }

                        if (Operand is ConstantExpression constant && constant.Value is bool)
                        {
                            sqlParameters = new List<IDataParameter>();
                            return "(1<>1)";
                        }

                        return $"not {Operand.ToString(sourceType, out sqlParameters, creator)}";
                    }

                    return $"(!{Operand.ToString(sourceType, out sqlParameters, creator)})";
                }
                case EExpressionType.BitNot:
                    return $"(~{Operand.ToString(sourceType, out sqlParameters, creator)})";
                default: throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的一元运算表达式类型{NodeType}");
            }
        }
    }
}