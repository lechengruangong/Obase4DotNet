/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据模型,此模型全局应只有一个.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 12:14:51
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示对象数据模型。
    /// </summary>
    public class ObjectDataModel
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     clr类型与代理类型字典
        /// </summary>
        private readonly Dictionary<Type, Type> _proxyReal = new Dictionary<Type, Type>();

        /// <summary>
        ///     clr类型与模型字典
        /// </summary>
        private readonly Dictionary<Type, StructuralType> _structuralTypes = new Dictionary<Type, StructuralType>();

        /// <summary>
        ///     模型的存储标记。
        /// </summary>
        private StorageSymbol _storageSymbol;

        /// <summary>
        ///     模型中的所有类型
        /// </summary>
        private List<StructuralType> _types;

        /// <summary>
        ///     clr类型与模型字典
        /// </summary>
        private Dictionary<Type, StructuralType> StructuralTypes => _structuralTypes;

        /// <summary>
        ///     获取模型中的所有类型。
        /// </summary>
        public List<StructuralType> Types => _types ?? (_types = new List<StructuralType>());

        /// <summary>
        ///     获取或设置存储标记。
        ///     说明
        ///     只能设置一次存储标记，对已设置的存储标记进行修改将引发异常。
        /// </summary>
        /// <returns>模型默认的存储标记。</returns>
        /// <exception cref="Exception">存储标记一经设置便不能修改。</exception>
        public StorageSymbol StorageSymbol
        {
            get => _storageSymbol;
            set
            {
                if (_storageSymbol != null) throw new InvalidOperationException("存储标记一经设置便不能修改");
                _storageSymbol = value;
            }
        }

        /// <summary>
        ///     根据指定的CLR类型获取实体型。
        /// </summary>
        /// <param name="type">CLR类型</param>
        public EntityType GetEntityType(Type type)
        {
            return GetTypeOrNull(type) as EntityType;
        }

        /// <summary>
        ///     根据指定的CLR类型获取关联型。
        /// </summary>
        /// <param name="type">CLR类型</param>
        public AssociationType GetAssociationType(Type type)
        {
            return GetTypeOrNull(type) as AssociationType;
        }

        /// <summary>
        ///     根据指定的CLR类型获取复杂类型。
        /// </summary>
        /// <param name="type">CLR类型</param>
        public ComplexType GetComplexType(Type type)
        {
            return GetTypeOrNull(type) as ComplexType;
        }

        /// <summary>
        ///     根据指定的CLR类型在模型中搜索对象类型。
        /// </summary>
        /// <param name="type">CLR类型</param>
        public ObjectType GetObjectType(Type type)
        {
            return GetTypeOrNull(type) as ObjectType;
        }

        /// <summary>
        ///     根据指定的CLR类型在模型中搜索相应的类型。
        /// </summary>
        /// <param name="type">CLR类型</param>
        public StructuralType GetStructuralType(Type type)
        {
            return GetTypeOrNull(type) as StructuralType;
        }

        /// <summary>
        ///     根据指定的类型名称及其所在命名空间在模型中搜索相应的类型。
        /// </summary>
        /// <param name="nameSpace">命名空间。</param>
        /// <param name="name">类型名称。</param>
        public StructuralType GetStructuralType(string nameSpace, string name)
        {
            //构造类型名
            var typeName = new TypeName { Name = name, Namespace = nameSpace };
            //在模型中查找
            foreach (var structuralType in StructuralTypes)
                if (structuralType.Value.TypeName == typeName)
                    return structuralType.Value;
            return null;
        }

        /// <summary>
        ///     向模型添加类型
        /// </summary>
        /// <param name="modelType">要添加到模型中的类型（实体型、关联型、复杂类型）</param>
        public void AddType(StructuralType modelType)
        {
            ReaderWriterLock.EnterWriteLock();
            //覆盖原有的类型
            StructuralTypes[modelType.ClrType] = modelType;
            if (!Types.Contains(modelType))
                Types.Add(modelType);
            //如果有代理类型，则将代理类型与实际类型映射
            if (modelType.ProxyType != null)
                _proxyReal[modelType.ProxyType] = modelType.ClrType;
            //指定结构类型所属的模型
            modelType.SetModel(this);
            ReaderWriterLock.ExitWriteLock();
        }

        /// <summary>
        ///     获取指定CLR类型的模型类型。
        /// </summary>
        /// <exception cref="UnknownTypeException">不能识别的类型</exception>
        /// <param name="type">类型</param>
        /// 注：
        /// （1）如果不存在相应的类型（既不为预定义的基元类型，又未在模型中注册为结构化类型）则引发UnknownTypeException；
        /// <returns></returns>
        public TypeBase GetType(Type type)
        {
            var result = GetTypeOrNull(type);
            if (result == null) throw new UnknownTypeException(type);
            return result;
        }

        /// <summary>
        ///     获取指定数据类型的模型类型
        /// </summary>
        /// <exception cref="UnknownTypeException">不能识别的类型。</exception>
        /// 注：
        /// （1）如果不存在相应的类型（既不为预定义的基元类型，又未在模型中注册为结构化类型）则引发UnknownTypeException；
        /// （2）如果指定的类型实现了IEnumerable，则依据元素的类型查找
        /// <param name="type">类型</param>
        /// <param name="isEnumerable">是否为可数的</param>
        public TypeBase GetType(Type type, out bool isEnumerable)
        {
            var result = GetTypeOrNull(type, out isEnumerable);
            if (result == null) throw new UnknownTypeException(type);
            return result;
        }

        /// <summary>
        ///     根据指定的CLR类型在模型中搜索可发出引用类型。
        /// </summary>
        /// <param name="type">CLR类型</param>
        /// <returns></returns>
        public ReferringType GetReferringType(Type type)
        {
            return GetTypeOrNull(type) as ReferringType;
        }

        /// <summary>
        ///     根据指定的CLR类型在模型中搜索可发出引用类型。
        /// </summary>
        /// <param name="type">CLR类型</param>
        /// <returns></returns>
        public TypeView GetTypeView(Type type)
        {
            var structType = GetStructuralType(type);
            if (structType == null) throw new Exception($"CLR类型对应的{type?.FullName}在模型中不存在");
            return (TypeView)structType;
        }

        /// <summary>
        ///     获取指定CLR类型的模型类型。
        ///     注：
        ///     （1）如果不存在相应的类型（既不为预定义的基元类型，又未在模型中注册为结构化类型）则返回null。
        /// </summary>
        /// <param name="type">程序语言中的类型。</param>
        public TypeBase GetTypeOrNull(Type type)
        {
            //从代理类型里取
            if (_proxyReal.TryGetValue(type, out var realType)) type = realType;
            //取出clr类型对应模型
            StructuralTypes.TryGetValue(type, out var result);
            //是否为系统基元类型
            if (PrimitiveType.IsObasePrimitiveType(type))
                return PrimitiveType.FromType(type);
            return result;
        }

        /// <summary>
        ///     获取指定数据类型的模型类型。
        ///     注：
        ///     （1）如果不存在相应的类型（既不为预定义的基元类型，又未在模型中注册为结构化类型）则返回null；
        ///     （2）如果指定的类型实现了IEnumerable，则依据元素的类型查找。
        /// </summary>
        /// <param name="type">程序语言中的类型。</param>
        /// <param name="isEnumerable">返回一个值，该值指示类型是否为可枚举的。</param>
        public TypeBase GetTypeOrNull(Type type, out bool isEnumerable)
        {
            //实现IEnumerable 并且不是string
            isEnumerable = type != typeof(string) && type.GetInterface("IEnumerable") != null;
            if (isEnumerable) type = type.GetGenericArguments()[0];
            var resultType = GetTypeOrNull(type);
            return resultType;
        }

        /// <summary>
        ///     检测模型中是否存在指定的类型。
        /// </summary>
        /// <returns>如果存在返回true，否则返回false。</returns>
        /// <param name="type">程序语言中的类型。</param>
        public bool Exist(Type type)
        {
            return _structuralTypes.ContainsKey(type);
        }

        /// <summary>
        ///     创建代理类型映射，即为模型类型的CLR类型指定一个代理类型。
        /// </summary>
        /// <param name="type">实际类型。</param>
        /// <param name="proxyType">代理类型。</param>
        internal void CreateProxyMapping(Type type, Type proxyType)
        {
            ReaderWriterLock.EnterWriteLock();
            //要移除的代理类型
            var removedProxy = _proxyReal.FirstOrDefault(q => q.Value == type).Key;
            if (removedProxy != null)
                _proxyReal.Remove(removedProxy);
            //添加新的代理类型映射
            _proxyReal.Add(proxyType, type);
            ReaderWriterLock.ExitWriteLock();
        }
    }
}