/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：时间段设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:50:47
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     时间字段设值器
    /// </summary>
    public class TimeSpanFieldSetter : FieldSetter<TimeSpan>
    {
        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public TimeSpanFieldSetter(string field, TimeSpan value) : base(field, value)
        {
        }

        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="source"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public TimeSpanFieldSetter(string source, string field, TimeSpan value) : base(source, field, value)
        {
        }

        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public TimeSpanFieldSetter(Field field, TimeSpan value) : base(field, value)
        {
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            return $"{_field.ToString(sourceType)} ='{_value:c}'";
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="field">返回字段名称</param>
        public override string ToString(out string field)
        {
            return ToString(out field, EDataSource.SqlServer);
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parameters"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public override string ToString(out string field, out IDataParameter parameters, IParameterCreator creator)
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
        public override string ToString(out IDataParameter parameters, out string field, EDataSource sourceType,
            IParameterCreator creator)
        {
            field = GetFiledString(sourceType);

            var valueStr = _value.ToString("c");

            return GetParameters(out parameters, sourceType, valueStr, creator);
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="field"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override string ToString(out string field, EDataSource sourceType)
        {
            field = GetFiledString(sourceType);
            return _value.ToString("c");
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public override string ToString(out IDataParameter parameters, IParameterCreator creator)
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
        public override string ToString(out IDataParameter parameters, EDataSource sourceType,
            IParameterCreator creator)
        {
            var valueStr = _value.ToString("c");

            var parameter = GetParameters(out parameters, sourceType, valueStr, creator);

            return $"{_field.ToString(sourceType)} = {parameter}";
        }

        /// <summary>
        ///     根据不同的数据源返回参数和参数名字符串
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="sourceType"></param>
        /// <param name="valueStr"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        protected override string GetParameters(out IDataParameter parameters, EDataSource sourceType, object valueStr,
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
            var aNull = !valueStr.ToString().Trim().Equals("null");
            parameters.Value = aNull ? valueStr : null;
            if (sourceType == EDataSource.PostgreSql && aNull) parameters.Value = Value;

            return parameter;
        }
    }
}