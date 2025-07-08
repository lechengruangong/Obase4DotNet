/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象集，提供对象的逻辑视图.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 16:14:47
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Obase.Core
{
    /// <summary>
    ///     对象集，提供对象的逻辑视图。
    /// </summary>
    public class ObjectSet<T> : IOrderedQueryable<T>
    {
        //对象上下文

        /// <summary>
        ///     查询提供程序
        /// </summary>
        private IQueryProvider _provider;

        /// <summary>
        ///     创建对象集实例。
        /// </summary>
        /// <param name="objectContext">该对象集所属的对象上下文。</param>
        public ObjectSet(ObjectContext objectContext) : this(objectContext, null)
        {
            if (objectContext.Model != null)
            {
                if (objectContext.Model.GetStructuralType(typeof(T)) == null)
                    throw new ArgumentException($"不能为未注册的{typeof(T)}类型创建对象集");

                var associationType = objectContext.Model.GetAssociationType(typeof(T));
                if (associationType != null && !associationType.Visible)
                    throw new ArgumentException($"不能为一个隐式关联型{typeof(T)}创建对象集");
            }
        }

        /// <summary>
        ///     创建对象集实例。
        /// </summary>
        /// <param name="objectContext">该对象集所属的对象上下文。</param>
        /// <param name="expression">表达式</param>
        internal ObjectSet(ObjectContext objectContext, Expression expression = null)
        {
            ObjectContext = objectContext;
            Expression = expression ?? Expression.Constant(this);
        }

        /// <summary>
        ///     构造对象集实例
        /// </summary>
        /// <param name="objectContext">对象上下文</param>
        /// <param name="expression">表达式</param>
        /// <param name="provider">查询提供者</param>
        internal ObjectSet(ObjectContext objectContext, IQueryProvider provider, Expression expression = null)
        {
            ObjectContext = objectContext;
            _provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }

        /// <summary>
        ///     计算对象集中对象的个数。
        /// </summary>
        public int Count
        {
            get
            {
                var thisIQueryable = this as IQueryable<T>;
                return thisIQueryable.Count();
            }
        }

        /// <summary>
        ///     获取定义当前对象集的对象上下文
        /// </summary>
        public ObjectContext ObjectContext { get; }

        /// <summary>
        ///     返回循环访问集合的枚举数
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }


        /// <summary>
        ///     返回循环访问集合的枚举数
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        /// <summary>
        ///     表达式
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        ///     元素类型
        /// </summary>
        public Type ElementType => typeof(T);

        /// <summary>
        ///     查询提供者
        /// </summary>
        public IQueryProvider Provider => _provider ?? (_provider = ObjectContext.ConfigProvider.QueryProvider);

        /// <summary>
        ///     将指定的对象作为新对象附加到对象上下文。
        /// </summary>
        /// <param name="obj">要附加的对象。</param>
        public void Attach(T obj)
        {
            if (!ObjectContext.Attached(obj))
                ObjectContext.Attach(ref obj, true, true);
        }

        /// <summary>
        ///     使用无参构造函数创建对象的新实例并附加到上下文
        ///     默认使用HasNewInstanceConstructor配置的新实例构造函数 未配置时使用HasConstructor配置的构造函数
        /// </summary>
        /// <returns></returns>
        public T Create()
        {
            var structuralType = ObjectContext.Model.GetStructuralType(typeof(T));
            var constructor = structuralType.NewInstanceConstructor ?? structuralType.Constructor;
            //创建对象
            var obj = (T)constructor.Construct();
            //附加到上下文
            Attach(obj);
            return obj;
        }

        /// <summary>
        ///     使用参数创建对象的新实例并附加到上下文
        ///     默认使用HasNewInstanceConstructor配置的新实例构造函数 未配置时使用HasConstructor配置的构造函数
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public T Create(params object[] parameters)
        {
            var structuralType = ObjectContext.Model.GetStructuralType(typeof(T));
            var constructor = structuralType.NewInstanceConstructor ?? structuralType.Constructor;
            //创建对象
            var obj = (T)constructor.Construct(parameters);
            //附加到上下文
            Attach(obj);
            return obj;
        }


        /// <summary>
        ///     将指定的对象标记为已删除。（标记删除（SaveChanges时才真正删除））
        /// </summary>
        /// <param name="obj">要标记为已删除的对象。</param>
        public void Remove(T obj)
        {
            ObjectContext.Remove(obj);
        }

        /// <summary>
        ///     根据传入的筛选条件即时删除对象
        ///     <para>注意:此方法不会探测关联对象,仅删除符合条件的当前对象集合内对象</para>
        /// </summary>
        /// <param name="filterExpression">筛选表达式</param>
        /// <returns></returns>
        public int RemoveDirectly(Expression<Func<T, bool>> filterExpression)
        {
            return Delete(filterExpression);
        }

        /// <summary>
        ///     根据传入的筛选条件即时删除对象
        ///     <para>注意:此方法不会探测关联对象,仅删除符合条件的当前对象集合内对象,不会有对象追踪也不会处理关联对象</para>
        /// </summary>
        /// <param name="filterExpression">筛选条件</param>
        public int Delete(Expression<Func<T, bool>> filterExpression)
        {
            //没有条件 返回
            if (filterExpression == null) return 0;
            //删除
            return ObjectContext.ConfigProvider.SavingProvider.Delete(ObjectContext.Model.GetObjectType(typeof(T)),
                filterExpression);
        }


        /// <summary>
        ///     为符合条件的对象的属性即时设置新值。
        ///     <para>注意:此方法仅能修改符合条件的当前对象集合内对象,不会有对象追踪也不会处理关联对象</para>
        /// </summary>
        /// <param name="newValues">存储属性新值的键值对集合，其中键为属性名称，值为属性的新值。</param>
        /// <param name="filterExpression">筛选条件</param>
        public int SetAttributes(KeyValuePair<string, object>[] newValues, Expression<Func<T, bool>> filterExpression)
        {
            //没有条件 返回
            if (filterExpression == null) return 0;
            //设值值
            return ObjectContext.ConfigProvider.SavingProvider.SetAttributes(
                ObjectContext.Model.GetObjectType(typeof(T)), filterExpression,
                newValues);
        }

        /// <summary>
        ///     为符合条件的对象的属性即时设置新值，其中新值为原值加上增量值。属性必须为数值类型。
        ///     <para>注意:此方法仅能修改符合条件的当前对象集合内对象,不会有对象追踪也不会处理关联对象</para>
        /// </summary>
        /// <param name="increaseValues">存储增量值的键值对集合，其中键为属性名称，值为增量值。</param>
        /// <param name="filterExpression">筛选条件</param>
        public int IncreaseAttributes(KeyValuePair<string, object>[] increaseValues,
            Expression<Func<T, bool>> filterExpression)
        {
            //没有条件 返回
            if (filterExpression == null) return 0;
            return ObjectContext.ConfigProvider.SavingProvider.IncreaseAttributes(
                ObjectContext.Model.GetObjectType(typeof(T)),
                filterExpression, increaseValues);
        }
    }

    /// <summary>
    ///     对象集扩展
    /// </summary>
    public static class ObjectSetExtensions
    {
        /// <summary>
        ///     Include 方法 根据MemberAccess表达式进行包含
        /// </summary>
        /// <typeparam name="T">上级查询实体类型</typeparam>
        /// <typeparam name="TProperty">一或多级MemberAccess</typeparam>
        /// <param name="source">上级查询</param>
        /// <param name="path">表达式表示的关联路径</param>
        /// <returns></returns>
        public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> source,
            Expression<Func<T, TProperty>> path)
        {
            var methods = typeof(ObjectSetExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var methodInfo = methods.FirstOrDefault(p => p.Name == "Include" && p.GetGenericArguments().Length == 2);
            //找到此方法
            if (methodInfo == null) throw new ArgumentException("ObjectSet扩展不包含符合条件的Include方法", nameof(methodInfo));
            //加入泛型参数
            methodInfo = methodInfo.MakeGenericMethod(typeof(T), typeof(TProperty));

            var result = Expression.Call(methodInfo, source.Expression, Expression.Quote(path));
            return source.Provider.CreateQuery<T>(result);
        }

        /// <summary>
        ///     Include 方法 根据关联路径字符串进行包含
        /// </summary>
        /// <typeparam name="T">上级查询实体类型</typeparam>
        /// <param name="source">上级查询</param>
        /// <param name="associationPath">字符串表示的关联路径 形如A内有B B内有C 则为B.C</param>
        /// <returns></returns>
        public static IQueryable<T> Include<T>(this IQueryable<T> source,
            string associationPath)
        {
            var methods = typeof(ObjectSetExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public);
            var methodInfo = methods.FirstOrDefault(p => p.Name == "Include" && p.GetGenericArguments().Length == 1);
            //找到此方法
            if (methodInfo == null) throw new ArgumentException("ObjectSet扩展不包含符合条件的Include方法", nameof(methodInfo));
            //加入泛型参数
            methodInfo = methodInfo.MakeGenericMethod(typeof(T));

            var result = Expression.Call(methodInfo, source.Expression, Expression.Constant(associationPath));
            return source.Provider.CreateQuery<T>(result);
        }

        /// <summary>
        ///     WhereIf方法 根据是否符合条件拼接Where表达式
        /// </summary>
        /// <typeparam name="T">上级查询实体类型</typeparam>
        /// <param name="source">上级查询</param>
        /// <param name="predicate">表达式</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }
    }
}