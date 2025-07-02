/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户的补充配置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:42:49
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户的补充配置器
    /// </summary>
    public class ComplementConfigurator : IComplementConfigurator
    {
        /// <summary>
        ///     构造补充配置器
        /// </summary>
        /// <param name="next">下一节</param>
        public ComplementConfigurator(IComplementConfigurator next)
        {
            Next = next;
        }

        /// <summary>
        ///     补充配置管道中的下一个配置器。
        /// </summary>
        public IComplementConfigurator Next { get; }

        /// <summary>
        ///     根据类型配置项中的元数据配置指定的类型。
        /// </summary>
        /// <param name="targetType">要配置的类型。</param>
        /// <param name="configuration">包含配置元数据的类型配置项。</param>
        public void Configurate(StructuralType targetType, StructuralTypeConfiguration configuration)
        {
            var ext = targetType.GetExtension<MultiTenantExtension>();
            if (ext != null && string.IsNullOrEmpty(ext.TenantIdMark))
            {
                var attribute = new Attribute(ext.TenantIdType, "obase_gen_tenantIdMark")
                {
                    //目标字段 若果未设置TenantIdField就和TenantIdMark相同
                    TargetField = string.IsNullOrEmpty(ext.TenantIdField) ? ext.TenantIdMark : ext.TenantIdField
                };
                var field = targetType.RebuildingType.GetField($"{attribute.Name}");
                //构造FieldValueGetter
                IValueGetter valueGetter;
                if (ext.TenantIdType == typeof(string) || ext.TenantIdType == typeof(Guid))
                    valueGetter = new MultiTenantStringFieldValueGetter(field, targetType.RebuildingType,
                        configuration.ModelBuilder.ContextType);
                else if (ext.TenantIdType == typeof(int))
                    valueGetter = new MultiTenantIntFieldValueGetter(field, targetType.RebuildingType,
                        configuration.ModelBuilder.ContextType);
                else if (ext.TenantIdType == typeof(long))
                    valueGetter = new MultiTenantLongFieldValueGetter(field, targetType.RebuildingType,
                        configuration.ModelBuilder.ContextType);
                else
                    throw new ArgumentException("多租户主键属性必须为string,int,long,Guid类型中的一种");
                attribute.ValueGetter = valueGetter;
                //构造FieldValueSetter
                var setter = new MultiTenantFieldValueSetter(field, targetType);
                attribute.ValueSetter = setter;

                targetType.AddAttribute(attribute);
            }
        }
    }
}