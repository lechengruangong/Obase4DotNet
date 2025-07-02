/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：隐式关联型配置器的建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:56:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     隐式关联型配置器的建造器
    /// </summary>
    public class AssociationConfiguratorBuilder
    {
        /// <summary>
        ///     关联端配置项。
        /// </summary>
        private readonly List<AssociationEndConfiguration> _endConfigurations = new List<AssociationEndConfiguration>();

        /// <summary>
        ///     关联的扩展配置器。
        ///     键:扩展配置器的类型
        /// </summary>
        private readonly Dictionary<Type, TypeExtensionConfiguration> _extensionConfigurations =
            new Dictionary<Type, TypeExtensionConfiguration>();

        /// <summary>
        ///     所属的建模器。
        /// </summary>
        private readonly ModelBuilder _modelBuilder;

        /// <summary>
        ///     生成的关联类型。
        /// </summary>
        private Type _associationType;

        /// <summary>
        ///     当前配置出的关联型配置
        /// </summary>
        private StructuralTypeConfiguration _associationTypeConfiguration;

        /// <summary>
        ///     关联端个数。
        /// </summary>
        private byte _endCount;

        /// <summary>
        ///     关联端标签
        /// </summary>
        private string _endsTag;

        /// <summary>
        ///     映射表。
        /// </summary>
        private string _targetTable;


        /// <summary>
        ///     初始化AssociationConfiguratorBuilder类的新实例。
        /// </summary>
        /// <param name="modelBuilder">所属的建模器。</param>
        public AssociationConfiguratorBuilder(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        /// <summary>
        ///     获取生成的关联类型。
        /// </summary>
        public Type AssociationType => _associationType;

        /// <summary>
        ///     获取关联端个数。
        /// </summary>
        public byte EndCount => _endCount;

        /// <summary>
        ///     获取建模器
        /// </summary>
        public ModelBuilder ModelBuilder => _modelBuilder;

        /// <summary>
        ///     关联端配置项。
        /// </summary>
        public List<AssociationEndConfiguration> EndConfigurations => _endConfigurations;

        /// <summary>
        ///     启动对一个新关联端的配置。
        /// </summary>
        /// <returns>关联端配置项。</returns>
        public AssociationEndConfiguration<TEntity> AssociationEnd<TEntity>() where TEntity : class
        {
            return (AssociationEndConfiguration<TEntity>)AssociationEnd(typeof(TEntity));
        }

        /// <summary>
        ///     启动对一个新关联端的配置。
        /// </summary>
        /// <returns>关联端配置项。</returns>
        /// <param name="endType">作为关联端的实体类型。</param>
        public AssociationEndConfiguration AssociationEnd(Type endType)
        {
            _endsTag = string.Empty;
            //检测端类型
            var endModelType = _modelBuilder.FindConfiguration(endType);
            if (endModelType == null)
                throw new ArgumentException($"{endType}类型还未注册,不能参与构建隐式关联.");
            if (!(endModelType is IEntityTypeConfigurator))
                throw new ArgumentException($"{endType}类型不是实体型,不能参与构建隐式关联.");
            //反射创建关联端配置项
            var endConfigurationType = typeof(AssociationEndConfiguration<>).MakeGenericType(endType);
            //递增关联端计数
            _endCount++;
            var endConfiguration =
                (AssociationEndConfiguration)Activator.CreateInstance(endConfigurationType, _endCount, this);
            //加入端配置
            _endConfigurations.Add(endConfiguration);

            return endConfiguration;
        }

        /// <summary>
        ///     为类型配置项设置一个扩展配置器。
        /// </summary>
        public TExtensionConfiguration HasExtension<TExtensionConfiguration>()
            where TExtensionConfiguration : TypeExtensionConfiguration
        {
            var extType = typeof(TExtensionConfiguration);
            //检查扩展配置器类型 如果有 直接返回
            if (_extensionConfigurations.TryGetValue(extType, out var extensionConfiguration))
                return (TExtensionConfiguration)extensionConfiguration;
            //反射创建扩展配置项
            extensionConfiguration =
                (TypeExtensionConfiguration)Activator.CreateInstance(extType);
            //加入配置
            _extensionConfigurations.Add(extType, extensionConfiguration);
            return (TExtensionConfiguration)_extensionConfigurations[extType];
        }

        /// <summary>
        ///     生成关联端标签。
        ///     关联端标签是以关联端类型（实体型）的完全限定名（即以命名空间限定的名称）串联而成的字符串。同一组类型（顺序无关）建立的多个隐式关联具有相同的关联端标签。
        /// </summary>
        public string GenerateEndsTag()
        {
            //实施说明
            //遍历关联端配置项，获取实体类型，然后调用本方法的静态版本。
            //寄存生成结果以免重复生成。AssociationEnd方法被调用时清空寄存的结果。

            if (_endCount < 2)
                throw new ArgumentException($"隐式关联端的数量不能少于2,当前仅有关联端{_endCount}个.");

            if (!string.IsNullOrEmpty(_endsTag))
                return _endsTag;
            //构造端类型数组
            var endTypes = _endConfigurations.Select(p => p.EntityType).ToArray();
            //生成关联端标签
            _endsTag = GenerateEndsTag(endTypes, _modelBuilder);
            return _endsTag;
        }

        /// <summary>
        ///     设置映射表。
        /// </summary>
        /// <param name="table">映射表</param>
        public AssociationConfiguratorBuilder ToTable(string table)
        {
            _targetTable = table;
            return this;
        }

        /// <summary>
        ///     建造关联型配置器。
        ///     说明
        ///     在建造器的整个生命周期中只能执行一次生成操作，如果此前已执行过，不执行任何操作。
        ///     参见顺序图“生成隐式关联配置项”。
        /// </summary>
        internal StructuralTypeConfiguration Build()
        {
            //已生成 直接返回
            if (_associationTypeConfiguration != null)
                return _associationTypeConfiguration;

            //检查关联端
            if (_endCount < 2)
                throw new ArgumentException($"隐式关联端的数量不能少于2,当前仅有关联端{_endCount}个.");

            //生成隐式关联型
            //以ImplicitAssociation为基类 定义若干个关联端字段
            var fileds = _endConfigurations.OrderBy(p => p.EntityType.FullName).Select(end =>
                new FieldDescriptor(end.EntityType, end.Name)
                    { HasGetter = true, HasSetter = true, PublicGetter = true, PublicSetter = true }).ToArray();
            //组合SubName
            var subName = _endConfigurations.OrderBy(p => p.EntityType.FullName).Select(p => p.EntityType.Name)
                .ToArray();
            //动态创建的关联型完全限定名
            var fullName = $"ImplicitAssociation_{string.Join("_", subName)}";

            //定义一个隐式关联型的Clr类型
            _associationType = ImplicitAssociationManager.Current.ApplyType(fileds.ToArray(), fullName);

            var companionEndTargetTable = string.Empty;
            //查找伴随端
            var endConfig = _endConfigurations.FirstOrDefault(p => p.IsCompanionEnd);
            if (endConfig != null) companionEndTargetTable = GetEntityTargetTable(endConfig.EntityType);

            //如果根据关联端伴随推断了表 且 表和设置的不同
            if (!string.IsNullOrEmpty(companionEndTargetTable) && !string.IsNullOrEmpty(_targetTable) &&
                companionEndTargetTable != _targetTable)
                throw new ArgumentException(
                    $"{GenerateEndsTag()}间的关联,设置的映射表{_targetTable}与设置的伴随端映射表{companionEndTargetTable}不相同.");
            //推断成功赋值 否则等到处理类型管道时赋值
            if (!string.IsNullOrEmpty(companionEndTargetTable))
                _targetTable = companionEndTargetTable;

            //构造关联型配置
            var associationTypeConfigType = typeof(AssociationTypeConfiguration<>).MakeGenericType(_associationType);
            var parameter = new object[]
            {
                _endConfigurations.ToArray(), _extensionConfigurations.Values.ToArray(),
                GenerateEndsTag(), _modelBuilder
            };

            //动态绑定
            var associationTypeConfig = (IAssociationTypeConfigurator)Activator.CreateInstance(
                associationTypeConfigType, BindingFlags.NonPublic | BindingFlags.Instance, null, parameter, null);

            //配置 如果关联端等于2 就设为普通的隐式关联
            associationTypeConfig.IsVisible(_endCount != 2);
            associationTypeConfig.ToTable(_targetTable);
            //保存
            _associationTypeConfiguration = (StructuralTypeConfiguration)associationTypeConfig;

            return _associationTypeConfiguration;
        }

        /// <summary>
        ///     获取实体型的映射表
        /// </summary>
        /// <param name="entityType">实体型</param>
        /// <returns></returns>
        private string GetEntityTargetTable(Type entityType)
        {
            var structuralTypeConfiguration = _modelBuilder.FindConfiguration(entityType);
            if (structuralTypeConfiguration is IEntityTypeConfigurator configurator)
                return configurator.TargetTable;
            throw new ArgumentException($"{entityType}没有被配置为实体型.");
        }

        /// <summary>
        ///     根据指定的关联端实体型（顺序不敏感）生成关联端标签。
        ///     实施说明
        ///     从建模器查找类型的配置项，从中获取类型的命名空间和名称，组合成完全限定名。
        ///     将上述完全限定名排序然后串联，即为当前关联型的关联端标签。
        /// </summary>
        /// <param name="endTypes">作为关联端的实体型，顺序不敏感。</param>
        /// <param name="modelBuilder">建模器。</param>
        public static string GenerateEndsTag(Type[] endTypes, ModelBuilder modelBuilder)
        {
            //取限定名
            var fullNameList = endTypes.Select(modelBuilder.FindConfiguration)
                .Select(endModelType => endModelType.ClrType.FullName).ToList();
            //排序
            fullNameList = fullNameList.OrderBy(p => p).ToList();
            //组合
            var tags = string.Join("/", fullNameList);

            return tags;
        }
    }
}