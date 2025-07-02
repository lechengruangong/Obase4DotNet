/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：提供构造Sql语句参数的方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:58:53
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     提供构造Sql语句参数的方法。
    /// </summary>
    public interface IParameterCreator
    {
        /// <summary>
        ///     构造一个Sql语句参数。
        /// </summary>
        IDataParameter Create();
    }
}