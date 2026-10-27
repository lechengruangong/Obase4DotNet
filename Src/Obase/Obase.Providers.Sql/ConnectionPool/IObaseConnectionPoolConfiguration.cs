/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Obase连接池的配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:27:41
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Providers.Sql.ConnectionPool
{
    /// <summary>
    ///     Obase连接池的配置
    ///     取值约定(所有配置项一致):字符串为null或空字符串、数值小于等于<see cref="TimeSpan.Zero" />、
    ///     可空类型为null时,均表示使用默认值,即不做调整.
    ///     各项配置在.NET版自有连接池中的生效情况见各属性的说明.
    /// </summary>
    public interface IObaseConnectionPoolConfiguration
    {
        /// <summary>
        ///     连接池的名称 如果为空或空字符串 则使用默认值Obase ConnectionPool
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     连接池的最大大小 如果小于等于0 则使用默认值100
        /// </summary>
        int MaximumPoolSize { get; }

        /// <summary>
        ///     连接池的最小空闲连接数 如果小于等于0 则使用默认值5
        ///     在.NET版中体现为连接池预热时建立的连接数
        /// </summary>
        int MinimumIdle { get; }

        /// <summary>
        ///     获取连接的超时时间 如果小于等于<see cref="TimeSpan.Zero" /> 则使用默认值10秒
        /// </summary>
        TimeSpan ConnectionTimeout { get; }

        /// <summary>
        ///     空闲连接的超时时间 如果小于等于<see cref="TimeSpan.Zero" /> 则使用默认值20秒
        /// </summary>
        TimeSpan IdleTimeout { get; }

        /// <summary>
        ///     连接有效性校验的超时时间 如果小于等于<see cref="TimeSpan.Zero" /> 则使用默认值5秒
        ///     在.NET版中体现为校验连接可用性时执行校验语句的命令超时时间
        /// </summary>
        TimeSpan ValidationTimeout { get; }

        /// <summary>
        ///     连接保活的时间间隔 如果小于等于<see cref="TimeSpan.Zero" /> 则使用默认值5秒
        ///     在.NET版中体现为后台定时检查连接可用性的间隔
        /// </summary>
        TimeSpan KeepaliveTime { get; }

        /// <summary>
        ///     连接的最大存活时间 如果小于等于<see cref="TimeSpan.Zero" /> 则表示不限制
        ///     .NET版的自有连接池没有该能力,配置后不生效
        /// </summary>
        TimeSpan MaxLifetime { get; }

        /// <summary>
        ///     连接泄漏检测的阈值 如果小于等于<see cref="TimeSpan.Zero" /> 则表示不启用
        ///     .NET版的自有连接池没有该能力,配置后不生效
        /// </summary>
        TimeSpan LeakDetectionThreshold { get; }

        /// <summary>
        ///     连接是否自动提交事务 为null则不设置
        ///     .NET版的DbConnection没有该能力,配置后不生效
        /// </summary>
        bool? AutoCommit { get; }

        /// <summary>
        ///     连接是否只读 为null则不设置
        ///     .NET版的DbConnection没有该能力,配置后不生效
        /// </summary>
        bool? ReadOnly { get; }

        /// <summary>
        ///     连接有效性校验使用的语句 如果为空或空字符串 则使用默认值SELECT 1
        /// </summary>
        string ConnectionTestQuery { get; }
    }
}
