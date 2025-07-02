/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定于分组聚合运算的视图查询解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 10:34:51
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     特定于分组聚合运算的视图查询解析器。
    ///     分组聚合运算是指GroupBy(keySelector, elementSelector, resultSelector)，其中：
    ///     （1）keySelector为键选择器，类型为，类型为Func`2[TSource, TKey];
    ///     （2）elementSelector为组元素选择器，为可选参数，类型为Func`2[TSource, TElement];
    ///     （3）resultSelector为投影函数，当eleementSelector为空时其类型为Func`3[TKey, IEnumerable`1[TSource], TResult]，否则其类型为Func`3[TKey,
    ///     IEnumerable`1[TElement], TResult]。
    /// </summary>
    /// 实施说明:
    /// keySelector的第一个形参的模型类型为视图源。
    /// resultSelector为视图表达式，其中：第一个形参绑定到keySelector；对于第二个形参，如果elementSelector不存在指代到Sequence，否则绑定到elementSelector并指代Sequence。
    /// 该视图无标识属性。
    public class GroupingAggregationParser : ExpressionBasedViewQueryParser
    {
        /// <summary>
        ///     提取平展表达式
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="flatteningPara">平展表达式</param>
        /// <returns></returns>
        protected override LambdaExpression ExtractFlatteningExpression(QueryOp queryOp,
            out ParameterExpression flatteningPara)
        {
            flatteningPara = null;
            return null;
        }

        /// <summary>
        ///     提取键属性
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <returns></returns>
        protected override string[] ExtractKeyAttributes(QueryOp queryOp)
        {
            return Array.Empty<string>();
        }

        /// <summary>
        ///     提取形参绑定
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <returns></returns>
        protected override ParameterBinding[] ExtractParameterBinding(QueryOp queryOp)
        {
            var bindings = new List<ParameterBinding>();
            if (!(queryOp is GroupAggregationOp op))
                throw new ArgumentException("GroupingAggregationParser从查询运算抽取视图表达式涉及的形参绑定失败");
            //resultSelector第一个形参绑定到keySelector；
            bindings.Add(new ParameterBinding(op.ResultSelector.Parameters[0],
                op.KeySelector.Body));
            if (op.ResultSelector.Parameters.Count == 2)
            {
                //elementSelector不存在
                if (op.ElementSelector == null)
                    //resultSelector第二个形参指代到Sequence
                    bindings.Add(new ParameterBinding(op.ResultSelector.Parameters[1],
                        EParameterReferring.Sequence));

                //elementSelector存在
                else
                    //resultSelector第二个形参绑定到elementSelector并指代Sequence
                    bindings.Add(new ParameterBinding(op.ResultSelector.Parameters[1], EParameterReferring.Sequence,
                        op.ElementSelector.Body));
            }

            return bindings.ToArray();
        }

        /// <summary>
        ///     提取源类型
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <returns></returns>
        protected override Type ExtractSourceType(QueryOp queryOp)
        {
            return queryOp.SourceType;
        }

        /// <summary>
        ///     从查询运算抽取描述视图结构的Lambda表达式（简称视图表达式），后续将据此表达式构造TypeView实例。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="viewType">视图的CLR类型。</param>
        protected override LambdaExpression ExtractViewExpression(QueryOp queryOp, Type viewType)
        {
            if (queryOp is GroupAggregationOp groupAggregationOp)
                return groupAggregationOp.ResultSelector;
            throw new ArgumentException("GroupingAggregationParser从查询运算抽取视图表达式失败。");
        }

        /// <summary>
        ///     从查询运算抽取视图的CLR类型。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected override Type ExtractViewType(QueryOp queryOp)
        {
            if (queryOp is GroupAggregationOp groupAggregationOp)
                return groupAggregationOp.ResultType;
            throw new ArgumentException("GroupingAggregationParser从查询运算抽取视图的CLR类型失败。");
        }
    }
}