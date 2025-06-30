/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：运算执行器基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 12:09:28
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Query
{
    /// <summary>
    ///     为运算执行器定义了基础实现。
    /// </summary>
    public abstract class OpExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        protected readonly QueryOp _queryOp;

        /// <summary>
        ///     查询运算管道中的下一个执行器。
        /// </summary>
        protected OpExecutor _next;

        /// <summary>
        ///     初始化OpExecutor类的新实例。
        /// </summary>
        /// <param name="queryOp">要执行的查询运算</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        protected OpExecutor(QueryOp queryOp, OpExecutor next = null)
        {
            _queryOp = queryOp;
            _next = next;
        }

        /// <summary>
        ///     获取运算管道中的下一个执行器。
        /// </summary>
        public OpExecutor Next => _next;

        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        public QueryOp QueryOp => _queryOp;
    }


    /// <summary>
    ///     为查询运算执行器提供模板化实现。
    ///     类型参数：
    ///     TContext 运算上下文的类型
    ///     按执行方式对运算进行分类，目前已知的有两种。一种为称对象运算，即对内存中的对象集执行操作；一种称为关系运算，即对关系数据库中的关系实例执行操作。
    ///     根据运算方式不同，可以将运算执行器分为对象运算执行器和关系运算执行器。相应地，运算管道也可以分为对象运算管道和关系运算管道。
    ///     本类为运算执行器定义了基础实现，根据具体的运算方式可以定义具体的运算执行器。
    /// </summary>
    public abstract class OpExecutor<TContext> : OpExecutor
        where TContext : class
    {
        /// <summary>
        ///     构造OpExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        protected OpExecutor(QueryOp queryOp, OpExecutor<TContext> next = null)
            : base(queryOp, next)
        {
        }

        /// <summary>
        ///     执行运算。
        /// </summary>
        /// <param name="context">运算上下文。</param>
        public abstract void Execute(TContext context);
    }
}