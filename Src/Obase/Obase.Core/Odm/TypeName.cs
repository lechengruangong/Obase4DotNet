/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构化的类型名称.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:40:00
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     提供结构化的类型名称表示。
    /// </summary>
    [Serializable]
    public struct TypeName
    {
        /// <summary>
        ///     类型的命名空间。
        /// </summary>
        public string Namespace;

        /// <summary>
        ///     类型名称。
        /// </summary>
        public string Name;

        /// <summary>
        ///     整型名称的结构化表示法。
        /// </summary>
        public static TypeName Int = new TypeName { Name = "Int", Namespace = "Obase" };

        /// <summary>
        ///     长整型名称的结构化表示法。
        /// </summary>
        public static TypeName Long = new TypeName { Name = "Long", Namespace = "Obase" };

        /// <summary>
        ///     短整型名称的结构化表示法。
        /// </summary>
        public static TypeName SInt = new TypeName { Name = "SInt", Namespace = "Obase" };

        /// <summary>
        ///     字节型名称的结构化表示法。
        /// </summary>
        public static TypeName Byte = new TypeName { Name = "Byte", Namespace = "Obase" };

        /// <summary>
        ///     布尔型名称的结构化表示法。
        /// </summary>
        public static TypeName Boolean = new TypeName { Name = "Boolean", Namespace = "Obase" };

        /// <summary>
        ///     字符型名称的结构化表示法。
        /// </summary>
        public static TypeName Char = new TypeName { Name = "Char", Namespace = "Obase" };

        /// <summary>
        ///     单精度浮点型名称的结构化表示法。
        /// </summary>
        public static TypeName Float = new TypeName { Name = "Float", Namespace = "Obase" };

        /// <summary>
        ///     双精度浮点型名称的结构化表示法。
        /// </summary>
        public static TypeName Double = new TypeName { Name = "Double", Namespace = "Obase" };

        /// <summary>
        ///     日期时间型名称的结构化表示法。
        /// </summary>
        public static TypeName DateTime = new TypeName { Name = "DateTime", Namespace = "Obase" };

        /// <summary>
        ///     字符串型名称的结构化表示法。
        /// </summary>
        public static TypeName String = new TypeName { Name = "String", Namespace = "Obase" };

        /// <summary>
        ///     获取类型的完全限定名。
        /// </summary>
        public string FullName => $"{Namespace}.{Name}";

        /// <summary>
        ///     指示是否为实体类型。
        /// </summary>
        public bool IsEntity;

        /// <summary>
        ///     指示是否为关联类型。
        /// </summary>
        public bool IsAssociation;

        /// <summary>
        ///     测试两个对象是否相等，重写自Object.Equals。
        /// </summary>
        /// <param name="otherObj">参与比较的另一个对象。</param>
        public override bool Equals(object otherObj)
        {
            if (ReferenceEquals(null, otherObj)) return false;
            var right = (TypeName)otherObj;
            return Equals(right);
        }

        /// <summary>
        ///     测试两个类型名称是否相等。
        /// </summary>
        /// <param name="other">参与比较的另一个类型名称。</param>
        public bool Equals(TypeName other)
        {
            return other.Name.Equals(Name) && other.Namespace.Equals(Namespace);
        }

        /// <summary>
        ///     获取当前类型名称代表的CLR类型。
        /// </summary>
        public new Type GetType()
        {
            //从当前所有加载的程序集内搜索
            var assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblyArray)
            {
                //用于搜索的名称
                var searchName = FullName;
                //处理预定义类型
                if (Equals(Int) || Equals(Long) || Equals(SInt) || Equals(Byte) || Equals(Boolean) || Equals(Char) ||
                    Equals(Float) || Equals(Double) || Equals(DateTime) ||
                    Equals(String)) searchName = $"System.{Name}";
                //获取类型
                var type = assembly.GetType(searchName);
                if (type != null) return type;
            }

            return null;
        }

        /// <summary>
        ///     从指定的程序集中获取当前类型名称代表的CLR类型。
        /// </summary>
        /// <param name="assembly">程序集名称。</param>
        public Type GetType(string assembly)
        {
            var type = Assembly.Load(assembly).GetType(FullName);
            return type;
        }

        /// <summary>
        ///     获取当前类型名称的哈希代码。
        /// </summary>
        public override int GetHashCode()
        {
            var uniteCode = Namespace.GetHashCode() + Name.GetHashCode();
            return uniteCode.GetHashCode();
        }

        /// <summary>
        ///     判定当前类型名称代表的类型是否为基元类型。
        /// </summary>
        public bool IsPrimitive()
        {
            var type = GetType();
            return type != null && PrimitiveType.IsObasePrimitiveType(type);
        }

        /// <summary>
        ///     转换成字符串表达形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        ///     重载运算符 ==
        /// </summary>
        /// <param name="thisTypeName"></param>
        /// <param name="otherTypeName"></param>
        /// <returns></returns>
        public static bool operator ==(TypeName thisTypeName, TypeName otherTypeName)
        {
            return thisTypeName.Equals(otherTypeName);
        }

        /// <summary>
        ///     重载运算符 !=
        /// </summary>
        /// <param name="thisTypeName"></param>
        /// <param name="otherTypeName"></param>
        /// <returns></returns>
        public static bool operator !=(TypeName thisTypeName, TypeName otherTypeName)
        {
            return !(thisTypeName == otherTypeName);
        }
    }
}