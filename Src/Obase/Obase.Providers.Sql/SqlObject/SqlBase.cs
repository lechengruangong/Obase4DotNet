/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为查询Sql语句和修改Sql语句提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:40:18
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     为查询Sql语句和修改Sql语句提供基础实现。
    /// </summary>
    public abstract class SqlBase
    {
        /// <summary>
        ///     表示条件，用于生成Where子句。
        /// </summary>
        private ICriteria _criteria;

        /// <summary>
        ///     表示源，用于生成From子句。
        /// </summary>
        private ISource _source;

        /// <summary>
        ///     Sql语句的类型。
        /// </summary>
        private ESqlType _sqlType;

        /// <summary>
        ///     无参构造SqlBase实例
        /// </summary>
        protected SqlBase()
        {
        }

        /// <summary>
        ///     创建SqlBase实例，并设置其源和类型。
        /// </summary>
        /// <param name="source">源。</param>
        /// <param name="sqlType">Sql类型。</param>
        protected SqlBase(ISource source, ESqlType sqlType)
        {
            _source = source;
            _sqlType = sqlType;
        }

        /// <summary>
        ///     创建SqlBase实例，并设置其源、条件和类型。
        /// </summary>
        /// <param name="source">源。</param>
        /// <param name="criteria">条件。</param>
        /// <param name="sqlType">Sql类型。</param>
        protected SqlBase(ISource source, ICriteria criteria, ESqlType sqlType) : this(source, sqlType)
        {
            _criteria = criteria;
        }


        /// <summary>
        ///     获取或设置源，用于生成From子句。
        /// </summary>
        public ISource Source
        {
            get => _source;
            set => _source = value;
        }

        /// <summary>
        ///     获取或设置条件，用于生成Where子句。
        /// </summary>
        public ICriteria Criteria
        {
            get => _criteria;
            set => _criteria = value;
        }

        /// <summary>
        ///     Sql语句的类型。
        /// </summary>
        public ESqlType SqlType
        {
            get => _sqlType;
            set => _sqlType = value;
        }

        /// <summary>
        ///     针对指定的数据源类型，根据查询Sql语句的对象表示法生成Sql语句。
        /// </summary>
        /// <param name="sourceType">数据源类型.</param>
        public abstract string ToSql(EDataSource sourceType);

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public abstract string ToSql(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator);

        /// <summary>
        ///     使用参数化的方式 和 默认的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public abstract string ToSql(out List<IDataParameter> sqlParameters, IParameterCreator creator);
    }
}