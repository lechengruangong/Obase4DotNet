/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举表达式运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:51:03
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举表达式运算。
    /// </summary>
    public enum EExpressionType
    {
        /// <summary>
        ///     算术加法运算。
        /// </summary>
        Add,

        /// <summary>
        ///     逻辑AND运算。
        /// </summary>
        AndAlso,

        /// <summary>
        ///     表示常量值。
        /// </summary>
        Constant,

        /// <summary>
        ///     递减运算（a-1），不应就地修改a。
        /// </summary>
        Decrement,

        /// <summary>
        ///     算术除法运算。
        /// </summary>
        Divide,

        /// <summary>
        ///     相等比较运算。
        /// </summary>
        Equal,

        /// <summary>
        ///     表达关系表的一个字段。
        /// </summary>
        Field,

        /// <summary>
        ///     调用某一函数的运算。
        /// </summary>
        Function,

        /// <summary>
        ///     “大于”比较运算。
        /// </summary>
        GreaterThan,

        /// <summary>
        ///     “大于或等于”比较运算。
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        ///     “IN”运算。
        /// </summary>
        In,

        /// <summary>
        ///     递增运算（a+1），不应就地修改a。
        /// </summary>
        Increment,

        /// <summary>
        ///     “小于”比较运算。
        /// </summary>
        LessThan,

        /// <summary>
        ///     “小于或等于”比较运算。
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        ///     “LIKE”运算。
        /// </summary>
        Like,

        /// <summary>
        ///     算术余数运算。
        /// </summary>
        Modulo,

        /// <summary>
        ///     算术乘法运算。
        /// </summary>
        Multiply,

        /// <summary>
        ///     算术取反运算（-a），不应就地修改a。
        /// </summary>
        Negate,

        /// <summary>
        ///     逻辑求反运算。
        /// </summary>
        Not,

        /// <summary>
        ///     不相等比较运算。
        /// </summary>
        NotEqual,

        /// <summary>
        ///     “NOT IN”运算。
        /// </summary>
        NotIn,

        /// <summary>
        ///     逻辑OR运算。
        /// </summary>
        OrElse,

        /// <summary>
        ///     幂运算。
        /// </summary>
        Power,

        /// <summary>
        ///     算术减法运算。
        /// </summary>
        Subtract,

        /// <summary>
        ///     一元加法运算（+a），不应就地修改a。
        /// </summary>
        UnaryPlus,

        /// <summary>
        ///     按位与运算。
        /// </summary>
        BitAnd,

        /// <summary>
        ///     按位取反运算。
        /// </summary>
        BitNot,

        /// <summary>
        ///     按位或运算。
        /// </summary>
        BitOr,

        /// <summary>
        ///     按位异或运算。
        /// </summary>
        BitXor,

        /// <summary>
        ///     按位左移运算。
        /// </summary>
        LeftShift,

        /// <summary>
        ///     按位右移运算。
        /// </summary>
        RightShift
    }
}