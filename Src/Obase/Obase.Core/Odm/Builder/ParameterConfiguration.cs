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
                if (constructor == null) throw new ArgumentException("当前类型的构造函数不是实例化构造函数，不能设置参数。");
                if (elementExp != null && elementExp.Body.NodeType != ExpressionType.MemberAccess)
                    throw new ArgumentException("请用成员表达式绑定元素名称");
                var memberExpression = (MemberExpression)elementExp?.Body;
                var memberName = memberExpression?.Member.Name;
                //校验绑定元素 避免构造参数被绑定到关联引用、关联端等引用元素上
                EnsureBindableElement(memberName, para.Name, nameof(elementExp));
                constructor.SetParameter(para.Name, memberName, valueConverter);
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
                if (constructor == null) throw new ArgumentException("当前类型的构造函数不是实例化构造函数，不能设置参数。");
                //校验绑定元素 避免构造参数被绑定到关联引用、关联端等引用元素上
                EnsureBindableElement(elementName, para.Name, nameof(elementName));
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
                if (constructor == null) throw new ArgumentException("当前类型的构造函数不是实例化构造函数，不能设置参数。");
                //校验绑定元素 避免参数名恰好与导航属性同名时被静默绑定到关联引用、关联端等引用元素上
                EnsureBindableElement(para.Name, para.Name, "parameterName");
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
            if (_constructorParameters.Count >= 1) throw new ArgumentException("还有参数没有配置，不能返回。");
            return (TTypeConfiguration)_typeConfiguration;
        }

        /// <summary>
        ///     校验构造参数的绑定元素是否合法。
        ///     构造参数只能绑定到本类型的简单属性（包括复杂类型属性和多重性属性），不能绑定到关联引用、关联端等引用元素。
        ///     说明
        ///     本校验尽力而为：Map执行时元素配置项未必全部建立，能判定则抛出可读异常，判不了则交由建模阶段的其它检查兜底。
        /// </summary>
        /// <param name="elementName">绑定元素名称。</param>
        /// <param name="parameterName">参数名称（用于报错定位）。</param>
        /// <param name="paramName">调用方参数名（用于ArgumentException的ParamName）。</param>
        private void EnsureBindableElement(string elementName, string parameterName, string paramName)
        {
            //名称都未给出 无法判定
            if (string.IsNullOrEmpty(elementName)) return;

            var structuralType = typeof(TStructural);
            //① 优先使用已建立的元素配置项判断：引用元素（关联引用、关联端）一律不允许
            if (_typeConfiguration.ElementConfigurations != null &&
                _typeConfiguration.ElementConfigurations.TryGetValue(elementName, out var element))
            {
                if (IsReferenceElementConfiguration(element))
                    throw new ArgumentException(
                        $"类型{structuralType.Name}的构造函数参数{parameterName}不能绑定到{elementName}：" +
                        $"该元素是引用元素（关联引用或关联端），构造函数参数只能绑定到本类型的简单属性。" +
                        $"关系请改用AssociationEnd + HasMapping + AssociationReference配置。",
                        paramName);
            }

            //② 兜底：配置项可能尚未建立，按CLR属性的类型和类型的注册情况判断
            //注：MapDefault按参数名绑定，参数名与属性名只是大小写不同（如homeCountry与HomeCountry），故忽略大小写取属性
            var property = structuralType.GetProperty(elementName,
                             BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy) ??
                           structuralType.GetProperty(elementName,
                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                               BindingFlags.IgnoreCase);
            //取不到属性（名称写错或非public成员）交由建模阶段的检查兜底
            if (property == null) return;
            //Obase基元类型 按简单属性处理
            var propertyType = property.PropertyType;
            if (PrimitiveType.IsObasePrimitiveType(propertyType)) return;

            //集合或数组 取元素类型判断
            var elementType = propertyType;
            if (elementType.IsArray)
                elementType = elementType.GetElementType();
            else if (elementType.GetInterface("IEnumerable") != null && elementType.GenericTypeArguments.Length == 1)
                elementType = elementType.GenericTypeArguments[0];

            if (elementType == null) return;
            elementType = Nullable.GetUnderlyingType(elementType) ?? elementType;
            if (PrimitiveType.IsObasePrimitiveType(elementType)) return;

            //未注册的类型不是结构化类型 不在此处判定
            var propertyConfig = _typeConfiguration.ModelBuilder?.FindConfiguration(elementType);
            if (propertyConfig == null) return;

            //复杂类型允许作为构造参数
            if (propertyConfig.GetType().IsGenericType &&
                propertyConfig.GetType().GetGenericTypeDefinition() == typeof(ComplexTypeConfiguration<>))
                return;

            //实体型、关联型等结构化类型属于导航或关联 不是可落库的简单属性
            //绑定名与属性名大小写不同时（如MapDefault的参数名homeCountry与属性HomeCountry）把两者都报出来
            var bindingDescription = string.Equals(elementName, property.Name, StringComparison.Ordinal)
                ? elementName
                : $"{elementName}（实际命中属性{property.Name}）";
            throw new ArgumentException(
                $"类型{structuralType.Name}的构造函数参数{parameterName}不能绑定到{bindingDescription}：" +
                $"该属性的类型{elementType.Name}是已注册的结构化类型（实体型或关联型），属于导航或关联，不是可落库的简单属性。" +
                $"关系请改用AssociationEnd + HasMapping + AssociationReference配置。",
                paramName);
        }

        /// <summary>
        ///     判断一个元素配置项是否为引用元素（关联引用或关联端）。
        ///     说明
        ///     ReferenceElementConfiguration只有泛型形态ReferenceElementConfiguration&lt;TObject, TConfiguration&gt;，
        ///     没有非泛型基类，因此沿基类链比对泛型定义。AssociationReferenceConfiguration与AssociationEndConfiguration都继承它，
        ///     AttributeConfiguration与ComplexTypeConfiguration不会命中。
        /// </summary>
        /// <param name="element">元素配置项</param>
        /// <returns>是引用元素返回true</returns>
        private static bool IsReferenceElementConfiguration(TypeElementConfiguration element)
        {
            if (element == null) return false;
            for (var type = element.GetType(); type != null && type != typeof(object); type = type.BaseType)
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReferenceElementConfiguration<,>))
                    return true;
            return false;
        }
    }
}
