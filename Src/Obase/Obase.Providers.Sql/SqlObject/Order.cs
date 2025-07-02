/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示排序规则.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:24:58
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示排序规则。排序规则由排序依据（字段）和排序方向构成。
    /// </summary>
    public class Order
    {
        /// <summary>
        ///     作为排序依据的表达式。
        /// </summary>
        private readonly Expression _expression;

        /// <summary>
        ///     排序方向
        /// </summary>
        private EOrderDirection _direction;


        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="field">作为排序依据的字段</param>
        public Order(string field) : this(Expression.Fields(new Field(field)))
        {
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="field">排序字段</param>
        /// <param name="direction">排序方向</param>
        public Order(Field field, EOrderDirection direction) : this(Expression.Fields(field))
        {
            _direction = direction;
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="source">作为排序依据的字段所属的源</param>
        /// <param name="field">作为排序依据的字段</param>
        /// <param name="direction">排序方向</param>
        public Order(ISource source, string field, EOrderDirection direction) : this(
            new Field((MonomerSource)source, field),
            direction)
        {
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="source">作为排序依据的字段所属的源</param>
        /// <param name="field">作为排序依据的字段</param>
        public Order(ISource source, string field) : this(Expression.Fields(new Field((MonomerSource)source, field)))
        {
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="field">作为排序依据的字段</param>
        /// <param name="direction">排序方向</param>
        public Order(string field, EOrderDirection direction) : this(new Field(field), direction)
        {
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="source">作为排序依据的字段所属源的名称</param>
        /// <param name="field">作为排序依据的字段</param>
        public Order(string source, string field) : this(Expression.Fields(new Field(source, field)))
        {
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="source">作为排序依据的字段所属源的名称</param>
        /// <param name="field">作为排序依据的字段</param>
        /// <param name="direction">排序方向</param>
        public Order(string source, string field, EOrderDirection direction) : this(new Field(source, field), direction)
        {
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="expression">排序表达式</param>
        public Order(Expression expression)
        {
            _expression = expression;
        }

        /// <summary>
        ///     创建排序规则。
        /// </summary>
        /// <param name="expression">排序字段</param>
        /// <param name="direction">排序方向</param>
        public Order(Expression expression, EOrderDirection direction)
            : this(expression)
        {
            Direction = direction;
        }

        /// <summary>
        ///     获取作为排序依据的表达式。
        /// </summary>
        public Expression Expression => _expression;

        /// <summary>
        ///     获取作为排序依据的字段。
        /// </summary>
        public Field Field => ((FieldExpression)_expression).Field;

        /// <summary>
        ///     获取或设置排序方向。
        /// </summary>
        public EOrderDirection Direction
        {
            get => _direction;
            set => _direction = value;
        }

        /// <summary>
        ///     将表达式访问者引导至排序依据表达式。
        /// </summary>
        /// <param name="visitor">要引导的表达式访问者。</param>
        public void GuideExpressionVisitor(ExpressionVisitor visitor)
        {
            Expression.Accept(visitor);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(EDataSource.SqlServer);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            return $"{Expression.ToString(sourceType)}  {Direction}";
        }

        /// <summary>
        ///     私有Equal方法
        /// </summary>
        /// <param name="other">另一个Order</param>
        /// <returns></returns>
        private bool Equals(Order other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_expression, other._expression) && _direction == other._direction;
        }

        /// <summary>
        ///     重写Equal方法
        /// </summary>
        /// <param name="obj">另一个Order</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Order)obj);
        }

        /// <summary>
        ///     重写获取哈希
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_expression != null ? _expression.GetHashCode() : 0) * 397) ^ (int)_direction;
            }
        }

        /// <summary>
        ///     重写==运算符
        /// </summary>
        /// <param name="left">一个Order</param>
        /// <param name="right">另一个Order</param>
        /// <returns></returns>
        public static bool operator ==(Order left, Order right)
        {
            if (Equals(left, null) && Equals(right, null)) return true;
            return !Equals(left, null) && left.Equals(right);
        }

        /// <summary>
        ///     重写!=运算符
        /// </summary>
        /// <param name="left">一个Order</param>
        /// <param name="right">另一个Order</param>
        /// <returns></returns>
        public static bool operator !=(Order left, Order right)
        {
            return !(left == right);
        }
    }
}