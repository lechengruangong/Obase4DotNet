/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：适用于隐式关联的关联引用配置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:24:35
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     适用于隐式关联的关联引用配置器
    /// </summary>
    /// <typeparam name="TEntity">关联引用所属的实体类型</typeparam>
    /// <typeparam name="TReferred">被引对象组成的元组的类型 被引对象是指关联引用指向的一个或一组对象，如果关联引用是多重性的，它是指其中的一个或一组。</typeparam>
    public class AssociationReferenceConfiguration<TEntity, TReferred> : AssociationReferenceConfiguration<TEntity>
        where TEntity : class
        where TReferred : class
    {
        /// <summary>
        ///     关联引用所在关联端在关联型上的索引号（从1开始计数）。
        /// </summary>
        private readonly byte _associationEndIndex;

        /// <summary>
        ///     隐式关联建造器
        /// </summary>
        private readonly AssociationConfiguratorBuilder _builder;


        /// <summary>
        ///     元组标准化函数及其反函数。
        /// </summary>
        private ITupleStandardizer _tupleStandardizer;

        /// <summary>
        ///     创建类型元素配置项实例
        /// </summary>
        /// <param name="name">关联引用名称。</param>
        /// <param name="isMultiple">指示关联引用是否具有多重性，即其值是否为集合。</param>
        /// <param name="endIndex">关联引用所在关联端在关联型上的索引号（从1开始计数）</param>
        /// <param name="assoConfigBuilder">关联配置器建造器</param>
        public AssociationReferenceConfiguration(string name, bool isMultiple,
            byte endIndex, AssociationConfiguratorBuilder assoConfigBuilder) : base(name, isMultiple,
            (EntityTypeConfiguration<TEntity>)assoConfigBuilder.ModelBuilder.FindConfiguration(typeof(TEntity)),
            () => assoConfigBuilder.AssociationType)
        {
            //向基类构造函数传入获取关联类型的方法，该方法通过调用AssociationConfiguratorBuilder的AssociationType访问器获取关联类型。
            // 通过调用AssociationConfiguratorBuilder的EndCount访问器获取关联端个数。
            // 通过调用AssociationConfiguratorBuilder的ModelBuilder访问器获取建模器，然后根据类型参数TEntity查找关联引用所属实体型的配置器。
            _builder = assoConfigBuilder;
            _associationEndIndex = endIndex;
        }

        /// <summary>
        ///     设置元组标准化函数及其反函数。
        /// </summary>
        /// <param name="standardingFunc">元组标准化函数。</param>
        /// <param name="revertingFunc">标准化函数的反函数。</param>
        public AssociationReferenceConfiguration<TEntity, TReferred> HasTupleStandardizer(
            Func<TReferred, object> standardingFunc, Func<object, TReferred> revertingFunc)
        {
            _tupleStandardizer = new DelegateTupleStandardizer<TReferred>(standardingFunc, revertingFunc);
            return this;
        }

        /// <summary>
        ///     根据元素配置项包含的元数据信息实际执行创建元素实例的操作。
        ///     实施说明
        ///     首先替换取值器和设值器，（参见活动图“替换取值器和设值器”）然后调用基础实现。
        /// </summary>
        /// <param name="objectModel">对象数据模型</param>
        protected override TypeElement CreateReally(ObjectDataModel objectModel)
        {
            //首先检查_associationType是否为空，如果是则调用_associationTypeFunc委托获取关联类型。成功获取后调用基础实现。
            if (_associationType == null)
            {
                if (AssociationTypeFunc == null)
                    throw new ArgumentException("关联型和关联型获取委托不能同时为空.");

                _associationType = AssociationTypeFunc.Invoke();
            }

            var property = typeof(TEntity).GetProperty(Name);
            //查找关联型配置
            var assConfig = _builder.ModelBuilder.FindConfiguration(_associationType);
            var assType = assConfig.CreatedType;
            //构造元组标准化处理器
            if (_tupleStandardizer == null)
            {
                if (_builder.EndCount == 2)
                    _tupleStandardizer = new TwoAssociationTupleStandardizer();
                else
                    _tupleStandardizer = new MultiAssociationTupleStandardizer(property);
            }

            //构造包装器
            var wrapper = new AssociationReferenceValueWrapper(_valueGetter, _valueSetter, _tupleStandardizer,
                (AssociationType)assType, _isMultiple, _associationEndIndex, _builder.EndCount != 2);
            //替换
            _valueGetter = wrapper;
            _valueSetter = wrapper;

            return base.CreateReally(objectModel);
        }
    }
}