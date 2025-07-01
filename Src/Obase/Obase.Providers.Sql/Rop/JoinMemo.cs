/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：源联接备忘录.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:45:30
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     源联接备忘录。
    /// </summary>
    public class JoinMemo
    {
        /// <summary>
        ///     在备忘录中添加一个节点别名及该节点的映射源。
        /// </summary>
        private readonly Dictionary<string, MonomerSource> _aliasSource = new Dictionary<string, MonomerSource>();

        /// <summary>
        ///     获取备忘录条数。
        /// </summary>
        public int Count => _aliasSource.Count;

        /// <summary>
        ///     在备忘录中添加一个节点别名及该节点的映射源。
        /// </summary>
        /// <param name="nodeAlias">要添加的节点别名。当节点别名为null时，用空字符串作键。</param>
        /// <param name="source">节点代表类型的映射源。</param>
        public bool Append(string nodeAlias, MonomerSource source)
        {
            nodeAlias = nodeAlias ?? "";
            if (_aliasSource.ContainsKey(nodeAlias)) return false;
            _aliasSource[nodeAlias] = source;
            return true;
        }

        /// <summary>
        ///     检查备忘录中是否存在指定的节点别名。
        /// </summary>
        /// <param name="nodeAlias">要检查的节点别名。当节点别名为null时，用空字符串作键。</param>
        public bool Exists(string nodeAlias)
        {
            nodeAlias = nodeAlias ?? "";
            return _aliasSource.ContainsKey(nodeAlias);
        }

        /// <summary>
        ///     查询指定节点的代表类型的映射源。
        /// </summary>
        /// <param name="nodeAlias">节点别名。当节点别名为null时，用空字符串作键。</param>
        public MonomerSource GetSource(string nodeAlias)
        {
            nodeAlias = nodeAlias ?? "";
            if (_aliasSource.TryGetValue(nodeAlias, out var source)) return source;
            return null;
        }

        /// <summary>
        ///     重置备忘录。
        /// </summary>
        public void Reset()
        {
            _aliasSource.Clear();
        }
    }
}