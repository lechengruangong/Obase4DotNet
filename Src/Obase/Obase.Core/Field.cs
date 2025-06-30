/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示表中的一个字段.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:51:20
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core
{
    /// <summary>
    ///     表示表中的一个字段。
    /// </summary>
    public class Field
    {
        /// <summary>
        ///     数据类型。
        /// </summary>
        private readonly PrimitiveType _dataType;

        /// <summary>
        ///     是否自增
        /// </summary>
        private readonly bool _isSelfIncreasing;

        /// <summary>
        ///     字段的长度，以位为单位。
        /// </summary>
        private readonly ushort _length;

        /// <summary>
        ///     字段名。
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     指示字段值是否可空。
        /// </summary>
        private bool _nullable;

        /// <summary>
        ///     值的精度，以小数位数表示，0表示不限制。
        /// </summary>
        private byte _precision;

        /// <summary>
        ///     初始化Field类的新实例。
        /// </summary>
        /// <param name="name">字段名称。</param>
        /// <param name="dataType">字段的数据类型。</param>
        /// <param name="length">字段长度，以位为单位。</param>
        /// <param name="isSelfIncreasing">是否自增</param>
        /// <param name="precision">精度</param>
        /// <param name="nullable">是否可空</param>
        public Field(string name, PrimitiveType dataType, ushort length, bool isSelfIncreasing, byte precision,
            bool nullable)
        {
            _name = name;
            _dataType = dataType;
            _length = length;
            _isSelfIncreasing = isSelfIncreasing;
            _precision = precision;
            _nullable = nullable;
        }

        /// <summary>
        ///     获取字段的数据类型。
        /// </summary>
        public PrimitiveType DataType => _dataType;

        /// <summary>
        ///     获取字段的长度，以位为单位。
        ///     比如一个int 四个字节 一个字节8位 故 int类型为 4*8=32位
        /// </summary>
        public ushort Length => _length;

        /// <summary>
        ///     获取字段名。
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     是否自增
        /// </summary>
        public bool IsSelfIncreasing => _isSelfIncreasing;

        /// <summary>
        ///     指示字段值是否可空。
        /// </summary>
        public bool Nullable => _nullable;

        /// <summary>
        ///     值的精度，以小数位数表示，0表示不限制。
        /// </summary>
        public byte Precision => _precision;


        /// <summary>
        ///     生成字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"{nameof(_dataType)}: {_dataType}, {nameof(_isSelfIncreasing)}: {_isSelfIncreasing}, {nameof(_length)}: {_length}, {nameof(_name)}: {_name}, {nameof(_nullable)}: {_nullable}, {nameof(_precision)}: {_precision}, {nameof(DataType)}: {DataType}, {nameof(Length)}: {Length}, {nameof(Name)}: {Name}, {nameof(IsSelfIncreasing)}: {IsSelfIncreasing}, {nameof(Nullable)}: {Nullable}, {nameof(Precision)}: {Precision}";
        }
    }
}