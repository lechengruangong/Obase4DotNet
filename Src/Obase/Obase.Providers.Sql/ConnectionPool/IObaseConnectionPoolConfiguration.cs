/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Obase连接池的配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:27:41
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.ConnectionPool
{
    /// <summary>
    ///     Obase连接池的配置
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
    }
}