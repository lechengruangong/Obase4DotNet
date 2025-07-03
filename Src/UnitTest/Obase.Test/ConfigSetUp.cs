using System;
using Microsoft.Extensions.Logging;
using Obase.Core.DependencyInjection;
using Obase.Core.MappingPipeline;
using Obase.MultiTenant;
using Obase.Providers.Sql.ConnectionPool;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional.DependencyInjection;
using Obase.Test.Infrastructure;
using Obase.Test.Infrastructure.Configuration;
using Obase.Test.Service;

namespace Obase.Test;

/// <summary>
///     配置的设置类
/// </summary>
[SetUpFixture]
public class ConfigSetUp
{
    /// <summary>
    ///     此方法仅在所有测试运行前执行一次 在此方法中可以进行一些全局的配置
    ///     首先触发RelationshipDataBaseConfigurationManager的构造函数读取测试配置文件
    ///     之后对Obase进行依赖注入并且调用Obase的预热器
    /// </summary>
    [OneTimeSetUp]
    public void GlobalSetUp()
    {
        //根据测试配置输出当前的数据源
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            //此处为普通的对象上下文
            var context = ContextUtils.CreateContext(dataSource);
            var builder = ObaseDependencyInjection.CreateBuilder(context.GetType());
            //注入单例 用于单例测试
            builder.AddSingleton(typeof(ServiceSa))
                .AddSingleton(typeof(ServiceSb), typeof(ServiceSb))
                .AddSingleton(typeof(IServiceS), typeof(ServiceSb))
                .AddSingleton<ServiceSc>()
                .AddSingleton<IServiceS, ServiceSd>()
                .AddSingleton<ServiceSd, ServiceSd>()
                .AddSingleton<ServiceSe>()
                .AddSingleton<ServiceSf>()
                .AddSingleton<ServiceSg>(_ => new ServiceSg(new DateTime(2000, 1, 1)))
                .AddSingleton<IServiceSo, ServiceSh>(_ => new ServiceSh(new DateTime(1999, 1, 1)));
            //注入多例 用于多例测试
            builder.AddTransient(typeof(ServiceTa))
                .AddTransient(typeof(ServiceTb), typeof(ServiceTb))
                .AddTransient(typeof(IServiceT), typeof(ServiceTb))
                .AddTransient<ServiceTc>()
                .AddTransient<IServiceT, ServiceTd>()
                .AddTransient<ServiceTd, ServiceTd>()
                .AddTransient<ServiceTe>()
                .AddTransient<ServiceTf>()
                .AddTransient<ServiceTg>(_ => new ServiceTg(new DateTime(2000, 1, 1)))
                .AddTransient<IServiceTo, ServiceTh>(_ => new ServiceTh(new DateTime(1999, 1, 1)));
            //注入消息发送器 用于对象变更通知
            builder.AddSingleton<IChangeNoticeSender, MessageSender>();
            //注入日志 用于预热器输出
            builder.AddSingleton<ILoggerFactory, LoggerFactory>(_ => LoggerFactory.Create(p =>
                p.AddFile("logs/{Date}.txt",
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {RequestId,13} [{Level:u3}] {Message} ({EventId:x8}){NewLine}{Exception}")));
            //依赖注入连接池配置 可以在日志中看到相关的更改
            builder.AddSingleton<IObaseConnectionPoolConfiguration, ObaseConnectionPoolConfiguration>(_ =>
                new ObaseConnectionPoolConfiguration($"{dataSource} ConnectionPool"));
            //建造依赖注入容器 结束依赖注入的配置
            builder.Build();

            //预热器
            var preHeater = new ObasePreHeater();
            //预热 会在日志中输出预热的结果
            preHeater.PreHeat(context);

            //此处获取插件测试的上下文
            //此处为首次访问插件上下文的代码 如果想要标注建模可以自动注册 需要在此处之前令有标注的程序集加载
            //最简单的方案是构造一个有标注的程序集的对象 如果有标注的程序没有加载 就必须要在上下文的配置代码里使用RegisterType方法
            //具体参考AddonModelRegister类的注释
            var addonContext = ContextUtils.CreateAddonContext(dataSource);
            //创建插件的依赖注入容器
            var addonBuilder = ObaseDependencyInjection.CreateBuilder(addonContext.GetType());
            //注入插件的服务 多租户ID读取器
            addonBuilder.AddSingleton<ITenantIdReader, TenantIdReader>();
            //建造依赖注入容器 结束依赖注入的配置
            addonBuilder.Build();
        }
    }
}