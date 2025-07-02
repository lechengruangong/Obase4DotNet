/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基类的构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:46:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     适用于基类的构造器，它根据类型代码（作为构造函数）选择一个具体类型，构造该具体类型的实例。
    /// </summary>
    public class AbstractConstructor : InstanceConstructor
    {
        /// <summary>
        ///     指示派生类型的属性的名称
        /// </summary>
        private readonly string _typeAttributeName;

        /// <summary>
        ///     具体类型判别器。
        /// </summary>
        private readonly IConcreteTypeDiscriminator _typeDiscriminator;

        /// <summary>
        ///     初始化AbstractConstructor类的实例。
        /// </summary>
        /// <param name="parameters">派生类型的构造函数参数。</param>
        /// <param name="typeDiscriminator">派生类型判别器。</param>
        /// <param name="typeAttributeName">指示派生类型的属性的名称。</param>
        public AbstractConstructor(List<Parameter> parameters, IConcreteTypeDiscriminator typeDiscriminator,
            string typeAttributeName)
        {
            //复制参数
            if (parameters != null && parameters.Count > 0)
                foreach (var parameter in parameters)
                    SetParameter(parameter.Name, parameter.ElementName, parameter.ValueConverter);
            //加入判断区别的参数
            SetParameter("obase_gen_typeCode", typeAttributeName);
            //判别器
            _typeDiscriminator = typeDiscriminator;
            //名称
            _typeAttributeName = typeAttributeName;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments">构造函数参数。</param>
        public override object Construct(object[] arguments = null)
        {
            //判别类型
            var structuralType = GetDiscriminateType(arguments);

            //是自己 用基础类型构造器构造 否则用自己的构造器
            var constructor = structuralType == InstanceType
                ? structuralType.BaseTypeConstructor
                : structuralType.Constructor;
            //还是基类的构造器 继续传递
            if (constructor is AbstractConstructor abstractConstructor) return abstractConstructor.Construct(arguments);

            //去掉判断字段
            var realValues = arguments?.Take(arguments.Length - 1).ToArray();
            //构造具体值
            return constructor.Construct(realValues);
        }

        /// <summary>
        ///     根据字段获取判别类型
        /// </summary>
        /// <param name="arguments">构造函数的参数集合</param>
        /// <returns></returns>
        public StructuralType GetDiscriminateType(object[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
                throw new ArgumentException($"无法获取用于判别类型的属性{_typeAttributeName}.");
            //获取判别用值
            var value = arguments[arguments.Length - 1];
            //进行判别
            return _typeDiscriminator.Discriminate(value);
        }
    }
}