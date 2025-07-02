/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义关联型的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:20:10
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义关联型的规范。
    /// </summary>
    public interface IAssociationTypeConfigurator : IObjectTypeConfigurator
    {
        /// <summary>
        ///     关联型
        /// </summary>
        Type AssociationType { get; }

        /// <summary>
        ///     关联端集合
        /// </summary>
        IAssociationEndConfigurator[] AssociationEnds { get; }

        /// <summary>
        ///     设置是否为显式关联型
        /// </summary>
        /// <param name="value">是否为显式关联型</param>
        /// <param name="overrided">是否覆盖</param>
        void IsVisible(bool value, bool overrided = true);

        /// <summary>
        ///     启动一个关联端配置项，如果要启动的配置项未创建则新建一个。
        /// </summary>
        /// <param name="name">关联端的名称。</param>
        /// <param name="entityType">作为关联端的实体类型。</param>
        IAssociationEndConfigurator AssociationEnd(string name, Type entityType);

        /// <summary>
        ///     启动一个关联端配置项，如果要启动的配置项未创建则新建一个。
        ///     类型参数：
        ///     TEnd    作为关联端的实体类型。
        /// </summary>
        /// <param name="name">关联端的名称。</param>
        IAssociationEndConfigurator AssociationEnd<TEnd>(string name) where TEnd : class;
    }
}