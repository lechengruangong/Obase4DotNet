/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义查询投影集规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:22:43
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     定义查询投影集规范。
    /// </summary>
    public interface ISelectionSet
    {
        /// <summary>
        ///     获取投影集中的投影列集合。
        /// </summary>
        List<SelectionColumn> Columns { get; }

        /// <summary>
        ///     向投影集中添加一列。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="column">生成列的表达式。</param>
        void Add(SelectionColumn column);

        /// <summary>
        ///     向投影集中添加一个不界定通配范围的通配列。注：如果列已存在则不执行任何操作。
        /// </summary>
        void Add();

        /// <summary>
        ///     向投影集中添加一个通配列，并界定其通配范围。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="source">界定通配范围的源。</param>
        void Add(ISource source);

        /// <summary>
        ///     向投影集添加投影列，该列以指定的表达式作为投影表达式。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="expression">投影表达式。</param>
        void Add(Expression expression);

        /// <summary>
        ///     向投影集添加投影列，该列以指定的表达式作为投影表达式。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="expression">投影表达式。</param>
        /// <param name="alias">投影列的别名。</param>
        void Add(Expression expression, string alias);

        /// <summary>
        ///     向投影集添加投影列，该列为指定的字段。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="field">作为投影列的字段。</param>
        void Add(Field field);

        /// <summary>
        ///     向投影集添加投影列，该列为指定的字段。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="field">作为投影列的字段。</param>
        /// <param name="alias">投影列的别名。</param>
        void Add(Field field, string alias);

        /// <summary>
        ///     向投影集中添加一组列。注：如果某一列已存在则忽略它。
        /// </summary>
        /// <param name="columns">生成列的表达式构成的集合。</param>
        void AddRange(SelectionColumn[] columns);

        /// <summary>
        ///     生成投影集的文本表示形式，该文本可直接用于Select子句。
        /// </summary>
        string ToString();

        /// <summary>
        ///     向投影集中添加一组列。注：如果某一列已存在则忽略它。
        /// </summary>
        /// <param name="columns">生成列的表达式构成的集合。</param>
        /// <param name="alias">列的别名构成的集合。</param>
        void AddRange(SelectionColumn[] columns, string[] alias);

        /// <summary>
        ///     确定投影集是否包含与指定的表达式相对应的列，同时返回该列的别名。
        /// </summary>
        /// <param name="expression">指定的表达式。</param>
        /// <param name="alias">返回相应列的别名。</param>
        bool Contains(Expression expression, out string alias);

        /// <summary>
        ///     确定投影集是否包含指定的列。
        /// </summary>
        /// <param name="column">指定的表达式。</param>
        bool Contains(SelectionColumn column);

        /// <summary>
        ///     为各投影列涉及到的源的别名设置前缀。
        ///     注：只有简单源有别名，忽略非简单源。于2017/11/28 增加
        /// </summary>
        /// <param name="prefix">别名前缀。</param>
        void SetSourceAliasPrefix(string prefix);


        /// <summary>
        ///     针对指定的数据源类型，生成投影集的文本表示形式，该文本可直接用于Select子句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        string ToString(EDataSource sourceType);
    }
}