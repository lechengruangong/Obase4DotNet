/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：实体型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:55:15
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示实体型。
    /// </summary>
    public class EntityType : ObjectType
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     默认的存储排序规则
        /// </summary>
        private List<OrderRule> _defaultStoringOrder;

        /// <summary>
        ///     标识属性组
        /// </summary>
        private List<string> _keyAttributes = new List<string>();

        /// <summary>
        ///     标识字段组
        /// </summary>
        private List<string> _keyFields;

        /// <summary>
        ///     标识是否自增
        /// </summary>
        private bool _keyIsSelfIncreased;

        /// <summary>
        ///     根据指定的CLR类型创建类型实例。
        /// </summary>
        /// <param name="clrType">CLR类型</param>
        /// <param name="derivingFrom">基类</param>
        public EntityType(Type clrType, StructuralType derivingFrom = null)
            : base(clrType, derivingFrom)
        {
            _typeName.IsAssociation = false;
            _typeName.IsEntity = true;
        }

        /// <summary>
        ///     获取实体型包含的关联引用的集合。
        /// </summary>
        public List<AssociationReference> AssociationReferences =>
            Elements.OfType<AssociationReference>().ToList();

        /// <summary>
        ///     获取或设置一个值，该值指示标识是否自增。
        /// </summary>
        public bool KeyIsSelfIncreased
        {
            get => _keyIsSelfIncreased;
            set
            {
                ReaderWriterLock.EnterWriteLock();
                _keyIsSelfIncreased = value;
                //设置所有标识对应属性的生成值
                KeyAttributes.ForEach(s =>
                    {
                        var attr = GetAttribute(s);
                        if (attr != null)
                            attr.DbGenerateValue = _keyIsSelfIncreased;
                    }
                );
                ReaderWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     获取或设置标识属性组。
        /// </summary>
        public List<string> KeyAttributes
        {
            get => _keyAttributes ?? (_keyAttributes = new List<string>());
            set
            {
                ReaderWriterLock.EnterWriteLock();
                _keyAttributes = value;
                //设置所有标识对应属性的生成值
                KeyAttributes.ForEach(s =>
                {
                    var attr = GetAttribute(s);
                    if (attr != null)
                        attr.DbGenerateValue = _keyIsSelfIncreased;
                });
                ReaderWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     获取当前对象类型的映射表的键字段集合。
        /// </summary>
        public override List<string> KeyFields
        {
            get
            {
                ReaderWriterLock.EnterUpgradeableReadLock();
                try
                {
                    if (_keyFields == null)
                    {
                        ReaderWriterLock.EnterWriteLock();
                        try
                        {
                            //实体的键字段是其标识属性对应的字段
                            _keyFields = KeyAttributes.Select(key => GetAttribute(key).TargetField).ToList();
                        }
                        finally
                        {
                            ReaderWriterLock.ExitWriteLock();
                        }
                    }

                    return _keyFields;
                }
                finally
                {
                    ReaderWriterLock.ExitUpgradeableReadLock();
                }
            }

            set => _keyFields = value;
        }


        /// <summary>
        ///     获取默认的存储排序规则。
        ///     注：该属性由派生类实现。派生类通过实现此属性来提供特定于自身的默认存储排序规则。
        /// </summary>
        protected override List<OrderRule> DefaultStoringOrder
        {
            get
            {
                ReaderWriterLock.EnterUpgradeableReadLock();
                try
                {
                    if (_defaultStoringOrder == null)
                    {
                        ReaderWriterLock.EnterWriteLock();
                        try
                        {
                            //实体的默认存储顺序是其标识属性对应的属性
                            _defaultStoringOrder = new List<OrderRule>();
                            KeyAttributes.ForEach(k =>
                                _defaultStoringOrder.Add(new OrderRule { OrderBy = GetAttribute(k) }));
                        }
                        finally
                        {
                            ReaderWriterLock.ExitWriteLock();
                        }
                    }

                    return _defaultStoringOrder;
                }
                finally
                {
                    ReaderWriterLock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>
        ///     获取对象标识成员的名称的序列。
        ///     备注：对于实体型，其对象的标识成员为各标识属性；对于关联型，标识成员为各关联端对应的实体型的标识属性。
        /// </summary>
        public override string[] KeyMemberNames => _keyAttributes.ToArray();


        /// <summary>
        ///     根据名称查找关联引用。
        /// </summary>
        /// <param name="name">关联引用名称</param>
        public AssociationReference GetAssociationReference(string name)
        {
            return GetElement(name) as AssociationReference;
        }


        /// <summary>
        ///     完整性检查
        ///     对于实体型 完整性检查检查此实体型上的键属性是否配置了取值器和设值器 以及关联引用的端和关联端是否匹配
        /// </summary>
        /// <param name="errDictionary">错误信息字典</param>
        public override void IntegrityCheck(Dictionary<string, List<string>> errDictionary)
        {
            //错误消息
            var message = new List<string>();
            //没设置主键
            if (_keyAttributes == null || _keyAttributes.Count == 0)
                //有继承 将继承的复制过来
                if (DerivingFrom is EntityType derivingFrom)
                    _keyAttributes = derivingFrom.KeyAttributes;
            //再次检查 没有就抛异常
            if (_keyAttributes == null || _keyAttributes.Count == 0)
                message.Add($"实体{Name}的键属性未设置");
            //检查键
            var keyAttrs = Attributes.Where(p => KeyAttributes.Contains(p.Name)).ToList();

            //自增 但是是联合主键
            if (_keyIsSelfIncreased && keyAttrs.Count > 1)
                message.Add($"实体{Name}的键属性是联合主键,不能是自增的");
            //检查主键
            foreach (var keyAttr in keyAttrs)
            {
                //检查键属性
                if (keyAttr.ValueSetter == null)
                    if (Constructor.GetParameterByElement(keyAttr.Name) == null)
                        message.Add($"实体{Name}的键属性{keyAttr.Name}没有设值器,且没有在构造函数中使用.");

                if (_keyIsSelfIncreased && keyAttr.DataType != typeof(int) && keyAttr.DataType != typeof(long) &&
                    keyAttr.DataType != typeof(short) && keyAttr.DataType != typeof(uint) &&
                    keyAttr.DataType != typeof(ulong) && keyAttr.DataType != typeof(ushort))
                    message.Add($"实体{Name}的键属性{keyAttr.Name}是自增的但不是short,int,long类型.");


                if (keyAttr.ValueGetter == null)
                    message.Add($"实体{Name}的键属性{keyAttr.Name}没有取值器.");
            }

            //检查关联引用
            foreach (var reference in AssociationReferences)
            {
                //检查左端
                if (string.IsNullOrEmpty(reference.LeftEnd))
                    message.Add(
                        $"{ClrType}的关联引用{reference.Name}的端未能自动配置,请手动配置此关联引用.");

                if (reference.AssociationType.AssociationEnds.All(p => p.Name != reference.LeftEnd))
                    message.Add(
                        $"{ClrType}的关联引用{reference.Name}的左端{reference.LeftEnd}无法与关联端的名字相匹配,请检查关联端的名称和左端名称是否一致.");

                //检查右端
                if (reference.AssociationType.AssociationEnds.All(p => p.Name != reference.RightEnd) &&
                    !reference.AssociationType.Visible)
                    message.Add(
                        $"{ClrType}的关联引用{reference.Name}的右端{reference.RightEnd}无法与关联端的名字相匹配,请检查关联端的名称和右端名称是否一致.");
                //检查设值器和取值器
                if (reference.ValueGetter == null)
                    message.Add(
                        $"{ClrType}的关联引用{reference.Name}没有取值器.");

                if (reference.ValueSetter == null)
                    message.Add(
                        $"{ClrType}的关联引用{reference.Name}没有设值器.");

                //检查左端是否和右端相同
                if(reference.LeftEnd == reference.RightEnd)
                    message.Add($"{ClrType}的关联引用{reference.Name}的左端和右端不能相同.");
            }

            //检查键属性
            if (_keyFields == null)
            {
                _keyFields = new List<string>();
                foreach (var key in KeyAttributes)
                {
                    var attr = GetAttribute(key);
                    if (attr == null)
                        message.Add($"{ClrType}的主键{key}没有对应属性.");
                    if (attr != null)
                        _keyFields.Add(attr.TargetField);
                }
            }

            //检查默认排序
            if (_defaultStoringOrder == null)
            {
                _defaultStoringOrder = new List<OrderRule>();
                foreach (var key in KeyAttributes)
                {
                    var attr = GetAttribute(key);
                    if (attr == null)
                        message.Add($"{ClrType}的主键{key}没有对应属性.");
                    _defaultStoringOrder.Add(new OrderRule { OrderBy = attr });
                }
            }

            //通用的对象类型检查
            CommonIntegrityCheck(errDictionary);

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


        /// <summary>
        ///     获取对象标识。
        /// </summary>
        /// <param name="targetObj">目标对象</param>
        /// <returns></returns>
        public override ObjectKey GetObjectKey(object targetObj)
        {
            //对象标识成员由标识属性名与属性值组合
            var objectKeyMemberList = new List<ObjectKeyMember>();
            if (KeyAttributes != null)
                foreach (var key in KeyAttributes)
                {
                    //获取属性值
                    var value = GetValue(targetObj, this, key);
                    //创建标识成员
                    var member = new ObjectKeyMember($"{ClrType.FullName}-{key}", value);
                    objectKeyMemberList.Add(member);
                }

            //创建对象标识
            return new ObjectKey(this, objectKeyMemberList);
        }

        /// <summary>
        ///     从对象中获取元素（属性、关联引用、关联端）的值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="type">对象的类型（实体型、关联型、复杂类型）</param>
        /// <param name="elementName">目标元素的名称</param>
        private object GetValue(object obj, StructuralType type, string elementName)
        {
            //获取元素
            var typeElement = type.GetElement(elementName);
            if (typeElement == null) throw new ArgumentException($"无法获取到{elementName}的类型元素.", nameof(elementName));
            //获取对象元素的值
            return GetValue(obj, typeElement);
        }

        /// <summary>
        ///     从对象中获取元素（属性、关联引用、关联端）的值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="element">目标元素</param>
        private object GetValue(object obj, TypeElement element)
        {
            object result;
            if (element is ReferenceElement && obj is IIntervene inter)
            {
                //禁用延迟加载（防止延迟加载期间内部访问属性又开始加载，造成死循环）
                inter.ForbidLazyLoading();
                //获取值
                result = element.ValueGetter.GetValue(obj);
                //启用延迟加载
                inter.EnableLazyLoading();
            }
            else
            {
                //获取值
                result = element.ValueGetter.GetValue(obj);
            }

            return result;
        }

        /// <summary>
        ///     获取实体型的键（即标识属性序列）。
        /// </summary>
        public Attribute[] GetKey()
        {
            //从键属性名称投影到属性
            return _keyAttributes.Select(GetAttribute).ToArray();
        }

        /// <summary>
        ///     获取类型的筛选键。
        ///     对于类型的某一个属性或属性序列，如果其值或值序列可以作为该类型实例的标识，该属性或属性序列即可作为该类型的筛选键。
        ///     对于实体型，可以用主键作为筛选键。对于关联型，可以用其在各关联端上的外键属性组合成的属性序列作为筛选键。
        /// </summary>
        /// <returns>构成筛选键的属性序列。</returns>
        public override Attribute[] GetFilterKey()
        {
            //实体型的键属性就是其筛选键
            return GetKey();
        }

        /// <summary>
        ///     字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"EntityType:{{Name-\"{Name}\",ClrType-\"{ClrType}\"}}";
        }
    }
}