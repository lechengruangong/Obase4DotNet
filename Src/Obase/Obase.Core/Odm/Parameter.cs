/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：构造参数,用于描述类型构造函数的参数
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:58:15
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm.Builder;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     描述构造参数。构造参数用于描述类型构造函数的参数。
    ///     构造参数必须绑定到该类型的一个元素，该元素称为绑定元素。绑定元素为关联引用时，该关联引用的类型不能为显式关联型，即构造函数不能以关联对象作为参数。
    /// </summary>
    public class Parameter
    {
        /// <summary>
        ///     参数所属的构造器。
        /// </summary>
        private readonly IInstanceConstructor _constructor;

        /// <summary>
        ///     参数名称。
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     绑定元素的名称。
        /// </summary>
        private string _elementName;

        /// <summary>
        ///     如果为投影得出的 此参数绑定的表达式
        /// </summary>
        private Expression _expression;

        /// <summary>
        ///     值转换器，用于将存储源中的值转换为元素的值。
        /// </summary>
        private Func<object, object> _valueConverter;

        /// <summary>
        ///     创建Parameter实例。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="constructor">构造参数所属的构造器。</param>
        internal Parameter(string name, IInstanceConstructor constructor)
        {
            _name = name;
            _constructor = constructor;
        }

        /// <summary>
        ///     获取或设置绑定元素的名称。
        /// </summary>
        public string ElementName
        {
            get => _elementName;
            set => _elementName = value;
        }

        /// <summary>
        ///     获取参数名称。
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     获取参数绑定元素的类别（属性、关联引用等）。
        /// </summary>
        public EElementType ElementType => GetElement().ElementType;

        /// <summary>
        ///     获取或设置值转换器，用于将存储源中的值转换为元素的值。
        /// </summary>
        public Func<object, object> ValueConverter
        {
            get => _valueConverter;
            set => _valueConverter = value;
        }

        /// <summary>
        ///     如果为投影得出的 此参数绑定的表达式
        /// </summary>
        public Expression Expression
        {
            get => _expression;
            set => _expression = value;
        }

        /// <summary>
        ///     获取构造参数的绑定元素。
        /// </summary>
        public TypeElement GetElement()
        {
            TypeElement result;
            //如果获取的是具体类型区分标识的元素
            if (_constructor.InstanceType.ConcreteTypeSign != null && string.Equals(
                    _constructor.InstanceType.ConcreteTypeSign.Item1, _elementName,
                    StringComparison.CurrentCultureIgnoreCase))
                //返回映射字段为标识字段的元素
                result = _constructor.InstanceType.FindAttributeByTargetField(_constructor.InstanceType.ConcreteTypeSign
                    .Item1);
            else
                //否则返回普通的元素
                result = _constructor.InstanceType.GetElement(_elementName);

            return result;
        }

        /// <summary>
        ///     获取构造参数的类型。
        /// </summary>
        public new Type GetType()
        {
            return GetElement().ValueType.ClrType;
        }
    }
}