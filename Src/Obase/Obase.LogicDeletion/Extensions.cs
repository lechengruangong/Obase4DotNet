/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除的拓展方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:23:03
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除的拓展方法
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     逻辑删除方法
        /// </summary>
        /// <typeparam name="T">源类型</typeparam>
        /// <param name="objectSet">数据集</param>
        /// <param name="obj">对象</param>
        public static void RemoveLogically<T>(this ObjectSet<T> objectSet, T obj)
        {
            ChangeLogicalDeletionState(objectSet, obj, true);
        }


        /// <summary>
        ///     逻辑删除恢复方法
        /// </summary>
        /// <typeparam name="T">源类型</typeparam>
        /// <param name="objectSet">数据集</param>
        /// <param name="obj">对象</param>
        public static void RecoveryLogically<T>(this ObjectSet<T> objectSet, T obj)
        {
            ChangeLogicalDeletionState(objectSet, obj, false);
        }

        /// <summary>
        ///     改变逻辑删除状态
        /// </summary>
        /// <typeparam name="T">源类型</typeparam>
        /// <param name="objectSet">数据集</param>
        /// <param name="obj">对象</param>
        /// <param name="value">是否逻辑删除</param>
        private static void ChangeLogicalDeletionState<T>(ObjectSet<T> objectSet, T obj, bool value)
        {
            var structuralType = objectSet.ObjectContext.Model.GetStructuralType(typeof(T));
            var ext = structuralType.GetExtension<LogicDeletionExtension>();
            if (ext == null)
                throw new ArgumentException("此类型未进行逻辑删除配置");

            var attr = string.IsNullOrEmpty(ext.DeletionMark)
                ? structuralType.GetAttribute("obase_gen_deletionMark")
                : structuralType.GetAttribute(ext.DeletionMark);

            try
            {
                attr.SetValue(obj, value);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"无法处理{typeof(T)}的逻辑删除,请检查此对象是否为上下文查出的对象", e);
            }
        }

        /// <summary>
        ///     逻辑直接删除
        /// </summary>
        /// <typeparam name="T">源类型</typeparam>
        /// <param name="objectSet">数据集</param>
        /// <param name="filter">过滤条件</param>
        /// <returns></returns>
        public static int DeleteLogically<T>(this ObjectSet<T> objectSet, Expression<Func<T, bool>> filter)
        {
            return ChangeLogicalDeletionState(objectSet, filter, true);
        }

        /// <summary>
        ///     逻辑直接删除恢复
        /// </summary>
        /// <typeparam name="T">源类型</typeparam>
        /// <param name="objectSet">数据集</param>
        /// <param name="filter">过滤条件</param>
        /// <returns></returns>
        public static int RecoveryLogically<T>(this ObjectSet<T> objectSet, Expression<Func<T, bool>> filter)
        {
            return ChangeLogicalDeletionState(objectSet, filter, false);
        }

        /// <summary>
        ///     直接改变逻辑删除状态
        /// </summary>
        /// <typeparam name="T">源类型</typeparam>
        /// <param name="objectSet">数据集</param>
        /// <param name="filter">过滤条件</param>
        /// <param name="value">是否逻辑删除</param>
        /// <returns></returns>
        private static int ChangeLogicalDeletionState<T>(ObjectSet<T> objectSet, Expression<Func<T, bool>> filter,
            bool value)
        {
            var structuralType = objectSet.ObjectContext.Model.GetStructuralType(typeof(T));
            var ext = structuralType.GetExtension<LogicDeletionExtension>();
            if (ext == null)
                throw new ArgumentException("此类型未进行逻辑删除配置");

            var deletionField = string.IsNullOrEmpty(ext.DeletionField)
                ? structuralType.GetAttribute(ext.DeletionMark).TargetField
                : ext.DeletionField;

            return objectSet.SetAttributes(new[] { new KeyValuePair<string, object>(deletionField, value) },
                filter);
        }

        /// <summary>
        ///     启用逻辑删除
        /// </summary>
        /// <param name="context">对象上下文</param>
        public static void EnableLogicDeletion(this ObjectContext context)
        {
            context.RegisterModule(new LogicDeletionModule());
        }
    }
}