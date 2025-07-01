/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：投影集.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:42:33
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示集，即Select子句所确定的结果列。
    /// </summary>
    public class SelectionSet : ISelectionSet
    {
        /// <summary>
        ///     投影列集合。
        /// </summary>
        private readonly List<SelectionColumn> _columns;


        /// <summary>
        ///     创建SelectionSet的实例。
        /// </summary>
        public SelectionSet()
        {
            _columns = new List<SelectionColumn>();
        }

        /// <summary>
        ///     使用指定的投影列创建SelectionSet的实例。
        /// </summary>
        /// <param name="column">投影集包含的投影列。</param>
        public SelectionSet(SelectionColumn column)
        {
            _columns = new List<SelectionColumn> { column };
        }

        /// <summary>
        ///     使用指定的投影列集合创建SelectionSet的实例。
        /// </summary>
        /// <param name="columns">投影集包含的投影列的集合。</param>
        public SelectionSet(List<SelectionColumn> columns)
        {
            _columns = columns;
        }

        /// <summary>
        ///     向投影集中添加一列。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="column">生成列的表达式。</param>
        public void Add(SelectionColumn column)
        {
            if (!_columns.Contains(column)) _columns.Add(column);
        }

        /// <summary>
        ///     向投影集中添加一个不界定通配范围的通配列。注：如果列已存在则不执行任何操作。
        /// </summary>
        public void Add()
        {
            _columns.Add(new WildcardColumn());
        }

        /// <summary>
        ///     向投影集中添加一个通配列，并界定其通配范围。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="source">界定通配范围的源。</param>
        public void Add(ISource source)
        {
            _columns.Add(new WildcardColumn { Source = (MonomerSource)source });
        }


        /// <summary>
        ///     向投影集添加投影列，该列以指定的表达式作为投影表达式。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="expression">投影表达式。</param>
        public void Add(Expression expression)
        {
            Add(expression, null);
        }

        /// <summary>
        ///     向投影集添加投影列，该列以指定的表达式作为投影表达式。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="expression">投影表达式。</param>
        /// <param name="alias">投影列的别名。</param>
        public void Add(Expression expression, string alias)
        {
            SelectionColumn column = new ExpressionColumn { Expression = expression, Alias = alias };
            Add(column);
        }

        /// <summary>
        ///     向投影集添加投影列，该列为指定的字段。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="field">作为投影列的字段。</param>
        public void Add(Field field)
        {
            Add(field, null);
        }

        /// <summary>
        ///     向投影集添加投影列，该列为指定的字段。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="field">作为投影列的字段。</param>
        /// <param name="alias">投影列的别名。</param>
        public void Add(Field field, string alias)
        {
            SelectionColumn column = new ExpressionColumn { Expression = Expression.Fields(field), Alias = alias };
            Add(column);
        }

        /// <summary>
        ///     生成投影集的文本表示形式，该文本可直接用于Select子句。
        /// </summary>
        public override string ToString()
        {
            return ToString(EDataSource.SqlServer);
        }

        /// <summary>
        ///     向投影集中添加一组列。注：如果某一列已存在则忽略它。
        /// </summary>
        /// <param name="columns">生成列的表达式构成的集合。</param>
        public void AddRange(SelectionColumn[] columns)
        {
            foreach (var item in columns) Add(item);
        }

        /// <summary>
        ///     获取投影集中的投影列集合。
        /// </summary>
        public List<SelectionColumn> Columns => _columns;

        /// <summary>
        ///     向投影集中添加一组列。注：如果某一列已存在则忽略它。
        /// </summary>
        /// <param name="columns">生成列的表达式构成的集合。</param>
        /// <param name="alias">列的别名构成的集合。</param>
        public void AddRange(SelectionColumn[] columns, string[] alias)
        {
            foreach (var column in columns)
                Add(column);
        }

        /// <summary>
        ///     确定投影集是否包含与指定的表达式相对应的列，同时返回该列的别名。
        /// </summary>
        /// <param name="expression">指定的表达式。</param>
        /// <param name="alias">返回相应列的别名。</param>
        public bool Contains(Expression expression, out string alias)
        {
            alias = null;
            foreach (var item in _columns)
            {
                var expressionColumn = item as ExpressionColumn;
                if (expressionColumn != null && expression == expressionColumn.Expression)
                {
                    alias = expressionColumn.Alias;
                    return true;
                }

                var wildCol = item as WildcardColumn;
                if (wildCol != null && wildCol.Implies(expression))
                {
                    //表示已包含在通配符列
                    var fieldExp = expression as FieldExpression;
                    if (fieldExp != null) alias = fieldExp.Field.Name;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     确定投影集是否包含指定的列。
        /// </summary>
        /// <param name="column">指定的表达式。</param>
        public bool Contains(SelectionColumn column)
        {
            var result = false;

            foreach (var col in Columns)
            {
                var expCol = col as ExpressionColumn;
                if (expCol != null)
                {
                    result = expCol.Equals(column);
                    if (result) break; //判断任意包含
                }

                var wildCol = col as WildcardColumn;
                if (wildCol != null)
                {
                    result = wildCol.Implies(column);
                    if (result) break; //判断任意包含
                }
            }

            return result;
        }

        /// <summary>
        ///     为各投影列涉及到的源的别名设置前缀。
        ///     注：只有简单源有别名，忽略非简单源。
        /// </summary>
        /// <param name="prefix">别名前缀。</param>
        public void SetSourceAliasPrefix(string prefix)
        {
            foreach (var item in Columns) item.SetSourceAliasPrefix(prefix);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            return string.Join(",", Columns.Select(s => s.ToString(sourceType)));
        }
    }
}