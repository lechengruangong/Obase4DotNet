/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置关联引用的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:07:37
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置关联引用的规范。
    /// </summary>
    public interface IAssociationReferenceConfigurator : IReferenceElementConfigurator
    {
        /// <summary>
        ///     设置聚合级别。
        /// </summary>
        /// <param name="level"></param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasAggregationLevel(eAggregationLevel level, bool overrided = true);

        /// <summary>
        ///     设置左端名。
        /// </summary>
        /// <param name="leftEnd"></param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasLeftEnd(string leftEnd, bool overrided = true);

        /// <summary>
        ///     设置右端名。
        /// </summary>
        /// <param name="rightEnd"></param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasRightEnd(string rightEnd, bool overrided = true);
    }
}
