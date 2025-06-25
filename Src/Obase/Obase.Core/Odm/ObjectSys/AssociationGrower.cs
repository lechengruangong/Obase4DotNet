/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联生长器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:37:24
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Common;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     关联生长器。
    /// </summary>
    public class AssociationGrower : ExpressionVisitor
    {
        /// <summary>
        ///     对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     待生长的关联树。
        /// </summary>
        private AssociationTree _associationTree;

        /// <summary>
        ///     从表达式抽取的属性树。
        /// </summary>
        private AttributeTree _attributeTree;

        /// <summary>
        ///     指示是否抽取属性树。
        /// </summary>
        private bool _extractingAttribute;

        /// <summary>
        ///     关联树生长后的末节点。
        /// </summary>
        private AssociationTree _lastAssociationNode;

        /// <summary>
        ///     从表达式抽取的属性树的末节点。
        /// </summary>
        private AttributeTree _lastAttributeNode;

        /// <summary>
        ///     形参绑定。
        /// </summary>
        private ParameterBinding[] _parameterBindings;

        /// <summary>
        ///     表达式中的前一部分
        ///     目前仅在判断隐式多方关联中使用
        /// </summary>
        private Expression _preExpression;

        /// <summary>
        ///     创建AssociationGrower实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="assoTree">待生长的关联树。如果未指定，生长器在解析表达式的过程中将创建一棵新树。</param>
        public AssociationGrower(ObjectDataModel model, AssociationTree assoTree = null)
        {
            _model = model;
            _associationTree = assoTree;
        }

        /// <summary>
        ///     获取待生长的关联树。
        /// </summary>
        public AssociationTree AssociationTree => _associationTree;

        /// <summary>
        ///     获取从表达式抽取的属性树。
        /// </summary>
        public AttributeTree AttributeTree => _attributeTree;

        /// <summary>
        ///     获取或设置一个值，该值指示是否抽取属性树。
        /// </summary>
        public bool ExtractingAttribute
        {
            get => _extractingAttribute;
            set => _extractingAttribute = value;
        }

        /// <summary>
        ///     获取关联树生长后的末节点。
        /// </summary>
        public AssociationTree LastAssociationNode => _lastAssociationNode;

        /// <summary>
        ///     获取从表达式抽取的属性树的末节点。
        /// </summary>
        public AttributeTree LastAttributeNode => _lastAttributeNode;

        /// <summary>
        ///     获取或设置形参绑定。
        /// </summary>
        public ParameterBinding[] ParameterBindings
        {
            get => _parameterBindings;
            set => _parameterBindings = value;
        }

        /// <summary>
        ///     表达式中的前一部分
        ///     目前仅在判断隐式多方关联中使用
        /// </summary>
        public Expression PreExpression
        {
            get => _preExpression;
            set => _preExpression = value;
        }

        /// <summary>
        ///     访问常量表达式。
        /// </summary>
        /// <param name="node"></param>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node;
        }

        /// <summary>
        ///     访问成员表达式。
        /// </summary>
        /// <param name="node"></param>
        protected override Expression VisitMember(MemberExpression node)
        {
            //读取表达式的值
            var host = node.Expression;
            var name = node.Member.Name;
            //访问内部表达式
            Visit(host);

            //在模型内查找
            var type = _model.GetTypeOrNull(host.Type);
            //如果是元组
            if (Utils.IsTuple(host.Type))
                if (_preExpression is MemberExpression preMemberExpression)
                {
                    //前序的Host
                    var preHostType = preMemberExpression.Expression.Type;
                    //从Host内取引用
                    var preStructuralType = _model.GetTypeOrNull(preHostType) as StructuralType;
                    var refElement =
                        preStructuralType?.Elements?.FirstOrDefault(p => p.Name == preMemberExpression.Member.Name);

                    //为关联引用
                    if (refElement is AssociationReference reference)
                    {
                        var currentEnd = reference.AssociationType.AssociationEnds.FirstOrDefault(p =>
                            p.EntityType.ClrType == node.Type);

                        if (currentEnd != null)
                        {
                            //获取关联树子树
                            var sub = _lastAssociationNode?.GetSubTree(currentEnd.Name);
                            if (sub != null)
                            {
                                if (currentEnd.NavigationUse == ENavigationUse.DirectlyReference)
                                {
                                    var navi = currentEnd.Navigation;
                                    var refTree = sub.GetSubTree(navi.TargetEndName);
                                    if (refTree != null)
                                        _lastAssociationNode = refTree;
                                }
                                else
                                {
                                    _lastAssociationNode = sub;
                                }
                            }
                            else
                            {
                                //构造树
                                sub = new AssociationTree(currentEnd.ReferenceType, currentEnd.Name);
                                _lastAssociationNode?.AddSubTree(sub);

                                if (currentEnd.NavigationUse == ENavigationUse.DirectlyReference)
                                {
                                    _lastAssociationNode = sub;
                                    //根据导航构造树
                                    var navi = currentEnd.Navigation;
                                    sub = new AssociationTree(navi.TargetEnd.EntityType,
                                        navi.TargetEndName);
                                    _lastAssociationNode?.AddSubTree(sub);
                                    _lastAssociationNode = sub;
                                }
                                else
                                {
                                    _lastAssociationNode = sub;
                                }
                            }
                        }
                    }
                }

            //查不到 直接退出
            if (type is StructuralType structural)
            {
                //查找Name
                var element = structural.Elements?.FirstOrDefault(p => p.Name == name);
                //如果为属性
                if (element is Attribute attribute)
                {
                    //是否要抽取属性树
                    if (_extractingAttribute)
                    {
                        //抽取 则在抽取树的末节点上增加一节
                        var sub = new AttributeTree(attribute);
                        _lastAttributeNode?.AddSubTree(sub);
                        _lastAttributeNode = sub;
                        //无抽取的树 则此节为属性树
                        if (_attributeTree == null) _attributeTree = sub;
                    }
                }
                //为关联引用或关联端
                else if (element is ReferenceElement referenceElement)
                {
                    //获取关联树子树
                    var sub = _lastAssociationNode?.GetSubTree(name);
                    if (sub != null)
                    {
                        if (referenceElement.NavigationUse == ENavigationUse.DirectlyReference)
                        {
                            var navi = referenceElement.Navigation;
                            var refTree = sub.GetSubTree(navi.TargetEndName);
                            if (refTree != null)
                                _lastAssociationNode = refTree;
                        }
                        else
                        {
                            _lastAssociationNode = sub;
                        }
                    }
                    else
                    {
                        //构造树
                        sub = new AssociationTree(referenceElement.ReferenceType, name);
                        _lastAssociationNode?.AddSubTree(sub);

                        //如果当前为显式关联型 且表达式不完整 仅指向对端
                        if (referenceElement is AssociationReference associationReference &&
                            associationReference.AssociationType.Visible)
                        {
                            var currentType = node.Type;
                            if (node.Type.GetInterface("IEnumerable") != null)
                                currentType = node.Type.IsArray
                                    ? node.Type.GetElementType()
                                    : node.Type.GenericTypeArguments[0];

                            if (currentType != associationReference.AssociationType.ClrType)
                            {
                                var end = associationReference.AssociationType.AssociationEnds.FirstOrDefault(p =>
                                    p.EntityType.ClrType == currentType);

                                if (end != null)
                                {
                                    //下移 补一层
                                    _lastAssociationNode = sub;
                                    sub = new AssociationTree(end.ReferenceType, end.Name);
                                    _lastAssociationNode?.AddSubTree(sub);
                                }
                            }
                        }

                        if (referenceElement.NavigationUse == ENavigationUse.DirectlyReference)
                        {
                            _lastAssociationNode = sub;
                            //根据导航构造树
                            var navi = referenceElement.Navigation;
                            sub = new AssociationTree(navi.TargetEnd.EntityType,
                                navi.TargetEndName);
                            _lastAssociationNode?.AddSubTree(sub);
                            _lastAssociationNode = sub;
                        }
                        else
                        {
                            _lastAssociationNode = sub;
                        }
                    }
                }
            }

            return node;
        }

        /// <summary>
        ///     访问参数表达式。
        /// </summary>
        /// <param name="node"></param>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            //查找符合的参数绑定
            var binding = _parameterBindings?.FirstOrDefault(p => p.Parameter == node);

            if (binding != null)
            {
                Visit(binding.Expression);
            }
            else
            {
                if (_associationTree == null)
                {
                    //获取引用类型
                    var referringType = _model.GetReferringType(node.Type);
                    //关联树
                    _associationTree = new AssociationTree(referringType);
                }

                //末节点即为待
                _lastAssociationNode = _associationTree;
            }

            return node;
        }

        /// <summary>
        ///     处理Select
        /// </summary>
        /// <param name="node">方法调用节点</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var methodName = node.Method.Name;
            if (!methodName.Equals("select", StringComparison.OrdinalIgnoreCase) &&
                !methodName.Equals("selectmany", StringComparison.OrdinalIgnoreCase))
                return base.VisitMethodCall(node);

            /*select or selectmany*/
            Expression obj, arg;
            if (node.Object == null)
            {
                obj = node.Arguments[0];
                arg = node.Arguments[1];
            }
            else
            {
                obj = node.Object;
                arg = node.Arguments[0];
            }

            VisitMember((MemberExpression)obj);
            _lastAssociationNode = arg.GrowAssociationTree(_lastAssociationNode, _model, out var attrTail,
                _parameterBindings, obj).AsTree();
            _attributeTree = attrTail?.AsTree() ?? _attributeTree;
            return node;
        }
    }
}