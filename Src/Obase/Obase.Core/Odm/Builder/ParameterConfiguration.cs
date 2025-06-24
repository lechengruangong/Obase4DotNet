/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：构造参数配置项.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:48:15
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     构造参数配置项。
    ///     类型参数
    /// </summary>
    /// <typeparam name="TStructural">要配置的实体型、关联型或复杂类型。</typeparam>
    /// <typeparam name="TTypeConfiguration">创建当前参数配置项的类型配置项的类型。</typeparam>
    public class ParameterConfiguration<TStructural, TTypeConfiguration> : IParameterConfigurator
        where TTypeConfiguration : StructuralTypeConfiguration<TStructural, TTypeConfiguration>
    {
        /// <summary>
        ///     构造函数的参数序列。
        /// </summary>
        private readonly Queue<ParameterInfo> _constructorParameters;

        /// <summary>
        ///     创建参数配置项的类型配置项。
        /// </summary>
        private readonly StructuralTypeConfiguration<TStructural, TTypeConfiguration> _typeConfiguration;

        /// <summary>
        ///     创建ParameterConfiguration实例。
        /// </summary>
        /// <param name="constructorParas">构造函数参数信息集合。</param>
        /// <param name="typeConfiguration">创建当前参数配置项的类型配置项。</param>
        internal ParameterConfiguration(ParameterInfo[] constructorParas, TTypeConfiguration typeConfiguration)
        {
            _constructorParameters = new Queue<ParameterInfo>();
            foreach (var para in constructorParas) _constructorParameters.Enqueue(para);
            _typeConfiguration = typeConfiguration;
        }

        /// <summary>
        ///     在所有参数配置完成后返回到当前类型。
        /// </summary>
        /// <exception cref="Exception">还有参数没有配置，不能返回。</exception>
        IStructuralTypeConfigurator IParameterConfigurator.End()
        {
            return End();
        }


        /// <summary>
        ///     从构造函数参数队列取出一项，将之绑定到指定的类型元素。
        /// </summary>
        /// <param name="elementName">绑定元素的名称。</param>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        void IParameterConfigurator.Map(string elementName, Func<object, object> valueConverter)
        {
            Map(elementName, valueConverter);
        }

        /// <summary>
        ///     从构造函数参数队列取出一项，将之绑定到同名类型元素。
        /// </summary>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        void IParameterConfigurator.MapDefault(Func<object, object> valueConverter)
        {
            MapDefault(valueConverter);
        }

        /// <summary>
        ///     从构造函数参数队列取出一项，将之绑定到指定的类型元素。
        /// </summary>
        /// <param name="elementExp">表示绑定元素名称的表达式。</param>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        public ParameterConfiguration<TStructural, TTypeConfiguration> Map<TResult>(
            Expression<Func<TStructural, TResult>> elementExp, Func<object, object> valueConverter = null)
        {
            if (_constructorParameters.Count > 0)
            {
                var para = _constructorParameters.Dequeue();
                var constructor = _typeConfiguration.Constructor as InstanceConstructor;
                if(constructor == null) throw new Exception("当前类型的构造函数不是实例化构造函数，不能设置参数。");
                if(elementExp.Body.NodeType != ExpressionType.MemberAccess) throw new Exception("请用成员表达式绑定元素名称");
                var memberExpression = (MemberExpression)elementExp.Body;
                constructor.SetParameter(para.Name, memberExpression.Member.Name, valueConverter);
            }

            return this;
        }

        /// <summary>
        ///     从构造函数参数队列取出一项，将之绑定到指定的类型元素。
        /// </summary>
        /// <param name="elementName">绑定元素的名称。</param>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        public ParameterConfiguration<TStructural, TTypeConfiguration> Map(string elementName,
            Func<object, object> valueConverter = null)
        {
            if (_constructorParameters.Count > 0)
            {
                var para = _constructorParameters.Dequeue();
                var constructor = _typeConfiguration.Constructor as InstanceConstructor;
                if (constructor == null) throw new Exception("当前类型的构造函数不是实例化构造函数，不能设置参数。");
                constructor.SetParameter(para.Name, elementName, valueConverter);
            }

            return this;
        }

        /// <summary>
        ///     从构造函数参数队列取出一项，将之绑定到同名类型元素。
        /// </summary>
        /// <param name="valueConverter">值转换器，用于将存储源中的值转换为元素的值。</param>
        public ParameterConfiguration<TStructural, TTypeConfiguration> MapDefault(
            Func<object, object> valueConverter = null)
        {
            if (_constructorParameters.Count > 0)
            {
                var para = _constructorParameters.Dequeue();
                var constructor = _typeConfiguration.Constructor as InstanceConstructor;
                if (constructor == null) throw new Exception("当前类型的构造函数不是实例化构造函数，不能设置参数。");
                constructor.SetParameter(para.Name, para.Name, valueConverter);
            }

            return this;
        }

        /// <summary>
        ///     在所有参数配置完成后返回到当前类型。
        /// </summary>
        /// <exception cref="Exception">还有参数没有配置，不能返回。</exception>
        public TTypeConfiguration End()
        {
            if (_constructorParameters.Count >= 1) throw new Exception("还有参数没有配置，不能返回。");
            return (TTypeConfiguration)_typeConfiguration;
        }
    }
}
