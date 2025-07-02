/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表达式,Sql语句的基本单元.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:51:03
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表达式是构成Sql语句的基本单元，在Select、Where、Set、OrderBy、Join等子句中广泛存在。
    ///     常量表达式和字段表达式是两种基本表达式。两个或多个表达式经运算（如算术运算、关系运算、逻辑运算、函数运算）可得出一个更复杂的表达式。按此规则，不管一个表达式多么
    ///     复杂，最终都是由一系列基本表达式经多层次运算得出的，因此表达式可以看成树形结构（称为表达式树），其根节点为表达式本身，叶子节点为基本表达式，中间节点为算术表达式
    ///     、关系表达式、逻辑表达式或函数表达式等。
    ///     Expression类是一个抽象基类，表示表达式树节点的类派生自该基类。同时，它还包含用来创建各种节点类的 静态工厂方法。
    /// </summary>
    public abstract class Expression
    {
        /// <summary>
        ///     表达式的节点类型（运算类型）。
        /// </summary>
        private EExpressionType _nodeType;

        /// <summary>
        ///     表达式的静态类型。
        /// </summary>
        private Type _type;


        /// <summary>
        ///     获取表达式的节点类型（运算类型）。
        /// </summary>
        public EExpressionType NodeType => _nodeType;

        /// <summary>
        ///     获取表达式的静态类型。
        /// </summary>
        public Type Type => _type;

        /// <summary>
        ///     获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)_nodeType * 397) ^ (_type != null ? _type.GetHashCode() : 0);
            }
        }

        /// <summary>
        ///     创建一个Value属性设置为指定值的ConstantExpression。
        /// </summary>
        /// <param name="value">常量值。</param>
        public static ConstantExpression Constant(object value)
        {
            var temp = new ConstantExpression(value) { _nodeType = EExpressionType.Constant };
            return temp;
        }

        /// <summary>
        ///     创建一个Value属性和Type属性设置为指定值的ConstantExpression。
        /// </summary>
        /// <param name="value">常量值。</param>
        /// <param name="type">常量类型。</param>
        public static ConstantExpression Constant(object value, Type type)
        {
            var constant = new ConstantExpression(value) { _type = type, _nodeType = EExpressionType.Constant };
            return constant;
        }

        /// <summary>
        ///     创建一个表示算术加法运算的ArithmeticExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ArithmeticExpression Add(Expression left, Expression right)
        {
            var arithmeticExpression = new ArithmeticExpression(left, right) { _nodeType = EExpressionType.Add };
            return arithmeticExpression;
        }

        /// <summary>
        ///     创建一个表示算术减法运算的ArithmeticExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ArithmeticExpression Subtract(Expression left, Expression right)
        {
            var arithmeticExpression = new ArithmeticExpression(left, right) { _nodeType = EExpressionType.Subtract };
            return arithmeticExpression;
        }

        /// <summary>
        ///     创建一个表示算术乘法运算的ArithmeticExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ArithmeticExpression Multiply(Expression left, Expression right)
        {
            var arithmeticExpression = new ArithmeticExpression(left, right) { _nodeType = EExpressionType.Multiply };
            return arithmeticExpression;
        }

        /// <summary>
        ///     创建一个表示算术除法运算的ArithmeticExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ArithmeticExpression Devide(Expression left, Expression right)
        {
            var arithmeticExpression = new ArithmeticExpression(left, right) { _nodeType = EExpressionType.Divide };
            return arithmeticExpression;
        }

        /// <summary>
        ///     创建一个表示幂运算的ArithmeticExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ArithmeticExpression Power(Expression left, Expression right)
        {
            var arithmeticExpression = new ArithmeticExpression(left, right) { _nodeType = EExpressionType.Power };
            return arithmeticExpression;
        }

        /// <summary>
        ///     创建一个表示相等比较运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ComparisonExpression Equal(Expression left, Expression right)
        {
            var comparisonExpression = new ComparisonExpression(left, right) { _nodeType = EExpressionType.Equal };
            return comparisonExpression;
        }

        /// <summary>
        ///     创建一个表示不相等比较运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ComparisonExpression NotEqual(Expression left, Expression right)
        {
            var comparisonExpression = new ComparisonExpression(left, right) { _nodeType = EExpressionType.NotEqual };
            return comparisonExpression;
        }

        /// <summary>
        ///     创建一个表示“小于”比较运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ComparisonExpression LessThan(Expression left, Expression right)
        {
            var comparisonExpression = new ComparisonExpression(left, right) { _nodeType = EExpressionType.LessThan };
            return comparisonExpression;
        }

        /// <summary>
        ///     创建一个表示“小于或等于”比较运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ComparisonExpression LessThanOrEqual(Expression left, Expression right)
        {
            var comparisonExpression =
                new ComparisonExpression(left, right) { _nodeType = EExpressionType.LessThanOrEqual };
            return comparisonExpression;
        }

        /// <summary>
        ///     创建一个表示“大于”比较运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ComparisonExpression GreaterThan(Expression left, Expression right)
        {
            var comparisonExpression = new ComparisonExpression(left, right)
                { _nodeType = EExpressionType.GreaterThan };
            return comparisonExpression;
        }

        /// <summary>
        ///     创建一个表示“大于或等于”比较运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ComparisonExpression GreaterThanOrEqual(Expression left, Expression right)
        {
            var comparisonExpression =
                new ComparisonExpression(left, right) { _nodeType = EExpressionType.GreaterThanOrEqual };
            return comparisonExpression;
        }

        /// <summary>
        ///     创建一个表示LIKE运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="pattern">匹配模式。</param>
        /// <param name="likeType">Like类型</param>
        public static LikeExpression Like(Expression left, string pattern, ELikeType likeType = ELikeType.Contains)
        {
            return new LikeExpression(left, new ConstantExpression(pattern), likeType)
                { _nodeType = EExpressionType.Like };
        }

        /// <summary>
        ///     创建一个表示LIKE运算的ComparisonExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="pattern">表示匹配模式的表达式。</param>
        /// <param name="likeType">Like类型</param>
        public static LikeExpression Like(Expression left, Expression pattern, ELikeType likeType = ELikeType.Contains)
        {
            return new LikeExpression(left, pattern, likeType) { _nodeType = EExpressionType.Like };
        }

        /// <summary>
        ///     创建一个表示IN运算的表达式
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="valueSet">值域。</param>
        public static InExpression In(Expression left, object[] valueSet)
        {
            return new InExpression(left, valueSet, EInOperator.In) { _nodeType = EExpressionType.In };
        }

        /// <summary>
        ///     创建一个表示IN运算的表达式
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="valueSet">值域。</param>
        public static InExpression In(Expression left, IEnumerable valueSet)
        {
            return new InExpression(left, valueSet, EInOperator.In) { _nodeType = EExpressionType.In };
        }

        /// <summary>
        ///     创建一个表示NOT IN运算的表达式
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="valueSet">值域。</param>
        public static InExpression NotIn(Expression left, object[] valueSet)
        {
            return new InExpression(left, valueSet, EInOperator.Notin) { _nodeType = EExpressionType.NotIn };
        }

        /// <summary>
        ///     创建一个表示逻辑AND运算的BinaryLogicExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static BinaryLogicExpression AndAlse(Expression left, Expression right)
        {
            var binaryLogicExpression = new BinaryLogicExpression(left, right) { _nodeType = EExpressionType.AndAlso };
            return binaryLogicExpression;
        }

        /// <summary>
        ///     创建一个表示逻辑OR运算的BinaryLogicExpression。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static BinaryLogicExpression OrElse(Expression left, Expression right)
        {
            var binaryLogicExpression = new BinaryLogicExpression(left, right) { _nodeType = EExpressionType.OrElse };
            return binaryLogicExpression;
        }

        /// <summary>
        ///     创建一个表示逻辑求反运算的UnaryExpression。
        /// </summary>
        /// <param name="operand">操作数。</param>
        public static UnaryExpression Not(Expression operand)
        {
            return new UnaryExpression(operand) { _nodeType = EExpressionType.Not };
        }

        /// <summary>
        ///     创建一个表示函数调用的FunctionExpression。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="arguments">表示各实参的表达式组成的集合。</param>
        public static FunctionExpression Function(string functionName, Expression[] arguments)
        {
            return new FunctionExpression(functionName, arguments) { _nodeType = EExpressionType.Function };
        }

        /// <summary>
        ///     创建一个FunctionExpression，它表示对不带参数的函数的调用。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        public static FunctionExpression Function(string functionName)
        {
            return new FunctionExpression(functionName, Array.Empty<Expression>())
                { _nodeType = EExpressionType.Function };
        }

        /// <summary>
        ///     创建一个FunctionExpression，它表示对带一个参数的函数的调用。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="arg0">表示第一个实参的表达式。</param>
        public static FunctionExpression Function(string functionName, Expression arg0)
        {
            return new FunctionExpression(functionName, new[] { arg0 })
            {
                _nodeType = EExpressionType.Function
            };
        }

        /// <summary>
        ///     创建一个FunctionExpression，它表示对带两个参数的函数的调用。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="arg0">表示第一个实参的表达式。</param>
        /// <param name="arg1">表示第二个实参的表达式。</param>
        public static FunctionExpression Function(string functionName, Expression arg0, Expression arg1)
        {
            return new FunctionExpression(functionName, new[] { arg0, arg1 })
            {
                _nodeType = EExpressionType.Function
            };
        }

        /// <summary>
        ///     创建一个FunctionExpression，它表示对带三个参数的函数的调用。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="arg0">表示第一个实参的表达式。</param>
        /// <param name="arg1">表示第二个实参的表达式。</param>
        /// <param name="arg2">表示第三个实参的表达式。</param>
        public static FunctionExpression Function(string functionName, Expression arg0, Expression arg1,
            Expression arg2)
        {
            return new FunctionExpression(functionName, new[] { arg0, arg1, arg2 })
            {
                _nodeType = EExpressionType.Function
            };
        }

        /// <summary>
        ///     创建一个Field属性设置为指定值的FieldExpression。
        /// </summary>
        /// <param name="field">字段表达式所表示的字段。</param>
        public static FieldExpression Fields(Field field)
        {
            var fieldExpression = new FieldExpression(field)
            {
                _type = typeof(object),
                _nodeType = EExpressionType.Field
            };
            return fieldExpression;
        }


        /// <summary>
        ///     创建一个表示算术余数运算的ArithmeticExpression，其中Left属性为被除数，Right属性为除数。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static ArithmeticExpression Modulo(Expression left, Expression right)
        {
            var arithmetic = new ArithmeticExpression(left, right) { _nodeType = EExpressionType.Modulo };
            return arithmetic;
        }

        /// <summary>
        ///     创建一个表示递增运算（a+1，不就地修改a）的UnaryExpression。
        /// </summary>
        /// <param name="operand">操作数。</param>
        public static UnaryExpression Increment(Expression operand)
        {
            var unary = new UnaryExpression(operand) { _nodeType = EExpressionType.Increment };
            return unary;
        }

        /// <summary>
        ///     创建一个表示递减运算（a-1，不就地修改a）的UnaryExpression。
        /// </summary>
        /// <param name="operand">操作数。</param>
        public static UnaryExpression Decrement(Expression operand)
        {
            var unary = new UnaryExpression(operand) { _nodeType = EExpressionType.Decrement };
            return unary;
        }

        /// <summary>
        ///     创建一个表示一元加法运算（+a，不就地修改a）的UnaryExpression。
        /// </summary>
        /// <param name="operand">操作数。</param>
        public static UnaryExpression UnaryPlus(Expression operand)
        {
            var unary = new UnaryExpression(operand) { _nodeType = EExpressionType.UnaryPlus };
            return unary;
        }

        /// <summary>
        ///     创建一个表示算术求反运算（-a，不就地修改a）的UnaryExpression。
        /// </summary>
        /// <param name="operand">操作数。</param>
        public static UnaryExpression Negate(Expression operand)
        {
            var unary = new UnaryExpression(operand) { _nodeType = EExpressionType.Negate };
            return unary;
        }

        /// <summary>
        ///     构建一元运算表达式。
        /// </summary>
        /// <param name="operand">操作数。</param>
        /// <param name="type">一元运算类型。</param>
        public static UnaryExpression MakeUnary(Expression operand, EExpressionType type)
        {
            switch (type)
            {
                case EExpressionType.Decrement:
                    return Decrement(operand);
                case EExpressionType.Increment:
                    return Increment(operand);
                case EExpressionType.Negate:
                    return Negate(operand);
                case EExpressionType.Not:
                    return Not(operand);
                case EExpressionType.UnaryPlus:
                    return UnaryPlus(operand);
                case EExpressionType.BitOr:
                    return BitNot(operand);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"未知的一元表达式运算{type}");
            }
        }

        /// <summary>
        ///     创建一个表示按位与运算的表达式。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static BinaryBitExpression BitAnd(Expression left, Expression right)
        {
            return new BinaryBitExpression(left, right) { _nodeType = EExpressionType.BitAnd };
        }

        /// <summary>
        ///     创建一个表示按位取反运算的表达式。
        /// </summary>
        /// <param name="operand">操作数。</param>
        public static UnaryExpression BitNot(Expression operand)
        {
            return new UnaryExpression(operand) { _nodeType = EExpressionType.BitNot };
        }

        /// <summary>
        ///     创建一个表示按位或运算的表达式。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static BinaryBitExpression BitOr(Expression left, Expression right)
        {
            return new BinaryBitExpression(left, right) { _nodeType = EExpressionType.BitOr };
        }

        /// <summary>
        ///     创建一个表示按位异或运算的表达式。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static BinaryBitExpression BitXor(Expression left, Expression right)
        {
            return new BinaryBitExpression(left, right) { _nodeType = EExpressionType.BitXor };
        }

        /// <summary>
        ///     创建一个表示按位左移运算的表达式。
        /// </summary>
        /// <param name="left">左操作数</param>
        /// <param name="right">右操作数</param>
        public static BinaryBitExpression LeftShift(Expression left, Expression right)
        {
            return new BinaryBitExpression(left, right) { _nodeType = EExpressionType.LeftShift };
        }

        /// <summary>
        ///     创建一个表示按位右移运算的表达式。
        /// </summary>
        /// <param name="left">左操作数</param>
        /// <param name="right">右操作数</param>
        public static BinaryBitExpression RightShift(Expression left, Expression right)
        {
            return new BinaryBitExpression(left, right) { _nodeType = EExpressionType.RightShift };
        }

        /// <summary>
        ///     构建二元运算表达式。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        /// <param name="type">二元运算类型。</param>
        public static BinaryExpression MakeBinary(Expression left, Expression right, EExpressionType type)
        {
            switch (type)
            {
                case EExpressionType.Add:
                    return Add(left, right);
                case EExpressionType.AndAlso:
                    return AndAlse(left, right);
                case EExpressionType.Equal:
                    return Equal(left, right);
                case EExpressionType.GreaterThan:
                    return GreaterThan(left, right);
                case EExpressionType.GreaterThanOrEqual:
                    return GreaterThanOrEqual(left, right);
                case EExpressionType.LessThan:
                    return LessThan(left, right);
                case EExpressionType.LessThanOrEqual:
                    return LessThanOrEqual(left, right);
                case EExpressionType.Like:
                    return Like(left, right);
                case EExpressionType.Modulo:
                    return Modulo(left, right);
                case EExpressionType.Multiply:
                    return Multiply(left, right);
                case EExpressionType.NotEqual:
                    return NotEqual(left, right);
                case EExpressionType.OrElse:
                    return OrElse(left, right);
                case EExpressionType.Power:
                    return Power(left, right);
                case EExpressionType.Subtract:
                    return Subtract(left, right);
                case EExpressionType.BitAnd:
                    return BitAnd(left, right);
                case EExpressionType.BitOr:
                    return BitOr(left, right);
                case EExpressionType.BitXor:
                    return BitXor(left, right);
                case EExpressionType.LeftShift:
                    return LeftShift(left, right);
                case EExpressionType.RightShift:
                    return RightShift(left, right);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"未知的二元表达式运算{type}");
            }
        }

        /// <summary>
        ///     创建全局通配符表达式。
        /// </summary>
        public static WildcardExpression Wildcard()
        {
            return new WildcardExpression("*");
        }

        /// <summary>
        ///     创建在指定名称的源范围内的通配符表达式。
        /// </summary>
        /// <param name="source">源名称。</param>
        public static WildcardExpression Wildcard(string source)
        {
            return new WildcardExpression(source);
        }

        /// <summary>
        ///     创建在指定源范围内的通配符表达式。
        /// </summary>
        /// <param name="source">源。</param>
        public static WildcardExpression Wildcard(ISource source)
        {
            return new WildcardExpression(source);
        }

        /// <summary>
        ///     接受指定的访问者对当前表达式实例的访问。
        ///     注：本方法有可能返回一个新的表达式。如果访问者返回的表达式实例与当前实例相等，本方法返回当前实例，否则返回新实例。
        /// </summary>
        /// <returns>Expression 对当前表达式访问的结果。</returns>
        /// <param name="visitor">表达式访问者。</param>
        public Expression Accept(ExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }

        /// <summary>
        ///     确定指定的表达式与当前表达式是否相等。
        /// </summary>
        /// <param name="other">要与当前表达式进行比较的表达式。</param>
        protected virtual bool Equals(Expression other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (Equals(other, null))
                return false;
            if (GetType() != other.GetType())
                return false;
            if (_type == other._type && NodeType == other.NodeType && ConcreteEquals(other))
                return true;
            return false;
        }

        /// <summary>
        ///     确定指定的对象与当前表达式是否相等。（重写Object.Equals）
        /// </summary>
        /// <param name="otherObj">要与当前表达式进行比较的对象。</param>
        public override bool Equals(object otherObj)
        {
            return Equals(otherObj as Expression);
        }

        /// <summary>
        ///     派生类实现此方法以判定具体类型的表达式对象是否相等。
        /// </summary>
        /// <param name="other">要与当前表达式进行比较的表达式。</param>
        protected abstract bool ConcreteEquals(Expression other);

        /// <summary>
        ///     相等比较运算符。
        /// </summary>
        /// <param name="exp1">第一个操作数。</param>
        /// <param name="exp2">第二个操作数。</param>
        public static bool operator ==(Expression exp1, Expression exp2)
        {
            if (ReferenceEquals(exp1, exp2))
                return true;
            if (Equals(exp1, null))
                return false;
            return exp1.Equals(exp2);
        }

        /// <summary>
        ///     不相等比较运算符。
        /// </summary>
        /// <param name="exp1">第一个操作数。</param>
        /// <param name="exp2">第二个操作数。</param>
        public static bool operator !=(Expression exp1, Expression exp2)
        {
            return !(exp1 == exp2);
        }

        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public abstract string ToString(EDataSource sourceType);

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将表达式表示为字符串形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public abstract string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator);
    }
}