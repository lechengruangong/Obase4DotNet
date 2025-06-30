/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：环境事务.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:15:37
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     提供对.NET事务基础结构（System.Transaction infrastructure）的支持。
    ///     .NET事务基础结构为本地事务和分布式事务提供了统一的编程模型，并使用动态升级（Dynamic Escalation）和可提升登记(Promotable
    ///     Enlistments)两个关键策略确保只有在需要时才启用MSDTC，从而提升性能。
    /// </summary>
    public interface IAmbientTransactionable
    {
        /// <summary>
        ///     向受.NET事务基础结构支持的事务登记。
        /// </summary>
        void EnlistTransaction();
    }
}