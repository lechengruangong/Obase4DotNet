/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标准的参数构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 17:01:27
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;
using System.Data.Common;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     标准的参数构造器，该构造器遵循.NET数据提供程序工厂模型构造参数。
    /// </summary>
    public class StandardParameterCreator : IParameterCreator
    {
        /// <summary>
        ///     数据提供程序工厂
        /// </summary>
        private readonly DbProviderFactory _providerFactory;

        /// <summary>
        ///     初始化StandardParameterCreator类的新实例。
        /// </summary>
        /// <param name="providerFactory">数据提供程序工厂。</param>
        public StandardParameterCreator(DbProviderFactory providerFactory)
        {
            _providerFactory = providerFactory;
        }

        /// <summary>
        ///     构造一个Sql语句参数。
        /// </summary>
        public IDataParameter Create()
        {
            return _providerFactory.CreateParameter();
        }
    }
}