/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Sql别名收集器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-9-1 17:13:48
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Common
{
    /// <summary>
    ///     Sql别名收集器。
    ///     遍历Sql对象表示法（查询源树、投影集及嵌套子查询），收集其中出现的所有别名（表别名、列别名、派生表符号），
    ///     供Sql别名替换器在生成Sql字符串后统一替换。收集不改变任何对象的发射逻辑。
    /// </summary>
    public static class SqlAliasCollector
    {
        /// <summary>
        ///     收集指定查询Sql对象表示法中出现的所有别名。
        /// </summary>
        /// <param name="querySql">查询Sql对象。</param>
        public static HashSet<string> Collect(QuerySql querySql)
        {
            var aliases = new HashSet<string>();
            Collect(querySql, aliases);
            return aliases;
        }

        /// <summary>
        ///     收集指定修改Sql对象表示法中出现的所有别名。
        /// </summary>
        /// <param name="changeSql">修改Sql对象。</param>
        public static HashSet<string> Collect(ChangeSql changeSql)
        {
            var aliases = new HashSet<string>();
            if (changeSql == null) return aliases;
            CollectSource(changeSql.Source, aliases);
            CollectSource(changeSql.TargetSource, aliases);
            return aliases;
        }

        /// <summary>
        ///     收集查询Sql对象表示法中的别名。
        /// </summary>
        /// <param name="querySql">查询Sql对象。</param>
        /// <param name="aliases">别名集合。</param>
        private static void Collect(QuerySql querySql, HashSet<string> aliases)
        {
            if (querySql == null) return;
            //查询源（含嵌套子查询与集运算）
            CollectSource(querySql.Source, aliases);
            //投影集
            CollectSelectionSet(querySql.SelectionSet, aliases);
        }

        /// <summary>
        ///     收集查询源及其嵌套子查询中的别名。
        /// </summary>
        /// <param name="source">查询源。</param>
        /// <param name="aliases">别名集合。</param>
        private static void CollectSource(ISource source, HashSet<string> aliases)
        {
            switch (source)
            {
                case null:
                    return;
                case SimpleSource simpleSource:
                    if (!string.IsNullOrEmpty(simpleSource.Alias)) aliases.Add(simpleSource.Alias);
                    return;
                case SelectSource selectSource:
                    if (!string.IsNullOrEmpty(selectSource.Symbol)) aliases.Add(selectSource.Symbol);
                    Collect(selectSource.QuerySql, aliases);
                    return;
                case SetSource setSource:
                    if (!string.IsNullOrEmpty(setSource.Symbol)) aliases.Add(setSource.Symbol);
                    Collect(setSource.QuerySet, aliases);
                    return;
                case JoinedSource joinedSource:
                    foreach (var subSource in joinedSource.Sources)
                        CollectSource(subSource, aliases);
                    return;
            }
        }

        /// <summary>
        ///     收集集运算操作数中的别名。
        /// </summary>
        /// <param name="operand">集运算操作数。</param>
        /// <param name="aliases">别名集合。</param>
        private static void Collect(ISetOperand operand, HashSet<string> aliases)
        {
            switch (operand)
            {
                case null:
                    return;
                case QuerySql querySql:
                    Collect(querySql, aliases);
                    return;
                case QuerySet querySet:
                    Collect(querySet, aliases);
                    return;
            }
        }

        /// <summary>
        ///     收集集运算中的别名。
        /// </summary>
        /// <param name="querySet">集运算。</param>
        /// <param name="aliases">别名集合。</param>
        private static void Collect(QuerySet querySet, HashSet<string> aliases)
        {
            if (querySet == null) return;
            Collect(querySet.Left, aliases);
            Collect(querySet.Right, aliases);
        }

        /// <summary>
        ///     收集投影集中的别名。
        /// </summary>
        /// <param name="selectionSet">投影集。</param>
        /// <param name="aliases">别名集合。</param>
        private static void CollectSelectionSet(ISelectionSet selectionSet, HashSet<string> aliases)
        {
            switch (selectionSet)
            {
                case null:
                    return;
                case FieldSet fieldSet:
                    if (fieldSet.Aliases != null)
                        foreach (var alias in fieldSet.Aliases)
                            if (!string.IsNullOrEmpty(alias))
                                aliases.Add(alias);
                    CollectSource(fieldSet.Source, aliases);
                    return;
                default:
                    foreach (var column in selectionSet.Columns)
                        switch (column)
                        {
                            case ExpressionColumn expressionColumn:
                                if (!string.IsNullOrEmpty(expressionColumn.Alias))
                                    aliases.Add(expressionColumn.Alias);
                                break;
                            case WildcardColumn wildcardColumn:
                                CollectSource(wildcardColumn.Source, aliases);
                                break;
                        }

                    return;
            }
        }
    }
}