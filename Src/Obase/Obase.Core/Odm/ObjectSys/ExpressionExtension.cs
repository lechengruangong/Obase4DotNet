/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：System.Linq.Expressions.Expression类的扩展方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:30:05
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     定义一组扩展System.Linq.Expressions.Expression类的方法。
    /// </summary>
    public static class ExpressionExtension
    {
        /// <summary>
        ///     接受表达式访问者。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="visitor">表达式访问者。</param>
        public static void Accept(this Expression expression, ExpressionVisitor visitor)
        {
            visitor.Visit(expression);
        }

        /// <summary>
        ///     接受关联树访问者。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="visitor">关联树访问者。</param>
        /// <param name="model">对象数据模型。</param>
        public static void Accept(this Expression expression, IAssociationTreeDownwardVisitor visitor,
            ObjectDataModel model)
        {
            ExpressionVerify(expression);
            //根据表达式提取关联树
            var assoTree = ExtractAssociation(expression, model, attrTree: out _);
            //如果有关联树 遍历关联树访问者
            if (assoTree != null && visitor != null) assoTree.Accept(visitor);
        }

        /// <summary>
        ///     接受关联树访问者。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="visitor">关联树访问者。</param>
        /// <param name="model">对象数据模型。</param>
        public static TResult Accept<TResult>(this Expression expression,
            IAssociationTreeDownwardVisitor<TResult> visitor, ObjectDataModel model)
        {
            Accept(expression, visitor, null, model, out var assoResult, out _);
            return assoResult;
        }

        /// <summary>
        ///     接受关联树访问者和属性树访问者。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="assoVisitor">关联树访问者。</param>
        /// <param name="attrVisitor">属性树访问者。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="assoResult">返回关联树访问结果。</param>
        /// <param name="attrResult">返回属性树访问结果。</param>
        public static void Accept<TResult>(this Expression expression,
            IAssociationTreeDownwardVisitor<TResult> assoVisitor, IAttributeTreeDownwardVisitor<TResult> attrVisitor,
            ObjectDataModel model, out TResult assoResult, out TResult attrResult)
        {
            ExpressionVerify(expression);
            //根据表达式提取关联树
            var assoTree = ExtractAssociation(expression, model, attrTree: out var attrTree);
            assoResult = default;
            attrResult = default;
            //如果有关联树 遍历关联树访问者
            if (assoTree != null && assoVisitor != null) assoResult = assoTree.Accept(assoVisitor);
            // 如果有属性树 遍历属性树访问者
            if (attrTree != null && attrVisitor != null) attrResult = attrTree.Accept(attrVisitor);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            ParameterBinding[] paraBindings = null)
        {
            return ExtractAssociation(expression, model, out _, out _, out _, paraBindings);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树，同时抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attrTree">返回从表达式中抽取的属性树。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            out AttributeTree attrTree, ParameterBinding[] paraBindings = null)
        {
            return ExtractAssociation(expression, model, out _, out _, out attrTree, paraBindings);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attrTail">返回从表达式中抽取的属性树的末节点。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            out AttributeTreeNode attrTail,
            ParameterBinding[] paraBindings = null)
        {
            return ExtractAssociation(expression, model, out _, out attrTail, out _, paraBindings);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树，同时抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attrTree">返回从表达式抽取的属性树。</param>
        /// <param name="attrTail">返回从表达式抽取的属性树的末节点。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            out AttributeTree attrTree, out AttributeTreeNode attrTail,
            ParameterBinding[] paraBindings = null)
        {
            return ExtractAssociation(expression, model, out _, out attrTail, out attrTree, paraBindings);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="assoTail">返回从表达式中抽取的关联树的末节点。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            out AssociationTreeNode assoTail,
            ParameterBinding[] paraBindings = null)
        {
            return ExtractAssociation(expression, model, out assoTail, out _, out _, paraBindings);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树，同时抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="assoTail">返回从表达式中抽取的关联树末节点。</param>
        /// <param name="attrTree">返回从表达式中抽取的属性树。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            out AssociationTreeNode assoTail, out AttributeTree attrTree,
            ParameterBinding[] paraBindings = null)
        {
            return ExtractAssociation(expression, model, out assoTail, out _, out attrTree, paraBindings);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树，同时抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="assoTail">返回从表达式中抽取的关联树末节点。</param>
        /// <param name="attrTail">返回从表达式中抽取的属性树末节点。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            out AssociationTreeNode assoTail, out AttributeTreeNode attrTail,
            ParameterBinding[] paraBindings = null)
        {
            return ExtractAssociation(expression, model, out assoTail, out attrTail, out _, paraBindings);
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树，同时抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="assoTail">返回从表达式中抽取的关联树末节点。</param>
        /// <param name="attrTail">返回从表达式中抽取的属性树末节点。</param>
        /// <param name="attrTree">返回从表达式中抽取的属性树。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTree ExtractAssociation(this Expression expression, ObjectDataModel model,
            out AssociationTreeNode assoTail,
            out AttributeTreeNode attrTail,
            out AttributeTree attrTree,
            ParameterBinding[] paraBindings = null)
        {
            ExpressionVerify(expression);
            //构造生长器
            var grower = new AssociationGrower(model)
            {
                ExtractingAttribute = true,
                ParameterBindings = paraBindings
            };
            //访问
            grower.Visit(expression);
            //值
            assoTail = grower.LastAssociationNode?.Node;
            attrTail = grower.LastAttributeNode?.Node;
            attrTree = grower.AttributeTree;

            return grower.AssociationTree;
        }

        /// <summary>
        ///     从表达式中抽取关联并表示成关联树,不抽取属性树
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="paraBindings">形参绑定</param>
        /// <returns></returns>
        public static AssociationTree OnlyExtractAssociation(this Expression expression, ObjectDataModel model,
            ParameterBinding[] paraBindings = null)
        {
            ExpressionVerify(expression);
            //构造生长器
            var grower = new AssociationGrower(model)
            {
                ExtractingAttribute = false,
                ParameterBindings = paraBindings
            };
            //访问
            grower.Visit(expression);

            return grower.AssociationTree;
        }

        /// <summary>
        ///     根据表达式的指引生长指定的关联树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <returns>关联树生长后的末节点。</returns>
        /// <param name="expression">表达式</param>
        /// <param name="assoTree">待生长的关联树。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTreeNode GrowAssociationTree(this Expression expression, AssociationTree assoTree,
            ObjectDataModel model, ParameterBinding[] paraBindings = null)
        {
            return GrowAssociationTree(expression, assoTree, model, out _, out _, paraBindings);
        }

        /// <summary>
        ///     根据表达式的指引生长指定的关联树，同时从表达式抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <returns>关联树生长后的末节点。</returns>
        /// <param name="expression">表达式</param>
        /// <param name="assoTree">待生长的关联树。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attrTree">从表达式抽取的属性树。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public static AssociationTreeNode GrowAssociationTree(this Expression expression, AssociationTree assoTree,
            ObjectDataModel model, out AttributeTree attrTree, ParameterBinding[] paraBindings = null)
        {
            return GrowAssociationTree(expression, assoTree, model, out attrTree, out _, paraBindings);
        }

        /// <summary>
        ///     根据表达式的指引生长指定的关联树，同时从表达式抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <returns>关联树生长后的末节点。</returns>
        /// <param name="expression">表达式</param>
        /// <param name="assoTree">待生长的关联树。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attrTail">从表达式抽取的属性树的末节点。</param>
        /// <param name="paraBindings">形参绑定。</param>
        /// <param name="preExpression">前序表达式</param>
        public static AssociationTreeNode GrowAssociationTree(this Expression expression, AssociationTree assoTree,
            ObjectDataModel model, out AttributeTreeNode attrTail, ParameterBinding[] paraBindings = null,
            Expression preExpression = null)
        {
            return GrowAssociationTree(expression, assoTree, model, out _, out attrTail, paraBindings, preExpression);
        }

        /// <summary>
        ///     根据表达式的指引生长指定的关联树，同时从表达式抽取属性树。
        ///     注：本方法仅对成员表达式和参数表达式有效。
        /// </summary>
        /// <returns>关联树生长后的末节点。</returns>
        /// <param name="expression">表达式</param>
        /// <param name="assoTree">待生长的关联树。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attrTree">从表达式抽取的属性树。</param>
        /// <param name="attrTail">从表达式抽取的属性树的末节点。</param>
        /// <param name="paraBindings">形参绑定。</param>
        /// <param name="preExpression">前序表达式</param>
        public static AssociationTreeNode GrowAssociationTree(this Expression expression, AssociationTree assoTree,
            ObjectDataModel model, out AttributeTree attrTree,
            out AttributeTreeNode attrTail, ParameterBinding[] paraBindings = null, Expression preExpression = null)
        {
            ExpressionVerify(expression);
            var grower = new AssociationGrower(model, assoTree)
            {
                ExtractingAttribute = true,
                ParameterBindings = paraBindings,
                PreExpression = preExpression
            };

            //访问
            grower.Visit(expression);
            //值
            attrTree = grower.AttributeTree;
            attrTail = grower.LastAttributeNode?.Node;

            return grower.LastAssociationNode.Node;
        }

        /// <summary>
        ///     检测支持的表达式类型。
        /// </summary>
        /// <param name="expression">要判断的表达式</param>
        private static void ExpressionVerify(Expression expression)
        {
            //可接受的表达式类型
            if (expression is MemberExpression) return;
            if (expression is ParameterExpression) return;
            if (expression is LambdaExpression) return;
            if (expression is MethodCallExpression methodCallExpression)
            {
                var methodName = methodCallExpression.Method.Name;
                if (methodName.Equals("select", StringComparison.OrdinalIgnoreCase) ||
                    methodName.Equals("selectmany", StringComparison.OrdinalIgnoreCase))
                    return;
            }

            throw new ArgumentException($"Obase.Odm.ObjectSys.ExpressionExtension扩展不支持表达式({expression})。");
        }
    }
}