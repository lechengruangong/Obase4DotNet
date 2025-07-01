/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字段设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:10:30
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     字段设值器
    /// </summary>
    /// <typeparam name="TValue">字段类型</typeparam>
    public abstract class FieldSetter<TValue> : IFieldSetter
    {
        /// <summary>
        ///     字段
        /// </summary>
        protected readonly Field _field;


        /// <summary>
        ///     值
        /// </summary>
        protected readonly TValue _value;


        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        protected FieldSetter(string field, TValue value)
        {
            _field = new Field(field);
            _value = value;
        }

        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        protected FieldSetter(string source, string field, TValue value)
        {
            _field = string.IsNullOrEmpty(source) ? new Field(field) : new Field(source, field);
            _value = value;
        }

        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        protected FieldSetter(Field field, TValue value)
        {
            _field = field;
            _value = value;
        }

        /// <summary>
        ///     字段
        /// </summary>
        public TValue Value => _value;

        /// <summary>
        ///     值
        /// </summary>
        public Field Field => _field;

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public abstract string ToString(EDataSource sourceType);

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="field">返回字段名称</param>
        public abstract string ToString(out string field);

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public abstract string ToString(out string field, EDataSource sourceType);

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="parameters">参数化参数集合</param>
        /// <param name="creator">参数化参数建造器</param>
        /// <returns></returns>
        public abstract string ToString(out IDataParameter parameters, IParameterCreator creator);

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="creator">参数化参数建造器</param>
        /// <returns></returns>
        public abstract string ToString(out IDataParameter parameters, EDataSource sourceType,
            IParameterCreator creator);

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="parameters">参数化参数集合</param>
        /// <param name="creator">数据源类型</param>
        /// <returns></returns>
        public abstract string ToString(out string field, out IDataParameter parameters, IParameterCreator creator);

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数化参数集合</param>
        /// <param name="field">字段</param>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="creator">参数化参数建造器</param>
        /// <returns></returns>
        public abstract string ToString(out IDataParameter parameters, out string field, EDataSource sourceType,
            IParameterCreator creator);


        /// <summary>
        ///     根据不同的数据源返回字段字符串
        /// </summary>
        /// <param name="sourceType">数据源</param>
        /// <returns></returns>
        protected string GetFiledString(EDataSource sourceType)
        {
            //SqlServer [字段] MySql `字段`
            string field;
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    field = $"[{_field.Name}]";
                    break;
                }
                case EDataSource.PostgreSql:
                {
                    field = $"\"{_field.Name}\"";
                    break;
                }
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    field = $"`{_field.Name}`";
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }

            return field;
        }

        /// <summary>
        ///     根据不同的数据源返回参数和参数名字符串
        /// </summary>
        /// <param name="parameters">参数化参数集合</param>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="valueStr">值字符串表示</param>
        /// <param name="creator">参数化参数建造器</param>
        /// <returns></returns>
        protected virtual string GetParameters(out IDataParameter parameters, EDataSource sourceType, object valueStr,
            IParameterCreator creator)
        {
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
                    parameter = $"@fieldSetter{random}";
                    break;
                case EDataSource.Oracle:
                    parameter = $":fieldSetter{random}";
                    break;
                case EDataSource.MySql:
                    parameter = $"?fieldSetter{random}";
                    break;
                case EDataSource.Sqlite:
                    parameter = $"$fieldSetter{random}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
            }

            parameters = creator.Create();
            parameters.ParameterName = parameter;
            //非空 加入参数
            parameters.Value = valueStr.ToString().Trim().ToLower() != "null" ? valueStr : DBNull.Value;

            return parameter;
        }
    }

    /// <summary>
    ///     表示字段设值器。
    /// </summary>
    public class FieldSetter : IFieldSetter
    {
        /// <summary>
        ///     要设置其值的字段。
        /// </summary>
        private readonly Field _field;

        /// <summary>
        ///     表示字段值的表达式。
        /// </summary>
        private readonly Expression _value;

        /// <summary>
        ///     使用字段名称和值表达式创建FieldSetter实例。
        /// </summary>
        /// <param name="field">字段名称。</param>
        /// <param name="value">值表达式。</param>
        public FieldSetter(string field, Expression value)
        {
            _field = new Field(field);
            _value = value;
        }

        /// <summary>
        ///     使用源名称、字段名称和值表达式创建FieldSetter实例。
        /// </summary>
        /// <param name="source">源名称。</param>
        /// <param name="field">字段名称。</param>
        /// <param name="value">值表达式。</param>
        public FieldSetter(string source, string field, Expression value)
        {
            _field = string.IsNullOrEmpty(source) ? new Field(field) : new Field(source, field);
            _value = value;
        }

        /// <summary>
        ///     使用字段实例和值表达式创建FieldSetter实例。
        /// </summary>
        /// <param name="field">字段。</param>
        /// <param name="value">值表达式。</param>
        public FieldSetter(Field field, Expression value)
        {
            _field = field;
            _value = value;
        }

        /// <summary>
        ///     获取表示字段值的表达式。
        /// </summary>
        public Expression Value => _value;

        /// <summary>
        ///     获取要设置其值的字段。
        /// </summary>
        public Field Field => _field;

        /// <summary>
        ///     针对指定的数据源类型，将字段设值器实例转换成字符串表示形式，该字符串将用于Update Sql的Set字句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public string ToString(EDataSource sourceType)
        {
            var result =
                $"{_field.ToString(sourceType)} = {(_value == null ? "null" : $"'{_value.ToString(sourceType)}'")}";
            return result;
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="field">返回字段名称</param>
        public string ToString(out string field)
        {
            return ToString(out field, EDataSource.SqlServer);
        }

        /// <summary>
        ///     针对指定的数据源类型，将字段设值器实例转换成字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="field">返回字段名称</param>
        /// <param name="sourceType">数据源类型。</param>
        public string ToString(out string field, EDataSource sourceType)
        {
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                    field = $"[{_field.Name}]";
                    break;
                case EDataSource.PostgreSql:
                    field = $"\"{_field.Name}\"";
                    break;
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                    field = $"`{_field.Name}`";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
            }

            return Value.ToString(sourceType);
        }

        /// <summary>
        ///     字符串表示形式
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public string ToString(out IDataParameter parameters, IParameterCreator creator)
        {
            return ToString(out parameters, EDataSource.SqlServer, creator);
        }

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="sourceType">数据源</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public string ToString(out IDataParameter parameters, EDataSource sourceType, IParameterCreator creator)
        {
            string valueStr;
            if (_value == null)
            {
                valueStr = "null";
                parameters = null;
            }
            else
            {
                valueStr = $"{_value.ToString(sourceType, out var sqlParameters, creator)}";
                parameters = sqlParameters.Count > 0 ? sqlParameters[0] : null;
            }

            var result =
                $"{_field.ToString(sourceType)} = {valueStr}";

            return result;
        }

        /// <summary>
        ///     字符串表示形式
        /// </summary>
        /// <param name="field">返回字段名称</param>
        /// <param name="parameters">返回字符串中的参数及其值。</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public string ToString(out string field, out IDataParameter parameters, IParameterCreator creator)
        {
            return ToString(out parameters, out field, EDataSource.SqlServer, creator);
        }

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="field">字段</param>
        /// <param name="sourceType">数据源</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public string ToString(out IDataParameter parameters, out string field, EDataSource sourceType,
            IParameterCreator creator)
        {
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                    field = $"[{_field.Name}]";
                    break;
                case EDataSource.PostgreSql:
                    field = $"\"{_field.Name}\"";
                    break;
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                    field = $"`{_field.Name}`";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
            }

            var valueStr = Value.ToString(sourceType, out var sqlParameters, creator);

            parameters = sqlParameters.Count > 0 ? sqlParameters[0] : null;

            return valueStr;
        }
    }
}