/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象类型配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:44:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     为实体型配置项、关联型配置项提供基础实现
    /// </summary>
    /// <typeparam name="TObject">配置的对象类型</typeparam>
    /// <typeparam name="TConfiguration">具体的配置项类型</typeparam>
    public abstract class
        ObjectTypeConfiguration<TObject, TConfiguration> : StructuralTypeConfiguration<TObject, TConfiguration>,
        IObjectTypeConfigurator
        where TConfiguration : ObjectTypeConfiguration<TObject, TConfiguration>
    {
        /// <summary>
        ///     映射表
        /// </summary>
        protected string _targetTable;

        /// <summary>
        ///     并发冲突处理策略。
        /// </summary>
        protected EConcurrentConflictHandlingStrategy ConcurrentConflictHandlingStrategy =
            EConcurrentConflictHandlingStrategy.ThrowException;

        /// <summary>
        ///     设置对象变更通知包含的属性。
        /// </summary>
        protected List<string> NoticeAttributes;

        /// <summary>
        ///     指示是否发送对象创建通知
        /// </summary>
        protected bool NotifyCreation;

        /// <summary>
        ///     指示是否发送对象删除通知
        /// </summary>
        protected bool NotifyDeletion;

        /// <summary>
        ///     指示是否发送对象更新通知
        /// </summary>
        protected bool NotifyUpdate;

        /// <summary>
        ///     对象在关系数据库中的存储顺序（排序规则）
        /// </summary>
        protected List<OrderExpression> StoringOrder;

        /// <summary>
        ///     版本标识属性集（版本键）。
        /// </summary>
        protected List<string> VersionAttributes;

        /// <summary>
        ///     创建ObjectTypeConfiguration的实例。
        /// </summary>
        /// <param name="modelBuilder">指定类型配置项所属的建模器</param>
        protected ObjectTypeConfiguration(ModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        /// <summary>
        ///     映射表
        /// </summary>
        public string TargetTable => _targetTable;

        /// <summary>
        ///     设置要包含在对象变更通知中的属性。
        /// </summary>
        /// <param name="noticeAttributes">要包含的属性的名称的集合。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasNoticeAttributes(string[] noticeAttributes, bool overrided)
        {
            //覆盖的 直接设置
            if (overrided)
            {
                HasNoticeAttributes(new List<string>(noticeAttributes));
            }
            else
            {
                //不覆盖的 先检查传入的属性是否都存在
                foreach (var noticeAttribute in noticeAttributes)
                    if (_clrType.GetProperty(noticeAttribute) == null)
                        throw new ArgumentException($"{_clrType.Name}内找不到属性{noticeAttribute},无法配置变更通知属性.",
                            nameof(noticeAttribute));

                if (NoticeAttributes == null)
                    NoticeAttributes = new List<string>();
                foreach (var noticeAttribute in noticeAttributes)
                    //如果NoticeAttributes中不存在该属性，则添加
                    if (!NoticeAttributes.Contains(noticeAttribute))
                        NoticeAttributes.Add(noticeAttribute);
            }
        }

        /// <summary>
        ///     设置一个值，该值指示对象创建时是否发送通知。
        /// </summary>
        /// <param name="notifyCreation">指示是否发送对象创建通知。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IObjectTypeConfigurator.HasNotifyCreation(bool notifyCreation, bool overrided)
        {
            //如果覆盖的，则直接设置
            if (overrided)
                HasNotifyCreation(notifyCreation);
        }

        /// <summary>
        ///     设置一个值，该值指示对象删除时是否发送通知。
        /// </summary>
        /// <param name="notifyDeletion">指示是否发送对象删除通知。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IObjectTypeConfigurator.HasNotifyDeletion(bool notifyDeletion, bool overrided)
        {
            //如果覆盖的，则直接设置
            if (overrided)
                HasNotifyDeletion(notifyDeletion);
        }

        /// <summary>
        ///     设置一个值，该值指示对象更新时是否发送通知。
        /// </summary>
        /// <param name="notifyUpdate">指示是否发送对象更新通知。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IObjectTypeConfigurator.HasNotifyUpdate(bool notifyUpdate, bool overrided)
        {
            //如果覆盖的，则直接设置
            if (overrided)
                HasNotifyUpdate(notifyUpdate);
        }

        /// <summary>
        ///     设置版本标识属性集（版本键）。每调用一次本方法将追加一个版本标识属性。
        /// </summary>
        /// <param name="attribute">属性的名称。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IObjectTypeConfigurator.HasVersionAttribute(string attribute, bool overrided)
        {
            //如果覆盖的，则清除既有配置
            if (overrided)
            {
                if (VersionAttributes == null)
                    VersionAttributes = new List<string>();
                VersionAttributes.Clear();
            }

            HasVersionAttribute(attribute);
        }

        /// <summary>
        ///     设置映射表。
        /// </summary>
        /// <param name="table">映射表的名称。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IObjectTypeConfigurator.ToTable(string table, bool overrided)
        {
            //如果覆盖的，则直接设置
            if (overrided)
            {
                ToTable(table);
            }
            else
            {
                //不覆盖的 没设置过的才设置
                if (string.IsNullOrEmpty(TargetTable))
                    ToTable(table);
            }
        }

        /// <summary>
        ///     获取行为触发器触发的对象行为所涉及到的元素。
        /// </summary>
        /// <param name="trigger">指定的触发器实例。</param>
        ITypeElementConfigurator[] IObjectTypeConfigurator.GetBehaviorElements(IBehaviorTrigger trigger)
        {
            return GetBehaviorElements(trigger).Select(p => p as ITypeElementConfigurator).ToArray();
        }

        /// <summary>
        ///     设置并发冲突处理策略。
        /// </summary>
        /// <param name="strategy">冲突处理策略。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IObjectTypeConfigurator.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy strategy,
            bool overrided)
        {
            if (overrided)
            {
                HasConcurrentConflictHandlingStrategy(strategy);
            }
            else
            {
                if (ConcurrentConflictHandlingStrategy == EConcurrentConflictHandlingStrategy.ThrowException)
                    HasConcurrentConflictHandlingStrategy(strategy);
            }
        }

        /// <summary>
        ///     设置要包含在对象变更通知中的属性。
        /// </summary>
        /// <param name="noticeAttributes">要包含的属性的名称的集合。</param>
        public TConfiguration HasNoticeAttributes(List<string> noticeAttributes)
        {
            //检查传入的属性是否都存在
            foreach (var noticeAttribute in noticeAttributes)
                if (_clrType.GetProperty(noticeAttribute) == null)
                    throw new ArgumentException($"{_clrType.Name}内找不到属性{noticeAttribute},无法配置变更通知属性.",
                        nameof(noticeAttribute));
            //赋值
            NoticeAttributes = noticeAttributes;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置所有属性为要包含在对象变更通知中的属性
        ///     注意此方法会覆盖HasNoticeAttributes设置的属性
        /// </summary>
        /// <returns></returns>
        public TConfiguration HasNoticeAttributes()
        {
            //将所有属性设置为要包含在对象变更通知中的属性
            var allAttr = _clrType.GetProperties();
            NoticeAttributes = allAttr.Select(attr => attr.Name).ToList();
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置一个值，该值指示对象创建时是否发送通知。
        /// </summary>
        /// <param name="notifyCreation">指示是否发送对象创建通知。</param>
        public TConfiguration HasNotifyCreation(bool notifyCreation)
        {
            NotifyCreation = notifyCreation;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置一个值，该值指示对象删除时是否发送通知。
        /// </summary>
        /// <param name="notifyDeletion">指示是否发送对象删除通知。</param>
        public TConfiguration HasNotifyDeletion(bool notifyDeletion)
        {
            NotifyDeletion = notifyDeletion;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置一个值，该值指示对象更新时是否发送通知。
        /// </summary>
        /// <param name="notifyUpdate">指示是否发送对象更新通知。</param>
        public TConfiguration HasNotifyUpdate(bool notifyUpdate)
        {
            NotifyUpdate = notifyUpdate;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置映射表。
        /// </summary>
        /// <param name="table">映射表的名称。</param>
        public TConfiguration ToTable(string table)
        {
            _targetTable = table;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     根据lambda表达式包含的信息设置对象在关系数据库中的存储顺序。
        ///     注：该方法调用一次即追加一条排序规则。
        /// </summary>
        /// <param name="expression">
        ///     一个lambda表达式，用于指定要作为排序依据的属性。
        ///     注：如果该表达式指向一个关联端，则表示该关联端的所有标识属性都作为排序依据；如果指向某个关联端的某个标识属性，则表示该标识属性将作为排序依据；如果指向关联类的某
        ///     个属性，则该属性将作为排序依据。
        /// </param>
        public TConfiguration HasStoringOrder<TResult>(Expression<Func<TObject, TResult>> expression)
            where TResult : struct
        {
            if (StoringOrder == null)
                StoringOrder = new List<OrderExpression>();
            StoringOrder.Add(new OrderExpression { Expression = (MemberExpression)expression.Body, Inverted = true });
            return (TConfiguration)this;
        }

        /// <summary>
        ///     根据lambda表达式包含的信息设置对象在关系数据库中的存储顺序，同时指定是否倒序排列。
        ///     注：该方法调用一次即追加一条排序规则。
        /// </summary>
        /// <param name="expression">
        ///     一个lambda表达式，用于指定要作为排序依据的属性。
        ///     注：如果该表达式指向一个关联端，则表示该关联端的所有标识属性都作为排序依据；如果指向某个关联端的某个标识属性，则表示该标识属性将作为排序依据；如果指向关联类的某
        ///     个属性，则该属性将作为排序依据。
        /// </param>
        /// <param name="inverted">指示是否倒序（即降序）排列。</param>
        public TConfiguration HasStoringOrder<TResult>(Expression<Func<TObject, TResult>> expression,
            bool inverted) where TResult : struct
        {
            if (StoringOrder == null)
                StoringOrder = new List<OrderExpression>();
            StoringOrder.Add(
                new OrderExpression { Expression = (MemberExpression)expression.Body, Inverted = inverted });
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置并发冲突处理策略。
        /// </summary>
        /// <param name="strategy">冲突处理策略。</param>
        public TConfiguration HasConcurrentConflictHandlingStrategy(
            EConcurrentConflictHandlingStrategy strategy)
        {
            ConcurrentConflictHandlingStrategy = strategy;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置版本标识属性集（版本键）。每调用一次本方法将追加一个版本标识属性。
        /// </summary>
        /// <param name="attribute">属性的名称。</param>
        public TConfiguration HasVersionAttribute(string attribute)
        {
            if (_clrType.GetProperty(attribute) == null)
                throw new ArgumentException($"{_clrType.Name}内找不到属性{attribute},无法配置版本标识属性.", nameof(attribute));
            if (VersionAttributes == null) VersionAttributes = new List<string>();
            //如果VersionAttributes中不存在该属性，则添加
            if (VersionAttributes.IndexOf(attribute) == -1) VersionAttributes.Add(attribute);
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置版本标识属性集（版本键）。每调用一次本方法将追加一个版本标识属性。
        /// </summary>
        /// <param name="expression">表示属性的Lambda表达式。</param>
        public TConfiguration HasVersionAttribute<TAttribute>(
            Expression<Func<TObject, TAttribute>> expression)
        {
            if (expression.Body is MemberExpression member) return HasVersionAttribute(member.Member.Name);
            throw new ArgumentException("不能使用非属性访问表达式配置版本标识属性");
        }

        /// <summary>
        ///     根据类型配置项中的元数据配置模型类型，被配置的模型类型已根据当前类型配置项实例生成并已注册到指定的模型中。
        ///     注：调用方调用Create方法创建模型类型时，由于类型的元素还未创建，因此某些属性可能无法当场配置，可以等到类型元素创建（CreateElement被调用）完成
        ///     时，调用本方法完成类型配置。
        /// </summary>
        /// <param name="model">要配置的类型所属的模型。</param>
        internal override void Configurate(ObjectDataModel model)
        {
            //获取当前类型
            var modelType = model.GetStructuralType(_clrType);

            modelType.Namespace = _clrType.Namespace;

            //关联型 处理关联端
            if (modelType is AssociationType ass)
            {
                //隐式关联型 默认关闭关联端的延迟加载
                if (!ass.Visible) ass.AssociationEnds.ForEach(s => s.EnableLazyLoading = false);
                //配置版本键和冲突处理策略
                ass.VersionAttributes = VersionAttributes;
                ass.ConcurrentConflictHandlingStrategy = ConcurrentConflictHandlingStrategy;
            }
            //实体型 处理自增列
            else if (modelType is EntityType ent)
            {
                //主要为了走一遍Set方法
                ent.KeyIsSelfIncreased = ent.KeyIsSelfIncreased;
                ent.KeyAttributes = ent.KeyAttributes;
                //配置版本键和冲突处理策略
                ent.VersionAttributes = VersionAttributes;
                ent.ConcurrentConflictHandlingStrategy = ConcurrentConflictHandlingStrategy;
            }
        }
    }
}