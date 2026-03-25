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
using System.Threading;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化实体类型
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public class SerializationEntity
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

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
        private ISerializationConstructor _constructor;

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
        public List<SerializationConstructorParameter> ConstructorParameters => _constructor?.Parameters;

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
        public ISerializationConstructor Constructor
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
            
        }
    }
}