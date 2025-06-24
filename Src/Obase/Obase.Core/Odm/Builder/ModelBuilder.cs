/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：建模器,提供配置对象数据模型的配置方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 12:04:20
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
    ///     建模器，用于存储模型元数据，并可以依据这些元数据生成对象数据模型。
    ///     对应于对象数据模型的实体型、复杂类型和关联型，建模器包含实体型配置项、复杂类型配置项和关联型配置项，各类型的元数据即存储于这三种类型配置项中。
    ///     各类型配置项又包含相应元素配置项：属性配置项、关联引用配置项、关联端配置项，分别存储属性、关联引用和关联端的元数据。
    ///     当Build方法被调用时，建模器根据这些配置项中的元数据信息构建对象数据模型。生成模型前，建模器还利用反映自动从CLR类型中收集元数据信息，因此大多数元数据不需
    ///     要手工配置。
    ///     此外，建模器会自动生成对象类型的派生代理类型，该派生类型实现IIntervene接口以允许第三方介入者介入对象行为。
    /// </summary>
    public class ModelBuilder
    {
        /// <summary>
        ///     隐式关联型配置器的建造器集合
        /// </summary>
        private readonly List<AssociationConfiguratorBuilder> _associationConfiguratorBuilders =
            new List<AssociationConfiguratorBuilder>();

        /// <summary>
        ///     补充配置管道建造器
        /// </summary>
        private readonly ComplementConfigurationPipelineBuilder _complementConfigurationPipelineBuilder;

        /// <summary>
        ///     当前建模器所属的上下文类型
        /// </summary>
        private readonly Type _contextType;

        /// <summary>
        ///     存储从程序集解析类型过程中应忽略的类型
        /// </summary>
        private readonly HashSet<Type> _ignoredTypes = new HashSet<Type>();

        /// <summary>
        ///     代理类型生成管道建造器
        /// </summary>
        private readonly ProxyTypeGenerationPipelineBuilder _proxyTypeGenerationPipelineBuilder;

        /// <summary>
        ///     类型解析管道构造器
        /// </summary>
        private readonly TypeAnalyticPipelineBuilder _typeAnalyticPipelineBuilder;

        /// <summary>
        ///     类型成员解析管道建造器
        /// </summary>
        private readonly TypeMemberAnalyticPipelineBuilder _typeMemberAnalyticPipelineBuilder;

        /// <summary>
        ///     补充配置器
        /// </summary>
        private IComplementConfigurator _complementConfigurator;

        /// <summary>
        ///     模型默认的存储标记。
        /// </summary>
        private StorageSymbol _defaultStorageSymbol = StorageSymbols.Current.Default;

        /// <summary>
        ///     指示是否进行完整性检查
        /// </summary>
        private bool _integrityCheck = true;

        /// <summary>
        ///     对象数据模型
        /// </summary>
        private ObjectDataModel _objectDataModel;

        /// <summary>
        ///     代理类型生成器
        /// </summary>
        private IProxyTypeGenerator _proxyTypeGenerator;

        /// <summary>
        ///     类型解析器
        /// </summary>
        private ITypeAnalyzer _typeAnalyzer;


        /// <summary>
        ///     值为StructuralTypeConfiguration{TStructural}
        /// </summary>
        private Dictionary<Type, StructuralTypeConfiguration> _typeConfigs;

        /// <summary>
        ///     类型元素生成器
        /// </summary>
        private ITypeMemberAnalyzer _typeMemberAnalyzer;

        /// <summary>
        ///     初始化ModelBuilder的新实例。
        ///     实施说明
        /// </summary>
        internal ModelBuilder(ObjectContext context)
        {
            //实例化代理类型生成管道建造器，并自动添加默认的生成器。
            _proxyTypeGenerationPipelineBuilder = new ProxyTypeGenerationPipelineBuilder();
            _proxyTypeGenerationPipelineBuilder.Use(p => new DefaultProxyTypeGenerator(p));
            //实例化类型成员解析管道建造器，并自动添加默认的解析器。
            _typeMemberAnalyticPipelineBuilder = new TypeMemberAnalyticPipelineBuilder();
            _typeMemberAnalyticPipelineBuilder.Use(p => new DefaultTypeMemberAnalyzer(this, p));
            //实例化类型解析管道建造器，并自动添加默认的解析器。
            _typeAnalyticPipelineBuilder = new TypeAnalyticPipelineBuilder();
            _typeAnalyticPipelineBuilder.Use(p => new DefaultTypeAnalyzer(_ignoredTypes, this, p));
            //实例化补充管道建造器，并自动添加默认的解析器。
            _complementConfigurationPipelineBuilder = new ComplementConfigurationPipelineBuilder();
            _complementConfigurationPipelineBuilder.Use(p => new DefaultComplementConfigurator(p));
            //保存当前上下文类型
            _contextType = context.GetType();
        }

        /// <summary>
        ///     类型配置项字典
        /// </summary>
        private Dictionary<Type, StructuralTypeConfiguration> TypeConfigs =>
            _typeConfigs ?? (_typeConfigs = new Dictionary<Type, StructuralTypeConfiguration>());

        /// <summary>
        ///     当前建模器所属的上下文类型
        /// </summary>
        public Type ContextType => _contextType;

        /// <summary>
        ///     启动一个实体型配置项，如果要启动的实体型配置项未创建则新建一个。
        ///     类型参数TEntity指定该实体型对应的CLR类型（即对象系统中的类型），在建模器中它是配置项的键。
        /// </summary>
        public EntityTypeConfiguration<TEntity> Entity<TEntity>() where TEntity : class
        {
            //未配置
            if (!TypeConfigs.ContainsKey(typeof(TEntity)))
                TypeConfigs[typeof(TEntity)] = new EntityTypeConfiguration<TEntity>(this);
            //如果已配置的不是实体 就新建一个
            if (!(TypeConfigs[typeof(TEntity)] is EntityTypeConfiguration<TEntity> result))
            {
                result = new EntityTypeConfiguration<TEntity>(this);
                TypeConfigs[typeof(TEntity)] = result;
            }

            return result;
        }

        /// <summary>
        ///     启动一个复杂类型配置项，如果要启动的复杂类型配置项未创建则新建一个。
        ///     类型参数TComplex指定该复杂类型对应的CLR类型（即对象系统中的类型），在建模器中它是配置项的键。
        /// </summary>
        public ComplexTypeConfiguration<TComplex> Complex<TComplex>()
        {
            //未配置
            if (!TypeConfigs.ContainsKey(typeof(TComplex)))
                TypeConfigs[typeof(TComplex)] = new ComplexTypeConfiguration<TComplex>(this);

            //如果已配置的不是复杂类型 就新建一个
            if (!(TypeConfigs[typeof(TComplex)] is ComplexTypeConfiguration<TComplex> result))
            {
                result = new ComplexTypeConfiguration<TComplex>(this);
                TypeConfigs[typeof(TComplex)] = result;
            }

            return result;
        }

        /// <summary>
        ///     启动一个关联型配置项，如果要启动的关联型配置项未创建则新建一个。
        ///     类型参数TAssociation指定该实体型对应的CLR类型（即对象系统中的类型），在建模器中它是配置项的键。
        ///     使用此方法启动的关联型配置项将来生成的关联型为显式关联。
        /// </summary>
        public AssociationTypeConfiguration<TAssociation> Association<TAssociation>() where TAssociation : class
        {
            //未配置
            if (!TypeConfigs.ContainsKey(typeof(TAssociation)))
                TypeConfigs[typeof(TAssociation)] = new AssociationTypeConfiguration<TAssociation>(this);

            //如果已配置的不是关联型 就新建一个
            if (!(TypeConfigs[typeof(TAssociation)] is AssociationTypeConfiguration<TAssociation> result))
            {
                result = new AssociationTypeConfiguration<TAssociation>(this);
                TypeConfigs[typeof(TAssociation)] = result;
            }

            return result;
        }

        /// <summary>
        ///     启动一个隐式关联型配置器的建造器
        ///     注意:每次调用此方法都会返回一个新的建造器
        /// </summary>
        /// <returns>隐式关联型配置器的建造器</returns>
        public AssociationConfiguratorBuilder Association()
        {
            //每次调用都会新建一个建造器
            var builder = new AssociationConfiguratorBuilder(this);
            _associationConfiguratorBuilders.Add(builder);
            return builder;
        }

        /// <summary>
        ///     生成对象数据模型。
        ///     此方法仅会被调用一次
        ///     首先调用所有隐式关联配置器建造器的Build方法，建造隐式关联配置器。
        ///     第一步，遍历类型配置项构建类型实例（实体型、关联型、复杂类型）放入模型，（这个过程中会自动生成代理类型）；
        ///     第二步，再次遍历类型配置项，通过反射从CLR类型收集元素元数据，然后遍历元素配置项，构建元素实例。
        /// </summary>
        /// <param name="executor">模型结构映射执行器</param>
        public ObjectDataModel Build(IStructMappingExecutor executor = null)
        {
            if (_objectDataModel == null)
            {
                _objectDataModel = new ObjectDataModel();

                //忽略被忽略的类
                foreach (var ignored in _ignoredTypes) TypeConfigs.Remove(ignored);

                //生成管道
                GeneratePipeline();

                //遍历配置项 创建隐式关联配置器
                foreach (var item in TypeConfigs) item.Value.CreateImplicitAssociationConfiguration();

                //生成隐式关联
                foreach (var builder in _associationConfiguratorBuilders)
                {
                    var structuralTypeConfiguration = builder.Build();
                    //加入配置
                    if (structuralTypeConfiguration != null)
                        AddConfiguration(structuralTypeConfiguration);
                    //设置关联型
                    foreach (var endConfiguration in builder.EndConfigurations)
                        endConfiguration.SetAssociationType(structuralTypeConfiguration);
                }

                //排序 将实体型放置于关联型之前
                var typeConifgs = TypeConfigs
                    .OrderBy(p => p.Value,
                        Comparer<StructuralTypeConfiguration>.Create(CreateStructuralTypeComparison))
                    .ToDictionary(p => p.Key, p => p.Value);

                //遍历配置项 创建结构化类型配置
                foreach (var item in typeConifgs)
                {
                    //处理类型解析管道
                    item.Value.ReflectionModeling(_typeAnalyzer);
                    //创建模型
                    var structuralType = item.Value.Create(_objectDataModel);
                    //添加到对象数据模型
                    _objectDataModel.AddType(structuralType);
                }

                //遍历配置项 创建类型元素配置
                foreach (var item in typeConifgs)
                {
                    //反射生成元素配置项
                    item.Value.ReflectionModeling(_typeMemberAnalyzer);
                    //创建类型元素
                    item.Value.CreateElements(_objectDataModel);
                    //配置元素
                    item.Value.Configurate(_objectDataModel);
                }

                var deriving = new Dictionary<ObjectType, StructuralTypeConfiguration>();
                //处理继承关系
                foreach (var item in typeConifgs)
                {
                    //取出对象类型模型
                    var objectType = _objectDataModel.GetObjectType(item.Key);

                    //检查一下是否配置了继承 如果配置了 把基类存下来
                    if (objectType != null && objectType.DerivingFrom != null)
                    {
                        var derivingFrom = (ObjectType)objectType.DerivingFrom;
                        if (!deriving.ContainsKey(derivingFrom))
                            deriving.Add(derivingFrom, typeConifgs[derivingFrom.ClrType]);
                        if (objectType.ConcreteTypeSign != null && objectType.ConcreteTypeSign.Item1 !=
                            typeConifgs[derivingFrom.ClrType].TypeAttributeName)
                            throw new ArgumentException($"{objectType.Name}与基类类型{derivingFrom.ClrType}的判别字段名称不符.");
                    }
                }

                //遍历配置项 处理代理类型
                foreach (var item in typeConifgs)
                {
                    //取出对象类型模型
                    var objectType = _objectDataModel.GetObjectType(item.Key);
                    //如果此配置项为IObjectTypeConfigurator
                    if (item.Value is IObjectTypeConfigurator objectTypeConfigurator)
                        //是否配置触发器
                        if (ShouldCreateProxyType(_proxyTypeGenerator, objectType, objectTypeConfigurator))
                        {
                            //生成代理类对象
                            var proxyType = item.Value.CreateProxyType(_proxyTypeGenerator);
                            var ctorObj = CreateConstructor(proxyType, objectType.Constructor);
                            //反持久化对象构造器
                            objectType.Constructor = ctorObj;
                            //新对象构造器
                            if (objectType.NewInstanceConstructor != null)
                            {
                                //设置要构造的对象类型
                                objectType.NewInstanceConstructor.InstanceType = objectType;
                                //新对象构造器
                                var newCtorObj =
                                    CreateNewInstanceConstructor(proxyType, objectType.NewInstanceConstructor);
                                //对象构造器
                                objectType.NewInstanceConstructor = newCtorObj;
                            }

                            //生成代理类对象
                            objectType.ProxyType = proxyType;
                        }

                    //添加到对象数据模型
                    if (objectType != null)
                    {
                        _objectDataModel.AddType(objectType);
                        //检查一下构造器
                        if (objectType.Constructor.InstanceType == null)
                            objectType.Constructor.InstanceType = objectType;
                        //处理一下外键保证机制
                        if (item.Value.Adder != null)
                            item.Value.Adder.DefineValueGetterAndSetter();
                        else
                            CheckForeignKeyGuarantee(objectType);
                    }
                }

                //处理具体类型判别器
                foreach (var objectType in deriving)
                    objectType.Key.SetConcreteTypeDiscriminator(objectType.Value.ConcreteTypeDiscriminator,
                        objectType.Value.TypeAttributeName);

                //补充操作
                foreach (var item in typeConifgs)
                    //执行补充操作
                    item.Value.ConfigurateComplement(_complementConfigurator);

                //完整性检查
                if (_integrityCheck)
                    foreach (var structuralType in _objectDataModel.Types)
                        structuralType.IntegrityCheck();
            }

            var hasFlag = false;
            //如果有任意类型未配置存储标记
            foreach (var structuralType in _objectDataModel.Types.Where(p =>
                         p.GetExtension(typeof(HeterogStorageExtension)) == null))
            {
                hasFlag = true;
                structuralType.AddExtension<HeterogStorageExtension>().StorageSymbol = _defaultStorageSymbol;
            }

            //最后没有取得标记 使用默认的标记
            if (hasFlag)
                _objectDataModel.StorageSymbol = _defaultStorageSymbol;

            //如果有模型结构映射执行器 执行映射
            if (executor != null)
                executor.Execute(_objectDataModel);

            return _objectDataModel;
        }

        /// <summary>
        ///     创建结构化类型时的比较委托
        /// </summary>
        /// <param name="x">第一个结构化类型</param>
        /// <param name="y">第二个结构化类型</param>
        /// <returns></returns>
        private int CreateStructuralTypeComparison(StructuralTypeConfiguration x, StructuralTypeConfiguration y)
        {
            //根据具体的类型进行比较
            var xCode = GetCreateStructuralTypeSort(x);
            var yCode = GetCreateStructuralTypeSort(y);

            return xCode - yCode;
        }

        /// <summary>
        ///     创建结构化类型时的具体的比较方法
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private int GetCreateStructuralTypeSort(StructuralTypeConfiguration configuration)
        {
            //复杂类型 返回0 实体型 返回继承链的Index 关联型返回50
            //可以处理最多50层继承
            switch (configuration)
            {
                case IEntityTypeConfigurator _:
                {
                    //获取配置的继承链
                    var chain = Utils.GetDerivingConfigChain(configuration, this);
                    //有继承的排在没有继承的后面
                    return chain.IndexOf(configuration) + 1;
                }
                case IAssociationTypeConfigurator _:
                    return 50;
                default:
                    return 0;
            }
        }

        /// <summary>
        ///     生成管道
        /// </summary>
        private void GeneratePipeline()
        {
            //此三项有默认值 直接Build
            _proxyTypeGenerator = _proxyTypeGenerationPipelineBuilder.Build();
            _typeMemberAnalyzer = _typeMemberAnalyticPipelineBuilder.Build();
            _typeAnalyzer = _typeAnalyticPipelineBuilder.Build();
            _complementConfigurator = _complementConfigurationPipelineBuilder.Build();
            //检查官方管道个数
            var checkDic = new Dictionary<string, int>
            {
                { "Obase.Odm.Annotation.AnnotatedMemberAnalyzer", 0 },
                { "Obase.Odm.Annotation.AnnotatedTypeAnalyzer", 0 },
                { "Obase.LogicDeletion.ComplementConfigurator", 0 },
                { "Obase.LogicDeletion.TypeAnalyzer", 0 },
                { "Obase.LogicDeletion.ProxyTypeGenerator", 0 },
                { "Obase.MultiTenant.ComplementConfigurator", 0 },
                { "Obase.MultiTenant.TypeAnalyzer", 0 },
                { "Obase.MultiTenant.ProxyTypeGenerator", 0 }
            };
            //处理每个管道中的每个配置器
            //将每个配置器的类型完全名称存入字典中
            var currentProxy = _proxyTypeGenerator;
            while (currentProxy != null)
            {
                var fullName = currentProxy.GetType().FullName;
                if (fullName != null && checkDic.ContainsKey(fullName)) checkDic[fullName]++;
                currentProxy = currentProxy.Next;
            }

            var currentTypeMember = _typeMemberAnalyzer;
            while (currentTypeMember != null)
            {
                var fullName = currentTypeMember.GetType().FullName;
                if (fullName != null && checkDic.ContainsKey(fullName)) checkDic[fullName]++;
                currentTypeMember = currentTypeMember.Next;
            }

            var currentType = _typeAnalyzer;
            while (currentType != null)
            {
                var fullName = currentType.GetType().FullName;
                if (fullName != null && checkDic.ContainsKey(fullName)) checkDic[fullName]++;
                currentType = currentType.Next;
            }

            var currentComplement = _complementConfigurator;
            while (currentComplement != null)
            {
                var fullName = currentComplement.GetType().FullName;
                if (fullName != null && checkDic.ContainsKey(fullName)) checkDic[fullName]++;
                currentComplement = currentComplement.Next;
            }

            //重复注册则抛出异常
            foreach (var pair in checkDic.Where(pair => pair.Value > 1))
                throw new ArgumentException($"不能在管道中多次注册{pair.Key}");
        }

        /// <summary>
        ///     创建代理类型的构造器
        /// </summary>
        /// <param name="proxyType">代理类型</param>
        /// <param name="constructor">原构造器</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IInstanceConstructor CreateConstructor(Type proxyType,
            IInstanceConstructor constructor)
        {
            //用原有的类型参数查找构造信息
            var paraObjs = constructor.Parameters;
            var ctorInfo = paraObjs == null
                ? proxyType.GetConstructor(Type.EmptyTypes)
                : proxyType.GetConstructor(paraObjs.Select(p => p.GetType()).ToArray());
            //构造函数参数表达式
            var paraExps = paraObjs
                ?.Select(paraObj => Expression.Parameter(paraObj.GetType(), paraObj.Name))
                .ToArray();

            //找不到 构造器可能是自定义的 此时继续使用其原有的构造器
            if (ctorInfo == null) return constructor;

            //找到了 构造新的代理类型的构造器
            var ctorObj = new ReflectionConstructor(ctorInfo);
            //设置参数
            if (paraExps != null)
                foreach (var p in paraObjs)
                    ctorObj.SetParameter(p.Name, p.ElementName, p.ValueConverter, p.Expression);

            return ctorObj;
        }

        /// <summary>
        ///     创建新对象构造器
        /// </summary>
        /// <param name="proxyType"></param>
        /// <param name="constructor"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IInstanceConstructor CreateNewInstanceConstructor(Type proxyType, IInstanceConstructor constructor)
        {
            //参数
            var paraObjs = ((InstanceConstructor)constructor).ParameterTypes.ToArray();
            //构造信息
            var ctorInfo = paraObjs.Length == 0
                ? proxyType.GetConstructor(Type.EmptyTypes)
                : proxyType.GetConstructor(paraObjs.Select(p => p).ToArray());

            if (ctorInfo == null)
                throw new InvalidOperationException($"建模错误:{constructor.InstanceType.FullName}没有配置正确的新对象构造函数");
            //构造器
            var ctorObj = new ReflectionConstructor(ctorInfo);

            return ctorObj;
        }

        /// <summary>
        ///     调用管道判断是否需要创建代理对象
        /// </summary>
        /// <param name="generator">代理创建器</param>
        /// <param name="objType">当前的对象类型</param>
        /// <param name="configurator">对象类型配置器</param>
        /// <returns></returns>
        private bool ShouldCreateProxyType(IProxyTypeGenerator generator, ObjectType objType,
            IObjectTypeConfigurator configurator)
        {
            var should = false;

            //后续管道的判定
            var pipeLine = generator;
            while (pipeLine != null)
            {
                should |= pipeLine.Should(objType, configurator);
                pipeLine = pipeLine.Next;
            }

            return should;
        }

        /// <summary>
        ///     检查外键保证机制是否正确执行
        ///     如果因为之前注册过此代理类型 此时进行增补
        /// </summary>
        /// <param name="objType"></param>
        private void CheckForeignKeyGuarantee(ObjectType objType)
        {
            //获取定义的外键
            var attrs = Utils.GetDefinedForeignAttributes(objType, null, out _);

            if (attrs.Count > 0)
                foreach (var attribute in attrs)
                {
                    var field = objType.RebuildingType.GetField($"{attribute.Name}");
                    if (field != null)
                    {
                        //构造FieldValueGetter
                        var valueGetter = new FieldValueGetter(field);
                        attribute.ValueGetter = valueGetter;
                        //构造FieldValueSetter
                        var setter = ValueSetter.Create(field);
                        attribute.ValueSetter = setter;

                        objType.AddAttribute(attribute);
                    }
                }
        }

        /// <summary>
        ///     从模型查找类型配置项。如果未找到返回null。
        /// </summary>
        /// <param name="type">目标类型的对象系统类型</param>
        public StructuralTypeConfiguration FindConfiguration(Type type)
        {
            return TypeConfigs.ContainsKey(type) ? TypeConfigs[type] : null;
        }

        /// <summary>
        ///     从模型查找隐式关联配置项建造器
        ///     如果未找到返回null
        /// </summary>
        /// <param name="endsTag">端标签</param>
        /// <returns></returns>
        public AssociationConfiguratorBuilder FindImplicitAssociationConfigurationBuilder(string endsTag)
        {
            return _associationConfiguratorBuilders.FirstOrDefault(p =>
                p.GenerateEndsTag() == endsTag);
        }

        /// <summary>
        ///     设置模型是否进行完整性检查
        /// </summary>
        /// <param name="integrityCheck"></param>
        public void HasIntegrityCheck(bool integrityCheck)
        {
            _integrityCheck = integrityCheck;
        }

        /// <summary>
        ///     为模型设置默认的存储标记。
        /// </summary>
        public ModelBuilder HasDefaultStorageSymbol(StorageSymbol defaultStorageSymbol)
        {
            _defaultStorageSymbol = defaultStorageSymbol;
            return this;
        }

        /// <summary>
        ///     启动一个关联型配置项，如果要启动的关联型配置项未创建则新建一个。
        ///     使用此方法启动的关联型配置项将来生成的关联型为显式关联。
        /// </summary>
        /// <param name="assoType">要配置的关联的类型。</param>
        public IAssociationTypeConfigurator Association(Type assoType)
        {
            if (!TypeConfigs.ContainsKey(assoType))
            {
                var associationType = typeof(AssociationTypeConfiguration<>).MakeGenericType(assoType);
                TypeConfigs[assoType] = (StructuralTypeConfiguration)Activator.CreateInstance(associationType, this);
            }

            return TypeConfigs[assoType] as IAssociationTypeConfigurator;
        }

        /// <summary>
        ///     启动一个复杂类型配置项，如果要启动的复杂类型配置项未创建则新建一个。
        /// </summary>
        /// <param name="complexType">要配置的复杂类型。</param>
        public IStructuralTypeConfigurator Complex(Type complexType)
        {
            if (!TypeConfigs.ContainsKey(complexType))
            {
                var tcomplexType = typeof(ComplexTypeConfiguration<>).MakeGenericType(complexType);
                TypeConfigs[complexType] = (StructuralTypeConfiguration)Activator.CreateInstance(tcomplexType, this);
            }

            return TypeConfigs[complexType] as IStructuralTypeConfigurator;
        }


        /// <summary>
        ///     启动一个实体型配置项，如果要启动的实体型配置项未创建则新建一个。
        /// </summary>
        /// <param name="entityType">要配置的实体类型。</param>
        public IEntityTypeConfigurator Entity(Type entityType)
        {
            if (!TypeConfigs.ContainsKey(entityType))
            {
                var tentityType = typeof(EntityTypeConfiguration<>).MakeGenericType(entityType);
                TypeConfigs[entityType] = (StructuralTypeConfiguration)Activator.CreateInstance(tentityType, this);
            }

            return TypeConfigs[entityType] as IEntityTypeConfigurator;
        }

        /// <summary>
        ///     从指定的类型集合中提取类型并注册到模型
        /// </summary>
        /// <param name="types">类型集合</param>
        public void RegisterType(params Type[] types)
        {
            var analyzer = new DefaultAssemblyAnalyzer(_ignoredTypes);
            analyzer.Analyze(types, this);
        }

        /// <summary>
        ///     从指定的类型集合中提取类型并注册到模型
        /// </summary>
        /// <param name="analyzer">程序集解析器</param>
        /// <param name="types">类型集合</param>
        public void RegisterType(IAssemblyAnalyzer analyzer, params Type[] types)
        {
            analyzer.Analyze(types, this);
        }

        /// <summary>
        ///     从指定的程序集中提取类型并注册到模型。
        /// </summary>
        /// <param name="assembly">类型所在的程序集。</param>
        /// <param name="analyzer">程序集解析器，负责从程序集中发现类型。</param>
        public void RegisterType(Assembly assembly, IAssemblyAnalyzer analyzer)
        {
            analyzer.Analyze(assembly, this);
        }

        /// <summary>
        ///     从指定的程序集中提取类型并注册到模型。
        /// </summary>
        /// <param name="assemblyString">表示程序集名称的字符串，支持长格式或短格式。</param>
        /// <param name="analyzer">程序集解析器，负责从程序集中发现类型。</param>
        public void RegisterType(string assemblyString, IAssemblyAnalyzer analyzer)
        {
            var assembly = Assembly.LoadFrom(assemblyString);
            RegisterType(assembly, analyzer);
        }

        /// <summary>
        ///     从指定的程序集中，按照推断约定提取类型并注册到模型。
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterType(Assembly assembly)
        {
            var analyzer = new DefaultAssemblyAnalyzer(_ignoredTypes);
            analyzer.Analyze(assembly, this);
        }

        /// <summary>
        ///     指定的程序集字符串加载程序集，按照推断约定提取类型并注册到模型。
        /// </summary>
        /// <param name="assemblyString"></param>
        public void RegisterType(string assemblyString)
        {
            var assembly = Assembly.LoadFrom(assemblyString);
            RegisterType(assembly);
        }

        /// <summary>
        ///     向类型成员解析管道注册中间件，该管道用于在反射建模过程中解析类型成员。
        /// </summary>
        /// <returns>
        ///     类型成员解析管道建造器。
        ///     实施说明
        ///     首次调用时实例化管道建造器，并自动添加默认的解析器。
        /// </returns>
        /// <param name="middlewareDelegate">
        ///     中间件委托，代表创建管道中间件（即解析器）的方法，该方法的参数用于指定管道中的下一个解析器，返回值为生成的中
        ///     间件。
        /// </param>
        public TypeMemberAnalyticPipelineBuilder Use(Func<ITypeMemberAnalyzer, ITypeMemberAnalyzer> middlewareDelegate)
        {
            return _typeMemberAnalyticPipelineBuilder.Use(middlewareDelegate);
        }

        /// <summary>
        ///     向代理类型生成管道注册中间件，该管道用于为模型中注册的类型生成代理类。
        /// </summary>
        /// <returns>
        ///     代理类型生成管道建造器。
        ///     实施说明
        ///     首次调用时实例化管道建造器，并自动添加默认的解析器。
        /// </returns>
        /// <param
        ///     name="middlewareDelegate">
        ///     中间件委托，代表创建管道中间件（即生成器）的方法，该方法的参数用于指定管道中的下一个生成器，返回值为生成的中
        ///     间件。
        /// </param>
        public ProxyTypeGenerationPipelineBuilder Use(Func<IProxyTypeGenerator, IProxyTypeGenerator> middlewareDelegate)
        {
            return _proxyTypeGenerationPipelineBuilder.Use(middlewareDelegate);
        }

        /// <summary>
        ///     向类型解析管道注册中间件，该管道用于在反射建模过程中解析类型。
        /// </summary>
        /// <param name="middlewareDelegate">中间件委托，代表创建管道中间件（即生成器）的方法，该方法的参数用于指定管道中的下一个生成器，返回值为生成的中间件。</param>
        /// <returns>类型解析管道建造器。</returns>
        public TypeAnalyticPipelineBuilder Use(Func<ITypeAnalyzer, ITypeAnalyzer> middlewareDelegate)
        {
            return _typeAnalyticPipelineBuilder.Use(middlewareDelegate);
        }

        /// <summary>
        ///     向补充配置管道注册中间件，该管道用于在生成模型过程中执行补充配置。
        /// </summary>
        /// <param name="middlewareDelegate">中间件委托，代表创建管道中间件（即生成器）的方法，该方法的参数用于指定管道中的下一个生成器，返回值为生成的中间件。</param>
        /// <returns>补充配置管道建造器。</returns>
        public ComplementConfigurationPipelineBuilder Use(
            Func<IComplementConfigurator, IComplementConfigurator> middlewareDelegate)
        {
            return _complementConfigurationPipelineBuilder.Use(middlewareDelegate);
        }

        /// <summary>
        ///     向建模器添加类型配置项
        /// </summary>
        /// <param name="configuration">类型配置项</param>
        public void AddConfiguration(StructuralTypeConfiguration configuration)
        {
            TypeConfigs[configuration.ClrType] = configuration;
        }

        /// <summary>
        ///     指定从程序集解析类型过程中应忽略的类型；如果类型已创建配置器，应删除该配置器。
        /// </summary>
        /// <typeparam name="T">要忽略的类型</typeparam>
        /// <returns></returns>
        public ModelBuilder Ignore<T>()
        {
            //加入忽略
            _ignoredTypes.Add(typeof(T));
            //删除已有
            TypeConfigs.Remove(typeof(T));

            return this;
        }
    }
}