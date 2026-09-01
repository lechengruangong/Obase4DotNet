/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Sql别名替换器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-9-1
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Text;

namespace Obase.Providers.Sql.Common
{
    /// <summary>
    ///     Sql别名替换器。
    ///     在Sql字符串生成完成后，按"原始别名→短别名"映射字典对Sql字符串做标识符级替换：
    ///     （1）仅替换对象模型确认的别名（SqlAliasCollector收集的白名单），避免误伤真实表名/列名；
    ///     （2）识别各数据库的标识符定界形式：双引号"..."、反引号`...`、方括号[...]、裸标识符；
    ///     （3）跳过单引号字符串字面量，避免误伤字面量文本；
    ///     （4）对完整标识符做精确匹配，避免子串误伤（如"_Order_Items"不会匹配"_Order_Items_Price"）。
    /// </summary>
    public static class SqlAliasReplacer
    {
        /// <summary>
        ///     按别名映射字典替换Sql字符串中的别名。
        /// </summary>
        /// <param name="sql">Sql字符串。</param>
        /// <param name="aliases">对象模型确认的别名白名单。</param>
        public static string Replace(string sql, ISet<string> aliases)
        {
            if (string.IsNullOrEmpty(sql) || aliases == null || aliases.Count == 0) return sql;

            var sb = new StringBuilder(sql.Length);
            var i = 0;
            while (i < sql.Length)
            {
                var c = sql[i];
                if (c == '\'')
                {
                    //单引号字符串字面量 原样复制
                    CopyStringLiteral(sql, ref i, sb);
                }
                else if (c == '"' || c == '`')
                {
                    //双引号/反引号定界标识符
                    CopyDelimitedIdentifier(sql, ref i, sb, c, aliases);
                }
                else if (c == '[')
                {
                    //方括号定界标识符(SqlServer)
                    CopyBracketIdentifier(sql, ref i, sb, aliases);
                }
                else if (IsIdentifierChar(c))
                {
                    //裸标识符
                    CopyBareIdentifier(sql, ref i, sb, aliases);
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///     原样复制单引号字符串字面量（含''转义）。
        /// </summary>
        /// <param name="sql">Sql字符串。</param>
        /// <param name="i">当前位置。</param>
        /// <param name="sb">输出。</param>
        private static void CopyStringLiteral(string sql, ref int i, StringBuilder sb)
        {
            var start = i;
            i++; //跳过开引号
            while (i < sql.Length)
            {
                if (sql[i] == '\'')
                {
                    if (i + 1 < sql.Length && sql[i + 1] == '\'')
                    {
                        i += 2; //''转义
                        continue;
                    }

                    i++; //闭合引号
                    break;
                }

                i++;
            }

            sb.Append(sql, start, i - start);
        }

        /// <summary>
        ///     复制定界标识符，若内部文本为别名则替换为短别名。
        /// </summary>
        /// <param name="sql">Sql字符串。</param>
        /// <param name="i">当前位置。</param>
        /// <param name="sb">输出。</param>
        /// <param name="delimiter">定界符（双引号或反引号）。</param>
        /// <param name="aliases">别名白名单。</param>
        private static void CopyDelimitedIdentifier(string sql, ref int i, StringBuilder sb, char delimiter,
            ISet<string> aliases)
        {
            var start = i;
            i++; //跳过开定界符
            var contentStart = i;
            while (i < sql.Length && sql[i] != delimiter) i++;
            var content = sql.Substring(contentStart, i - contentStart);
            if (i < sql.Length) i++; //闭合定界符

            if (aliases.Contains(content))
            {
                var shortName = SqlAliasShortener.GetShort(content);
                if (shortName != content)
                {
                    sb.Append(delimiter).Append(shortName).Append(delimiter);
                    return;
                }
            }

            sb.Append(sql, start, i - start);
        }

        /// <summary>
        ///     复制方括号定界标识符，若内部文本为别名则替换为短别名。
        /// </summary>
        /// <param name="sql">Sql字符串。</param>
        /// <param name="i">当前位置。</param>
        /// <param name="sb">输出。</param>
        /// <param name="aliases">别名白名单。</param>
        private static void CopyBracketIdentifier(string sql, ref int i, StringBuilder sb, ISet<string> aliases)
        {
            var start = i;
            i++; //跳过'['
            var contentStart = i;
            while (i < sql.Length && sql[i] != ']') i++;
            var content = sql.Substring(contentStart, i - contentStart);
            if (i < sql.Length) i++; //跳过']'

            if (aliases.Contains(content))
            {
                var shortName = SqlAliasShortener.GetShort(content);
                if (shortName != content)
                {
                    sb.Append('[').Append(shortName).Append(']');
                    return;
                }
            }

            sb.Append(sql, start, i - start);
        }

        /// <summary>
        ///     复制裸标识符，若完整标识符为别名则替换为短别名。
        /// </summary>
        /// <param name="sql">Sql字符串。</param>
        /// <param name="i">当前位置。</param>
        /// <param name="sb">输出。</param>
        /// <param name="aliases">别名白名单。</param>
        private static void CopyBareIdentifier(string sql, ref int i, StringBuilder sb, ISet<string> aliases)
        {
            var start = i;
            while (i < sql.Length && IsIdentifierChar(sql[i])) i++;
            var token = sql.Substring(start, i - start);

            if (aliases.Contains(token))
            {
                var shortName = SqlAliasShortener.GetShort(token);
                if (shortName != token)
                {
                    sb.Append(shortName);
                    return;
                }
            }

            sb.Append(token);
        }

        /// <summary>
        ///     判定字符是否为标识符字符。
        /// </summary>
        /// <param name="c">字符。</param>
        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '$' || c == '#';
        }
    }
}
