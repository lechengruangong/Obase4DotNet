/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体类型构造器配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-26 16:31:11
└──────────────────────────────────────────────────────────────┘
*/


using System;
using System.Collections.Generic;
using System.Reflection;
using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体类型构造器配置
    /// </summary>
    public class SerializationConstructorConfiguration<TStructural>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        private readonly ConstructorInfo _constructorInfo;

        /// <summary>
        ///     获取构造函数的形式参数。
        /// </summary>
        private readonly Dictionary<string, SerializationConstructorParameter> _parameters;

        /// <summary>
        ///     构造器的真实参数个数
        /// </summary>
        private readonly int _realParameterCount;

        /// <summary>
        ///     当前配置的参数索引
        /// </summary>
        private int _currentParameterIndex;

        /// <summary>
        ///     初始化序列化实体类型构造器配置
        /// </summary>
        /// <param name="constructorInfo">构造函数</param>
        public SerializationConstructorConfiguration(ConstructorInfo constructorInfo)
        {
            _constructorInfo = constructorInfo;
            _realParameterCount = constructorInfo.GetParameters().Length;
            _parameters = new Dictionary<string, SerializationConstructorParameter>();
        }


        /// <summary>
        ///     获取构造函数的形式参数。
        /// </summary>
        public Dictionary<string, SerializationConstructorParameter> Parameters => _parameters;

        /// <summary>
        ///     配置构造函数的参数
        /// </summary>
        /// <param name="field">取值字段</param>
        /// <param name="valueType">取得的值类型 如果设置needStorage为true 则在序列化时会检查取值器取得的值是否是此类型的</param>
        /// <param name="needStorage">
        ///     是否需要存储 如果是true 则取值器会在序列化时被调用 取得的值进行存储 此时传入的取值器的参数为当前要序列化的对象
        ///     如果是false 则取值器会在反序列化被调用 取得的值用于构造函数 此时传入的取值器的参数为null
        /// </param>
        /// <returns>自身</returns>
        public SerializationConstructorConfiguration<TStructural> HasParameter(FieldInfo field, Type valueType,
            bool needStorage)
        {
            //构造一个字段取值器
            var filedGetter = new FieldValueGetter(field);
            return HasParameter(filedGetter, valueType, needStorage);
        }

        /// <summary>
        ///     配置构造函数的参数
        /// </summary>
        /// <param name="getValue">取值委托</param>
        /// <param name="valueType">取得的值类型 如果设置needStorage为true 则在序列化时会检查取值器取得的值是否是此类型的</param>
        /// <param name="needStorage">
        ///     是否需要存储 如果是true 则取值器会在序列化时被调用 取得的值进行存储 此时传入的取值器的参数为当前要序列化的对象
        ///     如果是false 则取值器会在反序列化被调用 取得的值用于构造函数 此时传入的取值器的参数为null
        /// </param>
        /// <returns>自身</returns>
        public SerializationConstructorConfiguration<TStructural> HasParameter<TProperty>(
            Func<TStructural, TProperty> getValue, Type valueType, bool needStorage)
        {
            //创建一个委托取值器
            var valueGetter = new DelegateValueGetter<TStructural, TProperty>(getValue);
            return HasParameter(valueGetter, valueType, needStorage);
        }

        /// <summary>
        ///     配置构造函数的参数
        /// </summary>
        /// <param name="valueGetter">取值器</param>
        /// <param name="valueType">取得的值类型 如果设置needStorage为true 则在序列化时会检查取值器取得的值是否是此类型的</param>
        /// <param name="needStorage">
        ///     是否需要存储 如果是true 则取值器会在序列化时被调用 取得的值进行存储 此时传入的取值器的参数为当前要序列化的对象
        ///     如果是false 则取值器会在反序列化被调用 取得的值用于构造函数 此时传入的取值器的参数为null
        /// </param>
        /// <returns>自身</returns>
        public SerializationConstructorConfiguration<TStructural> HasParameter(IValueGetter valueGetter, Type valueType,
            bool needStorage)
        {
            //如果是需要存储的 检查值类型是否是Obase基础类型
            if (!PrimitiveType.IsObasePrimitiveType(valueType) && needStorage)
                throw new ArgumentException("需要存储的构造函数参数值类型必须是Obase基础类型。");
            var name = $"#{_currentParameterIndex}";
            //如果参数个数超过了构造函数的真实参数个数，抛出异常
            if (_currentParameterIndex >= _realParameterCount)
                throw new ArgumentException("构造函数的参数个数超过了构造函数的真实参数个数。");
            //如果配置的参数类型与构造函数的参数类型不匹配，抛出异常
            if (_constructorInfo.GetParameters()[_currentParameterIndex].ParameterType != valueType)
                throw new ArgumentException($"构造函数的第{_currentParameterIndex}个参数的类型与配置的值类型不匹配。");
            //添加参数配置
            _parameters.Add(name,
                new SerializationConstructorParameter(needStorage, name, valueType) { ValueGetter = valueGetter });
            _currentParameterIndex++;
            return this;
        }

        /// <summary>
        ///     创建序列化实体类型构造器
        /// </summary>
        /// <returns>序列化实体类型构造器</returns>
        public SerializationConstructor Create()
        {
            //创建一个序列化实体类型构造器
            var constructor = new SerializationConstructor(_constructorInfo);
            foreach (var parameter in _parameters) constructor.Parameters.Add(parameter.Key, parameter.Value);

            return constructor;
        }
    }
}