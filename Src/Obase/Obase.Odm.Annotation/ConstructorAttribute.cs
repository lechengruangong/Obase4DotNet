/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：构造函数标记.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:19:32
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     构造函数标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ConstructorAttribute : Attribute
    {
        /// <summary>
        ///     初始化构造函数标记
        /// </summary>
        /// <param name="propNames">构造函数参数映射属性</param>
        public ConstructorAttribute(params string[] propNames)
        {
            PropNames = propNames;
        }

        /// <summary>
        ///     构造函数参数映射属性
        /// </summary>
        public string[] PropNames { get; }
    }
}