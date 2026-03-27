/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 12:38:44
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化实体类型
    /// </summary>
    public class SerializationEntity
    {
        /// <summary>
        ///     类型对应的对象系统类型。
        /// </summary>
        private readonly Type _clrType;

        /// <summary>
        ///     所有的元素集合 包含属性和构造参数
        /// </summary>
        private readonly List<SerializationElement> _elements = new List<SerializationElement>();

        /// <summary>
        ///     类型名称。
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     构造器
        /// </summary>
        private SerializationConstructor _constructor;

        /// <summary>
        ///     初始化序列化实体类型
        /// </summary>
        /// <param name="clrType">运行时类型</param>
        public SerializationEntity(Type clrType)
        {
            _clrType = clrType;
            _name = clrType.FullName;
        }

        /// <summary>
        ///     所有的元素集合 包含属性和构造参数
        /// </summary>
        public List<SerializationElement> Elements => _elements;

        /// <summary>
        ///     所有的属性集合
        /// </summary>
        public List<SerializationAttribute> Attributes => _elements.OfType<SerializationAttribute>().ToList();

        /// <summary>
        ///     所有的构造函数参数集合
        /// </summary>
        public List<SerializationConstructorParameter> ConstructorParameters =>
            _constructor?.Parameters.Values.ToList();

        /// <summary>
        ///     所有的引用集合
        /// </summary>
        public List<SerializationReference> References => _elements.OfType<SerializationReference>().ToList();

        /// <summary>
        ///     类型对应的对象系统类型。
        /// </summary>
        public Type ClrType => _clrType;

        /// <summary>
        ///     类型名称。
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     构造器
        /// </summary>
        public SerializationConstructor Constructor
        {
            get => _constructor;
            set => _constructor = value;
        }

        /// <summary>
        ///     完整性检查
        ///     继承类需要检查则重写此方法
        /// </summary>
        /// <param name="errDictionary">错误信息字典</param>
        public void IntegrityCheck(Dictionary<string, List<string>> errDictionary)
        {
            //错误消息
            var message = new List<string>();
            //检查属性
            foreach (var attribute in Attributes)
            {
                if (string.IsNullOrWhiteSpace(attribute.Name))
                    message.Add("序列化实体的属性名称不能为空.");
                if (attribute.ValueGetter == null)
                    message.Add($"{Name}的属性{attribute.Name}没有取值器.");
                if (attribute.ValueSetter == null)
                    message.Add($"{Name}的属性{attribute.Name}没有设值器.");
            }

            //检查构造器
            if (Constructor == null)
            {
                message.Add($"{Name}没有构造器.");
            }
            else
            {
                if (Constructor.RealParameterCount != ConstructorParameters.Count)
                    message.Add(
                        $"{Name}的构造器应有{Constructor.RealParameterCount}参数,实际上仅配置了{ConstructorParameters.Count}个.");
            }

            //检查引用
            foreach (var reference in References)
            {
                if (string.IsNullOrWhiteSpace(reference.Name))
                    message.Add("序列化实体的引用名称不能为空.");
                if (reference.ValueGetter == null)
                    message.Add($"{Name}的引用{reference.Name}没有取值器.");
                if (reference.ValueSetter == null)
                    message.Add($"{Name}的引用{reference.Name}没有设值器.");
            }

            //如果有检查失败消息
            if (message.Any())
            {
                //就与现有的问题合并
                var name = _clrType?.FullName ?? _name;
                if (errDictionary.ContainsKey(name))
                    errDictionary[name].AddRange(message);
                else
                    errDictionary.Add(name, message);
            }
        }

        /// <summary>
        ///     字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"SerializationEntity:{{Name-\"{Name}\",ClrType-\"{ClrType}\"}}";
        }
    }
}