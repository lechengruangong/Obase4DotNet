/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：适用于Sql服务器的存储结构映射提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:59:33
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Obase.Core;
using Obase.Providers.Sql.Common;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     适用于Sql服务器的存储结构映射提供程序。
    /// </summary>
    public class SqlStorageStructMappingProvider : IStorageStructMappingProvider
    {
        /// <summary>
        ///     执行器
        /// </summary>
        private readonly ISqlExecutor _executor;

        /// <summary>
        ///     构造适用于Sql服务器的存储结构映射提供程序
        /// </summary>
        /// <param name="executor">SQL执行器</param>
        public SqlStorageStructMappingProvider(ISqlExecutor executor)
        {
            _executor = executor ?? throw new ArgumentException("存储结构映射的SQL执行器不可为空.");
        }

        /// <summary>
        ///     向指定的表追加字段。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">要追加的字段。</param>
        public void AppendField(string tableName, Field[] fields)
        {
            foreach (var field in fields)
            {
                string sql;
                //是否可空
                var nullable = field.Nullable;
                switch (_executor.SourceType)
                {
                    case EDataSource.Oracle:
                    case EDataSource.Oledb:
                    case EDataSource.Other:
                        throw new ArgumentException($"结构映射暂不支持{_executor.SourceType}");
                    case EDataSource.SqlServer:
                        sql =
                            $"ALTER TABLE [{tableName}] ADD [{field.Name}] [{SqlUtils.GetSqlServerDbType(field.DataType.ClrType)}] {(nullable ? "NULL" : "NOT NULL")}";
                        //实际上只有字符串类型和decimal类型需要长度 其他类型的长度与具体可以存储的长度无关
                        if (IsTypeNeedLength(field.DataType.ClrType, field, out var sqlServerFieldText))
                            sql =
                                $"ALTER TABLE {tableName} ADD {field.Name} {sqlServerFieldText} {(nullable ? "NULL" : "NOT NULL")}";
                        break;
                    case EDataSource.Sqlite:
                        sql =
                            $"ALTER TABLE `{tableName}` ADD COLUMN `{field.Name}` {SqlUtils.GetSqliteDbType(field.DataType.ClrType)} {(nullable ? "NULL" : "NOT NULL")}";
                        break;
                    case EDataSource.MySql:
                        sql =
                            $"ALTER TABLE `{tableName}` ADD COLUMN `{field.Name}` {SqlUtils.GetMySqlDbType(field.DataType.ClrType)} {(nullable ? "DEFAULT NULL " : "NOT NULL")}";
                        //实际上只有字符串类型和decimal类型需要长度 其他类型的长度与具体可以存储的长度无关
                        if (IsTypeNeedLength(field.DataType.ClrType, field, out var mySqlFieldText))
                            sql =
                                $"ALTER TABLE `{tableName}` ADD COLUMN `{field.Name}` {mySqlFieldText} {(nullable ? "DEFAULT NULL " : "NOT NULL")} ";
                        break;
                    case EDataSource.PostgreSql:
                        sql =
                            $"ALTER TABLE \"{tableName}\" ADD \"{field.Name}\" {SqlUtils.GetPostgreSqlDbType(field.DataType.ClrType)} {(nullable ? "DEFAULT  NULL" : "NOT NULL")}";
                        //实际上只有字符串类型和decimal类型需要长度  其他类型的长度与具体可以存储的长度无关
                        if (IsTypeNeedLength(field.DataType.ClrType, field, out var postgreSqlServerFieldText))
                            sql =
                                $"ALTER TABLE \"{tableName}\" ADD  \"{field.Name}\" {postgreSqlServerFieldText} {(nullable ? "DEFAULT NULL" : "NOT NULL")}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"未知的数据源类型{_executor.SourceType}");
                }

                _executor.ExecuteScalar(sql, Array.Empty<IDataParameter>());
            }
        }

        /// <summary>
        ///     索引一致性检查，确认表的既有索引与指定索引一致。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="keyFields">标识属性。</param>
        public bool CheckKey(string tableName, string[] keyFields)
        {
            return IndexExist(tableName, keyFields).All(p => p);
        }

        /// <summary>
        ///     创建索引。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">索引字段的名称序列。</param>
        public void CreateIndex(string tableName, string[] fields)
        {
            string sql;
            var name = $"ogi_{tableName}_{string.Join("_", fields)}";
            if (name.Length > 64)
                name = name.Substring(0, 64);
            switch (_executor.SourceType)
            {
                case EDataSource.Oracle:
                case EDataSource.Oledb:
                case EDataSource.Other:
                    throw new ArgumentException($"结构映射暂不支持{_executor.SourceType}");
                case EDataSource.SqlServer:
                    sql =
                        $"CREATE INDEX {name} ON [{tableName}] ({string.Join(",", fields.Select(p => $"[{p}]").ToList())})";
                    break;
                case EDataSource.Sqlite:
                    sql =
                        $"CREATE INDEX '{name}' ON `{tableName}` ({string.Join(",", fields.Select(p => $"`{p}`").ToList())})";
                    break;
                case EDataSource.MySql:
                    sql =
                        $"CREATE INDEX {name} ON `{tableName}` ({string.Join(",", fields.Select(p => $"`{p}`").ToList())})";
                    break;
                case EDataSource.PostgreSql:
                    sql =
                        $"CREATE INDEX {name} ON \"{tableName}\" ({string.Join(",", fields.Select(p => $"\"{p}\"").ToList())})";
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"未知的数据源类型{_executor.SourceType}");
            }

            _executor.ExecuteScalar(sql, Array.Empty<IDataParameter>());
        }

        /// <summary>
        ///     创建表。
        /// </summary>
        /// <param name="name">表名。</param>
        /// <param name="fields">表的字段。</param>
        /// <param name="keyFields">标识字段的名称序列。</param>
        public void CreateTable(string name, Field[] fields, string[] keyFields)
        {
            var sqlBuilder = new StringBuilder();
            switch (_executor.SourceType)
            {
                case EDataSource.Oracle:
                case EDataSource.Oledb:
                case EDataSource.Other:
                    throw new ArgumentException($"结构映射暂不支持{_executor.SourceType}");
                case EDataSource.SqlServer:
                {
                    sqlBuilder.Append($"CREATE TABLE [{name}](");
                    //联合主键
                    if (keyFields.Length > 1)
                    {
                        foreach (var field in fields)
                        {
                            //是否可空
                            var nullable = GetNullable(keyFields, field);
                            //实际上只有字符串类型和decimal类型需要长度 其他类型的长度与具体可以存储的长度无关
                            var filedText = IsTypeNeedLength(field.DataType.ClrType, field, out var sqlServerFieldText)
                                ? sqlServerFieldText
                                : $"[{SqlUtils.GetSqlServerDbType(field.DataType.ClrType)}]";

                            sqlBuilder.Append(
                                keyFields.Contains(field.Name)
                                    ? $"[{field.Name}] {filedText} {(field.IsSelfIncreasing ? "IDENTITY(1,1)" : "")} {(nullable ? "NULL" : "NOT NULL")},"
                                    : $"[{field.Name}] {filedText} {(nullable ? "NULL" : "NOT NULL")},");
                        }

                        sqlBuilder.Append($"PRIMARY KEY ({string.Join(",", keyFields)})");
                    }
                    else
                    {
                        foreach (var field in fields)
                        {
                            //是否可空
                            var nullable = GetNullable(keyFields, field);
                            //实际上只有字符串类型和decimal类型需要长度 其他类型的长度与具体可以存储的长度无关
                            var filedText = IsTypeNeedLength(field.DataType.ClrType, field, out var sqlServerFieldText)
                                ? sqlServerFieldText
                                : $"[{SqlUtils.GetSqlServerDbType(field.DataType.ClrType)}]";
                            sqlBuilder.Append(
                                keyFields.Contains(field.Name)
                                    ? $"[{field.Name}] {filedText} PRIMARY KEY {(field.IsSelfIncreasing ? "IDENTITY(1,1)" : "")} {(nullable ? "NULL" : "NOT NULL")},"
                                    : $"[{field.Name}] {filedText} {(nullable ? "NULL" : "NOT NULL")},");
                        }

                        sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
                    }

                    sqlBuilder.Append(")");
                }
                    break;
                case EDataSource.Sqlite:
                {
                    sqlBuilder.Append($"CREATE TABLE `{name}`(");
                    //联合主键
                    if (keyFields.Length > 1)
                    {
                        foreach (var field in fields)
                        {
                            //是否可空
                            var nullable = GetNullable(keyFields, field);
                            var sqliteText = SqlUtils.GetSqliteDbType(field.DataType.ClrType);
                            sqlBuilder.Append(
                                keyFields.Contains(field.Name)
                                    ? $"`{field.Name}` {sqliteText} {(field.IsSelfIncreasing ? "AUTOINCREMENT" : "")} {(nullable ? "NULL" : "NOT NULL")},"
                                    : $"`{field.Name}` {sqliteText} {(nullable ? "NULL" : "NOT NULL")},");
                        }

                        sqlBuilder.Append($"PRIMARY KEY ({string.Join(",", keyFields)})");
                    }
                    else
                    {
                        foreach (var field in fields)
                        {
                            //是否可空
                            var nullable = GetNullable(keyFields, field);
                            var sqliteText = SqlUtils.GetSqliteDbType(field.DataType.ClrType);
                            sqlBuilder.Append(
                                keyFields.Contains(field.Name)
                                    ? $"`{field.Name}` {sqliteText} PRIMARY KEY {(field.IsSelfIncreasing ? "AUTOINCREMENT" : "")} {(nullable ? "NULL" : "NOT NULL")},"
                                    : $"`{field.Name}` {sqliteText} {(nullable ? "NULL" : "NOT NULL")},");
                        }

                        sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
                    }

                    sqlBuilder.Append(");");
                    sqlBuilder.Append(
                        $"CREATE INDEX 'obase_gen_index_{name}_{string.Join("_", keyFields)}' ON `{name}` ({string.Join(",", keyFields)})");
                }
                    break;
                case EDataSource.MySql:
                {
                    sqlBuilder.Append($"CREATE TABLE `{name}`(");
                    foreach (var field in fields)
                    {
                        //是否可空
                        var nullable = GetNullable(keyFields, field);
                        var filedText = IsTypeNeedLength(field.DataType.ClrType, field, out var mysqlFieldText)
                            ? mysqlFieldText
                            : SqlUtils.GetMySqlDbType(field.DataType.ClrType);
                        sqlBuilder.Append(
                            $"`{field.Name}` {filedText} {(nullable ? "DEFAULT NULL " : "NOT NULL")} {(field.IsSelfIncreasing ? "AUTO_INCREMENT" : "")},");
                    }

                    sqlBuilder.Append("PRIMARY KEY(");
                    foreach (var field in fields)
                        if (keyFields.Contains(field.Name))
                            sqlBuilder.Append($"`{field.Name}`,");
                    sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
                    sqlBuilder.Append(")");
                    sqlBuilder.Append(") ENGINE=InnoDB DEFAULT CHARACTER SET = utf8mb4 COLLATE = utf8mb4_bin;");
                }
                    break;
                case EDataSource.PostgreSql:
                {
                    sqlBuilder.Append("CREATE TABLE \"").Append(name).Append("\"(");
                    foreach (var field in fields)
                    {
                        //是否可空
                        var nullable = GetNullable(keyFields, field);

                        sqlBuilder.Append("\"").Append(field.Name).Append("\"");

                        string filedText;
                        if (field.IsSelfIncreasing)
                            filedText = SqlUtils.GetPostgreSqlAutoIncreaseDbType(field.DataType.ClrType);
                        else
                            //实际上只有字符串类型和decimal类型需要长度 其他类型的长度与具体可以存储的长度无关
                            filedText = IsTypeNeedLength(field.DataType.ClrType, field, out var postgresqlFieldText)
                                ? postgresqlFieldText
                                : SqlUtils.GetPostgreSqlDbType(field.DataType.ClrType);
                        sqlBuilder.Append(filedText);
                        sqlBuilder.Append(nullable ? " DEFAULT  NULL" : " NOT NULL");
                        sqlBuilder.Append(",");
                    }

                    sqlBuilder.Append("PRIMARY KEY(");
                    foreach (var field in fields)
                        if (keyFields.Contains(field.Name))
                            sqlBuilder.Append("\"").Append(field.Name).Append("\",");
                    sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
                    sqlBuilder.Append(")").Append(")");
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"未知的数据源类型{_executor.SourceType}");
            }

            _executor.ExecuteScalar(sqlBuilder.ToString(), Array.Empty<IDataParameter>());
        }

        /// <summary>
        ///     扩大指定字段的长度。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">要增加宽度的字段。</param>
        public void ExpandField(string tableName, Field[] fields)
        {
            //Sqlite 无字段长度
            if (_executor.SourceType != EDataSource.Sqlite)
                foreach (var field in fields)
                {
                    string sql;
                    switch (_executor.SourceType)
                    {
                        case EDataSource.Oracle:
                        case EDataSource.Oledb:
                        case EDataSource.Other:
                            throw new ArgumentException($"结构映射暂不支持{_executor.SourceType}");
                        case EDataSource.SqlServer:
                            sql =
                                $"ALTER TABLE `{tableName}` ALTER COLUMN `{field.Name}` {SqlUtils.GetSqlServerDbType(field.DataType.ClrType)}({field.Length / 8})";
                            break;
                        case EDataSource.MySql:
                            sql =
                                $"ALTER TABLE `{tableName}` MODIFY COLUMN `{field.Name}` {SqlUtils.GetMySqlDbType(field.DataType.ClrType)}({field.Length / 8})";
                            break;
                        case EDataSource.PostgreSql:
                            sql =
                                $"ALTER TABLE {tableName} MODIFY COLUMN {field.Name} {SqlUtils.GetPostgreSqlDbType(field.DataType.ClrType)}({field.Length / 8})";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"未知的数据源类型{_executor.SourceType}");
                    }

                    //实际上只有字符串类型和decimal类型需要长度 其他类型的长度与具体可以存储的长度无关
                    if (IsTypeNeedLength(field.DataType.ClrType, field, out var fieldText))
                        sql = _executor.SourceType == EDataSource.SqlServer
                            ? $"ALTER TABLE {tableName} MODIFY COLUMN `{field.Name}` {fieldText}"
                            : $"ALTER TABLE {tableName} ALTER COLUMN [{field.Name}] {fieldText}";

                    _executor.ExecuteScalar(sql, Array.Empty<IDataParameter>());
                }
        }

        /// <summary>
        ///     探测指定的字段是否已存在。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">待检测的字段。</param>
        /// <param name="lackOnes">返回缺少的字段。</param>
        /// <param name="shorterOnes">返回长度不足的字段。</param>
        public void FieldExist(string tableName, Field[] fields, out Field[] lackOnes, out Field[] shorterOnes)
        {
            var lack = new List<Field>();
            //var shorter = new List<Field>();

            foreach (var field in fields)
            {
                string sql;
                switch (_executor?.SourceType)
                {
                    case EDataSource.Oracle:
                    case EDataSource.Oledb:
                    case EDataSource.Other:
                        throw new ArgumentException($"结构映射暂不支持{_executor?.SourceType}");
                    case EDataSource.SqlServer:
                        sql = $"SELECT TOP 1 [{tableName}].[{field.Name}] FROM [{tableName}]";
                        break;
                    case EDataSource.MySql:
                        sql = $"SELECT `{tableName}`.`{field.Name}` FROM `{tableName}` LIMIT 0,1";
                        break;
                    case EDataSource.Sqlite:
                        sql = $"SELECT `{tableName}`.`{field.Name}` FROM `{tableName}` LIMIT 0,1";
                        break;
                    case EDataSource.PostgreSql:
                        sql = $"SELECT \"{tableName}\".\"{field.Name}\" FROM \"{tableName}\" LIMIT 0 OFFSET 1";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"未知的数据源类型{_executor?.SourceType}");
                }

                IDataReader reader = null;
                try
                {
                    reader = _executor.ExecuteReader(sql, Array.Empty<IDataParameter>());
                    if (reader.Read())
                    {
                        //字段长度无法查询
                        //shorter.Add(filed)
                    }
                }
                catch
                {
                    lack.Add(field);
                }
                finally
                {
                    reader?.Close();
                    reader?.Dispose();
                    _executor?.CloseConnection();
                }
            }

            lackOnes = lack.ToArray();
            shorterOnes = Array.Empty<Field>();
        }

        /// <summary>
        ///     探测指定的索引是否已存在。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">索引字段的名称序列。</param>
        public bool[] IndexExist(string tableName, string[] fields)
        {
            var result = new bool[fields.Length];
            var i = 0;
            foreach (var field in fields)
            {
                string sql;
                switch (_executor?.SourceType)
                {
                    case EDataSource.Oracle:
                    case EDataSource.Oledb:
                    case EDataSource.Other:
                        throw new ArgumentException($"结构映射暂不支持{_executor?.SourceType}");
                    case EDataSource.SqlServer:
                        sql = $"sp_helpindex '{tableName}'";
                        break;
                    case EDataSource.MySql:
                        sql = $"SHOW INDEX FROM `{tableName}` WHERE column_name = '{field}'";
                        break;
                    case EDataSource.Sqlite:
                        sql =
                            $"select * From sqlite_master where type = 'index' and tbl_name = '{tableName}' and sql like '%{field}%'";
                        break;
                    case EDataSource.PostgreSql:
                        sql = $"Select indexdef FROM pg_indexes Where  tablename = '{tableName}'";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"未知的数据源类型{_executor?.SourceType}");
                }

                IDataReader reader = null;
                try
                {
                    reader = _executor.ExecuteReader(sql, Array.Empty<IDataParameter>());
                    if (_executor.SourceType == EDataSource.SqlServer)
                    {
                        while (reader.Read())
                        {
                            var key = reader["index_keys"];
                            if (key != null && key.ToString().ToLower().Contains(field.ToLower()))
                                result[i] = true;
                        }
                    }
                    else if (_executor.SourceType == EDataSource.PostgreSql)
                    {
                        while (reader.Read())
                        {
                            var key = reader["indexdef"];
                            if (key != null && key.ToString().ToLower().Contains(field.ToLower()))
                                result[i] = true;
                        }
                    }
                    else
                    {
                        if (reader.Read())
                            result[i] = true;
                    }
                }
                finally
                {
                    reader?.Close();
                    reader?.Dispose();
                    _executor?.CloseConnection();
                }

                i++;
            }

            return result;
        }

        /// <summary>
        ///     探测指定的表是否已存在。
        /// </summary>
        /// <param name="name">表名。</param>
        public bool TableExist(string name)
        {
            string sql;
            switch (_executor.SourceType)
            {
                case EDataSource.Oracle:
                case EDataSource.Oledb:
                case EDataSource.Other:
                    throw new ArgumentException($"结构映射暂不支持{_executor.SourceType}");
                case EDataSource.SqlServer:
                    sql = $"SELECT TOP 1 1 FROM [{name}]";
                    break;
                case EDataSource.MySql:
                    sql = $"SELECT 1 FROM `{name}` LIMIT 0,1";
                    break;
                case EDataSource.Sqlite:
                    sql = $"SELECT 1 FROM `{name}` LIMIT 0,1";
                    break;
                case EDataSource.PostgreSql:
                    sql = $"SELECT 1 FROM \"{name}\"  LIMIT 0 OFFSET 1";
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"未知的数据源类型{_executor.SourceType}");
            }

            try
            {
                _executor.ExecuteScalar(sql, Array.Empty<IDataParameter>());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     是否可空
        /// </summary>
        /// <param name="keyFields">主键</param>
        /// <param name="field">当前字段</param>
        /// <returns></returns>
        private bool GetNullable(string[] keyFields, Field field)
        {
            //是否可空
            var nullable = field.Nullable;

            //如果是主键 必然不可空
            if (keyFields.Contains(field.Name)) nullable = false;

            return nullable;
        }

        /// <summary>
        ///     判断类型是否需要长度
        ///     目前仅有映射为string的类型 和 decimal类型 需要长度
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="field">字段</param>
        /// <param name="fieldText">字段类型和长度文本</param>
        /// <returns></returns>
        private bool IsTypeNeedLength(Type type, Field field, out string fieldText)
        {
            var dataSource = _executor.SourceType;
            //字符串类型
            if (type == typeof(string) || type == typeof(Guid))
            {
                var length = field.Length / 8 == 0 ? 40 : field.Length / 8;
                switch (dataSource)
                {
                    case EDataSource.Oracle:
                    case EDataSource.Other:
                    case EDataSource.Oledb:
                        throw new ArgumentException($"结构映射暂不支持{dataSource}");
                    case EDataSource.MySql:
                        fieldText = length > 255
                            ? "Text"
                            : $"{SqlUtils.GetMySqlDbType(field.DataType.ClrType)}({length})";
                        break;
                    case EDataSource.Sqlite:
                        fieldText = $"{SqlUtils.GetSqliteDbType(field.DataType.ClrType)}";
                        break;
                    case EDataSource.PostgreSql:
                        fieldText = length > 255
                            ? "Text"
                            : $"{SqlUtils.GetPostgreSqlDbType(field.DataType.ClrType)}({length})";
                        break;
                    case EDataSource.SqlServer:
                        fieldText = length > 500
                            ? "[Text]"
                            : $"[{SqlUtils.GetSqlServerDbType(field.DataType.ClrType)}]({length})";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, null);
                }

                return true;
            }

            //十进制数类型
            if (type == typeof(decimal))
            {
                var precision = field.Precision;

                switch (dataSource)
                {
                    case EDataSource.Oracle:
                    case EDataSource.Other:
                    case EDataSource.Oledb:
                        throw new ArgumentException($"结构映射暂不支持{dataSource}");
                    case EDataSource.MySql:
                        fieldText = $"{SqlUtils.GetMySqlDbType(field.DataType.ClrType)}(65,{precision})";
                        break;
                    case EDataSource.Sqlite:
                        fieldText = $"{SqlUtils.GetMySqlDbType(field.DataType.ClrType)}";
                        break;
                    case EDataSource.SqlServer:
                        fieldText = $"[{SqlUtils.GetSqlServerDbType(field.DataType.ClrType)}](38,{precision})";
                        break;
                    case EDataSource.PostgreSql:
                        fieldText = $"{SqlUtils.GetPostgreSqlDbType(field.DataType.ClrType)}(65,{precision})";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, null);
                }

                return true;
            }

            //char类型
            if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(char))
            {
                var length = field.Length / 8 == 0 ? 2 : field.Length / 8;
                switch (dataSource)
                {
                    case EDataSource.Oracle:
                    case EDataSource.Other:
                    case EDataSource.Oledb:
                        throw new ArgumentOutOfRangeException(nameof(dataSource), "结构映射暂不支持" + dataSource);
                    case EDataSource.PostgreSql:
                        fieldText = $"{SqlUtils.GetPostgreSqlDbType(field.DataType.ClrType)}({length})";
                        return true;
                    case EDataSource.MySql:
                    case EDataSource.Sqlite:
                    case EDataSource.SqlServer:
                        fieldText = string.Empty;
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, null);
                }
            }

            fieldText = string.Empty;
            return false;
        }
    }
}