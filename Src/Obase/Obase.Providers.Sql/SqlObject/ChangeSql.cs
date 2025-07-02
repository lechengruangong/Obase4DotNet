/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：修改Sql语句的对象化表示.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:37:03
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Obase.Providers.Sql.Common;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     修改Sql语句的对象化表示。“修改Sql”是指Update、Insert、Delete三种Sql语句。
    /// </summary>
    public class ChangeSql : SqlBase
    {
        /// <summary>
        ///     字段设值器。
        ///     key:字段名称
        /// </summary>
        private readonly Dictionary<string, IFieldSetter> _fieldSetters = new Dictionary<string, IFieldSetter>();

        /// <summary>
        ///     修改类型
        /// </summary>
        private EChangeType _changeType;

        /// <summary>
        ///     要修改的源，参与构建Delete子句、Update子句和Insert Into子句。
        /// </summary>
        private MonomerSource _targetSource;

        /// <summary>
        ///     无参创建修改Sql语句
        /// </summary>
        public ChangeSql()
        {
        }

        /// <summary>
        ///     创建修改Sql语句，指定源、修改类型和筛选条件。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="changeType">修改类型</param>
        /// <param name="criteria">筛选条件</param>
        public ChangeSql(ISource source, EChangeType changeType, ICriteria criteria) : base(source, criteria,
            GetSqlTypeFromChangeType(changeType))
        {
            _changeType = changeType;
        }

        /// <summary>
        ///     创建修改Sql语句，指定源、修改类型和筛选条件。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="changeType">修改类型</param>
        /// <param name="criteria">筛选条件</param>
        public ChangeSql(string source, EChangeType changeType, ICriteria criteria) : base(ConstructSource(source),
            criteria, GetSqlTypeFromChangeType(changeType))
        {
            _changeType = changeType;
        }

        /// <summary>
        ///     创建修改Sql语句，指定源、修改类型。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="changeType">修改类型</param>
        public ChangeSql(ISource source, EChangeType changeType) : base(source, GetSqlTypeFromChangeType(changeType))
        {
            _changeType = changeType;
        }

        /// <summary>
        ///     创建修改Sql语句，指定源、修改类型。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="changeType">修改类型</param>
        public ChangeSql(string source, EChangeType changeType) : base(ConstructSource(source),
            GetSqlTypeFromChangeType(changeType))
        {
            _changeType = changeType;
        }

        /// <summary>
        ///     创建用于插入的Sql语句。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="fieldSetters">字段设值器集合</param>
        public ChangeSql(string source, List<IFieldSetter> fieldSetters) : base(ConstructSource(source),
            GetSqlTypeFromChangeType(EChangeType.Insert))
        {
            AppendFieldSetter(fieldSetters);
            _changeType = EChangeType.Insert;
        }

        /// <summary>
        ///     创建用于插入的Sql语句。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="fieldSetters">字段设值器集合</param>
        public ChangeSql(ISource source, List<IFieldSetter> fieldSetters) : base(source,
            GetSqlTypeFromChangeType(EChangeType.Insert))
        {
            AppendFieldSetter(fieldSetters);
            _changeType = EChangeType.Insert;
        }

        /// <summary>
        ///     创建用于更新的Sql语句。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="fieldSetters">字段设值器集合</param>
        public ChangeSql(string source, ICriteria criteria, List<IFieldSetter> fieldSetters) : base(
            ConstructSource(source), criteria, GetSqlTypeFromChangeType(EChangeType.Update))
        {
            AppendFieldSetter(fieldSetters);
            _changeType = EChangeType.Update;
        }

        /// <summary>
        ///     创建用于更新的Sql语句。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="fieldSetters">字段设值器集合</param>
        public ChangeSql(ISource source, ICriteria criteria, List<IFieldSetter> fieldSetters) : base(source, criteria,
            GetSqlTypeFromChangeType(EChangeType.Update))
        {
            AppendFieldSetter(fieldSetters);
            _changeType = EChangeType.Update;
        }

        /// <summary>
        ///     创建用于删除的Sql语句。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="criteria">筛选条件</param>
        public ChangeSql(string source, ICriteria criteria) : base(ConstructSource(source), criteria,
            GetSqlTypeFromChangeType(EChangeType.Delete))
        {
            _changeType = EChangeType.Delete;
        }

        /// <summary>
        ///     创建用于删除的Sql语句。
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="criteria">筛选条件</param>
        public ChangeSql(ISource source, ICriteria criteria) : base(source, criteria,
            GetSqlTypeFromChangeType(EChangeType.Delete))
        {
            _changeType = EChangeType.Delete;
        }


        /// <summary>
        ///     要修改的源，参与构建Delete子句、Update子句和Insert Into子句。
        /// </summary>
        public MonomerSource TargetSource
        {
            get => _targetSource;
            set => _targetSource = value;
        }

        /// <summary>
        ///     修改类型
        /// </summary>
        public EChangeType ChangeType
        {
            get => _changeType;
            set => _changeType = value;
        }


        /// <summary>
        ///     根据修改类型获取Sql语句类型
        /// </summary>
        /// <param name="changeType">修改类型</param>
        /// <returns></returns>
        private static ESqlType GetSqlTypeFromChangeType(EChangeType changeType)
        {
            switch (changeType)
            {
                case EChangeType.Insert:
                    return ESqlType.Insert;
                case EChangeType.Update:
                    return ESqlType.Update;
                case EChangeType.Delete:
                    return ESqlType.Delete;
                default:
                    throw new ArgumentOutOfRangeException(nameof(changeType), changeType, $"未知的修改类型{changeType}");
            }
        }

        /// <summary>
        ///     通过名称构造ISource
        /// </summary>
        /// <param name="sourceName">源名称</param>
        /// <returns></returns>
        private static ISource ConstructSource(string sourceName)
        {
            ISource source = new SimpleSource(sourceName);
            return source;
        }

        /// <summary>
        ///     向Sql语句追加字段设值器
        ///     注：一个字段最多只能有一个设值器，已存在的设值器再次追加时将被覆盖。
        /// </summary>
        /// <param name="fieldSetters">要追加的字段设值器</param>
        public void AppendFieldSetter(List<IFieldSetter> fieldSetters)
        {
            foreach (var fieldSetter in fieldSetters)
            {
                var fieldName = fieldSetter.Field.Name;
                _fieldSetters[fieldName] = fieldSetter;
            }
        }

        /// <summary>
        ///     从Sql语句中移除指定的字段设值器。
        /// </summary>
        /// <param name="fieldName">要移除其设值器的字段的名称。</param>
        public void RemoveFieldSetter(string fieldName)
        {
            _fieldSetters?.Remove(fieldName);
        }

        /// <summary>
        ///     强制覆盖Sql语句中指定字段设值器的值。
        /// </summary>
        /// <param name="fieldName">要覆盖其设值器值的字段的名称。</param>
        /// <param name="value">新值。</param>
        public void OverwriteField(string fieldName, object value)
        {
            if (_fieldSetters == null) return;
            _fieldSetters[fieldName] = SqlUtils.GetFieIdSetter(value?.GetType(), fieldName, value);
        }

        /// <summary>
        ///     强制覆盖Sql语句中指定字段设值器的值。
        /// </summary>
        /// <param name="fieldName">要覆盖其设值器值的字段的名称。</param>
        /// <param name="value">新值。</param>
        public void OverwriteField(string fieldName, Expression value)
        {
            if (_fieldSetters == null) return;
            _fieldSetters[fieldName] = new FieldSetter(fieldName, value);
        }


        /// <summary>
        ///     针对指定的数据源类型，根据修改Sql语句的对象表示法生成Sql语句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public override string ToSql(EDataSource sourceType)
        {
            StringBuilder reultBuilder;
            var columns = new List<string>();
            var values = new List<string>();

            switch (_changeType)
            {
                case EChangeType.Insert:
                {
                    reultBuilder = new StringBuilder($"insert into {Source.ToString(sourceType)} ");
                    foreach (var u in _fieldSetters ?? new Dictionary<string, IFieldSetter>())
                    {
                        values.Add(u.Value.ToString(out var column, sourceType));
                        columns.Add(column);
                    }

                    reultBuilder.Append($"({string.Join(",", columns)})");
                    reultBuilder.Append($" values({string.Join(",", values)})");
                    break;
                }
                case EChangeType.Update:
                {
                    //Sqlite不支持JoinSource
                    if (Source is JoinedSource && sourceType == EDataSource.Sqlite)
                        throw new InvalidOperationException($"{sourceType}不支持更新连接查询源");
                    //字段
                    foreach (var u in _fieldSetters ?? new Dictionary<string, IFieldSetter>())
                    {
                        var column = sourceType == EDataSource.Sqlite
                            ? $"{u.Value.ToString(sourceType)}"
                            : $"{TargetSource.Symbol}.{u.Value.ToString(sourceType)}";
                        columns.Add(column);
                    }

                    reultBuilder = new StringBuilder("update ");
                    //对于更新语句 SqlServer 和 MySql的语句组成方式有差异
                    switch (sourceType)
                    {
                        case EDataSource.SqlServer:
                        {
                            //SqlServer形如 update source set source.value = '' from Source
                            reultBuilder.Append(
                                $"{TargetSource.Symbol} set {string.Join(",", columns)}  from {Source.ToString(sourceType)}");
                            break;
                        }
                        case EDataSource.Oracle:
                        case EDataSource.MySql:
                        case EDataSource.PostgreSql:
                        case EDataSource.Sqlite:
                        {
                            //MySql形如 update Source set source.value = ''
                            reultBuilder.Append($"{Source.ToString(sourceType)} set {string.Join(",", columns)}");
                            break;
                        }
                    }

                    if (Criteria != null) reultBuilder.Append($" where {Criteria.ToString(sourceType)}");
                    break;
                }
                case EChangeType.Delete:
                {
                    //Sqlite不支持JoinSource
                    if (Source is JoinedSource &&
                        (sourceType == EDataSource.Sqlite || sourceType == EDataSource.PostgreSql))
                        throw new InvalidOperationException($"{sourceType}不支持删除连接查询源");

                    reultBuilder = new StringBuilder("delete ");

                    //补丁 用于处理直接删除等直接修改部分
                    var source = TargetSource.Symbol;
                    //简单源使用本名
                    if (TargetSource is SimpleSource simpleSource)
                    {
                        source = simpleSource.Name;
                        //连接源使用目标别名
                        if (Source is JoinedSource) source = simpleSource.Symbol;
                    }

                    //Sqlite无源名称
                    if (sourceType != EDataSource.Sqlite) reultBuilder.Append(source);
                    reultBuilder.Append($" from {Source.ToString(sourceType)}");
                    if (Criteria != null) reultBuilder.Append($" where {Criteria.ToString(sourceType)}");
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"未知的修改SQL类型{_changeType}", nameof(_changeType));
            }

            return reultBuilder.ToString();
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public override string ToSql(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //注意out值不要赋空
            StringBuilder reultBuilder;
            switch (_changeType)
            {
                case EChangeType.Insert:
                    reultBuilder = GenerateInsertTSql(sourceType, out sqlParameters, creator);
                    break;
                case EChangeType.Update:
                    reultBuilder = GenerateUpdateTSql(sourceType, out sqlParameters, creator);
                    break;
                case EChangeType.Delete:
                    reultBuilder = GenerateDeleteTSql(sourceType, out sqlParameters, creator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"未知的修改SQL类型{_changeType}", nameof(_changeType));
            }

            return reultBuilder.ToString();
        }

        /// <summary>
        ///     生成Insert TSQL 语句
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">输出 参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        private StringBuilder GenerateInsertTSql(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //注意out值不要赋空
            // Insert into 目标源
            sqlParameters = new List<IDataParameter>();

            StringBuilder reultBuilder;
            var columns = new List<string>();
            var values = new List<string>();

            #region 临时解决 TargetSource为null的情况

            //目标源
            var tSource = TargetSource;
            if (tSource == null)
                tSource = Source as MonomerSource;

            if (tSource == null)
                throw new ArgumentException("插入时源设置错误");

            #endregion

            /*INSERT INTO  */
            switch (sourceType)
            {
                //SqlServer数据源
                case EDataSource.SqlServer:
                    reultBuilder = new StringBuilder($"INSERT INTO [{tSource.Symbol}]");
                    break;
                // PostgreSQL数据源
                case EDataSource.PostgreSql:
                    reultBuilder = new StringBuilder("INSERT INTO \"" + tSource.Symbol + "\"");
                    break;
                //Oracle数据源
                case EDataSource.Oracle:
                //OLEDB数据提供程序
                case EDataSource.Oledb:
                //MySql数据源
                case EDataSource.MySql:
                //Sqlite数据源
                case EDataSource.Sqlite:
                //其他数据源
                case EDataSource.Other:
                    reultBuilder = new StringBuilder($"INSERT INTO `{tSource.Symbol}`");
                    break;
                default:
                    throw new ArgumentException($"不支持的数据源{sourceType}");
            }

            /*(columns) values(...)*/
            foreach (var u in _fieldSetters ?? new Dictionary<string, IFieldSetter>())
            {
                var placeholder = u.Value.ToString(out var para, out var column, sourceType, creator);
                sqlParameters.Add(para);
                values.Add(placeholder);
                columns.Add(column);
            }

            //(columns)
            reultBuilder.Append($"({string.Join(",", columns)})");
            //values(...)
            reultBuilder.Append($" VALUES({string.Join(",", values)})");
            return reultBuilder;
        }

        /// <summary>
        ///     生成Update TSQL 语句
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">输出 参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        private StringBuilder GenerateUpdateTSql(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //注意out值不要赋空
            //Update 目标源
            //Set ---
            //From 查询源（A inner join B on ----）
            //Where ---

            sqlParameters = new List<IDataParameter>();

            StringBuilder reultBuilder;
            var sets = new List<string>();

            #region 临时解决 TargetSource为null的情况

            //目标源
            var tSource = TargetSource;
            if (tSource == null)
                tSource = Source as MonomerSource;

            if (tSource == null)
                throw new ArgumentException("更新时源设置错误");

            #endregion

            /*UPDATE 目标源*/
            switch (sourceType)
            {
                //SqlServer数据源
                case EDataSource.SqlServer:
                    reultBuilder = new StringBuilder($" UPDATE [{tSource.Symbol}] ");
                    break;
                case EDataSource.PostgreSql:
                    reultBuilder = new StringBuilder("UPDATE \"" + tSource.Symbol + "\"" + tSource.Symbol);
                    break;
                //Oracle数据源
                case EDataSource.Oracle:
                //OLEDB数据提供程序
                case EDataSource.Oledb:
                //MySql数据源
                case EDataSource.MySql:
                //Sqlite数据源
                case EDataSource.Sqlite:
                //其他数据源
                case EDataSource.Other:
                    reultBuilder = new StringBuilder($" UPDATE `{tSource.Symbol}` ");
                    break;
                default:
                    throw new ArgumentException($"不支持的数据源{sourceType}");
            }

            /* SET -- 字段*/
            foreach (var u in _fieldSetters ?? new Dictionary<string, IFieldSetter>())
            {
                var setWithPlaceholder = u.Value.ToString(out var para, sourceType, creator);
                if (para != null) sqlParameters.Add(para);
                switch (sourceType)
                {
                    case EDataSource.SqlServer:
                    case EDataSource.Oracle:
                    case EDataSource.Oledb:
                    case EDataSource.PostgreSql:
                    case EDataSource.MySql:
                        break;
                    case EDataSource.Sqlite:
                        setWithPlaceholder = $"{setWithPlaceholder}";
                        break;
                }

                sets.Add(setWithPlaceholder);
            }

            reultBuilder.Append($" SET {string.Join(", ", sets)} ");

            /*FROM 查询源（A inner join B on ----）*/
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    //SqlServer形如 update source set source.value = '' from Source
                    reultBuilder.Append($" FROM {Source.ToString(sourceType)} ");
                    break;
                }
                case EDataSource.Oracle:
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                    break;
            }

            /*WHERE 条件*/
            if (Criteria != null)
            {
                reultBuilder.Append($" WHERE {Criteria.ToString(sourceType, out var paras, creator)} ");
                sqlParameters.AddRange(paras);
            }

            return reultBuilder;
        }


        /// <summary>
        ///     生成Delete TSQL 语句
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">输出 参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        private StringBuilder GenerateDeleteTSql(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //Delete 目标源
            //From 查询源
            //Where ----
            StringBuilder resultBuilder;
            sqlParameters = new List<IDataParameter>();

            #region 临时解决 TargetSource为null的情况

            //目标源
            var tSource = TargetSource;
            if (tSource == null)
                tSource = Source as MonomerSource;

            if (tSource == null)
                throw new ArgumentException("删除时源设置错误");

            #endregion

            /*Delete 目标源*/
            switch (sourceType)
            {
                //SqlServer数据源
                case EDataSource.SqlServer:
                    resultBuilder = new StringBuilder($" DELETE [{tSource.Symbol}] ");
                    break;
                //PostgreSQL数据源
                case EDataSource.PostgreSql:
                {
                    resultBuilder = Source is JoinedSource
                        ? new StringBuilder("DELETE \"" + tSource.Symbol + "\"")
                        : new StringBuilder(" DELETE ");
                    break;
                }
                //Sqlite数据源
                case EDataSource.Sqlite:
                    resultBuilder = new StringBuilder(" DELETE ");
                    break;
                //Oracle数据源
                case EDataSource.Oracle:
                //OLEDB数据提供程序
                case EDataSource.Oledb:
                //MySql数据源
                case EDataSource.MySql:
                //其他数据源
                case EDataSource.Other:
                    resultBuilder = new StringBuilder($" DELETE `{tSource.Symbol}` ");
                    break;
                default:
                    throw new ArgumentException($"不支持的数据源{sourceType}");
            }

            if ((sourceType == EDataSource.Sqlite || sourceType == EDataSource.PostgreSql) &&
                (Source is JoinedSource || Source is SimpleSource))
            {
                if (Source is JoinedSource) throw new ArgumentException("SqlIte和PostgreSql不支持Delete Join源");

                if (Source is SimpleSource simpleSource)
                {
                    /*From 查询源*/
                    resultBuilder.Append($" FROM {simpleSource.ToNoSymbolString(sourceType)} ");
                    if (sourceType == EDataSource.PostgreSql)
                        resultBuilder.Append(" ").Append(simpleSource.Name).Append(" ");
                }
            }
            else
            {
                /*From 查询源*/
                resultBuilder.Append($" FROM {Source.ToString(sourceType)} ");
            }

            /*WHERE 条件*/
            if (Criteria != null)
            {
                resultBuilder.Append($" WHERE {Criteria.ToString(sourceType, out var paras, creator)}");
                sqlParameters.AddRange(paras);
            }

            return resultBuilder;
        }

        /// <summary>
        ///     使用参数化的方式 和 默认的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public override string ToSql(out List<IDataParameter> sqlParameters, IParameterCreator creator)
        {
            return ToSql(EDataSource.SqlServer, out sqlParameters, creator);
        }
    }
}