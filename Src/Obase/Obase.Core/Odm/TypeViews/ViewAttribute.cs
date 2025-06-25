/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:06:53
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     视图属性。
    ///     视图属性来源于源（或源扩展）的一个属性，或一个或多个属性的算术运算。
    ///     视图属性属于简单属性。
    /// </summary>
    public class ViewAttribute : Attribute, ITypeViewElement
    {
        /// <summary>
        ///     属性绑定，一个表达式，说明视图属性的来源。
        /// </summary>
        private readonly Expression _binding;

        /// <summary>
        ///     锁对象
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        ///     视图属性的源。
        /// </summary>
        private readonly ViewAttributeSource[] _sources;

        /// <summary>
        ///     求值器，用于根据属性绑定表达式计算属性的值。
        /// </summary>
        private ViewAttributeEvaluator _evaluator;

        /// <summary>
        ///     (寄存)该值指示视图属性是否为直观属性。
        /// </summary>
        private bool? _isIntuitive;

        /// <summary>
        ///     影子元素。
        /// </summary>
        private ViewAttribute _shadow;

        /// <summary>
        ///     创建表示非直观属性的ViewAttribute实例。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="binding">属性绑定。</param>
        /// <param name="sources">属性源。</param>
        public ViewAttribute(string name, Expression binding, ViewAttributeSource sources)
            : this(name, binding, new[] { sources })
        {
        }

        /// <summary>
        ///     创建表示非直观属性的ViewAttribute实例。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="binding">属性绑定。</param>
        /// <param name="sources">属性源。</param>
        public ViewAttribute(string name, Expression binding, ViewAttributeSource[] sources)
            : base(binding.Type, name)
        {
            _binding = binding;
            _sources = sources;
        }

        /// <summary>
        ///     创建表示直观属性的ViewAttribute实例。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="attributeNode">属性树节点，代表构成属性源的属性。</param>
        /// <param name="extensionNode">构成属性源的扩展树节点，未指定表示根节点。</param>
        public ViewAttribute(string name, AttributeTreeNode attributeNode, AssociationTreeNode extensionNode = null)
            : this(name, attributeNode.Attribute, extensionNode)
        {
        }

        /// <summary>
        ///     创建表示直观属性的ViewAttribute实例。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="attribute">构成属性源的属性，须为顶级属性，不接受子属性。</param>
        /// <param name="extensionNode">构成属性源的扩展树节点，未指定表示根节点。</param>
        public ViewAttribute(string name, Attribute attribute, AssociationTreeNode extensionNode = null)
            : base(attribute.DataType, name)
        {
            var decType = attribute.HostType.ClrType;
            var memberInfo = decType.GetMember(attribute.Name).FirstOrDefault();
            if (memberInfo == null)
                memberInfo = decType
                    .GetMember(attribute.Name, BindingFlags.NonPublic | BindingFlags.Instance).First();
            var parExp = Expression.Parameter(decType);
            var representor = Expression.MakeMemberAccess(parExp, memberInfo);
            _binding = representor;
            _sources = extensionNode == null
                ? new[] { new ViewAttributeSource(attribute, representor) }
                : new[] { new ViewAttributeSource(extensionNode, attribute, representor) };
            _isIntuitive = true;
            TargetField = attribute.TargetField;
        }

        /// <summary>
        ///     获取属性绑定。
        /// </summary>
        public Expression Binding => _binding;

        /// <summary>
        ///     获取属性求值器。
        /// </summary>
        /// 实施说明：
        /// 检查寄存在器，如果已存在则直接返回，否则生成一个。生成算法参见顺序图“生成属性求值器”。
        /// 务必将生成的求值器放入寄存器，避免重复生成。
        public ViewAttributeEvaluator Evaluator
        {
            get
            {
                lock (_lockObject)
                {
                    if (_evaluator != null) return _evaluator;
                    var lambda = GenerateAgentExpression();
                    return _evaluator = ViewAttributeEvaluator.Create(lambda.Compile());
                }
            }
        }

        /// <summary>
        ///     获取一个值，该指指示视图属性是否为直观属性。
        ///     说明
        ///     一个视图属性是直观，意即它直接以其源的值为值，不经过任何计算。
        ///     直观属性必定是单源属性。
        /// </summary>
        /// 实施说明:
        /// 当且仅当一个视图属性满足以下条件时，它是直观属性：
        /// （1）为单源属性；
        /// （2）绑定表达式为成员表达式；
        /// （3）该成员表达式宿主对象是一个结构化类型，该成员为该类型的一个简单属性。
        /// 
        /// 实施注意:
        /// 为避免重复计算，应当在首次计算后将结果寄存。
        public bool IsIntuitive
        {
            get
            {
                lock (_lockObject)
                {
                    if (_isIntuitive == null)
                    {
                        var isIntuitive = false;
                        //简单源 并且 绑定表达式为成员表达式
                        if (SourceSingle && _binding is MemberExpression member)
                        {
                            var structuralType = HostType.Model.GetStructuralType(member.Expression.Type);
                            //该成员表达式宿主对象是一个结构化类型，该成员为该类型的一个简单属性。（即非复杂属性）
                            isIntuitive =
                                !(structuralType?.GetAttribute(member.Member.Name).IsComplex ??
                                  true);
                        }

                        _isIntuitive = isIntuitive;
                    }

                    return _isIntuitive.Value;
                }
            }
        }

        /// <summary>
        ///     获取或设置视图属性的影子元素。
        /// </summary>
        internal ViewAttribute Shadow
        {
            get => _shadow;
            set => _shadow = value;
        }

        /// <summary>
        ///     获取一个值，该值指示属性是否为多源属性。
        /// </summary>
        public bool SourceMultiple => _sources.Length > 1;

        /// <summary>
        ///     获取属性源。
        /// </summary>
        public ViewAttributeSource[] Sources => _sources;

        /// <summary>
        ///     获取一个值，该值指示属性是否为单源属性。
        /// </summary>
        public bool SourceSingle => _sources.Length == 1;

        /// <summary>
        ///     生成在视图表达式中定义视图属性的表达式，它规定了属性的锚点和绑定。
        /// </summary>
        /// <returns>定义当前视图属性的表达式。</returns>
        /// <param name="sourcePara">代表视图源的形参。</param>
        /// <param name="flatteningParaGetter">一个委托，用于获取代表指定平展点的形参。</param>
        /// 实施说明:
        /// 如果属性是非直观属性，直接返回其绑定表达式。
        /// 否则，使用AssociationExpressionGenerator和AttributeExpressionGenerator生成表达式。
        public Expression GenerateExpression(ParameterExpression sourcePara,
            Func<AssociationTreeNode, ParameterExpression> flatteningParaGetter)
        {
            if (IsIntuitive)
            {
                var attrSource = _sources[0];
                var typeExpGenerator = new AssociationExpressionGenerator(sourcePara, flatteningParaGetter);
                var hostExp = attrSource.ExtensionNode.AsTree().Accept(typeExpGenerator);
                var generator = new AttributeExpressionGenerator(hostExp);
                //使用属性树构造
                attrSource.AttributeNode.AsTree().Accept(generator);
                return generator.Result;
            }

            return _binding;
        }

        ITypeViewElement ITypeViewElement.Shadow
        {
            get => _shadow;
            set => _shadow = (ViewAttribute)value;
        }

        /// <summary>
        ///     生成代理表达式。
        ///     代理表达式基于属性源代理计算属性的值，它以属性的绑定表达式为基本框架，以代表属性源代理的形参替换其中的属性源表达式。
        /// </summary>
        private LambdaExpression GenerateAgentExpression()
        {
            var generator = new AgentExpressionGenerator();
            foreach (var attrSource in _sources)
            {
                var parameter = Expression.Parameter(attrSource.AttributeNode.Attribute.DataType);
                generator.AddParameter(parameter, attrSource.Representor);
            }

            _binding.Accept(generator);
            return (LambdaExpression)_binding;
        }

        /// <summary>
        ///     代理表达式生成器。
        ///     代理表达式基于属性源代理计算属性的值，它以属性的绑定表达式为基本框架，以代表属性源代理的形参替换其中的属性源表达式。
        /// </summary>
        public class AgentExpressionGenerator : ExpressionVisitor
        {
            /// <summary>
            ///     代表属性源代理的形参，每个属性源表达式对应一个形参。
            /// </summary>
            private Dictionary<MemberExpression, ParameterExpression> _parameters;

            /// <summary>
            ///     添加代表属性源代理的形参。
            /// </summary>
            /// <param name="parameter">代表属性源代理的形参。</param>
            /// <param name="sourceExp">属性源表达式。</param>
            internal void AddParameter(ParameterExpression parameter, MemberExpression sourceExp)
            {
                if (_parameters == null) _parameters = new Dictionary<MemberExpression, ParameterExpression>();
                _parameters[sourceExp] = parameter;
            }

            /// <summary>
            ///     访问Lambda表达式
            /// </summary>
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                var exp = Visit(node.Body);
                if (exp == null) throw new ArgumentException($"不能为{node.Body}生成代理表达式");
                return Expression.Lambda(exp, _parameters.Values);
            }

            /// <summary>
            ///     访问成员表达式
            /// </summary>
            protected override Expression VisitMember(MemberExpression node)
            {
                return _parameters[node];
            }
        }
    }
}