/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置参数的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:35:55
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置参数的规范。
    /// </summary>
    public interface IParameterConfigurator
    {
        /// <summary>
        ///     在所有参数配置完成后返回到当前类型。
        /// </summary>
        /// <exception cref="Exception">还有参数没有配置，不能返回。</exception>
        IStructuralTypeConfigurator End();

        /// <summary>
        ///     从构造函数参数队列取出一项，将之绑定到指定的类型元素。
        /// </summary>
        /// <param name="elementName">绑定元素的名称。</param>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        void Map(string elementName, Func<object, object> valueConverter = null);

        /// <summary>
        ///     从构造函数参数队列取出一项，将之绑定到同名类型元素。
        /// </summary>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        void MapDefault(Func<object, object> valueConverter = null);
    }
}