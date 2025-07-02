/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：平展点添加器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:24:52
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query
{
    /// <summary>
    ///     作为一个关联树访问者，为退化路径极限分解得到的基础路径和附加路径添加平展点。
    /// </summary>
    public class FlatteningPointAdder : IAssociationTreeUpwardVisitor
    {
        /// <summary>
        ///     附加路径。
        /// </summary>
        private readonly AtrophyPath _attachingPath;

        /// <summary>
        ///     基础路径。
        /// </summary>
        private readonly AtrophyPath _basePath;

        /// <summary>
        ///     被分解的退化路径。
        /// </summary>
        private readonly AtrophyPath _decomposedPath;

        /// <summary>
        ///     创建FlatteningPointAdder实例。
        /// </summary>
        /// <param name="decomposedPath">被分解的退化路径。</param>
        /// <param name="basePath">基础路径。</param>
        /// <param name="attachingPath">附加路径。</param>
        public FlatteningPointAdder(AtrophyPath decomposedPath, AtrophyPath basePath, AtrophyPath attachingPath)
        {
            _decomposedPath = decomposedPath;
            _basePath = basePath;
            _attachingPath = attachingPath;
        }

        /// <summary>
        ///     后置访问，即在访问父级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AssociationTree subTree, object childState, object previsitState)
        {
            // 后置访问不需要执行任何操作
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
            AssociationTreeNode currentNode;
            AtrophyPath targetPath;
            if (childState != null)
            {
                var tempChildState = (object[])childState;
                //如果是之前获取到的
                currentNode = ((AssociationTreeNode)tempChildState[0]).Parent ?? _basePath.AssociationPath;
                targetPath = (AtrophyPath)tempChildState[1];
            }
            else
            {
                //首个访问的节点 就是基础路径的关联路径
                currentNode = _attachingPath.AssociationPath;
                targetPath = _attachingPath;
            }

            outChildState = new object[] { currentNode, targetPath };
            outPrevisitState = null;
            //获取被分解路径的平展点
            var points = _decomposedPath.FlatteningPoints;
            //添加平展点
            if (points.Contains(subTree.Node)) targetPath.AddFlatteningPoint(currentNode);
            return false;
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
        }
    }
}