/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Sql执行器接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:49:49
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     Sql执行器接口
    /// </summary>
    public interface ISqlExecutor
    {
        /// <summary>
        ///     获取或设置执行超时时间（注：不是连接超时时间）
        /// </summary>
        int CommandTimeout { get; set; }

        /// <summary>
        ///     获取数据源类型。
        /// </summary>
        EDataSource SourceType { get; }

        /// <summary>
        ///     获取一个值，该值指示是否已开启本地事务。
        /// </summary>
        bool TransactionBegun { get; }

        /// <summary>
        ///     执行参数化的查询Sql语句，返回IDataReader。
        ///     不论执行前连接是否已打开，执行完后就将保持连接打开状态，调用方必须在合适时间手动关闭连接。
        /// </summary>
        /// <param name="sql">参数化的查询Sql语句</param>
        /// <param name="paras">参数列表</param>
        IDataReader ExecuteReader(string sql, IDataParameter[] paras);

        /// <summary>
        ///     执行非查询参数化Sql语句，并返回影响行数。
        ///     如果执行前连接未打开，执行完后会自动关闭连接；如果执行前连接已打开，执行完后会保持打开状态。
        /// </summary>
        /// <param name="sql">非查询参数化Sql语句</param>
        /// <param name="paras">参数列表</param>
        int Execute(string sql, IDataParameter[] paras);


        /// <summary>
        ///     执行返回单个值的参数化Sql语句。
        ///     如果执行前连接未打开，执行完后会自动关闭连接；如果执行前连接已打开，执行完后会保持打开状态。
        /// </summary>
        /// <param name="sql">返回单个值的参数化Sql语句</param>
        /// <param name="paras">参数列表</param>
        object ExecuteScalar(string sql, IDataParameter[] paras);

        /// <summary>
        ///     打开数据库连接。
        ///     如果连接已打开则不执行任何操作。
        /// </summary>
        void OpenConnection();

        /// <summary>
        ///     关闭数据库连接。
        ///     如果连接处于关闭状态则不执行任何操作。如果当时有事务未提交，则不执行任何操作。
        /// </summary>
        void CloseConnection();

        /// <summary>
        ///     开启本地事务，事务隔离级别为ReadCommitted，即读时发布共享锁，读完即释放，可以防止读脏，但不能消除数据幻影。
        ///     在事务结束前调用本方法不会开启另一个事务，也不会引发异常。
        /// </summary>
        void BeginTransaction();

        /// <summary>
        ///     以指定的隔离级别开启事务处理。
        ///     如果已开启事务，执行此方法不会重复开启。
        /// </summary>
        /// <param name="iso">事务隔离级别</param>
        void BeginTransaction(IsolationLevel iso);

        /// <summary>
        ///     回滚事务。
        ///     如果事务未开启，不执行任务操作
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        ///     提交事务。
        ///     如果事务未开启，不执行任务操作
        /// </summary>
        void CommitTransaction();

        /// <summary>
        ///     将当前执行器登记为环境事务的参与者。
        /// </summary>
        void EnlistTransaction();

        /// <summary>
        ///     创建参数构造器
        /// </summary>
        /// <returns></returns>
        IParameterCreator CreateParameterCreator();
    }
}