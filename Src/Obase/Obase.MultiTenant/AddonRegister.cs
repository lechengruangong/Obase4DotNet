/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户的注册器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:42:28
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core;
using Obase.Core.Odm.Builder;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户的注册器
    /// </summary>
    public class AddonRegister : IAddonRegister
    {
        /// <summary>
        ///     为某个插件注册
        /// </summary>
        /// <param name="modelBuilder">建模器</param>
        public void Regist(ModelBuilder modelBuilder)
        {
            modelBuilder.Use(next => new TypeAnalyzer(next));
            modelBuilder.Use(next => new ProxyTypeGenerator(next));
            modelBuilder.Use(next => new ComplementConfigurator(next));
        }
    }
}