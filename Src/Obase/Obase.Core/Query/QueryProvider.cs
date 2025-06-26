/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询提供程序基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:33:39
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query
{
    /// <summary>
    ///     为查询提供程序提供基础实现。
    /// </summary>
    public abstract class QueryProvider : IQueryProvider, IQueryPipeline
    {
        /// <summary>
        ///     附加委托
        /// </summary>
        protected readonly AttachObject _attachObject;

        /// <summary>
        ///     数据模型
        /// </summary>
        protected readonly ObjectDataModel _model;

        /// <summary>
        ///     上下文
        /// </summary>
        protected readonly ObjectContext Context;

        /// <summary>
        ///     构造QueryProvider的新实例。
        /// </summary>
        protected QueryProvider(ObjectDataModel model, AttachObject attachObject, ObjectContext context)
        {
            _model = model;
            _attachObject = attachObject;
            Context = context;
        }

        /// <summary>
        ///     数据模型
        /// </summary>
        public ObjectDataModel Model => _model;

        /// <summary>
        ///     附加委托
        /// </summary>
        public AttachObject AttachObject => _attachObject;

        /// <summary>
        ///     为PreExecuteSql事件附加或移除事件处理程序。
        /// </summary>
        public event EventHandler<QueryEventArgs> PreExecuteCommand;

        /// <summary>
        ///     执行Sql语句之后事件
        /// </summary>
        public event EventHandler<QueryEventArgs> PostExecuteCommand;

        /// <summary>
        ///     开始查询事件
        /// </summary>
        public event EventHandler<QueryEventArgs> BeginQuery;

        /// <summary>
        ///     结束查询事件
        /// </summary>
        public event EventHandler<QueryEventArgs> EndQuery;

        /// <summary>
        ///     构造一个 IQueryable{T]对象，该对象可计算指定表达式树所表示的查询。
        ///     类型参数
        ///     TElement	返回的 IQueryable{T} 的元素的类型。
        /// </summary>
        /// <param name="expression">表示查询的表达式树。</param>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new ObjectSet<TElement>(Context, this, expression);
        }

        /// <summary>
        ///     构造一个 IQueryable 对象，该对象可计算指定表达式树所表示的查询。
        /// </summary>
        /// <param name="expression">表示查询的表达式树。</param>
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = GetElementType(expression.Type);

            try
            {
                //创建一个ObjectSet
                return (IQueryable)Activator.CreateInstance(typeof(ObjectSet<>).MakeGenericType(elementType), Context,
                    this, expression);
            }

            catch (TargetInvocationException tie)
            {
                if (tie.InnerException != null) throw tie.InnerException;
                throw;
            }
        }


        /// <summary>
        ///     执行指定表达式树所表示的强类型查询。
        ///     类型参数
        ///     TResult执行查询所生成的值的类型。
        /// </summary>
        /// <param name="expression">表示查询的表达式树。</param>
        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            var result = Execute<TResult>(expression);
            return result;
        }

        /// <summary>
        ///     执行指定表达式树所表示的查询。
        /// </summary>
        /// <param name="expression">表示查询的表达式树。</param>
        object IQueryProvider.Execute(Expression expression)
        {
            var result = Execute<object>(expression);
            return result;
        }

        /// <summary>
        ///     执行查询。
        /// </summary>
        /// <returns>执行查询的结果。</returns>
        /// <param name="query">要执行的查询。</param>
        /// <param name="including">相对于查询源的包含树，指示在查询链中显式或隐含的包含运算外额外执行的包含运算。值为null表示不执行额外的包含运算。</param>
        internal object Execute(QueryOp query, AssociationTree including = null)
        {
            var context = new QueryContext(query);
            //触发开始查询事件
            BeginQuery?.Invoke(this, new QueryEventArgs(context));

            object result;
            if (context.HasCanceled)
            {
                result = context.Result;
            }
            else
            {
                var expType = query.SourceType;
                if (expType.IsGenericType) expType = expType.GetGenericArguments()[0];
                result = Execute(expType, query, including);
            }

            //触发结束查询事件
            EndQuery?.Invoke(this, new QueryEventArgs(context));
            //是已注册的或者是可枚举的 进行附加
            if (query.ResultType.GetInterface("IEnumerable") != null && _model.GetObjectType(query.ResultType) != null)
                _attachObject.Invoke(ref result, true);

            return result;
        }

        /// <summary>
        ///     执行查询。
        /// </summary>
        /// <returns>执行查询的结果。</returns>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="query">要执行的查询。值为null表示取出查询源中的所有对象。</param>
        /// <param name="including">相对于查询源的包含树，指示在查询链中显式或隐含的包含运算外额外执行的包含运算。值为null表示不执行额外的包含运算。</param>
        /// <param name="expression">查询表达式。调用方须自行确保该表达式与query参数指定的查询等效。</param>
        private object Execute(Type sourceType, QueryOp query = null, AssociationTree including = null,
            Expression expression = null)
        {
            //构造一个查询上下文
            var context = new QueryContext(query, expression) { UserState = sourceType.ToString() };
            //执行
            Execute(including, context, query);
            //返回结果
            var result = context.Result;

            return result;
        }

        /// <summary>
        ///     执行查询。
        /// </summary>
        /// <returns>执行查询的结果。</returns>
        /// <param name="query">要执行的查询。值为null表示取出查询源中的所有对象。</param>
        /// <param name="including"></param>
        /// <param name="context">查询上下文。</param>
        protected abstract void Execute(AssociationTree including, QueryContext context, QueryOp query = null);

        /// <summary>
        ///     执行指定表达式树所表示的查询。
        ///     类型参数：
        ///     TResult	执行查询所生成的值的类型。
        /// </summary>
        /// <param name="expression">表示查询的表达式树。</param>
        private TResult Execute<TResult>(Expression expression)
        {
            //首先使用QueryExpressionParser将表达式解析成查询链，然后调用Execute(Type, QueryOp, AssociationTree,Expression)
            //解析表达式
            var visitor = new QueryExpressionParser(_model);
            visitor.Visit(expression);
            //是空的 没解析 为空查询
            var query = visitor.QueryOp ?? QueryOp.Every(expression.Type.GenericTypeArguments[0], _model);

            //如果全是Include操作 则需要拼接一个NonQuery
            var includeCount = 0;
            var allopCount = 0;
            var currentQuery = query;
            while (currentQuery != null)
            {
                if (currentQuery.Name == EQueryOpName.Include) includeCount++;
                allopCount++;
                currentQuery = currentQuery.Next;
            }

            //如果是需要自己拼接的
            if (includeCount == allopCount)
                query = QueryOp.Every(expression.Type.GenericTypeArguments[0], _model, query);

            var context = new QueryContext(query, expression);
            //触发开始查询事件
            BeginQuery?.Invoke(this, new QueryEventArgs(context));

            object result;
            //如果查询实际上没有执行 直接返回结果
            if (context.HasCanceled)
            {
                result = context.Result;
            }
            else
            {
                //否则 执行查询
                query = context.Query;
                var expType = query.SourceType;
                if (expType.IsGenericType) expType = expType.GetGenericArguments()[0];
                result = Execute(expType, query);
            }

            //触发结束查询事件
            EndQuery?.Invoke(this, new QueryEventArgs(context));

            var resultType = result?.GetType();
            // 可枚结果在结果读取器里面有处理，所以这里需要对单值结果进行类型转换。
            if (resultType == null || resultType.GetInterface("IEnumerable") != null || resultType == typeof(TResult) ||
                resultType.IsSubclassOf(typeof(TResult)))
                return (TResult)result;

            return (TResult)TypeDescriptor.GetConverter(typeof(TResult)).ConvertFromInvariantString(result.ToString());
        }


        /// <summary>
        ///     获取元素类型
        /// </summary>
        /// <param name="seqType">元素类型</param>
        /// <returns></returns>
        private Type GetElementType(Type seqType)
        {
            //查找泛型的参数
            var ienum = FindIEnumerable(seqType);

            if (ienum == null) return seqType;

            return ienum.GetGenericArguments()[0];
        }

        /// <summary>
        ///     根据类型寻找IEnumerate包裹的泛型参数
        /// </summary>
        /// <param name="seqType">类型</param>
        /// <returns></returns>
        private static Type FindIEnumerable(Type seqType)
        {
            //字符串 让上层方法处理
            if (seqType == null || seqType == typeof(string))
                return null;
            //数组 取ElementType
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            //泛型的 取泛型参数
            if (seqType.IsGenericType)
                foreach (var arg in seqType.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType)) return ienum;
                }

            //实现了接口的 递归探测IEnumerable
            var ifaces = seqType.GetInterfaces();
            if (ifaces.Length > 0)
                foreach (var iface in ifaces)
                {
                    var ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }

            //继承的 找基类
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
                return FindIEnumerable(seqType.BaseType);
            return null;
        }

        /// <summary>
        ///     由查询执行器触发 将执行Sql事件前抛到上层
        /// </summary>
        /// <param name="args"></param>
        public void OnPreExecuteSql(QueryEventArgs args)
        {
            PreExecuteCommand?.Invoke(this, args);
        }

        /// <summary>
        ///     由查询执行器触发 将执行Sql事件后抛到上层
        /// </summary>
        /// <param name="args"></param>
        public void OnPostExecuteSql(QueryEventArgs args)
        {
            PostExecuteCommand?.Invoke(this, args);
        }
    }
}