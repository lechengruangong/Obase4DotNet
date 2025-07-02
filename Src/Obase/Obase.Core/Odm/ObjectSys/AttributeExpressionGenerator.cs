/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性树节点表达式生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:19:12
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     用于生成指向属性节点的表达式。
    /// </summary>
    public class AttributeExpressionGenerator : IAttributeTreeUpwardVisitor<LambdaExpression>
    {
        /// <summary>
        ///     宿主表达式
        /// </summary>
        private readonly LambdaExpression _hostExp;

        /// <summary>
        ///     结果表达式
        /// </summary>
        private Expression _resultExp;

        /// <summary>
        ///     创建AttributeExpressionGenerator实例。
        /// </summary>
        /// <param name="hostExp">一个Lambda表达式，其主体（Body）的Type为定义属性树根节点代表的属性的类型。</param>
        public AttributeExpressionGenerator(LambdaExpression hostExp)
        {
            _hostExp = hostExp;
        }

        /// <summary>
        ///     获取遍历属性树的结果。
        /// </summary>
        public LambdaExpression Result => Expression.Lambda(_resultExp, _hostExp?.Parameters);

        /// <summary>
        ///     后置访问，即在访问父级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AttributeTree subTree, object childState, object previsitState)
        {
            //顶级节点 为_resultExp赋值
            if (subTree.Parent == null) _resultExp = _hostExp.Body;

            _resultExp = Expression.PropertyOrField(_resultExp, subTree.AttributeName);
        }

        /// <summary>
        ///     前置访问，即在访问父级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="outChildState">返回一个状态数据，在遍历到父级时该数据将被视为子级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(AttributeTree subTree, object childState, out object outChildState,
            out object outPrevisitState)
        {
            //不需要前置访问
            outChildState = outPrevisitState = null;
            return false;
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
            _resultExp = null;
        }
    }
}