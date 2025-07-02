/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：提供访问对象上下文的快捷方式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:59:29
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Obase.Core
{
    /// <summary>
    ///     提供访问对象上下文的快捷方式，每次访问均会创建新的上下文实例。
    /// </summary>
    public static class ObaseGo
    {
        /// <summary>
        ///     应用程序域内默认的对象上下文类型。
        /// </summary>
        private static Type _defaultContextType;

        /// <summary>
        ///     获取一个新的对象上下文。
        /// </summary>
        public static ObjectContext ObjectContext => _defaultContextType == null
            ? throw new ArgumentException("未设置应用程序域内默认的对象上下文类型")
            : (ObjectContext)Activator.CreateInstance(_defaultContextType);

        /// <summary>
        ///     创建对象上下文，然后在其中创建一个对象集
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="objectContext">返回新建的对象上下文</param>
        /// <returns></returns>
        public static ObjectSet<T> CreateObjectSet<T>(out ObjectContext objectContext) where T : class
        {
            var current = ObjectContext;
            objectContext = current;
            return current.CreateSet<T>();
        }

        /// <summary>
        ///     创建一个对象上下文，基于它根据传入的筛选条件删除对象。
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="filterExpression">筛选条件</param>
        /// <returns>删除的对象数</returns>
        public static int Delete<T>(Expression<Func<T, bool>> filterExpression) where T : class
        {
            return ObjectContext.CreateSet<T>().Delete(filterExpression);
        }

        /// <summary>
        ///     创建一个对象上下文，基于它根据筛选条件为对象的属性设置新值，其中新值为原值加上增量值。属性必须为数值类型。
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="increaseValues">存储增量值的键值对集合，其中键为属性名称，值为增量值。</param>
        /// <param name="filterExpression">筛选条件</param>
        /// <returns></returns>
        public static int IncreaseAttributes<T>(KeyValuePair<string, object>[] increaseValues,
            Expression<Func<T, bool>> filterExpression) where T : class
        {
            return ObjectContext.CreateSet<T>().IncreaseAttributes(increaseValues, filterExpression);
        }

        /// <summary>
        ///     创建一个对象上下文，基于它对指定的新对象实施持久化。
        /// </summary>
        /// <typeparam name="T">新对象的类型</typeparam>
        /// <param name="obj">要保存的对象</param>
        public static void SaveNew<T>(T obj) where T : class
        {
            var context = ObjectContext;
            context.Attach(obj);
            context.SaveChanges();
        }

        /// <summary>
        ///     创建一个对象上下文，基于它根据筛选条件为对象的属性设置新值。
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="newValues">存储属性新值的键值对集合，其中键为属性名称，值为属性的新值</param>
        /// <param name="filterExpression">筛选条件</param>
        /// <returns>修改的对象数</returns>
        public static int SetAttributes<T>(KeyValuePair<string, object>[] newValues,
            Expression<Func<T, bool>> filterExpression) where T : class
        {
            return ObjectContext.CreateSet<T>().SetAttributes(newValues, filterExpression);
        }

        /// <summary>
        ///     设置应用程序域内默认的对象上下文类型。
        ///     说明
        ///     在应用程序的整个生命周期内只能调用一次。
        /// </summary>
        public static void SetDefault<TContext>() where TContext : ObjectContext, new()
        {
            if (_defaultContextType != null)
                throw new InvalidOperationException($"已设置默认的对象上下文类型{_defaultContextType.FullName}");

            if (_defaultContextType == null)
                _defaultContextType = typeof(TContext);
        }
    }

    /// <summary>
    ///     提供访问对象上下文的快捷方式，每次访问均会创建新的上下文实例
    /// </summary>
    /// <typeparam name="TContext">对象上下文的类型</typeparam>
    public class ObaseGo<TContext> where TContext : ObjectContext, new()
    {
        /// <summary>
        ///     获取一个新的对象上下文。
        /// </summary>
        public ObjectContext ObjectContext => (ObjectContext)Activator.CreateInstance(typeof(TContext));

        /// <summary>
        ///     创建对象上下文，然后在其中创建一个对象集
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="objectContext">返回新建的对象上下文</param>
        /// <returns></returns>
        public ObjectSet<T> CreateObjectSet<T>(out ObjectContext objectContext) where T : class
        {
            var current = ObjectContext;
            objectContext = current;
            return current.CreateSet<T>();
        }

        /// <summary>
        ///     创建一个对象上下文，基于它根据传入的筛选条件删除对象。
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="filterExpression">筛选条件</param>
        /// <returns>删除的对象数</returns>
        public int Delete<T>(Expression<Func<T, bool>> filterExpression) where T : class
        {
            return ObjectContext.CreateSet<T>().Delete(filterExpression);
        }

        /// <summary>
        ///     创建一个对象上下文，基于它根据筛选条件为对象的属性设置新值，其中新值为原值加上增量值。属性必须为数值类型。
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="increaseValues">存储增量值的键值对集合，其中键为属性名称，值为增量值。</param>
        /// <param name="filterExpression">筛选条件</param>
        /// <returns>修改的对象数</returns>
        public int IncreaseAttributes<T>(KeyValuePair<string, object>[] increaseValues,
            Expression<Func<T, bool>> filterExpression) where T : class
        {
            return ObjectContext.CreateSet<T>().IncreaseAttributes(increaseValues, filterExpression);
        }

        /// <summary>
        ///     创建一个对象上下文，基于它对指定的新对象实施持久化。
        /// </summary>
        /// <typeparam name="T">新对象的类型</typeparam>
        /// <param name="obj">要保存的对象</param>
        public void SaveNew<T>(T obj) where T : class
        {
            var context = ObjectContext;
            context.Attach(obj);
            context.SaveChanges();
        }

        /// <summary>
        ///     创建一个对象上下文，基于它根据筛选条件为对象的属性设置新值。
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="newValues">存储属性新值的键值对集合，其中键为属性名称，值为属性的新值</param>
        /// <param name="filterExpression">筛选条件</param>
        /// <returns>修改的对象数</returns>
        public int SetAttributes<T>(KeyValuePair<string, object>[] newValues,
            Expression<Func<T, bool>> filterExpression) where T : class
        {
            return ObjectContext.CreateSet<T>().SetAttributes(newValues, filterExpression);
        }
    }
}