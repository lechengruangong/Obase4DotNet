/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：强制包含执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:18:02
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     作为关联树向下访问者，执行强制包含。
    /// </summary>
    public class IncludingEnforcer : IAssociationTreeDownwardVisitor
    {
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
            //强制包含构造函数参数
            var parameters = subTree.RepresentedType.Constructor?.Parameters;

            if (parameters != null)
                foreach (var parameter in parameters)
                {
                    var paraElement = parameter.GetElement();
                    if (!(paraElement is ReferenceElement))
                        continue;
                    if (subTree.Node.HasChild(paraElement.Name))
                        continue;
                    subTree.Grow(paraElement.Name);
                }

            //强制包含关联端
            if (subTree.RepresentedType is AssociationType associationType)
            {
                var assoEnds = associationType.AssociationEnds;
                foreach (var end in assoEnds)
                {
                    if (end.EnableLazyLoading)
                        continue;
                    if (subTree.Node.HasChild(end.Name))
                        continue;
                    if (subTree.Node is ObjectTypeNode objectTypeNode &&
                        objectTypeNode.Element is AssociationReference assoref)
                    {
                        var left = assoref.GetLeftEnd();
                        if (end.Equals(left))
                            continue;
                    }

                    var hasAttribute = true;
                    foreach (var mapping in end.Mappings)
                    {
                        var targetField = associationType.FindAttributeByTargetField(mapping.TargetField);
                        if (targetField != null)
                            continue;
                        hasAttribute = false;
                        break;
                    }

                    if (hasAttribute)
                        continue;
                    subTree.Grow(end.Name);
                }
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