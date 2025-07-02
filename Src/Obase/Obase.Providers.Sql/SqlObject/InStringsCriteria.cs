/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字符串类型的IN条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:21:18
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     字符串类型的IN条件。
    /// </summary>
    public class InStringsCriteria : InCriteria<string>
    {
        /// <summary>
        ///     值
        /// </summary>
        private readonly IEnumerable<string> _values;

        /// <summary>
        ///     构造字符串类型的IN条件
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="relationOperator">操作符</param>
        /// <param name="values">值集合</param>
        public InStringsCriteria(string field, ERelationOperator relationOperator, IEnumerable<string> values) : base(
            field, relationOperator, values)
        {
            _values = values;
        }


        /// <summary>
        ///     构造字符串类型的IN条件
        /// </summary>
        /// <param name="sourecs">源</param>
        /// <param name="field">字段</param>
        /// <param name="relationOperator">操作符</param>
        /// <param name="values">值集合</param>
        public InStringsCriteria(string sourecs, string field, ERelationOperator relationOperator,
            IEnumerable<string> values) : base(sourecs, field, relationOperator, values)
        {
            _values = values;
        }

        /// <summary>
        ///     生成Sql语句中的值
        /// </summary>
        /// <returns></returns>
        protected override string GenerateSqlValue()
        {
            return _values.Aggregate("", (c, b) => c + "'" + b + "'" + ",").TrimEnd(',');
        }

        /// <summary>
        ///     生成value对应数据中的值并返回参数
        /// </summary>
        /// <param name="sourceType">数据源</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        protected override string GenerateSqlValue(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            var simpleInSqlParameter = new List<IDataParameter>();
            //值字符串
            var parameterStrList = new List<string>();
            var random =
                Guid.NewGuid().ToString().Replace("-", "")
                    .ToLower(); //Math.Abs(new TimeBasedIdGenerator().Next() + new Random().Next());
            //参数
            foreach (var value in _values)
            {
                var dataParameter = creator.Create();

                string parameter;
                switch (sourceType)
                {
                    case EDataSource.SqlServer:
                        parameter = $"@simpleInValue{random}";
                        break;
                    case EDataSource.Oracle:
                        parameter = $":simpleInValue{random}";
                        break;
                    case EDataSource.MySql:
                        parameter = $"?simpleInValue{random}";
                        break;
                    case EDataSource.Sqlite:
                        parameter = $"$simpleInValue{random}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }

                dataParameter.ParameterName = parameter;
                dataParameter.Value = value;

                simpleInSqlParameter.Add(dataParameter);
                parameterStrList.Add(parameter);
            }

            //字符串
            var resultStr = parameterStrList.Aggregate("", (c, b) => c + b + ",").TrimEnd(',');
            sqlParameters = new List<IDataParameter>();
            sqlParameters.AddRange(simpleInSqlParameter);

            return resultStr;
        }
    }
}