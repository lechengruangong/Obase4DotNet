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
using System.Reflection;

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
        ///     指示是否为复杂属性。
        /// </summary>
        private bool _isComplex;


        /// <summary>
        ///     属性的合并处理器。
        /// </summary>
        private IAttributeCombinationHandler _attributeCombinationHandler = new OverwriteCombinationHandler();

        /// <summary>
        ///     修改触发器集合
        /// </summary>
        private List<IBehaviorTrigger> _changeTriggers;

        /// <summary>
        ///     映射连接符。
        /// </summary>
        private char _mappingConnectionChar = char.MinValue;

        /// <summary>
        ///     数据类型（对应数据库类型，取值器取出的数据类型要和数据库字段兼容）
        /// </summary>
        internal readonly Type DataType;

        /// <summary>
        ///     映射字段（数据库字段名，用以从sql读取器取值）
        /// </summary>
        internal string TargetField;

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
        ///     属性配置项
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="dataType">属性类型</param>
        /// <param name="typeConfiguration">创建当前属性配置项的类型配置项。</param>
        protected AttributeConfiguration(string name, Type dataType, StructuralTypeConfiguration typeConfiguration) : base(name,
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
    }
}
