/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示一个字段的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:07:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示字段集。字段集由字段所属的查询源和名称列表组成。同一字段集中的字段必须属于同一个源。
    /// </summary>
    public class FieldSet : ISelectionSet
    {
        /// <summary>
        ///     字段别名列表。别名列表为空表示不设置别名。
        ///     注：别名列表中的元素必须与名称列表一一对应。
        /// </summary>
        private List<string> _aliases;

        /// <summary>
        ///     投影列集合
        /// </summary>
        private List<SelectionColumn> _columns;

        /// <summary>
        ///     字段名称列表。名称列表为空表示该源下的所有字段。
        /// </summary>
        private List<string> _names;

        /// <summary>
        ///     源
        /// </summary>
        private ISource _source;

        /// <summary>
        ///     创建字段集实例。该字段集表示指定源下的所有字段。
        /// </summary>
        /// <param name="source">字段所属的源</param>
        public FieldSet(ISource source)
        {
            var simpleSource = source as SimpleSource;

            if (simpleSource == null) throw new ArgumentException("字段的源只能为SimpleSource");
            _source = source;
        }

        /// <summary>
        ///     创建字段集实例。
        /// </summary>
        /// <param name="source">字段所属源的名称</param>
        /// <param name="names">字段的名称列表</param>
        public FieldSet(string source, List<string> names) : this(new SimpleSource(source), names)
        {
        }

        /// <summary>
        ///     创建字段集实例。该字段集表示指定源下的所有字段。
        /// </summary>
        /// <param name="source">字段所属源的名称</param>
        public FieldSet(string source) : this(new SimpleSource(source))
        {
        }

        /// <summary>
        ///     创建字段集实例。
        /// </summary>
        /// <param name="source">字段所属的源</param>
        /// <param name="names">字段的名称列表</param>
        public FieldSet(ISource source, List<string> names) : this(source)
        {
            _names = names;
        }

        /// <summary>
        ///     获取或设置字段的名称列表。名称列表为空表示该源下的所有字段。
        /// </summary>
        public List<string> Names
        {
            get => _names;
            set => _names = value;
        }

        /// <summary>
        ///     获取或设置字段所属的源。
        /// </summary>
        public ISource Source
        {
            get => _source;
            set
            {
                if (!(value is SimpleSource))
                    throw new ArgumentException("字段的源只能为SimpleSource");
                _source = value;
            }
        }

        /// <summary>
        ///     获取或设置字段别名列表。别名列表为空表示不设置别名。
        ///     注：别名列表中的元素必须与名称列表一一对应。
        /// </summary>
        public List<string> Aliases
        {
            get => _aliases;
            set => _aliases = value;
        }

        /// <summary>
        ///     获取投影集中的投影列集合。
        /// </summary>
        public List<SelectionColumn> Columns
        {
            get
            {
                if (_columns != null) return _columns;
                if (_names != null)
                {
                    var returnColumns = new List<SelectionColumn>();

                    if (_columns != null) returnColumns.AddRange(_columns);

                    for (var i = 0; i < _names.Count; i++)
                    {
                        var strAlias = "";
                        if (_aliases != null && _aliases.Count > 0)
                            if (!string.IsNullOrEmpty(_aliases[i]))
                                strAlias = _aliases[i];
                        returnColumns.Add(new ExpressionColumn
                        {
                            Expression = Expression.Fields(new Field(_names[i])),
                            Alias = strAlias
                        });
                    }

                    return returnColumns;
                }

                return new List<SelectionColumn>
                {
                    new WildcardColumn
                    {
                        Source = (MonomerSource)_source
                    }
                };
            }
        }

        /// <summary>
        ///     将当前字段集实例转换成字符串表示形式，该字符串将用于查询Sql的Select字句。
        /// </summary>
        public override string ToString()
        {
            return ToString(EDataSource.SqlServer);
        }

        /// <summary>
        ///     向投影集中添加一列。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="column">生成列的表达式。</param>
        public void Add(SelectionColumn column)
        {
            if (column is ExpressionColumn expressionColum)
            {
                var filedExp = expressionColum.Expression as FieldExpression;

                if (filedExp == null) throw new ArgumentException("只允许添加字段表达式列.");

                if (_names == null) _names = new List<string>();

                if (_aliases == null) _aliases = new List<string>();

                if (_columns == null)
                    _columns = new List<SelectionColumn>
                    {
                        new WildcardColumn
                        {
                            Source = (MonomerSource)_source
                        }
                    };

                _names.Add(filedExp.Field.Name);
                _columns.Add(expressionColum);
                _aliases.Add(expressionColum.Alias);
            }
            else if (column is WildcardColumn)
            {
                _names = null;
                _aliases = null;
                _columns = null;
            }
            else
            {
                throw new ArgumentException("只允许添加ExpressionColumn和WildcardColumn.");
            }
        }

        /// <summary>
        ///     向投影集中添加一个不界定通配范围的通配列。注：如果列已存在则不执行任何操作。
        /// </summary>
        public void Add()
        {
            _names = null;
            _aliases = null;
            _columns = null;
        }

        /// <summary>
        ///     向投影集中添加一个通配列，并界定其通配范围。注：如果列已存在则不执行任何操作。
        /// </summary>
        /// <param name="source">界定通配范围的源。</param>
        public void Add(ISource source)
        {
            _source = source;
            _names = null;
            _aliases = null;
            _columns = null;
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

            if (_names == null)
            {
                if (_columns == null) _columns = new List<SelectionColumn>();

                _columns.Add(new WildcardColumn
                {
                    Source = (MonomerSource)_source
                });
            }

            if (!Columns.Contains(column))
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
            if (!Columns.Contains(column))
                Add(column);
        }

        /// <summary>
        ///     向投影集中添加一组列。注：如果某一列已存在则忽略它。
        /// </summary>
        /// <param name="columns">生成列的表达式构成的集合。</param>
        public void AddRange(SelectionColumn[] columns)
        {
            foreach (var item in columns)
                if (!Columns.Contains(item))
                    Add(item);
        }

        /// <summary>
        ///     向投影集中添加一组列。注：如果某一列已存在则忽略它。
        /// </summary>
        /// <param name="columns">生成列的表达式构成的集合。</param>
        /// <param name="alias">列的别名构成的集合。</param>
        public void AddRange(SelectionColumn[] columns, string[] alias)
        {
            foreach (var item in columns)
                if (!Columns.Contains(item))
                    Add(item);

            if (_aliases == null || _aliases.Count == 0) return;

            foreach (var item in alias)
                if (!_aliases.Contains(item))
                    _aliases.Add(item);
        }

        /// <summary>
        ///     确定投影集是否包含与指定的表达式相对应的列，同时返回该列的别名。
        /// </summary>
        /// <param name="expression">指定的表达式。</param>
        /// <param name="alias">返回相应列的别名。</param>
        public bool Contains(Expression expression, out string alias)
        {
            alias = null;

            var result = false;

            var filedExp = expression as FieldExpression;

            if (filedExp != null && _names != null && _names.Count > 0)
            {
                for (var i = 0; i < _names.Count; i++)
                    if (_names[i] == filedExp.Field.Name)
                    {
                        alias = _aliases[i];
                        result = true;
                    }

                if (result) return true;
            }

            //增加通配符列的判断
            foreach (var col in Columns)
            {
                var column = col as WildcardColumn;
                if (column == null) continue;
                var wild = column;
                result = wild.Implies(expression);
                if (result) break; //判断任意包含
            }

            return result;
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
            //如果为MonomerSource简单源则设置别名
            if (Source is MonomerSource source)
                source.SetSymbolPrefix(prefix);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            string result;
            var simpleSource = _source as SimpleSource;

            if (simpleSource == null) throw new InvalidOperationException("字段的源只能为SimpleSource");

            if (_columns != null)
            {
                var names = _columns.Select(u => u.ToString(sourceType));
                return string.Join(",", names);
            }

            if (_names != null)
            {
                var names = _names.Select(u =>
                {
                    if (Aliases != null && Aliases.Count == _names.Count)
                    {
                        if (_source != null)
                            return (string.IsNullOrEmpty(simpleSource.Symbol)
                                ? _source.ToString(sourceType)
                                : simpleSource.Symbol) + "." + u + " as " + Aliases[_names.IndexOf(u)];
                        return u + " as " + Aliases[_names.IndexOf(u)];
                    }

                    if (_source != null)
                        return (string.IsNullOrEmpty(simpleSource.Symbol)
                            ? _source.ToString(sourceType)
                            : simpleSource.Symbol) + "." + u;
                    return u;
                });

                result = string.Join(",", names);
            }
            else
            {
                if (_source == null)
                    result = "*";
                else
                    result = (string.IsNullOrEmpty(simpleSource.Symbol)
                        ? _source.ToString(sourceType)
                        : simpleSource.Symbol) + ".*";
            }

            return result;
        }
    }
}