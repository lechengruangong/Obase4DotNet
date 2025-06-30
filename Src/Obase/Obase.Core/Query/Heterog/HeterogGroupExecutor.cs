/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构分组运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:28:10
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Query.TypeViews;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构分组运算执行器。
    /// </summary>
    public class HeterogGroupExecutor : HeterogOpExecutor
    {
        /// <summary>
        ///     初始化HeterogOpExecutor类的新实例。
        /// </summary>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        public HeterogGroupExecutor(Func<StorageSymbol, IStorageProvider> storageProviderCreator, ObjectDataModel model,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback) : base(storageProviderCreator, model,
            preexecutionCallback, postexecutionCallback)
        {
        }

        /// <summary>
        ///     执行异构运算。
        /// </summary>
        /// <param name="heterogOp">要执行的异构运算。</param>
        /// <param name="heterogQuery">要执行的异构运算所在的查询链，它是该查询链的末节点。</param>
        /// <param name="including">包含树。</param>
        /// <param name="attachObject">附加委托</param>
        /// <param name="attachRoot">是否附加根对象</param>
        public override object Execute(QueryOp heterogOp, QueryOp heterogQuery, AssociationTree including,
            AttachObject attachObject, bool attachRoot = true)
        {
            if (heterogOp is GroupOp groupOp)
            {
                var groupParser = new GroupingParser();
                var typeView = groupParser.Parse(heterogOp, Model);

                var typeViewExp = typeView.GenerateExpression(out _);

                var resultSelector = groupOp is GroupAggregationOp groupAggregation
                    ? groupAggregation.ResultSelector
                    : null;
                var comparer = groupOp.Comparer;
                var elementSelector = GetElementSelector(typeView, typeViewExp);
                var keySelecotr = GetKeySelector(typeView, typeViewExp);
                //转换表达式至异构查询的下一节
                var newOp = resultSelector == null
                    ? QueryOp.GroupBy(keySelecotr, elementSelector, comparer, Model)
                    : QueryOp.GroupBy(keySelecotr, elementSelector, resultSelector, comparer, Model);
                //与上一节拼接
                newOp = QueryOp.Select(typeView, Model, newOp);

                if (including != null)
                {
                    var newIncluding = elementSelector.ExtractAssociation(Model, assoTail: out var tail);
                    tail.AddChild(including.Node.Children);
                    newOp = IncludeOp.Create(newIncluding, Model, newOp);
                }

                var newQuery = heterogQuery.ReplaceTail(newOp);
                HeterogQueryProvider.AttachRoot = false;
                return HeterogQueryProvider.Execute(newQuery, including);
            }

            throw new ArgumentException("异构分组运算执行器只能处理GroupAggregationOp", nameof(heterogOp));
        }

        /// <summary>
        ///     根据视图获取键表达式
        /// </summary>
        /// <param name="typeView">视图类型</param>
        /// <param name="typeViewExp">视图类型的表达式</param>
        /// <returns></returns>
        private LambdaExpression GetKeySelector(TypeView typeView, LambdaExpression typeViewExp)
        {
            var type = typeViewExp.ReturnType;
            var parameterExp = Expression.Parameter(type, "p");
            var propertyInfo =
                type.GetProperty(typeView.Attributes[0].Name, BindingFlags.NonPublic | BindingFlags.Instance);
            var memberExp = Expression.MakeMemberAccess(parameterExp,
                propertyInfo ?? throw new ArgumentException($"{typeView.FullName}没有{typeView.Attributes[0].Name}属性."));
            return Expression.Lambda(memberExp, parameterExp);
        }

        /// <summary>
        ///     根据视图获取组元素表达式
        /// </summary>
        /// <param name="typeView">视图类型</param>
        /// <param name="typeViewExp">视图类型的表达式</param>
        /// <returns></returns>
        private LambdaExpression GetElementSelector(TypeView typeView, LambdaExpression typeViewExp)
        {
            var type = typeViewExp.ReturnType;
            var parameterExp = Expression.Parameter(type, "p");
            var propertyInfo =
                type.GetProperty(typeView.Attributes[1].Name, BindingFlags.NonPublic | BindingFlags.Instance);
            var memberExp = Expression.MakeMemberAccess(parameterExp,
                propertyInfo ?? throw new ArgumentException($"{typeView.FullName}没有{typeView.Attributes[1].Name}属性."));
            return Expression.Lambda(memberExp, parameterExp);
        }
    }
}