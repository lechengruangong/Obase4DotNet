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
using Obase.Core.Common;

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

        /// <summary>
        ///     对象类型的通用的完整性检查
        /// </summary>
        /// <param name="errDictionary">错误信息字典</param>
        protected void CommonIntegrityCheck(Dictionary<string, List<string>> errDictionary)
        {
            //错误消息
            var message = new List<string>();
            //检查构造函数
            if (Constructor == null)
                message.Add($"{_clrType}未配置有效的构造函数.");
            //检查映射表
            if (string.IsNullOrEmpty(_targetTable))
                message.Add($"{_clrType}未配置映射表.");
            //检查继承的配置
            if (DerivingFrom != null && ConcreteTypeSign == null)
                message.Add($"{_clrType}配置为继承{DerivingFrom.ClrType},却没有配置具体类型判别标志.");
            if (DerivedTypes.Count > 0 && ConcreteTypeSign == null)
                message.Add($"{_clrType}配置为基础类型,却没有配置具体类型判别标志.");
            //检查继承的映射表是否一致
            if (DerivingFrom is ObjectType derivingObjectType)
                if (derivingObjectType.TargetTable != TargetTable)
                    message.Add(
                        $"{_clrType}配置为继承{DerivingFrom.ClrType},但映射表与父类不一致,父类映射表为{derivingObjectType.TargetTable},当前为{TargetTable}.");

            //检查父类的构造器
            if (DerivingFrom != null)
            {
                //比较当前构造器的参数个数和父类构造器的参数个数
                var currentCount = Utils.GetConstructorParameterCount(_constructor);
                var derivingCount = Utils.GetConstructorParameterCount(DerivingFrom.Constructor);
                //不一致 抛出异常
                if (currentCount != derivingCount)
                    throw new ArgumentException(
                        $"{_clrType}的构造器参数个数与父类参数个数不一致,{_clrType}为{currentCount}个,但父类{DerivingFrom.ClrType}的构造器参数为{derivingCount}个.");
                //如果个数大于0 再检查每一个的类型
                if (currentCount > 0)
                    for (var i = 0; i < currentCount; i++)
                    {
                        var currentType = _constructor.Parameters?[i]?.GetType();
                        var derivingType = DerivingFrom.Constructor.Parameters?[i]?.GetType();
                        //检查类型是否相等
                        if (currentType != derivingType)
                            message.Add(
                                $"{_clrType}的构造器参数第{i + 1}个参数类型与父类参数类型不一致,{_clrType}为{currentType},但父类{DerivingFrom.ClrType}的构造器参数类型为{derivingType}.");
                    }
            }

            //检查一般属性
            foreach (var attribute in Attributes)
            {
                //检查属性
                if (attribute.ValueSetter == null)
                    if (Constructor != null && Constructor.GetParameterByElement(attribute.Name) == null)
                        //如果最顶层的继承也没有为此属性的构造函数参数
                        if (Utils.GetDerivedIInstanceConstructor(this)?.GetParameterByElement(attribute.Name) == null)
                            message.Add($"实体{Name}的属性{attribute.Name}没有设值器,且没有在构造函数中使用.");

                if (attribute.ValueGetter == null)
                    message.Add($"实体{Name}的属性{attribute.Name}没有取值器.");
            }

            //检查引用元素的延迟加载配置
            if (ReferenceElements != null)
                foreach (var referenceElement in ReferenceElements)
                {
                    //检查引用元素的get方法
                    var getMethod = referenceElement.HostType?.ClrType?.GetProperty(referenceElement.Name)?.GetMethod;
                    //如果有GetMethod 且 不是虚方法 且 启用了延迟加载 就增加异常消息
                    if (getMethod != null && !getMethod.IsVirtual && referenceElement.EnableLazyLoading)
                        message.Add($"对象类型{Name}的引用元素{referenceElement.Name}启用了延迟加载,但没有将其声明为virtual的.");
                }


            //如果有检查失败消息
            if (message.Any())
            {
                //就与现有的问题合并
                var name = _clrType?.FullName ?? _name;
                if (errDictionary.ContainsKey(name))
                    errDictionary[name].AddRange(message);
                else
                    errDictionary.Add(name, message);
            }
        }
    }
}