/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置实体型的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:13:27
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置实体型的规范。
    /// </summary>
    public interface IEntityTypeConfigurator : IObjectTypeConfigurator
    {
        /// <summary>
        ///     设置标识属性。
        ///     注：每调用一次本方法，追加一个标识属性。
        /// </summary>
        /// <param name="attrName">属性名称</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasKeyAttribute(string attrName, bool overrided = true);

        /// <summary>
        ///     设置一个值，该值指示标识属性是否为自增。
        /// </summary>
        /// <param name="keyIsSelfIncreased">是否自增</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasKeyIsSelfIncreased(bool keyIsSelfIncreased, bool overrided = true);

        /// <summary>
        ///     获取标识属性集合
        /// </summary>
        /// <returns></returns>
        string[] GetKeyAttributesFiled();
    }
}