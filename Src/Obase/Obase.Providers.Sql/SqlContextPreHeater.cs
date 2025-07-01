/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Obase配置预热器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:53:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Microsoft.Extensions.Logging;
using Obase.Core;
using Obase.Core.Common;
using Obase.Providers.Sql.ConnectionPool;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     Obase配置预热器
    /// </summary>
    public abstract class SqlContextPreHeater
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        protected SqlContextPreHeater()
        {
        }

        /// <summary>
        ///     预热方法
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ArgumentException"></exception>
        public void PreHeat(ObjectContext context)
        {
            var model = context.Model;
            var contextClass = context.GetType();

            if (GlobalModelCache.Current.GetModel(contextClass) != model)
                throw new ArgumentException("预热失败,模型未创建.");

            if (context.ConfigProvider is SqlContextConfigProvider sqlContextConfigProvider)
            {
                var connectionPool = ObaseConnectionPool.Current.GetPool(sqlContextConfigProvider.ConnectionString,
                    sqlContextConfigProvider.DbProviderFactory, contextClass);
                if (connectionPool == null)
                    throw new ArgumentException("预热失败,连接池未创建.");
            }

            //如果注入了日志 则在此处输出日志
            var loggerFactory = Utils.GetDependencyInjectionServiceOrNull<ILoggerFactory>(contextClass);
            loggerFactory?.CreateLogger(GetType()).Log(LogLevel.Information, "Obase Has Initialized");
        }
    }
}