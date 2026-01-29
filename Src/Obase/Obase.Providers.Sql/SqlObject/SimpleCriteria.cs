/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示简单条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:10:11
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示简单条件，形如：[源名].字段名=值、[源名1].字段名1=[源名2].字段名2
    /// </summary>
    public abstract class SimpleCriteria<TValue> : ExpressionCriteria
    {
        /// <summary>
        ///     创建简单条件实例。
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="relationOperator">关系运算符</param>
        /// <param name="value">参考值</param>
        protected SimpleCriteria(string field, ERelationOperator relationOperator, TValue value)
            : base(CreateExpression(new Field(field), relationOperator, value))
        {
        }

        /// <summary>
        ///     创建简单条件实例。
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="relationOperator">关系运算符</param>
        /// <param name="value">参考值</param>
        protected SimpleCriteria(Field field, ERelationOperator relationOperator, TValue value)
            : base(CreateExpression(field, relationOperator, value))
        {
        }

        /// <summary>
        ///     创建简单条件实例。
        /// </summary>
        /// <param name="source">源名称</param>
        /// <param name="field">字段名</param>
        /// <param name="relationOperator">关系运算符</param>
        /// <param name="value">参考值</param>
        protected SimpleCriteria(string source, string field, ERelationOperator relationOperator, TValue value)
            : base(CreateExpression(new Field(source, field), relationOperator, value))
        {
        }


        /// <summary>
        ///     获取或设置参考值，可以为一个实际值或另外一个字段。
        /// </summary>
        protected virtual TValue Value
        {
            get
            {
                switch (Operator)
                {
                    case ERelationOperator.Equal:
                    case ERelationOperator.Unequal:
                    case ERelationOperator.LessThanOrEqual:
                    case ERelationOperator.LessThan:
                    case ERelationOperator.GreaterThan:
                    case ERelationOperator.GreaterThanOrEqual:
                        var exp = ((BinaryExpression)Expression).Right;
                        if (exp.NodeType == EExpressionType.Constant)
                            return (TValue)Convert.ChangeType((exp as ConstantExpression)?.Value, typeof(TValue));
                        return (TValue)Convert.ChangeType((exp as FieldExpression)?.Field, typeof(TValue));
                    case ERelationOperator.Like:
                        return (TValue)Convert.ChangeType(
                            ((LikeExpression)((BinaryExpression)Expression).Right).Pattern, typeof(TValue));
                    case ERelationOperator.In:
                    case ERelationOperator.NotIn:
                        return (TValue)Convert.ChangeType(
                            ((InExpression)((BinaryExpression)Expression).Right).ValueSet, typeof(object[]));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(Operator), $"未知的关系运算{Operator}.");
                }
            }
            set => _expression = CreateExpression(Field, Operator, value);
        }

        /// <summary>
        ///     获取或设置字段。
        /// </summary>
        protected virtual Field Field
        {
            get => ((FieldExpression)((BinaryExpression)Expression).Left).Field;
            set => _expression = CreateExpression(value, Operator, Value);
        }

        /// <summary>
        ///     获取或设置关系运算符。
        /// </summary>
        public virtual ERelationOperator Operator
        {
            get => GetERelationOperator(Expression.NodeType);
            set => _expression = CreateExpression(Field, value, Value);
        }

        /// <summary>
        ///     创建表达式
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="relationoperator">操作符</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private static Expression CreateExpression(Field field, ERelationOperator relationoperator, TValue value)
        {
            Expression valueExp;
            //字段 取字段 否则 取常量
            if (value is Field field1)
                valueExp = Expression.Fields(field1);
            else
                valueExp = Expression.Constant(value);
            //根据关系运算符生成表达式
            switch (relationoperator)
            {
                case ERelationOperator.Equal:
                    return Expression.Equal(Expression.Fields(field), valueExp);
                case ERelationOperator.Unequal:
                    return Expression.NotEqual(Expression.Fields(field), valueExp);
                case ERelationOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(Expression.Fields(field), valueExp);
                case ERelationOperator.LessThan:
                    return Expression.LessThan(Expression.Fields(field), valueExp);
                case ERelationOperator.GreaterThan:
                    return Expression.GreaterThan(Expression.Fields(field), valueExp);
                case ERelationOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(Expression.Fields(field), valueExp);
                case ERelationOperator.Like:
                    return Expression.Like(Expression.Fields(field), value.ToString());
                case ERelationOperator.In:
                    return Expression.In(Expression.Fields(field),
                        (value as IEnumerable ?? throw new InvalidOperationException("In运算必须为集合类型.")).Cast<object>()
                        .ToArray());
                case ERelationOperator.NotIn:
                    return Expression.NotIn(Expression.Fields(field),
                        (value as IEnumerable ?? throw new InvalidOperationException("NotIn运算必须为集合类型.")).Cast<object>()
                        .ToArray());
                default:
                    throw new ArgumentOutOfRangeException(nameof(relationoperator), $"未知的关系运算{relationoperator}.");
            }
        }

        /// <summary>
        ///     关系运算符映射
        /// </summary>
        /// <param name="expressionType">表达式的关系运算</param>
        /// <returns></returns>
        private ERelationOperator GetERelationOperator(EExpressionType expressionType)
        {
            switch (expressionType)
            {
                case EExpressionType.Equal:
                    return ERelationOperator.Equal;
                case EExpressionType.NotEqual:
                    return ERelationOperator.Unequal;
                case EExpressionType.LessThan:
                    return ERelationOperator.LessThan;
                case EExpressionType.LessThanOrEqual:
                    return ERelationOperator.LessThanOrEqual;
                case EExpressionType.GreaterThan:
                    return ERelationOperator.GreaterThan;
                case EExpressionType.GreaterThanOrEqual:
                    return ERelationOperator.GreaterThanOrEqual;
                case EExpressionType.Like:
                    return ERelationOperator.Like;
                case EExpressionType.In:
                    return ERelationOperator.In;
                case EExpressionType.NotIn:
                    return ERelationOperator.NotIn;
                default:
                    throw new ArgumentOutOfRangeException(nameof(EExpressionType), $"未知的关系运算{expressionType}.");
            }
        }

        /// <summary>
        ///     生成value对应数据中的值
        /// </summary>
        /// <returns></returns>
        protected virtual string GenerateSqlValue()
        {
            string matchValue;
            if (Value == null) return null;

            if (Value is DateTime)
            {
                //处理日期
                var date = Convert.ToDateTime(Value);
                if (date < DateTime.Parse("1753/01/01"))
                {
                    if (Operator == ERelationOperator.Equal || Operator == ERelationOperator.Unequal)
                        matchValue = null;
                    else
                        matchValue = "'1753/01/01'";
                }
                else if (date > DateTime.Parse("9999/12/31"))
                {
                    if (Operator == ERelationOperator.Equal || Operator == ERelationOperator.Unequal)
                        matchValue = null;
                    else
                        matchValue = "'9999/12/31'";
                }
                else
                {
                    matchValue = "'" + date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                }
            }
            //处理时间
            else if (Value is TimeSpan time)
            {
                matchValue = "'" + time.ToString("c") + "'";
            }
            //GUID
            else if (Value is Guid guid)
            {
                matchValue = "'" + guid.ToString("D").ToUpper() + "'";
            }
            //布尔
            else if (Value is bool)
            {
                matchValue = Convert.ToBoolean(Value) ? "1" : "0";
            }
            else
            {
                matchValue = Value is Field field ? field.ToString() : Value.ToString();
            }

            return matchValue;
        }

        /// <summary>
        ///     生成value对应数据中的值并返回参数
        /// </summary>
        /// <param name="sourceType">数据源</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        protected virtual string GenerateSqlValue(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            sqlParameters = new List<IDataParameter>();
            //构造一个随机数
            var random =
                Guid.NewGuid().ToString().Replace("-", "")
                    .ToLower(); //Math.Abs(new TimeBasedIdGenerator().Next() + new Random().Next());
            //参数名
            string parameter;
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                case EDataSource.PostgreSql:
                    parameter = $"@simpleValue{random}";
                    break;
                case EDataSource.Oracle:
                {
                    long lg = 1;
                    foreach (var item in Guid.NewGuid().ToByteArray()) lg *= item + 1;
                    var code = $"{lg - DateTime.Now.Ticks:x}";
                    parameter = $":sv{code}";
                }

                    break;
                case EDataSource.MySql:
                    parameter = $"?simpleValue{random}";
                    break;
                case EDataSource.Sqlite:
                    parameter = $"$simpleValue{random}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
            }

            var value = Value;
            object paValue;

            if (value is bool boolValue)
            {
                if (sourceType == EDataSource.SqlServer)
                    paValue = boolValue ? 1 : 0;
                else if (sourceType == EDataSource.PostgreSql)
                    paValue = boolValue;
                else
                    paValue = value.ToString();
            }
            else if (value is Guid guid)
            {
                paValue = "'" + guid.ToString("D").ToUpper() + "'";
            }
            else
            {
                paValue = value;
            }

            var dataParameter = creator.Create();
            dataParameter.ParameterName = parameter;
            dataParameter.Value = paValue;

            sqlParameters.Add(dataParameter);

            return parameter;
        }

        /// <summary>
        ///     生成字符串表示形式
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <returns></returns>
        public new string ToString(EDataSource sourceType)
        {
            string returnValue;
            //字段
            var result = Field.ToString(sourceType);
            //值
            var matchValue = GenerateSqlValue();
            switch (Operator)
            {
                case ERelationOperator.Equal:
                    returnValue = matchValue != null ? $"{result} = {matchValue}" : $"{result} is null ";
                    break;
                case ERelationOperator.GreaterThan:
                    returnValue = $"{result} > {matchValue}";
                    break;
                case ERelationOperator.GreaterThanOrEqual:
                    returnValue = $"{result} >= {matchValue}";
                    break;
                case ERelationOperator.In:
                    returnValue = $"{result} in ({matchValue})";
                    break;
                case ERelationOperator.LessThan:
                    returnValue = $"{result} < {matchValue}";
                    break;
                case ERelationOperator.LessThanOrEqual:
                    returnValue = $"{result} <= {matchValue}";
                    break;
                case ERelationOperator.Like:
                    returnValue = $"{result} like '%{matchValue.TrimStart('\'').TrimEnd('\'')}%'";
                    break;
                case ERelationOperator.NotIn:
                    returnValue = $"{result} not in ({matchValue})";
                    break;
                case ERelationOperator.Unequal:
                    returnValue = matchValue != null ? $"{result} <> {matchValue}" : $"{result} is not null ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator), $"未知的简单条件操作符{Operator}");
            }

            return returnValue;
        }

        /// <summary>
        ///     生成条件实例的参数化的字符串表示形式。
        /// </summary>
        /// <param name="parameters">返回字符串中的参数及其值的集合。</param>
        /// <param name="creator">参数构造器</param>
        public new string ToString(out List<IDataParameter> parameters, IParameterCreator creator)
        {
            return ToString(EDataSource.SqlServer, out parameters, creator);
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public new string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            string returnValue;
            //字段
            var result = Field.ToString(sourceType);
            //值
            var matchValue = GenerateSqlValue(sourceType, out var matchValueParameters, creator);
            switch (Operator)
            {
                case ERelationOperator.Equal:
                    returnValue = matchValue != null ? $"{result} = {matchValue}" : $"{result} is null ";
                    break;
                case ERelationOperator.GreaterThan:
                    returnValue = $"{result} > {matchValue}";
                    break;
                case ERelationOperator.GreaterThanOrEqual:
                    returnValue = $"{result} >= {matchValue}";
                    break;
                case ERelationOperator.In:
                    returnValue = $"{result} in ({matchValue})";
                    break;
                case ERelationOperator.LessThan:
                    returnValue = $"{result} < {matchValue}";
                    break;
                case ERelationOperator.LessThanOrEqual:
                    returnValue = $"{result} <= {matchValue}";
                    break;
                case ERelationOperator.Like:
                    returnValue = $"{result} like '%{matchValue.TrimStart('\'').TrimEnd('\'')}%'";
                    break;
                case ERelationOperator.NotIn:
                    returnValue = $"{result} not in ({matchValue})";
                    break;
                case ERelationOperator.Unequal:
                    returnValue = matchValue != null ? $"{result} <> {matchValue}" : $"{result} is not null ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator), $"未知的简单条件操作符{Operator}");
            }

            //加入参数
            sqlParameters = new List<IDataParameter>();
            sqlParameters.AddRange(matchValueParameters);

            return returnValue;
        }
    }
}