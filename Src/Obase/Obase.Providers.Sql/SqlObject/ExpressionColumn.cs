/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表达式投影列.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:51:03
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表达式投影列，即以一个表达式指定查询结果列。
    /// </summary>
    public class ExpressionColumn : SelectionColumn
    {
        /// <summary>
        ///     列的别名。
        /// </summary>
        private string _alias;

        /// <summary>
        ///     生成列的表达式。
        /// </summary>
        private Expression _expression;


        /// <summary>
        ///     获取或设置投影列的别名。
        /// </summary>
        public string Alias
        {
            get => _alias;
            set => _alias = value;
        }

        /// <summary>
        ///     获取或设置生成列的表达式。
        /// </summary>
        public Expression Expression
        {
            get => _expression;
            set => _expression = value;
        }

        /// <summary>
        ///     获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Expression.GetHashCode() + $"_{Alias}".GetHashCode();
        }

        /// <summary>
        ///     判断是否与另一个投影集中的列相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(SelectionColumn other)
        {
            if (ReferenceEquals(this, other))
                return true;
            var newOther = other as ExpressionColumn;
            if (Equals(newOther, null))
                return false;
            if (newOther.Expression == Expression && newOther.Alias == Alias)
                return true;
            return false;
        }

        /// <summary>
        ///     转换为Sql语句字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(EDataSource.SqlServer);
        }

        /// <summary>
        ///     转换为Sql语句字符串
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            return $"{_expression.ToString(sourceType)} {_alias}";
        }

        /// <summary>
        ///     设置投影列涉及到的源的别名的前缀。
        ///     注：只有简单源有别名，忽略非简单源。
        ///     实施建议：
        ///     （1）定义一个表达式访问者（继承自ExpressionVisitor），该访问者访问字段表达式，对字段源的别名追加前缀；
        ///     （2）对投影列的表达式调用其Accept方法，传入上述访问者。
        /// </summary>
        public override void SetSourceAliasPrefix(string prefix)
        {
            var setter = new SourceAliasRootSetter(prefix);
            Expression.Accept(setter);
        }
    }
}