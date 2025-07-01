/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示单体源.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:17:56
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示单体源，即非由联接运算生成的源。
    /// </summary>
    public abstract class MonomerSource : ISource
    {
        /// <summary>
        ///     获取指代符，该指代符用于在Sql语句的其它部分引用源。
        /// </summary>
        public abstract string Symbol { get; }

        /// <summary>
        ///     获取一个值，该值指示源是否支持排序冒泡。
        /// </summary>
        public abstract bool CanBubbleOrder { get; }


        /// <summary>
        ///     将当前源与另一源执行左连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        public ISource LeftJoin(ISource other, ICriteria criteria)
        {
            if (other == null)
                return this;
            return new JoinedSource(this, other, criteria, ESourceJoinType.Left);
        }

        /// <summary>
        ///     将当前源与另一源执行内连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        public ISource RightJoin(ISource other, ICriteria criteria)
        {
            if (other == null)
                return this;
            return new JoinedSource(this, other, criteria, ESourceJoinType.Right);
        }

        /// <summary>
        ///     将当前源与另一源执行右连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        public ISource InnerJoin(ISource other, ICriteria criteria)
        {
            if (other == null)
                return this;
            return new JoinedSource(this, other, criteria, ESourceJoinType.Inner);
        }

        /// <summary>
        ///     将当前查询源的排序规则提升为指定查询的排序规则。
        ///     注：如果指定查询已设置排序规则，引发异常：“查询Sql语句已设置排序规则。“
        /// </summary>
        /// <param name="query">指定的查询。</param>
        public abstract void BubbleOrder(QuerySql query);

        /// <summary>
        ///     针对指定的数据源类型，生成数据源实例的字符串表示形式，该字符串可用于From子句、Update子句和Insert Into子句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public abstract string ToString(EDataSource sourceType);

        /// <summary>
        ///     使用参数化的方式 默认的用途 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public abstract string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator);

        /// <summary>
        ///     为源设置别名根。
        /// </summary>
        /// <param name="aliasRoot">要设置的别名根。</param>
        internal abstract void SetAliasRoot(string aliasRoot);

        /// <summary>
        ///     别称设为NULL
        /// </summary>
        public abstract void ResetSymbol();
    }
}