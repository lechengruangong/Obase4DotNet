/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Sql执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:54:10
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Obase.Core;
using Obase.Core.Saving;
using Obase.Providers.Sql.ConnectionPool;
using Obase.Providers.Sql.SqlObject;
using SafeObjectPool;
using IsolationLevel = System.Data.IsolationLevel;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     Sql执行器
    /// </summary>
    public abstract class SqlExecutor : ISqlExecutor
    {
        /// <summary>
        ///     数据源类型。
        /// </summary>
        private readonly EDataSource _sourceType;

        /// <summary>
        ///     连接字符串
        /// </summary>
        protected readonly string ConnString;


        /// <summary>
        ///     受影响的行数
        /// </summary>
        private int _affectRows;

        /// <summary>
        ///     执行超时时间（注：不是连接超时时间）
        /// </summary>
        private int _commandTimeout;

        /// <summary>
        ///     事务
        /// </summary>
        private DbTransaction _transaction;

        /// <summary>
        ///     事务影响的行数
        /// </summary>
        private int _transNumber;

        /// <summary>
        ///     数据库连接状态
        /// </summary>
        protected Object<DbConnection> Conn;

        /// <summary>
        ///     Sql命令
        /// </summary>
        protected DbCommand SqlCommand;


        /// <summary>
        ///     使用指定的连接字符串创建特定于指定数据源类型的Sql执行器实例。
        /// </summary>
        /// <param name="connString">连接字符串</param>
        /// <param name="sourceType">数据源类型。</param>
        protected SqlExecutor(string connString, EDataSource sourceType)
        {
            ConnString = connString;
            _sourceType = sourceType;
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
        public EDataSource SourceType => _sourceType;

        /// <summary>
        ///     获取一个值，该值指示是否已开启本地事务。
        /// </summary>
        public bool TransactionBegun => _transNumber > 0;

        /// <summary>
        ///     打开数据库连接。
        ///     如果连接已打开则不执行任何操作。
        /// </summary>
        public void OpenConnection()
        {
            if (Conn == null) Conn = CreateConnection(ConnString);
            if (Conn.Value.State == ConnectionState.Closed) Conn.Value.Open();
        }


        /// <summary>
        ///     关闭数据库连接。
        ///     如果连接处于关闭状态则不执行任何操作。如果当时有事务未提交，则不执行任何操作。
        /// </summary>
        public void CloseConnection()
        {
            if (_transNumber > 0) return;

            //先释放SqlCommand
            SqlCommand?.Dispose();
            SqlCommand = null;
            //归还连接 连接置空
            if (Conn != null && Conn.Value.State != ConnectionState.Closed)
            {
                ObaseConnectionPool.Current.ReturnConnection(ConnString, Conn);
                Conn = null;
            }
        }


        /// <summary>
        ///     以指定的隔离级别开启事务处理。
        ///     在事务结束前调用本方法不会开启另一个事务，也不会引发异常。
        /// </summary>
        /// <param name="iso">事务隔离级别</param>
        public void BeginTransaction(IsolationLevel iso)
        {
            OpenConnection();
            if (_transNumber == 0)
            {
                InteriorCreateCommand();
                _transaction = Conn.Value.BeginTransaction(iso);
                SqlCommand.Transaction = _transaction;
            }

            _transNumber++;
        }

        /// <summary>
        ///     开启本地事务，事务隔离级别为ReadCommitted，即读时发布共享锁，读完即释放，可以防止读脏，但不能消除数据幻影。
        ///     在事务结束前调用本方法不会开启另一个事务，也不会引发异常。
        /// </summary>
        public void BeginTransaction()
        {
            OpenConnection();
            if (_transNumber == 0)
            {
                InteriorCreateCommand();
                _transaction = Conn.Value.BeginTransaction(IsolationLevel.ReadCommitted);
                SqlCommand.Transaction = _transaction;
            }

            _transNumber++;
        }

        /// <summary>
        ///     回滚事务。
        ///     如果事务未开启，不执行任务操作
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
                _transNumber = 0;
            }
        }

        /// <summary>
        ///     提交事务。
        ///     如果事务未开启，不执行任务操作
        /// </summary>
        public void CommitTransaction()
        {
            if (_transNumber > 0) _transNumber--;

            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        ///     将当前执行器登记为环境事务的参与者
        /// </summary>
        public void EnlistTransaction()
        {
            OpenConnection();
            var connectionStr = new ConnectionString(ConnString);
            //如果不是自动附加至环境事务
            if (connectionStr["autoenlist"].ToLower().Equals("false"))
                Conn.Value.EnlistTransaction(Transaction.Current);
        }

        /// <summary>
        ///     创建参数构造器
        /// </summary>
        /// <returns></returns>
        public abstract IParameterCreator CreateParameterCreator();

        /// <summary>
        ///     执行非查询参数化Sql语句，并返回影响行数。
        ///     如果执行前连接未打开，执行完后会自动关闭连接；如果执行前连接已打开，执行完后会保持打开状态。
        /// </summary>
        /// <param name="sql">非查询参数化Sql语句</param>
        /// <param name="paras">参数列表</param>
        public int Execute(string sql, IDataParameter[] paras)
        {
            //判断是否为空 或者 连接是关闭的
            var isOpenByExecutor = Conn == null || Conn.Value.State == ConnectionState.Closed;
            try
            {
                //如果是关闭 或者空 则自己开
                if (isOpenByExecutor) OpenConnection();
                //构造命令
                InteriorCreateCommand();
                //设置具体内容 清除原有的参数
                SqlCommand.CommandText = sql;
                SqlCommand.Parameters.Clear();
                //加入此次参数
                foreach (var item in paras)
                    SqlCommand.Parameters.Add(item);
                //执行语句
                _affectRows = SqlCommand.ExecuteNonQuery();
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
                //自己开的自己关
                if (isOpenByExecutor)
                {
                    //先释放SqlCommand
                    SqlCommand?.Dispose();
                    SqlCommand = null;
                    //归还连接 连接置空
                    if (Conn != null)
                    {
                        ObaseConnectionPool.Current.ReturnConnection(ConnString, Conn);
                        Conn = null;
                    }
                }
            }
        }

        /// <summary>
        ///     执行返回单个值的参数化Sql语句。
        ///     如果执行前连接未打开，执行完后会自动关闭连接；如果执行前连接已打开，执行完后会保持打开状态。
        /// </summary>
        /// <param name="sql">返回单个值的参数化Sql语句</param>
        /// <param name="paras">参数列表</param>
        public object ExecuteScalar(string sql, IDataParameter[] paras)
        {
            object res;
            //判断是否为空 或者 连接是关闭的
            var isOpenByExecutor = Conn == null || Conn.Value.State == ConnectionState.Closed;
            try
            {
                //如果是关闭 或者空 则自己开
                if (isOpenByExecutor) OpenConnection();
                //构造命令
                InteriorCreateCommand();
                //设置具体内容 清除原有的参数
                SqlCommand.CommandText = sql;
                SqlCommand.Parameters.Clear();
                //加入此次参数
                foreach (var item in paras)
                    SqlCommand.Parameters.Add(item);
                //执行语句
                res = SqlCommand.ExecuteScalar();
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
                //自己开的自己关
                if (isOpenByExecutor)
                {
                    //先释放SqlCommand
                    SqlCommand?.Dispose();
                    SqlCommand = null;
                    //归还连接 连接置空
                    if (Conn != null)
                    {
                        ObaseConnectionPool.Current.ReturnConnection(ConnString, Conn);
                        Conn = null;
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     执行参数化的查询Sql语句，返回IDataReader。
        ///     不论执行前连接是否已打开，执行完后就将保持连接打开状态，调用方必须在合适时间手动关闭连接。
        /// </summary>
        /// <param name="sql">参数化的查询Sql语句</param>
        /// <param name="paras">参数列表</param>
        public IDataReader ExecuteReader(string sql, IDataParameter[] paras)
        {
            //判断是否为空 或者 连接是关闭的
            var isOpenByExecutor = Conn == null || Conn.Value.State == ConnectionState.Closed;
            try
            {
                var commandBehavior = CommandBehavior.Default;
                //如果是关闭 或者空 则自己开
                if (isOpenByExecutor) OpenConnection();

                //构造命令
                InteriorCreateCommand();
                //设置具体内容 清除原有的参数
                SqlCommand.CommandText = sql;
                SqlCommand.Parameters.Clear();
                //加入此次参数
                foreach (var item in paras)
                    SqlCommand.Parameters.Add(item);
                //执行语句 获取Reader
                var rd = SqlCommand.ExecuteReader(commandBehavior);

                return rd;
            }
            catch
            {
                //自己开的 此时需要在读取完之后关 所以只在发生异常的时候释放SqlCommand
                if (isOpenByExecutor)
                {
                    SqlCommand?.Dispose();
                    SqlCommand = null;
                }

                throw;
            }
        }

        /// <summary>
        ///     判定指定的异常是否归因于插入重复记录。判定逻辑特定于数据库类型。
        /// </summary>
        /// <param name="exception">被判定的异常实例。</param>
        protected abstract bool IsRepeatInsertionError(Exception exception);

        /// <summary>
        ///     由派生类实现以提供一个连接对象，该对象用于建立到数据源的连接。
        /// </summary>
        /// <param name="connString">连接字符串</param>
        protected abstract Object<DbConnection> CreateConnection(string connString);

        /// <summary>
        ///     由派生类实现以提供一个命令对象，该对象用于执行Sql语句。
        /// </summary>
        protected abstract DbCommand CreateCommand();

        /// <summary>
        ///     创建命令私有方法
        /// </summary>
        private void InteriorCreateCommand()
        {
            if (SqlCommand == null) SqlCommand = CreateCommand();
            SqlCommand.CommandTimeout = _commandTimeout;
            SqlCommand.Connection = Conn.Value;
        }
    }
}