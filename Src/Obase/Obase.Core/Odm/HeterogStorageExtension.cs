/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构存储扩展.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:11:39
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     异构存储扩展，用于对模型中对象类型的配置进行扩展。
    /// </summary>
    public class HeterogStorageExtension : TypeExtension
    {
        /// <summary>
        ///     存储标记。
        /// </summary>
        private StorageSymbol _storageSymbol;

        /// <summary>
        ///     获取或设置类型的存储标记。
        ///     说明
        ///     只能设置一次存储标记，对已设置的存储标记进行修改将引发异常。
        /// </summary>
        /// <returns>类型的存储标记。如果被扩展的类型为伴随关联，返回其伴随端实体型的存储标记。</returns>
        /// <exception cref="Exception">存储标记一经设置便不能修改。</exception>
        public StorageSymbol StorageSymbol
        {
            get
            {
                var result = _storageSymbol;
                if (ExtendedType is AssociationType associationType)
                    if (associationType.CompanionEnd != null)
                    {
                        // 如果是伴随关联，获取伴随端实体型的存储标记
                        var extension =
                            associationType.CompanionEnd.EntityType.GetExtension(typeof(HeterogStorageExtension));
                        if (extension is HeterogStorageExtension heterogStorageExtension)
                            result = heterogStorageExtension.StorageSymbol;
                    }

                return result;
            }
            set
            {
                if (_storageSymbol != null) throw new InvalidOperationException("存储标记一经设置便不能修改");
                _storageSymbol = value;
            }
        }
    }
}