/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：聚合运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:55:12
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     退化投影运算执行器
    /// </summary>
    public class AtrophySelectExecutor : RopExecutor
    {
        /// <summary>
        ///     在关联树上的投影结果。
        /// </summary>
        private readonly AssociationTreeNode _associationResult;

        /// <summary>
        ///     在属性树上的投影结果。
        /// </summary>
        private readonly AttributeTreeNode _attributeResult;

        /// <summary>
        ///     集合选择器。
        /// </summary>
        private readonly LambdaExpression _collectionSelector;

        /// <summary>
        ///     投影表达式。
        /// </summary>
        private readonly LambdaExpression _expression;

        /// <summary>
        ///     投影结果对应的别名。
        /// </summary>
        private readonly string _resultAlias;

        /// <summary>
        ///     根据投影表达式解析出的投影集。
        /// </summary>
        private readonly ISelectionSet _selectionSet;

        /// <summary>
        ///     构造SelectExecutor的新实例。
        /// </summary>
        /// <param name="queryOp">查询操作</param>
        /// <param name="expression">投影表达式。</param>
        /// <param name="collectionSelector">收集元素选择器</param>
        /// <param name="selectionSet">根据投影表达式解析出的投影集。</param>
        /// <param name="attrResult">属性树</param>
        /// <param name="resultAlias">投影结果对应的别名。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        /// <param name="assoResult">关联树</param>
        public AtrophySelectExecutor(QueryOp queryOp, LambdaExpression expression, LambdaExpression collectionSelector,
            ISelectionSet selectionSet, AssociationTreeNode assoResult, AttributeTreeNode attrResult,
            string resultAlias,
            OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
            _expression = expression;
            _collectionSelector = collectionSelector;
            _selectionSet = selectionSet;
            _associationResult = assoResult;
            _attributeResult = attrResult;
            _resultAlias = resultAlias;
        }

        /// <summary>
        ///     执行投影操作
        /// </summary>
        /// <param name="context">关系运算上下文</param>
        public override void Execute(RopContext context)
        {
            if (_collectionSelector == null && _expression?.Parameters?.Count == 2)
                context.AddIndexColumn();

            var resultSql = context.ResultSql;
            //是否提取或去重
            if (resultSql.TakeNumber > 0 || resultSql.Distinct) context.AcceptResult();
            //扩展源
            context.ExpandSource(_expression, ESourceJoinType.Inner, false);
            var aliasRoot = context.AliasRoot;
            if (aliasRoot != null) _selectionSet?.SetSourceAliasPrefix(aliasRoot);
            context.ResultSql.SelectionSet = _selectionSet;

            //根据是否有投影表达式，设置投影结果类型
            if (_associationResult != null)
                context.SetResultType(_associationResult, _attributeResult, _next.GetType() == typeof(RopTerminator));
            else
                context.SetResultType(PrimitiveType.FromType(_expression?.Body.Type), true,
                    _next is RopTerminator || _next is AggregateExecutor);

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}