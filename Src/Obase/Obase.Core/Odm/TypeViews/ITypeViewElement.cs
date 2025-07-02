/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义视图元素规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 16:22:30
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     定义视图元素规范。
    /// </summary>
    public interface ITypeViewElement
    {
        /// <summary>
        ///     获取元素的类型。
        /// </summary>
        EElementType ElementType { get; }

        /// <summary>
        ///     获取或设置一个值，该值指示元素是否具有多重性，即其值是否为集合类型。
        /// </summary>
        bool IsMultiple { get; set; }

        /// <summary>
        ///     获取元素的名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     获取元素宿主对象的类型。
        /// </summary>
        StructuralType HostType { get; }

        /// <summary>
        ///     获取或设置影子元素。
        /// </summary>
        ITypeViewElement Shadow { get; set; }

        /// <summary>
        ///     获取或设置取值器。
        /// </summary>
        IValueGetter ValueGetter { get; set; }

        /// <summary>
        ///     获取或设置设值器。
        /// </summary>
        IValueSetter ValueSetter { get; set; }


        /// <summary>
        ///     生成在视图表达式中定义当前元素的表达式，它规定了该元素的锚点和绑定。
        /// </summary>
        /// <returns>定义当前元素的表达式。</returns>
        /// <param name="sourcePara">代表视图源的形参。</param>
        /// <param name="flatteningParaGetter">一个委托，用于获取代表指定平展点的形参。</param>
        Expression GenerateExpression(ParameterExpression sourcePara,
            Func<AssociationTreeNode, ParameterExpression> flatteningParaGetter);

        /// <summary>
        ///     从指定对象取出当前元素的值。
        /// </summary>
        /// <returns>如果元素具有多重性，返回IEnumerable`1[T]，否则返回object。</returns>
        /// <param name="targetObj">要取其元素值的对象。</param>
        object GetValue(object targetObj);

        /// <summary>
        ///     为指定对象的当前元素设置值，适用于具有多重性的元素。
        /// </summary>
        /// <param name="targetObj">要为其元素设值的对象。</param>
        /// <param name="value">元素的值。</param>
        void SetValue(object targetObj, IEnumerable value);

        /// <summary>
        ///     为指定对象的当前元素设置值，适用于不具多重性的元素。
        /// </summary>
        /// <param name="targetObj">要为其元素设值的对象。</param>
        /// <param name="value">元素的值。</param>
        void SetValue(object targetObj, object value);
    }
}