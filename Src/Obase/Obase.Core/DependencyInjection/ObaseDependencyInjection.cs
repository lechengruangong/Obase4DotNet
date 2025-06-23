/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Obase依赖注入器,Obase依赖注入的入口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:58:56
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     Obase依赖注入器
    /// </summary>
    public static class ObaseDependencyInjection
    {
        /// <summary>
        ///     创建Obase依赖注入的建造器
        /// </summary>
        /// <param name="contextType">所属的上下文类型</param>
        /// <returns></returns>
        public static ServiceContainerBuilder CreateBuilder(Type contextType)
        {
            return new ServiceContainerBuilder(contextType);
        }

        /// <summary>
        ///     创建Obase依赖注入的建造器
        /// </summary>
        /// <typeparam name="TContext">所属的上下文类型</typeparam>
        /// <returns></returns>
        public static ServiceContainerBuilder CreateBuilder<TContext>() where TContext : ObjectContext
        {
            return CreateBuilder(typeof(TContext));
        }
    }
}
