/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：实体型配置项.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:17:46
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Common;
using Obase.Core.Odm.Builder.ImplicitAssociationConfigor;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     实体型配置项。
    /// </summary>
    public class EntityTypeConfiguration<TEntity> : ObjectTypeConfiguration<TEntity, EntityTypeConfiguration<TEntity>>,
        IEntityTypeConfigurator
        where TEntity : class
    {
        /// <summary>
        ///     显式关联关联引用访问器
        /// </summary>
        private Dictionary<Type, PropertyInfo> _explicitAssoRefProperties;

        /// <summary>
        ///     隐式关联关联引用访问器
        /// </summary>
        private Dictionary<string, PropertyInfo> _implicitAssoRefProperties;

        /// <summary>
        ///     标识属性组（一般表示为主键）
        /// </summary>
        private List<string> _keyAttributes;

        /// <summary>
        ///     自增是否被指定
        /// </summary>
        private bool _keyIncreaseHasSet;

        /// <summary>
        ///     标识属性是否自增
        /// </summary>
        private bool _keyIsSelfIncreased;

        /// <summary>
        ///     所有配置类型元素
        /// </summary>
        private Dictionary<string, TypeElementConfiguration> _typeElementConfigurations;

        /// <summary>
        ///     创建一个实体型配置
        /// </summary>
        /// <param name="modelBuilder">建模器</param>
        public EntityTypeConfiguration(ModelBuilder modelBuilder)
            : base(modelBuilder)
        {
        }

        /// <summary>
        ///     标识属性集合
        /// </summary>
        protected internal override List<string> KeyAttributes => _keyAttributes;

        /// <summary>
        ///     标识属性是否自增长
        /// </summary>
        internal bool KeyIsSelfIncreased => _keyIsSelfIncreased;

        /// <summary>
        ///     获取所有的元素配置项，包括属性配置项、关联引用配置项、关联端配置项。
        /// </summary>
        public override Dictionary<string, TypeElementConfiguration> ElementConfigurations
        {
            get => _typeElementConfigurations ??
                   (_typeElementConfigurations = new Dictionary<string, TypeElementConfiguration>());
            set => _typeElementConfigurations = value;
        }

        /// <summary>
        ///     根据名称获取元素配置器。
        /// </summary>
        /// <param name="name">元素名称。</param>
        public override ITypeElementConfigurator GetElement(string name)
        {
            return _typeElementConfigurations[name] as ITypeElementConfigurator;
        }

        /// <summary>
        ///     设置标识属性。
        ///     注：每调用一次本方法，追加一个标识属性。
        /// </summary>
        /// <param name="attrName">属性名称</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IEntityTypeConfigurator.HasKeyAttribute(string attrName, bool overrided)
        {
            //每调用一次本方法，如果override为false，追加一个标识属性，如果为true清空之前的所有设置。
            if (!overrided)
            {
                //如果是非覆盖 只有在没有设置过标识属性时才生效
                if (_keyAttributes == null || _keyAttributes.Count == 0)
                    HasKeyAttribute(attrName);
            }
            else
            {
                _keyAttributes?.Clear();
                HasKeyAttribute(attrName);
            }
        }

        /// <summary>
        ///     设置一个值，该值指示标识属性是否为自增。
        /// </summary>
        /// <param name="keyIsSelfIncreased">是否自增</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IEntityTypeConfigurator.HasKeyIsSelfIncreased(bool keyIsSelfIncreased, bool overrided)
        {
            //覆盖的 直接设置
            if (overrided)
            {
                HasKeyIsSelfIncreased(keyIsSelfIncreased);
            }
            else
            {
                //否则 有值才设置
                if (!_keyIsSelfIncreased) HasKeyIsSelfIncreased(keyIsSelfIncreased);
            }
        }

        /// <summary>
        ///     获取标识属性集合
        /// </summary>
        /// <returns></returns>
        public string[] GetKeyAttributesFiled()
        {
            if (_keyAttributes == null)
                _keyAttributes = new List<string>();

            var result = new List<string>();

            foreach (var keyAttribute in _keyAttributes)
                if (ElementConfigurations.TryGetValue(keyAttribute, out var configuration))
                    if (configuration is AttributeConfiguration<TEntity> attrConfig)
                        result.Add(attrConfig.TargetField);

            return result.ToArray();
        }


        /// <summary>
        ///     添加关联引用配置项。
        /// </summary>
        /// <param name="associationReference">关联引用</param>
        internal void AddAssociationReference(
            AssociationReferenceConfiguration<TEntity> associationReference)
        {
            ElementConfigurations[associationReference.Name] = associationReference;
        }

        /// <summary>
        ///     查找一个关联引用访问器，该关联引用基于指定显式关联定义。
        /// </summary>
        /// <param name="assoType">关联类型</param>
        /// <returns></returns>
        internal PropertyInfo FindProperty(Type assoType)
        {
            //定义一个字典（_explicitAssoRefProperties）用于寄存当前类型上定义的所有显式关联引用，其键为关联类型。
            //首次调用本方法时生成上述字典和_implicitAssoRefProperties字典(参见本方法另一重载)，后续调用时应避免重复生成。
            if (_explicitAssoRefProperties == null)
                _explicitAssoRefProperties = new Dictionary<Type, PropertyInfo>();

            if (_explicitAssoRefProperties.TryGetValue(assoType, out var property))
                return property;
            return null;
        }

        /// <summary>
        ///     查找一个关联引用访问器，该关联引用基于指定关联端标签代表的隐式关联定义，如果指定的关联端标签代表的关联不只一个，返回符合此标签的第一个关联引用。
        /// </summary>
        /// <param name="endsTag">关联端标签。</param>
        /// <returns></returns>
        internal PropertyInfo FindProperty(string endsTag)
        {
            //定义一个字典（_implicitAssoRefProperties）用于寄存当前类型上定义的所有隐式关联引用，其键为关联端标签。
            //首次调用本方法时生成上述字典和_explicitAssoRefProperties字典(参见本方法另一重载)，后续调用时应避免重复生成。
            if (_implicitAssoRefProperties == null)
                _implicitAssoRefProperties = new Dictionary<string, PropertyInfo>();
            if (_implicitAssoRefProperties.TryGetValue(endsTag, out var property))
                return property;
            return null;
        }

        /// <summary>
        ///     设置标识属性。
        ///     注：每调用一次本方法，追加一个标识属性。
        /// </summary>
        /// <param name="attrName">属性名称</param>
        public EntityTypeConfiguration<TEntity> HasKeyAttribute(string attrName)
        {
            if (_clrType.GetProperty(attrName) == null)
                throw new ArgumentException($"{_clrType.Name}内找不到属性{attrName},无法配置标识属性.", nameof(attrName));
            if (_keyAttributes == null)
                _keyAttributes = new List<string>();
            //没有配置过的才添加
            if (!_keyAttributes.Contains(attrName)) _keyAttributes.Add(attrName);
            return this;
        }

        /// <summary>
        ///     根据Lamda表达式包含的信息设置标识属性。
        ///     注：每调用一次本方法，追加一个标识属性。
        /// </summary>
        /// <param name="expression">一个Lamda表达式，用于指定一个属性</param>
        public EntityTypeConfiguration<TEntity> HasKeyAttribute<TAttribute>(
            Expression<Func<TEntity, TAttribute>> expression)
        {
            if (expression.Body is MemberExpression member) return HasKeyAttribute(member.Member.Name);

            throw new ArgumentException("不能使用非属性访问表达式配置标识属性");
        }

        /// <summary>
        ///     设置一个值，该值指示标识属性是否为自增。
        /// </summary>
        /// <param name="keyIsSelfIncreased">主键是否自增</param>
        public EntityTypeConfiguration<TEntity> HasKeyIsSelfIncreased(bool keyIsSelfIncreased)
        {
            _keyIsSelfIncreased = keyIsSelfIncreased;
            _keyIncreaseHasSet = true;
            return this;
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型。
        /// </summary>
        protected override StructuralType CreateReally(ObjectDataModel buidingModel)
        {
            //根据配置项数据创建模型对象并设值
            EntityType entityType;
            //处理基类
            if (_derivingFrom != null)
            {
                var derivingFrom = buidingModel.GetStructuralType(_derivingFrom);
                if (derivingFrom == null)
                    throw new ArgumentException($"无法找到{_clrType.FullName}所声明的基类{_derivingFrom.FullName},需要先注册基类.");
                entityType = new EntityType(_clrType, derivingFrom);
            }
            else
            {
                entityType = new EntityType(_clrType);
            }

            //检查构造器
            if (Constructor == null)
                throw new ArgumentException($"无法获取{_clrType.FullName}的public或protect internal且无参的构造函数,请为其配置构造函数.");
            //新实例构造器 和 构造器
            entityType.NewInstanceConstructor = NewInstanceConstructor;
            entityType.Constructor = Constructor;

            //如果只有一个标识属性 并且此属性为long或int 且未进行指定是否自增 则自增
            if (_keyAttributes != null && _keyAttributes.Count == 1 && !_keyIncreaseHasSet)
            {
                var keyAttr = _clrType.GetProperty(_keyAttributes[0])?.PropertyType;
                if (keyAttr == typeof(long) || keyAttr == typeof(int)) _keyIsSelfIncreased = true;
            }

            //设置值
            entityType.KeyIsSelfIncreased = _keyIsSelfIncreased;
            entityType.KeyAttributes = _keyAttributes;
            entityType.Name = Name;
            entityType.Namespace = Namespace;
            entityType.TargetTable = TargetTable;
            entityType.NoticeAttributes = NoticeAttributes;
            entityType.NotifyCreation = NotifyCreation;
            entityType.NotifyDeletion = NotifyDeletion;
            entityType.NotifyUpdate = NotifyUpdate;
            entityType.VersionAttributes = VersionAttributes;
            entityType.ConcurrentConflictHandlingStrategy = ConcurrentConflictHandlingStrategy;

            return entityType;
        }

        /// <summary>
        ///     通过反射从CLR类型中收集元数据，生成类型配置项。
        /// </summary>
        /// <param name="analyticPipeline">类型解析管道。</param>
        internal override void ReflectionModeling(ITypeAnalyzer analyticPipeline)
        {
            var pipeLine = analyticPipeline;
            while (pipeLine != null)
            {
                //调用管道配置方法
                pipeLine.Configurate(_clrType, (IStructuralTypeConfigurator)this);
                pipeLine = pipeLine.Next;
            }
        }

        /// <summary>
        ///     创建引用元素
        /// </summary>
        /// <returns></returns>
        protected override ITypeElementConfigurator CreateReferenceElement(PropertyInfo propInfo)
        {
            //关联重数（表示是否是集合属性）
            var isMultiplicity = Utils.GetIsMultiple(propInfo, out var type);
            //是否是元组
            var isTuple = Utils.IsTuple(type);

            //配置的关联引用
            ITypeElementConfigurator associationReferenceConfig = null;

            //尝试按照显式进行查询
            var obvious = ModelBuilder.FindConfiguration(type);
            //不为空 则查询是否为关联型配置
            if (obvious != null)
            {
                var obviousAssociationConfig = typeof(AssociationTypeConfiguration<>);
                obviousAssociationConfig = obviousAssociationConfig.MakeGenericType(type);
                //目标类型被配置为显式关联型
                if (obvious.GetType() == obviousAssociationConfig)
                    //此显式关联引用未被配置
                    if (FindProperty(type) == null)
                    {
                        //配置一个显式关联
                        var obviousAssociationReferenceConfig =
                            CreateAssociationReference(propInfo.Name, type, isMultiplicity);
                        associationReferenceConfig = obviousAssociationReferenceConfig;

                        //将此显式关联型加入访问器存储
                        _explicitAssoRefProperties.Add(type, propInfo);
                    }
            }

            //没找到显示关联型
            //按照隐式关联型查询 引用的类型是否被配置为实体型
            //不是元组 按照普通的两方关联处理
            var endTypes = Array.Empty<Type>();
            if (!isTuple)
            {
                //查询属性类型模型配置项
                var implicitEntityConfig = ModelBuilder.FindConfiguration(type);
                if (implicitEntityConfig is IEntityTypeConfigurator)
                    //提取关联端
                    endTypes = new[] { typeof(TEntity), type };
            }
            //是元组 要分拆为多方关联
            else
            {
                //如果是元组 取出所有类型参数判断
                var configs = type.GetGenericArguments().Select(ModelBuilder.FindConfiguration).ToArray();
                //都是实体型 才进入推断
                if (configs.All(p => p is IEntityTypeConfigurator))
                {
                    //加入自己这一端的类型
                    var endTypesList = new List<Type> { typeof(TEntity) };
                    //取出另外的端类型
                    endTypesList.AddRange(type.GetGenericArguments());
                    //提取关联端
                    endTypes = endTypesList.ToArray();
                }
            }

            var endTags = AssociationConfiguratorBuilder.GenerateEndsTag(endTypes, ModelBuilder);
            //Tag不是空 且 没有配置过
            if (!string.IsNullOrEmpty(endTags) && FindProperty(endTags) == null)
            {
                //配置一个隐式关联关联引用
                //只有配置过隐式关联的才能配置引用
                var builder = ModelBuilder.FindImplicitAssociationConfigurationBuilder(endTags);
                if (builder != null)
                {
                    var end = builder.EndConfigurations.FirstOrDefault(p => p.EntityType == typeof(TEntity));
                    if (end != null)
                        associationReferenceConfig = CreateAssociationReference(propInfo.Name, type, isMultiplicity,
                            end.EndIndex, builder);
                }

                //加入隐式关联访问器存储
                _implicitAssoRefProperties.Add(endTags, propInfo);
            }


            //没有创建出来
            if (associationReferenceConfig == null)
                throw new ArgumentException(
                    $"无法为{ClrType.Name}的属性{propInfo.Name}配置关联引用,请检查是否为此属性关联引用对应的关联型(如已存在相同关联端的其他引用或此属性的类型未被配置为显式关联型).",
                    propInfo.Name);

            return associationReferenceConfig;
        }

        /// <summary>
        ///     创建隐式关联型建造器
        /// </summary>
        /// <returns></returns>
        protected internal override void CreateImplicitAssociationConfiguration()
        {
            //查找属性
            var properties = _clrType.GetProperties();

            foreach (var propInfo in properties)
            {
                //过滤属性不参与隐式关联推断
                if (IgnoreList.Contains(propInfo.Name)) continue;

                //继承的配置 不是当前类定义的 不处理
                if (propInfo.DeclaringType != _clrType && _derivingFrom != null) continue;

                //关联重数（表示是否是集合属性）
                Utils.GetIsMultiple(propInfo, out var type);

                //基元类型 不参与
                if (PrimitiveType.IsObasePrimitiveType(type))
                    continue;

                //是否是元组
                var isTuple = Utils.IsTuple(type);

                //是否元组
                if (isTuple)
                {
                    //如果是元组 取出所有类型参数判断
                    var configs = type.GetGenericArguments().Select(ModelBuilder.FindConfiguration).ToArray();
                    //任意一个不是实体型 不参与推断
                    if (configs.Any(p => !(p is IEntityTypeConfigurator)))
                        continue;
                }
                else
                {
                    var config = ModelBuilder.FindConfiguration(type);
                    //类型没配置 不参与
                    if (config == null)
                        continue;

                    //类型不是实体型 不参与
                    if (!(config is IEntityTypeConfigurator))
                        continue;
                }

                //是元组 要分拆为多方关联
                Type[] endTypes;
                if (isTuple)
                {
                    //加入自己这一端的类型
                    var endTypesList = new List<Type> { typeof(TEntity) };
                    //取出另外的端类型
                    endTypesList.AddRange(type.GetGenericArguments());
                    endTypes = endTypesList.ToArray();
                }
                else
                {
                    //不是元组 按照普通的两方关联处理
                    //提取关联端
                    endTypes = new[] { typeof(TEntity), type };
                }

                var endTags = AssociationConfiguratorBuilder.GenerateEndsTag(endTypes, ModelBuilder);
                //已配置为隐式关联的 不参与
                if (ModelBuilder.FindImplicitAssociationConfigurationBuilder(endTags) != null)
                    continue;

                //创建建造器
                var builder = ModelBuilder.Association();
                //每个端
                foreach (var endType in endTypes) builder.AssociationEnd(endType);
            }
        }

        /// <summary>
        ///     创建显式关联引用
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="assoType">关联型</param>
        /// <param name="isMultiple">是否多重</param>
        /// <returns></returns>
        private AssociationReferenceConfiguration<TEntity, EntityTypeConfiguration<TEntity>> CreateAssociationReference(
            string name, Type assoType, bool isMultiple)
        {
            if (!ElementConfigurations.ContainsKey(name))
            {
                //创建关联应用配置类型
                var assRefCfgType =
                    typeof(AssociationReferenceConfiguration<,>).MakeGenericType(typeof(TEntity), GetType());
                //创建关联应用配置类型 实例
                var assRefCfgInstance =
                    Activator.CreateInstance(assRefCfgType, name, assoType, isMultiple, this) as
                        AssociationReferenceConfiguration
                        <TEntity, EntityTypeConfiguration<TEntity>>;
                //添加元素项集合
                ElementConfigurations.Add(name, assRefCfgInstance);
            }

            //返回当前配置项
            return (AssociationReferenceConfiguration
                <TEntity, EntityTypeConfiguration<TEntity>>)ElementConfigurations[name];
        }

        /// <summary>
        ///     创建隐式关联引用
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="endType">端类型</param>
        /// <param name="isMultiple">是否多重</param>
        /// <param name="endIndex">端序号</param>
        /// <param name="assoConfigBuilder">隐式关联型建造器</param>
        /// <returns></returns>
        private AssociationReferenceConfiguration<TEntity> CreateAssociationReference(string name, Type endType,
            bool isMultiple,
            byte endIndex, AssociationConfiguratorBuilder assoConfigBuilder)
        {
            if (!ElementConfigurations.ContainsKey(name))
            {
                //创建关联应用配置类型
                var assRefCfgType =
                    typeof(ImplicitAssociationConfigor.AssociationReferenceConfiguration<,>).MakeGenericType(
                        typeof(TEntity), endType);
                //创建关联应用配置类型 实例
                var assRefCfgInstance =
                    Activator.CreateInstance(assRefCfgType, name, isMultiple, endIndex, assoConfigBuilder) as
                        AssociationReferenceConfiguration<TEntity>;
                //添加元素项集合
                ElementConfigurations.Add(name, assRefCfgInstance);
            }

            //返回当前配置项
            return (AssociationReferenceConfiguration<TEntity>)ElementConfigurations[name];
        }
    }
}