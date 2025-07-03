/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：显式关联的关联端的配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:25:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     显式关联的关联端的配置
    /// </summary>
    /// <typeparam name="TAssociation">此关联端的关联型</typeparam>
    public abstract class AssociationEndConfiguration<TAssociation> : ReferenceElementConfiguration<
        TAssociation,
        AssociationEndConfiguration<TAssociation>>
        where TAssociation : class
    {
        /// <summary>
        ///     实体型对应的CLR类型
        /// </summary>
        protected readonly Type _entityType;

        /// <summary>
        ///     反射建模加入的映射
        /// </summary>
        private readonly HashSet<string> _reflectAddedMapping = new HashSet<string>();

        /// <summary>
        ///     指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        private bool _defaultAsNew;

        /// <summary>
        ///     指示当前关联端是否为聚合关联端。
        /// </summary>
        private bool _isAggregated;

        /// <summary>
        ///     指示当前关联端是否作为伴随端
        /// </summary>
        protected bool _isCompanionEnd;

        /// <summary>
        ///     关联端映射集合
        /// </summary>
        protected List<AssociationEndMapping> _mappings;

        /// <summary>
        ///     构造一个关联端配置项
        /// </summary>
        /// <param name="name">关联端名称</param>
        /// <param name="dataType">关联端的实体类型</param>
        /// <param name="typeConfiguration">关联端配置项所属的类型配置项。</param>
        protected AssociationEndConfiguration(string name, Type dataType,
            AssociationTypeConfiguration<TAssociation> typeConfiguration) : base(
            name, false, typeConfiguration)
        {
            _entityType = dataType;
            _mappings = new List<AssociationEndMapping>();
            ElementType = EElementType.AssociationEnd;
        }

        /// <summary>
        ///     获取元素类型。
        /// </summary>
        public override EElementType ElementType { get; }

        /// <summary>
        ///     触发器集合
        /// </summary>
        public override List<IBehaviorTrigger> BehaviorTriggers =>
            LoadingTriggers ?? (LoadingTriggers = new List<IBehaviorTrigger>());

        /// <summary>
        ///     端的Clr类型
        /// </summary>
        public Type EntityType => _entityType;

        /// <summary>
        ///     获取指示当前关联端是否作为伴随端
        /// </summary>
        internal bool IsCompanionEnd => _isCompanionEnd;

        /// <summary>
        ///     设置关联端映射
        /// </summary>
        /// <param name="keyAttribute">此端的标志属性</param>
        /// <param name="targetField">此段在关联表内的映射属性</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasMapping(string keyAttribute, string targetField, bool overrided)
        {
            if (_mappings == null)
                _mappings = new List<AssociationEndMapping>();
            //覆盖的 清除已有的映射
            if (overrided)
                _mappings.Clear();
            var keys = $"{keyAttribute}/{targetField}";
            //没有任何映射 直接加入
            if (_mappings.Count == 0)
            {
                _mappings.Add(new AssociationEndMapping { KeyAttribute = keyAttribute, TargetField = targetField });
                //记录一下 是由反射加入的
                _reflectAddedMapping.Add(keys);
            }
            //已有映射
            else
            {
                //当前Mapping内的所有映射
                var exKeys = _mappings.Select(p => $"{p.KeyAttribute}/{p.TargetField}").OrderBy(p => p).ToArray();
                var flag = _reflectAddedMapping.OrderBy(p => p).SequenceEqual(exKeys);
                //如果由反射加入的集合与当前Mapping集合一一对应
                if (flag)
                {
                    //就可以加入
                    HasMapping(keyAttribute, targetField);
                    //记录一下 是由反射加入的
                    _reflectAddedMapping.Add(keys);
                }
                //否则 不加入 因为当前Mapping内是由其他方式加入的 不可以覆盖
            }
        }

        /// <summary>
        ///     设置一个值，该值指示当前关联端是否为聚合关联端。
        /// </summary>
        /// <param name="isAggregated">指示当前关联端是否为聚合关联端。</param>
        public AssociationEndConfiguration<TAssociation> IsAggregated(bool isAggregated)
        {
            _isAggregated = isAggregated;
            return this;
        }

        /// <summary>
        ///     设置一个值地，该值指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        /// <param name="defaultAsNew">指示是否把关联端对象默认视为新对象。</param>
        public AssociationEndConfiguration<TAssociation> HasDefaultAsNew(bool defaultAsNew)
        {
            _defaultAsNew = defaultAsNew;
            return this;
        }

        /// <summary>
        ///     设置关联端映射。
        ///     每次调用此方法将追加一个关联端映射。
        ///     其中keyAttribute为此端的键属性
        ///     targetField为此端的键属性在关联表中映射的字段
        /// </summary>
        /// <param name="keyAttribute">此端的标志属性</param>
        /// <param name="targetField">此段在关联表内的映射属性</param>
        public AssociationEndConfiguration<TAssociation> HasMapping(string keyAttribute,
            string targetField)
        {
            if (_mappings == null)
                _mappings = new List<AssociationEndMapping>();
            //如果当前端的映射中不存在此映射才加入
            if (_mappings.All(p => p.TargetField != targetField && p.KeyAttribute != keyAttribute))
                _mappings.Add(new AssociationEndMapping { KeyAttribute = keyAttribute, TargetField = targetField });
            return this;
        }

        /// <summary>
        ///     指示是否将当前关联端作为伴随端。
        ///     设置当前端为伴随端会将之前设值的伴随端改设不作为伴随端
        /// </summary>
        /// <param name="asCompanion">是否为伴随端</param>
        /// <returns></returns>
        public AssociationEndConfiguration<TAssociation> AsCompanion(bool asCompanion)
        {
            //如果设置为伴随端 则先将其他端都设置为不作为伴随端
            if (asCompanion)
            {
                var endConfigs = ((AssociationTypeConfiguration<TAssociation>)_typeConfiguration).ElementConfigurations
                    .Values.OfType<AssociationEndConfiguration<TAssociation>>().ToList();
                foreach (var endConfig in endConfigs) endConfig.AsCompanion(false);
            }

            _isCompanionEnd = asCompanion;
            return this;
        }

        /// <summary>
        ///     根据元素配置项包含的元数据信息创建元素实例。
        /// </summary>
        protected override TypeElement CreateReally(ObjectDataModel objectModel)
        {
            var endEntityType = objectModel.GetEntityType(_entityType);
            //检查当前端的实体类型是否在模型中注册
            if (endEntityType == null)
                throw new ArgumentException($"{_entityType.Name}未在模型中注册.");

            //检查当前端的映射是否包含键属性
            foreach (var mapping in _mappings)
                if (_entityType.GetProperty(mapping.KeyAttribute) == null)
                    throw new ArgumentException(
                        $"关联型{typeof(TAssociation).FullName}的关联端{Name}中不包含键属性{mapping.KeyAttribute}", nameof(Name));


            //根据配置项数据创建模型对象并设值
            var end = new AssociationEnd(Name, endEntityType)
            {
                Mappings = _mappings,
                LoadingTriggers = BehaviorTriggers,
                EnableLazyLoading = _enableLazyLoading,
                IsMultiple = IsMultiple,
                ValueGetter = _valueGetter,
                ValueSetter = _valueSetter,
                DefaultAsNew = _defaultAsNew,
                LoadingPriority = LoadingPriority,
                IsAggregated = _isAggregated
            };
            return end;
        }
    }

    /// <summary>
    ///     适用于显式关联的关联端配置项。
    /// </summary>
    /// <typeparam name="TAssociation">关联端所属关联型的类型。</typeparam>
    /// <typeparam name="TEntity">关联端的实体类型。</typeparam>
    public class
        AssociationEndConfiguration<TAssociation, TEntity> :
        AssociationEndConfiguration<TAssociation>,
        IAssociationEndConfigurator
        where TAssociation : class
        where TEntity : class
    {
        /// <summary>
        ///     基于当前关联定义的关联引用的配置器。
        /// </summary>
        private AssociationReferenceConfiguration<TEntity> _associationReferenceConfiguration;

        /// <summary>
        ///     构造关联端配置项实例。
        /// </summary>
        /// <param name="name">关联端名称。</param>
        /// <param name="typeConfiguration">关联端配置项所属的类型配置项。</param>
        public AssociationEndConfiguration(string name, AssociationTypeConfiguration<TAssociation> typeConfiguration) :
            base(name,
                typeof(TEntity), typeConfiguration)
        {
        }

        /// <summary>
        ///     获取该关联端上基于当前关联定义的关联引用。
        /// </summary>
        public IAssociationReferenceConfigurator ReferenceConfigurator => _associationReferenceConfiguration;

        /// <summary>
        ///     设置一个值，该值指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        /// <param name="defaultAsNew">指示是否把关联端对象默认视为新对象。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAssociationEndConfigurator.HasDefaultAsNew(bool defaultAsNew, bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
                HasDefaultAsNew(defaultAsNew);
        }

        /// <summary>
        ///     设置一个值，该值指示当前关联端是否为聚合关联端。
        /// </summary>
        /// <param name="isAggregated">指示当前关联端是否为聚合关联端。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAssociationEndConfigurator.IsAggregated(bool isAggregated, bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
                IsAggregated(isAggregated);
        }

        /// <summary>
        ///     指示是否将当前关联端作为伴随端。
        ///     说明
        ///     设置当前端为伴随端会将之前设置的伴随端改设不作为伴随端。
        ///     当override为false时，其它端只要任意一端已设置为伴随端，本方法就不再执行设置操作。
        /// </summary>
        /// <param name="value">指示是否作为伴随端。</param>
        /// <param name="overrided">指示是否覆盖既有设置。</param>
        void IAssociationEndConfigurator.AsCompanion(bool value, bool overrided)
        {
            //覆盖 直接设置
            if (overrided)
            {
                AsCompanion(value);
            }
            else
            {
                //否则 查询当前所有的关联端配置
                var endConfigs = ((AssociationTypeConfiguration<TAssociation>)_typeConfiguration).ElementConfigurations
                    .Values.OfType<AssociationEndConfiguration<TAssociation>>().ToList();
                //只有不存在任何伴随端时才设置当前端为伴随端
                if (!endConfigs.Any(p => p.IsCompanionEnd)) AsCompanion(value);
            }
        }

        /// <summary>
        ///     生成基于当前关联定义的关联引用的配置器，如果配置器已存在返回该配置器。
        /// </summary>
        /// <returns>关联引用配置器；如果当前关联端实体型上未定义相应的关联引用，返回null。</returns>
        /// <param name="propInfo">返回关联引用的访问器，如果关联引用没有访问器返回null。</param>
        public IAssociationReferenceConfigurator AssociationReference(out PropertyInfo propInfo)
        {
            propInfo = _entityType.GetProperty(_name);
            return _associationReferenceConfiguration;
        }

        /// <summary>
        ///     配置关联端映射。
        /// </summary>
        /// <param name="expression">代表关联端实体型的标识属性的表达式。</param>
        /// <param name="targetField">上述标识属性的映射字段。</param>
        public AssociationEndConfiguration<TAssociation, TEntity> HasMapping<TProperty>(
            Expression<Func<TEntity, TProperty>> expression, string targetField)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException($"表达式({expression})不能配置关联端映射。");
            var member = (MemberExpression)expression.Body;
            _mappings.Add(new AssociationEndMapping { KeyAttribute = member.Member.Name, TargetField = targetField });
            return this;
        }

        /// <summary>
        ///     进入当前元素所属类型的配置项。
        /// </summary>
        public StructuralTypeConfiguration Upward()
        {
            return _typeConfiguration;
        }

        /// <summary>
        ///     启动对关联端上基于当前关联定义的关联引用的配置，如果相应的配置项未创建则新建一个。
        ///     实施说明
        ///     调用AssociationReference(PropertyInfo)方法。
        /// </summary>
        /// <param name="expression">lamda表达式</param>
        public AssociationReferenceConfiguration<TEntity> AssociationReference(
            Expression<Func<TEntity, TAssociation>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                //获取实体模型对应类型的属性
                var property = typeof(TEntity).GetProperty(member.Member.Name);

                if (property == null)
                    throw new ArgumentNullException(nameof(member.Member.Name),
                        $"无法在实体型{typeof(TEntity).FullName}内找到到关联引用{member.Member.Name}");
                return AssociationReference(property);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置关联引用");
        }

        /// <summary>
        ///     启动对关联端上基于当前关联定义的关联引用的配置，如果相应的配置项未创建则新建一个。
        ///     实施说明
        ///     调用AssociationReference(PropertyInfo)方法。
        /// </summary>
        /// <param name="expression">lamda表达式</param>
        public AssociationReferenceConfiguration<TEntity> AssociationReference(
            Expression<Func<TEntity, IEnumerable<TAssociation>>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                //获取实体模型对应类型的属性
                var property = typeof(TEntity).GetProperty(member.Member.Name);

                if (property == null)
                    throw new ArgumentNullException(nameof(member.Member.Name),
                        $"无法在实体型{typeof(TEntity).FullName}内找到到关联引用{member.Member.Name}");
                return AssociationReference(property);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置关联引用");
        }

        /// <summary>
        ///     启动对关联端上基于当前关联定义的关联引用的配置，如果相应的配置项未创建则新建一个。
        ///     实施说明
        ///     如果_associationReferenceConfiguration不为null，直接返回。应寄存新建的配置器，避免重复创建。
        ///     参见顺序图“配置显式关联”。
        /// </summary>
        /// <param name="name">关联引用名称，它将作为关联引用的键</param>
        /// <param name="isMultiple">关联引用是否具有多重性。</param>
        public AssociationReferenceConfiguration<TEntity> AssociationReference(string name, bool isMultiple)
        {
            //获取实体模型对应类型的属性
            var property = typeof(TEntity).GetProperty(name);

            if (property == null)
                throw new ArgumentNullException(nameof(name),
                    $"无法在实体型{typeof(TEntity).FullName}内找到到关联引用{name}");

            return AssociationReference(property, isMultiple);
        }


        /// <summary>
        ///     启动对关联端上基于当前关联定义的关联引用的配置，如果相应的配置项未创建则新建一个。
        ///     实施说明
        ///     如果_associationReferenceConfiguration不为null，直接返回。应寄存新建的配置器，避免重复创建。
        ///     以访问器的名称创建关联引用配置项实例，然后进行如下配置：
        ///     （1）检测是否存在get方法（含protected internal），如果存在配置取值器；
        ///     （2）检测是否存在set方法（含protected internal），如果存在配置设值器；
        ///     （3）如果存在get方法且为virtual则配置加载触发器；
        ///     （4）设置左端名。
        ///     参见顺序图“配置显式关联”。
        /// </summary>
        /// <param name="propInfo">关联引用的访问器。</param>
        /// <param name="multi">是否是多个</param>
        private AssociationReferenceConfiguration<TEntity> AssociationReference(PropertyInfo propInfo,
            bool? multi = null)
        {
            //没有配置的情况下 才创建新的配置
            if (_associationReferenceConfiguration == null)
            {
                //名称
                var name = propInfo.Name;

                //是否集合属性
                var isMultiple = Utils.GetIsMultipe(propInfo, out _);
                if (multi != null)
                    if (isMultiple != multi)
                        isMultiple = multi.Value;

                //首先 查找端的配置
                var entityTypeConfiguration =
                    (EntityTypeConfiguration<TEntity>)_typeConfiguration.ModelBuilder.FindConfiguration(
                        typeof(TEntity));

                if (entityTypeConfiguration == null)
                    throw new ArgumentException($"类型为{typeof(TEntity)}的实体型未注册");

                //创建关联应用配置类型
                var assRefCfgType =
                    typeof(AssociationReferenceConfiguration<,>).MakeGenericType(typeof(TEntity),
                        typeof(EntityTypeConfiguration<>).MakeGenericType(typeof(TEntity)));
                //创建关联应用配置类型 实例
                var configuration =
                    Activator.CreateInstance(assRefCfgType, name, typeof(TAssociation), isMultiple,
                            entityTypeConfiguration) as AssociationReferenceConfiguration
                        <TEntity, EntityTypeConfiguration<TEntity>>;

                if (configuration == null)
                    throw new ArgumentException($"创建类型为{propInfo.ReflectedType}.{propInfo.Name}关联引用配置类型失败");

                //取值器
                if (propInfo.GetMethod != null)
                    configuration.HasValueGetter(propInfo);
                //设值器
                if (propInfo.SetMethod != null)
                    configuration.HasValueSetter(propInfo);

                //配置
                configuration.HasLeftEnd(_name);
                //保存
                _associationReferenceConfiguration = configuration;
                //加入实体型的配置
                entityTypeConfiguration.AddAssociationReference(_associationReferenceConfiguration);
            }

            return _associationReferenceConfiguration;
        }
    }
}