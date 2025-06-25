/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性树的节点枚举器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:23:52
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     属性树节点枚举器，用于将树节点依次写入一个序列。
    ///     使用者注意：写入顺序依赖于遍历算法，不承诺在后续版本维持此顺序。
    /// </summary>
    public class AttributeTreeNodeEnumerator : IAttributeTreeDownwardVisitor<IEnumerable<AttributeTreeNode>>
    {
        /// <summary>
        ///     指示是否忽略代表复杂属性节点。
        /// </summary>
        private bool _ignoreComplex = true;

        /// <summary>
        ///     遍历属性树的结果
        /// </summary>
        private IEnumerable<AttributeTreeNode> _result;

        /// <summary>
        ///     获取或设置一个值，该值指示是否忽略代表复杂属性节点。
        /// </summary>
        public bool IgnoreComplex
        {
            get => _ignoreComplex;
            set => _ignoreComplex = value;
        }

        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public void Previsit(AttributeTree subTree, object parentState, out object outParentState,
            out object outPrevisitState)
        {
            //当前节点
            var node = subTree.Node;
            //flag
            outParentState = null;
            outPrevisitState = null;
            _result = new List<AttributeTreeNode>();
            //处理结果
            if (!(node.Attribute.IsComplex && _ignoreComplex)) ((List<AttributeTreeNode>)_result).Add(node);
        }

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AttributeTree subTree, object parentState, object previsitState)
        {
            //Nothing to Do
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            //Nothing to Do
        }

        /// <summary>
        ///     获取遍历属性树的结果。
        /// </summary>
        public IEnumerable<AttributeTreeNode> Result => _result;
    }
}