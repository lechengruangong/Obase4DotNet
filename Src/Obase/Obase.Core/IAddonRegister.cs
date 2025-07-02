/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：扩展构件注册器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:56:08
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.Builder;

namespace Obase.Core
{
    /// <summary>
    ///     扩展构件注册器
    /// </summary>
    public interface IAddonRegister
    {
        /// <summary>
        ///     为某个插件注册
        /// </summary>
        /// <param name="modelBuilder">模型建造器</param>
        void Regist(ModelBuilder modelBuilder);
    }
}