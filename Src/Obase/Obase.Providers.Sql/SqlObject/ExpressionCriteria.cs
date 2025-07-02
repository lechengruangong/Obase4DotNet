/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：以表达式表示的条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:51:03
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;
using Obase.Core;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     以表达式表示的条件。
    /// </summary>
    public class ExpressionCriteria : ICriteria
    {
        /// <summary>
        ///     表示条件的表达式。
        /// </summary>
        protected Expression _expression;

        /// <summary>
        ///     使用指定的布尔表达式创建ExpressionCriteria的实例。
        /// </summary>
        /// <param name="expression">一个表示条件的布尔表达式。</param>
        public ExpressionCriteria(Expression expression)
        {
            _expression = expression;
        }

        /// <summary>
        ///     获取表示条件的表达式。
        /// </summary>
        public Expression Expression => _expression;

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
            //如果是In操作 则翻转为NotIn
            if (_expression is InExpression inExpression)
            {
                inExpression.FlipOverOperator();
                return this;
            }

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
        ///     将表达式访问者引导至各投影列表达式。
        /// </summary>
        /// <param name="visitor">要引导的表达式访问者。</param>
        public void GuideExpressionVisitor(ExpressionVisitor visitor)
        {
            Expression.Accept(visitor);
        }

        /// <summary>
        ///     生成字符串表示形式
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            return Expression.ToString(sourceType);
        }

        /// <summary>
        ///     生成条件实例的参数化的字符串表示形式。
        /// </summary>
        /// <param name="parameters">返回字符串中的参数及其值的集合。</param>
        /// <param name="creator">参数对象构造器</param>
        public string ToString(out List<IDataParameter> parameters, IParameterCreator creator)
        {
            return ToString(EDataSource.SqlServer, out parameters, creator);
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            if (sourceType == EDataSource.SqlServer)
            {
                //如果是字段表达式 提取其中的字段 与 常量true 组合成结果
                if (Expression is FieldExpression)
                {
                    var exp = Expression.Equal(Expression, new ConstantExpression(true));
                    return exp.ToString(sourceType, out sqlParameters, creator);
                }

                //如果就是布尔值的常量表达式 转换为1=1作为条件
                if (Expression is ConstantExpression constant && constant.Value is bool)
                {
                    sqlParameters = new List<IDataParameter>();
                    return "(1=1)";
                }
            }

            return Expression.ToString(sourceType, out sqlParameters, creator);
        }
    }
}