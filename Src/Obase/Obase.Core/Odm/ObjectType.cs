/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象类型,包括实体型和关联型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:31:23
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     为对象类型提供基础实现，是实体型和关联型的基类。
    /// </summary>
    public abstract class ObjectType : ReferringType, IMappable
    {
        /// <summary>
        ///     并发冲突处理策略。
        /// </summary>
        protected EConcurrentConflictHandlingStrategy _concurrentConflictHandlingStrategy;


        /// <summary>
        ///     指定要包含在对象变更通知中的属性。
        /// </summary>
        protected List<string> _noticeAttributes;

        /// <summary>
        ///     指示对象创建时是否发送通知。
        /// </summary>
        protected bool _notifyCreation;

        /// <summary>
        ///     指示对象删除时是否发送通知。
        /// </summary>
        protected bool _notifyDeletion;

        /// <summary>
        ///     指示对象更新时是否发送通知。
        /// </summary>
        protected bool _notifyUpdate;

        /// <summary>
        ///     对象在关系数据库中的存储顺序（排序规则）。
        /// </summary>
        protected List<OrderRule> _storingOrder;

        /// <summary>
        ///     映射目标
        /// </summary>
        protected string _targetTable;

        /// <summary>
        ///     用于识别对象版本的属性集，简称版本键。
        /// </summary>
        protected List<string> _versionAttributes;

        /// <summary>
        ///     根据Clr类型创建Obj类型实例
        /// </summary>
        /// <param name="clrType">对象运行时类型</param>
        /// <param name="derivingFrom">基类</param>
        protected ObjectType(Type clrType, StructuralType derivingFrom = null) : base(clrType, derivingFrom)
        {
        }

        /// <summary>
        ///     获取或设置要包含在对象变更通知中的属性。
        /// </summary>
        public List<string> NoticeAttributes
        {
            get => _noticeAttributes;
            set => _noticeAttributes = value?.Distinct().ToList();
        }

        /// <summary>
        ///     获取或设置一个值，该值指示对象创建时是否发送通知。
        /// </summary>
        public bool NotifyCreation
        {
            get => _notifyCreation;
            set => _notifyCreation = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示对象删除时是否发送通知。
        /// </summary>
        public bool NotifyDeletion
        {
            get => _notifyDeletion;
            set => _notifyDeletion = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示对象更新时是否发送通知。
        /// </summary>
        public bool NotifyUpdate
        {
            get => _notifyUpdate;
            set => _notifyUpdate = value;
        }

        /// <summary>
        ///     获取或设置映射目标。
        /// </summary>
        public string TargetTable
        {
            get => _targetTable;
            set => _targetTable = value;
        }

        /// <summary>
        ///     获取或设置对象在关系数据库中的存储顺序（排序规则）。
        ///     注：实现get访问器时首先检查是否显式设置了存储顺序，如果没有则调用DefaultStoringOrder获取默认顺序。
        /// </summary>
        public List<OrderRule> StoringOrder
        {
            get
            {
                if (_storingOrder == null || _storingOrder.Count == 0)
                    return DefaultStoringOrder;
                return _storingOrder;
            }
            set => _storingOrder = value;
        }

        /// <summary>
        ///     获取默认的存储排序规则。
        ///     注：该属性由派生类实现。派生类通过实现此属性来提供特定于自身的默认存储排序规则。
        /// </summary>
        protected abstract List<OrderRule> DefaultStoringOrder { get; }


        /// <summary>
        ///     获取或设置用于识别对象版本的属性集，简称版本键。
        /// </summary>
        public List<string> VersionAttributes
        {
            get => _versionAttributes;
            set => _versionAttributes = value;
        }

        /// <summary>
        ///     获取或设置并发冲突处理策略。
        /// </summary>
        public EConcurrentConflictHandlingStrategy ConcurrentConflictHandlingStrategy
        {
            get => _concurrentConflictHandlingStrategy;
            set => _concurrentConflictHandlingStrategy = value;
        }


        /// <summary>
        ///     获取当前对象类型的映射表的键字段集合。
        /// </summary>
        public abstract List<string> KeyFields { get; set; }

        /// <summary>
        ///     获取对象标识成员的名称的序列。
        ///     备注
        ///     （1）对于实体型，其对象的标识成员为各标识属性；对于关联型，标识成员为各关联端对应的实体型的标识属性。
        ///     （2）关联对象标识成员的名称按以下规则生成：关联端名称 + ‘.’ + 实体型标识属性名称。
        /// </summary>
        public abstract string[] KeyMemberNames { get; }

        /// <summary>
        ///     获取映射目标名称
        /// </summary>
        public string TargetName
        {
            get => _targetTable;
            set => _targetTable = value;
        }

        /// <summary>
        ///     获取对象标识。
        /// </summary>
        /// 实施说明： 各派生类实现此方法时可参照ObjectSystemVisitor类中的相应方法及相关的顺序图。
        /// <param name="targetObj">要获取其标识的对象。</param>
        /// <returns></returns>
        public abstract ObjectKey GetObjectKey(object targetObj);
    }
}