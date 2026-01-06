/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：显式关联型配置项.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 14:13:23
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     显式关联型配置项
    /// </summary>
    /// <typeparam name="TAssociation">关联型</typeparam>
    public class
        AssociationTypeConfiguration<TAssociation> : ObjectTypeConfiguration<TAssociation,
        AssociationTypeConfiguration<TAssociation>>, IAssociationTypeConfigurator
        where TAssociation : class
    {
        /// <summary>
        ///     指示是否为显式关联
        /// </summary>
        private bool _visible = true;

        /// <summary>
        ///     包含元素配置项
        /// </summary>
        protected Dictionary<string, TypeElementConfiguration> TypeElementConfigurations;

        /// <summary>
        ///     创建一个关联型配置项
        /// </summary>
        /// <param name="modelbuilder">建模器</param>
        public AssociationTypeConfiguration(ModelBuilder modelbuilder)
            : base(modelbuilder)
        {
        }

        /// <summary>
        ///     获取所有的元素配置项，包括属性配置项、关联端配置项。
        /// </summary>
        public override Dictionary<string, TypeElementConfiguration> ElementConfigurations
        {
            get => TypeElementConfigurations ??
                   (TypeElementConfigurations = new Dictionary<string, TypeElementConfiguration>());
            set => TypeElementConfigurations = value;
        }

        /// <summary>
        ///     键属性
        /// </summary>
        protected internal override List<string> KeyAttributes => new List<string>();

        /// <summary>
        ///     关联型
        /// </summary>
        public Type AssociationType => _clrType;

        /// <summary>
        ///     关联端集合
        /// </summary>
        public IAssociationEndConfigurator[] AssociationEnds => ElementConfigurations.Values
            .Where(p => p is IAssociationEndConfigurator).Cast<IAssociationEndConfigurator>().ToArray();

        /// <summary>
        ///     设置是否为显式关联型
        /// </summary>
        /// <param name="value">是否为显式关联型</param>
        /// <param name="overrided">是否覆盖</param>
        public void IsVisible(bool value, bool overrided = true)
        {
            //覆盖 直接设值
            if (overrided)
            {
                _visible = value;
            }
            else
            {
                //不覆盖的情况下 只有默认值才会被设值
                if (_visible)
                    _visible = value;
            }
        }

        /// <summary>
        ///     启动一个关联端配置项，如果要启动的配置项未创建则新建一个。
        /// </summary>
        /// <param name="name">关联端的名称。</param>
        /// <param name="entityType">作为关联端的实体类型。</param>
        IAssociationEndConfigurator IAssociationTypeConfigurator.AssociationEnd(string name, Type entityType)
        {
            return (IAssociationEndConfigurator)AssociationEnd(name, entityType);
        }

        /// <summary>
        ///     根据名称获取元素配置器。
        /// </summary>
        /// <param name="name">元素名称。</param>
        public override ITypeElementConfigurator GetElement(string name)
        {
            return TypeElementConfigurations[name] as ITypeElementConfigurator;
        }

        /// <summary>
        ///     启动一个关联端配置项，如果要启动的配置项未创建则新建一个。
        /// </summary>
        /// <typeparam name="TEnd">关联端类型</typeparam>
        /// <param name="name">关联端名称</param>
        /// <returns></returns>
        IAssociationEndConfigurator IAssociationTypeConfigurator.AssociationEnd<TEnd>(string name)
        {
            return (IAssociationEndConfigurator)AssociationEnd(name, typeof(TEnd));
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
        ///     启动一个关联端配置项，如果要启动的配置项未创建则新建一个。
        ///     <para>此方法为手动配置关联端,仅使用传入名称和类型创建关联端配置,不检查参数类型,不创建取值器和设值器</para>
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="entityType">作为关联端的实体类型。</param>
        public AssociationEndConfiguration<TAssociation> AssociationEnd(
            string name, Type entityType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "关联端名称不能为空");
            //转换为首字母大写
            name = name.Substring(0, 1).ToUpper() + name.Substring(1);

            //不存在 创建一个新的关联端配置项
            if (!ElementConfigurations.ContainsKey(name))
            {
                //创建关联端配置项
                var associationEnd = GenrateAssociationEndConfiguration(name, entityType);
                //添加到元素配置项
                ElementConfigurations.Add(name, associationEnd);
            }

            //如果已存在则直接返回
            return (AssociationEndConfiguration<TAssociation>)ElementConfigurations[name];
        }

        /// <summary>
        ///     根据Lambda表达式包含的信息启动一个关联端配置项，如果要启动的配置项未创建则新建一个
        ///     <para>此方法会检查传入名称是否存在于关联型中,且使用属性的访问器名称作为关联端名称,并且会尝试自动配置取值器和设值器</para>
        ///     <para>注意 关联端名称应与关联引用的左右端名称对应</para>
        /// </summary>
        /// <typeparam name="TResult">lambda表达式的返回值</typeparam>
        /// <param name="expression">lambda表达式</param>
        /// <returns></returns>
        public AssociationEndConfiguration<TAssociation, TResult>
            AssociationEnd<TResult>(
                Expression<Func<TAssociation, TResult>> expression) where TResult : class
        {
            if (expression.Body is MemberExpression member)
            {
                //解析表达式的名称
                var assEndName = member.Member.Name;

                var property = typeof(TAssociation).GetProperty(assEndName);
                //检查名称是否存在
                if (property == null)
                    throw new ArgumentNullException(nameof(assEndName),
                        $"无法在找关联型{typeof(TAssociation).FullName}内找到到关联端{assEndName}");
                //获取关联端配置项
                var assEnd = AssociationEnd(assEndName, typeof(TResult));

                //取值器
                if (property.GetMethod != null)
                    assEnd.HasValueGetter(property);
                //设值器
                if (property.SetMethod != null)
                    assEnd.HasValueSetter(property);

                return (AssociationEndConfiguration<TAssociation, TResult>)assEnd;
            }

            throw new ArgumentException("不能使用非属性访问表达式配置关联端");
        }

        /// <summary>
        ///     使用关联端的名称启动一个关联端配置项，如果要启动的配置项未创建则新建一个。
        ///     <para>此方法会检查传入名称是否存在于关联型中,且使用传入名称作为关联端名称,并且会尝试自动配置取值器和设值器</para>
        ///     <para>注意 关联端名称应与关联引用的左右端名称对应</para>
        /// </summary>
        /// <typeparam name="TEnd">作为关联端的实体类型</typeparam>
        /// <param name="name">关联端的名称</param>
        /// <returns></returns>
        public AssociationEndConfiguration<TAssociation>
            AssociationEnd<TEnd>(string name) where TEnd : class
        {
            //检查名称是否存在
            var property = typeof(TAssociation).GetProperty(name);

            if (property == null)
                throw new ArgumentNullException(nameof(name), $"无法在找关联型{typeof(TAssociation).FullName}内找到到关联端{name}");
            //获取关联端配置项
            var assEnd = AssociationEnd(name, typeof(TEnd));

            //取值器
            if (property.GetMethod != null)
                assEnd.HasValueGetter(property);
            //设值器
            if (property.SetMethod != null)
                assEnd.HasValueSetter(property);

            return assEnd;
        }

        /// <summary>
        ///     设置是否为显式关联。
        /// </summary>
        /// <param name="visible">true-是;false-否</param>
        public AssociationTypeConfiguration<TAssociation> HasVisible(bool visible)
        {
            _visible = visible;
            return this;
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型。
        /// </summary>
        protected override StructuralType CreateReally(ObjectDataModel buidingModel)
        {
            //根据配置项数据创建模型对象并设值
            AssociationType associationType;
            //检测基类
            if (_derivingFrom != null)
            {
                var derivingFrom = buidingModel.GetStructuralType(_derivingFrom);
                if (derivingFrom == null)
                    throw new ArgumentException($"无法找到{_clrType.FullName}所声明的基类{_derivingFrom.FullName},需要先注册基类.");

                associationType = new AssociationType(_clrType, derivingFrom);
            }
            else
            {
                associationType = new AssociationType(_clrType);
            }

            //检测构造器
            if (Constructor == null)
                throw new ArgumentException($"无法获取{_clrType.FullName}的public或protect internal且无参的构造函数,请为其配置构造函数.");
            //设置新实例构造器
            associationType.NewInstanceConstructor = NewInstanceConstructor;

            //检查映射表
            if (string.IsNullOrEmpty(TargetTable))
            {
                //如果未设置映射表，检查各关联端，找出第一个伴随端，以其实体型的映射表作为映射表。
                var associationEnd = ElementConfigurations.Values.OfType<AssociationEndConfiguration<TAssociation>>()
                    .FirstOrDefault(p => p.IsCompanionEnd);

                if (associationEnd != null)
                {
                    var entityType =
                        buidingModel.GetEntityType(associationEnd.EntityType);
                    _targetTable = entityType.TargetTable;
                }
            }

            //设值
            associationType.Constructor = Constructor;
            associationType.Name = Name;
            associationType.TargetTable = _targetTable;
            associationType.Visible = _visible;
            associationType.Namespace = Namespace;
            associationType.NotifyUpdate = NotifyUpdate;
            associationType.NotifyDeletion = NotifyDeletion;
            associationType.NotifyCreation = NotifyCreation;
            associationType.NoticeAttributes = NoticeAttributes;
            return associationType;
        }

        /// <summary>
        ///     创建引用元素
        /// </summary>
        /// <returns></returns>
        protected override ITypeElementConfigurator CreateReferenceElement(PropertyInfo propInfo)
        {
            AssociationEndConfiguration<TAssociation>
                associationEndConfiguration;

            //获取关联端配置项
            if (ElementConfigurations.TryGetValue(propInfo.Name, out var config))
            {
                associationEndConfiguration = (AssociationEndConfiguration<TAssociation>)config;
            }
            else
            {
                //检测关联端类型是否已在模型中注册
                if (ModelBuilder.FindConfiguration(propInfo.PropertyType) == null)
                    throw new ArgumentException($"{propInfo.PropertyType}未在模型中注册,无法配置为关联端");

                //创建关联端配置项
                var endConfig = AssociationEnd(propInfo.Name, propInfo.PropertyType);
                ElementConfigurations[propInfo.Name] = endConfig;

                //设置值
                associationEndConfiguration =
                    (AssociationEndConfiguration<TAssociation>)ElementConfigurations[propInfo.Name];
            }

            return associationEndConfiguration;
        }

        /// <summary>
        ///     创建隐式关联型建造器
        /// </summary>
        /// <returns></returns>
        protected internal override void CreateImplicitAssociationConfiguration()
        {
            //关联型上无需创建隐式关联型
        }

        /// <summary>
        ///     生成AssociationEndConfiguration实例
        /// </summary>
        /// <param name="name">关联端名称</param>
        /// <param name="entityType">实体类型</param>
        /// <returns></returns>
        private AssociationEndConfiguration<TAssociation> GenrateAssociationEndConfiguration(string name,
            Type entityType)
        {
            //创建一个关联端配置项 类型为typeof(TAssociation),端实体型
            var type = typeof(AssociationEndConfiguration<,>).MakeGenericType(typeof(TAssociation), entityType);
            //使用反射创建实例
            var cfg = Activator.CreateInstance(type, name, this);
            var result = (AssociationEndConfiguration<TAssociation>)cfg;
            return result;
        }
    }
}