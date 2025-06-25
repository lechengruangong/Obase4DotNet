/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字段描述.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:38:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     负责描述类型的字段，提供字段的关键信息。
    /// </summary>
    public class FieldDescriptor
    {
        /// <summary>
        ///     表达式文本化器
        /// </summary>
        private readonly ExpressionTextualizer _expressionTextualizer;

        /// <summary>
        ///     字段类型。
        /// </summary>
        private readonly Type _type;

        /// <summary>
        ///     值表达式，该表达式的结果作为字段的值。
        /// </summary>
        private readonly Expression _valueExpression;

        /// <summary>
        ///     是否在构造函数内创建参数
        /// </summary>
        private bool _createConstructorParameter;

        /// <summary>
        ///     指示是否为字段附加取值器。
        /// </summary>
        private bool _hasGetter;

        /// <summary>
        ///     指示是否为字段附加设值器。
        /// </summary>
        private bool _hasSetter;

        /// <summary>
        ///     字段名称。
        /// </summary>
        private string _name;

        /// <summary>
        ///     公开的Get
        /// </summary>
        private bool _publicGetter;

        /// <summary>
        ///     公开的Set
        /// </summary>
        private bool _publicSetter;

        /// <summary>
        ///     转换为字符串表示形式的结果
        /// </summary>
        private string _toStringResult;

        /// <summary>
        ///     创建FieldDescriptor实例，该实例描述一个具有指定值表达式的字段。
        ///     实施说明
        ///     以表达式的返回值类型作为字段的数据类型。
        ///     如果valueExpression参数的值为Lambda表达式，取其Body作为字段的值表达式。
        /// </summary>
        /// <param name="valueExp">值表达式。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public FieldDescriptor(Expression valueExp, ParameterBinding[] paraBindings = null)
        {
            //如果为LambdaExpression则取Body
            if (valueExp is LambdaExpression lambdaExpression) valueExp = lambdaExpression.Body;
            _valueExpression = valueExp;
            _type = valueExp.Type;

            if (paraBindings != null)
                _expressionTextualizer = new ExpressionTextualizer(paraBindings);
        }

        /// <summary>
        ///     创建FieldDescriptor实例，该实例描述一个具有指定类型字段。
        /// </summary>
        /// <param name="type">字段类型。</param>
        public FieldDescriptor(Type type)
        {
            _type = type;
        }

        /// <summary>
        ///     创建FieldDescriptor实例，该实例描述一个具有指定名称和类型的字段。
        /// </summary>
        /// <param name="type">字段类型。</param>
        /// <param name="name">字段名称。</param>
        public FieldDescriptor(Type type, string name) : this(type)
        {
            _name = name;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示是否为字段附加取值器。
        /// </summary>
        public bool HasGetter
        {
            get => _hasGetter;
            set => _hasGetter = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示是否为字段附加设值器。
        /// </summary>
        public bool HasSetter
        {
            get => _hasSetter;
            set => _hasSetter = value;
        }

        /// <summary>
        ///     获取字段名称。
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     获取字段类型。
        /// </summary>
        public Type Type => _type;

        /// <summary>
        ///     获取值表达式，该表达式的结果作为字段的值。
        /// </summary>
        public Expression ValueExpression => _valueExpression;

        /// <summary>
        ///     是否在构造函数内创建参数
        /// </summary>
        public bool CreateConstructorParameter
        {
            get => _createConstructorParameter;
            set => _createConstructorParameter = value;
        }

        /// <summary>
        ///     公开的Get
        /// </summary>
        public bool PublicGetter
        {
            get => _publicGetter;
            set => _publicGetter = value;
        }

        /// <summary>
        ///     公开的Set
        /// </summary>
        public bool PublicSetter
        {
            get => _publicSetter;
            set => _publicSetter = value;
        }

        /// <summary>
        ///     属性（Property）命名规则
        ///     如果字段名以下划线开头，取第二个字符的大写形式（注意，第二个字符可能即为大写），然后串联从第三个开始直到末尾的字符。
        ///     如果字段名以“m_”、“M_”、“f_”、“F_”开头，取从第三个字符开始直到末尾的字符。
        /// </summary>
        public string GetPropertyName()
        {
            //没有 只能返回空字符串
            if (string.IsNullOrEmpty(_name))
                return "";

            //是否以下划线开头
            if (_name.StartsWith("_") && _name.Length > 2)
            {
                var result = new StringBuilder($"{_name[1].ToString().ToUpper()}");
                //剩下的字符
                var remain = _name.Skip(2).ToArray();
                foreach (var item in remain) result.Append(item);

                return result.ToString();
            }

            //以“m_”、“M_”、“f_”、“F_”开头
            if ((_name.StartsWith("m_") || _name.StartsWith("M_") || _name.StartsWith("f_") ||
                 _name.StartsWith("F_")) && _name.Length > 2)
            {
                var result = new StringBuilder();
                //剩下的字符
                var remain = _name.Skip(2).ToArray();
                foreach (var item in remain) result.Append(item);
                return result.ToString();
            }

            return _name;
        }

        /// <summary>
        ///     以文本形式返回字段信息。
        ///     文本格式
        ///     field-desc: {Type.FullName},{Name}, {Binding}，（{}是片段标识，不属于文本内容）。
        ///     如果未指定名称，忽略Name片段。如果未指定绑定，忽略Binding片段。
        ///     使用ExpressionTextualizer生成表达式的文本化形成作为Binding片段的内容。
        ///     实施说明
        ///     寄存生成的文本字符串，避免再次生成。寄存生成的表达式文本化字符串，避免再次生成。
        /// </summary>
        public override string ToString()
        {
            //已有结果 直接返回
            if (!string.IsNullOrEmpty(_toStringResult))
                return _toStringResult;
            //没有 但是有Type 且没有name 和 binding
            if (_type != null && string.IsNullOrEmpty(_name) && _expressionTextualizer == null)
                _toStringResult = $"{Type.FullName}";
            else if (_type != null && !string.IsNullOrEmpty(_name) && _expressionTextualizer == null)
                _toStringResult = $"{Type.FullName},{_name}";
            else if (_type != null && !string.IsNullOrEmpty(_name) && _expressionTextualizer != null)
                _toStringResult = $"{Type.FullName},{_name},{_expressionTextualizer.Parser(_valueExpression)}";

            return _toStringResult;
        }

        /// <summary>
        ///     实施说明
        ///     如果显式指定了名称，直接返回；否则调用namingFunc生成名称，并写入变量_name。
        /// </summary>
        /// <param name="namingFunc">未显式指定名称时生成名称的方法。</param>
        internal string GetName(Func<string> namingFunc)
        {
            //没指定 用命名委托指定
            if (string.IsNullOrEmpty(_name))
                _name = namingFunc();

            return _name;
        }

        /// <summary>
        ///     一个表达式访问者，其功能是将表达式转换为文本表示形式。
        /// </summary>
        private class ExpressionTextualizer : ExpressionVisitor
        {
            /// <summary>
            ///     形参绑定
            /// </summary>
            private readonly ParameterBinding[] _paraBindings;

            /// <summary>
            ///     文本化的结果
            /// </summary>
            private string _textResult;

            /// <summary>
            ///     创建ExpressionTextualizer实例。
            /// </summary>
            /// <param name="paraBindings">要转换的表达式的形参绑定。</param>
            public ExpressionTextualizer(ParameterBinding[] paraBindings)
            {
                _paraBindings = paraBindings;
            }

            /// <summary>
            ///     对表达式进行文本化
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public string Parser(Expression expression)
            {
                Visit(expression);
                return _textResult;
            }

            /// <summary>
            ///     文本化常量表达式
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitConstant(ConstantExpression node)
            {
                _textResult = node.Value.ToString();
                return Expression.Constant(_textResult);
            }

            /// <summary>
            ///     文本化一元表达式
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitUnary(UnaryExpression node)
            {
                //访问内部
                Visit(node.Operand);
                var result = _textResult;
                //模型化表达式
                switch (node.NodeType)
                {
                    case ExpressionType.Decrement:
                        _textResult = $"{result}-1";
                        break;
                    case ExpressionType.Increment:
                        _textResult = $"{result}+1";
                        break;
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        _textResult = $"-{result}";
                        break;
                    case ExpressionType.UnaryPlus:
                        _textResult = $"+{result}";
                        break;
                    case ExpressionType.Not:
                        _textResult = $"!{result}";
                        break;
                }

                return Expression.Constant(_textResult);
            }

            /// <summary>
            ///     文本化二元表达式
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitBinary(BinaryExpression node)
            {
                //对左右求值
                Visit(node.Left);
                var leftResult = _textResult;

                Visit(node.Right);
                var rightResult = _textResult;

                var operate = "   ";
                switch (node.NodeType)
                {
                    case ExpressionType.Add:
                        operate = " + ";
                        break;
                    case ExpressionType.AddAssign:
                        operate = " += ";
                        break;
                    case ExpressionType.AddAssignChecked:
                        operate = " += ";
                        break;
                    case ExpressionType.AddChecked:
                        operate = " + ";
                        break;
                    case ExpressionType.And:
                        operate = " & ";
                        break;
                    case ExpressionType.AndAlso:
                        operate = " && ";
                        break;
                    case ExpressionType.AndAssign:
                        operate = " &= ";
                        break;
                    case ExpressionType.Divide:
                        operate = " / ";
                        break;
                    case ExpressionType.DivideAssign:
                        operate = " /= ";
                        break;
                    case ExpressionType.Equal:
                        operate = " == ";
                        break;
                    case ExpressionType.Decrement:
                        operate = " - ";
                        break;
                    case ExpressionType.ExclusiveOr:
                        operate = " ^ ";
                        break;
                    case ExpressionType.ExclusiveOrAssign:
                        operate = " ^= ";
                        break;
                    case ExpressionType.GreaterThan:
                        operate = " > ";
                        break;
                    case ExpressionType.Increment:
                        operate = " + 1";
                        break;
                    case ExpressionType.LeftShift:
                        operate = " << ";
                        break;
                    case ExpressionType.LeftShiftAssign:
                        operate = " <<= ";
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        operate = " >= ";
                        break;
                    case ExpressionType.LessThan:
                        operate = " < ";
                        break;
                    case ExpressionType.LessThanOrEqual:
                        operate = " <= ";
                        break;
                    case ExpressionType.Modulo:
                        operate = " % ";
                        break;
                    case ExpressionType.ModuloAssign:
                        operate = " %= ";
                        break;
                    case ExpressionType.MultiplyAssign:
                        operate = " *= ";
                        break;
                    case ExpressionType.MultiplyAssignChecked:
                        operate = " *= ";
                        break;
                    case ExpressionType.MultiplyChecked:
                        operate = " * ";
                        break;
                    case ExpressionType.NotEqual:
                        operate = " != ";
                        break;
                    case ExpressionType.Or:
                        operate = " | ";
                        break;
                    case ExpressionType.OrAssign:
                        operate = " |= ";
                        break;
                    case ExpressionType.OrElse:
                        operate = " || ";
                        break;
                    case ExpressionType.PostDecrementAssign:
                        operate = " -- ";
                        break;
                    case ExpressionType.PostIncrementAssign:
                        operate = " ++ ";
                        break;
                    case ExpressionType.Power:
                        operate = " ^ ";
                        break;
                    case ExpressionType.PowerAssign:
                        operate = " ^= ";
                        break;
                    case ExpressionType.RightShift:
                        operate = " >> ";
                        break;
                    case ExpressionType.RightShiftAssign:
                        operate = " >>= ";
                        break;
                    case ExpressionType.Subtract:
                        operate = " - ";
                        break;
                    case ExpressionType.SubtractAssign:
                        operate = " -= ";
                        break;
                    case ExpressionType.SubtractAssignChecked:
                        operate = " -= ";
                        break;
                    case ExpressionType.SubtractChecked:
                        operate = " - ";
                        break;
                    case ExpressionType.Multiply:
                        operate = " * ";
                        break;
                }

                _textResult = $"({leftResult}) {operate} ({rightResult})";

                return Expression.Constant(_textResult);
            }

            /// <summary>
            ///     文本化Lambda表达式
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                //主体表达式
                Visit(node.Body);
                //主体结果
                var bodyResult = _textResult;
                //每个参数
                var list = new List<string>();
                foreach (var parameter in node.Parameters)
                {
                    Visit(parameter);
                    var parameterResult = _textResult;
                    list.Add(parameterResult);
                }

                //最终结果
                _textResult = $"{bodyResult}({string.Join(",", list)})";

                return Expression.Constant(_textResult);
            }

            /// <summary>
            ///     文本化参数表达式
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitParameter(
                ParameterExpression node)
            {
                //搜索绑定
                var bindingExp = _paraBindings.FirstOrDefault(p => p.Parameter == node);
                if (bindingExp == null)
                {
                    _textResult = "s";
                }
                else
                {
                    //如果是索引
                    if (bindingExp.Referring == eParameterReferring.Index)
                    {
                        _textResult = "index";
                    }
                    else
                    {
                        var expResult = "";
                        //绑定的表达式不为空 处理此表达式
                        if (bindingExp.Expression != null)
                        {
                            Visit(bindingExp.Expression);
                            expResult = _textResult;
                        }

                        //不是单个的
                        if (bindingExp.Referring != eParameterReferring.Single) _textResult = $"{expResult}[]";
                    }
                }

                return Expression.Constant(_textResult);
            }

            /// <summary>
            ///     文本化成员访问表达式
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitMember(MemberExpression node)
            {
                Visit(node.Expression);
                var hostStr = _textResult;
                _textResult = $"{hostStr}.{node.Member.Name}";

                return Expression.Constant(_textResult);
            }
        }
    }
}