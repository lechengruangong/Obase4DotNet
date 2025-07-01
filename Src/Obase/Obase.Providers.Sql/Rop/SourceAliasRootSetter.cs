/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：别名根设置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:11:57
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     别名根设置器，用于为SQL表达式中的源设置别名根，即在现有别名前加上前缀。
    /// </summary>
    public class SourceAliasRootSetter : ExpressionVisitor
    {
        /// <summary>
        ///     别名根，即要作为现有别名前缀的字符串。
        /// </summary>
        private readonly string _aliasRoot;

        /// <summary>
        ///     构造SourceAliasRootSetter的新实例。
        /// </summary>
        /// <param name="aliasRoot">别名根，即要作为现有别名的前缀的字符串。</param>
        public SourceAliasRootSetter(string aliasRoot)
        {
            _aliasRoot = string.IsNullOrEmpty(aliasRoot) ? null : aliasRoot;
        }

        /// <summary>
        ///     访问字段表达式。重写ExpressionVisitor.VisitField。
        /// </summary>
        /// <param name="field">要访问的字段表达式。</param>
        protected override Expression VisitField(FieldExpression field)
        {
            if (!string.IsNullOrEmpty(_aliasRoot) && field.Field.Source is SimpleSource source)
                source.SetAliasRoot(_aliasRoot);
            return field;
        }
    }
}