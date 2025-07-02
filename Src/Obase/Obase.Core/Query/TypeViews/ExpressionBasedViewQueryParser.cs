/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于表达式的视图查询解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 10:32:03
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     为基于表达式的视图查询解析器提供基础实现。
    /// </summary>
    public abstract class ExpressionBasedViewQueryParser : ViewQueryParser
    {
        /// <summary>
        ///     抽取到的类型视图实例
        /// </summary>
        private TypeView _typeView;

        /// <summary>
        ///     创建视图实例。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="viewType">视图的CLR类型。</param>
        /// <param name="sourceType">视图源的CLR类型。</param>
        /// <param name="model">对象数据模型。</param>
        protected override TypeView CreateView(QueryOp queryOp, Type viewType, Type sourceType,
            ObjectDataModel model)
        {
            if (_typeView != null) return _typeView;
            var source = model.GetStructuralType(ExtractSourceType(queryOp));
            var lambda = ExtractViewExpression(queryOp, viewType);
            var paraBindings = ExtractParameterBinding(queryOp);
            //调用工厂方法获取对应的建造器
            var builder = new TypeViewBuilderFactory().Create(lambda);
            var typeView = builder.Build(lambda.Body, source, model, Expression.Parameter(sourceType), paraBindings);
            typeView.KeyAttributes = ExtractKeyAttributes(queryOp);
            //如果可以抽取到平展表达式
            var lambda1 = ExtractFlatteningExpression(queryOp, out _);
            if (lambda1 != null)
            {
                //加入平展点
                lambda1.Body.ExtractAssociation(model, out AssociationTreeNode assoTail, paraBindings);
                if (assoTail != null) typeView.AddFlatteningPoint(assoTail);
            }

            return _typeView ?? (_typeView = typeView);
        }

        /// <summary>
        ///     从查询运算抽取代表平展点的表达式。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="flatteningPara">返回平展形参。</param>
        protected abstract LambdaExpression ExtractFlatteningExpression(QueryOp queryOp,
            out ParameterExpression flatteningPara);

        /// <summary>
        ///     从查询运算抽取视图的标识属性。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected abstract string[] ExtractKeyAttributes(QueryOp queryOp);

        /// <summary>
        ///     从查询运算抽取视图表达式涉及的形参绑定。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected abstract ParameterBinding[] ExtractParameterBinding(QueryOp queryOp);

        /// <summary>
        ///     从查询运算抽取描述视图结构的Lambda表达式（简称视图表达式），后续将据此表达式构造TypeView实例。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="viewType">视图的CLR类型。</param>
        protected abstract LambdaExpression ExtractViewExpression(QueryOp queryOp, Type viewType);
    }
}