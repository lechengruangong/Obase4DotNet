/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图查询解析器基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 10:30:39
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     为视图查询解析器提供基础实现。
    ///     视图查询是指一种查询运算，在运算过程中会将源类型投影（Select）成视图，它可能是投影运算,也可能是逻辑蕴涵投影运算的其它运算。
    ///     视图查询解析器的用途是分析查询运算从中解析出视图类型。
    /// </summary>
    public abstract class ViewQueryParser
    {
        /// <summary>
        ///     创建视图实例。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="viewType">视图的CLR类型。</param>
        /// <param name="sourceType">视图源的CLR类型。</param>
        /// <param name="model">对象数据模型。</param>
        protected abstract TypeView CreateView(QueryOp queryOp, Type viewType, Type sourceType,
            ObjectDataModel model);

        /// <summary>
        ///     从查询运算抽取视图源的CLR类型。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected abstract Type ExtractSourceType(QueryOp queryOp);

        /// <summary>
        ///     从查询运算抽取视图的CLR类型。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        protected abstract Type ExtractViewType(QueryOp queryOp);

        /// <summary>
        ///     执行解析操作。
        /// </summary>
        /// <param name="queryOp">要解析的查询运算。</param>
        /// <param name="model">对象数据模型。</param>
        /// <returns>解析出的类型视图。</returns>
        /// 实施说明
        /// 参见活动图“解析视图查询”。
        public TypeView Parse(QueryOp queryOp, ObjectDataModel model)
        {
            var candiateType = ExtractViewType(queryOp);
            var sourceType = ExtractSourceType(queryOp);
            var modelType = model.GetStructuralType(candiateType);
            Type dirivedType = null;
            if (modelType != null && modelType is TypeView typeView)
            {
                if (typeView.Source.ClrType == sourceType) return typeView;
                //从缓存中获取派生类型
                dirivedType =
                    ImpliedTypeManager.Current.ApplyType(candiateType,
                        new IdentityArray(typeView.Source.FullName));
                //从模型中获取模型视图
                var modelTypeView = model.GetTypeView(dirivedType);
                if (modelTypeView != null) return modelTypeView;
            }

            //创建视图。
            var view = CreateView(queryOp, candiateType, sourceType, model);
            view.SetModel(model);
            //缓存生成的派生类型
            if (dirivedType != null) view.ProxyType = dirivedType;
            //将视图添加到模型
            model.AddType(view);
            return view;
        }
    }
}