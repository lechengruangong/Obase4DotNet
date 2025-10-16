/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联引用配置项.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:57:45
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     关联引用配置项
    /// </summary>
    /// <typeparam name="TEntity">关联引用所属的实体类型</typeparam>
    public abstract class
        AssociationReferenceConfiguration<TEntity> :
        ReferenceElementConfiguration<TEntity, AssociationReferenceConfiguration<TEntity>>,
        IAssociationReferenceConfigurator
        where TEntity : class
    {
        /// <summary>
        ///     用于获取关联类型的委托。
        /// </summary>
        protected readonly Func<Type> AssociationTypeFunc;

        /// <summary>
        ///     聚合级别
        /// </summary>
        private EAggregationLevel _aggregationLevel = EAggregationLevel.None;

        /// <summary>
        ///     关联型对应的CLR类型
        /// </summary>
        protected Type _associationType;

        /// <summary>
        ///     左端名
        /// </summary>
        protected string _leftEnd;

        /// <summary>
        ///     右端名
        /// </summary>
        protected string _rightEnd;

        /// <summary>
        ///     创建类型元素配置项实例
        /// </summary>
        /// <param name="name">关联引用名称。</param>
        /// <param name="dataType">关联引用的关联类型。</param>
        /// <param name="isMultiple">指示关联引用是否具有多重性，即其值是否为集合。</param>
        /// <param name="typeConfiguration">关联引用所属的实体类型。</param>
        protected AssociationReferenceConfiguration(string name, Type dataType, bool isMultiple,
            EntityTypeConfiguration<TEntity> typeConfiguration) : base(name, isMultiple, typeConfiguration)
        {
            _associationType = dataType;
            ElementType = EElementType.AssociationReference;
        }

        /// <summary>
        ///     创建类型元素配置项实例
        /// </summary>
        /// <param name="name">关联引用名称</param>
        /// <param name="isMultiple">指示关联引用是否具有多重性，即其值是否为集合</param>
        /// <param name="typeConfiguration">关联引用所属的实体类型</param>
        /// <param name="associationTypeFunc">获取关联引用的关联类型的委托</param>
        protected AssociationReferenceConfiguration(string name, bool isMultiple,
            EntityTypeConfiguration<TEntity> typeConfiguration, Func<Type> associationTypeFunc) : base(name, isMultiple,
            typeConfiguration)
        {
            AssociationTypeFunc = associationTypeFunc;
            ElementType = EElementType.AssociationReference;
        }

        /// <summary>
        ///     左端名
        /// </summary>
        internal string LeftEnd => _leftEnd;

        /// <summary>
        ///     右端名
        /// </summary>
        internal string RightEnd => _rightEnd;

        /// <summary>
        ///     关联型对应的CLR类型
        /// </summary>
        internal Type AssociationType => _associationType;

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
        ///     设置聚合级别。
        /// </summary>
        /// <param name="level">聚合级别</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAssociationReferenceConfigurator.HasAggregationLevel(EAggregationLevel level, bool overrided)
        {
            // 如果是覆盖既有配置，则直接设置
            if (overrided)
            {
                HasAggregationLevel(level);
            }
            else
            {
                //是默认值
                if (_aggregationLevel == EAggregationLevel.None)
                    HasAggregationLevel(level);
            }
        }


        /// <summary>
        ///     设置左端名。
        /// </summary>
        /// <param name="leftEnd">左端名</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAssociationReferenceConfigurator.HasLeftEnd(string leftEnd, bool overrided)
        {
            // 如果是覆盖既有配置，则直接设置
            if (overrided)
            {
                HasLeftEnd(leftEnd);
            }
            else
            {
                //是默认值
                if (string.IsNullOrEmpty(_leftEnd))
                    HasLeftEnd(leftEnd);
            }
        }

        /// <summary>
        ///     设置右端名。
        /// </summary>
        /// <param name="rightEnd">右端名</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAssociationReferenceConfigurator.HasRightEnd(string rightEnd, bool overrided)
        {
            // 如果是覆盖既有配置，则直接设置
            if (overrided)
            {
                HasRightEnd(rightEnd);
            }
            else
            {
                //是默认值
                if (string.IsNullOrEmpty(_rightEnd))
                    HasRightEnd(rightEnd);
            }
        }

        /// <summary>
        ///     设置聚合级别。
        /// </summary>
        /// <param name="level">聚合级别</param>
        public AssociationReferenceConfiguration<TEntity> HasAggregationLevel(
            EAggregationLevel level)
        {
            _aggregationLevel = level;
            return this;
        }

        /// <summary>
        ///     设置左端名 即关联引用本端
        ///     注意 关联端名称应与关联引用的左右端名称对应
        /// </summary>
        /// <param name="leftEnd">左端名</param>
        internal AssociationReferenceConfiguration<TEntity> HasLeftEnd(string leftEnd)
        {
            _leftEnd = leftEnd;
            return this;
        }

        /// <summary>
        ///     设置右端名 即关联引用对端
        ///     注意 关联端名称应与关联引用的左右端名称对应
        /// </summary>
        /// <param name="rightEnd">右端名</param>
        internal AssociationReferenceConfiguration<TEntity> HasRightEnd(string rightEnd)
        {
            _rightEnd = rightEnd;
            return this;
        }

        /// <summary>
        ///     根据元素配置项包含的元数据信息创建元素实例。
        /// </summary>
        protected override TypeElement CreateReally(ObjectDataModel objectModel)
        {
            //首先检查_associationType是否为空，如果是则调用_associationTypeFunc委托获取关联类型。成功获取后调用基础实现。
            if (_associationType == null)
            {
                if (AssociationTypeFunc == null)
                    throw new ArgumentException("关联型和关联型获取委托不能同时为空.");

                _associationType = AssociationTypeFunc.Invoke();
            }

            if (_associationType == null)
                throw new ArgumentException("未能获取关联型类型.");

            var associationType = objectModel.GetAssociationType(_associationType);

            //根据配置项数据创建模型对象并设值
            var ass = new AssociationReference(Name, associationType, _leftEnd,
                _rightEnd)
            {
                AggregationLevel = _aggregationLevel,
                EnableLazyLoading = _enableLazyLoading,
                LoadingTriggers = LoadingTriggers,
                ValueGetter = ValueGetter,
                ValueSetter = ValueSetter,
                LoadingPriority = LoadingPriority,
                IsMultiple = IsMultiple
            };
            return ass;
        }
    }

    /// <summary>
    ///     关联引用配置项
    /// </summary>
    /// <typeparam name="TEntity">关联引用所属的实体类型</typeparam>
    /// <typeparam name="TTypeConfiguration">创建当前关联引用配置项的类型配置项的类型</typeparam>
    public class
        AssociationReferenceConfiguration<TEntity, TTypeConfiguration> : AssociationReferenceConfiguration<TEntity>
        where TEntity : class
        where TTypeConfiguration : EntityTypeConfiguration<TEntity>
    {
        /// <summary>
        ///     创建类型元素配置项实例
        /// </summary>
        /// <param name="name">关联引用名称。</param>
        /// <param name="dataType">关联引用的关联类型。</param>
        /// <param name="isMultiple">指示关联引用是否具有多重性，即其值是否为集合。</param>
        /// <param name="typeConfiguration">关联引用所属的实体类型。</param>
        public AssociationReferenceConfiguration(string name, Type dataType, bool isMultiple,
            EntityTypeConfiguration<TEntity> typeConfiguration) : base(name, dataType, isMultiple, typeConfiguration)
        {
        }

        /// <summary>
        ///     进入当前关联引用所属实体型的配置项。
        /// </summary>
        public TTypeConfiguration Upward()
        {
            return (TTypeConfiguration)_typeConfiguration;
        }
    }
}