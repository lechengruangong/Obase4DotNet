/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象构造器基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:47:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     为对象构造器提供基础实现。
    /// </summary>
    public abstract class InstanceConstructor : IInstanceConstructor
    {
        /// <summary>
        ///     要构造的实例的类型。
        /// </summary>
        private StructuralType _instanceType;

        /// <summary>
        ///     构造器的参数。
        /// </summary>
        private List<Parameter> _parameters;

        /// <summary>
        ///     构造器的参数类型
        /// </summary>
        private List<Type> _parameterTypes;

        /// <summary>
        ///     参数类型列表
        /// </summary>
        public List<Type> ParameterTypes
        {
            get { return _parameterTypes ?? (_parameterTypes = _parameters?.Select(p => p.GetType()).ToList()); }
            set => _parameterTypes = value;
        }

        /// <summary>
        ///     获取构造函数的形式参数。
        /// </summary>
        public List<Parameter> Parameters => _parameters;

        /// <summary>
        ///     获取或设置要构造的对象的类型。
        /// </summary>
        public StructuralType InstanceType
        {
            get => _instanceType;
            set => _instanceType = value;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments">构造函数参数。</param>
        public abstract object Construct(object[] arguments = null);

        /// <summary>
        ///     获取绑定到指定元素的构造函数参数。
        /// </summary>
        /// <param name="elementName">元素名称。</param>
        public Parameter GetParameterByElement(string elementName)
        {
            return _parameters?.FirstOrDefault(p => p.ElementName == elementName);
        }

        /// <summary>
        ///     设置构造参数，并指定其绑定元素的名称。
        ///     注：绑定元素为关联引用时，该关联引用的类型不能是显式关联型，否则引发异常“关联对象不能作为构造函数参数”。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="elementName">绑定元素的名称。</param>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        /// <param name="expression">如果此参数为投影得出的 此项表示此参数对应的表达式</param>
        protected internal void SetParameter(string name, string elementName,
            Func<object, object> valueConverter = null, Expression expression = null)
        {
            var para = new Parameter(name, this)
                { ElementName = elementName, ValueConverter = valueConverter, Expression = expression };
            if (_parameters == null) _parameters = new List<Parameter>();
            _parameters.Add(para);
        }

        /// <summary>
        ///     设置构造参数，其绑定元素与其同名。
        ///     注：绑定元素为关联引用时，该关联引用的类型不能是显式关联型，否则引发异常“关联对象不能作为构造函数参数”。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        protected internal void SetParameter(string name, Func<object, object> valueConverter = null)
        {
            SetParameter(name, name, valueConverter);
        }

        /// <summary>
        ///     默认的构造参数转换
        /// </summary>
        /// <param name="tValueType">目标类型</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        protected object DefaultConvert(Type tValueType, object value)
        {
            //目标对象和值对象空判断
            if (value == null || value is DBNull) return null;
            value = Utils.ConvertDbValue(value, tValueType);
            return value;
        }

        /// <summary>
        ///     创建一个构造器实例，该构造器使用委托创建类型实例。
        ///     实施说明
        ///     获取委托代表的方法的参数序列，根据参数个数及类型选择具体的DelegateConstructor类。
        /// </summary>
        /// <param name="constructor">用于创建类型实例的委托。</param>
        public static IInstanceConstructor Create(Delegate constructor)
        {
            var resultType = constructor.Method.ReturnType;
            var parameterTypes = constructor.Method.GetParameters().Skip(1).Select(p => p.ParameterType).ToArray();
            var genricTypes = parameterTypes.Concat(new[] { resultType }).ToArray();

            Type type;
            //根据参数个数选择具体的DelegateConstructor类
            switch (parameterTypes.Length)
            {
                case 0:
                    type = typeof(DelegateConstructor<>).MakeGenericType(genricTypes);
                    break;
                case 1:
                    type = typeof(DelegateConstructor<,>).MakeGenericType(genricTypes);
                    break;
                case 2:
                    type = typeof(DelegateConstructor<,,>).MakeGenericType(genricTypes);
                    break;
                case 3:
                    type = typeof(DelegateConstructor<,,,>).MakeGenericType(genricTypes);
                    break;
                case 4:
                    type = typeof(DelegateConstructor<,,,,>).MakeGenericType(genricTypes);
                    break;
                default:
                    throw new Exception($"不支持用参数个数为\"{parameterTypes.Count()}\"的视图表达式，构造类型视图。");
            }


            return (InstanceConstructor)Activator.CreateInstance(type, constructor);
        }
    }
}