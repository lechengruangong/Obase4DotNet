/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：跟踪对象修改并实施持久化的工作流机制.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:08:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;

namespace Obase.Core
{
    /// <summary>
    ///     跟踪对象修改并实施持久化的工作流机制。
    /// </summary>
    public interface IMappingWorkflow
    {
        /// <summary>
        ///     开始跟踪修改。
        ///     实施说明
        ///     须清空之前跟踪到的所有修改。
        /// </summary>
        void Begin();

        /// <summary>
        ///     接受本次工作流的存储源名称（如数据库表名）。
        /// </summary>
        /// <param name="targetSource"></param>
        IMappingWorkflow SetSource(string targetSource);

        /// <summary>
        ///     指示本次工作流将向存储源插入新对象。
        /// </summary>
        IMappingWorkflow ForInserting();

        /// <summary>
        ///     指示本次工作流将修改存储源中已有的对象。
        /// </summary>
        IMappingWorkflow ForUpdating();

        /// <summary>
        ///     指示本次工作流将删除存储源中的对象。
        /// </summary>
        IMappingWorkflow ForDeleting();

        /// <summary>
        ///     设置指定域（如数据库表的字段）的值。
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        IMappingWorkflow SetField(string field, object value);

        /// <summary>
        ///     对指定域（如数据库表的字段）的值施加一个增量。
        /// </summary>
        /// <param name="field"></param>
        /// <param name="increment"></param>
        IMappingWorkflow IncreaseField(string field, object increment);

        /// <summary>
        ///     指示本次工作流应当忽略指定域（如数据库表的字段），如果已跟踪到了该域的修改，应当将其排除。
        /// </summary>
        /// <param name="field"></param>
        IMappingWorkflow IgnoreField(string field);

        /// <summary>
        ///     为当前工作流新增一个映射筛选器，该筛选器与已存在的筛选器进行逻辑“与”运算。
        /// </summary>
        /// <returns>新增的映射筛选器。</returns>
        MappingFilter And();

        /// <summary>
        ///     为当前工作流新增一个映射筛选器，该筛选器与已存在的筛选器进行逻辑“或”运算。
        /// </summary>
        /// <returns>新增的映射筛选器。</returns>
        MappingFilter Or();

        /// <summary>
        ///     级联删除，即从基点类型开始沿关联关系递归删除。实施者制定具体的级联规则。
        /// </summary>
        /// <param name="initType"></param>
        void DeleteCascade(ObjectType initType);

        /// <summary>
        ///     提交工作流。
        /// </summary>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        void Commit(Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback);

        /// <summary>
        ///     提交工作流。
        /// </summary>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        /// <param name="identity">返回存储服务为新对象生成的标识。</param>
        void Commit(Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback, out object identity);
    }
}