/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象属性取值数据结构.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:19:28
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     对象属性取值数据结构
    /// </summary>
    [Serializable]
    public struct ObjectAttribute
    {
        /// <summary>
        ///     属性名称
        /// </summary>
        public string Attribute { get; set; }

        /// <summary>
        ///     属性的值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"ObjectAttributeValue:{{Attribute-\"{Attribute}\",Value-\"{Value}\"}}";
        }
    }
}