/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除代理类型生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:32:11
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection;
using System.Reflection.Emit;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除代理类型生成器
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
                var ext = objType.GetExtension<LogicDeletionExtension>();
                if (ext == null)
                {
                    //向上溯源
                    var baseType = objType.DerivingFrom;
                    while (baseType != null)
                    {
                        ext = baseType.GetExtension<LogicDeletionExtension>();
                        if (ext != null)
                        {
                            //如果应当生成代理类型，为该类型定义一个公有字段
                            typeBuilder.DefineField("obase_gen_deletionMark", typeof(bool), FieldAttributes.Public);
                            break;
                        }

                        baseType = baseType.DerivingFrom;
                    }
                }
                else
                {
                    //如果应当生成代理类型，为该类型定义一个公有字段
                    typeBuilder.DefineField("obase_gen_deletionMark", typeof(bool), FieldAttributes.Public);
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
            //如果已启用逻辑删除（GetExtension<LogicDeletionExtension>不为null），且_deletionMark属性未设值，则应当生成代理类型，否则不生成
            var ext = objType.GetExtension<LogicDeletionExtension>();
            //判定当前的结果
            var result = ext != null && string.IsNullOrEmpty(ext.DeletionMark);
            //向上溯源
            var baseType = objType.DerivingFrom;
            while (baseType != null)
            {
                ext = baseType.GetExtension<LogicDeletionExtension>();
                result |= ext != null && string.IsNullOrEmpty(ext.DeletionMark);
                baseType = baseType.DerivingFrom;
            }

            return result;
        }
    }
}