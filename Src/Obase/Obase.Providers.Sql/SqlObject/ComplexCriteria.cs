/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示复杂条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:48:18
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Obase.Core;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示复杂条件，该条件由两个条件通过逻辑运算得出。
    /// </summary>
    public class ComplexCriteria : ICriteria
    {
        /// <summary>
        ///     条件列表
        /// </summary>
        private readonly List<ICriteria> _criterias;


        /// <summary>
        ///     逻辑运算符
        /// </summary>
        private ELogicalOperator _logicalOperator;

        /// <summary>
        ///     创建复杂条件实例，该条件由两个条件通过逻辑与运算得出。
        /// </summary>
        /// <param name="criteria1">第一个条件</param>
        /// <param name="criteria2">第二个条件</param>
        public ComplexCriteria(ICriteria criteria1, ICriteria criteria2)
        {
            _criterias = new List<ICriteria>();
            _criterias.AddRange(new[] { criteria1, criteria2 });
        }

        /// <summary>
        ///     创建复杂条件实例。
        /// </summary>
        /// <param name="criteria1">第一个条件</param>
        /// <param name="criteria2">第一个条件</param>
        /// <param name="logicaloperator">逻辑运算符</param>
        public ComplexCriteria(ICriteria criteria1, ICriteria criteria2, ELogicalOperator logicaloperator)
        {
            _criterias = new List<ICriteria>();
            //不能连接两个空条件
            if (criteria1 == null && criteria2 == null) throw new InvalidOperationException("不能连接两个为空的条件.");
            //只有非操作才能连接第二个空条件
            if (criteria2 == null && logicaloperator != ELogicalOperator.Not)
                throw new InvalidOperationException("只有非操作才能连接空条件.");
            //分别添加
            if (criteria1 != null) _criterias.Add(criteria1);

            if (criteria2 != null) _criterias.Add(criteria2);

            _logicalOperator = logicaloperator;
        }

        /// <summary>
        ///     获取或设置逻辑运算符。
        /// </summary>
        public ELogicalOperator LogicalOperator
        {
            get => _logicalOperator;
            set => _logicalOperator = value;
        }

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
        ///     导航表达式树访问器
        /// </summary>
        /// <param name="visitor">表达式树翻译器</param>
        public void GuideExpressionVisitor(ExpressionVisitor visitor)
        {
            _criterias.ForEach(c => c.GuideExpressionVisitor(visitor));
        }


        /// <summary>
        ///     生成条件实例的字符串表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            // 如果逻辑运算符不为Not，执行以下优化：
            //（1）如果左操作数（left）为ComplexCriteria，且left.LogicalOperator == LogicalOperator，则不对左操作数使用括号；
            //（2）如果左操作数为ExpressionCriteria，且表达式为BinaryLogicExpression，且运算符为And或Or，不对左操作数使用括号；
            //（3）如果右操作数为ComplexCriteria，参照（1）处理；
            //（4）如果右操作数为ExpressionCriteria，参照（2）处理。
            var logical = string.Empty;

            //Not 直接取反条件中的第一个
            if (_logicalOperator == ELogicalOperator.Not)
            {
                if (_criterias.Count > 1)
                    throw new InvalidOperationException("取反操作数不可大于1.");
                return $" ( not ( {_criterias[0].ToString(sourceType)} ) )";
            }

            //不是Not 构造操作符
            switch (_logicalOperator)
            {
                case ELogicalOperator.And:
                    logical = " and ";
                    break;
                case ELogicalOperator.Or:
                    logical = " or ";
                    break;
            }

            return $"({string.Join(logical, _criterias.Select(u => $"({u.ToString(sourceType)})"))})";
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
            // 如果逻辑运算符不为Not，执行以下优化：
            //（1）如果左操作数（left）为ComplexCriteria，且left.LogicalOperator == LogicalOperator，则不对左操作数使用括号；
            //（2）如果左操作数为ExpressionCriteria，且表达式为BinaryLogicExpression，且运算符为And或Or，不对左操作数使用括号；
            //（3）如果右操作数为ComplexCriteria，参照（1）处理；
            //（4）如果右操作数为ExpressionCriteria，参照（2）处理。
            var logical = string.Empty;

            //Not 直接取反条件中的第一个
            if (_logicalOperator == ELogicalOperator.Not)
            {
                if (_criterias.Count > 1)
                    throw new InvalidOperationException("取反操作数不可大于1.");
                return $" ( not ( {_criterias[0].ToString(sourceType, out sqlParameters, creator)} ) )";
            }

            //不是Not 构造操作符
            switch (_logicalOperator)
            {
                case ELogicalOperator.And:
                    logical = " and ";
                    break;
                case ELogicalOperator.Or:
                    logical = " or ";
                    break;
            }

            //最终的集合
            sqlParameters = new List<IDataParameter>();

            //每个条件都ToString
            var resultStrList = new List<string>();
            foreach (var criteria in _criterias)
            {
                resultStrList.Add(criteria.ToString(sourceType, out var parameters, creator));
                sqlParameters.AddRange(parameters);
            }

            return $"({string.Join(logical, resultStrList.Select(q => $"({q})"))})";
        }

        /// <summary>
        ///     使用默认数据源和参数化的方式将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sqlParameters">参数</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public string ToString(out List<IDataParameter> sqlParameters, IParameterCreator creator)
        {
            return ToString(EDataSource.SqlServer, out sqlParameters, creator);
        }
    }
}