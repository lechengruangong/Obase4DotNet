/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Having子句.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:12:54
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示Having子句。
    /// </summary>
    public class Having
    {
        /// <summary>
        ///     作为过滤依据的表达式。
        /// </summary>
        private readonly Expression _expression;

        /// <summary>
        ///     创建表示依据指定的表达式进行筛选的Having实例。
        /// </summary>
        /// <param name="expression">筛选条件。</param>
        public Having(Expression expression)
        {
            _expression = expression;
        }

        /// <summary>
        ///     获取作为筛选条件的表达式。
        /// </summary>
        public Expression Expression => _expression;

        /// <summary>
        ///     Having的字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            return $" having {_expression.ToString(sourceType)}";
        }
    }
}