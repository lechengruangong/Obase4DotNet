/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示IN运算的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:19:16
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
    ///     表示IN运算的表达式。
    /// </summary>
    public class InExpression : BinaryExpression
    {
        /// <summary>
        ///     广义IN运算符。
        /// </summary>
        private EInOperator _operator;

        /// <summary>
        ///     IN运算的值域
        /// </summary>
        private IEnumerable _valueSet;

        /// <summary>
        ///     创建InExpression的实例，并设置Left属性、ValueSet属性和Operator属性的值。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="valueSet">值域。</param>
        /// <param name="eOperator">广义IN运算符。</param>
        internal InExpression(Expression left, object[] valueSet, EInOperator eOperator)
            : base(left, null)
        {
            _operator = eOperator;
            ValueSet = valueSet;
        }

        /// <summary>
        ///     创建InExpression的实例，并设置Left属性、ValueSet属性和Operator属性的值。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="valueSet">值域。</param>
        internal InExpression(Expression left, object[] valueSet)
            : base(left, null)
        {
            ValueSet = valueSet;
        }

        /// <summary>
        ///     创建InExpression的实例，并设置Left属性、ValueSet属性和Operator属性的值。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="valueSet">值域。</param>
        /// <param name="eOperator">广义IN运算符。</param>
        internal InExpression(Expression left, IEnumerable valueSet, EInOperator eOperator) : base(left, null)
        {
            _valueSet = valueSet;
            _operator = eOperator;
        }

        /// <summary>
        ///     获取IN运算的值域。
        /// </summary>
        public object[] ValueSet
        {
            get
            {
                var array = new ArrayList();
                var opter = _valueSet.GetEnumerator();
                while (opter.MoveNext()) array.Add(opter.Current);
                if (opter is IDisposable disposable)
                    disposable.Dispose();
                return array.ToArray();
            }
            set => _valueSet = value;
        }

        /// <summary>
        ///     获取广义IN运算符。
        /// </summary>
        public EInOperator Operator => _operator;


        /// <summary>
        ///     判定具体类型的表达式对象是否相等
        /// </summary>
        /// <param name="other">另一个表达式</param>
        /// <returns></returns>
        protected override bool ConcreteEquals(Expression other)
        {
            var inOther = other as InExpression;
            if (inOther != null && Operator == inOther.Operator && ValueSet.SequenceEqual(inOther.ValueSet))
                return true;
            return false;
        }

        /// <summary>
        ///     翻转操作符
        /// </summary>
        public void FlipOverOperator()
        {
            switch (_operator)
            {
                case EInOperator.In:
                    _operator = EInOperator.Notin;
                    break;
                case EInOperator.Notin:
                    _operator = EInOperator.In;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的IN运算表达式类型{NodeType}");
            }
        }

        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            switch (_operator)
            {
                case EInOperator.In:
                    return $"{Left.ToString(sourceType)} IN ({string.Join(",", ValueSet.Select(s => $"{s}"))})";
                case EInOperator.Notin:
                    return $"{Left.ToString(sourceType)} NOT IN ({string.Join(",", ValueSet.Select(s => $"{s}"))})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的IN运算表达式类型{NodeType}");
            }
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将表达式表示为字符串形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //每个部分的参数集合
            var rightSqlParameter = new List<IDataParameter>();
            //字符串
            string resultStr;
            //值字符串
            var parameterStrList = new List<string>();

            //参数
            foreach (var value in ValueSet)
            {
                var random =
                    Guid.NewGuid().ToString().Replace("-", "")
                        .ToLower(); //Math.Abs(new TimeBasedIdGenerator().Next() + new Random().Next());

                var dataParameter = creator.Create();

                string parameter;
                switch (sourceType)
                {
                    case EDataSource.SqlServer:
                    case EDataSource.PostgreSql:
                        parameter = $"@inValue{random}";
                        break;
                    case EDataSource.Oracle:
                        parameter = $":inValue{random}";
                        break;
                    case EDataSource.MySql:
                        parameter = $"?inValue{random}";
                        break;
                    case EDataSource.Sqlite:
                        parameter = $"$inValue{random}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }

                dataParameter.ParameterName = parameter;
                dataParameter.Value = value;

                rightSqlParameter.Add(dataParameter);
                parameterStrList.Add(parameter);
            }

            if (ValueSet.Length <= 0)
            {
                switch (_operator)
                {
                    case EInOperator.In:
                        resultStr = "(1<>1)";
                        break;
                    case EInOperator.Notin:
                        resultStr = "(1=1)";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的IN运算表达式类型{NodeType}");
                }

                sqlParameters = new List<IDataParameter>();
            }
            else
            {
                List<IDataParameter> leftSqlParameter;
                switch (_operator)
                {
                    case EInOperator.In:
                        resultStr =
                            $"{Left.ToString(sourceType, out leftSqlParameter, creator)} IN ({string.Join(",", parameterStrList.Select(s => $"{s}"))})";
                        break;
                    case EInOperator.Notin:
                        resultStr =
                            $"{Left.ToString(sourceType, out leftSqlParameter, creator)} NOT IN ({string.Join(",", parameterStrList.Select(s => $"{s}"))})";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(NodeType), $"不支持的IN运算表达式类型{NodeType}");
                }

                sqlParameters = new List<IDataParameter>();
                sqlParameters.AddRange(leftSqlParameter);
                sqlParameters.AddRange(rightSqlParameter);
            }

            return resultStr;
        }
    }
}