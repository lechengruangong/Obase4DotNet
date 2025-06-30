/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定于多重投影运算的视图查询解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:05:08
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     特定于多重投影运算的视图查询解析器。
    ///     多重投影是指投影到一个具有多重性的引用元素或其下级元素（下级元素不要求多重性）的运算。
    ///     下级元素是指关联树中代表当前元素的节点的后代所代表的元素，或者是当前节点或其后代所含属性树节点所代表的属性。
    ///     它有一个可选的集合选择器参数collectionSelector和一个投影函数参数resultSelector，其中：
    ///     （1）collectionSelector的类型为Func`2[TSource, IEnumerable[TCollection]] 或Func`3[TSource, Int32,
    ///     IEnumerable[TCollection]]；
    ///     （2）resultSelector的类型可能为Func`2[TSource, TResult]、Func`3[TSource, Int32, TResult] 或Func`3[TSource, TCollection,
    ///     TResult]（仅当collectionSelector存在）。
    /// </summary>
    /// 实施说明:
    /// 可拆分为两步。
    /// 第一步投影到一个隐含的类型视图，该视图是MultipleSelectionResult`2[TSource, TResult]的派生类型。
    /// 该视图没有属性，只有一个由resultSelector规定的视图引用。
    /// collectionSelector存在且有两个形参时，第二个指代Index。
    /// resultSelector有第二个形参时，当collectionSelector存在时该形参绑定到它，否则指代Index。
    /// 第二步执行退化投影，退化到上述视图引用。
    ///  
    /// 使用ImpliedTypeManager.ApplyType(baseType, fields, subIdentity)构建上述隐含视图，其中：
    /// （1）baseType为MultipleSelectionResult`2[TSource, TResult]；
    /// （2）fields根据上述视图属性和引用生成，（不需指定字段名称，但需指定valueExpression及形参绑定）；
    /// （3）subIdentity为queryOp.SourceType.FullType。
    public class MultipleSelectionParser : ExpressionBasedViewQueryParser
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
            //可能是集合中介投影或者合并的多重投影运算
            var bindings = new List<ParameterBinding>();
            if (queryOp is CollectionSelectOp collectionSelectOp)
            {
                if (collectionSelectOp.CollectionSelector.Parameters.Count == 2)
                    bindings.Add(new ParameterBinding(collectionSelectOp.CollectionSelector.Parameters[1],
                        EParameterReferring.Index));
                if (collectionSelectOp.ResultSelector.Parameters.Count == 2)
                    bindings.Add(new ParameterBinding(collectionSelectOp.ResultSelector.Parameters[1],
                        collectionSelectOp.CollectionSelector.Body));
            }
            else if (queryOp is CombiningSelectOp combiningSelectOp)
            {
                if (combiningSelectOp.ResultSelector.Parameters.Count == 2)
                    bindings.Add(new ParameterBinding(combiningSelectOp.ResultSelector.Parameters[1],
                        EParameterReferring.Index));
            }
            else
            {
                throw new Exception("MultipleSelectionParser从查询运算抽取视图表达式涉及的形参绑定失败。");
            }

            return bindings.ToArray();
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
        ///     从查询运算抽取描述视图结构的Lambda表达式（简称视图表达式），后续将据此表达式构造TypeView实例。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="viewType">视图的CLR类型。</param>
        protected override LambdaExpression ExtractViewExpression(QueryOp queryOp, Type viewType)
        {
            if (!(queryOp is SelectOp selectOp))
                throw new Exception($"MultipleSelectionParser视图查询解析器，不能解析{queryOp.Name}");

            //创建隐含视图
            var baseType = typeof(MultipleSelectionResult<,>).MakeGenericType(selectOp.SourceType, selectOp.ResultType);
            var bindings = ExtractParameterBinding(queryOp);
            var viewRef = new FieldDescriptor(selectOp.ResultSelector, bindings) { HasGetter = true, HasSetter = true };
            var subIdentity = new IdentityArray(selectOp.SourceType.FullName);
            var typeImpliedView = ImpliedTypeManager.Current.ApplyType(baseType, new[] { viewRef }, subIdentity);

            //构造一个委托 用于生成字段
            string NamingFunc()
            {
                //字段前半部分
                var filedStart = viewRef.HasGetter || viewRef.HasSetter ? "_field_" : "Field_";
                return $"{filedStart}1";
            }

            //表达式构建
            /*1.创建NewExpression*/
            var newExp = Expression.New(typeImpliedView.GetConstructor(Type.EmptyTypes) ??
                                        throw new ArgumentException(
                                            $"提取多重投影运算的视图失败,{typeImpliedView.FullName}没有无参构造函数."));


            /*2.初始化成员表达式*/
            viewRef.GetName(NamingFunc);
            var refPropertyNmae = viewRef.GetPropertyName(); //获取视图引用property名称
            var memberBindings = new MemberBinding[]
            {
                //当前类(MultipleSelectionParser)的“ResultSelector”表达式的Body,绑定给目标类型(MultipleSelectionResult<,>派生类)的"_result"字段
                Expression.Bind(
                    typeImpliedView.GetProperty(refPropertyNmae, BindingFlags.NonPublic | BindingFlags.Instance) ??
                    throw new ArgumentException($"提取多重投影运算的视图失败,无法获取名字为{refPropertyNmae}的视图引用.")
                    , selectOp.ResultSelector.Body
                )
            }; //创建成员绑定
            var memberInitExp = Expression.MemberInit(newExp, memberBindings); //创建"初始化成员表达式"
            /*3.创建Lambda表达式*/
            var lambdaExp = Expression.Lambda(memberInitExp, selectOp.ResultSelector.Parameters); //创建lambda表达式
            return lambdaExp;
        }


        /// <summary>
        ///     从查询运算抽取视图的CLR类型。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected override Type ExtractViewType(QueryOp queryOp)
        {
            if (queryOp is SelectOp selectOp)
                return selectOp.ResultType;
            throw new Exception("MultipleSelectionParser从查询运算抽取视图的CLR类型失败。");
        }
    }

    /// <summary>
    ///     投影结果
    /// </summary>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    public abstract class MultipleSelectionResult<TSource, TResult>
    {
        /// <summary>
        /// </summary>
        protected TResult _result;

        /// <summary>
        /// </summary>
        protected TSource _source;
    }
}