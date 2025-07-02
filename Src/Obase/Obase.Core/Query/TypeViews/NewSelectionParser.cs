/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定于实例化投影运算的视图查询解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:50:32
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     特定于实例化投影运算的视图查询解析器。
    ///     实例化投影运算是指投影函数的Body为New或MemberInit表达式的投影运算。
    ///     它有一个可选的集合选择器参数collectionSelector和一个投影函数参数resultSelector，其中：
    ///     （1）collectionSelector的类型为Func`2[TSource, IEnumerable`1[TCollection]] 或Func`3[TSource, Int32,
    ///     IEnumerable`1[TCollection]]；
    ///     （2）resultSelector的类型可能为Func`2[TSource, TResult]、Func`3[TSource, Int32, TResult]或Func`3[TSource, TCollection,
    ///     TResult]（仅当collectionSelector存在）。
    /// </summary>
    /// 实施说明:
    /// resultSelector的第一个形参的模型类型为视图源。
    /// collectionSelector存在且有两个形参时，第二个指代Index。
    /// resultSelector为视图表达式，如果它有两个形参，第二个形参：
    /// （1）当collectionSelector存在时，绑定到collectionSelector；
    /// （2）否则，指代Index。
    /// 该视图无标识属性。
    /// 当collectionSelector存在时，它指定该视图的平展点。
    internal class NewSelectionParser : ExpressionBasedViewQueryParser
    {
        /// <summary>
        ///     从查询运算抽取代表平展点的表达式。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="flatteningPara">返回平展形参。</param>
        protected override LambdaExpression ExtractFlatteningExpression(QueryOp queryOp,
            out ParameterExpression flatteningPara)
        {
            flatteningPara = null;
            //只能处理中介投影运算
            if (queryOp is CollectionSelectOp collectionSelectOp)
            {
                flatteningPara = collectionSelectOp.CollectionSelector.Parameters[0];
                return collectionSelectOp.CollectionSelector;
            }

            return null;
        }

        /// <summary>
        ///     从查询运算抽取视图的标识属性。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected override string[] ExtractKeyAttributes(QueryOp queryOp)
        {
            return Array.Empty<string>();
        }


        /// <summary>
        ///     从查询运算抽取视图表达式涉及的形参绑定。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected override ParameterBinding[] ExtractParameterBinding(QueryOp queryOp)
        {
            var bindings = new List<ParameterBinding>();
            //根据是中介投影还是普通投影，抽取形参绑定。
            if (queryOp is CollectionSelectOp collectionSelectOp)
            {
                if (collectionSelectOp.ResultSelector.Parameters.Count == 2)
                {
                    if (collectionSelectOp.ResultSelector.Parameters[0].Type == typeof(int))
                        bindings.Add(new ParameterBinding(collectionSelectOp.ResultSelector.Parameters[1],
                            EParameterReferring.Index));
                    else
                        bindings.Add(new ParameterBinding(collectionSelectOp.ResultSelector.Parameters[1],
                            collectionSelectOp.CollectionSelector.Body));
                }
            }
            else if (queryOp is SelectOp selectOp)
            {
                if (selectOp.ResultSelector.Parameters.Count == 2)
                    bindings.Add(new ParameterBinding(selectOp.ResultSelector.Parameters[1],
                        EParameterReferring.Index));
            }
            else
            {
                throw new ArgumentException("NewSelectionParser从查询运算抽取视图表达式涉及的形参绑定失败。");
            }

            return bindings.ToArray();
        }

        /// <summary>
        ///     从查询运算抽取描述视图结构的Lambda表达式（简称视图表达式），后续将据此表达式构造TypeView实例。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="viewType">视图的CLR类型。</param>
        protected override LambdaExpression ExtractViewExpression(QueryOp queryOp, Type viewType)
        {
            if (queryOp is SelectOp selectOp)
                return selectOp.ResultSelector;
            throw new ArgumentException("NewSelectionParser从查询运算抽取视图表达式失败。");
        }

        /// <summary>
        ///     从查询运算抽取视图源的CLR类型。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected override Type ExtractSourceType(QueryOp queryOp)
        {
            return queryOp.SourceType;
        }

        /// <summary>
        ///     从查询运算抽取视图的CLR类型。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected override Type ExtractViewType(QueryOp queryOp)
        {
            if (queryOp is SelectOp selectOp)
                return selectOp.ResultType;
            throw new ArgumentException("NewSelectionParser从查询运算抽取视图的CLR类型失败。");
        }
    }
}