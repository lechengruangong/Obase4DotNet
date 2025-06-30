/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的基础查询提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:01:27
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;
using Obase.Core.Query.Oop;

namespace Obase.Core
{
    /// <summary>
    ///     定义存储提供程序规范。
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        ///     获取一个值，该值指示是否已开启本地事务。
        /// </summary>
        bool TransactionBegun { get; }

        /// <summary>
        ///     准备存储资源，如打开数据库连接。
        /// </summary>
        [Obsolete]
        void PrepareResource();

        /// <summary>
        ///     释放存储资源，如关闭数据库连接。
        /// </summary>
        [Obsolete]
        void ReleaseResource();

        /// <summary>
        ///     开始一个本地事务。
        ///     在事务结束前调用本方法不会开启另一个事务，也不会引发异常。
        /// </summary>
        /// <param name="level">事务隔离级别。默认为ReadCommitted。</param>
        void BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted);

        /// <summary>
        ///     提交当前本地事务。如果事务未开启，不执行任务操作。
        /// </summary>
        void CommitTransaction();

        /// <summary>
        ///     回滚当前本地事务。如果事务未开启，不执行任务操作。
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        ///     启动一个新的映射工作流。
        /// </summary>
        /// <returns>一个用于跟踪工作流的对象，它实现了IMappingWorkflow接口。</returns>
        IMappingWorkflow CreateMappingWorkflow();

        /// <summary>
        ///     删除符合指定条件的对象。
        /// </summary>
        /// <param name="objType">要删除的对象的类型。</param>
        /// <param name="filterExpression">用于测试对象是否符合条件的断言函数。</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        int Delete(ObjectType objType, LambdaExpression filterExpression,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback);

        /// <summary>
        ///     搜索符合指定条件的对象，为其属性（部分或全部）设置新值。
        /// </summary>
        /// <param name="objType">要修改其属性的对象的类型。</param>
        /// <param name="filterExpression">用于测试对象是否符合条件的断言函数。</param>
        /// <param name="newValues">存储属性新值的字典。</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        int SetAttributes(ObjectType objType, LambdaExpression filterExpression,
            KeyValuePair<string, object>[] newValues,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback);

        /// <summary>
        ///     搜索符合指定条件的对象，为其属性（部分或全部）施加一个增量。
        /// </summary>
        /// <param name="objType">要修改其属性的对象的类型。</param>
        /// <param name="filterExpression">用于测试对象是否符合条件的断言函数。</param>
        /// <param name="increaseValues">存储增量值的字典。</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        int IncreaseAttributes(ObjectType objType, LambdaExpression filterExpression,
            KeyValuePair<string, object>[] increaseValues, Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback);

        /// <summary>
        ///     为指定的查询生成运算管道。
        /// </summary>
        /// <param name="query">要执行的查询。</param>
        /// <param
        ///     name="complement">
        ///     返回后续查询（或称后续链）。
        ///     在两种情况下会返回后续链：
        ///     （1）某一运算不能由存储服务执行，该运算及其后的运算构成后续链；
        ///     （2） 某一运算无法完全由存储服务执行，需要补充对象运算，则该补充运算和该运算的后续运算串联构成后续链。
        /// </param>
        /// <param name="complementBuilder">返回用于生成补充运算管道的生成器。</param>
        OpExecutor GeneratePipeline(QueryOp query, out QueryOp complement, out OopPipelineBuilder complementBuilder);

        /// <summary>
        ///     执行运算管道。
        /// </summary>
        /// <param name="pipeline">要执行的运算管道。</param>
        /// <param name="resultIncluding">指定由运算管道加载的对象须包含的引用（相对于结果类型），必须是同构的。</param>
        /// <param name="attachObject">用于在对象上下文中附加对象的委托 不指定将不执行附加操作</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        /// <param name="attachRoot">指示是否附加根对象</param>
        object ExecutePipeline(OpExecutor pipeline, AssociationTree resultIncluding,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, AttachObject attachObject = null, bool attachRoot = true);
    }
}