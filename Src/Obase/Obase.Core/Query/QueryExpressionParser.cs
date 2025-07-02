/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询表达式解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 12:17:30
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     查询表达式解析器。
    /// </summary>
    /// 实施说明:
    /// 将对象集的方法（主要是Queryable定义的扩展方法）映射到查询运算。
    /// 大多数方法都映射到同名运算，以下几个方法除外：
    /// （1）Aggregate方法映射到Accumulate运算；
    /// （2）ElementAtOrDefault方法映射到ElementAt运算；
    /// （3）FirstOrDefault方法映射到First运算；
    /// （4）LastOrDefault方法映射到Last运算；
    /// （5）LongCount方法映射到Count运算；
    /// （6）OrderByDescending方法映射到Order运算；
    /// （7）SelectMany方法映射到Select运算；
    /// （8）SingleOrDefault方法映射到Single运算；
    /// （9）ThenBy、ThenByDescending方法映射到Order运算；
    /// （10）Concat、Union、Intersect、Except四个方法映射到Set运算；
    /// （11）GroupJoin方法映射到Join运算；
    /// （12）Sum、Average、Max、Min四个方法映射到AtithAggregate运算。
    /// 此外，还有以下两个不由Queryable定义的方法：
    /// （1）SortBy方法映射到Order运算；
    /// （2）Include方法映射到Include运算。
    public class QueryExpressionParser : ExpressionVisitor
    {
        /// <summary>
        ///     对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     作为访问结果的查询链。
        /// </summary>
        private QueryOp _queryOp;

        /// <summary>
        ///     初始化QueryExpressionParser类的新实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        public QueryExpressionParser(ObjectDataModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     获取解析出的查询链。
        /// </summary>
        public QueryOp QueryOp => _queryOp;

        /// <summary>
        ///     访问MethodCall表达式。
        /// </summary>
        /// <param name="node">要访问的表达式树节点。</param>
        /// 实施说明：
        /// 见活动图“解析查询表达式”。
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = node.Arguments[0];
            //源类型
            var sourceType = obj.Type.GetGenericArguments()[0];
            //结果类型
            var resultType = node.Type;
            if (resultType.IsGenericType) resultType = resultType.GetGenericArguments()[0];
            //方法名
            var methodName = node.Method.Name;
            //参数集合
            var arguments = node.Arguments.Skip(1)
                .Select(p => p.NodeType == ExpressionType.Quote ? ((UnaryExpression)p).Operand : p).ToList();
            //根据方法名和参数数量，确定查询运算。
            switch (methodName)
            {
                case "Aggregate":
                {
                    LambdaExpression func = null;
                    object seed = null;
                    LambdaExpression selector = null;
                    //根据参数分别处理
                    if (arguments.Count == 1) func = (LambdaExpression)arguments[0];
                    if (arguments.Count == 2)
                    {
                        seed = GetArgumentValue<object>(arguments[0]);
                        func = (LambdaExpression)arguments[1];
                    }

                    if (arguments.Count == 3)
                    {
                        seed = GetArgumentValue<object>(arguments[0]);
                        func = (LambdaExpression)arguments[1];
                        selector = (LambdaExpression)arguments[2];
                    }

                    _queryOp = QueryOp.Accumulate(func, seed, selector, _model, _queryOp);
                }
                    break;
                case "All":
                {
                    LambdaExpression predicate = null;
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = QueryOp.All(predicate, _model, _queryOp);
                }
                    break;
                case "Any":
                {
                    LambdaExpression predicate = null;
                    //有没有参数 代表有没有查询条件
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.Any(sourceType, _model, _queryOp)
                        : QueryOp.Any(predicate, _model, _queryOp);
                }
                    break;
                case "Average":
                {
                    LambdaExpression selector = null;
                    if (arguments.Count > 0) selector = (LambdaExpression)arguments[0];
                    _queryOp = QueryOp.Average(selector, _model, _queryOp);
                }
                    break;
                case "Cast":
                    _queryOp = QueryOp.Cast(sourceType, resultType, _model, _queryOp);
                    break;
                case "Concat":
                    _queryOp = QueryOp.Set(sourceType, GetArgumentValue<IEnumerable>(arguments[0]), ESetOperator.Concat,
                        null, _model, _queryOp);
                    break;
                case "Contains":
                {
                    object item = null;
                    IEqualityComparer comparer = null;
                    //参数数量代表有没有查询条件和比较器
                    if (arguments.Count > 0) item = GetArgumentValue<object>(arguments[0]);
                    if (arguments.Count > 1) comparer = GetArgumentValue<IEqualityComparer>(arguments[1]);
                    _queryOp = QueryOp.Contains(item, comparer, _model, _queryOp);
                }
                    break;
                case "Count":
                {
                    LambdaExpression predicate = null;
                    //参数数量代表有没有查询条件
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.Counts(sourceType, _model, _queryOp)
                        : QueryOp.Counts(predicate, _model, _queryOp);
                }
                    break;
                case "DefaultIfEmpty":
                {
                    object defaultValue = null;
                    if (arguments.Count > 0) defaultValue = GetArgumentValue<object>(arguments[0]);
                    _queryOp = QueryOp.DefaultIfEmpty(sourceType, defaultValue, _model, _queryOp);
                }
                    break;
                case "Distinct":
                {
                    IEqualityComparer comparer = null;
                    if (arguments.Count > 0) comparer = GetArgumentValue<IEqualityComparer>(arguments[0]);
                    _queryOp = QueryOp.Distinct(sourceType, comparer, _model, _queryOp);
                }
                    break;
                case "ElementAt":
                {
                    var index = 0;
                    if (arguments.Count > 0) index = GetArgumentValue<int>(arguments[0]);
                    _queryOp = QueryOp.ElementAt(sourceType, index, false, _model, _queryOp);
                }
                    break;
                case "ElementAtOrDefault":
                {
                    var index = 0;
                    if (arguments.Count > 0) index = GetArgumentValue<int>(arguments[0]);
                    _queryOp = QueryOp.ElementAt(sourceType, index, true, _model, _queryOp);
                }
                    break;
                case "Except":
                {
                    IEnumerable source2 = null;
                    IEqualityComparer comparer = null;
                    if (arguments.Count > 0) source2 = GetArgumentValue<IEnumerable>(arguments[0]);
                    if (arguments.Count > 1) comparer = GetArgumentValue<IEqualityComparer>(arguments[1]);
                    _queryOp = QueryOp.Set(sourceType, source2, ESetOperator.Except, comparer, _model, _queryOp);
                }
                    break;
                case "First":
                {
                    LambdaExpression predicate = null;
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.First(sourceType, false, _model, _queryOp)
                        : QueryOp.First(predicate, false, _model, _queryOp);
                }
                    break;
                case "FirstOrDefault":
                {
                    LambdaExpression predicate = null;
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.First(sourceType, true, _model, _queryOp)
                        : QueryOp.First(predicate, true, _model, _queryOp);
                }
                    break;
                case "GroupBy":
                {
                    //分组有数个重载方法，参数数量和类型不同。
                    var genericArgs = node.Method.GetGenericArguments();
                    if (genericArgs.Length == 4)
                    {
                        LambdaExpression keySelector = null, elementSelector = null, resultSelector = null;
                        IEqualityComparer comparer = null;
                        if (arguments.Count > 0) keySelector = (LambdaExpression)arguments[0];
                        if (arguments.Count > 1) elementSelector = (LambdaExpression)arguments[1];
                        if (arguments.Count > 2) resultSelector = (LambdaExpression)arguments[2];
                        if (arguments.Count > 3) comparer = GetArgumentValue<IEqualityComparer>(arguments[3]);
                        _queryOp = QueryOp.GroupBy(keySelector, elementSelector, resultSelector, comparer, _model,
                            _queryOp);
                    }
                    else if (genericArgs.Length == 3)
                    {
                        LambdaExpression keySelector = null, elementOrResultSelector = null;
                        IEqualityComparer comparer = null;
                        if (arguments.Count > 0) keySelector = (LambdaExpression)arguments[0];
                        if (arguments.Count > 1) elementOrResultSelector = (LambdaExpression)arguments[1];
                        if (arguments.Count > 2) comparer = GetArgumentValue<IEqualityComparer>(arguments[2]);

                        if (elementOrResultSelector != null &&
                            elementOrResultSelector.Parameters.Count == 2) //对应方法GroupBy<TSource,TKey,TElement>
                            _queryOp = QueryOp.GroupBy(keySelector, comparer, elementOrResultSelector, _model,
                                _queryOp);
                        else //对应方法GroupBy<TSource,TKey,TResult>
                            _queryOp = QueryOp.GroupBy(keySelector, elementOrResultSelector, comparer, _model,
                                _queryOp);
                    }
                    else if (genericArgs.Length == 2)
                    {
                        LambdaExpression keySelector = null;
                        IEqualityComparer comparer = null;
                        if (arguments.Count > 0) keySelector = (LambdaExpression)arguments[0];
                        if (arguments.Count == 2) comparer = GetArgumentValue<IEqualityComparer>(arguments[1]);
                        _queryOp = QueryOp.GroupBy(keySelector, comparer, _model, _queryOp);
                    }
                }
                    break;
                case "GroupJoin":
                {
                    var inner = GetArgumentValue<IEnumerable>(arguments[0]);
                    var outerKeySelector = (LambdaExpression)arguments[1];
                    var innerKeySelector = (LambdaExpression)arguments[2];
                    var resultSelector = (LambdaExpression)arguments[3];
                    IEqualityComparer comparer = null;
                    //判断分组后连接操作有没有IEqualityComparer
                    if (arguments.Count == 5) comparer = GetArgumentValue<IEqualityComparer>(arguments[4]);
                    _queryOp = QueryOp.Join(inner, outerKeySelector, innerKeySelector, resultSelector, comparer,
                        _model, _queryOp);
                }
                    break;

                case "Intersect":
                {
                    var source2 = GetArgumentValue<IEnumerable>(arguments[0]);
                    IEqualityComparer comparer = null;
                    if (arguments.Count == 2) comparer = GetArgumentValue<IEqualityComparer>(arguments[1]);
                    _queryOp = QueryOp.Set(sourceType, source2, ESetOperator.Interact, comparer, _model, _queryOp);
                }
                    break;
                case "Join":
                {
                    var inner = GetArgumentValue<IEnumerable>(arguments[0]);
                    var outerKeySelector = (LambdaExpression)arguments[1];
                    var innerKeySelector = (LambdaExpression)arguments[2];
                    var resultSelector = (LambdaExpression)arguments[3];
                    IEqualityComparer comparer = null;
                    //连接操作有没有IEqualityComparer
                    if (arguments.Count == 5) comparer = GetArgumentValue<IEqualityComparer>(arguments[4]);
                    _queryOp = QueryOp.Join(inner, outerKeySelector, innerKeySelector, resultSelector, comparer,
                        _model, _queryOp);
                }
                    break;
                case "Last":
                {
                    LambdaExpression predicate = null;
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.Last(sourceType, false, _model, _queryOp)
                        : QueryOp.Last(predicate, false, _model, _queryOp);
                }
                    break;
                case "LastOrDefault":
                {
                    LambdaExpression predicate = null;
                    //是否有查询条件
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.Last(sourceType, true, _model, _queryOp)
                        : QueryOp.Last(predicate, true, _model, _queryOp);
                }
                    break;
                case "LongCount":
                {
                    LambdaExpression predicate = null;
                    //是否有查询条件
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.Counts(sourceType, _model, _queryOp)
                        : QueryOp.Counts(predicate, _model, _queryOp);
                }
                    break;
                case "Max":
                {
                    LambdaExpression selector = null;
                    //是否有查询条件
                    if (arguments.Count > 0) selector = (LambdaExpression)arguments[0];
                    _queryOp = QueryOp.Max(selector, _model, _queryOp);
                }
                    break;
                case "Min":
                {
                    LambdaExpression selector = null;
                    //是否有查询条件
                    if (arguments.Count > 0) selector = (LambdaExpression)arguments[0];
                    _queryOp = QueryOp.Min(selector, _model, _queryOp);
                }
                    break;
                case "OfType":
                    _queryOp = QueryOp.OfType(resultType, _model, _queryOp);
                    break;
                case "OrderBy":
                {
                    var keySelector = (LambdaExpression)arguments[0];
                    IComparer comparer = null;
                    //是否有比较器
                    if (arguments.Count == 2) comparer = GetArgumentValue<IComparer>(arguments[1]);
                    _queryOp = QueryOp.OrderBy(keySelector, false, comparer, _model, _queryOp);
                }
                    break;
                case "OrderByDescending":
                {
                    var keySelector = (LambdaExpression)arguments[0];
                    IComparer comparer = null;
                    //是否有比较器
                    if (arguments.Count == 2) comparer = GetArgumentValue<IComparer>(arguments[1]);
                    _queryOp = QueryOp.OrderBy(keySelector, true, comparer, _model, _queryOp);
                }
                    break;
                case "Prepend":
                    throw new InvalidOperationException("暂不支持Prepend方法");
                case "Reverse":
                    _queryOp = QueryOp.Reverse(sourceType, _queryOp);
                    break;
                case "Select":
                {
                    var selector = (LambdaExpression)arguments[0];
                    //确定投影的类型
                    if (selector.ReturnType != typeof(string) &&
                        selector.ReturnType.GetInterface("IEnumerable") != null)
                        _queryOp = QueryOp.Select(selector, resultType, _model, _queryOp);
                    else
                        _queryOp = QueryOp.Select(selector, _model, _queryOp);
                }
                    break;
                case "SelectMany":
                {
                    var genericArgs = node.Method.GetGenericArguments();
                    //确定是中介的还是普通的
                    if (genericArgs.Count() == 3)
                    {
                        var collectionSelector = (LambdaExpression)arguments[0];
                        var resultSelector = (LambdaExpression)arguments[1];
                        _queryOp = QueryOp.Select(resultSelector, collectionSelector, _model, _queryOp);
                    }
                    else
                    {
                        var selector = (LambdaExpression)arguments[0];
                        _queryOp = QueryOp.Select(selector, resultType, _model, _queryOp);
                    }
                }
                    break;
                case "SequenceEqual":
                {
                    var source2 = GetArgumentValue<IEnumerable>(arguments[0]);
                    IEqualityComparer comparer = null;
                    //是否有比较器
                    if (arguments.Count == 2) comparer = GetArgumentValue<IEqualityComparer>(arguments[1]);
                    _queryOp = QueryOp.SequenceEqual(source2, comparer, _model, _queryOp);
                }
                    break;
                case "Single":
                {
                    LambdaExpression predicate = null;
                    //是否有查询条件
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.Single(sourceType, false, _model, _queryOp)
                        : QueryOp.Single(predicate, false, _model, _queryOp);
                }
                    break;
                case "SingleOrDefault":
                {
                    LambdaExpression predicate = null;
                    //是否有查询条件
                    if (arguments.Count > 0) predicate = (LambdaExpression)arguments[0];
                    _queryOp = predicate == null
                        ? QueryOp.Single(sourceType, true, _model, _queryOp)
                        : QueryOp.Single(predicate, true, _model, _queryOp);
                }
                    break;
                case "Skip":
                {
                    var count = GetArgumentValue<int>(arguments[0]);
                    _queryOp = QueryOp.Skip(sourceType, count, _model, _queryOp);
                }
                    break;
                case "SkipLast":
                    throw new InvalidOperationException("暂不支持SkipLast方法");
                case "SkipWhile":
                    _queryOp = QueryOp.SkipWhile((LambdaExpression)arguments[0], _model, _queryOp);
                    break;
                case "Sum":
                {
                    LambdaExpression selector = null;
                    if (node.Method.IsGenericMethod) selector = (LambdaExpression)arguments[0];
                    _queryOp = QueryOp.Sum(selector, _model, _queryOp);
                }
                    break;
                case "Take":
                    _queryOp = QueryOp.Take(sourceType, GetArgumentValue<int>(arguments[0]), _model, _queryOp);
                    break;
                case "TakeLast":
                    throw new InvalidOperationException("暂不支持TakeLast方法");
                case "TakeWhile":
                    _queryOp = QueryOp.TakeWhile((LambdaExpression)arguments[0], _model, _queryOp);
                    break;
                case "ThenBy":
                {
                    var keySelector = (LambdaExpression)arguments[0];
                    IComparer comparer = null;
                    if (arguments.Count == 2) comparer = GetArgumentValue<IComparer>(arguments[1]);
                    _queryOp = QueryOp.ThenOrderBy(keySelector, false, comparer, _model, _queryOp);
                }
                    break;
                case "ThenByDescending":
                {
                    var keySelector = (LambdaExpression)arguments[0];
                    IComparer comparer = null;
                    if (arguments.Count == 2) comparer = GetArgumentValue<IComparer>(arguments[1]);
                    _queryOp = QueryOp.ThenOrderBy(keySelector, true, comparer, _model, _queryOp);
                }
                    break;
                case "Union":
                {
                    var source2 = GetArgumentValue<IEnumerable>(arguments[0]);
                    IEqualityComparer comparer = null;
                    if (arguments.Count == 2) comparer = GetArgumentValue<IEqualityComparer>(arguments[1]);
                    _queryOp = QueryOp.Set(sourceType, source2, ESetOperator.Union, comparer, _model, _queryOp);
                }
                    break;
                case "Where":
                    _queryOp = QueryOp.Where((LambdaExpression)arguments[0], _model, _queryOp);
                    break;
                case "Zip":
                {
                    var source2 = GetArgumentValue<IEnumerable>(arguments[0]);
                    LambdaExpression resultSelector = null;
                    //是否有结果选择器
                    if (arguments.Count == 2) resultSelector = (LambdaExpression)arguments[1];
                    _queryOp = resultSelector != null
                        ? QueryOp.Zip(source2, sourceType, resultSelector, _queryOp)
                        : QueryOp.Zip(source2, sourceType, resultType, _queryOp);
                }
                    break;
                //不由Queryable定义的方法 而是自己扩展的
                case "Include":
                {
                    if (arguments[0].NodeType == ExpressionType.Quote)
                    {
                        var selector = (LambdaExpression)((UnaryExpression)arguments[0]).Operand;
                        _queryOp = QueryOp.Include(selector, _model, _queryOp);
                    }
                    else if (arguments[0].NodeType == ExpressionType.Lambda)
                    {
                        var selector = (LambdaExpression)arguments[0];
                        _queryOp = QueryOp.Include(selector, _model, _queryOp);
                    }
                    //使用字符串的Include
                    else if (arguments[0].NodeType == ExpressionType.Constant)
                    {
                        var path = GetArgumentValue<string>(arguments[0]);
                        _queryOp = QueryOp.Include(path, sourceType, _model, _queryOp);
                    }
                }
                    break;
                case "SortBy":
                {
                    var keySelector = (LambdaExpression)arguments[0];
                    IComparer comparer = null;
                    if (arguments.Count == 2) comparer = GetArgumentValue<IComparer>(arguments[1]);
                    _queryOp = QueryOp.OrderBy(keySelector, false, comparer, _model, _queryOp);
                }
                    break;
                default:
                    throw new NotSupportedException($"未知方法名{methodName}");
            }

            return Visit(obj) ?? obj;
        }

        /// <summary>
        ///     处理参数的值
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private static TValue GetArgumentValue<TValue>(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Quote) expression = ((UnaryExpression)expression).Operand;
            if (expression.NodeType == ExpressionType.Constant) return (TValue)((ConstantExpression)expression).Value;
            //编译后即可得到
            var lambda = Expression.Lambda(expression);
            var del = (Func<TValue>)lambda.Compile();
            return del();
        }
    }
}