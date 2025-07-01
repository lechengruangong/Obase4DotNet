/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举各种类型的数据源.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:52:34
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     枚举各种类型的数据源
    /// </summary>
    public enum EDataSource
    {
        /// <summary>
        ///     SqlServer数据源
        /// </summary>
        SqlServer,

        /// <summary>
        ///     Oracle数据源
        /// </summary>
        Oracle,

        /// <summary>
        ///     OLEDB数据提供程序
        /// </summary>
        Oledb,

        /// <summary>
        ///     MySql数据源
        /// </summary>
        MySql,

        /// <summary>
        ///     Sqlite数据源
        /// </summary>
        Sqlite,

        /// <summary>
        ///     PostgreSql数据源
        /// </summary>
        PostgreSql,

        /// <summary>
        ///     其他数据源
        /// </summary>
        Other
    }
}