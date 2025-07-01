/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表达式翻译器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:24:07
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;
using Obase.Providers.Sql.SqlObject;
using BinaryExpression = System.Linq.Expressions.BinaryExpression;
using ConstantExpression = System.Linq.Expressions.ConstantExpression;
using Expression = Obase.Providers.Sql.SqlObject.Expression;
using ExpressionVisitor = System.Linq.Expressions.ExpressionVisitor;
using Field = Obase.Providers.Sql.SqlObject.Field;
using UnaryExpression = System.Linq.Expressions.UnaryExpression;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     表达式翻译器。
    /// </summary>
    public class ExpressionTranslator : ExpressionVisitor
    {
        /// <summary>
        ///     数据对象模型
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     子树求值器
        /// </summary>
        private readonly SubTreeEvaluator _subTreeEvaluator;

        /// <summary>
        ///     表达式
        /// </summary>
        private Expression _expression;

        /// <summary>
        ///     形参绑定。
        /// </summary>
        private ParameterBinding[] _parameterBindings;

        /// <summary>
        ///     构造ExpressionTranslator的新实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="subTreeEvaluator">子树求值器。</param>
        /// <param name="parameterBindings">形参绑定</param>
        public ExpressionTranslator(ObjectDataModel model, SubTreeEvaluator subTreeEvaluator,
            ParameterBinding[] parameterBindings = null)
        {
            _subTreeEvaluator = subTreeEvaluator;
            _parameterBindings = parameterBindings;
            _model = model;
        }

        /// <summary>
        ///     翻译指定的表达式。
        /// </summary>
        /// <param name="expression">要翻译的表达式。</param>
        public Expression Translate(System.Linq.Expressions.Expression expression)
        {
            if (expression.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)expression;
                Visit(lambda.Body);
            }
            else
            {
                Visit(expression);
            }

            return _expression;
        }

        /// <summary>
        ///     翻译成员访问表达式
        /// </summary>
        /// <param name="exp">成员访问表达式</param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitMember(MemberExpression exp)
        {
            var type = exp.Expression.Type;
            if (type != typeof(string) && type.GetInterface("IEnumerable") != null)
                type = type.GetGenericArguments()[0];
            //取模型类型
            var modelType = _model.GetStructuralType(type);
            if (modelType != null)
            {
                var targetField = exp.GenerateField(_model, _parameterBindings);
                _expression = Expression.Fields(targetField);
                return exp;
            }

            //未注册 生成func表达式
            var hostObj = exp.Expression;
            var member = exp.Member.Name;
            _expression = GenerateFuncExpression((MemberExpression)hostObj, member);
            return exp;
        }

        /// <summary>
        ///     方法表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitMethodCall(MethodCallExpression node)
        {
            _expression = CallTranslate(node);
            return node;
        }

        /// <summary>
        ///     常量表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitConstant(ConstantExpression node)
        {
            _expression = Expression.Constant(node.Value);
            return node;
        }

        /// <summary>
        ///     一元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitUnary(UnaryExpression node)
        {
            var nodeOperand = _subTreeEvaluator.Evaluate(node.Operand);
            Visit(nodeOperand);
            var operand = _expression;
            switch (node.NodeType)
            {
                case ExpressionType.Decrement:
                    _expression = Expression.Decrement(operand);
                    break;
                case ExpressionType.Increment:
                    _expression = Expression.Increment(operand);
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    _expression = Expression.Negate(operand);
                    break;
                case ExpressionType.UnaryPlus:
                    _expression = Expression.UnaryPlus(operand);
                    break;
                case ExpressionType.Not:
                {
                    //特殊处理In表达式 统一使用In和NotIn
                    if (operand is InExpression inexp)
                        inexp.FlipOverOperator();
                    else
                        _expression = node.Type == typeof(bool) ? Expression.Not(operand) : Expression.BitNot(operand);
                    break;
                }
                case ExpressionType.Convert:
                {
                    //int16
                    if (node.Type == typeof(short) || node.Type == typeof(ushort))
                        _expression = Expression.Function("CONVERT", Expression.Constant("smallint"), operand);
                    //int32
                    if (node.Type == typeof(int) || node.Type == typeof(uint))
                        _expression = Expression.Function("CONVERT", Expression.Constant("int"), operand);
                    //int64
                    if (node.Type == typeof(long) || node.Type == typeof(ulong))
                        _expression = Expression.Function("CONVERT", Expression.Constant("bigint"), operand);
                    //byte
                    if (node.Type == typeof(byte) || node.Type == typeof(sbyte))
                        _expression = Expression.Function("CONVERT", Expression.Constant("binary"), operand);
                    break;
                }
                default:
                    throw new ExpressionIllegalException(node);
            }

            return node;
        }


        /// <summary>
        ///     二元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitBinary(BinaryExpression node)
        {
            var nodeLeft = _subTreeEvaluator.Evaluate(node.Left);
            Visit(nodeLeft);
            var left = _expression;

            var nodeRight = _subTreeEvaluator.Evaluate(node.Right);
            Visit(nodeRight);
            var right = _expression;

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    _expression = Expression.Add(left, right);
                    break;
                case ExpressionType.Subtract:
                    _expression = Expression.Subtract(left, right);
                    break;
                case ExpressionType.Multiply:
                    _expression = Expression.Multiply(left, right);
                    break;
                case ExpressionType.Divide:
                    _expression = Expression.Devide(left, right);
                    break;
                case ExpressionType.Power:
                    _expression = Expression.Power(left, right);
                    break;
                case ExpressionType.Modulo:
                    _expression = Expression.Modulo(left, right);
                    break;
                case ExpressionType.AndAlso:
                    _expression = Expression.AndAlse(left, right);
                    break;
                case ExpressionType.OrAssign:
                    _expression = Expression.OrElse(left, right);
                    break;
                case ExpressionType.OrElse:
                    _expression = Expression.OrElse(left, right);
                    break;
                case ExpressionType.Equal:
                {
                    //特殊处理In表达式 统一使用In和NotIn
                    if (left is InExpression inExp)
                    {
                        if (right is SqlObject.ConstantExpression constant)
                            if (!(bool)constant.Value)
                                inExp.FlipOverOperator();

                        _expression = left;
                    }
                    else
                    {
                        _expression = Expression.Equal(left, right);
                    }

                    break;
                }
                case ExpressionType.NotEqual:
                {
                    //特殊处理In表达式 统一使用In和NotIn
                    if (left is InExpression inExp)
                    {
                        if (right is SqlObject.ConstantExpression constant)
                            if ((bool)constant.Value)
                                inExp.FlipOverOperator();

                        _expression = left;
                    }
                    else
                    {
                        _expression = Expression.NotEqual(left, right);
                    }

                    break;
                }
                case ExpressionType.GreaterThan:
                    _expression = Expression.GreaterThan(left, right);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _expression = Expression.GreaterThanOrEqual(left, right);
                    break;
                case ExpressionType.LessThan:
                    _expression = Expression.LessThan(left, right);
                    break;
                case ExpressionType.LessThanOrEqual:
                    _expression = Expression.LessThanOrEqual(left, right);
                    break;
                case ExpressionType.And:
                    _expression = Expression.BitAnd(left, right);
                    break;
                case ExpressionType.Or:
                    _expression = Expression.BitOr(left, right);
                    break;
                case ExpressionType.ExclusiveOr:
                    _expression = Expression.BitXor(left, right);
                    break;
                case ExpressionType.LeftShift:
                    _expression = Expression.LeftShift(left, right);
                    break;
                case ExpressionType.RightShift:
                    _expression = Expression.RightShift(left, right);
                    break;
                default:
                    throw new ExpressionIllegalException(node);
            }

            return node;
        }

        /// <summary>
        ///     翻译参数表达式
        ///     考虑投影到基元类型后后续运算对结果再进行操作
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
        {
            //查找形参banding
            var binding = _parameterBindings?.FirstOrDefault(p => p.Parameter == node);

            if (binding != null)
            {
                switch (binding.Referring)
                {
                    case EParameterReferring.Single:
                    case EParameterReferring.Sequence:
                    {
                        Visit(binding.Expression);
                        break;
                    }
                    case EParameterReferring.Index:
                    {
                        var filed = new Field(@"obase$index");
                        _expression = Expression.Fields(filed);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException($"未知的形参指代{binding.Referring}", nameof(binding.Referring));
                }
            }
            else
            {
                var filed = new Field(@"obase$result");
                _expression = Expression.Fields(filed);
            }

            return node;
        }

        /// <summary>
        ///     访问Lambda表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression VisitLambda<T>(Expression<T> node)
        {
            return Visit(node.Body) ?? throw new InvalidOperationException();
        }

        /// <summary>
        ///     翻译调用表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private Expression CallTranslate(MethodCallExpression expression)
        {
            //表达式的实例值
            var objectValue = _subTreeEvaluator.Evaluate(expression.Object ?? expression.Arguments[0]);

            if (objectValue is MethodCallExpression objectValueMethodCall)
                try
                {
                    //尝试解析
                    var obj = System.Linq.Expressions.Expression.Lambda(objectValueMethodCall).Compile()
                        .DynamicInvoke();
                    //可以解析 则转换为IEnumerable即可
                    if (obj is IEnumerable enumerable)
                    {
                        //存储
                        var objList = new List<object>();
                        var enumerator = enumerable.GetEnumerator();
                        while (enumerator.MoveNext()) objList.Add(enumerator.Current);
                        //释放资源
                        if (enumerator is IDisposable disposable)
                            disposable.Dispose();
                        objectValue = System.Linq.Expressions.Expression.Constant(objList);
                    }
                    //解析不了 抛异常
                    else
                    {
                        throw new InvalidOperationException($"[{objectValue}]不是可枚举的操作");
                    }
                }
                //如果是InvalidOperationException 直接抛出
                catch (InvalidOperationException)
                {
                    throw;
                }
                //其他异常 处理为InvalidOperationException
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        $"无法直接解析本地IQueryable[{objectValue}],请将此IQueryable转换为List并存储于本地变量.", e);
                }

            //表达式的参数值
            var argsValue = _subTreeEvaluator.Evaluate(expression.Arguments.Count > 1
                ? expression.Arguments[1]
                : expression.Arguments[0]);

            //翻译表达式
            var host = Translate(objectValue ?? argsValue);
            var args = new List<Expression>();
            if (expression.Object == null)
                for (var i = 1; i < expression.Arguments.Count; i++)
                    //翻译表达式
                    args.Add(Translate(_subTreeEvaluator.Evaluate(expression.Arguments[i])));
            else
                foreach (var argument in expression.Arguments)
                    args.Add(Translate(_subTreeEvaluator.Evaluate(argument)));
            //翻译Contains StartsWith EndsWith
            if (expression.Method.Name == "Contains" || expression.Method.Name == "StartsWith" ||
                expression.Method.Name == "EndsWith")
                return TranslateContains(expression, objectValue, argsValue, host, args);
            //翻译聚合函数
            if (expression.Method.Name == "Average" || expression.Method.Name == "Count" ||
                expression.Method.Name == "Max"
                || expression.Method.Name == "Min" || expression.Method.Name == "Sum")
            {
                //参数表达式
                System.Linq.Expressions.Expression argExp;
                //形参绑定表达式
                System.Linq.Expressions.Expression bindingExp = null;
                //根据参数个数分别处理
                if (expression.Arguments.Count >= 2)
                {
                    argExp = expression.Arguments[0];
                    bindingExp = expression.Arguments[1];
                }
                else
                {
                    var targetExp = expression.Arguments[0];
                    //如果是参数表达式
                    if (targetExp is ParameterExpression parameterExpression)
                    {
                        //赋值给argExp
                        argExp = parameterExpression;
                    }
                    //不是参数表达式 但是是Select方法
                    else if (targetExp is MethodCallExpression methodCallExpression &&
                             methodCallExpression.Method.Name == "Select")
                    {
                        argExp = methodCallExpression.Arguments[0];
                        bindingExp = methodCallExpression.Arguments[1];
                    }
                    else
                    {
                        throw new InvalidOperationException("无法翻译组聚合运算，对于组元素集只允许聚合运算和Select运算。");
                    }
                }

                //生成形参绑定
                if (argExp is LambdaExpression lambdaExpression && bindingExp != null)
                    GenerateParameterBinding(bindingExp, lambdaExpression.Parameters[0]);

                var argType = _model.GetTypeOrNull(argExp.Type, out _);
                if (expression.Method.Name == "Count" && argType is StructuralType)
                {
                    //提取关联树
                    var associationTree = expression.ExtractAssociation(_model, assoTail: out var assoTail,
                        attrTail: out var attrTail, _parameterBindings);

                    //取目标名和键字段
                    var tragetName = ((ObjectType)assoTail.RepresentedType).TargetName;
                    var keyField = ((ObjectType)assoTail.RepresentedType).KeyFields;

                    //生成源
                    var alias = associationTree.Accept(new AliasGenerator());
                    var source = new SimpleSource(tragetName, alias);

                    //表达式
                    FieldExpression[] filedExps;
                    if (attrTail != null)
                    {
                        //生长属性树
                        var attrTree = attrTail.AsTree();
                        attrTree.Accept(new AttributeTreeGrower());
                        filedExps = attrTree.Accept(new CountingFieldGenerator(source));
                    }
                    else
                    {
                        filedExps = keyField.Select(key => new FieldExpression(new Field(source, key))).ToArray();
                    }

                    //加入true 表示Distinct
                    var realExps = new List<Expression>();
                    realExps.AddRange(filedExps);
                    realExps.Add(Expression.Constant(true));
                    //组成方法调用表达式
                    return Expression.Function(expression.Method.Name, realExps.ToArray());
                }

                Visit(argExp);
                return Expression.Function(expression.Method.Name == "Average" ? "Avg" : expression.Method.Name,
                    _expression);
            }

            //其他简单函数
            switch (expression.Method.Name)
            {
                case "ToString":
                    return Expression.Function("CONVERT", Expression.Constant("varchar"), host);
                case "Substring":
                    return Expression.Function("SUBSTRING", new[] { host, args[0], args[1] });
                case "IndexOf":
                    args[0] = Expression.Add(Expression.Constant("%"), args[0]);
                    args[0] = Expression.Add(args[0], Expression.Constant("%"));
                    var e = Expression.Function("PATINDEX", new[] { args[0], host });
                    return Expression.Subtract(e, Expression.Constant(1));
                case "ToUpper":
                    return Expression.Function("UPPER", host);
                case "ToLower":
                    return Expression.Function("LOWER", host);
                case "ToBoolean":
                    return Expression.Function("CONVERT", Expression.Constant("bit"), host);
                case "ToByte":
                    return Expression.Function("CONVERT", Expression.Constant("binary"), host);
                case "ToInt16":
                    return Expression.Function("CONVERT", Expression.Constant("smallint"), host);
                case "ToInt32":
                    return Expression.Function("CONVERT", Expression.Constant("int"), host);
                case "ToInt64":
                    return Expression.Function("CONVERT", Expression.Constant("bigint"), host);
                case "ToSingle":
                    return Expression.Function("CONVERT", Expression.Constant("real"), host);
                case "ToDouble":
                    return Expression.Function("CONVERT", Expression.Constant("float"), host);
                case "ToDateTime":
                    return Expression.Function("CONVERT", Expression.Constant("datetime"), host);
                case "ToDecimal":
                    return Expression.Function("CONVERT", Expression.Constant("numeric"), host);
                case "ToChar":
                    return Expression.Function("CONVERT", Expression.Constant("char"), host);
                case "Abs":
                    return Expression.Function("ABS", host);
                case "Acos":
                    return Expression.Function("Acos", host);
                case "Asin":
                    return Expression.Function("Asin", host);
                case "Atan":
                    return Expression.Function("Atan", host);
                case "Atan2":
                    return Expression.Function("Atan2", host);
                case "Ceiling":
                    return Expression.Function("Ceiling", host);
                case "Cos":
                    return Expression.Function("Cos", host);
                case "Exp":
                    return Expression.Function("Exp", host);
                case "Floor":
                    return Expression.Function("Floor", host);
                case "Log":
                    return Expression.Function("Log", host);
                case "Pow":
                    return Expression.Function("Pow", host);
                case "Round":
                    return Expression.Function("Round", host);
                case "Sin":
                    return Expression.Function("Sin", host);
                case "Sqrt":
                    return Expression.Function("Sqrt", host);
                case "Tan":
                    return Expression.Function("Tan", host);
                default:
                    throw new InvalidOperationException($"无法将{expression.Method.Name}方法翻译成Sql函数");
            }
        }

        /// <summary>
        ///     生成形参绑定
        /// </summary>
        /// <param name="bindingExp"></param>
        /// <param name="parameter"></param>
        private void GenerateParameterBinding(System.Linq.Expressions.Expression bindingExp,
            ParameterExpression parameter)
        {
            ParameterBinding newParameterBinding = null;
            ParameterExpression subParameter = null;
            //生成形参绑定
            if (bindingExp is ParameterExpression parameterExp)
            {
                var parabinding = _parameterBindings?.FirstOrDefault(p => p.Parameter == parameterExp);
                if (parabinding != null)
                    newParameterBinding =
                        new ParameterBinding(parameter, parabinding.Referring, parabinding.Expression);
            }
            else if (bindingExp is MethodCallExpression callExpression &&
                     callExpression.Method.Name == "Select")
            {
                if (callExpression.Arguments[0] is ParameterExpression parameterExpression &&
                    callExpression.Arguments[1] is LambdaExpression lambdaExpression)
                {
                    newParameterBinding = new ParameterBinding(parameterExpression, lambdaExpression.Body);
                    subParameter = lambdaExpression.Parameters[0];
                }
            }

            //加入形参绑定
            var tempList = new List<ParameterBinding>();
            if (_parameterBindings != null)
                tempList.AddRange(_parameterBindings);

            tempList.Add(newParameterBinding);
            _parameterBindings = tempList.ToArray();
            //如果有下一级
            if (subParameter != null)
                //递归调用
                GenerateParameterBinding(bindingExp, subParameter);
        }

        /// <summary>
        ///     翻译包含函数
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="objectValue"></param>
        /// <param name="argsValue"></param>
        /// <param name="host"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Expression TranslateContains(MethodCallExpression expression,
            System.Linq.Expressions.Expression objectValue, System.Linq.Expressions.Expression argsValue,
            Expression host, List<Expression> args)
        {
            //先对Host进行解析 是否为配置的Attribute
            //如果是配置的Attribute 且 配置类型为string 则按照like处理
            var isAttributeString = false;
            //按照Member表达式查找
            if (objectValue is MemberExpression objectValueMember)
            {
                var hostType = objectValueMember.Expression.Type;
                //取模型类型
                var modelType = _model.GetStructuralType(hostType);

                //找到对应的Attribute
                var attribute = modelType?.GetAttribute(objectValueMember.Member.Name);
                if (attribute != null)
                    //是否是配置成string的Attribute
                    isAttributeString = attribute.DataType == typeof(string);
            }

            //是原生string或者被配置为string 进行模式匹配
            if (argsValue != null && ((objectValue ?? argsValue).Type == typeof(string) || isAttributeString))
            {
                //如果是
                if (argsValue is MemberExpression argsMember)
                {
                    //包含 做模式匹配
                    if (expression.Method.Name == "Contains") return Expression.Like(host, Translate(argsMember));

                    //开始
                    if (expression.Method.Name == "StartsWith")
                        return Expression.Like(host, Translate(argsMember), ELikeType.StartWith);

                    //结束
                    if (expression.Method.Name == "EndsWith")
                        return Expression.Like(host, Translate(argsMember), ELikeType.EndWith);
                }

                var containArgs = argsValue.ToString().Replace("\"", string.Empty);
                //包含 做模式匹配
                if (expression.Method.Name == "Contains")
                {
                    //自己指定的模式匹配
                    if (containArgs.StartsWith("%") || containArgs.EndsWith("%")) return Expression.Like(host, args[0]);

                    return Expression.Like(host, $"%{containArgs}%");
                }

                //开始 尾加%
                if (expression.Method.Name == "StartsWith") return Expression.Like(host, $"{containArgs}%");

                //结束 前加%
                if (expression.Method.Name == "EndsWith") return Expression.Like(host, $"%{containArgs}");
            }

            //外部值Contains 按照IN处理
            var value = ((objectValue ?? argsValue) as ConstantExpression)?.Value;
            //集合 翻译为IN函数
            if (value is IEnumerable source) return Expression.In(args[0], source);

            throw new ExpressionIllegalException(expression, $"要翻译的属性{host}不是string或外部集合,无法翻译为Sql语句.");
        }

        /// <summary>
        ///     生成函数表达式
        /// </summary>
        /// <param name="hostObj"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        private Expression GenerateFuncExpression(MemberExpression hostObj, string memberName)
        {
            if (memberName == "Length")
            {
                _subTreeEvaluator.Evaluate(hostObj);
                Visit(_subTreeEvaluator.Evaluate(hostObj));
                var arg = _expression;
                return Expression.Function("len", arg);
            }

            throw new ExpressionIllegalException(hostObj, "无法将属性" + memberName + "翻译成SQL函数");
        }
    }
}