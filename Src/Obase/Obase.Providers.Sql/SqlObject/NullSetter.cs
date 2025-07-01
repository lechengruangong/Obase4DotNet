/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Null设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:25:17
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     Null设值器，用于将字段的值设值为NULL。
    /// </summary>
    public class NullSetter : IFieldSetter
    {
        /// <summary>
        ///     创建NullSetter实例。
        /// </summary>
        /// <param name="field">表示要为其设置值的字段。</param>
        public NullSetter(Field field)
        {
            Field = field;
        }

        /// <summary>
        ///     创建NullSetter实例。
        /// </summary>
        /// <param name="fieldName">表示要为其设置值的字段的名称。</param>
        public NullSetter(string fieldName)
        {
            Field = new Field(fieldName);
        }

        /// <summary>
        ///     字段
        /// </summary>
        public Field Field { get; }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    return $" [{Field.Name}] = null ";
                }
                case EDataSource.PostgreSql:
                {
                    return $" \"{Field.Name}\" = null ";
                }
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    return $" `{Field.Name}` = null ";
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }
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
        ///     转换Wie字符串表示形式
        /// </summary>
        /// <param name="field"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToString(out string field, EDataSource sourceType)
        {
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    field = $"[{Field.Name}]";
                    break;
                }
                case EDataSource.PostgreSql:
                {
                    field = $" \"{Field.Name}\" = null ";
                    break;
                }
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    field = $"`{Field.Name}`";
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }

            return " null ";
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="creator"></param>
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
        /// <param name="creator"></param>
        /// <returns></returns>
        public string ToString(out IDataParameter parameters, EDataSource sourceType, IParameterCreator creator)
        {
            var paramete = GetParameters(out parameters, sourceType, creator);

            return $"{Field.ToString(sourceType)} = {paramete}";
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parameters"></param>
        /// <param name="creator"></param>
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
        /// <param name="creator"></param>
        /// <returns></returns>
        public string ToString(out IDataParameter parameters, out string field, EDataSource sourceType,
            IParameterCreator creator)
        {
            field = GetFiledString(sourceType);

            return GetParameters(out parameters, sourceType, creator);
        }

        /// <summary>
        ///     根据不同的数据源返回字段字符串
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetFiledString(EDataSource sourceType)
        {
            //SqlServer [字段] MySql `字段`
            string field;
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    field = $"[{Field.Name}]";
                    break;
                }
                case EDataSource.PostgreSql:
                {
                    field = $"\"{Field.Name}\"";
                    break;
                }
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    field = $"`{Field.Name}`";
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
        /// <param name="parameters"></param>
        /// <param name="sourceType"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        private string GetParameters(out IDataParameter parameters, EDataSource sourceType, IParameterCreator creator)
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
                    parameter = $"@nullSetter{random}";
                    break;
                case EDataSource.Oracle:
                    parameter = $":nullSetter{random}";
                    break;
                case EDataSource.MySql:
                    parameter = $"?nullSetter{random}";
                    break;
                case EDataSource.Sqlite:
                    parameter = $"$nullSetter{random}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
            }

            parameters = creator.Create();
            parameters.ParameterName = parameter;
            parameters.Value = DBNull.Value;

            return parameter;
        }
    }
}