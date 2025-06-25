/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：一些内部使用的工具,封装了常用的方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:49:16
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.Common
{
    /// <summary>
    ///     一些内部使用的工具
    /// </summary>
    public static class Utils
    {
        /// <summary>
        ///     值长度字典 单位 位
        /// </summary>
        private static readonly Dictionary<Type, ushort> ValueLengthDictionary = new Dictionary<Type, ushort>();

        /// <summary>
        ///     初始化一些内部使用的工具
        /// </summary>
        static Utils()
        {
            //初始化值长度字典
            ValueLengthDictionary.Add(typeof(byte), 1 * 8);
            ValueLengthDictionary.Add(typeof(sbyte), 1 * 8);
            ValueLengthDictionary.Add(typeof(short), 2 * 8);
            ValueLengthDictionary.Add(typeof(ushort), 2 * 8);
            ValueLengthDictionary.Add(typeof(int), 4 * 8);
            ValueLengthDictionary.Add(typeof(uint), 4 * 8);
            ValueLengthDictionary.Add(typeof(long), 8 * 8);
            ValueLengthDictionary.Add(typeof(ulong), 8 * 8);
            ValueLengthDictionary.Add(typeof(char), 2 * 8);
            ValueLengthDictionary.Add(typeof(bool), 1 * 8);
            ValueLengthDictionary.Add(typeof(float), 4 * 8);
            ValueLengthDictionary.Add(typeof(double), 8 * 8);
            ValueLengthDictionary.Add(typeof(decimal), 16 * 8);
            ValueLengthDictionary.Add(typeof(string), 0);
            ValueLengthDictionary.Add(typeof(DateTime), 8 * 8);
            ValueLengthDictionary.Add(typeof(TimeSpan), 4 * 8);
            ValueLengthDictionary.Add(typeof(Guid), 32 * 8);
        }

        /// <summary>
        ///     获取类型值长度
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static ushort GetValueLength(Type type)
        {
            //枚举 2字节
            if (type.IsEnum)
                return 2 * 8;

            if (ValueLengthDictionary.TryGetValue(type, out var length))
                return length;
            throw new ArgumentException($"无法确定{type}类型的长度,因为其不是Obase基元类型");
        }

        /// <summary>
        ///     Db类型转换
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="tValueType">要转换的目标的类型</param>
        /// <returns></returns>
        public static object ConvertDbValue(object value, Type tValueType)
        {
            //常用的DbType转换
            if (tValueType == typeof(float) && value is double) value = Convert.ToSingle(value);
            if (tValueType == typeof(short) && value is ushort) value = Convert.ToInt16(value);
            if (tValueType == typeof(int) && value is uint) value = Convert.ToInt32(value);
            if (tValueType == typeof(long) && value is ulong) value = Convert.ToInt64(value);
            if (tValueType == typeof(bool) && value is sbyte) value = Convert.ToBoolean(value);
            if (tValueType == typeof(double) && value is decimal) value = Convert.ToDouble(value);
            if (tValueType == typeof(TimeSpan) && value is string) value = TimeSpan.Parse(value.ToString());
            if (tValueType == typeof(Guid) && value is string) value = Guid.Parse(value.ToString());
            if (tValueType == typeof(byte) && value is string) value = Convert.ToByte(value.ToString().Trim());
            if (tValueType == typeof(char) && value is string) value = Convert.ToChar(value.ToString().Trim());

            //转换枚举
            if (tValueType.IsEnum) value = Enum.Parse(tValueType, value.ToString());

            //都没有 对于非类 接口 加入兜底的ChangeType
            if (!(tValueType.IsClass || tValueType.IsInterface) && tValueType != value.GetType())
                value = Convert.ChangeType(value, tValueType);
            return value;
        }

        /// <summary>
        ///     获取衍生的目标表
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns></returns>
        public static string GetDerivedTargetTable(ObjectType objectType)
        {
            //如果没有继承 则直接返回当前的目标表
            var result = objectType.TargetTable;
            var currentObjectType = (ObjectType)objectType.DerivingFrom;
            //如果有继承 则一直向上找
            while (currentObjectType != null)
            {
                result = currentObjectType.TargetTable;
                currentObjectType = (ObjectType)currentObjectType.DerivingFrom;
            }

            return result;
        }

        /// <summary>
        ///     获取衍生的构造函数
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns></returns>
        public static IInstanceConstructor GetDerivedIInstanceConstructor(ObjectType objectType)
        {
            //如果没有继承 则直接返回当前的构造函数
            var result = objectType.Constructor;
            var currentObjectType = (ObjectType)objectType.DerivingFrom;
            //如果有继承 则一直向上找
            while (currentObjectType != null)
            {
                result = currentObjectType.Constructor;
                currentObjectType = (ObjectType)currentObjectType.DerivingFrom;
            }

            return result;
        }


        /// <summary>
        ///     是否存在符合标识推断的属性
        /// </summary>
        /// <param name="type">要推断的类型</param>
        /// <param name="propertyInfos">符合的属性</param>
        /// <returns></returns>
        public static bool ExistIdentity(Type type, out List<PropertyInfo> propertyInfos)
        {
            //默认的推断属性名称 Code ID 类名+Code 类名+ID
            var keyAttrName = new List<string>
                { "code", "id", $"{type.Name.ToLower()}code", $"{type.Name.ToLower()}id" };
            //是否为以上四种名称 且 是int long 和 string
            var result = type.GetProperties()
                .Where(property => keyAttrName.Contains(property.Name.ToLower())
                                   && (property.PropertyType == typeof(short) ||
                                       property.PropertyType == typeof(int) || property.PropertyType == typeof(long)
                                       || property.PropertyType == typeof(string))).ToList();
            propertyInfos = result;

            return result.Any();
        }

        /// <summary>
        ///     获取属性是否为多重性
        /// </summary>
        /// <param name="propInfo">属性</param>
        /// <param name="argType">如果是多重性的,为propInfo.PropertyType.GenericTypeArguments[0]否则为propInfo.PropertyType</param>
        /// <returns></returns>
        public static bool GetIsMultipe(PropertyInfo propInfo, out Type argType)
        {
            //关联重数（表示是否是集合属性）
            var isMultiplicity = false;
            //属性为集合类型
            var type = propInfo.PropertyType.GetInterface("IEnumerable");
            if (type != null && propInfo.PropertyType != typeof(string))
            {
                //集合元素类型
                argType = propInfo.PropertyType.IsArray
                    ? propInfo.PropertyType.GetElementType()
                    : propInfo.PropertyType.GenericTypeArguments[0];

                isMultiplicity = true;
            }
            else
            {
                //单值 直接取
                argType = propInfo.PropertyType;
            }

            return isMultiplicity;
        }

        /// <summary>
        ///     是否是元组
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static bool IsTuple(Type type)
        {
            var typeArgs = type.GetGenericArguments();
            //判断是否是元组
            switch (typeArgs.Length)
            {
                case 1:
                    return typeof(Tuple<>).MakeGenericType(typeArgs) == type;
                case 2:
                    return typeof(Tuple<,>).MakeGenericType(typeArgs) == type;
                case 3:
                    return typeof(Tuple<,,>).MakeGenericType(typeArgs) == type;
                case 4:
                    return typeof(Tuple<,,,>).MakeGenericType(typeArgs) == type;
                case 5:
                    return typeof(Tuple<,,,,>).MakeGenericType(typeArgs) == type;
                case 6:
                    return typeof(Tuple<,,,,,>).MakeGenericType(typeArgs) == type;
                case 7:
                    return typeof(Tuple<,,,,,,,>).MakeGenericType(typeArgs) == type;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     初始化模块构造器
        /// </summary>
        /// <returns></returns>
        public static ModuleBuilder InitModuleBuilder()
        {
            //在当前应用程序域动态创建模块存放生成的代理类
            var assemblyName = new AssemblyName
                { Name = "ObaseProxyModule", Version = typeof(ImpliedTypeManager).Assembly.GetName().Version };
            //程序集合构建器
            var assemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            return assemblyBuilder.DefineDynamicModule("ObaseProxyModule");
        }

        /// <summary>
        ///     获取需要定义的外键属性
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <param name="returnEnd">外键关联端</param>
        /// <param name="returnKey">返回的外键</param>
        /// <returns></returns>
        public static List<Attribute> GetDefinedForeignAttributes(ObjectType objType, AssociationEnd returnEnd,
            out List<Attribute> returnKey)
        {
            //关联端集合
            var associationEnds = new List<AssociationEnd>();
            if (objType is AssociationType associationType)
            {
                if (associationType.Visible || associationType.Independent)
                    associationEnds.AddRange(associationType.AssociationEnds);
            }
            else if (objType is EntityType entityType)
            {
                //关联引用
                var assoRef = entityType.AssociationReferences;
                foreach (var associationReference in assoRef)
                {
                    var assoType = associationReference.AssociationType;
                    if (!assoType.Visible && !assoType.Independent)
                        if (assoType.IsCompanionEnd(associationReference.LeftEnd))
                            associationEnds.AddRange(assoType.AssociationEnds
                                .Where(p => p.Name != associationReference.LeftEnd).ToList());
                }
            }

            returnKey = new List<Attribute>();
            var attrs = new List<Attribute>();
            var i = 1;
            //取到的关联端
            foreach (var end in associationEnds)
                //映射
            foreach (var mapping in end.Mappings)
            {
                var attr = objType.FindAttributeByTargetField(mapping.TargetField);
                //找到目标属性
                if (attr == null)
                {
                    //键的属性
                    var keyAttr = end.EntityType.GetAttribute(mapping.KeyAttribute);
                    //当前的属性们
                    var attrsArray = attrs.Cast<TypeElement>().ToArray();
                    var name = objType.NameNew($"obase_gen_fk_{i}", attrsArray);
                    i++;
                    //构造一个新属性
                    var newAttr = new Attribute(keyAttr.DataType, name)
                        { TargetField = mapping.TargetField, IsForeignKeyDefineMissing = true };
                    attrs.Add(newAttr);

                    //与外键关联端相等
                    if (end == returnEnd) returnKey.Add(newAttr);
                }
                else
                {
                    //与外键关联端相等
                    if (end == returnEnd) returnKey.Add(attr);
                }
            }

            return attrs;
        }

        /// <summary>
        ///     获取继承的顶级父类的类型区别标记
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns></returns>
        public static Tuple<string, object> GetDerivedConcreteTypeSign(ObjectType objectType)
        {
            var derviving = objectType.DerivingFrom;
            //没有继承别人 但被人继承的 返回标记
            if (derviving == null && objectType.DerivedTypes.Count > 0)
                //如果因为没有配置此处出现误判 会在后续检查中处理
                if (objectType.ConcreteTypeSign != null &&
                    objectType.FindAttributeByTargetField(objectType.ConcreteTypeSign.Item1) == null)
                    return objectType.ConcreteTypeSign;
            //有继承 要一直向上找
            if (derviving != null)
            {
                var current = objectType;
                while (derviving != null)
                {
                    current = (ObjectType)derviving;
                    derviving = derviving.DerivingFrom;
                }

                //如果因为没有配置此处出现误判 会在后续检查中处理
                if (current.ConcreteTypeSign != null &&
                    current.FindAttributeByTargetField(current.ConcreteTypeSign.Item1) == null)
                    return objectType.ConcreteTypeSign;
            }

            return null;
        }

        /// <summary>
        ///     获取继承链
        /// </summary>
        /// <param name="targetType">目标结构化类型</param>
        /// <returns></returns>
        public static List<StructuralType> GetDerivingChain(StructuralType targetType)
        {
            //一路找到顶级
            var current = targetType;
            var deriving = targetType.DerivingFrom;
            //组合成继承链
            var derivingList = new List<StructuralType> { current };
            while (deriving != null)
            {
                current = deriving;
                derivingList.Add(current);
                deriving = current.DerivingFrom;
            }

            //反序后才是继承链 沿着继承链处理每一个
            derivingList.Reverse();
            return derivingList;
        }

        /// <summary>
        ///     获取配置的继承链
        /// </summary>
        /// <param name="targetTypeConfiguration">目标结构化配置类型</param>
        /// <param name="modelBuilder">建模器</param>
        /// <returns></returns>
        public static List<StructuralTypeConfiguration> GetDerivingConfigChain(
            StructuralTypeConfiguration targetTypeConfiguration, ModelBuilder modelBuilder)
        {
            //一路找到顶级
            var current = targetTypeConfiguration;
            var deriving = targetTypeConfiguration.DerivedFrom;
            //组合成继承链
            var derivingList = new List<StructuralTypeConfiguration> { current };
            while (deriving != null)
            {
                current = modelBuilder.FindConfiguration(deriving);
                derivingList.Add(current);
                deriving = current.DerivedFrom;
            }

            //反序后才是继承链 沿着继承链处理每一个
            derivingList.Reverse();
            return derivingList;
        }
    }
}