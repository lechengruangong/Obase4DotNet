/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：管道终结执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:56:44
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;
using Obase.Providers.Sql.Common;
using Obase.Providers.Sql.SqlObject;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     管道终结执行器。
    ///     算法:
    ///     includings.Grow(_including)
    ///     if(resultSql.Top > 0) AcceptResult()；
    ///     ExpandSource(当前包含树);
    ///     ExecuteIncluding();
    /// </summary>
    public class RopTerminator : RopExecutor
    {
        /// <summary>
        ///     构造OpExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        public RopTerminator(QueryOp queryOp) : base(queryOp)
        {
        }

        /// <summary>
        ///     执行挂起的包含操作。
        /// </summary>
        /// <param name="assoTree">包含挂起的包含操作的关联树。</param>
        /// <param name="aliasRoot">别名根。</param>
        /// <param name="parentSourceAlias">父节点源的别名</param>
        /// <param name="selectionSet">投影列集合</param>
        /// <param name="aliasPrefix">字段别名前缀。</param>
        private void ExecuteIncluding(AssociationTree assoTree, string aliasRoot, string parentSourceAlias,
            ISelectionSet selectionSet,
            string aliasPrefix = "")
        {
            var currentType = assoTree.RepresentedType;
            if (currentType == null)
                return;
            var sourceJoiner = new SourceJoiner(currentType);

            foreach (var subTree in assoTree.SubTrees)
            {
                var eleName = subTree.ElementName;
                var shouldJoin = sourceJoiner.ShouldJoin(eleName);
                var source = shouldJoin ? $"{aliasPrefix}_{eleName}" : parentSourceAlias;
                var aliasPrefixThisRef = aliasPrefix + "_" + eleName;
                var objType = subTree.RepresentedType as ObjectType;
                GenerateColumn(objType, aliasRoot == source ? source : aliasRoot + source, aliasPrefixThisRef,
                    selectionSet);
                ExecuteIncluding(subTree, aliasRoot, source, selectionSet, aliasPrefixThisRef);
            }
        }

        /// <summary>
        ///     根据指定的对象类型生成投影列。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="source">属性映射字段所属源的名称。</param>
        /// <param name="aliasPrefix">投影列别名的前缀。</param>
        /// <param name="selectionSet">生成的投影列所属的投影集。</param>
        private void GenerateColumn(ObjectType objectType, string source, string aliasPrefix,
            ISelectionSet selectionSet)
        {
            if (string.IsNullOrEmpty(source)) source = objectType.TargetTable;
            //GenerateColumn(objectType.Attributes, source, aliasPrefix, selectionSet);
            var generator = new SelectionColumnGenerator(selectionSet, new SimpleSource(source), aliasPrefix);
            foreach (var attrTree in objectType.EnumerateAttributeTree() ?? new List<AttributeTree>())
                attrTree.Accept(generator);

            if (objectType is AssociationType associationType)
                foreach (var end in associationType.AssociationEnds)
                foreach (var map in end.Mappings)
                {
                    var fieldName = map.TargetField;
                    var field = new Field(source, fieldName);
                    selectionSet.Add(field, aliasPrefix + "_" + fieldName);
                }
        }

        /// <summary>
        ///     根据指定的属性集生成投影列。
        /// </summary>
        /// <param name="attributes">投影集。</param>
        /// <param name="source">属性映射字段所属源的名称。</param>
        /// <param name="aliasPrefix">投影列别名的前缀。</param>
        /// <param name="selectionSet">生成的投影列所属的投影集。</param>
        private void GenerateColumn(List<Attribute> attributes, string source, string aliasPrefix,
            ISelectionSet selectionSet)
        {
            foreach (var attr in attributes)
                if (attr.IsComplex)
                {
                    var comType = (attr as ComplexAttribute)?.ComplexType;
                    if (comType != null) GenerateColumn(comType.Attributes, source, aliasPrefix, selectionSet);
                }
                else
                {
                    var fieldName = attr.TargetField;
                    var field = new Field(source, fieldName);
                    selectionSet.Add(field, aliasPrefix + "_" + fieldName);
                }
        }

        /// <summary>
        ///     执行与管道终结相关的操作。
        /// </summary>
        /// <param name="context">关系运算上下文。</param>
        public override void Execute(RopContext context)
        {
            var includeings = context.Includings;
            context.ResultIncluding?.Grow(context.Includings);
            if (includeings != null && includeings.SubTrees.Length > 0)
            {
                if (context.ResultSql.TakeNumber > 0) context.AcceptResult();
                context.ExpandSource(false);
                ExecuteIncluding(includeings, context.AliasRoot, context.AliasRoot,
                    context.ResultSql.SelectionSet);
            }

            //如果存在同一个Field的仅保留一个
            context.ResultSql.Orders = SqlUtils.DistinctOrders(context.ResultSql.Orders);
        }
    }
}