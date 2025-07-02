/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：节点别名生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:06:27
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Text;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     节点别名生成器。
    /// </summary>
    public class AssociationTreeNodeAliasGenerator : IAssociationTreeUpwardVisitor<string>
    {
        /// <summary>
        ///     用于缓存别名的字典，其键为节点，值为别名。
        /// </summary>
        private readonly Dictionary<AssociationTreeNode, string> _aliasCache =
            new Dictionary<AssociationTreeNode, string>();

        /// <summary>
        ///     指示是否启用缓存。
        /// </summary>
        private bool _enableCache = true;

        /// <summary>
        ///     生成的别名。
        /// </summary>
        private string _nodeAlias;

        /// <summary>
        ///     获取或设置一个值，该值指示是否启用缓存。
        /// </summary>
        public bool EnableCache
        {
            get => _enableCache;
            set => _enableCache = value;
        }

        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        public string Result => _nodeAlias;

        /// <summary>
        ///     后置访问，即在访问父级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AssociationTree subTree, object childState, object previsitState)
        {
            //如果前置数据是true
            if (previsitState is bool previsitBool && previsitBool) return;
            if (!string.IsNullOrWhiteSpace(subTree.ElementName))
                //生成别名
                _nodeAlias = $"{_nodeAlias}_{subTree.ElementName}";
            //是否缓存
            if (!_enableCache) return;

            _aliasCache.Add(subTree.Node, _nodeAlias);
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
            //未启用缓存 继续向上递归
            if (_enableCache == false)
            {
                outChildState = null;
                outPrevisitState = null;
                return true;
            }

            //如果是根节点或父节点是反身引用
            if (subTree.IsRoot ||
                subTree.Parent?.RepresentedType.GetElement(subTree.ElementName) is SelfReference)
            {
                outPrevisitState = true;
                outChildState = null;
                _nodeAlias = null;
                return false;
            }

            //缓存项存在
            if (_aliasCache.TryGetValue(subTree.Node, out var value))
            {
                //取缓存
                _nodeAlias = value;
                outPrevisitState = true;
                outChildState = null;
                return false;
            }

            outChildState = null;
            outPrevisitState = null;
            return true;
        }


        /// <summary>
        ///     重置访问者。
        /// </summary>
        public virtual void Reset()
        {
            //Nothing to Do
        }

        /// <summary>
        ///     基于关联树的某一节点，生成指定指定元素指向的子节点的别名。
        ///     实施说明：
        ///     （1）根据别名协定生成；
        ///     （2）baseNodeAlias为空时推定基节点为根节点。
        /// </summary>
        /// <param name="element">指向子节点的元素。</param>
        /// <param name="baseNodeAlias">基节点别名。</param>
        public static string GenerateAlias(ReferenceElement element, string baseNodeAlias = null)
        {
            //放入基节点
            var stringBuilder = baseNodeAlias == null ? new StringBuilder() : new StringBuilder(baseNodeAlias);

            //反身节点 无别名
            if (element is SelfReference) return null;
            //其他的_element.Name
            stringBuilder.Append($"_{element.Name}");

            return stringBuilder.ToString();
        }
    }
}