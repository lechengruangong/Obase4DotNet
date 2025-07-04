/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:47:33
└──────────────────────────────────────────────────────────────┘
*/

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

            //查找排序的源类型主键
            var keyCount = 1;
            var objectKeyFields = QueryOp.Model.GetObjectType(QueryOp.SourceType)?.KeyFields;
            if (objectKeyFields != null && objectKeyFields.Count > 0)
                keyCount = objectKeyFields.Count;

            //要清理的
            if (_clearPrevious && context.HasOrdered)
            {
                var needRemovedCount = context.ResultSql.Orders.Count - keyCount;
                for (var i = 0; i < needRemovedCount; i++) context.ResultSql.Orders.RemoveAt(i);
                context.HasOrdered = false;
            }

            var order = new Order(_orderBy, _reverted ? EOrderDirection.Desc : EOrderDirection.Asc);
            //加到主键的前面
            if (context.ResultSql.Orders.Count >= 1)
            {
                //检测当前的Orders数量是否大于等于主键数量 如果小于要添加的数量则将新排序添加到最前面即可
                if (context.ResultSql.Orders.Count - keyCount < 0)
                    keyCount = 0;
                context.ResultSql.Orders.Insert(context.ResultSql.Orders.Count - keyCount, order);
            }
            else
                context.ResultSql.Orders.Add(order);
            context.HasOrdered = true;
            //如果存在同一个Field的 在终结点中处理
            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}