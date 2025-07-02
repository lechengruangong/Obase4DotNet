/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 09:47:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示关联型
    /// </summary>
    public class AssociationType : ObjectType
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     伴随端
        /// </summary>
        private AssociationEnd _companionEnd;

        /// <summary>
        ///     默认的存储排序规则
        /// </summary>
        private List<OrderRule> _defaultStoringOrder;

        /// <summary>
        ///     获取当前对象类型的映射表的键字段集合。
        /// </summary>
        private List<string> _keyFields;

        /// <summary>
        ///     是否为显式关联，默认为false（表示有关联关系类型）
        /// </summary>
        private bool _visible;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="clrType">运行时类型</param>
        /// <param name="derivingFrom">基类类型</param>
        public AssociationType(Type clrType, StructuralType derivingFrom = null) : base(clrType, derivingFrom)
        {
            _typeName.IsAssociation = true;
            _typeName.IsEntity = false;
        }

        /// <summary>
        ///     获取伴随关联端(和表名相同的端)，如果关联型为独立映射返回Null。
        ///     伴随端的判定条件：（1）映射表与关联型相同；（2）关联端的映射字段与实体型标识属性的映射字段相同
        ///     增补:如果关联端实体型有基类（含间接基类），还要比对基类的映射表，只要其本身或基数中有一个与关联型的映射表相同，就应判定为伴随。
        /// </summary>
        public AssociationEnd CompanionEnd
        {
            get
            {
                if (_companionEnd == null)
                    foreach (var item in AssociationEnds)
                    {
                        //条件1和条件2
                        var targetFieldList = item.Mappings.Select(p => p.TargetField).ToList();
                        if (_targetTable == item.EntityType.TargetTable &&
                            !targetFieldList.Except(item.EntityType.KeyFields).Any() &&
                            !item.EntityType.KeyFields.Except(targetFieldList).Any())
                            _companionEnd = item;
                        //增补条件
                        var currentObjectType = (ObjectType)item.EntityType.DerivingFrom;
                        var derivingFromTableNames = new List<string>();
                        while (currentObjectType != null)
                        {
                            derivingFromTableNames.Add(currentObjectType.TargetTable);
                            currentObjectType = (ObjectType)currentObjectType.DerivingFrom;
                        }

                        //检测是否包含
                        if (derivingFromTableNames.Contains(_targetTable))
                            _companionEnd = item;
                    }

                return _companionEnd;
            }
        }


        /// <summary>
        ///     获取关联型包含的关联端的集合。
        /// </summary>
        public List<AssociationEnd> AssociationEnds => Elements.OfType<AssociationEnd>().ToList();

        /// <summary>
        ///     获取关联型包含的聚合关联端的集合
        /// </summary>
        public List<AssociationEnd> AggregatedEnds =>
            Elements.OfType<AssociationEnd>().Where(p => p.IsAggregated).ToList();

        /// <summary>
        ///     获取或设置一个值，该值表示是否为显式关联，默认为false。
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        /// <summary>
        ///     获取一个值，该值指示关联型是否为独立映射。
        /// </summary>
        public bool Independent
        {
            get
            {
                var isIndependent = true;
                //任意关联端的映射表与关联型的映射表相同 则不是独立映射
                foreach (var item in AssociationEnds)
                    if (_targetTable == item.EntityType.TargetTable ||
                        _targetTable == Utils.GetDerivedTargetTable(item.EntityType))
                    {
                        isIndependent = false;
                        break;
                    }

                return isIndependent;
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
                            //关联型的键字段是各关联端的映射字段组合而成的
                            _keyFields =
                                AssociationEnds.SelectMany(p => p.Mappings.Select(q => q.TargetField)).ToList();
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
                            //关联型的默认存储顺序是各关联端的映射字段组合而成的
                            _defaultStoringOrder = AssociationEnds
                                .SelectMany(end => end.Mappings.Select(m => new OrderRule { OrderBy = m })).ToList();
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
        ///     备注:关联对象标识成员的名称按以下规则生成：关联端名称 + ‘.’ + 实体型标识属性名称。
        /// </summary>
        public override string[] KeyMemberNames
        {
            get
            {
                //各关联端对应的实体型的标识属性
                var result = AssociationEnds
                    .SelectMany(end => end.EntityType.KeyMemberNames.Select(m => $"{end.Name}.{m}")).ToList();
                return result.ToArray();
            }
        }

        /// <summary>
        ///     根据指定的关联端名称及标识属性查找关联端映射，并返回映射目标字段。
        /// </summary>
        /// <param name="endName">关联端名称</param>
        /// <param name="keyAttribute">关联端对象的标识属性</param>
        public string FindAssociationEndMapping(string endName, string keyAttribute)
        {
            //第一个参数是端 第二个参数是标识属性
            var end = GetAssociationEnd(endName);
            return end.Mappings.Single(s => s.KeyAttribute.Equals(keyAttribute)).TargetField;
        }

        /// <summary>
        ///     根据指定的路径（用点号分隔的关联端名称及标识属性）查找关联端映射，并返回映射目标字段。
        /// </summary>
        /// <param name="path">关联端名称</param>
        public string FindAssociationEndMapping(string path)
        {
            //按照点号分隔路径处理
            var paths = path.Split('.');
            return FindAssociationEndMapping(paths[0], paths[1]);
        }

        /// <summary>
        ///     判定指定的关联端是否为当前关联型的伴随关联端（简称伴随端）,和关联型表名相同返回true,否则false。
        /// </summary>
        /// <param name="associationEnd">要判定的关联端</param>
        /// <returns>和关联型表名相同返回true,否则false</returns>
        public bool IsCompanionEnd(AssociationEnd associationEnd)
        {
            return associationEnd?.Equals(CompanionEnd) ?? false;
        }

        /// <summary>
        ///     判定指定的关联端是否为当前关联型的伴随关联端（简称伴随端）。
        /// </summary>
        /// <param name="endName">要判定的关联端的名称</param>
        public bool IsCompanionEnd(string endName)
        {
            return IsCompanionEnd(GetAssociationEnd(endName));
        }


        /// <summary>
        ///     根据名称获取关联端。
        /// </summary>
        /// <param name="name">关联端名称</param>
        public AssociationEnd GetAssociationEnd(string name)
        {
            return GetElement(name) as AssociationEnd;
        }

        /// <summary>
        ///     完整性检查
        ///     对于关联型 完整性检查检查关联端上的映射关系 检查关联端是否在关联型内
        /// </summary>
        public override void IntegrityCheck()
        {
            //隐式关联型 不能有属性
            if (!_visible)
            {
                var attr = _elements.Values.FirstOrDefault(p => p.ElementType == EElementType.Attribute);
                if (attr != null)
                    if (!((Attribute)attr).IsForeignKeyDefineMissing)
                        throw new ArgumentException($"隐式关联型{Name}内应只有关联端,属性{attr.Name}不应被定义.", nameof(attr));
            }

            //关联端数量
            if (AssociationEnds == null || AssociationEnds.Count == 0)
                throw new ArgumentException($"关联型{Name}内无关联端.", nameof(AssociationEnds));

            if (AssociationEnds.Count < 2) throw new ArgumentException($"关联型{Name}内关联端少于2个.", nameof(AssociationEnds));

            //检查一般属性
            foreach (var attribute in Attributes)
            {
                //检查属性
                if (attribute.ValueSetter == null)
                    if (Constructor.GetParameterByElement(attribute.Name) == null)
                        throw new ArgumentException($"关联型{Name}的属性{attribute.Name}没有设值器,且没有在构造函数中使用.", attribute.Name);


                if (attribute.ValueGetter == null)
                    throw new ArgumentException($"关联型{Name}的属性{attribute.Name}没有取值器.", attribute.Name);
            }

            //检查关联端
            foreach (var end in AssociationEnds)
            {
                //检查关联端本身
                if (ClrType.GetProperty(end.Name) == null)
                    throw new ArgumentException($"关联型{Name}内无法找到关联端{end.Name}的属性访问器.", nameof(end));

                if (end.Mappings == null || end.Mappings.Count == 0)
                    throw new ArgumentException($"关联型{Name}的关联端{end.Name}没有映射.");

                //检查映射
                if (end.Mappings != null)
                {
                    //按照KeyAttr分组 分组后如果与之前个数不相同 则有重复
                    var isRepeat = end.Mappings.GroupBy(p => p.KeyAttribute).Count() != end.Mappings.Count;

                    if (isRepeat) throw new ArgumentException($"关联型{Name}的关联端{end.Name}内有重复的映射.", nameof(end.Mappings));
                }

                //检查设值器和取值器
                if (end.ValueGetter == null)
                    throw new ArgumentException(
                        $"{ClrType}的关联端{end.Name}没有取值器.");

                if (end.ValueSetter == null)
                    throw new ArgumentException(
                        $"{ClrType}的关联端{end.Name}没有设值器.");

                //检查Mapping
                foreach (var mapping in end.Mappings)
                    if (end.EntityType.GetAttribute(mapping.KeyAttribute) == null)
                        throw new ArgumentException(
                            $"关联型{Name}的关联端{end.Name}映射{mapping.KeyAttribute}属性无法在端类型{end.EntityType.ClrType}中找到.");
            }

            //检查键属性
            if (_keyFields == null)
                _keyFields =
                    AssociationEnds.SelectMany(p => p.Mappings.Select(q => q.TargetField)).ToList();

            //检查默认排序
            if (_defaultStoringOrder == null)
                _defaultStoringOrder = AssociationEnds
                    .SelectMany(end => end.Mappings.Select(m => new OrderRule { OrderBy = m })).ToList();

            //检查构造函数
            if (Constructor == null)
                throw new ArgumentException($"{_clrType}未配置有效的构造函数.");
            //检查映射表
            if (string.IsNullOrEmpty(_targetTable))
                throw new ArgumentException($"{_clrType}未配置映射表.");
            //检查继承的配置
            if (DerivingFrom != null && ConcreteTypeSign == null)
                throw new ArgumentException($"{_clrType}配置为继承{DerivingFrom.ClrType},却没有配置具体类型判别标志.");
            if (DerivedTypes.Count > 0 && ConcreteTypeSign == null)
                throw new ArgumentException($"{_clrType}配置为基础类型,却没有配置具体类型判别标志.");
        }

        /// <summary>
        ///     获取对象标识。
        /// </summary>
        /// <returns></returns>
        public override ObjectKey GetObjectKey(object targetObj)
        {
            var objectKeyMemberList = new List<ObjectKeyMember>();
            //遍历关联端
            foreach (var associationEnd in AssociationEnds)
            {
                //获取端对象
                var endObj = GetValue(targetObj, associationEnd);
                //遍历关联端的映射
                foreach (var mapping in associationEnd.Mappings)
                {
                    object value;
                    if (endObj == null)
                    {
                        //根据字段名查找属性
                        var attr = FindAttributeByTargetField(mapping.TargetField);
                        if (attr == null)
                            continue;
                        //取出目标对象的属性值
                        value = GetValue(targetObj, attr);
                    }
                    else
                    {
                        //取出关联端的标识属性值
                        value = GetValue(endObj, associationEnd.EntityType, mapping.KeyAttribute);
                    }

                    //创建标识成员
                    var member =
                        new ObjectKeyMember(
                            associationEnd.EntityType.ClrType.FullName + "-" + associationEnd.Name + "." +
                            mapping.KeyAttribute, value);
                    objectKeyMemberList.Add(member);
                }
            }

            //创建对象标识
            return new ObjectKey(this, objectKeyMemberList);
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
        ///     获取类型的筛选键。
        ///     对于类型的某一个属性或属性序列，如果其值或值序列可以作为该类型实例的标识，该属性或属性序列即可作为该类型的筛选键。
        ///     对于实体型，可以用主键作为筛选键。对于关联型，可以用其在各关联端上的外键属性组合成的属性序列作为筛选键。
        /// </summary>
        /// <returns>构成筛选键的属性序列。</returns>
        public override Attribute[] GetFilterKey()
        {
            //关联型 取在各关联端上的外键属性组合成的属性序列作为筛选键
            var attributes = AssociationEnds?.SelectMany(ae => ae.GetForeignKey(true)).ToArray() ??
                             Array.Empty<Attribute>();
            return attributes;
        }
    }
}