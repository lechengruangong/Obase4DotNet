/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 17:03:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using Obase.Core.Common;
using Obase.Core.Odm.Builder;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示属性。
    /// </summary>
    public class Attribute : TypeElement, IOrderBy
    {
        /// <summary>
        ///     数据类型
        /// </summary>
        private readonly Type _dataType;


        /// <summary>
        ///     修改触发器集合
        /// </summary>
        private List<IBehaviorTrigger> _changeTriggers = new List<IBehaviorTrigger>();

        /// <summary>
        ///     属性的合并处理器，负责在对象执行版本合并期间对属性进行处理。
        /// </summary>
        private IAttributeCombinationHandler _combinationHandler;

        /// <summary>
        ///     指示属性的值是否由数据库生成
        /// </summary>
        private bool _dbGenerateValue;

        /// <summary>
        ///     指示字段值是否可空。
        /// </summary>
        private bool _nullable = true;

        /// <summary>
        ///     值的精度，以小数位数表示，0表示不限制。
        /// </summary>
        private byte _precision;

        /// <summary>
        ///     映射字段
        /// </summary>
        private string _targetField;

        /// <summary>
        ///     属性值的长度，以位为单位，值为0表示不限制长度。
        ///     即数据类型所占字节数 * 8 对于字符串类型 默认为0 不限制具体的长度
        /// </summary>
        private ushort _valueLength;

        /// <summary>
        ///     创建Attribute实例。
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="name">属性名称</param>
        public Attribute(Type dataType, string name)
            : base(name, EElementType.Attribute)
        {
            _dataType = dataType;
            //不是复杂类型 也不是字符串 获取默认的长度
            if (!(this is ComplexAttribute) && _dataType != typeof(string))
                _valueLength = Utils.GetValueLength(dataType);
        }

        /// <summary>
        ///     获取一个值，该值指示是否为复杂属性。
        /// </summary>
        public bool IsComplex { get; internal set; }

        /// <summary>
        ///     获取或设置修改触发器集合。
        /// </summary>
        public List<IBehaviorTrigger> ChangeTriggers
        {
            get => _changeTriggers;
            set => _changeTriggers = value;
        }

        /// <summary>
        ///     获取或设置属性的数据类型。（给字段设的值就是这个类型，考虑能否和数据库类型兼容）
        /// </summary>
        public Type DataType => _dataType;

        /// <summary>
        ///     指示属性的值是否由数据库生成
        /// </summary>
        public bool DbGenerateValue
        {
            get => _dbGenerateValue;
            set => _dbGenerateValue = value;
        }

        /// <summary>
        ///     获取或设置属性的合并处理器，负责在对象执行版本合并期间对属性进行处理。
        /// </summary>
        public IAttributeCombinationHandler CombinationHandler
        {
            get => _combinationHandler;
            set => _combinationHandler = value;
        }

        /// <summary>
        ///     获取属性的值的类型。
        /// </summary>
        /// 实施说明:
        /// 属性值的类型实际上是其数据类型（DataType）在对象数据模型中的类型，可使用ObjectDataModel的GetType方法获取。
        public override TypeBase ValueType => HostType.Model.GetType(_dataType);

        /// <summary>
        ///     是否是由外键保证机制定义的
        /// </summary>
        internal bool IsForeignKeyDefineMissing { get; set; }

        /// <summary>
        ///     属性值的长度，以位为单位，值为0表示不限制长度。
        ///     即数据类型所占字节数 * 8 对于字符串类型 默认为0 不限制具体的长度
        ///     当此数值超过255时MySql和Sqlserver会被设定为Text类型
        /// </summary>
        public ushort ValueLength
        {
            get => _valueLength;
            set => _valueLength = value;
        }

        /// <summary>
        ///     指示字段值是否可空。
        /// </summary>
        public bool Nullable
        {
            get => _nullable;
            set => _nullable = value;
        }

        /// <summary>
        ///     值的精度，以小数位数表示，0表示不限制。
        /// </summary>
        public byte Precision
        {
            get => _precision;
            set => _precision = value;
        }

        /// <summary>
        ///     获取或设置映射字段。
        /// </summary>
        public string TargetField
        {
            get => _targetField;
            set => _targetField = value;
        }
    }
}