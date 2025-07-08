/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联端.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:31:23
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示关联端。
    /// </summary>
    public class AssociationEnd : ReferenceElement
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     关联端的实体型
        /// </summary>
        private readonly EntityType _entityType;

        /// <summary>
        ///     指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        private bool _defaultAsNew;

        /// <summary>
        ///     外键所在的类型的寄存器
        /// </summary>
        private ObjectType _definingTypeOfForeignKey;

        /// <summary>
        ///     外键寄存器
        /// </summary>
        private Attribute[] _foreignKey;

        /// <summary>
        ///     指示当前关联端是否为聚合关联端。
        /// </summary>
        private bool _isAggregated;

        /// <summary>
        ///     关联端映射
        /// </summary>
        private List<AssociationEndMapping> _mappings = new List<AssociationEndMapping>();

        /// <summary>
        ///     寄存 引用元素所承载的对象导航行为
        /// </summary>
        private ObjectNavigation _navigation;

        /// <summary>
        ///     创建关联端的实例
        /// </summary>
        /// <param name="name">关联端名称</param>
        private AssociationEnd(string name)
            : base(name, EElementType.AssociationEnd)
        {
        }

        /// <summary>
        ///     创建关联端实例。
        /// </summary>
        /// <param name="name">关联端名称</param>
        /// <param name="entityType">实体型</param>
        public AssociationEnd(string name, EntityType entityType)
            : this(name)
        {
            _entityType = entityType;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示当前关联端是否为聚合关联端。
        /// </summary>
        public bool IsAggregated
        {
            get => _isAggregated;
            set => _isAggregated = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        public bool DefaultAsNew
        {
            get => _defaultAsNew;
            set => _defaultAsNew = value;
        }

        /// <summary>
        ///     获取或设置关联端到关联表的映射。（关联端实体型的属性名到关联表的字段名）
        /// </summary>
        public List<AssociationEndMapping> Mappings
        {
            get => _mappings;
            set => _mappings = value;
        }

        /// <summary>
        ///     获取关联端的实体型。
        /// </summary>
        public EntityType EntityType => _entityType;

        /// <summary>
        ///     获取引用元素的类型。
        ///     为关联端时返回EntityType。
        /// </summary>
        [Obsolete]
        public override ObjectType ReferenceType => _entityType;

        /// <summary>
        ///     获取引用元素所承载的对象导航行为。
        /// </summary>
        /// 实施说明：
        /// 源端不明确。
        /// 如果关联端所属的关联型为显式的，为间接导航；
        /// 如果为隐式的，则为直接导航。
        public override ObjectNavigation Navigation
        {
            get
            {
                ReaderWriterLock.EnterUpgradeableReadLock();
                try
                {
                    if (_navigation == null)
                    {
                        ReaderWriterLock.EnterWriteLock();
                        try
                        {
                            //使用HostType的AssociationType构造导航
                            if (HostType is AssociationType association)
                                _navigation = new ObjectNavigation(association, null, Name);
                        }
                        finally
                        {
                            ReaderWriterLock.ExitWriteLock();
                        }
                    }

                    return _navigation;
                }
                finally
                {
                    ReaderWriterLock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>
        ///     获取引用元素在对象导航中承担的用途。
        /// </summary>
        /// 实施说明:
        /// 关联端为到达引用。
        public override ENavigationUse NavigationUse => ENavigationUse.ArrivingReference;

        /// <summary>
        /// </summary>
        public override TypeBase ValueType => _entityType;

        /// <summary>
        ///     获取指定的关联端标识属性的映射目标字段。
        /// </summary>
        /// <param name="keyAttribute">要获取其映射目标的标识属性的名称</param>
        public string GetTargetField(string keyAttribute)
        {
            return Mappings.Single(m => m.KeyAttribute.Equals(keyAttribute)).TargetField;
        }

        /// <summary>
        ///     在基于当前引用元素实施关联导航的过程中，向前推进一步。
        ///     基于特定的关联，可以从一个对象转移到另一个对象，这个过程称为导航。有两种类型的导航。一种是间接导航，即借助于关联对象，先从源对象转移到关联对象，然后再转移到目标
        ///     对象。另一种是直接导航，即从源对象直接转移到目标对象。
        ///     注意，不论基于隐式关联还是显式关联，本方法实施的关联导航统一遵循间接导航路径，即如果是隐式关联，将自动实施显式化操作。
        /// </summary>
        /// <returns>
        ///     本次导航步的到达地。
        ///     实施说明
        ///     参照ObjectSystemVisitor.
        ///     AssociationNavigate方法及相应的顺序图“执行映射/Saving/访问对象系统/关联导航”。
        /// </returns>
        /// <param name="sourceObj">本次导航步的出发地。</param>
        public override object[] NavigationStep(object sourceObj)
        {
            //端对象 导航自己
            return new[] { sourceObj };
        }

        /// <summary>
        ///     验证延迟加载合法性，由派生类实现。
        /// </summary>
        /// <returns>如果可以启用延迟加载返回true，否则返回false，同时返回原因。</returns>
        /// <param name="reason">返回不能启用延迟加载的原因。</param>
        protected override bool ValidateLazyLoading(out string reason)
        {
            reason = "";
            foreach (var mapping in Mappings)
            {
                var attr = HostType.FindAttributeByTargetField(mapping.TargetField);
                if (attr != null) continue;
                reason = $"当前对象（{HostType.Namespace}{HostType.Name}）没有关联端（{HostType.Name}）对象的标识属性";
                return false;
            }

            return true;
        }

        /// <summary>
        ///     检测关联端是否为伴随关联端。
        ///     实施说明
        ///     调用所属关联型的IsCompanionEnd方法。
        /// </summary>
        public bool IsCompanionEnd()
        {
            //找关联型 检测自己
            if (HostType is AssociationType association) return association.IsCompanionEnd(this);

            return false;
        }

        /// <summary>
        ///     获取关联端所属关联型在该端上的外键。
        ///     对于显式关联或隐式伴随关联的伴随端实体型，如果存在一个属性序列，各属性的映射字段依次为所述关联某端的映射字段，则该属性序列为该关联型在该关联端上的外键，其中的属
        ///     性称为外键属性。
        ///     显式关联（独立或伴随）的外键定义在关联类型上；隐式伴随关联的外键定义在伴随端实体类型上；隐式独立关联一般没有外键，需要时可将隐式独立关联显式化，在显式化的关联类
        ///     上定义外键。显式关联或隐式伴随关联也可能没有外键。
        ///     说明
        ///     defineMissing参数指示外键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发异常。
        ///     但是，即使指示自动定义缺失的外键属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发ForeignKeyGuarantingExceptio
        ///     n。
        /// </summary>
        /// <exception cref="ForeignKeyNotFoundException">外键属性没有定义</exception>
        /// <exception cref="ForeignKeyGuarantingException">无法确保定义外键属性</exception>
        /// <param name="defineMissing">指示当外键属性缺失时是否定义该属性。</param>
        /// <param name="definingType">返回定义外键的类型。</param>
        /// 实施说明:
        /// 设置两个寄存器_foreignKey和_definingTypeOfForeignKey，分别用于寄存外键和其所在的类型。
        /// 方法被调用时应当首先检查寄存器，避免重复计算。
        public Attribute[] GetForeignKey(out ObjectType definingType, bool defineMissing = false)
        {
            if (_foreignKey != null && _foreignKey.Length > 0 && _definingTypeOfForeignKey != null)
            {
                definingType = _definingTypeOfForeignKey;
                return _foreignKey;
            }

            //HostType 为 AssociationType 转换为ObjectType
            _definingTypeOfForeignKey = definingType = (ObjectType)HostType;

            //从Mapping里查
            Attribute[] result = null;

            if (HostType is AssociationType associationType)
            {
                //隐式非独立
                if (!associationType.Visible && !associationType.Independent)
                    //伴随段的实体型
                    _definingTypeOfForeignKey = definingType = associationType.CompanionEnd.EntityType;

                if (defineMissing)
                {
                    //当前端为伴随端
                    var isCom = IsCompanionEnd();
                    if (isCom && !associationType.Visible)
                    {
                        var type = definingType;
                        result = definingType.Attributes.Where(p => type.KeyFields.Contains(p.Name)).ToArray();
                    }
                    else
                    {
                        //自己补
                        var guarantor = new DerivingBasedForeignKeyGuarantor();
                        result = guarantor.Guarantee(definingType, this);
                    }
                }
                else
                {
                    //不需要补 那就只做检查
                    var tempResult = new List<Attribute>();
                    foreach (var mapping in _mappings)
                    {
                        var attribute = definingType.FindAttributeByTargetField(mapping.TargetField);
                        if (attribute != null)
                            tempResult.Add(attribute);
                        else
                            throw new ForeignKeyNotFoundException($"{definingType.ClrType.FullName}外键属性没有定义");
                    }

                    result = tempResult.ToArray();
                }

                //最后检查一下
                if (result.Length <= 0)
                {
                    var tempResult = new List<Attribute>();
                    foreach (var mapping in _mappings)
                    {
                        var attribute = definingType.FindAttributeByTargetField(mapping.TargetField);
                        if (attribute != null)
                            tempResult.Add(attribute);
                        else
                            throw new ForeignKeyNotFoundException("外键属性没有定义");
                    }

                    result = tempResult.ToArray();
                }
            }

            return result;
        }

        /// <summary>
        ///     获取关联端所属关联型在该端上的外键。
        ///     说明
        ///     defineMissing参数指示外键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发异常。
        ///     但是，即使指示自动定义缺失的外键属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发ForeignKeyGuarantingExceptio
        ///     n。
        /// </summary>
        /// <exception cref="Exception">外键属性没有定义</exception>
        /// <exception cref="ForeignKeyNotFoundException">无法确保定义外键属性</exception>
        /// <param name="defineMissing">指示当外键属性缺失时是否定义该属性。</param>
        public Attribute[] GetForeignKey(bool defineMissing = false)
        {
            ReaderWriterLock.EnterUpgradeableReadLock();
            try
            {
                //检查寄存器_foreignKey
                if (_foreignKey == null || _foreignKey.Length == 0)
                {
                    ReaderWriterLock.EnterWriteLock();
                    try
                    {
                        //没有 需要定义
                        _foreignKey = GetForeignKey(out _, defineMissing);
                    }
                    finally
                    {
                        ReaderWriterLock.ExitWriteLock();
                    }
                }

                return _foreignKey;
            }
            finally
            {
                ReaderWriterLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        ///     检查关联端所属关联型在该端上的外键是否已定义。
        /// </summary>
        /// 实施说明:
        /// 参照GetForeignKey方法，但不要直接调用该方法。注意删除不需要的操作。
        /// 首先检查寄存器_foreignKey，如果有值则直接返回true，避免重复计算。
        /// <returns> 如果已定义返回true，否则返回false。</returns>
        public bool ForeignKeyExist()
        {
            ReaderWriterLock.EnterUpgradeableReadLock();
            try
            {
                //检查寄存器_foreignKey
                if (_foreignKey == null || _foreignKey.Length == 0 || _definingTypeOfForeignKey == null)
                {
                    ReaderWriterLock.EnterWriteLock();
                    try
                    {
                        _definingTypeOfForeignKey = (ObjectType)HostType;
                        if (HostType is AssociationType associationType)
                        {
                            //隐式非独立
                            if (!associationType.Visible && !associationType.Independent)
                                //伴随端的实体型
                                _definingTypeOfForeignKey = associationType.CompanionEnd.EntityType;
                            var tempResult = new List<Attribute>();
                            foreach (var mapping in _mappings)
                            {
                                //使用映射字段检查
                                var attribute =
                                    _definingTypeOfForeignKey.FindAttributeByTargetField(mapping.TargetField);
                                if (attribute != null)
                                    tempResult.Add(attribute);
                                else
                                    return false;
                            }

                            _foreignKey = tempResult.ToArray();
                        }
                    }
                    finally
                    {
                        ReaderWriterLock.ExitWriteLock();
                    }
                }

                return _foreignKey?.Length > 0;
            }
            finally
            {
                ReaderWriterLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        ///     获取关联端的引用键。
        ///     说明
        ///     关联端的引用键是其实体型的标识键。
        ///     defineMissing参数指示引用键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发KeyAttributeLackException。
        ///     但是，即使指示自动定义缺失的属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发CannotDefiningAttributeExcepti
        ///     on。
        /// </summary>
        /// <exception cref="ForeignKeyGuarantingException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">
        ///     无法定义缺失的引用键属性
        ///     实施说明
        ///     捕获ForeignKeyGuarantingException后引发CannotDefiningAttributeException。
        /// </exception>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public override Attribute[] GetReferringKey(bool defineMissing = false)
        {
            //从端实体型的属性中获取
            var array = EntityType.Attributes.Where(p => EntityType.KeyFields.Contains(p.TargetField)).ToArray();
            if (array.Length <= 0) throw new CannotDefiningAttributeException("无法定义键属性", null);

            return array;
        }

        /// <summary>
        ///     获取关联端的参考键。
        ///     说明
        ///     关联端的参考键是其所属关联型在该端上的外键。
        ///     defineMissing参数指示参考键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发KeyAttributeLackException。
        ///     但是，即使指示自动定义缺失的属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发CannotDefiningAttributeExcepti
        ///     on。
        /// </summary>
        /// <exception cref="KeyAttributeLackException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">
        ///     无法定义缺失的参考键属性
        ///     实施说明
        ///     捕获ForeignKeyGuarantingException后引发CannotDefiningAttributeException。
        /// </exception>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public override Attribute[] GetReferredKey(bool defineMissing = false)
        {
            try
            {
                if (HostType is AssociationType association)
                    //从端上取参考键
                    return association.GetAssociationEnd(Name).GetForeignKey(defineMissing);
            }
            catch (ForeignKeyGuarantingException e)
            {
                throw new CannotDefiningAttributeException("无法定义键属性", e);
            }

            return null;
        }

        /// <summary>
        ///     获取关联端标识属性的值。
        ///     实施说明
        ///     首先探测关联对象上是否定义了退化属性，如果是则取该属性的值，否则从关联端对象取值。参见活动图“获取关联端标识属性的值”。
        /// </summary>
        /// <param name="assoObj">关联端所属的关联对象。</param>
        /// <param name="keyAttribute">关联端标识属性的名称。</param>
        public object GetKeyAttributeValue(object assoObj, string keyAttribute)
        {
            var targetField = GetTargetField(keyAttribute);
            //寻找退化属性
            var fieldAttribute = HostType.FindAttributeByTargetField(targetField);
            //未定义退化属性 或者 退化属性为默认值 则去查询端对象
            if (fieldAttribute == null)
                return GetEndKeyAttributeValue(assoObj, keyAttribute);
            try
            {
                //获取关联对象的映射字段的值 从端上取
                var value = fieldAttribute.GetValue(assoObj);
                return IsDefaultValue(value) ? GetEndKeyAttributeValue(assoObj, keyAttribute) : value;
            }
            catch
            {
                try
                {
                    //获取不到 从端上取
                    return GetEndKeyAttributeValue(assoObj, keyAttribute);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"既无法从关联型{assoObj.GetType()}的映射字段为{targetField}的属性中获取有效的标识也无法从端对象{EntityType.ClrType}中获取有效的标识,请检查此关联型的赋值.",
                        nameof(keyAttribute), ex);
                }
            }
        }

        /// <summary>
        ///     获取端对象的键值
        /// </summary>
        /// <param name="assoObj">端对象</param>
        /// <param name="keyAttribute">键属性的名称</param>
        /// <returns></returns>
        private object GetEndKeyAttributeValue(object assoObj, string keyAttribute)
        {
            //从端对象获取标识属性的值
            var endObj = GetValue(assoObj);
            var attr = EntityType.GetAttribute(keyAttribute);
            return attr.GetValue(endObj);
        }

        /// <summary>
        ///     判断是否为类型的默认值
        /// </summary>
        /// <param name="value">要判断的值</param>
        /// <returns></returns>
        private bool IsDefaultValue(object value)
        {
            //支持Obase基元类型
            switch (value)
            {
                case ushort uShortValue:
                    return uShortValue == 0;
                case short shortValue:
                    return shortValue == 0;
                case uint uIntValue:
                    return uIntValue == 0;
                case int intValue:
                    return intValue == 0;
                case ulong uLongValue:
                    return uLongValue == 0;
                case long longValue:
                    return longValue == 0;
                case char charValue:
                    return charValue == 0;
                case double doubleValue:
                    return doubleValue == 0;
                case float floatValue:
                    return floatValue == 0;
                case string stringValue:
                    return string.IsNullOrEmpty(stringValue);
                case DateTime dateTimeValue:
                    return dateTimeValue == default;
                case Guid guid:
                    return Guid.Empty.Equals(guid);
                default:
                    return value == null;
            }
        }

        /// <summary>
        ///     字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"AssociationEnd:{{Name-\"{Name}\",EntityType-\"{EntityType}\"}}";
        }
    }
}