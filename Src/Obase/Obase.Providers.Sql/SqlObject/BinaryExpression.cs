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
        private Expression _left;

        /// <summary>
        ///     右操作数。
        /// </summary>
        private Expression _right;


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

        /// <summary>
        ///     替换布尔字段表达式
        ///     在SqlSever里 不能直接使用布尔字段作为条件表达式 需要转换为等于true的表达式
        ///     如果是常量布尔值 则替换为 1=1
        /// </summary>
        protected void ReplaceBoolField()
        {
            //如果是字段表达式 提取其中的字段 与 常量true 组合成结果
            if (_left is FieldExpression) _left = Equal(_left, new ConstantExpression(true));
            //如果是常量表达式 且值为布尔类型 则替换为 1=1
            if (_left is ConstantExpression constant1 && constant1.Value is bool)
                _left = Equal(new ConstantExpression(1), new ConstantExpression(1));

            //如果是字段表达式 提取其中的字段 与 常量true 组合成结果
            if (_right is FieldExpression) _right = Equal(_right, new ConstantExpression(true));
            //如果是常量表达式 且值为布尔类型 则替换为 1=1
            if (_right is ConstantExpression constant2 && constant2.Value is bool)
                _right = Equal(new ConstantExpression(1), new ConstantExpression(1));
        }
    }
}