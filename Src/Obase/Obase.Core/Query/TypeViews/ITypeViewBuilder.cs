/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：公开构造类型视图的方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:01:00
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     公开构造类型视图的方法。
    /// </summary>
    public interface ITypeViewBuilder
    {
        /// <summary>
        ///     构造类型视图。
        /// </summary>
        /// <param name="viewExp">视图表达式。</param>
        /// <param name="source">视图源。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="sourcePara">视图表达式中代表视图源的形式参数。</param>
        /// <param name="paraBindings">形参绑定。</param>
        TypeView Build(Expression viewExp, StructuralType source, ObjectDataModel model,
            ParameterExpression sourcePara, params ParameterBinding[] paraBindings);
    }
}