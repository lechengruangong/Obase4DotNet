/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示集运算操作数.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:22:38
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示集运算操作数。
    /// </summary>
    public interface ISetOperand
    {
        /// <summary>
        ///     使用参数化的方式 和 默认的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="parameters">返回字符串中的参数及其值的集合。</param>
        /// <param name="creator">参数构造器</param>
        string ToSql(out List<IDataParameter> parameters, IParameterCreator creator);

        /// <summary>
        ///     针对指定的数据源类型，根据查询Sql语句的对象表示法生成Sql语句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        string ToSql(EDataSource sourceType);

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">指定的数据源</param>
        /// <param name="parameters">参数</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        string ToSql(EDataSource sourceType, out List<IDataParameter> parameters, IParameterCreator creator);
    }
}