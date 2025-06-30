/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定于（普通）分组运算的视图查询解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 10:38:43
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     特定于（普通）分组运算的视图查询解析器。
    ///     分组运算是指GroupBy(keySelector, elementSelector)，其中：
    ///     （1）keySelector为键选择器，类型为Func`2[TSource, TKey];
    ///     （2）elementSelector为组元素选择器，类型为Func`2[TSource, TElement]。
    /// </summary>
    /// 实施说明:
    /// 本运算可以拆分成两步。
    /// 第一步，投影到一个隐含的类型视图，该视图是SelectionResult`3[TSource, TKey, TElement]的派生类型。它有一个绑定到keySelector的视图属性。
    /// 另外：
    /// （1）当elementSelector投影到对象类型（即TElement为ObjectType）时，有一个视图引用，其锚点和绑定由elementSelector规定；
    /// （2）当elementSelector投影到基元类型或复杂类型时，有一个视图属性或视图复杂属性，其锚点和绑定由elementSelector规定。
    /// 该视图没有标识属性。
    /// 第二步，对投影结果进行分组运算。
    ///  
    /// 使用ImpliedTypeManager.ApplyType(baseType, fields, subIdentity) 构建上述隐含视图，其中：
    /// （1）baseType为SelectionResult`3[TSource, TKey, TElement]；
    /// （2）fields根据上述视图属性和引用生成，（不需指定字段名称，但需指定valueExpression及形参绑定）；
    /// （3）subIdentity为queryOp.SourceType.FullType。
    public class GroupingParser : ExpressionBasedViewQueryParser
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
            return Array.Empty<ParameterBinding>();
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
            if (!(queryOp is GroupOp groupOp)) throw new Exception("GroupingParser从查询运算抽取视图表达式失败。");

            //创建隐含视图
            var baseType =
                typeof(SelectionResult<,,>).MakeGenericType(groupOp.SourceType, groupOp.KeyType, groupOp.ElementType);
            var valueExpression = Expression.Lambda(Expression.Parameter(viewType));
            var bindings = ExtractParameterBinding(queryOp);
            var viewAttr = new FieldDescriptor(groupOp.KeySelector, bindings)
                { HasGetter = true, HasSetter = true, CreateConstructorParameter = queryOp.Heterogeneous() };
            var viewRef = new FieldDescriptor(groupOp.ElementSelector ?? valueExpression, bindings)
                { HasGetter = true, HasSetter = true, CreateConstructorParameter = queryOp.Heterogeneous() };
            var fields = new[] { viewAttr, viewRef };
            var subIdentity = new IdentityArray(groupOp.SourceType.FullName);
            //源
            var parameterTypes = new[]
            {
                viewAttr.Type, viewRef.Type
            };
            var typeImpliedView =
                ImpliedTypeManager.Current.ApplyType(baseType, fields, subIdentity);


            var index = 0;

            //构造一个委托 用于生成字段
            string NamingFunc()
            {
                var field = fields[index];
                //字段前半部分
                var filedStart = field.HasGetter || field.HasSetter ? "_field_" : "Field_";
                return $"{filedStart}{++index}";
            }


            //统一表达式参数
            var parameter = groupOp.KeySelector.Parameters[0];
            var visitor = new ParameterVisitor(parameter);
            var keySelector = groupOp.KeySelector;
            var newElementSelector = (LambdaExpression)visitor.Visit(groupOp.ElementSelector);
            if (newElementSelector == null)
                throw new ArgumentException("提取分组运算的视图失败，分组运算的ElementSelector不能为空。");

            var newExpression = queryOp.Heterogeneous()
                ? Expression.New(
                    typeImpliedView.GetConstructor(parameterTypes) ?? throw new ArgumentException(
                        $"提取分组运算的视图失败,{typeImpliedView.FullName}没有参数为{string.Join(",", parameterTypes.Select(p => p.FullName))}的构造函数."),
                    parameterTypes.Select(Expression.Parameter))
                : Expression.New(typeImpliedView.GetConstructor(Type.EmptyTypes) ??
                                 throw new ArgumentException($"提取分组运算的视图失败,{typeImpliedView.FullName}没有无参构造函数."));
            //视图属性
            viewAttr.GetName(NamingFunc);
            //获取视图属性property名称
            var arrtPropertyNmae = viewAttr.GetPropertyName();
            //获取视图属性
            var attrProperty =
                typeImpliedView.GetProperty(arrtPropertyNmae, BindingFlags.NonPublic | BindingFlags.Instance);
            //绑定到视图属性
            var attrBindExp =
                Expression.Bind(
                    attrProperty ?? throw new ArgumentException($"提取分组运算的视图失败,无法获取名字为{arrtPropertyNmae}的视图属性."),
                    keySelector.Body);

            //视图引用
            viewRef.GetName(NamingFunc);
            //获取视图引用property名称
            var refPropertyNmae = viewRef.GetPropertyName();
            //获取视图引用
            var refProperty =
                typeImpliedView.GetProperty(refPropertyNmae, BindingFlags.NonPublic | BindingFlags.Instance);
            //绑定到视图引用
            var refBindExp =
                Expression.Bind(
                    refProperty ?? throw new ArgumentException($"提取分组运算的视图失败,无法获取名字为{refPropertyNmae}的视图引用."),
                    newElementSelector.Body);

            var memberInitExp = Expression.MemberInit(newExpression, attrBindExp, refBindExp);
            return Expression.Lambda(memberInitExp, parameter);
        }


        /// <summary>
        ///     从查询运算抽取视图的CLR类型。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected override Type ExtractViewType(QueryOp queryOp)
        {
            if (queryOp is GroupOp groupOp)
                return groupOp.ElementType;
            throw new ArgumentException("GroupingParser从查询运算抽取视图的CLR类型失败。");
        }

        /// <summary>
        ///     参数访问器
        ///     替换表达式参数为指定参数。
        /// </summary>
        private class ParameterVisitor : ExpressionVisitor
        {
            /// <summary>
            ///     替换的参数表达式
            /// </summary>
            private readonly ParameterExpression _parExp;

            /// <summary>
            ///     通过指定参数实例化ParameterVisitor。
            /// </summary>
            /// <param name="parExp">指定参数。</param>
            internal ParameterVisitor(ParameterExpression parExp)
            {
                _parExp = parExp;
            }

            /// <summary>
            ///     访问参数表达式
            /// </summary>
            /// <param name="node">参数表达式</param>
            /// <returns></returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parExp;
            }
        }
    }

    /// <summary>
    ///     投影结果
    /// </summary>
    /// <typeparam name="TSource">源类型</typeparam>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TElement">元素类型</typeparam>
    public abstract class SelectionResult<TSource, TKey, TElement>
    {
        /// <summary>
        /// </summary>
        protected TElement _element;

        /// <summary>
        /// </summary>
        protected TKey _key;

        /// <summary>
        /// </summary>
        protected TSource _source;
    }
}