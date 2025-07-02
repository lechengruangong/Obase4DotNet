/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：复杂属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 09:57:29
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示复杂属性。
    /// </summary>
    public class ComplexAttribute : Attribute
    {
        /// <summary>
        ///     复杂属性的类型。
        /// </summary>
        private readonly ComplexType _complexType;

        /// <summary>
        ///     映射连接符，用于将当前复杂属性的映射目标（TargetField）与其子属性的映射目标（TargetField）串联起来，构成子属性的映射字段。
        ///     术语约定
        ///     当属性为复杂属性或子属性时，TargetField并非完整的字段名，而是字段名的一部分，简称映射目标。沿属性路径，以映射连接符依次将映射目标串联起来即构成完整的
        ///     映射字段。
        /// </summary>
        private char _mappingConnectionChar = char.MinValue;

        /// <summary>
        ///     创建ComplexAttribute类的实例。
        /// </summary>
        /// <param name="type">复杂属性的CLR类型。</param>
        /// <param name="name">属性名称。</param>
        /// <param name="complexType">复杂属性的类型。</param>
        public ComplexAttribute(Type type, string name, ComplexType complexType) : base(type, name)
        {
            _complexType = complexType;
        }

        /// <summary>
        ///     获取复杂属性的类型。
        /// </summary>
        public ComplexType ComplexType => _complexType;


        /// <summary>
        ///     映射连接符，用于将当前复杂属性的映射目标（TargetField）与其子属性的映射目标（TargetField）串联起来，构成子属性的映射字段。
        /// </summary>
        public char MappingConnectionChar
        {
            get => _mappingConnectionChar;
            set => _mappingConnectionChar = value;
        }
    }
}