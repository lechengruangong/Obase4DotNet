/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户代理类型生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:01:38
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection;
using System.Reflection.Emit;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户代理类型生成器
    /// </summary>
    public class ProxyTypeGenerator : IProxyTypeGenerator
    {
        /// <summary>
        ///     构造代理类型生成器
        /// </summary>
        /// <param name="next">下一节</param>
        public ProxyTypeGenerator(IProxyTypeGenerator next)
        {
            Next = next;
        }

        /// <summary>
        ///     获取代理类型生成管道中的下一个生成器。
        /// </summary>
        public IProxyTypeGenerator Next { get; }

        /// <summary>
        ///     为指定类型的代理类型定义成员。
        /// </summary>
        /// <param name="typeBuilder">一个类型建造器，用于定义代理类型。</param>
        /// <param name="objType">要为其定义代理类的类型，即代理类的基类。</param>
        /// <param name="configurator">上述类型的配置器。</param>
        public void DefineMembers(TypeBuilder typeBuilder, ObjectType objType, IObjectTypeConfigurator configurator)
        {
            if (Should(objType, configurator))
            {
                //如果应当生成代理类型，为该类型定义一个公有字段
                var ext = objType.GetExtension<MultiTenantExtension>();
                if (ext == null)
                {
                    //向上溯源
                    var baseType = objType.DerivingFrom;
                    while (baseType != null)
                    {
                        ext = baseType.GetExtension<MultiTenantExtension>();
                        if (ext != null)
                        {
                            typeBuilder.DefineField("obase_gen_tenantIdMark", ext.TenantIdType, FieldAttributes.Public);
                            break;
                        }

                        baseType = baseType.DerivingFrom;
                    }
                }
                else
                {
                    typeBuilder.DefineField("obase_gen_tenantIdMark", ext.TenantIdType, FieldAttributes.Public);
                }
            }
        }

        /// <summary>
        ///     判定指定的类型是否需要生成代理类型。
        /// </summary>
        /// <param name="objType">要判定的类型。</param>
        /// <param name="configurator">上述类型的配置器。</param>
        public bool Should(ObjectType objType, IObjectTypeConfigurator configurator)
        {
            //如果已启用多租户（GetExtension<MultiTenantExtension>不为null），且TenantIdMark属性未设值，则应当生成代理类型，否则不生成
            var ext = objType.GetExtension<MultiTenantExtension>();
            //判定当前的结果
            var result = ext != null && string.IsNullOrEmpty(ext.TenantIdMark);
            //向上溯源
            var baseType = objType.DerivingFrom;
            while (baseType != null)
            {
                ext = baseType.GetExtension<MultiTenantExtension>();
                result |= ext != null && string.IsNullOrEmpty(ext.TenantIdMark);
                baseType = baseType.DerivingFrom;
            }

            return result;
        }
    }
}