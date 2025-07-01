/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：一般投影运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:07:35
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     一般投影运算执行器。
    ///     一般投影运算是指以查询源为基础源生成一个类型视图。
    /// </summary>
    public class SelectExecutor : RopExecutor
    {
        /// <summary>
        ///     由视图属性绑定解析出的SQL表达式。
        /// </summary>
        private readonly IDictionary<string, Expression> _sqlExpressions;

        /// <summary>
        ///     作为投影目标的类型视图。
        /// </summary>
        private readonly TypeView _typeView;

        /// <summary>
        ///     节点别名生成器，用于生成扩展树各节点的别名。
        /// </summary>
        private AliasGenerator _aliasGenerator;

        /// <summary>
        ///     属性树生长器，用于将指定的复杂属性生长至叶子节点。
        /// </summary>
        private AttributeTreeGrower _attributeTreeGrower;

        /// <summary>
        ///     简单属性枚举器，用于枚举属性树上的简单属性节点（即叶子节点）。
        /// </summary>
        private AttributeTreeNodeEnumerator _simpleAttributeEnumerator;

        /// <summary>
        ///     映射字段生成器，用于为复杂视图属性所含的简单属性生成映射字段。
        /// </summary>
        private TargetFieldGenerator _targetFieldGenerator;

        /// <summary>
        ///     创建SelectExecutor实例。
        /// </summary>
        /// <param name="queryOp"></param>
        /// <param name="typeView">作为投影目录的类型视图。</param>
        /// <param name="sqlExpressions">由视图属性绑定翻译的Sql表达式。</param>
        /// <param name="next">下一个执行器</param>
        public SelectExecutor(QueryOp queryOp, TypeView typeView, Dictionary<string, Expression> sqlExpressions,
            OpExecutor<RopContext> next) : base(queryOp, next)
        {
            _typeView = typeView;
            _sqlExpressions = sqlExpressions;
        }

        /// <summary>
        ///     获取别名生成器。
        /// </summary>
        /// 实施说明
        /// 需要时创建，创建后寄存。
        public AliasGenerator AliasGenerator => _aliasGenerator ?? (_aliasGenerator = new AliasGenerator());

        /// <summary>
        ///     获取属性树生长器。
        /// </summary>
        /// 实施说明
        /// 需要时创建，创建后寄存。
        public AttributeTreeGrower AttributeTreeGrower =>
            _attributeTreeGrower ?? (_attributeTreeGrower = new AttributeTreeGrower());

        /// <summary>
        ///     获取简单属性枚举器。
        /// </summary>
        /// 实施说明
        /// 需要时创建，创建后寄存。
        public AttributeTreeNodeEnumerator SimpleAttributeEnumerator =>
            _simpleAttributeEnumerator ?? (_simpleAttributeEnumerator = new AttributeTreeNodeEnumerator());

        /// <summary>
        ///     获取映射字段生成器。
        /// </summary>
        /// 实施说明
        /// 需要时创建，创建后寄存。
        public TargetFieldGenerator TargetFieldGenerator =>
            _targetFieldGenerator ?? (_targetFieldGenerator = new TargetFieldGenerator());

        /// <summary>
        ///     为简单属性生成投影列。
        /// </summary>
        /// <param name="simpleAttr">视图属性。</param>
        /// <param name="context">上下文。</param>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        private void GenerateAttribueColumns(ViewAttribute simpleAttr, RopContext context, SelectionSet selectionSet)
        {
            Expression sqlExp;
            if (simpleAttr.IsIntuitive) //直观属性
            {
                var attrSource = simpleAttr.Sources[0];
                var anchorTree = attrSource.ExtensionNode.AsTree();
                var nodeAlias = anchorTree.Accept(AliasGenerator);
                var source = context.JoinMemo.GetSource($"{context.AliasRoot}{nodeAlias}");
                var bindingTree = attrSource.AttributeNode.AsTree();
                TargetFieldGenerator.Reset();
                var filedName = bindingTree.Accept(TargetFieldGenerator);
                var field = new Field(source, filedName);
                sqlExp = Expression.Fields(field);
            }
            else //非直观属性
            {
                var attrName = simpleAttr.Name;
                sqlExp = _sqlExpressions[attrName];
            }

            selectionSet.Add(sqlExp, simpleAttr.TargetField);
        }

        /// <summary>
        ///     为复杂属性生成投影列。
        /// </summary>
        /// <param name="complexAttr">复杂属性。</param>
        /// <param name="joinMemo">源联接备忘录。</param>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        private void GenerateAttribueColumns(ViewComplexAttribute complexAttr, JoinMemo joinMemo,
            SelectionSet selectionSet)
        {
            var anctorTree = complexAttr.Anchor.AsTree();
            var nodeAlias = anctorTree.Accept(_aliasGenerator);
            var source = joinMemo.GetSource(nodeAlias);

            var tempSet = new SelectionSet();
            var columnGen = new SelectionColumnGenerator(tempSet, source);

            var bindingTree = complexAttr.Binding.AsTree();
            bindingTree.Accept(AttributeTreeGrower);
            bindingTree.Accept(columnGen);

            var attrTree = new AttributeTree(complexAttr);

            attrTree.Accept(AttributeTreeGrower);
            var leafNodes = attrTree.Accept(SimpleAttributeEnumerator);

            foreach (var node in leafNodes)
            foreach (var column in tempSet.Columns)
                if (column is ExpressionColumn expressionColumn)
                {
                    var nodeTree = node.AsTree();
                    var targetFiled = nodeTree.Accept(TargetFieldGenerator);
                    expressionColumn.Alias = targetFiled;
                    selectionSet.Add(expressionColumn);
                }
        }

        /// <summary>
        ///     生成构造函数参数列
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="selectionSet"></param>
        private void GenerateConstructorParametColumns(Parameter parameter, SelectionSet selectionSet)
        {
            var selection = new ExpressionColumn
                { Expression = _sqlExpressions[parameter.Name], Alias = parameter.Name };
            //已存在的 或者 相同别名的不添加
            if (!selectionSet.Contains(selection) &&
                selectionSet.Columns.Cast<ExpressionColumn>().All(p => p.Alias != parameter.Name))
                selectionSet.Add(selection);
        }

        /// <summary>
        ///     生成标识列。
        /// </summary>
        /// <param name="selectionSet">收集投影列的投影集。</param>
        /// <param name="context">关系运算上下文。</param>
        /// 实施说明：
        /// 如果视图不包含引用元素，则不生成标识列。
        private void GenerateIdColumns(SelectionSet selectionSet, RopContext context)
        {
            if (_typeView?.ReferenceElements?.Length <= 0) return;
            var source = _typeView?.Source;
            if (source is IMappable)
            {
                var idgen = new IdColumnGenerator(AliasGenerator, context.JoinMemo, selectionSet);
                _typeView.Extension.Accept(idgen);
            }
        }

        /// <summary>
        ///     执行运算。
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(RopContext context)
        {
            if (_typeView.ParameterBindings.Any(pb => pb.Referring == EParameterReferring.Index))
                context.AddIndexColumn();

            if (context.ResultSql.TakeNumber > 0 || context.ResultSql.Distinct) context.AcceptResult();

            if (!_typeView.IsDecomposeExtremelyResult) context.ExpandSource(_typeView.Extension, false);


            var selectionSet = new SelectionSet();
            //加入ID列
            GenerateIdColumns(selectionSet, context);

            //处理类型视图的参数列
            foreach (var attribute in _typeView.Attributes)
                if (attribute is ViewAttribute viewAttribute)
                    GenerateAttribueColumns(viewAttribute, context, selectionSet);
                else if (attribute is ViewComplexAttribute viewComplexAttribute)
                    GenerateAttribueColumns(viewComplexAttribute, context.JoinMemo, selectionSet);

            if (_typeView.Constructor.Parameters != null && _typeView.Constructor.Parameters.Count > 0)
                //生成构造参数
                foreach (var parameter in _typeView.Constructor.Parameters)
                    GenerateConstructorParametColumns(parameter, selectionSet);

            selectionSet.SetSourceAliasPrefix(context.AliasRoot);
            //设置投影列集合
            context.ResultSql.SelectionSet = selectionSet;

            context.SetResultType(_typeView, _next.GetType() == typeof(RopTerminator));

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}