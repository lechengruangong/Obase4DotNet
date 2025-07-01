/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标识列生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:27:49
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     标识列生成器。
    /// </summary>
    public class IdColumnGenerator : IAssociationTreeDownwardVisitor
    {
        /// <summary>
        ///     节点别名生成器，用于生成源扩展树各节点的别名。
        /// </summary>
        private readonly AliasGenerator _aliasGenerator;

        /// <summary>
        ///     源联接备忘录。
        /// </summary>
        private readonly JoinMemo _joinMemo;

        /// <summary>
        ///     投影集，作为收集所生成的标识列的容器。
        /// </summary>
        private readonly ISelectionSet _selectionSet;

        /// <summary>
        ///     创建IDColumnGenerator实例。
        /// </summary>
        /// <param name="aliasGenerator">别名生成器。</param>
        /// <param name="joinMemo">源联接备忘录。</param>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        public IdColumnGenerator(AliasGenerator aliasGenerator, JoinMemo joinMemo, ISelectionSet selectionSet)
        {
            _aliasGenerator = aliasGenerator;
            _joinMemo = joinMemo;
            _selectionSet = selectionSet;
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
            //_aliasGenerator.SetArgument(null);
            var nodeAlias = subTree.Accept(_aliasGenerator);
            var source = _joinMemo.GetSource(nodeAlias) ?? _joinMemo.GetSource(null);

            if (subTree.RepresentedType is IMappable mappable)
            {
                var filedNames = mappable.KeyFields;

                foreach (var keyName in filedNames ?? new List<string>())
                {
                    var columnName = subTree.Accept(_aliasGenerator, keyName);
                    _selectionSet?.Add(new Field(source, keyName), columnName);
                }
            }

            outParentState = null;
            outPrevisitState = null;

            return true;
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
        ///     重置访问者
        /// </summary>
        public void Reset()
        {
            //Nothing to Do
        }
    }
}