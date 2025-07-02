/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询源,如表.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:14:39
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     查询源，如表、视图等
    /// </summary>
    public interface ISource
    {
        /// <summary>
        ///     获取一个值，该值指示源是否支持排序冒泡。
        /// </summary>
        bool CanBubbleOrder { get; }

        /// <summary>
        ///     将当前源与另一源执行左连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        ISource LeftJoin(ISource other, ICriteria criteria);

        /// <summary>
        ///     将当前源与另一源执行内连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        ISource RightJoin(ISource other, ICriteria criteria);

        /// <summary>
        ///     将当前源与另一源执行右连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        ISource InnerJoin(ISource other, ICriteria criteria);

        /// <summary>
        ///     将当前查询源的排序规则提升为指定查询的排序规则。
        ///     注：如果指定查询已设置排序规则，引发异常：“查询Sql语句已设置排序规则。“
        /// </summary>
        /// <param name="query">指定的查询。</param>
        void BubbleOrder(QuerySql query);

        /// <summary>
        ///     针对指定的数据源类型，生成数据源实例的字符串表示形式，该字符串可用于From子句、Update子句和Insert Into子句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        string ToString(EDataSource sourceType);

        /// <summary>
        ///     使用参数化的方式 默认的用途 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters, IParameterCreator creator);
    }
}