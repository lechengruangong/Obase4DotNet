/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：子查询源.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:42:33
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     Select源，即用一条查询Sql语句作为源。
    /// </summary>
    public class SelectSource : MonomerSource
    {
        /// <summary>
        ///     源的名称。
        /// </summary>
        private string _name;

        /// <summary>
        ///     查询Sql语句的对象表示法。
        /// </summary>
        private QuerySql _querySql;

        /// <summary>
        ///     创建Select源的实例，并指定其查询Sql语句。
        /// </summary>
        /// <param name="querySql">查询Sql语句的对象表示法。</param>
        public SelectSource(QuerySql querySql)
        {
            _querySql = querySql;
        }

        /// <summary>
        ///     创建Select源的实例，并指定其查询Sql语句和名称。
        /// </summary>
        /// <param name="querySql">查询Sql语句的对象表示法。</param>
        /// <param name="name">源的名称。</param>
        public SelectSource(QuerySql querySql, string name) : this(querySql)
        {
            _name = name;
        }

        /// <summary>
        ///     获取或设置源的名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     获取或设置查询Sql语句的对象表示法。
        /// </summary>
        public QuerySql QuerySql
        {
            get => _querySql;
            set => _querySql = value;
        }

        /// <summary>
        ///     获取一个值，该值指示源是否支持排序冒泡
        /// </summary>
        public override bool CanBubbleOrder => true;

        /// <summary>
        ///     获取或设置指代符，该指代符用于在Sql语句的其它部分引用源。
        /// </summary>
        public override string Symbol => _name;


        /// <summary>
        ///     排序冒泡
        /// </summary>
        /// <param name="query">要排序的查询</param>
        public override void BubbleOrder(QuerySql query)
        {
            if (QuerySql.Orders.Count == 0 && QuerySql.Source.CanBubbleOrder)
                QuerySql.BubbleOrder();
            var i = 0;
            foreach (var order in QuerySql.Orders)
            {
                var orderExp = order.Expression;
                if (QuerySql.SelectionSet.Contains(orderExp, out var alias) == false || string.IsNullOrEmpty(alias))
                {
                    alias = _name + "_obaseOrderCol" + i;
                    QuerySql.SelectionSet.Add(orderExp, alias);
                    i++;
                }

                var bubbledOrder = new Order(this, alias, order.Direction);
                query.Orders.Add(bubbledOrder);
            }

            if (QuerySql.TakeNumber == 0)
                QuerySql.ClearOrder();
        }


        /// <summary>
        ///     针对指定的数据源类型，生成数据源实例的字符串表示形式，该字符串可用于From子句、Update子句和Insert Into子句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public override string ToString(EDataSource sourceType)
        {
            if (string.IsNullOrEmpty(Symbol)) return $"({QuerySql.ToSql(sourceType)})";

            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    return $"({QuerySql.ToSql(sourceType)}) [{Symbol}]";
                }
                case EDataSource.PostgreSql:
                case EDataSource.Oracle:
                {
                    return $"({QuerySql.ToSql(sourceType)}) {Symbol}";
                }
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                {
                    return $"({QuerySql.ToSql(sourceType)}) `{Symbol}`";
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
            if (string.IsNullOrEmpty(Symbol)) return $"({QuerySql.ToSql(sourceType, out sqlParameters, creator)})";

            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    return $"({QuerySql.ToSql(sourceType, out sqlParameters, creator)}) [{Symbol}]";
                }
                case EDataSource.PostgreSql:
                case EDataSource.Oracle:
                {
                    return $"({QuerySql.ToSql(sourceType, out sqlParameters, creator)}) {Symbol}";
                }
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                {
                    return $"({QuerySql.ToSql(sourceType, out sqlParameters, creator)}) `{Symbol}`";
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }
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