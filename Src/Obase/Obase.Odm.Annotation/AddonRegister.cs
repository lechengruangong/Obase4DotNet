/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标注建模的注册器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:05:16
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     标注建模的注册器
    /// </summary>
    public class AddonRegister : IAddonRegister
    {
        /// <summary>
        ///     为某个插件注册
        /// </summary>
        /// <param name="modelBuilder">建模器</param>
        public void Regist(ModelBuilder modelBuilder)
        {
            modelBuilder.Use(p => new AnnotatedTypeAnalyzer(p));
            modelBuilder.Use(next => new AnnotatedMemberAnalyzer(next));
        }
    }
}