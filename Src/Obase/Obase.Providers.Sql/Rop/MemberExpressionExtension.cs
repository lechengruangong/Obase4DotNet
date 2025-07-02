/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：扩展成员表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:46:21
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Common;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     扩展成员表达式。
    /// </summary>
    public static class MemberExpressionExtension
    {
        /// <summary>
        ///     生成表达式表示的关联树节点或属性树节点的别名。
        /// </summary>
        /// <param name="memberExp">成员表达式</param>
        /// <param name="model">对象数据模型。</param>
        public static string GenerateAlias(this MemberExpression memberExp, ObjectDataModel model)
        {
            var lastAssNode = memberExp.ExtractAssociation(model, attrTree: out var lastAttrNode);
            var alias = "";

            if (lastAssNode != null)
            {
                //生成别名
                var aliasGen = new AliasGenerator();
                if (lastAttrNode != null)
                {
                    //生成字段
                    var filedGen = new TargetFieldGenerator();
                    var filed = lastAttrNode.Accept(filedGen);
                    alias = lastAssNode.Accept(aliasGen, filed);
                }
                else
                {
                    alias = lastAssNode.Accept(aliasGen);
                }
            }

            return alias;
        }

        /// <summary>
        ///     生成表达式表示的属性树节点的映射字段。
        /// </summary>
        /// <exception cref="Exception">表达式未指向一个简单属性，无法转换成字段。</exception>
        /// <param name="memberExp">成员表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="source">映射字段所属的源。</param>
        /// <param name="parameterBindings">形参绑定。</param>
        public static Field GenerateField(this MemberExpression memberExp, ObjectDataModel model,
            MonomerSource source = null, ParameterBinding[] parameterBindings = null)
        {
            //提取关联树
            memberExp.ExtractAssociation(model, assoTail: out var assoTail, attrTail: out var attrTail,
                parameterBindings);
            //所属源不存在 构造源
            if (source == null && assoTail != null)
            {
                if (assoTail is ObjectTypeNode objectTypeNode && objectTypeNode.Parent != null)
                {
                    var assoTailParent = objectTypeNode.Parent;
                    var aliasGen = new AliasGenerator();
                    var parentAlias = assoTailParent.AsTree().Accept(aliasGen);
                    var sourceJoiner = new SourceJoiner(assoTailParent.RepresentedType, null, parentAlias);
                    sourceJoiner.Join(objectTypeNode.ElementName, out source, out _);
                }
                else
                {
                    var tailType = assoTail.RepresentedType;
                    source = tailType is ObjectType objectType
                        ? new SimpleSource(objectType.TargetName, Utils.GetDerivedTargetTable(objectType))
                        : new SimpleSource(((IMappable)tailType)?.TargetName);
                }
            }

            //处理属性树
            if (attrTail != null)
            {
                //生成字段
                var filedGen = new TargetFieldGenerator();
                var filed = attrTail.AsTree().Accept(filedGen);
                var field = new Field(source, filed);
                return field;
            }

            throw new ArgumentException("表达式未指向一个简单属性,无法转换成字段", nameof(memberExp));
        }

        /// <summary>
        ///     生成表达式表示的属性树节点的映射字段。
        /// </summary>
        /// <exception cref="Exception">表达式未指向一个简单属性，无法转换成字段。</exception>
        /// <param name="memberExp">成员表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static Field GenerateField(this MemberExpression memberExp, ObjectDataModel model,
            ParameterBinding[] paraBindings)
        {
            return GenerateField(memberExp, model, null, paraBindings);
        }

        /// <summary>
        ///     生成投影列。
        ///     当表达式表示关联树节点时，生成该节点代表类型的所有属性的投影列；当表达式表示复杂属性节点时，生成所有子节点（包括间接）的投影列；当表达式表示简单属性节点时，生成
        ///     该属性的投影列。
        /// </summary>
        /// <param name="memberExp">成员表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="selectionSet">收集所创建的投影列的投影集。</param>
        /// <param name="parameterBindings">形参绑定。</param>
        /// <param name="source">投影列源字段所属的源。</param>
        /// <param name="assoResult">返回在关联树上的投影结果。</param>
        /// <param name="attrResult">返回在属性树上的投影结果。</param>
        public static void GenerateSelectionColumn(this MemberExpression memberExp, ObjectDataModel model,
            ISelectionSet selectionSet, ParameterBinding[] parameterBindings, out AssociationTreeNode assoResult,
            out AttributeTreeNode attrResult, MonomerSource source = null)
        {
            //提取关联树
            memberExp.ExtractAssociation(model, out var assoTail, out var attrTail, out var attrTree,
                parameterBindings);

            //未指定源 构造源
            if (source == null && assoTail != null)
            {
                if (assoTail is ObjectTypeNode objectTypeNode && objectTypeNode.Parent != null)
                {
                    var assoTailParent = objectTypeNode.Parent;
                    var aliasGen = new AliasGenerator();
                    var parentAlias = assoTailParent.AsTree().Accept(aliasGen);
                    var sourceJoiner = new SourceJoiner(assoTailParent.RepresentedType, null, parentAlias);
                    sourceJoiner.Join(objectTypeNode.ElementName, out source, out _);
                }
                else
                {
                    var tailType = assoTail.RepresentedType;
                    source = new SimpleSource(((IMappable)tailType).TargetName);
                }
            }

            if (attrTree != null)
            {
                var columnGen = new SelectionColumnGenerator(source);
                ISelectionSet tempSet = new SelectionSet();

                if (attrTail.Attribute.IsComplex)
                {
                    columnGen.SelectionSet = tempSet;
                    attrTree.Accept(new AttributeTreeGrower());
                    attrTree.Accept(columnGen);
                    //必为ComplexType
                    var comType = (ComplexType)attrTail.AttributeType;
                    var comAttributeTrees = comType.EnumerateAttributeTree();
                    tempSet = columnGen.SelectionSet;

                    foreach (var tree in comAttributeTrees)
                    {
                        //枚举节点
                        var leafNodes = tree.Accept(new AttributeTreeNodeEnumerator());
                        foreach (var node in leafNodes)
                        {
                            var targetFiled = node.AsTree().Accept(new TargetFieldGenerator());
                            if (tempSet?.Columns != null && tempSet?.Columns.Count > 0)
                                foreach (var column in tempSet?.Columns)
                                    if (column is ExpressionColumn expressionColumn)
                                    {
                                        expressionColumn.Alias = targetFiled;
                                        selectionSet.Add(column);
                                    }
                        }
                    }
                }
                else
                {
                    columnGen.SelectionSet = selectionSet;
                    attrTree.Accept(columnGen);
                }
            }
            else
            {
                //构造通配列
                var wildColumn = new WildcardColumn { Source = source };
                selectionSet.Add(wildColumn);
            }

            assoResult = assoTail;
            attrResult = attrTail;
        }
    }
}