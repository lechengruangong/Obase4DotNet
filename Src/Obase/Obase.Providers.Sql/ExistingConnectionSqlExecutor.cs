/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于已有连接的SQL执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-21 11:40:49
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using Obase.Core.Saving;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     基于已有连接的SQL执行器
    /// </summary>
    public class ExistingConnectionSqlExecutor : ISqlExecutor
    {
        /// <summary>
        ///     数据库连接
        /// </summary>
        private readonly DbConnection _connection;

        /// <summary>
        ///     数据库连接模式，即如何管理数据库连接的打开与关闭。
        /// </summary>
        private readonly EConnectionMode _connectionMode;

        /// <summary>
        ///     用于创建数据提供程序类实例的工厂
        /// </summary>
        private readonly DbProviderFactory _providerFactory;

        /// <summary>
        ///     已有连接上目前已开启的事务
        /// </summary>
        private readonly DbTransaction _transaction;

        /// <summary>
        ///     受影响的行数
        /// </summary>
        private int _affectRows;

        /// <summary>
        ///     执行超时时间（注：不是连接超时时间）
        /// </summary>
        private int _commandTimeout;

        /// <summary>
        ///     Sql命令
        /// </summary>
        private DbCommand _sqlCommand;

        /// <summary>
        ///     事务的个数
        /// </summary>
        private int _transNumber;

        /// <summary>
        ///     初始化基于已有连接的SQL执行器。
        /// </summary>
        /// <param name="providerFactory">用于创建数据提供程序类实例的工厂</param>
        /// <param name="connection">连接</param>
        /// <param name="sourceType">数据源类型。</param>
        /// <param name="transaction">当前连接中已执行命令的事务,没有开启事务时不需要此参数</param>
        public ExistingConnectionSqlExecutor(DbProviderFactory providerFactory, DbConnection connection,
            EDataSource sourceType, DbTransaction transaction = null)
        {
            SourceType = sourceType;
            _providerFactory = providerFactory;
            _connection = connection;
            _transaction = transaction;
            _connectionMode = EConnectionMode.Caller;
        }

        /// <summary>
        ///     获取或设置执行超时时间（注：不是连接超时时间）
        /// </summary>
        public int CommandTimeout
        {
            get => _commandTimeout;
            set => _commandTimeout = value;
        }

        /// <summary>
        ///     获取数据源类型。
        /// </summary>
        public EDataSource SourceType { get; }

        /// <summary>
        ///     获取一个值，该值指示是否已开启本地事务。
        /// </summary>
        public bool TransactionBegun => _transNumber > 0;

        /// <summary>
        ///     获取数据库连接模式，即如何管理数据库连接的打开与关闭。
        /// </summary>
        public EConnectionMode ConnectionMode => _connectionMode;

        /// <summary>
        ///     执行参数化的查询Sql语句，返回IDataReader。
        ///     不论执行前连接是否已打开，执行完后就将保持连接打开状态，调用方必须在合适时间手动关闭连接。
        ///     如果执行前连接未打开，自动打开并将数据库连接模式设置为“执行模式”。
        /// </summary>
        /// <param name="sql">参数化的查询Sql语句</param>
        /// <param name="paras">参数列表</param>
        public IDataReader ExecuteReader(string sql, IDataParameter[] paras)
        {
            //检查连接
            CheckConnection();

            try
            {
                //构造命令
                InteriorCreateCommand();
                //设置具体内容 清除原有的参数
                _sqlCommand.CommandText = sql;
                _sqlCommand.Parameters.Clear();
                //加入此次参数
                foreach (var item in paras)
                    _sqlCommand.Parameters.Add(item);
                //执行语句 获取Reader
                return _sqlCommand.ExecuteReader(CommandBehavior.Default);
            }
            catch
            {
                //由执行器打开 此时需要在读取完之后关 所以只在发生异常的时候释放SqlCommand
                if (_connectionMode == EConnectionMode.Execution)
                {
                    _sqlCommand?.Dispose();
                    _sqlCommand = null;
                }

                throw;
            }
        }


        /// <summary>
        ///     执行非查询参数化Sql语句，并返回影响行数。
        ///     如果执行前连接未打开，自动打开并将数据库连接模式设置为“执行模式”。
        ///     执行完后会自动关闭连接；如果执行前连接已打开，执行完后会保持打开状态。
        /// </summary>
        /// <param name="sql">非查询参数化Sql语句</param>
        /// <param name="paras">参数列表</param>
        public int Execute(string sql, IDataParameter[] paras)
        {
            //检查连接
            CheckConnection();

            try
            {
                //构造命令
                InteriorCreateCommand();
                //设置具体内容 清除原有的参数
                _sqlCommand.CommandText = sql;
                _sqlCommand.Parameters.Clear();
                //加入此次参数
                foreach (var item in paras)
                    _sqlCommand.Parameters.Add(item);
                //执行语句
                _affectRows = _sqlCommand.ExecuteNonQuery();
                //如果影响结果为0 则抛出没更新任何值异常
                if (_affectRows == 0) throw new NothingUpdatedException();
                //返回查询结果
                return _affectRows;
            }
            catch (Exception ex)
            {
                //发生异常 是否为主键重复插入异常
                if (IsRepeatInsertionError(ex))
                {
                    var ex1 = new RepeatInsertionException(SourceType == EDataSource.PostgreSql);
                    if (SourceType == EDataSource.PostgreSql)
                        ex1.UnSupportMessage = "PostgreSQL不支持在单一事务块中发生异常后再次执行其他命令.";
                    throw ex1;
                }

                throw;
            }
            finally
            {
                //由执行器打开 自己开的自己关
                if (_connectionMode == EConnectionMode.Execution)
                    CloseConnection();
            }
        }

        /// <summary>
        ///     执行返回单个值的参数化Sql语句。
        ///     如果执行前连接未打开，自动打开并将数据库连接模式设置为“执行模式”。
        ///     执行完后会自动关闭连接；如果执行前连接已打开，执行完后会保持打开状态。
        /// </summary>
        /// <param name="sql">返回单个值的参数化Sql语句</param>
        /// <param name="paras">参数列表</param>
        public object ExecuteScalar(string sql, IDataParameter[] paras)
        {
            object res;
            //检查连接
            CheckConnection();

            try
            {
                //构造命令
                InteriorCreateCommand();
                //设置具体内容 清除原有的参数
                _sqlCommand.CommandText = sql;
                _sqlCommand.Parameters.Clear();
                //加入此次参数
                foreach (var item in paras)
                    _sqlCommand.Parameters.Add(item);
                //执行语句
                res = _sqlCommand.ExecuteScalar();
                //影响行数设为1
                _affectRows = 1;
            }
            catch (Exception ex)
            {
                //发生异常 是否为主键重复插入异常
                if (IsRepeatInsertionError(ex))
                {
                    var ex1 = new RepeatInsertionException(SourceType == EDataSource.PostgreSql);
                    if (SourceType == EDataSource.PostgreSql)
                        ex1.UnSupportMessage = "PostgreSQL不支持在单一事务块中发生异常后再次执行其他命令.";
                    throw ex1;
                }

                throw;
            }
            finally
            {
                //由执行器打开 自己开的自己关
                if (_connectionMode == EConnectionMode.Execution)
                    CloseConnection();
            }

            return res;
        }

        /// <summary>
        ///     打开数据库连接并将数据库连接模式设置为“调用方模式”。
        ///     如果连接已打开则不执行任何操作。
        /// </summary>
        public void OpenConnection()
        {
            //由传入连接的提供方处理 此处仅检查一下
            CheckConnection();
        }

        /// <summary>
        ///     关闭数据库连接。
        ///     如果连接处于关闭状态则不执行任何操作。如果当时有事务未提交，则不执行任何操作。
        /// </summary>
        public void CloseConnection()
        {
            //不需要处理连接的关闭 实质上此执行器也不会关闭连接
        }

        /// <summary>
        ///     开启本地事务，事务隔离级别为ReadCommitted，即读时发布共享锁，读完即释放，可以防止读脏，但不能消除数据幻影。
        ///     在事务结束前调用本方法不会开启另一个事务，也不会引发异常。
        ///     如果数据库连接未打开则自动打开，并将数据库连接模式设置为“事务模式”。
        /// </summary>
        public void BeginTransaction()
        {
            //事务由传入连接的提供方处理 此执行器不进行实质的处理
            //记录一下由Obase开启的事务个数
            _transNumber++;
        }

        /// <summary>
        ///     以指定的隔离级别开启事务处理。
        ///     如果已开启事务，执行此方法不会重复开启。
        ///     如果数据库连接未打开则自动打开，并将数据库连接模式设置为“事务模式”。
        /// </summary>
        /// <param name="iso">事务隔离级别</param>
        public void BeginTransaction(IsolationLevel iso)
        {
            //事务由传入连接的提供方处理 此执行器不进行实质的处理
            //记录一下由Obase开启的事务个数
            _transNumber++;
        }

        /// <summary>
        ///     回滚事务。
        ///     如果事务未开启，不执行任务操作
        ///     如果数据库连接模式为“事务模式”则自动关闭连接。
        /// </summary>
        public void RollbackTransaction()
        {
            //事务由传入连接的提供方处理 此执行器不处理
            //重置由Obase开启的事务个数
            _transNumber = 0;
        }

        /// <summary>
        ///     提交事务。
        ///     如果事务未开启，不执行任务操作
        ///     如果数据库连接模式为“事务模式”则自动关闭连接。
        /// </summary>
        public void CommitTransaction()
        {
            //事务由传入连接的提供方处理 此执行器不处理
            //记录一下由Obase开启的事务个数
            if (_transNumber > 0) _transNumber--;
        }

        /// <summary>
        ///     将当前执行器登记为环境事务的参与者。
        ///     如果数据库连接未打开则自动打开，并将数据库连接模式设置为“事务模式”。
        /// </summary>
        public void EnlistTransaction()
        {
            //事务由传入连接的提供方处理 此执行器不处理
        }

        /// <summary>
        ///     创建参数构造器
        /// </summary>
        /// <returns></returns>
        public IParameterCreator CreateParameterCreator()
        {
            return new StandardParameterCreator(_providerFactory);
        }

        /// <summary>
        ///     创建命令私有方法
        /// </summary>
        private void InteriorCreateCommand()
        {
            if (_sqlCommand == null) _sqlCommand = _providerFactory.CreateCommand();
            if (_sqlCommand == null)
                throw new ArgumentException("ExistingConnectionSqlExecutor无法创建命令.");
            _sqlCommand.CommandTimeout = _commandTimeout;
            _sqlCommand.Connection = _connection;
            _sqlCommand.Transaction = _transaction;
        }

        /// <summary>
        ///     检查当前的连接
        /// </summary>
        private void CheckConnection()
        {
            //判断是否为空 或者 连接是关闭的
            var isOpenByExecutor = _connection == null || _connection.State == ConnectionState.Closed;
            if (isOpenByExecutor)
                throw new ArgumentException("ExistingConnectionSqlExecutor传入的连接没有打开或者是空.");
        }

        /// <summary>
        ///     判断某个异常是否是重复插入异常
        /// </summary>
        /// <param name="exception">异常</param>
        /// <returns></returns>
        private bool IsRepeatInsertionError(Exception exception)
        {
            if (exception is DbException dbException)
            {
                foreach (DictionaryEntry data in dbException.Data)
                    if (data.Value.ToString() == GetRepeatInsertionErrorNumber().ToString())
                        return true;

                if (SourceType == EDataSource.Sqlite)
                    if (exception.Message.Contains($"Error {GetRepeatInsertionErrorNumber()}"))
                        return true;
                if (SourceType == EDataSource.PostgreSql)
                    if (exception.Message.Contains("duplicate key"))
                        return true;

                return false;
            }

            throw exception;
        }

        /// <summary>
        ///     获取重复插入异常的代码
        /// </summary>
        /// <returns></returns>
        private int GetRepeatInsertionErrorNumber()
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
}