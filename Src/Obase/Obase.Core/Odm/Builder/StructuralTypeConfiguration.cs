/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构化类型,提供结构化配置基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:32:02
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     为实体型配置项、关联型配置项和复杂类型配置项提供基础实现。
    /// </summary>
    public abstract class StructuralTypeConfiguration
    {
        /// <summary>
        ///     过滤属性名集合
        /// </summary>
        protected readonly List<string> _ignoreList = new List<string>();

        /// <summary>
        ///     建模器
        /// </summary>
        private readonly ModelBuilder _modelBuilder;

        /// <summary>
        ///     类型扩展配置器。
        /// </summary>
        protected readonly List<TypeExtensionConfiguration> ExtensionConfigs = new List<TypeExtensionConfiguration>();

        /// <summary>
        ///     类型的CLR类型
        /// </summary>
        protected Type _clrType;

        /// <summary>
        ///     具体类型判别器
        /// </summary>
        protected IConcreteTypeDiscriminator _concreteTypeDiscriminator;

        /// <summary>
        ///     判别类型标记
        ///     即判别字段的名称和判别字段的值
        /// </summary>
        protected Tuple<string, object> _concreteTypeSign;

        /// <summary>
        ///     根据当前配置信息生成的类型。
        /// </summary>
        protected StructuralType _createdType;

        /// <summary>
        ///     基类型。
        /// </summary>
        protected Type _derivingFrom;

        /// <summary>
        ///     当前的外键定义器
        /// </summary>
        private ForeignKeyAdder _foreignKeyAdder;

        /// <summary>
        ///     用于判断类型的字段名称
        /// </summary>
        protected string _typeAttributeName;

        /// <summary>
        ///     类型的实例构造器
        /// </summary>
        protected internal IInstanceConstructor Constructor;

        /// <summary>
        ///     类型的名称
        /// </summary>
        protected string Name;

        /// <summary>
        ///     类型的命名空间
        /// </summary>
        protected string Namespace;

        /// <summary>
        ///     新实例构造函数
        /// </summary>
        protected IInstanceConstructor NewInstanceConstructor;

        /// <summary>
        ///     代理类型，如果未生成代理类则为null
        /// </summary>
        protected Type ProxyClrType;


        /// <summary>
        ///     触发器集合
        /// </summary>
        protected Dictionary<IBehaviorTrigger, List<TypeElementConfiguration>> TriggerElems;

        /// <summary>
        ///     创建StructuralTypeConfiguration的实例。
        /// </summary>
        /// <param name="modelBuilder">指定类型配置项所属的建模器</param>
        protected StructuralTypeConfiguration(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        /// <summary>
        ///     标识属性集合
        /// </summary>
        protected internal abstract List<string> KeyAttributes { get; }

        /// <summary>
        ///     建模器
        /// </summary>
        public ModelBuilder ModelBuilder => _modelBuilder;

        /// <summary>
        ///     获取类型各元素上设置的行为触发器，注：相同的触发器只返回一个实例。
        /// </summary>
        public List<IBehaviorTrigger> BehaviorTriggers
        {
            get
            {
                //没有触发器元素时加载触发器元素
                if (TriggerElems == null) LoadTriggerElems();
                var result = TriggerElems == null ? new List<IBehaviorTrigger>() : TriggerElems.Keys.ToList();
                if (TriggerElems != null)
                {
                    //如果是派生类型，则添加基类型的触发器
                    var baseTypeConfiguration = _modelBuilder.FindConfiguration(_derivingFrom);
                    result.AddRange(baseTypeConfiguration.BehaviorTriggers);
                }

                return result;
            }
        }

        /// <summary>
        ///     类型的CLR类型
        /// </summary>
        public Type ClrType
        {
            get => _clrType;
            set => _clrType = value;
        }

        /// <summary>
        ///     获取所有的元素配置项，包括属性配置项、关联引用配置项、关联端配置项。
        /// </summary>
        public abstract Dictionary<string, TypeElementConfiguration> ElementConfigurations { get; set; }

        /// <summary>
        ///     当前的外键定义器
        /// </summary>
        internal ForeignKeyAdder Adder
        {
            get => _foreignKeyAdder;
            set => _foreignKeyAdder = value;
        }

        /// <summary>
        ///     获取创建的类型
        /// </summary>
        public StructuralType CreatedType => _createdType;

        /// <summary>
        ///     具体类型判别器
        /// </summary>
        public IConcreteTypeDiscriminator ConcreteTypeDiscriminator => _concreteTypeDiscriminator;

        /// <summary>
        ///     用于判断类型的字段名称
        /// </summary>
        public string TypeAttributeName => _typeAttributeName;

        /// <summary>
        ///     判别类型标记
        ///     即判别字段的名称和判别字段的值
        /// </summary>
        protected Tuple<string, object> ConcreteTypeSign => _concreteTypeSign;

        /// <summary>
        ///     继承自谁
        /// </summary>
        public Type DerivedFrom => _derivingFrom;

        /// <summary>
        ///     过滤属性名集合
        /// </summary>
        public List<string> IgnoreList => _ignoreList;

        /// <summary>
        ///     设置类型的命名空间。
        /// </summary>
        /// <param name="nameSpace"></param>
        public StructuralTypeConfiguration HasNamespace(string nameSpace)
        {
            Namespace = nameSpace;
            return this;
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型。
        /// </summary>
        internal StructuralType Create(ObjectDataModel buidingModel)
        {
            //调用实现类的CreateReally方法构建模型类型
            var structuralType = CreateReally(buidingModel);
            //获取当前的类型扩展 并设置到模型类型中
            var typeExtensions = ExtensionConfigs;
            foreach (var typeExtensionConfiguration in typeExtensions)
                structuralType.AddExtension(typeExtensionConfiguration.MakeExtension());
            //设置判别标识
            structuralType.ConcreteTypeSign = ConcreteTypeSign;
            //为根据当前配置信息生成的类型赋值
            _createdType = structuralType;
            return _createdType;
        }

        /// <summary>
        ///     遍历元素配置项，根据配置项中的元数据生成元素实例，并添加到指定的模型类型实例中。
        /// </summary>
        /// <param name="objectModel">模型类型实例，是即将生成的元素实例的宿主</param>
        internal void CreateElements(ObjectDataModel objectModel)
        {
            var modelType = objectModel.GetStructuralType(_clrType);
            //遍历类型的元素配置项
            foreach (var item in ElementConfigurations)
            {
                //创建元素模型对象
                var typeElement = item.Value.Create(objectModel);
                modelType.AddElement(typeElement);
            }
        }

        /// <summary>
        ///     加载触发器
        /// </summary>
        private void LoadTriggerElems()
        {
            //触发器元素字典
            TriggerElems = new Dictionary<IBehaviorTrigger, List<TypeElementConfiguration>>();
            //遍历元素配置项
            foreach (var element in ElementConfigurations)
                //遍历元素的触发器
            foreach (var tri in element.Value.BehaviorTriggers)
                //添加到字典
                if (!TriggerElems.ContainsKey(tri))
                    TriggerElems.Add(tri, new List<TypeElementConfiguration> { element.Value });
                else
                    TriggerElems[tri].Add(element.Value);
        }

        /// <summary>
        ///     作补充管道的操作
        /// </summary>
        /// <param name="complementConfigurator"></param>
        internal void ConfigurateComplement(IComplementConfigurator complementConfigurator)
        {
            //有补充 先做补充
            var pipeLine = complementConfigurator;
            while (pipeLine != null)
            {
                pipeLine.Configurate(_createdType, this);
                pipeLine = pipeLine.Next;
            }
        }

        /// <summary>
        ///     获取行为触发器触发的对象行为所涉及到的元素。（有触发器的元素配置项）
        /// </summary>
        /// <param name="trigger">指定的触发器实例。</param>
        protected List<TypeElementConfiguration> GetBehaviorElements(IBehaviorTrigger trigger)
        {
            //获取所有的触发器元素
            var triggerElems = GetTriggerElems();
            if (triggerElems != null && triggerElems.ContainsKey(trigger))
            {
                //排序触发器元素
                triggerElems[trigger].Sort((x, y) =>
                {
                    //设置一个默认的加载优先级
                    var xloadingPriority = 99999;
                    var yloadingPriority = 99999;
                    //根据ILazyLoadingConfiguration接口获取加载优先级
                    if (x is ILazyLoadingConfiguration configuration)
                        xloadingPriority = configuration.LoadingPriority;
                    if (y is ILazyLoadingConfiguration loadingConfiguration)
                        yloadingPriority = loadingConfiguration.LoadingPriority;
                    //比较加载优先级
                    return xloadingPriority - yloadingPriority;
                });
                return triggerElems[trigger];
            }

            return new List<TypeElementConfiguration>();
        }

        /// <summary>
        ///     获取所有的触发器
        /// </summary>
        /// <returns></returns>
        private Dictionary<IBehaviorTrigger, List<TypeElementConfiguration>> GetTriggerElems()
        {
            //如果触发器元素字典为null，则加载触发器元素
            if (TriggerElems == null)
                LoadTriggerElems();

            var triggerElems = TriggerElems ?? new Dictionary<IBehaviorTrigger, List<TypeElementConfiguration>>();
            if (_derivingFrom != null)
            {
                //如果是派生类型，则添加基类型的触发器
                var baseTypeConfiguration = ModelBuilder.FindConfiguration(_derivingFrom);
                foreach (var elem in baseTypeConfiguration.GetTriggerElems())
                    //如果触发器元素字典中没有该触发器，则添加
                    if (!triggerElems.ContainsKey(elem.Key))
                        triggerElems.Add(elem.Key, elem.Value);
            }

            return triggerElems;
        }

        /// <summary>
        ///     创建指定对象类型的代理类型。
        /// </summary>
        /// <summary>
        ///     创建指定对象类型的代理类型。
        ///     实施说明
        ///     调用ImpliedTypeManager.ApplyType(baseType, interfaces, defineMembers)，其中：
        ///     （1）baseType的实参为objectType；
        ///     （2）interfaces的实参为typeof(IIntervene)；
        ///     （3）defineMembers委托代表的方法参见顺序图“定义代理类型的成员”，其中modelType为闭包成员。
        /// </summary>
        internal Type CreateProxyType(IProxyTypeGenerator generationPipeline)
        {
            //本地方法作为委托
            void DefineMembers(TypeBuilder builder)
            {
                //定义代理类型的构造函数
                DefineProxyTypeConstructor(builder);

                var pipeLine = generationPipeline;
                while (pipeLine != null)
                {
                    //外部已经检测过类型 此处强转即可
                    pipeLine.DefineMembers(builder, (ObjectType)_createdType, (IObjectTypeConfigurator)this);
                    pipeLine = pipeLine.Next;
                }
            }

            try
            {
                //生成一个Type
                return ImpliedTypeManager.Current.ApplyType(_createdType.ClrType, new[] { typeof(IIntervene) },
                        DefineMembers);
            }
            catch (TypeLoadException ex)
            {
                throw new InvalidOperationException(
                    $"无法为{_createdType.ClrType.Name}创建代理类型,请参照内部异常,如果为无法创建代理类则在{_createdType.ClrType.Name}所属的命名空间上使用标记 [assembly: InternalsVisibleTo(\"ObaseProxyModule\")] 或 将{_createdType.ClrType.Name}内的触发属性改非internal",
                    ex);
            }
        }

        /// <summary>
        ///     定义代理类型的构造函数
        /// </summary>
        /// <param name="typeBuilder">代理类型的建造器</param>
        private void DefineProxyTypeConstructor(TypeBuilder typeBuilder)
        {
            //原始类型的构造函数们
            var baseCtors =
                _createdType.ClrType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic |
                                                     BindingFlags.Public);
            foreach (var ctor in baseCtors)
            {
                //参数
                var ctorPara = ctor.GetParameters();
                //定义构造函数
                var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                    CallingConventions.Standard, ctorPara.Select(p => p.ParameterType).ToArray());
                for (var i = 0; i < ctorPara.Length; i++)
                    ctorBuilder.DefineParameter(i + 1, ctorPara[i].Attributes, ctorPara[i].Name);
                //重新压栈
                var ctorIl = ctorBuilder.GetILGenerator();
                ctorIl.Emit(OpCodes.Ldarg_0);
                for (var i = 0; i < ctorPara.Length; i++) ctorIl.Emit(OpCodes.Ldarg, i + 1);
                ctorIl.Emit(OpCodes.Call, ctor);
                ctorIl.Emit(OpCodes.Ret);
            }
        }

        /// <summary>
        ///     为类型配置项设置一个扩展配置器，如果指定类型的配置器已存在，返回该配置器。
        /// </summary>
        /// <param name="configType">扩展配置器的类型，须继承自TypeExtensionConfiguration。</param>
        public TypeExtensionConfiguration HasExtension(Type configType)
        {
            try
            {
                //检查是否已经配置 如果有 则不进行配置
                var ext = ExtensionConfigs.FirstOrDefault(p => p.GetType() == configType);
                if (ext != null)
                    return ext;
                //创建一个TypeExtensionConfiguration
                var extensionConfiguration =
                    (TypeExtensionConfiguration)Activator.CreateInstance(configType);
                //加入到扩展配置器列表中
                ExtensionConfigs.Add(extensionConfiguration);
                return extensionConfiguration;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展配置器失败,{configType.Name}没有适合的无参构造函数", nameof(configType), e);
            }
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型
        ///     本方法由派生类实现
        /// </summary>
        /// <returns></returns>
        protected abstract StructuralType CreateReally(ObjectDataModel buidingModel);

        /// <summary>
        ///     创建隐式关联型建造器
        /// </summary>
        /// <returns></returns>
        protected internal abstract void CreateImplicitAssociationConfiguration();

        /// <summary>
        ///     通过反射从指定的CLR类型中收集元数据，生成元素配置项。
        ///     实施说明
        ///     参见活动图“反射建模-配置元素算法”。
        /// </summary>
        /// <param name="analyticPipeline">类型成员解析管道。</param>
        internal abstract void ReflectionModeling(ITypeMemberAnalyzer analyticPipeline);

        /// <summary>
        ///     根据类型配置项中的元数据配置模型类型，被配置的模型类型已根据当前类型配置项实例生成并已注册到指定的模型中。
        ///     注：调用方调用Create方法创建模型类型时，由于类型的元素还未创建，因此某些属性可能无法当场配置，可以等到类型元素创建（CreateElement被调用）完成
        ///     时，调用本方法完成类型配置。
        /// </summary>
        /// <param name="model">要配置的类型所属的模型。</param>
        internal abstract void Configurate(ObjectDataModel model);


        /// <summary>
        ///     根据名称获取元素配置器。
        /// </summary>
        /// <param name="name">元素名称。</param>
        public abstract ITypeElementConfigurator GetElement(string name);

        /// <summary>
        ///     通过反射从CLR类型中收集元数据，生成类型配置项。
        /// </summary>
        /// <param name="analyticPipeline">类型解析管道。</param>
        internal abstract void ReflectionModeling(ITypeAnalyzer analyticPipeline);

        /// <summary>
        ///     将基类型缺失的外键属性定义到代理类型。
        ///     实施说明
        ///     为每一属性（Attribute）定义一个公有字段，字段名称为属性名。
        ///     为每一属性（Attribute）设置取值器和设置器，使用委托取/设值器。委托可基于访问上述字段的MemberExpression生成。
        /// </summary>
        public class ForeignKeyAdder : ForeignKeyGuarantor
        {
            /// <summary>
            ///     基类型的模型类型
            /// </summary>
            private readonly ObjectType _objType;

            /// <summary>
            ///     代理类型的建造器
            /// </summary>
            private readonly TypeBuilder _proxyTypeBuilder;

            /// <summary>
            ///     被定义的属性
            /// </summary>
            private Attribute[] _definedAttrs;


            /// <summary>
            ///     创建ForeignKeyAdder实例。
            /// </summary>
            /// <param name="objType">基类型的模型类型。</param>
            /// <param name="proxyTypeBuilder">代理类型的建造器。</param>
            public ForeignKeyAdder(ObjectType objType, TypeBuilder proxyTypeBuilder)
            {
                _objType = objType;
                _proxyTypeBuilder = proxyTypeBuilder;
            }

            /// <summary>
            ///     在外键属性缺失的情况下定义所缺的属性。
            /// </summary>
            /// <param name="attrs">要定义的外键属性。</param>
            /// <param name="objType">要定义属性的类型。</param>
            protected override void DefineMissing(Attribute[] attrs, ObjectType objType)
            {
                //处理每个属性
                foreach (var attribute in attrs)
                {
                    //定义一个字段
                    var field = _proxyTypeBuilder.DefineField($"{attribute.Name}", attribute.DataType,
                        FieldAttributes.Public);

                    //构造FieldValueGetter
                    var valueGetter = new FieldValueGetter(field);
                    attribute.ValueGetter = valueGetter;

                    //构造FieldValueSetter
                    var setter = ValueSetter.Create(field);
                    attribute.ValueSetter = setter;
                }

                _definedAttrs = attrs;
            }

            /// <summary>
            ///     在定义了字段后覆盖定义取值和设值器
            /// </summary>
            public void DefineValueGetterAndSetter()
            {
                if (_definedAttrs == null)
                    return;
                foreach (var attribute in _definedAttrs)
                {
                    var field = _objType.RebuildingType.GetField($"{attribute.Name}");
                    //构造FieldValueGetter
                    var valueGetter = new FieldValueGetter(field);
                    attribute.ValueGetter = valueGetter;
                    //构造FieldValueSetter
                    var setter = ValueSetter.Create(field);
                    attribute.ValueSetter = setter;
                    _objType.AddAttribute(attribute);
                }
            }
        }
    }

    /// <summary>
    ///     泛型版本的结构化类型配置项。
    ///     为实体型配置项、关联型配置项和复杂类型配置项提供一个泛型类的基础实现，该泛型类的类型参数是上述三个类型对应的对象系统类型。
    /// </summary>
    public abstract class StructuralTypeConfiguration<TStructural, TConfiguration> : StructuralTypeConfiguration,
        IStructuralTypeConfigurator
        where TConfiguration : StructuralTypeConfiguration<TStructural, TConfiguration>
    {
        /// <summary>
        ///     创建StructuralTypeConfiguration的实例。
        /// </summary>
        /// <param name="modelBuilder">指定类型配置项所属的建模器</param>
        protected StructuralTypeConfiguration(ModelBuilder modelBuilder)
            : base(modelBuilder)
        {
            _clrType = typeof(TStructural);
            Name = _clrType.Name;
        }

        /// <summary>
        ///     启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个。
        /// </summary>
        /// <param name="name">属性名称，它将作为配置项的键</param>
        /// <param name="dataType">属性的数据类型。</param>
        IAttributeConfigurator IStructuralTypeConfigurator.Attribute(string name, Type dataType)
        {
            return Attribute(name, dataType);
        }

        /// <summary>
        ///     启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个。
        ///     类型参数：
        ///     TAttribute    属性的数据类型。
        /// </summary>
        /// <param name="name">属性名称，它将作为配置项的键</param>
        public IAttributeConfigurator Attribute<TAttribute>(string name) where TAttribute : struct
        {
            return Attribute(name);
        }

        /// <summary>
        ///     指定当前类型的基类型。
        /// </summary>
        /// <param name="type">基类型。</param>
        void IStructuralTypeConfigurator.DeriveFrom(Type type)
        {
            DeriveFrom(type);
        }

        /// <summary>
        ///     指定当前类型的基类型。
        ///     类型参数
        ///     TDerived
        ///     基类型。
        /// </summary>
        public void DeriveFrom<TDerived>()
        {
            DeriveFrom(typeof(TDerived));
        }

        /// <summary>
        ///     根据名称获取元素配置器。
        /// </summary>
        /// <param name="name">元素名称。</param>
        ITypeElementConfigurator IStructuralTypeConfigurator.GetElement(string name)
        {
            return GetElement(name);
        }

        /// <summary>
        ///     使用一个构造函数为类型创建实例构造器。
        /// </summary>
        /// <param name="constructorInfo">构造函数。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        IParameterConfigurator IStructuralTypeConfigurator.HasConstructor(ConstructorInfo constructorInfo,
            bool overrided)
        {
            //覆盖既有配置 直接调用配置方法
            if (overrided)
                return HasConstructor(constructorInfo);
            //否则 如果当前配置项没有构造器则创建一个
            if (Constructor == null)
                //反射构造器
                Constructor = new ReflectionConstructor(constructorInfo);
            var parameters = constructorInfo.GetParameters();
            //并且返回构造器参数配置项
            var paraConfiguration =
                new ParameterConfiguration<TStructural, TConfiguration>(parameters, (TConfiguration)this);
            return paraConfiguration;
        }

        /// <summary>
        ///     设置类型的实例构造器。
        /// </summary>
        /// <param name="constructor">实例构造器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IStructuralTypeConfigurator.HasConstructor(IInstanceConstructor constructor, bool overrided)
        {
            //覆盖既有配置 直接调用配置方法
            if (overrided)
                HasConstructor(constructor);
            //否则 如果当前配置项没有构造器则使用传入的构造器
            if (Constructor == null)
                Constructor = constructor;
        }

        /// <summary>
        ///     为类型配置项设置一个扩展配置器 ，如果指定类型的配置器已存在，返回该配置器。
        /// </summary>
        /// <typeparam name="TExtensionConfiguration">扩展配置器的类型，须继承自TypeExtensionConfiguration。</typeparam>
        /// <returns></returns>
        TypeExtensionConfiguration IStructuralTypeConfigurator.HasExtension<TExtensionConfiguration>()
        {
            var extensionConfigurationType = typeof(TExtensionConfiguration);
            try
            {
                //检查是否已经配置 如果有 则不进行配置
                var ext = ExtensionConfigs.FirstOrDefault(p => p.GetType() == extensionConfigurationType);
                if (ext != null)
                    return ext;
                //创建一个TypeExtensionConfiguration
                var extensionConfiguration =
                    (TypeExtensionConfiguration)Activator.CreateInstance(extensionConfigurationType);
                //加入到扩展配置器列表中
                ExtensionConfigs.Add(extensionConfiguration);
                return extensionConfiguration;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展配置器失败,{extensionConfigurationType.Name}没有适合的无参构造函数",
                    nameof(extensionConfigurationType), e);
            }
        }

        /// <summary>
        ///     设置类型的命名空间。
        /// </summary>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="overrided">是否覆盖</param>
        void IStructuralTypeConfigurator.HasNamespace(string nameSpace, bool overrided)
        {
            //如果是覆盖既有配置
            if (overrided)
            {
                //设置命名空间
                HasNamespace(nameSpace);
            }
            else
            {
                //没有才设置
                if (string.IsNullOrEmpty(Namespace))
                    HasNamespace(nameSpace);
            }
        }

        /// <summary>
        ///     启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个。
        ///     <para>注意:此方法为手动配置属性,仅创建属性配置项,不检查参数类型,不配置默认取值器和设值器</para>
        /// </summary>
        /// <param name="name">属性名称，它将作为配置项的键</param>
        /// <param name="dataType">属性的属性类型</param>
        public AttributeConfiguration<TStructural, TConfiguration> Attribute(string name, Type dataType)
        {
            //声明一个属性配置项
            AttributeConfiguration<TStructural, TConfiguration> result;

            //已有配置项
            if (ElementConfigurations.ContainsKey(name))
            {
                //从元素配置项中获取属性配置项
                result = (AttributeConfiguration<TStructural, TConfiguration>)ElementConfigurations[name];
            }
            //没有配置项
            else
            {
                result = new AttributeConfiguration<TStructural, TConfiguration>(name, dataType, (TConfiguration)this);
                //添加到元素配置项
                ElementConfigurations.Add(result.Name, result);
            }

            //返回属性配置项
            return result;
        }

        /// <summary>
        ///     启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个。
        ///     <para>此方法会检查传入名称是否存在于实体中,且使用属性的访问器名称作为属性名称,自动侦测属性类型,并且会尝试自动配置取值器和设值器</para>
        /// </summary>
        /// <param name="name">属性名称，它将作为配置项的键</param>
        public AttributeConfiguration<TStructural, TConfiguration> Attribute(string name)
        {
            return CreateAttributeConfiguration(name, null);
        }

        /// <summary>
        ///     根据Lamda表达式包含的信息启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个
        ///     <para>此方法会检查传入名称是否存在于实体中,且使用属性的访问器名称作为属性名称,自动侦测属性类型,并且会尝试自动配置取值器和设值器</para>
        /// </summary>
        /// <typeparam name="TResult">Lamda表达式的返回值</typeparam>
        /// <param name="expression">lamda表达式</param>
        /// <returns></returns>
        public AttributeConfiguration<TStructural, TConfiguration> Attribute<TResult>(
            Expression<Func<TStructural, TResult>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                //获取表达式代表的属性名称
                var attrName = memberExpression.Member?.Name;
                return CreateAttributeConfiguration(attrName, null);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置属性.");
        }

        /// <summary>
        ///     根据Lamda表达式包含的信息启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个
        ///     <para>此方法会检查传入名称是否存在于实体中,且使用属性的访问器名称作为属性名称,传入的属性类型作为属性的类型,并且会尝试自动配置取值器和设值器</para>
        /// </summary>
        /// <typeparam name="TResult">Lamda表达式的返回值</typeparam>
        /// <param name="expression">lamda表达式</param>
        /// <param name="dataType">属性的属性类型</param>
        /// <returns></returns>
        public AttributeConfiguration<TStructural, TConfiguration> Attribute<TResult>(
            Expression<Func<TStructural, TResult>> expression,
            Type dataType)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                //获取表达式代表的属性名称
                var attrName = memberExpression.Member?.Name;
                return CreateAttributeConfiguration(attrName, dataType);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置属性.");
        }

        /// <summary>
        ///     创建属性配置项
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="dataType">属性数据类型</param>
        /// <returns></returns>
        private AttributeConfiguration<TStructural, TConfiguration> CreateAttributeConfiguration(string name,
            Type dataType)
        {
            //获取反射属性
            var property = typeof(TStructural).GetProperty(name);
            if (property == null)
                throw new ArgumentNullException(nameof(name),$"{typeof(TStructural).FullName}无法找到{name},不能配置为属性.");

            //如果没有传入属性的数据类型 调用类型侦测方法
            if (dataType == null)
                dataType = AttributeTypeConvert(property.PropertyType);

            //已有配置项
            if (ElementConfigurations.ContainsKey(name))
                //直接从元素配置项中获取属性配置项
                return (AttributeConfiguration<TStructural, TConfiguration>)ElementConfigurations[name];

            //创建或获取配置项
            var attribute = Attribute(name, dataType);

            //取值器
            if (property.ReflectedType?.IsValueType == true)
                attribute.HasValueGetter(property);
            else
                attribute.HasValueGetter(property.GetMethod);
            //有设值方法 构造委托设值器
            if (property.SetMethod != null)
            {
                var delType = typeof(Action<,>).MakeGenericType(typeof(TStructural),
                    property.SetMethod.GetParameters()[0].ParameterType);
                var del = property.SetMethod.CreateDelegate(delType);
                attribute.HasValueSetter(ValueSetter.Create(del, EValueSettingMode.Assignment));
            }

            //字段名默认使用属性名
            if (string.IsNullOrEmpty(attribute.TargetField))
                attribute.ToField(name);

            return attribute;
        }

        /// <summary>
        ///     针对未指定具体类型的属性配置时自动侦测的类型是否可配置
        ///     不可配置则抛异常
        ///     可配置则转换为可配置成的属性
        /// </summary>
        /// <param name="targetType">要侦测的目标类型</param>
        /// <returns></returns>
        private Type AttributeTypeConvert(Type targetType)
        {
            //检测类型
            if (targetType == null)
                throw new ArgumentException("不能侦测null的类型.");

            //真实类型 等于目标类型
            var realType = targetType;

            //是否为IEnumerable
            if (targetType.GetInterface("IEnumerable") != null)
            {
                //string就按string配置 真实类型为string
                if (targetType == typeof(string)) realType = typeof(string);

                //只有一个 判断其泛型类型
                if (targetType.GenericTypeArguments?.Length == 1)
                    //泛型类型
                    realType = targetType.GenericTypeArguments[0];

                //小于一个 判断是不是数组
                if (targetType.IsArray)
                    //获取数组元素的类型
                    realType = targetType.GetElementType();
            }

            //再次检测
            if (realType == null)
                throw new ArgumentException("不能侦测null的类型.");

            //如果是Obase的基元类型
            if (PrimitiveType.IsObasePrimitiveType(realType))
                //存在Nullable包装则返回泛型参数 否则返回自身
                return Nullable.GetUnderlyingType(realType) != null
                    ? realType.GenericTypeArguments[0]
                    : realType;

            throw new ArgumentException($"{targetType.FullName}不能拆解为Obase的基元类型,不能配置为属性,请使用带有类型参数的属性配置方法.");
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建实例构造器。
        /// </summary>
        /// <param name="construct">构造类型实例的委托。</param>
        public TConfiguration HasConstructor(Func<TStructural> construct)
        {
            //使用委托创建一个委托构造器
            Constructor = new DelegateConstructor<TStructural>(construct);
            return (TConfiguration)this;
        }

        /// <summary>
        ///     设置类型的实例构造器。
        /// </summary>
        /// <param name="constructor"></param>
        public StructuralTypeConfiguration HasConstructor(IInstanceConstructor constructor)
        {
            Constructor = constructor;
            return this;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建实例构造器。
        /// </summary>
        /// <param name="construct">构造类型实例的委托。</param>
        public ParameterConfiguration<TStructural, TConfiguration> HasConstructor<T>(Func<T, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            Constructor = new DelegateConstructor<T, TStructural>(construct);
            //获取一个参数的构造函数
            var constructorInfo = typeof(TStructural).GetConstructor(new[] { typeof(T) });
            if (constructorInfo == null) throw new ArgumentException($"一个参数(类型为{typeof(T)})的构造函数不存在");
            //构造参数配置项
            var parameters = constructorInfo.GetParameters();
            var paraConfiguration =
                new ParameterConfiguration<TStructural, TConfiguration>(parameters, (TConfiguration)this);
            return paraConfiguration;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建新实例构造器
        /// </summary>
        /// <typeparam name="T">要创建的实例类型</typeparam>
        /// <param name="construct">构造类型实例的委托</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public StructuralTypeConfiguration HasNewInstanceConstructor<T>(Func<T, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            NewInstanceConstructor = new DelegateConstructor<T, TStructural>(construct);
            //获取一个参数的构造函数
            var constructorInfo = typeof(TStructural).GetConstructor(new[] { typeof(T) });
            if (constructorInfo == null) throw new ArgumentException($"一个参数(类型为{typeof(T)})的构造函数不存在");
            //设置新实例构造器的参数类型 不需要后续配置
            ((InstanceConstructor)NewInstanceConstructor).ParameterTypes = new List<Type> { typeof(T) };
            return this;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建实例构造器。
        /// </summary>
        /// <param name="construct">构造类型实例的委托。</param>
        public ParameterConfiguration<TStructural, TConfiguration> HasConstructor<T1, T2>(
            Func<T1, T2, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            Constructor = new DelegateConstructor<T1, T2, TStructural>(construct);
            //获取两个参数的构造函数
            var constructorInfo = typeof(TStructural).GetConstructor(new[] { typeof(T1), typeof(T2) });
            if (constructorInfo == null) throw new ArgumentException($"两个参数(类型为{typeof(T1)},{typeof(T2)})的构造函数不存在");
            //构造参数配置项
            var parameters = constructorInfo.GetParameters();
            var paraConfiguration =
                new ParameterConfiguration<TStructural, TConfiguration>(parameters, (TConfiguration)this);
            ((InstanceConstructor)NewInstanceConstructor).ParameterTypes = new List<Type> { typeof(T1) };
            return paraConfiguration;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建新实例构造器
        /// </summary>
        /// <typeparam name="T1">第一个参数</typeparam>
        /// <typeparam name="T2">第二个参数</typeparam>
        /// <param name="construct">委托</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public StructuralTypeConfiguration HasNewInstanceConstructor<T1, T2>(
            Func<T1, T2, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            NewInstanceConstructor = new DelegateConstructor<T1, T2, TStructural>(construct);
            //获取两个参数的构造函数
            var constructorInfo = typeof(TStructural).GetConstructor(new[] { typeof(T1), typeof(T2) });
            if (constructorInfo == null) throw new ArgumentException($"两个参数(类型为{typeof(T1)},{typeof(T2)})的构造函数不存在");
            //设置新实例构造器的参数类型 不需要后续配置
            ((InstanceConstructor)NewInstanceConstructor).ParameterTypes = new List<Type> { typeof(T1), typeof(T2) };
            return this;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建实例构造器。
        /// </summary>
        /// <param name="construct">构造类型实例的委托。</param>
        public ParameterConfiguration<TStructural, TConfiguration> HasConstructor<T1, T2, T3>(
            Func<T1, T2, T3, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            Constructor = new DelegateConstructor<T1, T2, T3, TStructural>(construct);
            //获取三个参数的构造函数
            var constructorInfo = typeof(TStructural).GetConstructor(new[] { typeof(T1), typeof(T2), typeof(T3) });
            if (constructorInfo == null)
                throw new ArgumentException($"三个参数(类型为{typeof(T1)},{typeof(T2)},{typeof(T3)})的构造函数不存在");
            //构造参数配置项
            var parameters = constructorInfo.GetParameters();
            var paraConfiguration =
                new ParameterConfiguration<TStructural, TConfiguration>(parameters, (TConfiguration)this);
            return paraConfiguration;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建新实例构造器
        /// </summary>
        /// <typeparam name="T1">第一个参数</typeparam>
        /// <typeparam name="T2">第二个参数</typeparam>
        /// <typeparam name="T3">第三个参数</typeparam>
        /// <param name="construct">委托</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public StructuralTypeConfiguration HasNewInstanceConstructor<T1, T2, T3>(
            Func<T1, T2, T3, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            NewInstanceConstructor = new DelegateConstructor<T1, T2, T3, TStructural>(construct);
            //获取三个参数的构造函数
            var constructorInfo = typeof(TStructural).GetConstructor(new[] { typeof(T1), typeof(T2), typeof(T3) });
            if (constructorInfo == null)
                throw new ArgumentException($"三个参数(类型为{typeof(T1)},{typeof(T2)},{typeof(T3)})的构造函数不存在");
            //设置新实例构造器的参数类型 不需要后续配置
            ((InstanceConstructor)NewInstanceConstructor).ParameterTypes =
                new List<Type> { typeof(T1), typeof(T2), typeof(T3) };
            return this;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建实例构造器。
        /// </summary>
        /// <param name="construct">构造类型实例的委托。</param>
        public ParameterConfiguration<TStructural, TConfiguration> HasConstructor<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            Constructor = new DelegateConstructor<T1, T2, T3, T4, TStructural>(construct);
            //获取四个参数的构造函数
            var constructorInfo =
                typeof(TStructural).GetConstructor(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
            if (constructorInfo == null)
                throw new ArgumentException($"四个参数(类型为{typeof(T1)},{typeof(T2)},{typeof(T3)},{typeof(T4)})的构造函数不存在");
            //构造参数配置项
            var parameters = constructorInfo.GetParameters();
            var paraConfiguration =
                new ParameterConfiguration<TStructural, TConfiguration>(parameters, (TConfiguration)this);
            return paraConfiguration;
        }

        /// <summary>
        ///     使用一个可以创建类型实例的委托为类型创建新实例构造器
        /// </summary>
        /// <typeparam name="T1">第一个参数</typeparam>
        /// <typeparam name="T2">第二个参数</typeparam>
        /// <typeparam name="T3">第三个参数</typeparam>
        /// <typeparam name="T4">第四个参数</typeparam>
        /// <param name="construct">构造委托</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public StructuralTypeConfiguration HasNewInstanceConstructor<T1, T2, T3, T4>(
            Func<T1, T2, T3, T4, TStructural> construct)
        {
            //使用委托创建一个委托构造器
            NewInstanceConstructor = new DelegateConstructor<T1, T2, T3, T4, TStructural>(construct);
            //获取四个参数的构造函数
            var constructorInfo =
                typeof(TStructural).GetConstructor(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
            if (constructorInfo == null)
                throw new ArgumentException($"四个参数(类型为{typeof(T1)},{typeof(T2)},{typeof(T3)},{typeof(T4)})的构造函数不存在");
            //设置新实例构造器的参数类型 不需要后续配置
            ((InstanceConstructor)NewInstanceConstructor).ParameterTypes = new List<Type>
                { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
            return this;
        }

        /// <summary>
        ///     使用一个构造函数为类型创建新实例构造器。
        /// </summary>
        /// <param name="constructorInfo">构造函数。</param>
        public ParameterConfiguration<TStructural, TConfiguration> HasConstructor(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
                throw new ArgumentException($"传入的{typeof(TStructural)}构造函数为空.");
            //反射构造器
            Constructor = new ReflectionConstructor(constructorInfo);
            var parameters = constructorInfo.GetParameters();
            //反射构造器的参数类型
            var paraConfiguration =
                new ParameterConfiguration<TStructural, TConfiguration>(parameters, (TConfiguration)this);
            return paraConfiguration;
        }

        /// <summary>
        ///     使用一个构造函数为类型创建实例构造器
        /// </summary>
        /// <param name="constructorInfo">构造函数</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public StructuralTypeConfiguration HasNewInstanceConstructor(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
                throw new ArgumentException($"传入的{typeof(TStructural)}构造函数为空.");
            //反射构造器
            NewInstanceConstructor = new ReflectionConstructor(constructorInfo);
            var parameters = constructorInfo.GetParameters();
            //反射构造器的参数类型
            ((InstanceConstructor)NewInstanceConstructor).ParameterTypes =
                parameters.Select(p => p.ParameterType).ToList();
            return this;
        }

        /// <summary>
        ///     设置此类型的具体类型判别规范和判别字段
        ///     用于判断此类型的要如何创建具体的类型
        /// </summary>
        /// <param name="concreteTypeDiscriminator">具体类型判别器</param>
        /// <param name="typeAttributeName">用于判断类型的字段名称</param>
        /// <returns></returns>
        public StructuralTypeConfiguration HasConcreteTypeDiscriminator(
            IConcreteTypeDiscriminator concreteTypeDiscriminator, string typeAttributeName)
        {
            _concreteTypeDiscriminator = concreteTypeDiscriminator;
            _typeAttributeName = typeAttributeName;
            return this;
        }

        /// <summary>
        ///     设置此类型的判别字段和判别字段的值
        ///     与实际类型一一对应即可 如果某个类型是抽象的 配置一个不会被使用的值即可
        /// </summary>
        /// <param name="typeName">用于判断类型的字段名称</param>
        /// <param name="value">对应的值</param>
        /// <returns></returns>
        public StructuralTypeConfiguration HasConcreteTypeSign(string typeName, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "不能设置空的类型判别字段.");
            var valueType = value.GetType();
            if (valueType != typeof(short) && valueType != typeof(int) && valueType != typeof(long) &&
                valueType != typeof(string))
                throw new ArgumentException("判别字段必须为string,short,int,long类型中的一种");
            //设置判断类型字段的名称和值
            _concreteTypeSign = new Tuple<string, object>(typeName, value);
            return this;
        }

        /// <summary>
        ///     通过反射从指定的CLR类型中收集元数据，生成元素配置项。
        /// </summary>
        internal override void ReflectionModeling(ITypeMemberAnalyzer analyticPipeline)
        {
            var hangUps = new List<PropertyInfo>();
            //遍历类型属性
            foreach (var properties in _clrType.GetProperties())
            {
                //继承的配置 不是当前类定义的 不处理
                if (properties.DeclaringType != _clrType && _derivingFrom != null) continue;
                //过滤属性挂起
                if (IgnoreList.Contains(properties.Name))
                {
                    hangUps.Add(properties);
                    continue;
                }

                Utils.GetIsMultipe(properties, out var type);

                //首先 查找已有配置
                ElementConfigurations.TryGetValue(properties.Name, out var typeElementConfiguration);
                var configurator = typeElementConfiguration as ITypeElementConfigurator;

                //使用管道判定结果为true 或者 被配置为了模型的一部分 或者是个元组
                if (AsElement(properties, analyticPipeline, out var name) ||
                    ModelBuilder.FindConfiguration(type) != null || Utils.IsTuple(type))
                {
                    if (configurator == null)
                        //没有配置 创建一个
                        configurator = CreateTypeElementConfigurator(properties, name);
                }
                //挂起
                else
                {
                    hangUps.Add(properties);
                    continue;
                }

                //使用管道处理当前的配置
                var pipeLine = analyticPipeline;
                while (pipeLine != null)
                {
                    if (configurator != null)
                        pipeLine.Configurate(properties, configurator);
                    pipeLine = pipeLine.Next;
                }
            }

            //处理挂起的
            foreach (var hangUp in hangUps)
            {
                var pipeLine = analyticPipeline;
                while (pipeLine != null)
                {
                    pipeLine.Configurate(hangUp, this);
                    pipeLine = pipeLine.Next;
                }
            }
        }

        /// <summary>
        ///     创建具体的配置
        /// </summary>
        /// <param name="properties">属性</param>
        /// <param name="name">名称</param>
        /// <returns></returns>
        private ITypeElementConfigurator CreateTypeElementConfigurator(PropertyInfo properties, string name)
        {
            //获取多重性
            Utils.GetIsMultipe(properties, out var type);

            //判断是否配置为复杂属性
            var isComlex = false;
            //查找是否有配置
            var complexConfig = ModelBuilder.FindConfiguration(type);
            if (complexConfig != null)
            {
                //构造一个复杂类型配置 用于比较
                var complexTypeConfig = typeof(ComplexTypeConfiguration<>).MakeGenericType(type);
                isComlex = complexConfig.GetType() == complexTypeConfig;
            }

            //基元类型或者结构 或者被配置为复杂类型 按照属性处理
            if (PrimitiveType.IsObasePrimitiveType(properties.PropertyType) ||
                !(type.IsClass || type.IsInterface) || isComlex)
            {
                //如果是可空类型
                type = Nullable.GetUnderlyingType(type) != null
                    ? type.GenericTypeArguments[0]
                    : type;

                //创建属性配置项
                return Attribute(name, type);
            }

            //不是简单属性 查找关联型或者实体型
            var structuralTypeConfiguration = ModelBuilder.FindConfiguration(type);
            //被配置过 或者 是个元组 元组要继续处理
            if (structuralTypeConfiguration != null || Utils.IsTuple(type))
            {
                //仅为关联型
                if (this is IAssociationTypeConfigurator && !(this is IEntityTypeConfigurator))
                    return CreateReferenceElement(properties);
                //仅为实体型
                if (this is IEntityTypeConfigurator && !(this is IAssociationTypeConfigurator))
                    return CreateReferenceElement(properties);
                //同时做关联型和实体型
                if (this is IEntityTypeConfigurator && this is IAssociationTypeConfigurator)
                    throw new ArgumentException("暂不支持一个类型同时为关联型和实体型");
            }

            return null;
        }


        /// <summary>
        ///     调用管道判断是否作为元素
        /// </summary>
        /// <returns></returns>
        private bool AsElement(MemberInfo memberInfo, ITypeMemberAnalyzer analyticPipeline, out string name)
        {
            var result = false;

            //后续管道的判定
            var pipeLine = analyticPipeline;
            while (pipeLine != null)
            {
                result |= pipeLine.AsElement(memberInfo, out name);
                pipeLine = pipeLine.Next;
            }

            //管道没有定名字 取默认值
            name = null;
            if (string.IsNullOrEmpty(name))
                name = memberInfo.Name;

            return result;
        }

        /// <summary>
        ///     添加类型扩展配置器 ，如果指定类型的配置器已存在，返回该配置器。
        /// </summary>
        /// <typeparam name="TExtensionConfiguration">类型扩展配置器</typeparam>
        /// <returns></returns>
        public TExtensionConfiguration HasExtension<TExtensionConfiguration>()
            where TExtensionConfiguration : TypeExtensionConfiguration, new()
        {
            //取得扩展配置器类型
            var extensionConfigurationType = typeof(TExtensionConfiguration);
            try
            {
                //调用基类配置方法
                return (TExtensionConfiguration)HasExtension(extensionConfigurationType);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展配置器失败,{extensionConfigurationType.Name}没有适合的无参构造函数",
                    nameof(extensionConfigurationType), e);
            }
        }

        /// <summary>
        ///     通知建模器，在反射建模时忽略lambda表达式指定的属性（Property），不为其生成类型元素。
        /// </summary>
        /// <param name="expression">指定要忽略的属性的Lambda表达式</param>
        public void Ignore<TResult>(Expression<Func<TStructural, TResult>> expression)
        {
            //分析属性名
            var member = (MemberExpression)expression.Body;
            //添加到过滤属性集合中
            IgnoreList.Add(member.Member.Name);
        }

        /// <summary>
        ///     指定当前类型的基类型。
        /// </summary>
        /// <param name="type">基类型。</param>
        public TConfiguration DeriveFrom(Type type)
        {
            if (!type.IsAssignableFrom(_clrType))
                throw new ArgumentException($"{type.FullName}不是{_clrType}的基类");
            //指定当前类型的基类型
            _derivingFrom = type;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     创建引用元素
        /// </summary>
        /// <returns></returns>
        protected abstract ITypeElementConfigurator CreateReferenceElement(PropertyInfo propInfo);
    }
}