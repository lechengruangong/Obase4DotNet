/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：向下遍历关联树过程中对子树实施访问的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:22:25
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     定义在向下遍历关联树过程中对子树实施访问的规范。
    /// </summary>
    public interface IAssociationTreeDownwardVisitor
    {
        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        bool Previsit(AssociationTree subTree, object parentState, out object outParentState,
            out object outPrevisitState);

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        void Postvisit(AssociationTree subTree, object parentState, object previsitState);

        /// <summary>
        ///     重置访问者。
        /// </summary>
        void Reset();
    }

    /// <summary>
    ///     定义在向下遍历关联树过程中对子树实施访问的规范，该遍历操作会返回一个结果。
    /// </summary>
    /// <typeparam name="TResult">遍历操作返回结果的类型。</typeparam>
    public interface IAssociationTreeDownwardVisitor<out TResult> : IAssociationTreeDownwardVisitor
    {
        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        TResult Result { get; }
    }

    /// <summary>
    ///     定义在向下遍历关联树过程中对子树实施访问的规范，该遍历操作会返回一个结果并以输出参数返回另一结果
    /// </summary>
    /// <typeparam name="TResult">遍历操作返回结果的类型</typeparam>
    /// <typeparam name="TOut">输出参数的类型</typeparam>
    public interface IAssociationTreeDownwardVisitor<out TResult, out TOut> : IAssociationTreeDownwardVisitor<TResult>
    {
        /// <summary>
        ///     获取输出参数的值。
        /// </summary>
        TOut OutArgument { get; }
    }

    /// <summary>
    ///     定义在向下遍历关联树过程中对子树实施访问的规范，该遍历操作接收一个参数。
    /// </summary>
    /// <typeparam name="TArg">遍历操作参数的类型。</typeparam>
    public interface IParameterizedAssociationTreeDownwardVisitor<in TArg> : IAssociationTreeDownwardVisitor
    {
        /// <summary>
        ///     为即将开始的遍历操作设置参数。
        /// </summary>
        /// <param name="argument">参数值。</param>
        void SetArgument(TArg argument);
    }

    /// <summary>
    ///     定义在向下遍历关联树过程中对子树实施访问的规范，该遍历操作接收一个参数并返回一个结果。
    /// </summary>
    /// <typeparam name="TArg">遍历操作参数的类型。</typeparam>
    /// <typeparam name="TResult">遍历操作返回结果的类型。</typeparam>
    public interface IParameterizedAssociationTreeDownwardVisitor<in TArg, out TResult> :
        IAssociationTreeDownwardVisitor<TResult>, IParameterizedAssociationTreeDownwardVisitor<TArg>
    {
    }

    /// <summary>
    ///     定义在向下遍历关联树过程中对子树实施访问的规范，该遍历操作接收一个参数，返回一个结果并以输出参数返回另一结果。
    /// </summary>
    /// <typeparam name="TArg">遍历操作参数的类型</typeparam>
    /// <typeparam name="TResult">遍历操作返回结果的类型</typeparam>
    /// <typeparam name="TOut">输出参数的类型</typeparam>
    public interface IParameterizedAssociationTreeDownwardVisitor<in TArg, out TResult, out TOut> :
        IAssociationTreeDownwardVisitor<TResult, TOut>, IParameterizedAssociationTreeDownwardVisitor<TArg, TResult>
    {
    }
}