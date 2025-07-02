/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示二元运算的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:55:17
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示二元运算的表达式。
    /// </summary>
    public abstract class BinaryExpression : Expression
    {
        /// <summary>
        ///     左操作数。
        /// </summary>
        private readonly Expression _left;

        /// <summary>
        ///     右操作数。
        /// </summary>
        private readonly Expression _right;


        /// <summary>
        ///     创建BinaryExpression的实例，并设置Left属性和Right属性的值。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        protected BinaryExpression(Expression left, Expression right)
        {
            _left = left;
            _right = right;
        }

        /// <summary>
        ///     获取左操作数。
        /// </summary>
        public Expression Left => _left;

        /// <summary>
        ///     获取右操作数。
        /// </summary>
        public Expression Right => _right;

        /// <summary>
        ///     判定具体类型的表达式对象是否相等
        /// </summary>
        /// <param name="other">另外一个表达式</param>
        /// <returns></returns>
        protected override bool ConcreteEquals(Expression other)
        {
            var binaryOther = other as BinaryExpression;
            //对比左操作数和右操作数
            if (binaryOther != null && Left == binaryOther.Left && Right == binaryOther.Right)
                return true;
            return false;
        }
    }
}