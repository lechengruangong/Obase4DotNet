/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：QuerySql或者ChangeSql的参数化视图.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:57:10
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     QuerySql或者ChangeSql的参数化视图
    /// </summary>
    public class SqlParameterizedView
    {
        /// <summary>
        ///     初始化QuerySql或者ChangeSq
        /// </summary>
        /// <param name="sqlString">Sql语句</param>
        /// <param name="parameters">参数字典</param>
        private SqlParameterizedView(string sqlString, Dictionary<string, object> parameters)
        {
            SqlString = sqlString;
            Parameters = parameters;
        }

        /// <summary>
        ///     获取Sql语句
        /// </summary>
        public string SqlString { get; }

        /// <summary>
        ///     获取Sql语句所用的参数字典
        /// </summary>
        public Dictionary<string, object> Parameters { get; }

        /// <summary>
        ///     获取Sql的简单表示形式
        /// </summary>
        public string SimpleViewString => Parameters.Aggregate(SqlString,
            (current, parameter) => current.Replace(parameter.Key,
                string.IsNullOrEmpty(parameter.Value.ToString()) ? "''" : parameter.Value.ToString()));

        /// <summary>
        ///     获取QuerySql的参数化视图
        /// </summary>
        /// <param name="querySql">查询用Sql</param>
        /// <param name="dataSource">查询源类型</param>
        /// <returns></returns>
        public static SqlParameterizedView GetSqlParameterizedView(QuerySql querySql, EDataSource dataSource)
        {
            var querySqlStr =
                querySql.ToSql(dataSource, out var parameterList, new SqlParameteredViewParameterCreator());
            var parameters = parameterList.ToDictionary(sqlParameter => sqlParameter.ParameterName,
                sqlParameter => sqlParameter.Value);
            return new SqlParameterizedView(querySqlStr, parameters);
        }

        /// <summary>
        ///     获取ChangeSql的参数化视图
        /// </summary>
        /// <param name="changeSql">修改用Sql</param>
        /// <param name="dataSource">查询源类型</param>
        /// <returns></returns>
        public static SqlParameterizedView GetSqlParameterizedView(ChangeSql changeSql, EDataSource dataSource)
        {
            var querySqlStr =
                changeSql.ToSql(dataSource, out var parameterList, new SqlParameteredViewParameterCreator());
            if (changeSql.Source is MonomerSource monomerSource)
                //如果是空字符串 是因为调用了清除Symbol 此时应还原
                if (monomerSource.Symbol != null && string.IsNullOrEmpty(monomerSource.Symbol))
                    monomerSource.ResetSymbol();
            var parameters = parameterList.ToDictionary(sqlParameter => sqlParameter.ParameterName,
                sqlParameter => sqlParameter.Value);
            return new SqlParameterizedView(querySqlStr, parameters);
        }

        /// <summary>
        ///     当前查看用的简易参数
        /// </summary>
        private class SqlParameteredViewParameter : IDataParameter
        {
            /// <summary>
            ///     类型 无需关注
            /// </summary>
            public DbType DbType { get; set; } = default;

            /// <summary>
            ///     参数传入传出的类型 无需关注
            /// </summary>
            public ParameterDirection Direction { get; set; } = default;

            /// <summary>
            ///     是否可空 无需关注
            /// </summary>
            public bool IsNullable { get; } = default;

            /// <summary>
            ///     源列名 无需关注
            /// </summary>
            public string SourceColumn { get; set; } = default;

            /// <summary>
            ///     源版本 无需关注
            /// </summary>
            public DataRowVersion SourceVersion { get; set; } = default;

            /// <summary>
            ///     参数名
            /// </summary>
            public string ParameterName { get; set; }

            /// <summary>
            ///     参数值
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        ///     当前查看用的简易参数构造器
        /// </summary>
        private class SqlParameteredViewParameterCreator : IParameterCreator
        {
            /// <summary>
            ///     构造一个Sql语句参数。
            /// </summary>
            public IDataParameter Create()
            {
                return new SqlParameteredViewParameter();
            }
        }
    }
}