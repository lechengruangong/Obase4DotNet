/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：投影表达式解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:08:32
└──────────────────────────────────────────────────────────────┘
*/


using System;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Providers.Sql.SqlObject;
using Expression = System.Linq.Expressions.Expression;
using ExpressionVisitor = System.Linq.Expressions.ExpressionVisitor;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     投影表达式解析器。
    /// </summary>
    public class SelectionExpressionParser : ExpressionVisitor
    {
        /// <summary>
        ///     宿主别名
        /// </summary>
        private readonly string _hostAlias = @"obase$result";

        /// <summary>
        ///     对象数据模型
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
        ///     存储访问者模式中的关联树节点
        /// </summary>
        private AssociationTreeNode _assoResult;

        /// <summary>
        ///     存储访问者模式中的属性树节点
        /// </summary>
        private AttributeTreeNode _attrResult;

        /// <summary>
        ///     指示是否将每个元素的投影结果序列组合为一个序列。注：当投影结果为单值时，该属性为false。
        /// </summary>
        private bool _isCombining;

        /// <summary>
        ///     投影集
        /// </summary>
        private ISelectionSet _set;

        /// <summary>
        ///     存储问者模式中传入的投影集
        /// </summary>
        private ISelectionSet _tempSet;

        /// <summary>
        ///     构造SelectionExpressionParser的新实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="subTreeEvaluator">子树求值器。</param>
        /// <param name="isCombining">指示是否将每个元素的投影结果序列组合为一个序列。注：当投影结果为单值时，该属性为false。</param>
        /// <param name="parameterBindings">形参绑定</param>
        public SelectionExpressionParser(ObjectDataModel model, SubTreeEvaluator subTreeEvaluator, bool isCombining,
            ParameterBinding[] parameterBindings = null)
        {
            _subTreeEvaluator = subTreeEvaluator;
            _model = model;
            _isCombining = isCombining;
            _parameterBindings = parameterBindings;
        }


        /// <summary>
        ///     指示是否将每个元素的投影结果序列组合为一个序列。注：当投影结果为单值时，该属性为false。
        /// </summary>
        public bool IsCombining
        {
            get => _isCombining;
            set => _isCombining = value;
        }

        /// <summary>
        ///     访问Lambda表达式对应的成员访问表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Visit(node.Body) ?? throw new InvalidOperationException("投影表达式解错误:无效的成员访问表达式");
        }

        /// <summary>
        ///     访问新建对象函数
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression node)
        {
            var sqlExpTranslator = new ExpressionTranslator(_model, _subTreeEvaluator);
            var selection = new SelectionSet();

            //将每个参数作为投影列 对应的匿名属性名称作为别名
            for (var i = 0; i < node.Arguments.Count; i++)
            {
                var sqlExp = sqlExpTranslator.Translate(node.Arguments[i]);
                selection.Add(new ExpressionColumn { Expression = sqlExp, Alias = node.Members[i].Name });
            }

            _set = selection;

            return node;
        }

        /// <summary>
        ///     翻译成员表达式
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression exp)
        {
            //此处为图中member节点后

            //取模型类型
            var modelType = _model.GetStructuralType(exp.Expression.Type);

            //是否使用翻译表达式的方式处理
            var shouldTranslate = false;

            if (modelType != null)
            {
                var attr = modelType.GetAttribute(exp.Member.Name);

                //判断类型是否为空或为复杂类型
                if (!(attr != null && attr.IsComplex == false))
                {
                    exp.GenerateSelectionColumn(_model, _tempSet, _parameterBindings, out _assoResult, out _attrResult);
                    _set = _tempSet;
                }
                else
                {
                    shouldTranslate = true;
                }
            }
            else
            {
                shouldTranslate = true;
            }

            //判断标识
            if (shouldTranslate)
            {
                var sqlExp = new ExpressionTranslator(_model, _subTreeEvaluator, _parameterBindings).Translate(exp);
                var selection = new SelectionSet(new ExpressionColumn { Expression = sqlExp, Alias = _hostAlias });
                _set = selection;
            }

            return exp;
        }

        /// <summary>
        ///     解析指定的投影表达式。
        /// </summary>
        /// <param name="expression">要解析的投影表达式。</param>
        /// <param name="assoResult">关联树节点</param>
        /// <param name="attrResult">属性书节点</param>
        public ISelectionSet Parse(Expression expression, out AssociationTreeNode assoResult,
            out AttributeTreeNode attrResult)
        {
            var selectionSet = new SelectionSet();
            return Parse(expression, selectionSet, out assoResult, out attrResult);
        }

        /// <summary>
        ///     解析指定的投影表达式。
        /// </summary>
        /// <param name="expression">要解析的投影表达式。</param>
        /// <param name="selectionSet">投影集，用于在解析过程中收集投影列的容器</param>
        /// <param name="assoResult">关联树节点</param>
        /// <param name="attrResult">属性书节点</param>
        public ISelectionSet Parse(Expression expression, ISelectionSet selectionSet,
            out AssociationTreeNode assoResult, out AttributeTreeNode attrResult)
        {
            _tempSet = selectionSet;
            Visit(expression);
            //out值
            assoResult = _assoResult;
            attrResult = _attrResult;
            //结果
            return _set;
        }
    }
}