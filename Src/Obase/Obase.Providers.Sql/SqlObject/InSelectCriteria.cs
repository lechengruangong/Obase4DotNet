/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：以查询结果集作为值域的IN运算条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:20:08
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using Obase.Core;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     以查询结果集作为值域的IN运算（广义）表示的条件。
    /// </summary>
    public class InSelectCriteria : ICriteria
    {
        /// <summary>
        ///     作为IN运算（广义）左操作数的表达式。
        /// </summary>
        private readonly Expression _left;

        /// <summary>
        ///     一个查询表达式，其查询结果将作为IN运算（广义）的值域。
        /// </summary>
        private readonly QuerySql _valueSetSql;

        /// <summary>
        ///     广义IN运算符。
        /// </summary>
        private EInOperator _operator;


        /// <summary>
        ///     创建InSelectCriteria的实例，指定IN运算的左操作数和生成值域的查询Sql语句。默认运算符为IN。
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="valueSet"></param>
        public InSelectCriteria(Expression expression, QuerySql valueSet)
        {
            _valueSetSql = valueSet;
            _left = expression;
        }

        /// <summary>
        ///     创建InSelectCriteria的实例，指定作为IN运算左操作数的字段和生成值域的查询Sql语句。默认运算符为IN。
        /// </summary>
        /// <param name="field"></param>
        /// <param name="valueSet"></param>
        public InSelectCriteria(Field field, QuerySql valueSet)
            : this(Expression.Fields(field), valueSet)
        {
        }

        /// <summary>
        ///     创建InSelectCriteria的实例，指定作为IN运算左操作数的字段的名称，同时指定生成值域的查询Sql语句。默认运算符为IN。
        /// </summary>
        /// <param name="field"></param>
        /// <param name="valueSet"></param>
        public InSelectCriteria(string field, QuerySql valueSet)
            : this(new Field(field), valueSet)
        {
        }

        /// <summary>
        ///     创建InSelectCriteria的实例，指定作为IN运算左操作数的字段的名称及其所在的源的名称，同时指定生成值域的查询Sql语句。默认运算符为IN。
        /// </summary>
        /// <param name="field"></param>
        /// <param name="source"></param>
        /// <param name="valueSet"></param>
        public InSelectCriteria(string field, string source, QuerySql valueSet)
            : this(new Field(source, field), valueSet)
        {
        }

        /// <summary>
        ///     创建InSelectCriteria的实例，指定作为IN运算左操作数的字段的名称及其所在的源，同时指定生成值域的查询Sql语句。默认运算符为IN。
        /// </summary>
        /// <param name="field"></param>
        /// <param name="source"></param>
        /// <param name="valueSet"></param>
        public InSelectCriteria(string field, ISource source, QuerySql valueSet)
            : this(new Field((MonomerSource)source, field), valueSet)
        {
        }

        /// <summary>
        ///     获取或设置广义IN运算符。默认值为IN。
        /// </summary>
        public EInOperator Operator
        {
            get => _operator;
            set => _operator = value;
        }

        /// <summary>
        ///     获取IN运算（广义）的左操作数。
        /// </summary>
        public Expression Left => _left;

        /// <summary>
        ///     获取生成IN运算（广义）值域的查询Sql语句。
        /// </summary>
        public QuerySql ValueSetSql => _valueSetSql;


        /// <summary>
        ///     将当前条件与另一条件执行逻辑与运算，得出一个新条件。
        /// </summary>
        /// <param name="other">另一个条件</param>
        public ICriteria And(ICriteria other)
        {
            if (other == null)
                return this;
            return new ComplexCriteria(this, other, ELogicalOperator.And);
        }

        /// <summary>
        ///     对当前条件执行逻辑非运算，得出一个新条件。
        /// </summary>
        public ICriteria Not()
        {
            return new ComplexCriteria(this, null, ELogicalOperator.Not);
        }

        /// <summary>
        ///     将当前条件与另一条件执行逻辑或运算，得出一个新条件。
        /// </summary>
        /// <param name="other">另一个条件</param>
        public ICriteria Or(ICriteria other)
        {
            if (other == null)
                return this;
            return new ComplexCriteria(this, other, ELogicalOperator.Or);
        }

        /// <summary>
        ///     将表达式访问者引导至条件内部的表达式。
        ///     特别约定：仅引导至直接包含的表达式，规避通过其它对象间接包含的表达式（如InSelectCriteria中作为值域的子查询所包含的表达式）。
        /// </summary>
        /// <param name="visitor">要引导的表达式访问者。</param>
        public void GuideExpressionVisitor(ExpressionVisitor visitor)
        {
            _left.Accept(visitor);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            switch (Operator)
            {
                case EInOperator.In:
                    return $" {Left.ToString(sourceType)} IN （{ValueSetSql.ToSql(sourceType)})";
                case EInOperator.Notin:
                    return $" {Left.ToString(sourceType)} NOT IN （{ValueSetSql.ToSql(sourceType)})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator), $"不支持的IN操作{Operator}");
            }
        }

        /// <summary>
        ///     使用默认数据源和参数化的方式将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sqlParameters">参数</param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public string ToString(out List<IDataParameter> sqlParameters, IParameterCreator creator)
        {
            return ToString(EDataSource.SqlServer, out sqlParameters, creator);
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //每个部分的参数集合
            List<IDataParameter> leftSqlParameter;
            List<IDataParameter> rightSqlParameter;
            //字符串
            string resultStr;
            switch (Operator)
            {
                case EInOperator.In:
                    resultStr =
                        $" {Left.ToString(sourceType, out leftSqlParameter, creator)} IN ({ValueSetSql.ToSql(sourceType, out rightSqlParameter, creator)})";
                    break;
                case EInOperator.Notin:
                    resultStr =
                        $" {Left.ToString(sourceType, out leftSqlParameter, creator)} NOT IN ({ValueSetSql.ToSql(sourceType, out rightSqlParameter, creator)})";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Operator), $"不支持的IN操作{Operator}");
            }

            //最终的集合
            sqlParameters = new List<IDataParameter>();
            sqlParameters.AddRange(leftSqlParameter);
            sqlParameters.AddRange(rightSqlParameter);

            return resultStr;
        }
    }
}