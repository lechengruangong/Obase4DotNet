/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图在源扩展树上的平展节点.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:19:45
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     表示视图在源扩展树上的平展节点以及代表该节点的形参。
    /// </summary>
    public class ViewFlatteningPoint
    {
        /// <summary>
        ///     源扩展树上的一个节点，表示在该节点上实施平展。
        /// </summary>
        private readonly AssociationTreeNode _extensionNode;

        /// <summary>
        ///     平展形参，即在表达式（如视图属性的绑定表达式）中代表平展点的形式参数。
        /// </summary>
        private readonly ParameterExpression _flatteningParameter;

        /// <summary>
        ///     创建ViewFlatteningPoint实例。
        /// </summary>
        /// <param name="extensionNode">源扩展树上的节点。</param>
        /// <param name="flatteningPara">平展形参。</param>
        public ViewFlatteningPoint(AssociationTreeNode extensionNode, ParameterExpression flatteningPara)
        {
            _extensionNode = extensionNode;
            _flatteningParameter = flatteningPara;
        }

        /// <summary>
        ///     获取源扩展树的节点，该节点为平展节点。
        /// </summary>
        public AssociationTreeNode ExtensionNode => _extensionNode;

        /// <summary>
        ///     获取平展形参。平展形参是在表达式（如视图属性的绑定表达式）中代表平展点的形式参数。
        /// </summary>
        public ParameterExpression FlatteningParameter => _flatteningParameter;
    }
}