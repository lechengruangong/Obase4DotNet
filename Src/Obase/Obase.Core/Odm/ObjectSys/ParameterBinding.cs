/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：形参绑定.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:37:44
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     表示形参绑定。
    ///     形参绑定是指Lambda表达式形式参数的取值，该值也是一个表达式。
    /// </summary>
    public class ParameterBinding
    {
        /// <summary>
        ///     作为形参值的表达式。
        /// </summary>
        private readonly Expression _expression;

        /// <summary>
        ///     lambda表达式的形式参数。
        /// </summary>
        private readonly ParameterExpression _parameter;

        /// <summary>
        ///     形参指代，表明该形参指代的内容，如查询源中的单个对象、查询源序列等。
        /// </summary>
        private readonly EParameterReferring _referring;


        /// <summary>
        ///     创建ParameterBinding实例。
        /// </summary>
        /// <param name="parameter">形参。</param>
        /// <param name="referring">形参指代。</param>
        /// <param name="expression">作为形参取值的表达式。</param>
        public ParameterBinding(ParameterExpression parameter, EParameterReferring referring,
            Expression expression = null)
        {
            _parameter = parameter;
            _expression = expression;
            _referring = referring;
        }

        /// <summary>
        ///     创建ParameterBinding实例，在该绑定中，形式参数指代查询源中的单个对象或值。
        /// </summary>
        /// <param name="parameter">形参。</param>
        /// <param name="expression">作为形参取值的表达式。</param>
        public ParameterBinding(ParameterExpression parameter, Expression expression) : this(parameter,
            EParameterReferring.Single, expression)
        {
        }

        /// <summary>
        ///     获取作为绑定目标的表达式。
        /// </summary>
        public Expression Expression => _expression;

        /// <summary>
        ///     获取该绑定的形式参数。
        /// </summary>
        public ParameterExpression Parameter => _parameter;

        /// <summary>
        ///     获取形参指代。
        /// </summary>
        public EParameterReferring Referring => _referring;
    }
}