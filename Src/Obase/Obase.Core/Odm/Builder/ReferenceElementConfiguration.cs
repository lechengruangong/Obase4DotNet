/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联元素配置项,为关联引用配置项和关联端配置项提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:59:44
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     为关联引用配置项和关联端配置项提供基础实现。
    /// </summary>
    /// <typeparam name="TObject">关联型或关联端的类型</typeparam>
    /// <typeparam name="TConfiguration">具体的配置项类型</typeparam>
    public abstract class
        ReferenceElementConfiguration<TObject, TConfiguration> :
        TypeElementConfiguration<TObject, TConfiguration>,
        IReferenceElementConfigurator,
        ILazyLoadingConfiguration
        where TObject : class
        where TConfiguration : ReferenceElementConfiguration<TObject, TConfiguration>
    {
        /// <summary>
        ///     是否启用延迟加载
        /// </summary>
        protected bool _enableLazyLoading;

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        protected int _loadingPriority;

        /// <summary>
        ///     加载触发器
        /// </summary>
        protected List<IBehaviorTrigger> LoadingTriggers;

        /// <summary>
        ///     创建类型元素配置项实例
        /// </summary>
        /// <param name="name">元素（关联引用、关联端）名称</param>
        /// <param name="isMultiple">指示元素是否具有多重性，即其值是否为集合。</param>
        /// <param name="typeConfiguration">创建当前引用元素配置项的类型配置项的类型。</param>
        protected ReferenceElementConfiguration(string name, bool isMultiple,
            StructuralTypeConfiguration typeConfiguration) :
            base(name, isMultiple, typeConfiguration)
        {
        }

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        internal int LoadingPriority => _loadingPriority;

        /// <summary>
        ///     显式实现是否使用延迟加载
        /// </summary>
        int ILazyLoadingConfiguration.LoadingPriority => LoadingPriority;

        /// <summary>
        ///     显式实现延迟加载的优先级
        /// </summary>
        bool ILazyLoadingConfiguration.EnableLazyLoading => EnableLazyLoading;

        /// <summary>
        ///     是否启用延迟加载
        /// </summary>
        public bool EnableLazyLoading => _enableLazyLoading;

        /// <summary>
        ///     设置是否支持延迟加载。
        /// </summary>
        /// <param name="enableLazyLoading">是否启用延迟加载</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IReferenceElementConfigurator.HasEnableLazyLoading(bool enableLazyLoading, bool overrided)
        {
            //覆盖的 则直接设置
            if (overrided)
                HasEnableLazyLoading(enableLazyLoading);
        }

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        /// <param name="loadingPriority">加载优先级。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IReferenceElementConfigurator.HasLoadingPriority(int loadingPriority, bool overrided)
        {
            //覆盖的 则直接设置
            if (overrided)
                HasLoadingPriority(loadingPriority);
        }

        /// <summary>
        ///     设置加载触发器。
        ///     每次调用本方法将追加一个加载触发器。
        /// </summary>
        /// <param name="loadingTrigger">加载触发器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IReferenceElementConfigurator.HasLoadingTrigger(IBehaviorTrigger loadingTrigger, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
            {
                if (LoadingTriggers == null)
                    LoadingTriggers = new List<IBehaviorTrigger>();
                LoadingTriggers.Clear();
            }

            HasLoadingTrigger(loadingTrigger);
        }

        /// <summary>
        ///     使用一个能触发引用加载的方法为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发引用加载的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IReferenceElementConfigurator.HasLoadingTrigger(MethodInfo method, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
            {
                if (LoadingTriggers == null)
                    LoadingTriggers = new List<IBehaviorTrigger>();
                LoadingTriggers.Clear();
            }

            HasLoadingTrigger(method);
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IReferenceElementConfigurator.HasLoadingTrigger(PropertyInfo property, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
            {
                if (LoadingTriggers == null)
                    LoadingTriggers = new List<IBehaviorTrigger>();
                LoadingTriggers.Clear();
            }

            HasLoadingTrigger(property, EBehaviorTriggerType.PropertyGet);
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IReferenceElementConfigurator.HasLoadingTrigger(PropertyInfo property, EBehaviorTriggerType triggerType,
            bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
            {
                if (LoadingTriggers == null)
                    LoadingTriggers = new List<IBehaviorTrigger>();
                LoadingTriggers.Clear();
            }

            HasLoadingTrigger(property, triggerType);
        }

        /// <summary>
        ///     使用一个能触发引用加载的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IReferenceElementConfigurator.HasLoadingTrigger(string memberName, EBehaviorTriggerType triggerType,
            bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
            {
                if (LoadingTriggers == null)
                    LoadingTriggers = new List<IBehaviorTrigger>();
                LoadingTriggers.Clear();
            }

            HasLoadingTrigger(memberName, triggerType);
        }

        /// <summary>
        ///     使用与引用元素同名的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(EBehaviorTriggerType triggerType, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
            {
                if (LoadingTriggers == null)
                    LoadingTriggers = new List<IBehaviorTrigger>();
                LoadingTriggers.Clear();
            }

            HasLoadingTrigger(_name, triggerType);
        }

        /// <summary>
        ///     使用与引用元素同名的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
            {
                if (LoadingTriggers == null)
                    LoadingTriggers = new List<IBehaviorTrigger>();
                LoadingTriggers.Clear();
            }

            HasLoadingTrigger(_name, EBehaviorTriggerType.PropertyGet);
        }

        /// <summary>
        ///     使用一个能触发引用加载的方法为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发引用加载的方法。</param>
        public TConfiguration HasLoadingTrigger(MethodInfo method)
        {
            var methodTrigger = new MethodTrigger(method);
            return HasLoadingTrigger(methodTrigger);
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        public TConfiguration HasLoadingTrigger(PropertyInfo property)
        {
            return HasLoadingTrigger(property, EBehaviorTriggerType.PropertyGet);
        }

        /// <summary>
        ///     设置加载触发器。
        ///     每次调用本方法将追加一个加载触发器。
        /// </summary>
        /// <param name="loadingTrigger">加载触发器</param>
        public TConfiguration HasLoadingTrigger(
            IBehaviorTrigger loadingTrigger)
        {
            //没有加载触发器列表，则创建一个新的列表
            if (LoadingTriggers == null)
                LoadingTriggers = new List<IBehaviorTrigger>();
            //加入加载触发器列表
            LoadingTriggers.Add(loadingTrigger);
            return (TConfiguration)this;
        }

        /// <summary>
        ///     使用一个能触发引用加载的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        public TConfiguration HasLoadingTrigger(string memberName,
            EBehaviorTriggerType triggerType)
        {
            var property = typeof(TObject).GetProperty(memberName);
            return HasLoadingTrigger(property, triggerType);
        }

        /// <summary>
        ///     使用与引用元素同名的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        public TConfiguration HasLoadingTrigger<TProperty>(
            EBehaviorTriggerType triggerType = EBehaviorTriggerType.PropertyGet)
        {
            return HasLoadingTrigger(typeof(TProperty).Name, triggerType);
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        ///     类型参数：
        ///     TProperty  属性访问器的类型
        /// </summary>
        /// <param name="propertyExp">表示属性访问器的Lambda表达式。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        public TConfiguration HasLoadingTrigger<TProperty>(
            Expression<Func<TObject, TProperty>> propertyExp,
            EBehaviorTriggerType triggerType = EBehaviorTriggerType.PropertyGet) where TProperty : class
        {
            //解析表达式
            var member = (MemberExpression)propertyExp.Body;
            return HasLoadingTrigger(member.Member.Name, triggerType);
        }


        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        public TConfiguration HasLoadingTrigger(
            PropertyInfo property, EBehaviorTriggerType triggerType)
        {
            MethodInfo method;
            switch (triggerType)
            {
                case EBehaviorTriggerType.Method:
                    throw new ArgumentException("方法型触发器不能用PropertyInfo构造.");
                case EBehaviorTriggerType.PropertyGet:
                    method = property.GetMethod;
                    break;
                case EBehaviorTriggerType.PropertySet:
                    method = property.SetMethod;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(triggerType), triggerType,
                        $"未知的行为触发器的类型{triggerType}");
            }

            return HasLoadingTrigger(method);
        }

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        /// <param name="loadingPriority">加载优先级。</param>
        public TConfiguration HasLoadingPriority(
            int loadingPriority)
        {
            _loadingPriority = loadingPriority;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置是否支持延迟加载。
        /// </summary>
        /// <param name="enableLazyLoading">是否启用延迟加载</param>
        public TConfiguration HasEnableLazyLoading(
            bool enableLazyLoading)
        {
            _enableLazyLoading = enableLazyLoading;
            return (TConfiguration)this;
        }
    }
}