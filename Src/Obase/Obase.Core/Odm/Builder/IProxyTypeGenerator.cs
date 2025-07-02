/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义为模型中的类型生成代理类的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:02:00
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection.Emit;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义为模型中的类型生成代理类的规范。
    /// </summary>
    public interface IProxyTypeGenerator
    {
        /// <summary>
        ///     获取代理类型生成管道中的下一个生成器。
        /// </summary>
        IProxyTypeGenerator Next { get; }

        /// <summary>
        ///     为指定类型的代理类型定义成员。
        /// </summary>
        /// <param name="typeBuilder">一个类型建造器，用于定义代理类型。</param>
        /// <param name="objType">要为其定义代理类的类型，即代理类的基类。</param>
        /// <param name="configurator">上述类型的配置器。</param>
        void DefineMembers(TypeBuilder typeBuilder, ObjectType objType, IObjectTypeConfigurator configurator);

        /// <summary>
        ///     判定指定的类型是否需要生成代理类型。
        /// </summary>
        /// <param name="objType">要判定的类型。</param>
        /// <param name="configurator">上述类型的配置器。</param>
        bool Should(ObjectType objType, IObjectTypeConfigurator configurator);
    }
}