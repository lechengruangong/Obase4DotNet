/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基础查询提供程序规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:59:32
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     基础查询提供程序规范，提供在异构查询中执行基础查询的方案。
    ///     说明
    ///     执行基础查询总体上分为三步。首先，从基础查询链中分离出存储服务无法执行的尾部片段，称为补充查询。然后，调用存储服务，执行分离后剩余的部分（可执行部分）。最后，基
    ///     于存储服务返回的结果，执行补充运算，得到最终结果。
    ///     通常情况下，在执行第一步分离操作时，需要同时生成可执行部分的运算管道；执行第二步时可直接使用该管道，不需要再重新生成。状态参数queryState可用于传递该管
    ///     道。
    /// </summary>
    public interface IBaseQueryProvider
    {
        /// <summary>
        ///     调用存储服务。
        /// </summary>
        /// <param name="executionState">一个状态对象，携带查询执行流程中生成的数据。</param>
        /// <param name="including">指定由运算管道加载的对象须包含的引用，必须是同构的。</param>
        /// <param name="postexecutionCallback">执行命令后委托</param>
        /// <param name="attachObject">用于将对象附加到对象上下文的委托。</param>
        /// <param name="preexecutionCallback">执行命令前委托</param>
        object CallService(object executionState, AssociationTree including,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, AttachObject attachObject);

        /// <summary>
        ///     执行补充运算。
        /// </summary>
        /// <param name="complement">要执行的补充查询。</param>
        /// <param name="serviceResult">存储服务输出的结果。</param>
        /// <param name="executionState">一个状态对象，携带查询执行流程中生成的数据。</param>
        object ExecuteComplement(QueryOp complement, object serviceResult, object executionState);

        /// <summary>
        ///     从基础查询中分离出补充查询。
        ///     补充运算是特定的存储服务无法执行，须以对象运算方式补充执行的片段。
        /// </summary>
        /// <param name="baseQuery">要执行的基础查询。</param>
        /// <param name="executionState">一个状态对象，携带查询执行流程中生成的数据。</param>
        QueryOp SeparateOutComplement(QueryOp baseQuery, out object executionState);
    }
}