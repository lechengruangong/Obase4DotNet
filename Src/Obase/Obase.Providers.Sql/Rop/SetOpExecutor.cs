/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：集运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:08:32
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     集运算执行器。
    /// </summary>
    public class SetOpExecutor : RopExecutor
    {
        /// <summary>
        ///     集运算操作符。
        /// </summary>
        private readonly ESetOperator _operator;

        /// <summary>
        ///     与当前查询源执行集运算的另一个集。
        /// </summary>
        private readonly ISetOperand _other;

        /// <summary>
        ///     另一个集的包含树。
        /// </summary>
        private AssociationTree _otherIncludings;

        /// <summary>
        ///     构造SetOpExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="other">与当前查询源执行集运算的另一个集。</param>
        /// <param name="eOperator">集运算操作符。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public SetOpExecutor(QueryOp queryOp, ISetOperand other, ESetOperator eOperator,
            OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
            _other = other;
            _operator = eOperator;
        }


        /// <summary>
        ///     获取或设置另一个集的包含树。
        /// </summary>
        public AssociationTree OtherIncludings
        {
            get => _otherIncludings;
            set => _otherIncludings = value;
        }

        /// <summary>
        ///     执行映射
        /// </summary>
        /// <param name="context">关系运算上下文</param>
        public override void Execute(RopContext context)
        {
            //     算法：
            //     QuerySet set = new QuerySet(resultSql, 操作数, 另一个集);
            //     string name = resultModelType.TargetTable;
            //     SetSource source = new SetSource(QuerySet, name);
            //     resultSql = new QuerySql(source)
            //     includings.Grow(_otherIncludings);

            var set = new QuerySet(context.ResultSql, _other, _operator);
            var name = (context.ResultModelType as ObjectType)?.TargetTable;
            var source = new SetSource(set, name);
            context.ResultSql = new QuerySql(source);
            //原投影列会消失 在此处增加一个通配列
            var wildcardColumn = new WildcardColumn
            {
                Source = source
            };
            context.ResultSql.SelectionSet.Add(wildcardColumn);
            context.Includings.Grow(_otherIncludings);
            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}