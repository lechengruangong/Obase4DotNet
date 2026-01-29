/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构存储扩展对应的配置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:55:00
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     HeterogStorageExtension（异构存储扩展）对应的配置器。
    /// </summary>
    public class HeterogStorageExtensionConfiguration : TypeExtensionConfiguration
    {
        /// <summary>
        ///     类型的存储标记。
        /// </summary>
        private StorageSymbol _storageSymbol;

        /// <summary>
        ///     扩展的类型
        /// </summary>
        public override Type ExtensionType => typeof(HeterogStorageExtension);

        /// <summary>
        ///     配置类型的存储标记。
        /// </summary>
        /// <param name="symbol">存储标记。</param>
        public HeterogStorageExtensionConfiguration HasStorageSymbol(StorageSymbol symbol)
        {
            _storageSymbol = symbol;
            return this;
        }

        /// <summary>
        ///     获取类型扩展
        /// </summary>
        /// <returns></returns>
        public override TypeExtension MakeExtension()
        {
            var extension = new HeterogStorageExtension
            {
                StorageSymbol = _storageSymbol
            };

            return extension;
        }
    }
}