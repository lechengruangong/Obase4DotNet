/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：条件表达式解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:11:48
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;
using BinaryExpression = System.Linq.Expressions.BinaryExpression;
using ConstantExpression = System.Linq.Expressions.ConstantExpression;
using Expression = System.Linq.Expressions.Expression;
using ExpressionVisitor = System.Linq.Expressions.ExpressionVisitor;
using UnaryExpression = System.Linq.Expressions.UnaryExpression;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     条件表达式解析器。
    /// </summary>
    public class CriteriaExpressionParser : ExpressionVisitor
    {
        /// <summary>
        ///     数据对象模型
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     形参绑定。
        /// </summary>
        private readonly ParameterBinding[] _parameterBindings;

        /// <summary>
        ///     子树求值器
        /// </summary>
        private readonly SubTreeEvaluator _subTreeEvaluator;

        /// <summary>
        ///     数据源类型
        /// </summary>
        private readonly EDataSource _targetSource;

        /// <summary>
        ///     表达式翻译器
        /// </summary>
        private readonly ExpressionTranslator _translator;

        /// <summary>
        ///     条件
        /// </summary>
        private ICriteria _criteria;

        /// <summary>
        ///     构造CriteriaExpressionParser的新实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="subTreeEvaluator">子树求值器。</param>
        /// <param name="targetSource">目标源</param>
        /// <param name="parameterBindings">形参绑定</param>
        public CriteriaExpressionParser(ObjectDataModel model, SubTreeEvaluator subTreeEvaluator,
            EDataSource targetSource, ParameterBinding[] parameterBindings = null)
        {
            _model = model;
            _subTreeEvaluator = subTreeEvaluator;
            _targetSource = targetSource;
            _parameterBindings = parameterBindings;
            _translator = new ExpressionTranslator(_model, _subTreeEvaluator);
        }

        /// <summary>
        ///     默认节点的翻译操作
        /// </summary>
        /// <param name="node">表达式节点</param>
        private void DefaultTranslate(Expression node)
        {
            var sqlExp = _translator.Translate(node);
            _criteria = new ExpressionCriteria(sqlExp);
        }

        /// <summary>
        ///     解析指定的条件表达式。
        /// </summary>
        /// <param name="expression">要解析的条件表达式。</param>
        public ICriteria Parse(Expression expression)
        {
            expression = _subTreeEvaluator.Evaluate(expression);
            Visit(expression);
            return _criteria;
        }

        /// <summary>
        ///     访问Lambda表达式
        /// </summary>
        /// <typeparam name="T">表达式类型</typeparam>
        /// <param name="node">表达式节点</param>
        /// <returns></returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var body = _subTreeEvaluator.Evaluate(node.Body);
            Visit(body);
            DefaultTranslate(node);
            return node;
        }

        /// <summary>
        ///     翻译成员访问表达式
        /// </summary>
        /// <param name="node">成员访问表达式</param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            //如果是关联对象访问
            if (node.Expression is MemberExpression member)
            {
                var hostType = _model.GetTypeOrNull(member.Member.DeclaringType);
                if (hostType == null)
                    hostType = _model.GetType(member.Expression.Type);
                if (hostType is EntityType entityType)
                {
                    var assRef = entityType.GetAssociationReference(member.Member.Name);
                    if (assRef != null)
                    {
                        _criteria = new ExpressionCriteria(_translator.Translate(node));
                        return node;
                    }
                }

                if (hostType is AssociationType associationType)
                {
                    var assEnd = associationType.GetAssociationEnd(member.Member.Name);
                    if (assEnd != null)
                    {
                        _criteria = new ExpressionCriteria(_translator.Translate(node));
                        return node;
                    }
                }
            }

            //一般Member访问
            var exp = _subTreeEvaluator.Evaluate(node.Expression);
            Visit(exp);
            DefaultTranslate(node);
            return node;
        }

        /// <summary>
        ///     访问常量表达式
        /// </summary>
        /// <param name="node">常量表达式</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            DefaultTranslate(node);
            return node;
        }

        /// <summary>
        ///     访问调用表达式
        /// </summary>
        /// <param name="node">调用表达式</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var objectValue = _subTreeEvaluator.Evaluate(node.Object ?? node.Arguments[0]);

            if (node.Method.Name == "Contains" && objectValue.NodeType == ExpressionType.Constant &&
                (objectValue as ConstantExpression)?.Value is IQueryable queryable)
            {
                //解析查询表达式
                var expVisitor = new QueryExpressionParser(_model);
                expVisitor.Visit(queryable.Expression);
                var op = expVisitor.QueryOp;
                var builder = new RopPipelineBuilder(_model, _targetSource);
                op.Accept(builder);

                if (builder.Complement == null)
                {
                    if (node.Arguments.Count > 0)
                    {
                        var argsValue = _subTreeEvaluator.Evaluate(node.Arguments[0]);
                        var context = new RopContext(op.SourceType, _model, _targetSource);
                        builder.Pipeline.Execute(context);
                        _criteria = new InSelectCriteria(_translator.Translate(argsValue), context.ResultSql);
                    }
                }
                else
                {
                    _criteria = new ExpressionCriteria(_translator.Translate(node));
                }
            }
            else
            {
                _criteria = new ExpressionCriteria(_translator.Translate(node));
            }

            return node;
        }

        /// <summary>
        ///     访问一元表达式
        /// </summary>
        /// <param name="node">一元表达式</param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var nodeOperand = _subTreeEvaluator.Evaluate(node.Operand);
            Visit(nodeOperand);
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    if (node.Type == typeof(bool))
                    {
                        _criteria = _criteria.Not();
                    }
                    else
                    {
                        var translator = new ExpressionTranslator(_model, _subTreeEvaluator);
                        var sqlExp = translator.Translate(node);
                        _criteria = new ExpressionCriteria(sqlExp);
                    }

                    break;
            }

            return node;
        }

        /// <summary>
        ///     访问二元表达式
        /// </summary>
        /// <param name="node">二元表达式</param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var nodeLeft = _subTreeEvaluator.Evaluate(node.Left);
            Visit(nodeLeft);
            var left = _criteria;
            var nodeRight = _subTreeEvaluator.Evaluate(node.Right);
            Visit(nodeRight);
            var right = _criteria;
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    _criteria = new ComplexCriteria(left, right, ELogicalOperator.And);
                    break;
                case ExpressionType.OrElse:
                    _criteria = new ComplexCriteria(left, right, ELogicalOperator.Or);
                    break;
                default:
                    var translator = new ExpressionTranslator(_model, _subTreeEvaluator);
                    var sqlExp = translator.Translate(node);
                    _criteria = new ExpressionCriteria(sqlExp);
                    break;
            }

            return node;
        }
    }
}