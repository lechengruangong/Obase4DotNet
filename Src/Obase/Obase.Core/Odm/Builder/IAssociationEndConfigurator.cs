/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置关联端的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:06:51
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置关联端的规范。
    /// </summary>
    public interface IAssociationEndConfigurator : IReferenceElementConfigurator
    {
        /// <summary>
        ///     端的ClrType
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        ///     获取该关联端上基于当前关联定义的关联引用。
        /// </summary>
        IAssociationReferenceConfigurator ReferenceConfigurator { get; }

        /// <summary>
        ///     设置一个值，该值指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        /// <param name="defaultAsNew">指示是否把关联端对象默认视为新对象。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasDefaultAsNew(bool defaultAsNew, bool overrided = true);

        /// <summary>
        ///     设置一个值，该值指示当前关联端是否为聚合关联端。
        /// </summary>
        /// <param name="isAggregated">指示当前关联端是否为聚合关联端。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IsAggregated(bool isAggregated, bool overrided = true);

        /// <summary>
        ///     配置关联端映射
        /// </summary>
        /// <param name="keyAttribute">关联端标识属性的名称。</param>
        /// <param name="targetField">上述标识属性的映射字段。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasMapping(string keyAttribute, string targetField, bool overrided = true);

        /// <summary>
        ///     指示是否将当前关联端作为伴随端。
        ///     说明
        ///     设置当前端为伴随端会将之前设置的伴随端改设不作为伴随端。
        ///     当override为false时，其它端只要任意一端已设置为伴随端，本方法就不再执行设置操作。
        /// </summary>
        /// <param name="value">指示是否作为伴随端。</param>
        /// <param name="overrided">指示是否覆盖既有设置。</param>
        void AsCompanion(bool value, bool overrided = true);

        /// <summary>
        ///     生成基于当前关联定义的关联引用的配置器，如果配置器已存在返回该配置器。
        /// </summary>
        /// <returns>关联引用配置器；如果当前关联端实体型上未定义相应的关联引用，返回null。</returns>
        /// <param name="propInfo">返回关联引用的访问器，如果关联引用没有访问器返回null。</param>
        IAssociationReferenceConfigurator AssociationReference(out PropertyInfo propInfo);
    }
}