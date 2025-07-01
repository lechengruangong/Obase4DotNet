/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：投影列生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:08:01
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     投影列生成器，为被其访问的属性树的映射字段生成投影列。
    ///     属性树的映射字段是指属性树各简单属性节点所代表属性的映射字段，它是一个字段集合。
    /// </summary>
    public class SelectionColumnGenerator : IAttributeTreeDownwardVisitor
    {
        /// <summary>
        ///     要生成投影列的属性树在关联树上的锚点，即属性属于哪个关联树节点代表的类型。如果不指定，则认为属于根节点。
        /// </summary>
        private readonly AssociationTreeNode _anchor;

        /// <summary>
        ///     被访问属性树的锚点的别名。不启用别名时，本属性无效。
        ///     注：本属性用于提供向前兼容性，一般情况下请使用属性树锚点。
        /// </summary>
        private readonly string _anchorAlias;

        /// <summary>
        ///     指示是否启用别名。
        /// </summary>
        private readonly bool _enableAlias;

        /// <summary>
        ///     属性树映射字段所属的源。
        /// </summary>
        private readonly MonomerSource _source;

        /// <summary>
        ///     映射字段生成器，用于生成属性对应的映射字段，（将基于该字段生成投影列）。
        /// </summary>
        private readonly TargetFieldGenerator _targetFieldGenerator = new TargetFieldGenerator { EnableCache = true };

        /// <summary>
        ///     别名生成器，用于在生成投影列的过程中生成投影列的别名。
        /// </summary>
        private AliasGenerator _aliasGenerator;

        /// <summary>
        ///     收集投影列的投影集。
        /// </summary>
        private ISelectionSet _selectionSet;

        /// <summary>
        ///     创建SelectionColumnGenerator实例，被访问属性树的映射字段属于指定的源，同时指定收集投影列的投影集。
        /// </summary>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        /// <param name="source">属性树映射字段所属的源。</param>
        /// <param name="anchor">属性树的锚点。不指定表示不启用别名。</param>
        public SelectionColumnGenerator(ISelectionSet selectionSet, MonomerSource source,
            AssociationTreeNode anchor = null)
        {
            _selectionSet = selectionSet;
            _source = source;
            if (anchor != null)
            {
                _anchor = anchor;
                _enableAlias = true;
            }
        }

        /// <summary>
        ///     创建SelectionColumnGenerator实例，被访问属性树的映射字段属于属性树锚点的映射源（该源的别名为锚点的别名，如果未指定锚点则不为这些字段明确
        ///     源），同时指定收集投影列的投影集。
        ///     锚点的映射源是指作为锚点的关联树节点代表的类型的映射源。
        ///     实施说明
        ///     使用SimpleSource创建锚点的映射源，使用AliasGenerator生成锚点的别名。
        /// </summary>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        /// <param name="anchor">属性树的锚点。不指定表示不启用别名。</param>
        public SelectionColumnGenerator(ISelectionSet selectionSet, AssociationTreeNode anchor = null)
        {
            _selectionSet = selectionSet;
            if (anchor != null && anchor.RepresentedType is ObjectType objectType)
            {
                //生成别名
                var aliasGen = new AliasGenerator();
                var alias = anchor.AsTree().Accept(aliasGen);
                _source = new SimpleSource(objectType.TargetName, alias);
                _enableAlias = true;
            }
        }

        /// <summary>
        ///     创建SelectionColumnGenerator实例，被访问属性树的映射字段属于指定的源，同时指定收集投影列的投影集。
        ///     本构造方法用于提供向前兼容性，一般情况下请使用带anchor参数的版本。
        /// </summary>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        /// <param name="source">属性树映射字段所属的源。</param>
        /// <param name="anchorAlias">属性树锚点的别名。不指定则表示不启用别名。</param>
        public SelectionColumnGenerator(ISelectionSet selectionSet, MonomerSource source, string anchorAlias)
        {
            _selectionSet = selectionSet;
            _source = source;
            _anchorAlias = anchorAlias;
            _enableAlias = true;
        }

        /// <summary>
        ///     创建SelectionColumnGenerator实例，被访问属性树的映射字段属于一个简单源（该源的别名为指定的锚点别名，如果未指定锚点别名则不为这些字段明确
        ///     源），同时指定收集投影列的投影集。
        ///     本构造方法用于提供向前兼容性，一般情况下请使用带anchor参数的版本。
        /// </summary>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        /// <param name="anchorAlias">属性树锚点的别名。不指定则表示不启用别名。</param>
        public SelectionColumnGenerator(ISelectionSet selectionSet, string anchorAlias)
        {
            _selectionSet = selectionSet;
            _source = new SimpleSource(anchorAlias);
            _anchorAlias = anchorAlias;
            _enableAlias = true;
        }

        /// <summary>
        ///     创建SelectionColumnGenerator实例，被访问属性树的映射字段属于指定的源。
        /// </summary>
        /// <param name="source">属性树映射字段所属的源。</param>
        /// <param name="anchor">属性树的锚点。不指定表示不启用别名。</param>
        public SelectionColumnGenerator(MonomerSource source, AssociationTreeNode anchor = null)
        {
            _source = source;
            _anchor = anchor;
            _enableAlias = true;
        }

        /// <summary>
        ///     创建SelectionColumnGenerator实例，被访问属性树的映射字段属于属性树锚点的映射源（该源的别名为锚点的别名，如果未指定锚点则不为这些字段明确
        ///     源）。
        ///     锚点的映射源是指作为锚点的关联树节点代表的类型的映射源。
        ///     实施说明
        ///     使用SimpleSource创建锚点的映射源，使用AliasGenerator生成锚点的别名。
        /// </summary>
        /// <param name="anchor">属性树的锚点。不指定表示不启用别名。</param>
        public SelectionColumnGenerator(AssociationTreeNode anchor = null)
        {
            if (anchor != null && anchor.RepresentedType is ObjectType objectType)
            {
                //生成别名
                var aliasGen = new AliasGenerator();
                var alias = anchor.AsTree().Accept(aliasGen);
                _source = new SimpleSource(objectType.TargetName, alias);
                _enableAlias = true;
            }
        }

        /// <summary>
        ///     获取或设置别名生成器，获取时如果不存在则创建一个。
        /// </summary>
        internal AliasGenerator AliasGenerator
        {
            get => _aliasGenerator;
            set => _aliasGenerator = value;
        }

        /// <summary>
        ///     获取或设置收集投影列的投影集。
        /// </summary>
        public ISelectionSet SelectionSet
        {
            get => _selectionSet;
            set => _selectionSet = value;
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
            //用不到
            outParentState = null;
            outPrevisitState = null;

            //复杂属性 返回
            if (subTree.IsComplex) return;


            //目标字段
            var targetField = subTree.Accept(_targetFieldGenerator);
            var filed = new Field(_source, targetField);

            //添加
            if (_selectionSet == null)
                _selectionSet = new SelectionSet();
            //是否启用别名
            if (_enableAlias)
            {
                if (_anchor != null)
                {
                    var alias = _anchor.AsTree().Accept(_aliasGenerator);
                    _selectionSet.Add(filed, alias);

                    return;
                }

                if (!string.IsNullOrEmpty(_anchorAlias))
                {
                    var alias = $"{_anchorAlias}_{targetField}";
                    _selectionSet.Add(filed, alias);

                    return;
                }
            }

            //无别名
            _selectionSet.Add(filed);
        }

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AttributeTree subTree, object parentState, object previsitState)
        {
            //Nothing to do
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