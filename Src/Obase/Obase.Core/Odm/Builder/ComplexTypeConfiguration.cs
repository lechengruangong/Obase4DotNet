/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：复杂类型配置项.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 14:44:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     复杂类型配置项
    /// </summary>
    /// <typeparam name="TComplex">复杂类型</typeparam>
    public class
        ComplexTypeConfiguration<TComplex> : StructuralTypeConfiguration<TComplex, ComplexTypeConfiguration<TComplex>>
    {
        /// <summary>
        ///     获取所有的元素配置项，包括属性配置项、关联端配置项
        /// </summary>
        private Dictionary<string, TypeElementConfiguration> _typeElementConfigurations;

        /// <summary>
        ///     创建一个复杂类型配置项
        /// </summary>
        /// <param name="modelbuilder">建模器</param>
        public ComplexTypeConfiguration(ModelBuilder modelbuilder)
            : base(modelbuilder)
        {
        }

        /// <summary>
        ///     复杂类型所包含元素配置项
        /// </summary>
        public override Dictionary<string, TypeElementConfiguration> ElementConfigurations
        {
            get => _typeElementConfigurations ??
                   (_typeElementConfigurations = new Dictionary<string, TypeElementConfiguration>());

            set => _typeElementConfigurations = value;
        }

        /// <summary>
        ///     标识属性集合
        /// </summary>
        protected internal override List<string> KeyAttributes => new List<string>();

        /// <summary>
        ///     创建引用元素
        /// </summary>
        /// <returns></returns>
        protected override ITypeElementConfigurator CreateReferenceElement(PropertyInfo propInfo)
        {
            //复杂类型无需配置引用元素
            return null;
        }

        /// <summary>
        ///     创建隐式关联型建造器
        /// </summary>
        /// <returns></returns>
        protected internal override void CreateImplicitAssociationConfiguration()
        {
            //复杂类型上无需创建隐式关联型
        }

        /// <summary>
        ///     创建复杂类型
        /// </summary>
        protected override StructuralType CreateReally(ObjectDataModel buidingModel)
        {
            //根据配置项数据创建模型对象并设值
            ComplexType structuralType;
            //处理派生关系
            if (_derivingFrom != null)
            {
                var derivingFrom = buidingModel.GetStructuralType(_derivingFrom);
                if (derivingFrom == null)
                    throw new ArgumentException($"无法找到{_clrType.FullName}所声明的基类{_derivingFrom.FullName},需要先注册基类.");
                structuralType = new ComplexType(_clrType, derivingFrom);
            }
            else
            {
                structuralType = new ComplexType(_clrType);
            }

            //检查构造器
            if (Constructor == null)
                //默认使用委托构造器
                Constructor = new DelegateConstructor<TComplex>(() => (TComplex)Activator.CreateInstance(_clrType));
            //新实例构造器
            structuralType.NewInstanceConstructor = NewInstanceConstructor;
            //设值
            structuralType.Constructor = Constructor;
            structuralType.Namespace = Namespace;
            structuralType.Name = Name;
            return structuralType;
        }

        /// <summary>
        ///     配置复杂类型
        /// </summary>
        /// <param name="model">对象数据模型</param>
        internal override void Configurate(ObjectDataModel model)
        {
            //复杂类型无需额外配置
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
        ///     通过反射从CLR类型中收集元数据，生成类型配置项。
        /// </summary>
        /// <param name="analyticPipeline">类型解析管道。</param>
        internal override void ReflectionModeling(ITypeAnalyzer analyticPipeline)
        {
            var pipeLine = analyticPipeline;
            while (pipeLine != null)
            {
                //调用管道配置方法
                pipeLine.Configurate(_clrType, this);
                pipeLine = pipeLine.Next;
            }
        }
    }
}