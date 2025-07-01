/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示分组子句.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:11:18
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示分组子句。
    /// </summary>
    public class GroupBy
    {
        /// <summary>
        ///     作为分组依据的表达式。
        /// </summary>
        private readonly ReadOnlyCollection<Expression> _expressions;

        /// <summary>
        ///     创建表示依据指定的表达式分组的GroupBy实例。
        /// </summary>
        /// <param name="expressions">作为分组依据的表达式。</param>
        public GroupBy(params Expression[] expressions)
        {
            _expressions = new ReadOnlyCollection<Expression>(expressions);
        }

        /// <summary>
        ///     创建表示依据指定的字段分组的GroupBy实例。
        /// </summary>
        /// <param name="fields">作为分组依据的字段。</param>
        public GroupBy(params Field[] fields)
        {
            //转换为FiledExpression 放入List<Expression>
            var expressions = fields.Select(Expression.Fields).Cast<Expression>().ToList();
            _expressions = new ReadOnlyCollection<Expression>(expressions);
        }

        /// <summary>
        ///     获取作为分组依据的表达式。
        /// </summary>
        public Expression[] Expressions => _expressions.ToArray();

        /// <summary>
        ///     GroupBy的字符串表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            var strbuilder = new StringBuilder(" group by ");

            for (var i = 0; i < _expressions.Count; i++)
                strbuilder.Append(i != _expressions.Count - 1
                    ? $"{_expressions[i].ToString(sourceType)},"
                    : $"{_expressions[i].ToString(sourceType)}");

            return strbuilder.ToString();
        }
    }
}