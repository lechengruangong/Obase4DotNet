﻿/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:47:33
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;
using Expression = Obase.Providers.Sql.SqlObject.Expression;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     排序运算执行器。
    ///     算法：
    ///     if (resultSql.Top > 0) AcceptResult();
    ///     ExpandSource(依据表达式);
    ///     SourceAliasRootSetter setter = new SourceAliasRootSetter(aliasRoot);
    ///     依据表达式.Accept(setter)
    ///     Order order = new Order(排序依据, 排序方向);
    ///     resultSql.Orders.Append(order);
    /// </summary>
    public class OrderExecutor : RopExecutor
    {
        /// <summary>
        ///     指示是否清除以前的排序
        /// </summary>
        private readonly bool _clearPrevious;

        /// <summary>
        ///     作为排序依据的表达式。
        /// </summary>
        private readonly LambdaExpression _expression;

        /// <summary>
        ///     根据排序依据表达式翻译出的排序依据（Sql表达式）。
        /// </summary>
        private readonly Expression _orderBy;

        /// <summary>
        ///     指示是否倒序。
        /// </summary>
        private readonly bool _reverted;

        /// <summary>
        ///     构造OrderExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="expression">排序依据表达式。</param>
        /// <param name="orderBy">根据排序依据表达式翻译出的排序依据（Sql表达式）。</param>
        /// <param name="reverted">指示是否倒序。</param>
        /// <param name="clearPrevious">指示是否清除以前的排序。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public OrderExecutor(QueryOp queryOp, LambdaExpression expression, Expression orderBy, bool reverted,
            bool clearPrevious,
            OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
            _expression = expression;
            _orderBy = orderBy;
            _reverted = reverted;
            _clearPrevious = clearPrevious;
        }

        /// <summary>
        ///     执行映射
        /// </summary>
        /// <param name="context">关系运算上下文</param>
        public override void Execute(RopContext context)
        {
            //如果_clearPrevious == true且RopContext.HasOrdered == true时，清理resultSql.Orders，否则不清理。
            //清理后须将RopContext.HasOrdered设置为false。
            //执行排序后将RopContext.HasOrdered设置为true。

            if (context.ResultSql.TakeNumber > 0) context.AcceptResult();
            context.ExpandSource(_expression);
            var settr = new SourceAliasRootSetter(context.AliasRoot);
            _orderBy.Accept(settr);
            //清理时要只清理由执行器添加的排序
            //因为在构造RopContext时添加的主键排序是为了保证在执行排序的字段值相同的时候仍可以让主键相同的记录相邻 所以不能清理掉

            //查找哪些是由执行器添加的排序
            var clearCount = context.ResultSql.Orders.Count(p => p.IsAddByExecutor);

            //要清理的情况
            if (_clearPrevious && context.HasOrdered)
            {
                //因为是使用Insert插入的 所有的由执行器插入的排序都在Count这个索引前面 所以可以直接删除前面添加的排序
                for (var i = 0; i < clearCount; i++) context.ResultSql.Orders.RemoveAt(i);
                //如果清理了 需要将HasOrdered设置为false
                context.HasOrdered = false;
                //此处的值肯定为0 因为都清理掉了
                clearCount = 0;
            }

            //添加排序 并设置IsAddByExecutor为true 以供后续的执行器判断
            var order = new Order(_orderBy, _reverted ? EOrderDirection.Desc : EOrderDirection.Asc)
                { IsAddByExecutor = true };
            //插入到所有由执行器添加的排序后面
            context.ResultSql.Orders.Insert(clearCount, order);
            context.HasOrdered = true;
            //如果存在同一个Field的 在终结点中处理
            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}