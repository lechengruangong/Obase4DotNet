/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树生长器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:00:43
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联树生长器
    ///     执行关联树生长的访问者
    /// </summary>
    public class AssociationTreeGrower : IParameterizedAssociationTreeDownwardVisitor<AssociationTree>
    {
        /// <summary>
        ///     生成的关联树。
        /// </summary>
        private AssociationTree _growingTree;

        /// <summary>
        ///     为即将开始的遍历操作设置参数。
        /// </summary>
        /// <param name="argument">参数值。</param>
        public void SetArgument(AssociationTree argument)
        {
            _growingTree = argument;
        }

        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(AssociationTree subTree, object parentState, out object outParentState,
            out object outPrevisitState)
        {
            if (subTree.Parent == null)
            {
                outParentState = _growingTree.Node;
                outPrevisitState = null;
                return true;
            }

            //如果父级是AssociationTree，则获取其Node
            if (parentState is AssociationTree associationTree) parentState = associationTree.Node;

            if (parentState is AssociationTreeNode associationnode)
            {
                var correspondingNode = associationnode.GetChild(subTree.ElementName);
                if (correspondingNode != null)
                {
                    outParentState = correspondingNode;
                    outPrevisitState = null;
                    return true;
                }

                //与自己相同的类型 不添加
                if (subTree.Node is ObjectTypeNode objectTypeNode &&
                    associationnode.RepresentedType != objectTypeNode.RepresentedType)
                {
                    var result = associationnode.AddChild(objectTypeNode);
                    //如果不是因为同名 而是因为同类型而添加的
                    if (result.ElementName != subTree.ElementName)
                    {
                        outParentState = result;
                        outPrevisitState = null;
                        return true;
                    }
                }

                outParentState = null;
                outPrevisitState = null;
                return false;
            }

            outParentState = null;
            outPrevisitState = null;
            return false;
        }

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AssociationTree subTree, object parentState, object previsitState)
        {
            //Nothing to do
        }

        /// <summary>
        ///     重置访问者。
        /// </summary>
        public void Reset()
        {
            //Nothing to do
        }
    }
}