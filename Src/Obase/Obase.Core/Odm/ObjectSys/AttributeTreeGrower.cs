/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性树生长器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:19:36
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     属性树生长器。
    /// </summary>
    public class AttributeTreeGrower : IAttributeTreeDownwardVisitor
    {
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
            //读取属性
            var attr = subTree.Attribute;
            //非复杂属性 直接返回
            if (!attr.IsComplex)
            {
                outParentState = null;
                outPrevisitState = null;
            }

            //取出复杂属性中的属性
            var attrs = (attr as ComplexAttribute)?.ComplexType.Attributes;
            //加入复杂类型的属性
            if (attrs != null && attrs.Count > 0)
                foreach (var subAttr in attrs)
                {
                    var sub = new AttributeTree(subAttr);
                    subTree.AddSubTree(sub);
                }

            outParentState = null;
            outPrevisitState = null;
        }

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AttributeTree subTree, object parentState, object previsitState)
        {
            //Nohing To Do
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            //Nothing to Do
        }
    }
}