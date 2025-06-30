/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：本地事务.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:20:21
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     提供对本地事务的支持。
    ///     本地事务包含的操作局限于单一数据资源（如数据库或消息队列），由该数据资源负责管理。
    /// </summary>
    public interface ITransactionable
    {
        /// <summary>
        ///     开始本地事务。
        /// </summary>
        /// <param name="isolationLevel">事务隔离级别。</param>
        void BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        ///     提交本地事务。
        /// </summary>
        void CommitTransaction();

        /// <summary>
        ///     回滚本地事务。
        /// </summary>
        void RollbackTransaction();
    }
}