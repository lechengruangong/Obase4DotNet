/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：用于生成指向关联节点的表达式的生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:32:36
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     用于生成指向关联节点的表达式。
    /// </summary>
    public class AssociationExpressionGenerator : IAssociationTreeUpwardVisitor<LambdaExpression>
    {
        /// <summary>
        ///     平展形参获取委托。
        /// </summary>
        private readonly Func<AssociationTreeNode, ParameterExpression> _flatteningParaGetter;

        /// <summary>
        ///     参数
        /// </summary>
        private readonly ParameterExpression[] _parameters = new ParameterExpression[2];

        /// <summary>
        ///     代表查询源的形参.
        /// </summary>
        private readonly ParameterExpression _sourceParameter;


        /// <summary>
        ///     元素名称
        /// </summary>
        private string _memberName;

        /// <summary>
        ///     结果表达式
        /// </summary>
        private Expression _resultExp;

        /// <summary>
        ///     创建AssociationExpressionGenerator实例。
        /// </summary>
        /// <param name="sourcePara">代表查询源的形参。</param>
        /// <param name="flatteningParaGetter">平展形参获取委托。</param>
        public AssociationExpressionGenerator(ParameterExpression sourcePara,
            Func<AssociationTreeNode, ParameterExpression> flatteningParaGetter = null)
        {
            _sourceParameter = sourcePara;
            _flatteningParaGetter = flatteningParaGetter;
            _parameters[0] = _sourceParameter;
        }

        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        public LambdaExpression Result
        {
            get
            {
                return _resultExp != null
                    ? Expression.Lambda(_resultExp, _parameters.Where(p => p != null).ToArray())
                    : null;
            }
        }

        /// <summary>
        ///     后置访问，即在访问父级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AssociationTree subTree, object childState, object previsitState)
        {
            if (previsitState != null)
            {
                _resultExp = (Expression)previsitState;
            }
            else
            {
                var parentType = subTree.Parent.RepresentedType;
                var representedRef = parentType.GetReferenceElement(subTree.ElementName);
                //对应图中蓝色部分
                var navType = representedRef.Navigation.NavigationType;
                var navUse = representedRef.NavigationUse;

                if (navType == ENavigationType.Indirectly || navUse == ENavigationUse.DirectlyReference ||
                    subTree.Parent.Parent == null)
                    _memberName = subTree.ElementName;

                if (navType == ENavigationType.Indirectly || navUse == ENavigationUse.ArrivingReference)
                    _resultExp = Expression.PropertyOrField(_resultExp, _memberName);
            }
        }

        /// <summary>
        ///     前置访问，即在访问父级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="outChildState">返回一个状态数据，在遍历到父级时该数据将被视为子级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(AssociationTree subTree, object childState, out object outChildState,
            out object outPrevisitState)
        {
            outChildState = null;
            outPrevisitState = null;
            if (subTree.IsRoot)
            {
                outPrevisitState = _sourceParameter;
                return false;
            }

            //如果符合条件 不再遍历 返回平展参数
            if (Contraint())
            {
                var flatteningPara = _flatteningParaGetter(subTree.Node);
                if (flatteningPara == null)
                    throw new ArgumentException("生成指向引用的表达式错误,无法获取平展参数.");
                outPrevisitState = _parameters[1] = flatteningPara;
                return false;
            }

            return true;

            //判断函数
            bool Contraint()
            {
                //获取父级
                var parentType = subTree.Parent.RepresentedType;
                //获取当前节点代表的元素
                var representedRef = parentType.GetReferenceElement(subTree.ElementName);
                //当前节点属性
                var navUse = representedRef.NavigationUse;
                var isMultiple = representedRef.IsMultiple;
                var navType = representedRef.Navigation.NavigationType;
                //父级是否为一对多
                var parentIsMultiple = subTree.Parent?.Element?.IsMultiple ?? false;
                //当前子树个数
                var subCount = subTree.SubCount;

                return ((navUse == ENavigationUse.EmittingReference && isMultiple) ||
                        (navUse == ENavigationUse.ArrivingReference && navType == ENavigationType.Directly &&
                         parentIsMultiple)) && subCount > 0;
            }
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
            _resultExp = null;
            _memberName = null;
        }
    }
}