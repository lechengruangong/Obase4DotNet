/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：向下遍历属性树过程中对子树实施访问的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:27:26
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     定义在向下遍历属性树过程中对子树实施访问的规范。
    /// </summary>
    public interface IAttributeTreeDownwardVisitor
    {
        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        void Previsit(AttributeTree subTree, object parentState, out object outParentState,
            out object outPrevisitState);

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        void Postvisit(AttributeTree subTree, object parentState, object previsitState);

        /// <summary>
        ///     重置访问者。
        /// </summary>
        void Reset();
    }

    /// <summary>
    ///     定义在向下遍历属性树过程中对子树实施访问的规范，该遍历操作会返回一个结果。
    /// </summary>
    /// <typeparam name="TResult">遍历操作返回结果的类型。</typeparam>
    public interface IAttributeTreeDownwardVisitor<out TResult> : IAttributeTreeDownwardVisitor
    {
        /// <summary>
        ///     获取遍历属性树的结果。
        /// </summary>
        TResult Result { get; }
    }
}