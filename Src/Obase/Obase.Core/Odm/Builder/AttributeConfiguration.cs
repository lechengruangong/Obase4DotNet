/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举属性的合并处理策略.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:13:27
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     属性的配置项
    /// </summary>
    /// <typeparam name="TStructural">属性所属于的类型</typeparam>
    public class
        AttributeConfiguration<TStructural> : TypeElementConfiguration<TStructural,
        AttributeConfiguration<TStructural>>, IAttributeConfigurator
    {
        /// <summary>
        ///     数据类型（对应数据库类型，取值器取出的数据类型要和数据库字段兼容）
        /// </summary>
        internal readonly Type DataType;


        /// <summary>
        ///     属性的合并处理器。
        /// </summary>
        private IAttributeCombinationHandler _attributeCombinationHandler = new OverwriteCombinationHandler();

        /// <summary>
        ///     修改触发器集合
        /// </summary>
        private List<IBehaviorTrigger> _changeTriggers;

        /// <summary>
        ///     指示是否为复杂属性。
        /// </summary>
        private bool _isComplex;

        /// <summary>
        ///     映射连接符。
        /// </summary>
        private char _mappingConnectionChar = char.MinValue;

        /// <summary>
        ///     字符串的最大长度（字符数），仅当属性类型为字符串时有效。
        /// </summary>
        private ushort _maxCharNumber;

        /// <summary>
        ///     指示字段值是否可空。
        /// </summary>
        private bool _nullable = true;

        /// <summary>
        ///     值的精度，以小数位数表示，0表示不限制。
        ///     仅限于映射类型为decimal的使用
        ///     这个精度指的是小数点后的长度
        ///     对于
        /// </summary>
        private byte _precision;

        /// <summary>
        ///     对属性值实施序列化和反序列化的程序
        /// </summary>
        private ITextSerializer _serializer;

        /// <summary>
        ///     类型的原始类型
        /// </summary>
        private Type _valueType;

        /// <summary>
        ///     映射字段（数据库字段名，用以从sql读取器取值）
        /// </summary>
        internal string TargetField;

        /// <summary>
        ///     属性配置项
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="dataType">属性类型</param>
        /// <param name="typeConfiguration">创建当前属性配置项的类型配置项。</param>
        protected AttributeConfiguration(string name, Type dataType, StructuralTypeConfiguration typeConfiguration) :
            base(name,
                false, typeConfiguration)
        {
            DataType = dataType;
            ElementType = EElementType.Attribute;
            //不是Obase基元类型
            if (!PrimitiveType.IsObasePrimitiveType(dataType))
                _isComplex = true;
        }

        /// <summary>
        ///     获取元素类型。
        /// </summary>
        public override EElementType ElementType { get; }

        /// <summary>
        ///     修改触发器集合(修改代理类属性时将对象状态标为修改)
        /// </summary>
        public override List<IBehaviorTrigger> BehaviorTriggers =>
            _changeTriggers ?? (_changeTriggers = new List<IBehaviorTrigger>());

        /// <summary>
        ///     取值器
        /// </summary>
        protected override IValueGetter ValueGetter
        {
            get
            {
                //如果已启用序列化且传入的取值器不为SerializedValueGetter，使用传入的取值器构造SerializedValueGetter，并将其作为实际取值
                // 器。如果传入的取值器是SerializedValueGetter，直接使用该取值器。
                if (_serializer != null && !(base.ValueGetter is SerializedValueGetter))
                {
                    if (base.ValueGetter == null)
                        throw new ArgumentNullException(nameof(ValueGetter), "设置属性的序列化器前必须先设置设值器.");
                    return new SerializedValueGetter(base.ValueGetter, _serializer);
                }

                return base.ValueGetter;
            }
        }

        /// <summary>
        ///     设值器
        /// </summary>
        protected override IValueSetter ValueSetter
        {
            get
            {
                // 如果已启用序列化且传入的设值器不为SerializedValueSetter，使用传入的设值器构造SerializedValueSetter，并将其作为实际设值
                // 器。如果传入的设值器是SerializedValueSetter，直接使用该设值器。
                if (_serializer != null && !(base.ValueSetter is SerializedValueSetter))
                {
                    if (base.ValueGetter == null)
                        throw new ArgumentNullException(nameof(ValueGetter), "设置属性的序列化器前必须先设置设值器.");
                    return new SerializedValueSetter(base.ValueSetter, _serializer, _valueType);
                }

                return base.ValueSetter;
            }
        }

        /// <summary>
        ///     使用一个能触发属性修改的方法为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发属性修改的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasChangeTrigger(MethodInfo method, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
                BehaviorTriggers.Clear();
            HasChangeTrigger(method);
        }

        /// <summary>
        ///     使用一个能触发属性修改的属性访问器为属性创建Property-Set型修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发属性修改的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasChangeTrigger(PropertyInfo property, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
                BehaviorTriggers.Clear();
            HasChangeTrigger(property, EBehaviorTriggerType.PropertySet);
        }

        /// <summary>
        ///     使用一个能触发属性修改的属性访问器为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发属性修改的属性访问器。</param>
        /// <param name="triggerType">要创建的触发器类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasChangeTrigger(PropertyInfo property, EBehaviorTriggerType triggerType,
            bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
                BehaviorTriggers.Clear();
            HasChangeTrigger(property, triggerType);
        }

        /// <summary>
        ///     使用一个能触发属性修改的成员为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的触发器类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasChangeTrigger(string memberName, EBehaviorTriggerType triggerType,
            bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
                BehaviorTriggers.Clear();
            HasChangeTrigger(memberName, triggerType);
        }

        /// <summary>
        ///     使用与属性同名的成员为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的触发器类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasChangeTrigger(EBehaviorTriggerType triggerType, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
                BehaviorTriggers.Clear();
            HasChangeTrigger(_name, triggerType);
        }

        /// <summary>
        ///     使用与属性同名的属性访问器为属性创建Property-Set型修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasChangeTrigger(bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
                BehaviorTriggers.Clear();
            HasChangeTrigger(_name, EBehaviorTriggerType.PropertySet);
        }

        /// <summary>
        ///     设置修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="changeTrigger">修改触发器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasChangeTrigger(IBehaviorTrigger changeTrigger, bool overrided)
        {
            //每次调用本方法，如果override为false将追加一个触发器，为true将清空之前的所有设置。
            if (overrided)
                BehaviorTriggers.Clear();
            HasChangeTrigger(changeTrigger);
        }

        /// <summary>
        ///     设置属性的合并处理器。
        /// </summary>
        /// <param name="combiner">属性的合并处理器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasCombinationHandler(IAttributeCombinationHandler combiner, bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
            {
                HasCombinationHandler(combiner);
            }
            else
            {
                //等于OverwriteCombinationHandler 未设置 才设置
                if (_attributeCombinationHandler.GetType() == typeof(OverwriteCombinationHandler))
                    HasCombinationHandler(combiner);
            }
        }

        /// <summary>
        ///     设置与指定的属性合并处理策略对应的合并处理器。
        /// </summary>
        /// <param name="strategy">属性的合并处理策略。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasCombinationHandler(EAttributeCombinationHandlingStrategy strategy,
            bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
            {
                HasCombinationHandler(strategy);
            }
            else
            {
                //等于OverwriteCombinationHandler 未设置 才设置
                if (_attributeCombinationHandler.GetType() == typeof(OverwriteCombinationHandler))
                    HasCombinationHandler(strategy);
            }
        }

        /// <summary>
        ///     设置映射连接符。
        /// </summary>
        /// <param name="value">连接符的值</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.HasMappingConnectionChar(char value, bool overrided)
        {
            if (overrided)
            {
                HasMappingConnectionChar(value);
            }
            else
            {
                if (_mappingConnectionChar == char.MinValue)
                    HasMappingConnectionChar(value);
            }
        }

        /// <summary>
        ///     设置映射字段。
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAttributeConfigurator.ToField(string field, bool overrided)
        {
            if (overrided)
            {
                ToField(field);
            }
            else
            {
                if (string.IsNullOrEmpty(TargetField))
                    ToField(field);
            }
        }

        /// <summary>
        ///     最大字符数
        ///     仅限字符串类型
        /// </summary>
        /// <param name="maxcharNumber">最大字符数 只有1到255是有效的 如果设置为0 会被设置为255 如果超过255 会被设置为Text字段</param>
        /// <param name="overrided">是否覆盖</param>
        /// <returns></returns>
        void IAttributeConfigurator.HasMaxcharNumber(ushort maxcharNumber, bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
            {
                HasMaxcharNumber(maxcharNumber);
            }
            else
            {
                //等于0 未设置 才设置
                if (_maxCharNumber == 0)
                    HasMaxcharNumber(maxcharNumber);
            }
        }

        /// <summary>
        ///     设置精度
        ///     只支持为映射类型decimal设置精度
        /// </summary>
        /// <param name="precision">以小数位数表示的精度，0表示小数点后没有位数。精度最大值28</param>
        /// <param name="overrided">是否覆盖</param>
        /// <returns></returns>
        void IAttributeConfigurator.HasPrecision(byte precision, bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
            {
                HasPrecision(precision);
            }
            else
            {
                //等于0 未设置 才设置
                if (_precision == 0)
                    HasPrecision(precision);
            }
        }

        /// <summary>
        ///     设置是否可空
        /// </summary>
        /// <param name="value">指示是否可空。对于主键设置为可空是无效的</param>
        /// <param name="overrided">是否覆盖</param>
        /// <returns></returns>
        void IAttributeConfigurator.HasNullable(bool value, bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
            {
                HasNullable(value);
            }
            else
            {
                //等于true 未设置 才设置
                if (_nullable)
                    HasNullable(value);
            }
        }


        /// <summary>
        ///     使用一个能触发属性修改的方法为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发属性修改的方法。</param>
        public AttributeConfiguration<TStructural> HasChangeTrigger(MethodInfo method)
        {
            //追加一个方法触发器
            var methodTrigger = new MethodTrigger(method);
            return HasChangeTrigger(methodTrigger);
        }

        /// <summary>
        ///     设置修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="changeTrigger">修改触发器</param>
        public AttributeConfiguration<TStructural> HasChangeTrigger(
            IBehaviorTrigger changeTrigger)
        {
            //不包含触发器时才添加
            if (!BehaviorTriggers.Contains(changeTrigger))
                BehaviorTriggers.Add(changeTrigger);
            return this;
        }

        /// <summary>
        ///     使用与属性同名的属性访问器为属性创建Property-Set型修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        public AttributeConfiguration<TStructural> HasChangeTrigger()
        {
            return HasChangeTrigger(_name, EBehaviorTriggerType.PropertySet);
        }

        /// <summary>
        ///     使用一个能触发属性修改的属性访问器为属性创建Property-Set型修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发属性修改的属性访问器。</param>
        public void HasChangeTrigger(PropertyInfo property)
        {
            HasChangeTrigger(property, EBehaviorTriggerType.PropertySet);
        }

        /// <summary>
        ///     使用一个能触发属性修改的成员为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的触发器类型。</param>
        public AttributeConfiguration<TStructural> HasChangeTrigger(string memberName,
            EBehaviorTriggerType triggerType)
        {
            var property = typeof(TStructural).GetProperty(memberName);
            return HasChangeTrigger(property, triggerType);
        }


        /// <summary>
        ///     使用与属性同名的成员为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的触发器类型。</param>
        public AttributeConfiguration<TStructural> HasChangeTrigger(
            EBehaviorTriggerType triggerType)
        {
            return HasChangeTrigger(_name, triggerType);
        }

        /// <summary>
        ///     使用一个能触发属性修改的属性访问器为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        ///     类型参数：
        ///     TProperty  属性访问器的类型
        /// </summary>
        /// <param name="propertyExp">表示属性访问器的Lambda表达式。</param>
        /// <param name="triggerType">要创建的触发器的类型。</param>
        public AttributeConfiguration<TStructural> HasChangeTrigger<TProperty>(
            Expression<Func<TStructural, TProperty>> propertyExp,
            EBehaviorTriggerType triggerType = EBehaviorTriggerType.PropertySet) where TProperty : struct
        {
            //解析表达式
            var member = (MemberExpression)propertyExp.Body;
            return HasChangeTrigger(member.Member.Name, triggerType);
        }

        /// <summary>
        ///     使用一个能触发属性修改的属性访问器为属性创建修改触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发属性修改的属性访问器。</param>
        /// <param name="triggerType">要创建的触发器类型。</param>
        public AttributeConfiguration<TStructural> HasChangeTrigger(PropertyInfo property,
            EBehaviorTriggerType triggerType)
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


            return HasChangeTrigger(method);
        }

        /// <summary>
        ///     设置映射字段。
        /// </summary>
        /// <param name="field">映射字段</param>
        public AttributeConfiguration<TStructural> ToField(string field)
        {
            TargetField = field;
            return this;
        }

        /// <summary>
        ///     设置属性的合并处理器。
        /// </summary>
        /// <param name="combiner">属性的合并处理器。</param>
        public AttributeConfiguration<TStructural> HasCombinationHandler(
            IAttributeCombinationHandler combiner)
        {
            _attributeCombinationHandler = combiner;
            return this;
        }

        /// <summary>
        ///     设置映射连接符。
        /// </summary>
        /// <param name="value">映射连接符</param>
        public AttributeConfiguration<TStructural> HasMappingConnectionChar(char value)
        {
            _mappingConnectionChar = value;
            return this;
        }


        /// <summary>
        ///     设置与指定的属性合并处理策略对应的合并处理器。
        /// </summary>
        /// <param name="strategy">属性的合并处理策略。</param>
        public AttributeConfiguration<TStructural> HasCombinationHandler(
            EAttributeCombinationHandlingStrategy strategy)
        {
            switch (strategy)
            {
                case EAttributeCombinationHandlingStrategy.Overwrite:
                    _attributeCombinationHandler = new OverwriteCombinationHandler();
                    break;
                case EAttributeCombinationHandlingStrategy.Ignore:
                    _attributeCombinationHandler = new IgnoreCombinationHandler();
                    break;
                case EAttributeCombinationHandlingStrategy.Accumulate:
                    _attributeCombinationHandler = new AccumulateCombinationHandler();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), $"未知的合并策略{strategy}");
            }

            return this;
        }

        /// <summary>
        ///     最大字符数
        ///     仅限字符串类型
        /// </summary>
        /// <param name="maxcharNumber">最大字符数 只有1到255是有效的 如果设置为0 会被设置为255 如果超过255 会被设置为Text字段</param>
        /// <returns></returns>
        public AttributeConfiguration<TStructural> HasMaxcharNumber(ushort maxcharNumber)
        {
            if (DataType != typeof(string))
                throw new ArgumentException("只支持为映射类型string设置最大字符数");

            _maxCharNumber = (ushort)(maxcharNumber * 8);
            return this;
        }

        /// <summary>
        ///     设置精度
        ///     只支持为映射类型decimal设置精度
        /// </summary>
        /// <param name="precision">以小数位数表示的精度，0表示小数点后没有位数。精度最大值28</param>
        /// <returns></returns>
        public AttributeConfiguration<TStructural> HasPrecision(byte precision)
        {
            //只支持为映射类型decimal设置精度
            if (DataType != typeof(decimal))
                throw new ArgumentException("只支持为映射类型decimal设置精度");
            //精度不可为负值
            if (precision < 0)
                throw new ArgumentException("映射类型decimal设置精度不可为负值");
            //精度最大值28
            if (precision > 28)
                throw new ArgumentException("映射类型decimal设置精度最大值28");

            _precision = precision;
            return this;
        }

        /// <summary>
        ///     设置是否可空
        /// </summary>
        /// <param name="value">指示是否可空。对于主键设置为可空是无效的</param>
        public AttributeConfiguration<TStructural> HasNullable(bool value)
        {
            _nullable = value;
            return this;
        }

        /// <summary>
        ///     使用一个能够为类型元素设值的委托为类型元素创建设值器。
        /// </summary>
        /// <typeparam name="TProperty">属性的数据类型</typeparam>
        /// <param name="setValue">为类型元素设值的委托。</param>
        public AttributeConfiguration<TStructural> HasValueSetter<TProperty>(
            Action<TStructural, TProperty> setValue)
        {
            return HasValueSetter(Odm.ValueSetter.Create(setValue, EValueSettingMode.Assignment));
        }

        /// <summary>
        ///     使用自定义的的序列化方案，对当前属性启用序列化。
        /// </summary>
        /// <param name="serializer">自定义的序列化器</param>
        /// <param name="valueType">要序列化的原始类型</param>
        /// <returns>当前属性配置</returns>
        public AttributeConfiguration<TStructural> UseSerializer(ITextSerializer serializer, Type valueType)
        {
            _valueType = valueType;
            _serializer = serializer;
            return this;
        }

        /// <summary>
        ///     使用自定义的的序列化方案，对当前属性启用序列化。
        /// </summary>
        /// <typeparam name="TValue">要序列化的原始类型</typeparam>
        /// <param name="serializer">自定义的序列化器</param>
        /// <returns>当前属性配置</returns>
        public AttributeConfiguration<TStructural> UseSerializer<TValue>(ITextSerializer serializer)
        {
            _valueType = typeof(TValue);
            _serializer = serializer;
            return this;
        }

        /// <summary>
        ///     使用预制的序列化方案基类，对当前属性启用序列化。
        /// </summary>
        /// <param name="serializer">实现序列化方案基类的序列化方案</param>
        /// <param name="valueType">要序列化的原始类型</param>
        /// <returns>当前属性配置</returns>
        public AttributeConfiguration<TStructural> UseSerializer(TextSerializer serializer, Type valueType)
        {
            _valueType = valueType;
            _serializer = serializer;
            return this;
        }

        /// <summary>
        ///     使用预制的序列化方案基类，对当前属性启用序列化。
        /// </summary>
        /// <typeparam name="TValue">要序列化的原始类型</typeparam>
        /// <param name="serializer">实现序列化方案基类的序列化方案</param>
        /// <returns>当前属性配置</returns>
        public AttributeConfiguration<TStructural> UseSerializer<TValue>(TextSerializer serializer)
        {
            _valueType = typeof(TValue);
            _serializer = serializer;
            return this;
        }

        /// <summary>
        ///     根据属性配置项创建属性元素实例。
        /// </summary>
        protected override TypeElement CreateReally(ObjectDataModel objectModel)
        {
            //根据配置项数据创建模型对象并设值
            if (string.IsNullOrWhiteSpace(Name) || DataType == null) return null;

            //尝试从对象数据模型获取对应的复杂类型模型
            var complexType = objectModel.GetComplexType(DataType);
            //补充一次判断
            _isComplex = complexType != null;
            var attribute = complexType != null
                ? new ComplexAttribute(DataType, Name, complexType)
                : new Attribute(DataType, Name);

            if (string.IsNullOrEmpty(TargetField)) TargetField = Name;
            //赋值属性
            attribute.TargetField = TargetField;
            attribute.ChangeTriggers = BehaviorTriggers;
            attribute.ValueGetter = ValueGetter;
            attribute.ValueSetter = ValueSetter;
            //属性合并处理器
            attribute.CombinationHandler = _attributeCombinationHandler;
            attribute.IsComplex = _isComplex;
            //映射连接字符
            if (attribute is ComplexAttribute complexAttribute)
                complexAttribute.MappingConnectionChar = _mappingConnectionChar;
            //字段精度和是否可空
            attribute.ValueLength = _maxCharNumber;
            attribute.Nullable = _nullable;
            attribute.Precision = _precision;
            return attribute;
        }
    }

    /// <summary>
    ///     泛型的属性配置项
    /// </summary>
    /// <typeparam name="TStructural">属性所属的类型</typeparam>
    /// <typeparam name="TTypeConfiguration">创建当前属性配置项的类型配置项的类型</typeparam>
    public class AttributeConfiguration<TStructural, TTypeConfiguration> : AttributeConfiguration<TStructural>
        where TTypeConfiguration : StructuralTypeConfiguration
    {
        /// <summary>
        ///     属性配置项
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="dataType">属性类型</param>
        /// <param name="typeConfiguration">创建当前属性配置项的类型配置项。</param>
        public AttributeConfiguration(string name, Type dataType, TTypeConfiguration typeConfiguration) : base(name,
            dataType, typeConfiguration)
        {
        }

        /// <summary>
        ///     进入当前属性所属类型的配置项。
        /// </summary>
        public TTypeConfiguration Upward()
        {
            return (TTypeConfiguration)_typeConfiguration;
        }
    }
}