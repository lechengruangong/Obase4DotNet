/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示具有常量值的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:48:55
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示具有常量值的表达式。
    /// </summary>
    public class ConstantExpression : Expression
    {
        /// <summary>
        ///     常量表达式的值。
        /// </summary>
        private readonly object _value;

        /// <summary>
        ///     创建ConstantExpression的实例，并设置其Value属性值。
        /// </summary>
        /// <param name="value">常量值。</param>
        internal ConstantExpression(object value)
        {
            _value = value;
        }

        /// <summary>
        ///     获取常量表达式的值。
        /// </summary>
        public object Value => _value;

        /// <summary>
        ///     确定指定的表达式与当前表达式是否相等。
        /// </summary>
        /// <param name="other">要与当前表达式进行比较的表达式。</param>
        protected override bool ConcreteEquals(Expression other)
        {
            var constOther = other as ConstantExpression;
            if (constOther != null && constOther.Value == Value)
                return true;
            return false;
        }

        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            if (_value == null) return "NULL";

            if (_value is DateTime dateTime) return dateTime.ToString("yyyy-MM-dd HH:mm:ss");

            return _value.ToString();
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
            //参数值
            sqlParameters = new List<IDataParameter>();
            //if (_value == null) return "NULL";

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
                    parameter = $"@constantValue{random}";
                    break;
                case EDataSource.Oracle:
                    parameter = $":constantValue{random}";
                    break;
                case EDataSource.MySql:
                    parameter = $"?constantValue{random}";
                    break;
                case EDataSource.Sqlite:
                    parameter = $"$constantValue{random}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
            }

            var dataParameter = creator.Create();
            dataParameter.ParameterName = parameter;
            if (_value == null)
            {
                dataParameter.Value = DBNull.Value;
            }
            else
            {
                //具体值
                if (_value is DateTime dateTime)
                {
                    if (sourceType == EDataSource.PostgreSql)
                        dataParameter.Value = dateTime;
                    else
                        dataParameter.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                //布尔值 转换为1或者0
                else if (_value is bool boolValue)
                {
                    if (sourceType == EDataSource.SqlServer)
                        dataParameter.Value = boolValue ? 1 : 0;
                    else
                        dataParameter.Value = _value;
                }
                //时间类型
                else if (Value is TimeSpan time)
                {
                    if (sourceType == EDataSource.PostgreSql)
                        dataParameter.Value = time;
                    else
                        dataParameter.Value = time.ToString("c");
                }
                //GUID
                else if (Value is Guid guid)
                {
                    dataParameter.Value = guid.ToString("D").ToUpper();
                }
                //枚举类型
                else if (Value.GetType().IsEnum)
                {
                    var dataType = Value.GetType();
                    var realEnumType = dataType.GetFields(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault()
                        ?.FieldType;
                    if (realEnumType != null) dataParameter.Value = Convert.ChangeType(_value, realEnumType);
                }
                else
                {
                    dataParameter.Value = _value;
                }
            }

            sqlParameters.Add(dataParameter);

            return parameter;
        }
    }
}