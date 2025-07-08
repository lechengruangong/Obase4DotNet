/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：通配列.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:51:35
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     通配列，即以一个通配符指定查询结果列。
    /// </summary>
    public class WildcardColumn : SelectionColumn
    {
        /// <summary>
        ///     指定一个源，该源界定通配范围。
        /// </summary>
        private MonomerSource _source;


        /// <summary>
        ///     获取或设置界定通配范围的源。
        /// </summary>
        public MonomerSource Source
        {
            get => _source;
            set => _source = value;
        }

        /// <summary>
        ///     返回哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _source.GetHashCode();
        }

        /// <summary>
        ///     重写相等方法
        /// </summary>
        /// <param name="other">另一个表达式</param>
        /// <returns></returns>
        public override bool Equals(SelectionColumn other)
        {
            if (ReferenceEquals(this, other))
                return true;
            var newOther = other as WildcardColumn;
            if (Equals(newOther, null))
                return false;
            if (newOther.Source == _source)
                return true;
            return false;
        }

        /// <summary>
        ///     返回字符串表达形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(EDataSource.SqlServer);
        }

        /// <summary>
        ///     返回字符串表达形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            if (Equals(_source, null) || string.IsNullOrWhiteSpace(_source.Symbol))
                return " * ";
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                    return $"[{_source.Symbol}].*";
                case EDataSource.PostgreSql:
                    return $"{_source.Symbol}.*";
                case EDataSource.Oracle:
                    return $"{_source.Symbol}.*";
                case EDataSource.Oledb:
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                case EDataSource.Other:
                    return $"`{_source.Symbol}`.*";
                default:
                    throw new ArgumentException($"通配列不支持改数据源{sourceType}");
            }
        }

        /// <summary>
        ///     为投影列涉及到的源的别名设置前缀。
        ///     注：只有简单源有别名，忽略非简单源。
        /// </summary>
        /// <param name="prefix">别名前缀。</param>
        public override void SetSourceAliasPrefix(string prefix)
        {
            //如果为MonomerSource简单源则设置别名
            if (Source is MonomerSource source)
                source.SetSymbolPrefix(prefix);
        }

        /// <summary>
        ///     确定当前通配列是否逻辑蕴含指定的投影列。
        ///     判定规则：
        ///     （1）如果目标列为表达式列但表达式不为字段表达式，判为不蕴含；
        ///     （2）如果当前列未指定通配范围，则蕴含所有字段表达式列和通配列；
        ///     （3）如果当前列指定了通配范围，其通配范围为S0，目标列为通配列，其通配范围为S1，当S0.ToString(Select-Clause)==S1.
        ///     ToString(Select-Clause)时，判定为蕴含；
        ///     （4）如果当前列指定了通配范围，其通配范围为S0，目标列为字段表达式列，字段所属的源为S1，当S0.ToString(Select-Clause)==S1.
        ///     ToString(Select-Clause)时，判定为蕴含。
        /// </summary>
        /// <returns>如果蕴含返回true，否则返回false。</returns>
        /// <param name="other">目标投影列。</param>
        public bool Implies(SelectionColumn other)
        {
            //如果为表达式列 不为字段表达式
            var otherExpression = other as ExpressionColumn;
            if (otherExpression != null && !(otherExpression.Expression is FieldExpression)) return false;

            var otherWild = other as WildcardColumn;
            //如果当前列未指定通配范围
            if (Equals(_source, null)) return true;

            //目标列为通配列
            if (otherWild != null && _source.ToString(EDataSource.SqlServer) ==
                otherWild.Source.ToString(EDataSource.SqlServer))
                return true;
            //目标为表达式列
            if (otherExpression != null)
            {
                var filedExp = (FieldExpression)otherExpression.Expression;
                if (filedExp != null && _source.ToString(EDataSource.SqlServer) ==
                    filedExp.Field.Source.ToString(EDataSource.SqlServer))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     确定当前通配列是否逻辑蕴含指定表达式构建的投影列。
        ///     判定规则：
        ///     （1）如果目标表达式不为字段表达式，判为不蕴含；
        ///     （2）如果当前列未指定通配范围，则蕴含所有字段表达式列；
        ///     （3）如果当前列指定了通配范围，其通配范围为S0，目标字段所属的源为S1，当S0.ToString(Select-Clause)==S1.
        ///     ToString(Select-Clause)时，判定为蕴含。
        /// </summary>
        /// <returns>如果蕴含返回true，否则返回false。</returns>
        /// <param name="otherExp">作为目标投影列的表达式。</param>
        public bool Implies(Expression otherExp)
        {
            if (!(otherExp is FieldExpression)) return false;

            var otherField = (FieldExpression)otherExp;

            //如果当前列未指定通配范围
            if (Equals(_source, null)) return true;

            if (_source.ToString(EDataSource.SqlServer) ==
                otherField.Field.Source.ToString(EDataSource.SqlServer)) return true;

            return false;
        }
    }
}