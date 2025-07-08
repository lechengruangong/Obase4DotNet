/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：集源.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:46:33
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示将集运算结果作为查询源。
    /// </summary>
    public class SetSource : MonomerSource
    {
        /// <summary>
        ///     作为查询源的集运算。
        /// </summary>
        private readonly QuerySet _querySet;

        /// <summary>
        ///     源的名称。
        /// </summary>
        private string _name;

        /// <summary>
        ///     用指定的集运算创建SetSource实例。
        /// </summary>
        /// <param name="querySet">作为查询源的集运算。</param>
        public SetSource(QuerySet querySet)
        {
            _querySet = querySet;
        }

        /// <summary>
        ///     用指定的集运算创建SetSource实例，同时设置源的名称。
        /// </summary>
        /// <param name="querySet">作为查询源的集运算。</param>
        /// <param name="name">源的名称。</param>
        public SetSource(QuerySet querySet, string name)
        {
            _querySet = querySet;
            _name = name;
        }

        /// <summary>
        ///     获取或设置查询源的名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     获取作为查询源的集运算。
        /// </summary>
        public QuerySet QuerySet => _querySet;

        /// <summary>
        ///     获取一个值，该值指示源是否支持排序冒泡
        /// </summary>
        public override bool CanBubbleOrder => false;

        /// <summary>
        ///     获取或设置指代符，该指代符用于在Sql语句的其它部分引用源。
        /// </summary>
        public override string Symbol => _name;

        /// <summary>
        ///     调用此方法将引发OrderBubblingUnsuportedException
        /// </summary>
        /// <param name="query">指定的查询。</param>
        public override void BubbleOrder(QuerySql query)
        {
            throw new OrderBubblingUnsuportedException(this);
        }

        /// <summary>
        ///     针对指定的数据源类型，生成数据源实例的字符串表示形式，该字符串可用于From子句、Update子句和Insert Into子句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public override string ToString(EDataSource sourceType)
        {
            if (string.IsNullOrEmpty(Symbol)) return $"({QuerySet.ToSql(sourceType)})";
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    return $"({QuerySet.ToSql(sourceType)}) [{Symbol}]";
                }
                case EDataSource.PostgreSql:
                    return $"({QuerySet.ToSql(sourceType)}) {QuerySet}";
                case EDataSource.Oracle:
                {
                    return $"({QuerySet.ToSql(sourceType)}) {Symbol}";
                }
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                {
                    return $"({QuerySet.ToSql(sourceType)}) `{Symbol}`";
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }
        }

        /// <summary>
        ///     使用参数化的方式 默认的用途 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            if (string.IsNullOrEmpty(Symbol)) return $"({QuerySet.ToSql(sourceType, out sqlParameters, creator)})";
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    return $"({QuerySet.ToSql(sourceType, out sqlParameters, creator)}) [{Symbol}]";
                }
                case EDataSource.PostgreSql:
                    return $"({QuerySet.ToSql(sourceType, out sqlParameters, creator)}) {Symbol}";
                case EDataSource.Oracle:
                {
                    return $"({QuerySet.ToSql(sourceType, out sqlParameters, creator)}) {Symbol}";
                }
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                {
                    return $"({QuerySet.ToSql(sourceType, out sqlParameters, creator)}) `{Symbol}`";
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }
        }

        /// <summary>
        ///     为源设置别名根。
        /// </summary>
        /// <param name="aliasRoot">要设置的别名根。</param>
        internal override void SetAliasRoot(string aliasRoot)
        {
            _name = aliasRoot;
        }

        /// <summary>
        ///     为源的指代符设置前缀，设置前缀后源的指代符变更为该前缀串联原指代符。
        /// </summary>
        /// <param name="prefix">前缀</param>
        public override void SetSymbolPrefix(string prefix)
        {
            //设置指代符前缀即在名称前加上前缀。
            _name = prefix + _name;
        }

        /// <summary>
        ///     别称设为NULL
        /// </summary>
        public override void ResetSymbol()
        {
        }
    }
}