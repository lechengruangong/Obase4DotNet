/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义在反射建模过程中解析类型的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:03:48
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义在反射建模过程中解析类型的规范。
    /// </summary>
    public interface ITypeAnalyzer
    {
        /// <summary>
        ///     获取类型解析管道中的下一个解析器。
        /// </summary>
        ITypeAnalyzer Next { get; }

        /// <summary>
        ///     配置指定的类型。
        /// </summary>
        /// <param name="type">要配置的类型。</param>
        /// <param name="configurator">该类型的配置器。</param>
        void Configurate(Type type, IStructuralTypeConfigurator configurator);

        /// <summary>
        ///     配置指定的对象类型。
        /// </summary>
        /// <param name="type">要配置的对象类型。</param>
        /// <param name="configurator">该对象类型的配置器。</param>
        void Configurate(Type type, IObjectTypeConfigurator configurator);

        /// <summary>
        ///     配置指定的实体型。
        /// </summary>
        /// <param name="type">要配置的实体类。</param>
        /// <param name="configurator">该实体型的配置器。</param>
        void Configurate(Type type, IEntityTypeConfigurator configurator);

        /// <summary>
        ///     配置指定的关联型。
        /// </summary>
        /// <param name="type">要配置的关联型。</param>
        /// <param name="configurator">该关联型的配置器。</param>
        void Configurate(Type type, IAssociationTypeConfigurator configurator);
    }
}