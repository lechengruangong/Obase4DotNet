/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象构造器接口,提供构造对象的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 12:00:54
└──────────────────────────────────────────────────────────────┘
*/


using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     对象构造器接口。
    ///     对象构造器用于构造类型（实体型、复杂类型、关联型）的对象。
    /// </summary>
    public interface IInstanceConstructor
    {
        /// <summary>
        ///     获取构造函数的形式参数。
        /// </summary>
        List<Parameter> Parameters { get; }

        /// <summary>
        ///     获取或设置要构造的对象的类型。
        /// </summary>
        StructuralType InstanceType { get; set; }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments">构造函数参数。</param>
        object Construct(object[] arguments = null);

        /// <summary>
        ///     获取绑定到指定元素的构造函数参数。
        /// </summary>
        /// <param name="elementName">元素名称。</param>
        Parameter GetParameterByElement(string elementName);
    }
}