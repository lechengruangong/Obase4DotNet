/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询Sql语句的对象化表示.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:31:00
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Obase.Providers.Sql.Common;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     查询Sql语句的对象化表示。
    /// </summary>
    public class QuerySql : SqlBase, ISetOperand
    {
        /// <summary>
        ///     聚合函数。默认值为None。
        /// </summary>
        private EAggregationFunction _aggregation;

        /// <summary>
        ///     表示是否对结果集去重
        /// </summary>
        private bool _distinct;

        /// <summary>
        ///     分组子句。
        /// </summary>
        private GroupBy _groupBy;

        /// <summary>
        ///     结果集过滤子句。
        /// </summary>
        private Having _having;

        /// <summary>
        ///     排序规则
        /// </summary>
        private List<Order> _orders;

        /// <summary>
        ///     投影集。
        /// </summary>
        private ISelectionSet _selectionSet;

        /// <summary>
        ///     指定跳过多少行
        /// </summary>
        private int _skipNumber;

        /// <summary>
        ///     指定提取多少行。
        ///     注：
        ///     （1）同时设置_takeNumber和_distinct表示先执行去重操作再提取指定行数。
        ///     （2）仅对MySql和Oracle有效，其它数据源将忽略此属性。
        /// </summary>
        private int _takeNumber;

        /// <summary>
        ///     创建查询Sql语句，指定查询源。
        /// </summary>
        /// <param name="source">查询源</param>
        public QuerySql(ISource source) : base(source, ESqlType.Query)
        {
            Source = source;
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源、筛选条件和排序字段。
        /// </summary>
        /// <param name="sourceName">源名</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="orderField">排序字段</param>
        public QuerySql(string sourceName, ICriteria criteria, string orderField)
            : this(sourceName, criteria)
        {
            Orders.Add(new Order(orderField));
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源和筛选条件。
        /// </summary>
        /// <param name="sourceName">源名</param>
        /// <param name="criteria">筛选条件</param>
        public QuerySql(string sourceName, ICriteria criteria)
            : this(sourceName)
        {
            Criteria = criteria;
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源、字段列表、筛选条件、排序字段和排序方向。
        /// </summary>
        /// <param name="sourceName">源名</param>
        /// <param name="fields">字段列表</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderDirection">排序方向</param>
        public QuerySql(string sourceName, string[] fields, ICriteria criteria, string orderField,
            EOrderDirection orderDirection)
            : this(sourceName, fields, criteria)
        {
            Orders.Add(new Order(Source, orderField, orderDirection));
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源、字段列表、筛选条件和排序字段。
        /// </summary>
        /// <param name="sourceName">源名</param>
        /// <param name="fields">字段列表</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="orderField">排序字段</param>
        public QuerySql(string sourceName, string[] fields, ICriteria criteria, string orderField)
            : this(sourceName, fields, criteria)
        {
            Orders.Add(new Order(orderField));
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源、字段列表和筛选条件。
        /// </summary>
        /// <param name="sourceName">源名</param>
        /// <param name="fields">字段列表</param>
        /// <param name="criteria">筛选条件</param>
        public QuerySql(string sourceName, string[] fields, ICriteria criteria)
            : this(sourceName, fields)
        {
            Criteria = criteria;
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源和字段列表。
        /// </summary>
        /// <param name="sourceName">源名</param>
        /// <param name="fields">字段列表</param>
        public QuerySql(string sourceName, string[] fields)
            : this(sourceName)
        {
            //fields?.ToList().ForEach(fidId => SelectionSet.Add(new Field(sourceName,fidId)));
            //FieldSets.Add(new FieldSet(_source, fields.ToList()));
            SelectionSet = new FieldSet(Source, fields.ToList());
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源、筛选条件、排序字段和排序方向。
        /// </summary>
        /// <param name="sourceName">源名</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderDirection">排序方向</param>
        public QuerySql(string sourceName, ICriteria criteria, string orderField, EOrderDirection orderDirection)
            : this(sourceName, criteria)
        {
            Orders.Add(new Order(Source, orderField, orderDirection));
        }

        /// <summary>
        ///     创建查询Sql语句，目标字段属于多个源。
        /// </summary>
        /// <param name="fieldSet">字段集数组</param>
        /// <param name="source">源名</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="orders">排序</param>
        public QuerySql(FieldSet fieldSet, ISource source, ICriteria criteria, params Order[] orders) : base(source,
            criteria, ESqlType.Query)
        {
            //_fieldSets = fieldSet == null ? null : new FieldSet[] { fieldSet }.ToList();
            SelectionSet = fieldSet;
            Source = source;
            Criteria = criteria;
            if (orders != null)
                Orders.AddRange(orders);
        }

        /// <summary>
        ///     创建查询Sql语句，目标字段属于多个源。
        /// </summary>
        /// <param name="fieldSets">字段集数组</param>
        /// <param name="source">源名</param>
        /// <param name="criteria">筛选条件</param>
        /// <param name="orders">排序</param>
        public QuerySql(List<FieldSet> fieldSets, ISource source, ICriteria criteria, params Order[] orders) : base(
            source, criteria, ESqlType.Query)
        {
            var columns = new List<SelectionColumn>();
            foreach (var item in fieldSets ?? new List<FieldSet>())
            {
                if (Equals(item.Names, null) || item.Names.Count <= 0)
                {
                    columns.Add(new WildcardColumn { Source = (MonomerSource)item.Source });
                    continue;
                }

                var names = item.Names.ToArray();
                var alias = item.Aliases.ToArray();
                for (var i = 0; i < names.Length; i++)
                {
                    var col = new ExpressionColumn { Alias = alias[i] };
                    var fieId = new Field(names[i]) { Source = (MonomerSource)item.Source };
                    col.Expression = Expression.Fields(fieId);
                    columns.Add(col);
                }
            }

            SelectionSet = new SelectionSet(columns);
            Source = source;
            Criteria = criteria;
            if (orders != null)
                Orders.AddRange(orders);
        }

        /// <summary>
        ///     创建查询Sql语句，指定查询源。
        /// </summary>
        /// <param name="sourceName">源名</param>
        public QuerySql(string sourceName) : base(new SimpleSource(sourceName), ESqlType.Query)
        {
        }

        /// <summary>
        ///     获取或设置投影集。
        /// </summary>
        public ISelectionSet SelectionSet
        {
            get => _selectionSet ?? (_selectionSet = new SelectionSet());
            set => _selectionSet = value;
        }

        /// <summary>
        ///     表示是否对结果集去重
        ///     同时设置_topNumber和_distinct表示先执行去重操作再提取指定行数。
        /// </summary>
        public bool Distinct
        {
            get => _distinct;
            set => _distinct = value;
        }

        /// <summary>
        ///     获取或设置排序集。
        /// </summary>
        public List<Order> Orders
        {
            get => _orders ?? (_orders = new List<Order>());
            set => _orders = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指定跳过多少行。
        /// </summary>
        public int SkipNumber
        {
            get => _skipNumber;
            set => _skipNumber = value;
        }

        /// <summary>
        ///     获取或设置聚合函数。默认值为None。
        /// </summary>
        public EAggregationFunction Aggregation
        {
            get => _aggregation;
            set => _aggregation = value;
        }

        /// <summary>
        ///     指定提取多少行。
        ///     注：
        ///     （1）同时设置_takeNumber和_distinct表示先执行去重操作再提取指定行数。
        ///     （2）仅对MySql和Oracle有效，其它数据源将忽略此属性。
        /// </summary>
        public int TakeNumber
        {
            get => _takeNumber;
            set => _takeNumber = value;
        }

        /// <summary>
        ///     获取或设置分组子句。
        /// </summary>
        public GroupBy GroupBy
        {
            get => _groupBy;
            set => _groupBy = value;
        }

        /// <summary>
        ///     结果集过滤子句。
        /// </summary>
        public Having Having
        {
            get => _having;
            set => _having = value;
        }

        /// <summary>
        ///     针对指定的数据源类型，根据查询Sql语句的对象表示法生成Sql语句。
        /// </summary>
        /// <param name="sourceType">数据源类型.</param>
        public override string ToSql(EDataSource sourceType)
        {
            //判定是否为集源
            if (Source is SetSource setSource)
                if (TakeNumber == 0 && Distinct == false && Orders.Count == 0 &&
                    Aggregation == EAggregationFunction.None && SelectionSet.Columns.Count == 1 &&
                    SelectionSet.Columns[0] is WildcardColumn)
                    return setSource.QuerySet.ToSql(sourceType);

            StringBuilder sqlStrBuilder;

            string isNullStr;
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    isNullStr = "isnull";
                    break;
                }
                case EDataSource.PostgreSql:
                {
                    isNullStr = "COALESCE";
                    break;
                }
                case EDataSource.Oledb:
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    isNullStr = "ifnull";
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }

            //聚合函数
            switch (Aggregation)
            {
                case EAggregationFunction.None:
                    break;
                case EAggregationFunction.Average:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(Avg(cast({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))} as decimal(10,2))),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");
                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Count:
                    sqlStrBuilder = new StringBuilder($"select count(1) from {Source.ToString(sourceType)} ");
                    if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");
                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Max:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(max({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))}),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");
                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Min:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(min({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))}),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");
                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Sum:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(sum({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))}),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");
                    return sqlStrBuilder.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(Aggregation), $"未知的聚合操作{Aggregation}");
            }


            sqlStrBuilder = new StringBuilder($"select {(Distinct ? "Distinct " : "")}");

            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    var orderStringBuilder = new StringBuilder();
                    //加入Take
                    if (_takeNumber > 0) sqlStrBuilder.Append(" top " + _takeNumber + " ");
                    //Select部分
                    if (SelectionSet != null && SelectionSet.Columns.Count > 0)
                        sqlStrBuilder.Append(SelectionSet.ToString(sourceType));
                    else
                        sqlStrBuilder.Append("*");
                    //From部分
                    sqlStrBuilder.Append($" from {Source.ToString(sourceType)} ");
                    //Where部分
                    if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");
                    //Group部分
                    if (GroupBy != null) sqlStrBuilder.Append($"{GroupBy.ToString(sourceType)} ");
                    //Having部分
                    if (Having != null) sqlStrBuilder.Append($"{Having.ToString(sourceType)} ");
                    //Order部分
                    if (Orders != null && Orders.Count > 0)
                    {
                        orderStringBuilder.Append(" order by ");
                        for (var i = 0; i < Orders.Count; i++)
                        {
                            var order = Orders[i];
                            orderStringBuilder.Append(i != Orders.Count - 1
                                ? $" {order.Expression.ToString(sourceType)} {order.Direction},"
                                : $" {order.Expression.ToString(sourceType)} {order.Direction}");
                        }
                    }

                    //跳过(Skip)部分
                    if (SkipNumber <= 0)
                    {
                        sqlStrBuilder.Append(orderStringBuilder.Append(" "));
                    }
                    else
                    {
                        sqlStrBuilder = new StringBuilder($"select {(Distinct ? "Distinct " : "")}");
                        if (_takeNumber > 0) sqlStrBuilder.Append($" top {_takeNumber} ");
                        sqlStrBuilder.Append(" t.* ");
                        var selectStr = SelectionSet != null && SelectionSet.Columns.Count > 0
                            ? string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))
                            : "*";
                        var orderStr = Orders == null || Orders.Count == 0
                            ? "1"
                            : string.Join(",",
                                Orders.Select(s =>
                                    " " + s.Field?.ToString(sourceType) + " " + s.Direction + " "));
                        sqlStrBuilder.Append(
                            $" from (select {selectStr},ROW_NUMBER() over(order by {orderStr} ) as rownum from {Source.ToString(sourceType)} ");
                        if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");

                        sqlStrBuilder.Append($" ) t where t.rownum > {SkipNumber}");
                        if (_orders != null && _orders.Count > 0) sqlStrBuilder.Append(" order by t.rownum asc");
                    }

                    break;
                }
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    var orderStringBuilder = new StringBuilder();
                    //Select部分
                    if (SelectionSet != null && SelectionSet.Columns.Count > 0)
                        sqlStrBuilder.Append(SelectionSet.ToString(sourceType));
                    else
                        sqlStrBuilder.Append("*");
                    //From部分
                    sqlStrBuilder.Append($" from {Source.ToString(sourceType)} ");
                    //Where部分
                    if (Criteria != null) sqlStrBuilder.Append($" where {Criteria.ToString(sourceType)} ");
                    //Group部分
                    if (GroupBy != null) sqlStrBuilder.Append($"{GroupBy.ToString(sourceType)} ");
                    //Having部分
                    if (Having != null) sqlStrBuilder.Append($"{Having.ToString(sourceType)} ");
                    //Order部分
                    if (Orders != null && Orders.Count > 0)
                    {
                        orderStringBuilder.Append(" order by ");
                        for (var i = 0; i < Orders.Count; i++)
                        {
                            var order = Orders[i];
                            orderStringBuilder.Append(i != Orders.Count - 1
                                ? $" {order.Expression.ToString(sourceType)} {order.Direction},"
                                : $" {order.Expression.ToString(sourceType)} {order.Direction}");
                        }
                    }

                    sqlStrBuilder.Append(orderStringBuilder.Append(" "));
                    //Limit Skip和Take部分
                    if (_takeNumber > 0)
                    {
                        if (_skipNumber >= 0) sqlStrBuilder.Append($" limit {_skipNumber},{_takeNumber}");
                    }
                    else
                    {
                        if (_skipNumber > 0) sqlStrBuilder.Append($" limit {_skipNumber}");
                    }

                    break;
                }
                case EDataSource.PostgreSql:
                {
                    var orderStringBuilder = new StringBuilder();
                    //Select部分
                    if (SelectionSet != null && SelectionSet.Columns.Count > 0)
                        sqlStrBuilder.Append(SelectionSet.ToString(sourceType));
                    else
                        sqlStrBuilder.Append("*");

                    //From部分
                    sqlStrBuilder.Append("from ").Append(Source.ToString(sourceType)).Append(" ");
                    //Where部分
                    if (Criteria != null)
                        sqlStrBuilder.Append("where ").Append(Criteria.ToString(sourceType)).Append(" ");
                    //Group部分
                    if (GroupBy != null) sqlStrBuilder.Append(GroupBy.ToString(sourceType)).Append(" ");
                    //Having部分
                    if (Having != null) sqlStrBuilder.Append(Having.ToString(sourceType)).Append(" ");
                    //Order部分
                    if (Orders != null && Orders.Count > 0)
                    {
                        orderStringBuilder.Append(" order by ");
                        for (var i = 0; i < Orders.Count; i++)
                        {
                            var order = Orders[i];
                            orderStringBuilder.Append(i != Orders.Count - 1
                                ? " " + order.Expression.ToString(sourceType) + " " + order.Direction + ","
                                : " " + order.Expression.ToString(sourceType) + " " + order.Direction);
                        }
                    }

                    sqlStrBuilder.Append(orderStringBuilder.Append(" "));
                    //Limit Skip和Take部分
                    if (_takeNumber > 0)
                    {
                        if (_skipNumber >= 0)
                            sqlStrBuilder.Append(" limit ").Append(_takeNumber).Append(" OFFSET ").Append(_skipNumber);
                    }
                    else
                    {
                        if (_skipNumber > 0) sqlStrBuilder.Append(" OFFSET ").Append(_skipNumber);
                    }

                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }

            return sqlStrBuilder.ToString();
        }


        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public override string ToSql(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //注意out值不要赋空
            //判定是否为集源
            if (Source is SetSource setSource)
                if (TakeNumber == 0 && Distinct == false && Orders.Count == 0 &&
                    Aggregation == EAggregationFunction.None && SelectionSet.Columns.Count == 1 &&
                    SelectionSet.Columns[0] is WildcardColumn)
                    return setSource.QuerySet.ToSql(sourceType, out sqlParameters, creator);
            StringBuilder sqlStrBuilder;

            string isNullStr;
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    isNullStr = "isnull";
                    break;
                }
                case EDataSource.PostgreSql:
                {
                    isNullStr = "COALESCE";
                    break;
                }
                case EDataSource.Oledb:
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    isNullStr = "ifnull";
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }

            sqlParameters = new List<IDataParameter>();

            //聚合函数
            switch (Aggregation)
            {
                case EAggregationFunction.None:
                    break;
                case EAggregationFunction.Average:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(Avg({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))}),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Count:
                    sqlStrBuilder = new StringBuilder($"select count(1) from {Source.ToString(sourceType)} ");
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Max:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(max({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))}),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Min:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(min({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))}),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    return sqlStrBuilder.ToString();
                case EAggregationFunction.Sum:
                    sqlStrBuilder =
                        new StringBuilder(
                            $"select {isNullStr}(sum({string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))}),0) from {Source.ToString(sourceType)} ");
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    return sqlStrBuilder.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(Aggregation), $"未知的聚合操作{Aggregation}");
            }

            sqlStrBuilder = new StringBuilder($"select {(Distinct ? "Distinct " : "")}");

            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    var orderStringBuilder = new StringBuilder();
                    //top take部分
                    if (_takeNumber > 0) sqlStrBuilder.Append(" top " + _takeNumber + " ");
                    //Select部分
                    if (SelectionSet != null && SelectionSet.Columns.Count > 0)
                        sqlStrBuilder.Append(SelectionSet.ToString(sourceType));
                    else
                        sqlStrBuilder.Append("*");
                    //From部分
                    {
                        //sqlStrBuilder.Append($" from {Source.ToString(sourceType)} ");

                        sqlStrBuilder.Append($" from {Source.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }
                    //Where部分
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    //Group部分
                    if (GroupBy != null) sqlStrBuilder.Append($"{GroupBy.ToString(sourceType)} ");
                    //Having部分
                    if (Having != null) sqlStrBuilder.Append($"{Having.ToString(sourceType)} ");
                    //Order部分
                    if (Orders != null && Orders.Count > 0)
                    {
                        orderStringBuilder.Append(" order by ");
                        var orders = SqlUtils.DistinctOrders(Orders);
                        for (var i = 0; i < orders.Count; i++)
                        {
                            var order = orders[i];
                            orderStringBuilder.Append(i != orders.Count - 1
                                ? $" {order.Expression.ToString(sourceType)} {order.Direction},"
                                : $" {order.Expression.ToString(sourceType)} {order.Direction}");
                        }
                    }

                    //Skip部分
                    if (SkipNumber <= 0)
                    {
                        sqlStrBuilder.Append(orderStringBuilder.Append(" "));
                    }
                    else
                    {
                        sqlStrBuilder = new StringBuilder($"select {(Distinct ? "Distinct " : "")}");
                        if (_takeNumber > 0) sqlStrBuilder.Append($" top {_takeNumber} ");
                        sqlStrBuilder.Append(" t.* ");
                        var selectStr = SelectionSet != null && SelectionSet.Columns.Count > 0
                            ? string.Join(",", SelectionSet.Columns.Select(s => s.ToString(sourceType)))
                            : "*";
                        var orderStr = Orders == null || Orders.Count == 0
                            ? "1"
                            : string.Join(",",
                                Orders.Select(s =>
                                    " " + s.Field?.ToString(sourceType) + " " + s.Direction + " "));
                        sqlStrBuilder.Append(
                            $" from (select {selectStr},ROW_NUMBER() over(order by {orderStr} ) as rownum from {Source.ToString(sourceType)} ");
                        if (Criteria != null)
                        {
                            sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                            sqlParameters.AddRange(paras);
                        }

                        sqlStrBuilder.Append($" ) t where t.rownum > {SkipNumber}");
                        if (_orders != null && _orders.Count > 0) sqlStrBuilder.Append(" order by t.rownum asc");
                    }

                    break;
                }
                case EDataSource.Oracle:
                {
                    var orderStringBuilder = new StringBuilder();
                    //Select部分
                    if (SelectionSet != null && SelectionSet.Columns.Count > 0)
                        sqlStrBuilder.Append(SelectionSet.ToString(sourceType));
                    else
                        sqlStrBuilder.Append("*");
                    //From部分
                    {
                        sqlStrBuilder.Append(
                            $",ROWNUM paging_rownumber from {Source.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    //Where部分
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    //Group部分
                    if (GroupBy != null) sqlStrBuilder.Append($"{GroupBy.ToString(sourceType)} ");
                    //Having部分
                    if (Having != null) sqlStrBuilder.Append($"{Having.ToString(sourceType)} ");
                    //Order部分
                    if (Orders != null && Orders.Count > 0)
                    {
                        orderStringBuilder.Append(" order by ");
                        for (var i = 0; i < Orders.Count; i++)
                        {
                            var order = Orders[i];
                            orderStringBuilder.Append(i != Orders.Count - 1
                                ? $" {order.Expression.ToString(sourceType)} {order.Direction},"
                                : $" {order.Expression.ToString(sourceType)} {order.Direction}");
                        }
                    }

                    sqlStrBuilder.Append(orderStringBuilder.Append(" "));
                    //Limit Skip和Take部分
                    if (_takeNumber > 0)
                    {
                        sqlStrBuilder.Append(
                            $"{(Criteria == null ? " WHERE " : " AND ")} ROWNUM <= {_skipNumber + _takeNumber}");
                        sqlStrBuilder =
                            new StringBuilder(
                                $"SELECT * FROM ({sqlStrBuilder}) TP WHERE TP.paging_rownumber >{_skipNumber} ");
                    }
                }
                    break;
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                {
                    var orderStringBuilder = new StringBuilder();
                    //Select部分
                    if (SelectionSet != null && SelectionSet.Columns.Count > 0)
                        sqlStrBuilder.Append(SelectionSet.ToString(sourceType));
                    else
                        sqlStrBuilder.Append("*");
                    //From部分
                    {
                        //sqlStrBuilder.Append($" from {Source.ToString(sourceType)} ");

                        sqlStrBuilder.Append($" from {Source.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    //Where部分
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append($" where {Criteria.ToString(sourceType, out var paras, creator)} ");
                        sqlParameters.AddRange(paras);
                    }

                    //Group部分
                    if (GroupBy != null) sqlStrBuilder.Append($"{GroupBy.ToString(sourceType)} ");
                    //Having部分
                    if (Having != null) sqlStrBuilder.Append($"{Having.ToString(sourceType)} ");
                    //Order部分
                    if (Orders != null && Orders.Count > 0)
                    {
                        orderStringBuilder.Append(" order by ");
                        for (var i = 0; i < Orders.Count; i++)
                        {
                            var order = Orders[i];
                            orderStringBuilder.Append(i != Orders.Count - 1
                                ? $" {order.Expression.ToString(sourceType)} {order.Direction},"
                                : $" {order.Expression.ToString(sourceType)} {order.Direction}");
                        }
                    }

                    sqlStrBuilder.Append(orderStringBuilder.Append(" "));
                    //Limit Skip和Take部分
                    if (_takeNumber > 0)
                    {
                        if (_skipNumber >= 0) sqlStrBuilder.Append($" limit {_skipNumber},{_takeNumber}");
                    }
                    else
                    {
                        if (_skipNumber > 0) sqlStrBuilder.Append($" limit {_skipNumber}");
                    }

                    break;
                }
                case EDataSource.PostgreSql:
                {
                    var orderStringBuilder = new StringBuilder();
                    //Select部分
                    if (SelectionSet != null && SelectionSet.Columns.Count > 0)
                        sqlStrBuilder.Append(SelectionSet.ToString(sourceType));
                    else
                        sqlStrBuilder.Append("*");

                    //From部分
                    sqlStrBuilder.Append(" from ").Append(Source.ToString(sourceType, out var paras, creator))
                        .Append(" ");
                    sqlParameters.AddRange(paras);
                    //Where部分
                    if (Criteria != null)
                    {
                        sqlStrBuilder.Append("where ").Append(Criteria.ToString(sourceType, out var cparas, creator))
                            .Append(" ");
                        sqlParameters.AddRange(cparas);
                    }

                    //Group部分
                    if (GroupBy != null) sqlStrBuilder.Append(GroupBy.ToString(sourceType)).Append(" ");
                    //Having部分
                    if (Having != null) sqlStrBuilder.Append(Having.ToString(sourceType)).Append(" ");
                    //Order部分
                    if (Orders != null && Orders.Count > 0)
                    {
                        orderStringBuilder.Append(" order by ");
                        for (var i = 0; i < Orders.Count; i++)
                        {
                            var order = Orders[i];
                            orderStringBuilder.Append(i != Orders.Count - 1
                                ? " " + order.Expression.ToString(sourceType) + " " + order.Direction + ","
                                : " " + order.Expression.ToString(sourceType) + " " + order.Direction);
                        }
                    }

                    sqlStrBuilder.Append(orderStringBuilder.Append(" "));
                    //Limit Skip和Take部分
                    if (_takeNumber > 0)
                    {
                        if (_skipNumber >= 0)
                            sqlStrBuilder.Append(" limit ").Append(_takeNumber).Append(" OFFSET ").Append(_skipNumber);
                    }
                    else
                    {
                        if (_skipNumber > 0) sqlStrBuilder.Append(" OFFSET ").Append(_skipNumber);
                    }

                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }

            return sqlStrBuilder.ToString();
        }

        /// <summary>
        ///     使用参数化的方式 和 默认的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public override string ToSql(out List<IDataParameter> sqlParameters, IParameterCreator creator)
        {
            return ToSql(EDataSource.SqlServer, out sqlParameters, creator);
        }

        /// <summary>
        ///     清除所有排序规则。
        /// </summary>
        public void ClearOrder()
        {
            Orders.Clear();
        }

        /// <summary>
        ///     排序冒泡。
        ///     排序冒泡是指在不改变结果集顺序的条件下将查询源的排序规则提升为查询的排序规则。
        /// </summary>
        public void BubbleOrder()
        {
            if (GroupBy != null) return;
            if (Orders == null || Orders.Count <= 0)
                Source.BubbleOrder(this);
        }

        /// <summary>
        ///     将查询结果集反序。
        ///     如果当前查询语句未设置排序规则，则先执行排序冒泡，然后再实施反序。
        /// </summary>
        public void Reverse()
        {
            BubbleOrder();
            foreach (var order in Orders)
                switch (order.Direction)
                {
                    case EOrderDirection.Desc:
                        order.Direction = EOrderDirection.Asc;
                        break;
                    case EOrderDirection.Asc:
                        order.Direction = EOrderDirection.Desc;
                        break;
                }
        }
    }
}