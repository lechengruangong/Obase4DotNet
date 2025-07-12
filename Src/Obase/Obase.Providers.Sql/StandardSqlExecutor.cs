/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标准的参数构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 17:01:27
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Data.Common;
using Obase.Providers.Sql.ConnectionPool;
using Obase.Providers.Sql.SqlObject;
using SafeObjectPool;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     标准Sql执行器，该执行器基于.NET数据提供程序工厂模型构造特定于数据源提供程序实例，然后使用该提供程序实例访问数据源。
    ///     实施说明
    ///     .NET数据提供程序工厂模型参见https://docs.microsoft.com/zh-
    ///     cn/dotnet/framework/data/adonet/obtaining-a-dbproviderfactory。
    ///     根据提供程序名称确定数据源类型。
    /// </summary>
    public class StandardSqlExecutor : SqlExecutor
    {
        /// <summary>
        ///     上下文类型
        /// </summary>
        private readonly Type _contexType;

        /// <summary>
        ///     数据源提供工厂
        /// </summary>
        private readonly DbProviderFactory _providerFactory;


        /// <summary>
        ///     初始化StandardSqlExecutor类的新实例。
        /// </summary>
        /// <param name="providerFactory">用于创建数据提供程序类实例的工厂。</param>
        /// <param name="sourceType">源类型</param>
        /// <param name="connString">连接字符串。</param>
        /// <param name="contexType">上下文类型</param>
        public StandardSqlExecutor(DbProviderFactory providerFactory, EDataSource sourceType, string connString,
            Type contexType) :
            base(connString, sourceType)
        {
            _providerFactory = providerFactory;
            _contexType = contexType;
        }

        /// <summary>
        ///     获取表示“重复插入”错误的代码，该代码特定于数据库引擎。
        /// </summary>
        private int RepeatInsertionErrorNumber
        {
            get
            {
                switch (SourceType)
                {
                    case EDataSource.SqlServer:
                        return 2627;
                    case EDataSource.Oracle:
                        return 1;
                    case EDataSource.MySql:
                        return 1062;
                    case EDataSource.Sqlite:
                        return 19;
                    case EDataSource.PostgreSql:
                        return 23505;
                    case EDataSource.Other:
                    case EDataSource.Oledb:
                        return -1;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(SourceType), $"未知的数据源类型{SourceType}");
                }
            }
        }

        /// <summary>
        ///     创建参数构造器
        /// </summary>
        /// <returns></returns>
        public override IParameterCreator CreateParameterCreator()
        {
            return new StandardParameterCreator(_providerFactory);
        }

        /// <summary>
        ///     判定指定的异常是否归因于插入重复记录。判定逻辑特定于数据库类型。
        /// </summary>
        /// <param name="exception">被判定的异常实例。</param>
        protected override bool IsRepeatInsertionError(Exception exception)
        {
            if (exception is DbException dbException)
            {
                foreach (DictionaryEntry data in dbException.Data)
                    if (data.Value.ToString() == RepeatInsertionErrorNumber.ToString())
                        return true;

                if (SourceType == EDataSource.Sqlite)
                    if (exception.Message.Contains($"Error {RepeatInsertionErrorNumber}"))
                        return true;
                if (SourceType == EDataSource.PostgreSql)
                    if (exception.Message.Contains("duplicate key"))
                        return true;

                return false;
            }

            throw exception;
        }

        /// <summary>
        ///     由派生类实现以提供一个连接对象，该对象用于建立到数据源的连接。
        /// </summary>
        /// <param name="connString">连接字符串</param>
        protected override Object<DbConnection> CreateConnection(string connString)
        {
            var pool = ObaseConnectionPool.Current.GetPool(connString, _providerFactory, _contexType);
            var conn = pool.Get();
            return conn;
        }

        /// <summary>
        ///     由派生类实现以提供一个命令对象，该对象用于执行Sql语句。
        /// </summary>
        protected override DbCommand CreateCommand()
        {
            return _providerFactory.CreateCommand();
        }
    }
}