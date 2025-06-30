/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图查询解析器工厂.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:49:30
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     视图查询解析器工厂。
    /// </summary>
    public class ViewQueryParserFactory
    {
        /// <summary>
        ///     针对指定的查询运算创建视图查询解析器实例。
        /// </summary>
        /// <returns>返回解析器实例。如果指定的查询运算不是视图查询返回null。</returns>
        /// <param name="queryOp">被解析的查询运算。</param>
        /// 实施说明:
        /// 根据查询运算类型实例化相应的解析器。
        /// 对于分组（普通）运算，实例化GroupingParser，但若使用了比较器或未使用元素投影函数，均不属于视图查询。
        /// 对于分组（聚合）运算，实例化GroupingAggregationParser，但若使用了比较器或IsNew==false，均不属于视图查询。
        /// 对于投影运算，如果ResultType是IEnumerable，且从投影表达式中抽取的关联树有子节点，实例化MultipleParser。
        /// 对于投影运算，如果IsNew==true，实例化NewSelectionParser。
        public ViewQueryParser Create(QueryOp queryOp)
        {
            ViewQueryParser parser = null;
            switch (queryOp.Name)
            {
                case EQueryOpName.Select:
                {
                    var op = (SelectOp)queryOp;
                    //投影运算，如果ResultType是IEnumerable，且从投影表达式中抽取的关联树有子节点，实例化MultipleParser。
                    if (op.IsMultiple)
                        parser = new MultipleSelectionParser();
                    // 对于投影运算，如果IsNew==true，实例化NewSelectionParser。
                    else if (op.IsNew)
                        parser = new NewSelectionParser();
                }
                    break;
                case EQueryOpName.Group:
                {
                    //分组（聚合）运算
                    if (queryOp is GroupAggregationOp groupAggregationOp)
                    {
                        if (groupAggregationOp.Comparer == null && groupAggregationOp.IsNew)
                            parser = new GroupingAggregationParser();
                    }
                    //分组（普通）运算
                    else if (queryOp is GroupOp groupOp)
                    {
                        if (groupOp.Comparer == null && groupOp.ElementSelector != null)
                            parser = new GroupingParser();
                    }
                }
                    break;
            }

            return parser;
        }
    }
}