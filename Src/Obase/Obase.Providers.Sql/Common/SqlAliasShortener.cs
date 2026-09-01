/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Sql别名缩短器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-9-1
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Obase.Providers.Sql.Common
{
    /// <summary>
    ///     Sql别名缩短器。
    ///     部分数据库（如PostgreSql、Oracle、MySql等）对标识符（表别名、列别名）的长度有限制，
    ///     别名过长时会被数据库截断，导致SQL语句异常。
    ///     本类维护"原始别名↔短别名"的映射字典（_obase_gen_alias + 唯一哈希，总长度40以下），
    ///     Sql生成侧与结果读取侧（DataRow）共用该字典进行转换，保证对应关系不被破坏。
    ///     实施要点：
    ///     （1）仅缩短规则生成的别名（以'_'开头）：非下划线前缀的名称（如字段名、派生表名、视图目标字段等）
    ///     会被Sql的其它部分按原名引用（如投影后继续筛选时引用投影列名），缩短会破坏这些引用，故保持原样；
    ///     （2）使用SHA-256取前16位十六进制作为哈希，确定性且跨进程稳定（不可使用string.GetHashCode，其每次进程随机化）；
    ///     （3）生成的短别名全部小写、纯ASCII、以'_'开头，是各数据库的合法标识符；
    ///     （4）幂等：已以"_obase_gen_alias"开头的别名直接返回，避免重复缩短。
    /// </summary>
    public static class SqlAliasShortener
    {
        /// <summary>
        ///     短别名前缀。
        /// </summary>
        public const string Prefix = "_obase_gen_alias";

        /// <summary>
        ///     哈希长度（十六进制字符数）。
        /// </summary>
        public const int HashLength = 16;

        /// <summary>
        ///     生成短别名的最大总长度，即"前缀+哈希"的长度，恒小于40。
        /// </summary>
        public const int MaxGeneratedLength = 32;

        /// <summary>
        ///     原始别名到短别名的映射字典，Sql生成侧与DataRow读取侧共用。
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> MappingCache =
            new ConcurrentDictionary<string, string>();

        /// <summary>
        ///     获取指定原始别名对应的短别名（未超过映射规则时返回原值），并缓存于映射字典。
        /// </summary>
        /// <param name="alias">原始别名。</param>
        public static string GetShort(string alias)
        {
            if (string.IsNullOrEmpty(alias)) return alias;
            return MappingCache.GetOrAdd(alias, ComputeShort);
        }

        /// <summary>
        ///     缩短指定的别名：规则生成的别名（以'_'开头）统一替换为"_obase_gen_alias+哈希"，其余名称保持原样。
        /// </summary>
        /// <param name="alias">原始别名。</param>
        public static string Shorten(string alias)
        {
            return GetShort(alias);
        }

        /// <summary>
        ///     计算别名的唯一哈希（SHA-256前16位十六进制，共64位，碰撞概率可忽略）。
        /// </summary>
        /// <param name="value">原始别名。</param>
        private static string ComputeHash(string value)
        {
            var sb = new StringBuilder(HashLength);
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                for (var i = 0; i < HashLength / 2; i++)
                    sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        ///     计算原始别名到短别名的映射。
        /// </summary>
        /// <param name="alias">原始别名。</param>
        private static string ComputeShort(string alias)
        {
            //已生成过短别名 幂等返回
            if (alias.StartsWith(Prefix, StringComparison.Ordinal)) return alias;
            //仅缩短规则生成的别名（下划线前缀） 其余名称（字段名/派生表名等）保持原样
            if (!alias.StartsWith("_", StringComparison.Ordinal)) return alias;
            //生成"前缀+哈希"
            return Prefix + ComputeHash(alias);
        }
    }
}
