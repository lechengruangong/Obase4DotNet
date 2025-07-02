/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型扩展的配置器,根据配置生成类型扩展.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:33:44
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     类型扩展的配置器
    /// </summary>
    public abstract class TypeExtensionConfiguration
    {
        /// <summary>
        ///     获取类型扩展的类型。
        /// </summary>
        public abstract Type ExtensionType { get; }

        /// <summary>
        ///     根据配置元数据生成类型扩展实例。
        ///     实施说明
        ///     寄存生成结果，避免重复生成。
        /// </summary>
        /// <returns>生成的类型扩展实例。</returns>
        public abstract TypeExtension MakeExtension();
    }
}