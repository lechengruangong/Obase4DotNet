/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:43:56
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     为类型提供基础实现。
    /// </summary>
    public abstract class TypeBase
    {
        /// <summary>
        ///     类型对应的对象系统类型。
        /// </summary>
        protected Type _clrType;

        /// <summary>
        ///     类型名称。
        /// </summary>
        protected string _name;

        /// <summary>
        ///     类型所属的命名空间。
        /// </summary>
        protected string _namespace;

        /// <summary>
        ///     表示类型名称的结构体。
        /// </summary>
        protected TypeName _typeName;

        /// <summary>
        ///     初始化TypeBase的新实例，该实例描述指定的对象系统类型。
        /// </summary>
        /// <param name="clrType">对象系统的类型。</param>
        protected TypeBase(Type clrType)
        {
            ClrType = clrType;
        }

        /// <summary>
        ///     初始化TypeBase的新实例，该实例还没有关联的对象系统类型，有待后续指定。
        /// </summary>
        protected TypeBase()
        {
        }

        /// <summary>
        ///     获取表示类型名称的结构体。
        /// </summary>
        public TypeName TypeName => _typeName;

        /// <summary>
        ///     获取/设置类型所属的命名空间。
        /// </summary>
        public string Namespace
        {
            get => _namespace;
            set => _namespace = value;
        }

        /// <summary>
        ///     获取/设置类型名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     获取类型对应的对象系统类型。
        /// </summary>
        /// <exception cref="Exception">不能修改类型关联的对象系统类型。</exception>
        /// 实施说明
        /// 附加一个受保护的set访问器，执行以下操作：
        /// （1）设置_clrType属性；
        /// （2）同时设置_name和_nameSpace。
        /// 须注意，_clrType不允许修改，即如果其值不为null，调用该访问器会引发异常。

        public Type ClrType
        {
            get => _clrType;
            protected set
            {
                if (_clrType != null)
                    throw new ArgumentException("不能修改类型关联的对象系统类型。");
                _clrType = value;
                _typeName = GenerateObaseTypeName(value);
                _name = value.Name;
                _namespace = value.Namespace;
            }
        }

        /// <summary>
        ///     获取类型的完全限定名。
        /// </summary>
        public string FullName => $"{_clrType.FullName}";

        /// <summary>
        ///     生成TypeName
        ///     当为基元类型时返回对应的结构化表示法。
        /// </summary>
        private TypeName GenerateObaseTypeName(Type type)
        {
            //从基础类型中寻找预制的类型
            if (type == typeof(string))
                return TypeName.String;
            if (type == typeof(DateTime))
                return TypeName.DateTime;
            if (type == typeof(double))
                return TypeName.Double;
            if (type == typeof(float))
                return TypeName.Float;
            if (type == typeof(char))
                return TypeName.Char;
            if (type == typeof(bool))
                return TypeName.Boolean;
            if (type == typeof(byte))
                return TypeName.Byte;
            if (type == typeof(uint))
                return TypeName.SInt;
            if (type == typeof(long))
                return TypeName.Long;
            if (type == typeof(int))
                return TypeName.Int;
            return new TypeName { Name = type.Name, Namespace = type.Namespace };
        }
    }
}