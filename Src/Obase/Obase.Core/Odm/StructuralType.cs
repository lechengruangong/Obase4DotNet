/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构化类型,为实体类、关联型和复杂类型提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:45:06
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Obase.Core.Common;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     为实体类、关联型和复杂类型提供基础实现。
    /// </summary>
    public abstract class StructuralType : TypeBase
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     当前类型的继承类型
        /// </summary>
        private readonly List<StructuralType> _derivedTypes = new List<StructuralType>();

        /// <summary>
        ///     当前类型的基类型。
        /// </summary>
        private readonly StructuralType _derivingFrom;

        /// <summary>
        ///     键为元素名值为元素（属性、引用元素（关联端、关联引用））
        /// </summary>
        protected readonly Dictionary<string, TypeElement> _elements = new Dictionary<string, TypeElement>();

        /// <summary>
        ///     类型扩展
        /// </summary>
        private readonly List<TypeExtension> _extensions = new List<TypeExtension>();

        /// <summary>
        ///     以类型各属性为根节点生长而成的属性树。
        /// </summary>
        private Dictionary<string, AttributeTree> _attibuteTrees;

        /// <summary>
        ///     基类型的构造器
        /// </summary>
        protected IInstanceConstructor _baseTypeConstructor;

        /// <summary>
        ///     具体类型判别标志
        /// </summary>
        private Tuple<string, object> _concreteTypeSign;


        /// <summary>
        ///     构造器
        /// </summary>
        protected IInstanceConstructor _constructor;

        /// <summary>
        ///     对象数据模型
        /// </summary>
        private ObjectDataModel _model;

        /// <summary>
        ///     新实例构造函数
        /// </summary>
        private IInstanceConstructor _newInstanceConstructor;


        /// <summary>
        ///     代理类型，如果未生成代理类则为null
        /// </summary>
        private Type _proxyType;

        /// <summary>
        ///     根据指定的CLR类型创建类型实例。
        /// </summary>
        /// <param name="clrType">CLR类型</param>
        /// <param name="derivingFrom">基类型</param>
        protected StructuralType(Type clrType, StructuralType derivingFrom = null) : base(clrType)
        {
            _derivingFrom = derivingFrom;
        }

        /// <summary>
        ///     创建类型实例，该实例还没有关联的对象系统类型，有待后续指定。
        /// </summary>
        protected StructuralType()
        {
        }

        /// <summary>
        ///     获取或设置该类型对象的构造器。
        /// </summary>
        public IInstanceConstructor Constructor
        {
            get => _constructor;
            set
            {
                _constructor = value;
                _constructor.InstanceType = this;
            }
        }

        /// <summary>
        ///     基类型的构造器
        /// </summary>
        public IInstanceConstructor BaseTypeConstructor => _baseTypeConstructor;

        /// <summary>
        ///     获取类型包含的属性<font color="#ff0000">（包含继承自基类的）</font>的集合。
        ///     <font color="#ff0000">实施说明</font>
        ///     <font color="#ff0000">
        ///     </font>
        ///     <font color="#ff0000">忽略基类的同名属性。</font>
        /// </summary>
        public List<Attribute> Attributes => Elements.OfType<Attribute>().ToList();

        /// <summary>
        ///     获取类型包含的所有元素<font color="#ff0000">（包含继承自基类的）</font>。
        ///     <font color="#ff0000">实施说明</font>
        ///     <font color="#ff0000">
        ///     </font>
        ///     <font color="#ff0000">忽略基类的同名元素。</font>
        /// </summary>
        public IReadOnlyCollection<TypeElement> Elements
        {
            get
            {
                //获取继承链
                var derivingList = Utils.GetDerivingChain(this);
                //用字典存储元素 同名的子级覆盖
                var result = new Dictionary<string, TypeElement>();
                //处理继承链上的每个类型
                foreach (var derivingType in derivingList)
                    //加入当前类型的元素
                foreach (var element in derivingType._elements.Values)
                    result[element.Name] = element;
                return result.Values.ToList();
            }
        }

        /// <summary>
        ///     获取或设置类型对应的对象系统类型的代理类型
        ///     如果类型已放入模型，要模型中创建一条代理映射。
        /// </summary>
        public Type ProxyType
        {
            get => _proxyType;
            internal set
            {
                //创建代理映射
                _model.CreateProxyMapping(_clrType, value);
                _proxyType = value;
            }
        }

        /// <summary>
        ///     获取结构化类型所属的对象数据模型。
        /// </summary>
        public ObjectDataModel Model => _model;

        /// <summary>
        ///     获取重建对象时实际使用的程序语言类型。如果已生成代理类型，则使用代理类型，否则使用原始类型。
        /// </summary>
        public Type RebuildingType => _proxyType ?? _clrType;

        /// <summary>
        ///     当前类型的基类型。
        /// </summary>
        public StructuralType DerivingFrom
        {
            get
            {
                //为当前类型的基类注册继承类
                if (_derivingFrom != null) _derivingFrom.RegisterDerivedType(this);

                //返回基类
                return _derivingFrom;
            }
        }

        /// <summary>
        ///     继承类的集合
        /// </summary>
        public List<StructuralType> DerivedTypes => _derivedTypes;

        /// <summary>
        ///     新实例构造函数
        /// </summary>
        public IInstanceConstructor NewInstanceConstructor
        {
            get => _newInstanceConstructor;
            internal set => _newInstanceConstructor = value;
        }

        /// <summary>
        ///     具体类型判别标志
        /// </summary>
        public Tuple<string, object> ConcreteTypeSign
        {
            get => _concreteTypeSign;
            internal set => _concreteTypeSign = value;
        }

        /// <summary>
        ///     向类型（实体型、关联型、复杂类型）添加属性。
        /// </summary>
        /// <param name="attribute">要添加的属性</param>
        public void AddAttribute(Attribute attribute)
        {
            AddElement(attribute);
        }

        /// <summary>
        ///     向类型（实体型、关联型、复杂类型）添加元素（属性、关联引用或关联端）
        /// </summary>
        /// <param name="element">要添加的元素</param>
        public virtual void AddElement(TypeElement element)
        {
            //在读写锁中添加元素
            ReaderWriterLock.EnterWriteLock();
            element.HostType = this;
            _elements[element.Name] = element;
            ReaderWriterLock.ExitWriteLock();
        }

        /// <summary>
        ///     设置结构化类型所属的对象数据模型。
        /// </summary>
        /// <param name="model">类型所属的模型。</param>
        internal void SetModel(ObjectDataModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     为当前类型添加扩展。
        /// </summary>
        /// <param name="extension">要添加的类型扩展。</param>
        public void AddExtension(TypeExtension extension)
        {
            extension.SetExtendedType(this);
            _extensions.Add(extension);
        }

        /// <summary>
        ///     为当前类型添加扩展。
        /// </summary>
        /// <returns>新创建的类型扩展实例。</returns>
        /// <param name="extensionType">扩展类型，它是一个继承自TypeExtension的类型。</param>
        public TypeExtension AddExtension(Type extensionType)
        {
            if (!typeof(TypeExtension).IsAssignableFrom(extensionType))
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}不是TypeExtension类型", nameof(extensionType));
            try
            {
                //反射创建扩展实例
                var extension = (TypeExtension)Activator.CreateInstance(extensionType);
                extension.SetExtendedType(this);
                _extensions.Add(extension);
                return extension;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}没有适合的无参构造函数", nameof(extensionType), e);
            }
        }

        /// <summary>
        ///     为当前类型添加扩展。
        ///     类型扩展（TypeExtension的派生类）须定义无参构造函数。
        /// </summary>
        /// <returns>新创建的类型扩展实例。</returns>
        public TExtension AddExtension<TExtension>() where TExtension : TypeExtension
        {
            var extensionType = typeof(TExtension);
            if (!typeof(TypeExtension).IsAssignableFrom(extensionType))
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}不是TypeExtension类型", nameof(extensionType));
            try
            {
                //使用反射创建扩展实例
                var extension = (TypeExtension)Activator.CreateInstance(extensionType);
                extension.SetExtendedType(this);
                _extensions.Add(extension);
                return (TExtension)extension;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}没有适合的无参构造函数", nameof(extensionType), e);
            }
        }

        /// <summary>
        ///     枚举以各属性<font color="#ff0000">（包含继承自基类的）</font>为根生成的属性树。
        ///     <font color="#ff0000">实施说明</font>
        ///     <font color="#ff0000">
        ///     </font>
        ///     <font color="#ff0000">忽略基类的同名属性。</font>
        /// </summary>
        public IEnumerable<AttributeTree> EnumerateAttributeTree()
        {
            ReaderWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (_attibuteTrees == null)
                {
                    ReaderWriterLock.EnterWriteLock();
                    try
                    {
                        //属性
                        var attrs = Attributes;
                        //生长器
                        var grower = new AttributeTreeGrower();

                        _attibuteTrees = new Dictionary<string, AttributeTree>();

                        foreach (var attribute in attrs)
                        {
                            var attrTree = new AttributeTree(attribute);
                            attrTree.Accept(grower);
                            _attibuteTrees.Add(attribute.Name, attrTree);
                        }
                    }
                    finally
                    {
                        ReaderWriterLock.ExitWriteLock();
                    }
                }

                return _attibuteTrees.Values;
            }
            finally
            {
                ReaderWriterLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        ///     获取以指定属性为根生成的属性树。
        ///     <font color="#ff0000">实施说明</font>
        ///     <font color="#ff0000">
        ///     </font>
        ///     <font color="#ff0000">首先检查当前类型，未找到则从基类型查找。</font>
        /// </summary>
        /// <param name="attrName">属性名称。</param>
        public AttributeTree GetAttributeTree(string attrName)
        {
            return EnumerateAttributeTree().FirstOrDefault(tree => tree.AttributeName == attrName);
        }

        /// <summary>
        ///     根据名称查询类型包含的元素（属性、关联引用或关联端）。
        ///     <font color="#ff0000">实施说明</font>
        ///     <font color="#ff0000">
        ///     </font>
        ///     <font color="#ff0000">首先检查当前类型，未找到则从基类型查找。</font>
        /// </summary>
        /// <param name="name">元素名称</param>
        public TypeElement GetElement(string name)
        {
            return Elements.FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        ///     根据名称查询属性。
        ///     <font color="#ff0000">实施说明</font>
        ///     <font color="#ff0000">
        ///     </font>
        ///     <font color="#ff0000">首先检查当前类型，未找到则从基类型查找。</font>
        /// </summary>
        /// <param name="name">属性名称</param>
        public Attribute GetAttribute(string name)
        {
            var result = GetElement(name);
            //只返回属性类型的元素
            if (result is Attribute attribute)
                return attribute;
            return null;
        }

        /// <summary>
        ///     获取类型扩展。
        /// </summary>
        /// <returns>返回类型扩展实例；如果指定的扩展类型不存在，返回null。</returns>
        /// <param name="extensionType">扩展类型，即派生自TypeExtension的具体类型。</param>
        public TypeExtension GetExtension(Type extensionType)
        {
            return _extensions.FirstOrDefault(p => p.GetType() == extensionType);
        }

        /// <summary>
        ///     获取类型扩展。
        /// </summary>
        /// <returns>返回类型扩展实例；如果指定的扩展类型不存在，返回null。</returns>
        public TExtension GetExtension<TExtension>() where TExtension : TypeExtension
        {
            return (TExtension)_extensions.FirstOrDefault(p => p.GetType() == typeof(TExtension));
        }

        /// <summary>
        ///     根据映射字段查找属性，未找到则返回null。
        ///     <font color="#ff0000">实施说明</font>
        ///     <font color="#ff0000">
        ///     </font>
        ///     <font color="#ff0000">首先检查当前类型，未找到则从基类型查找。</font>
        /// </summary>
        /// <param name="field">映射字段</param>
        public Attribute FindAttributeByTargetField(string field)
        {
            foreach (var item in Attributes)
                if (item.TargetField.Equals(field))
                    return item;
            return null;
        }

        /// <summary>
        ///     实例化结构类型，但不初始化实例属性。
        /// </summary>
        /// <param name="argGetter">一个委托，用于为构造参数取值。</param>
        /// <returns></returns>
        public object Instantiate(Func<Parameter, object> argGetter)
        {
            return Instantiate(argGetter, attrNodeValueGetter: null);
        }

        /// <summary>
        ///     实例化结构类型，并初始化实例属性。仅支持构造参数均为属性参数的类型。
        /// </summary>
        /// <param name="attrValueGetter">一个委托，用于为属性树节点代表的简单属性取值。</param>
        /// <returns></returns>
        public object Instantiate(Func<SimpleAttributeNode, object> attrValueGetter)
        {
            //构造取参器
            var argValueGetter = new AttributeValueGetterBasedArgumentGetter(attrValueGetter);

            object ArgGetter(Parameter p)
            {
                return argValueGetter.Get(p);
            }

            return Instantiate(ArgGetter, attrValueGetter);
        }


        /// <summary>
        ///     实例化结构类型，并初始化实例属性。
        /// </summary>
        /// <param name="argGetter">一个委托，用于为构造参数取值。</param>
        /// <param name="attrNodeValueGetter">一个委托，用于为属性树节点代表的简单属性取值。</param>
        /// <returns></returns>
        protected object Instantiate(Func<Parameter, object> argGetter,
            Func<SimpleAttributeNode, object> attrNodeValueGetter)
        {
            //构造Func<Attribute, object> 委托
            //此处用本地方法代替
            object AttrValueGetter(Attribute attribute)
            {
                var attrTree = new AttributeTree(attribute);
                var grower = new AttributeTreeGrower();
                attrTree.Accept(grower);
                var generator = new AttributeValueGenerator(attrNodeValueGetter);
                attrTree.Accept(generator);
                var value = generator.Result;
                return value;
            }

            return Instantiate(argGetter, AttrValueGetter);
        }

        /// <summary>
        ///     实例化结构类型，并初始化实例属性。
        /// </summary>
        /// <param name="argGetter">一个委托，用于为构造参数取值。</param>
        /// <param name="attrValueGetter">属性取值委托，属性须为类型的直接属性。</param>
        protected object Instantiate(Func<Parameter, object> argGetter, Func<Attribute, object> attrValueGetter)
        {
            var paras = _constructor.Parameters;
            //取出所有的值
            var paraValues = paras?.Select(argGetter).ToArray();

            var resultObj = _constructor.Construct(paraValues);
            var isValueType = resultObj.GetType().IsValueType;
            //如果是结构 需要用结构包装器
            if (isValueType) resultObj = new StructWrapper(resultObj);

            var attrs = Attributes;
            //构造函数为基类构造器 需要用类型判别器获取到具体的结构化类型
            if (_constructor is AbstractConstructor abstractConstructor)
                attrs = abstractConstructor.GetDiscriminateType(paraValues).Attributes;
            //为属性设置值
            foreach (var attribute in attrs)
            {
                var attrName = attribute.Name;
                var parameter = _constructor.GetParameterByElement(attrName);
                if (parameter != null)
                    //如果是生成的类型判断参数 则跳过
                    if (parameter.Name != "obase_gen_typeCode")
                        continue;

                var value = attrValueGetter(attribute);
                if (value != null)
                    attribute.SetValue(resultObj, value);
            }

            if (isValueType) resultObj = ((StructWrapper)resultObj).Struct;
            return resultObj;
        }

        /// <summary>
        ///     完整性检查
        ///     继承类需要检查则重写此方法
        /// </summary>
        public abstract void IntegrityCheck();

        /// <summary>
        ///     根据快照重建对象。
        /// </summary>
        /// <param name="snapshot">对象快照</param>
        /// <param name="attachObj">附加委托</param>
        /// <param name="asRoot">是否作为根对象</param>
        /// <returns></returns>
        // 实施说明:
        // 首先，从快照取出引用字典references = snapshot.AllReferences；
        // 然后，创建类型为Dictionary<ObjectKey, Object> 的字典（rebuiltObjs）作为已重建对象的容器；
        // 最后，调用Rebuilt(snapshot, attachObj, asRoot, references, rebuiltObjs)并返回；
        public virtual object Rebuild(ObjectSnapshot snapshot, AttachObject attachObj, bool asRoot)
        {
            var references = snapshot.AllReferences;
            var rebuiltObjs = new Dictionary<ObjectKey, object>();
            return Rebuild(snapshot, attachObj, asRoot, references, rebuiltObjs);
        }

        /// <summary>
        ///     根据快照重建对象。
        /// </summary>
        /// <param name="snapshot">对象快照。</param>
        /// <param name="attachObj">用于将对象附加到对象上下文的委托。</param>
        /// <param name="asRoot">对象是否为根对象。</param>
        /// <param name="references">在重建过程中存储被引用对象的容器，它将沿递归路径逐级传递。</param>
        /// <param name="rebuiltObjs">在重建过程中存储已重建对象的容器，它将沿递归路径逐级传递。</param>
        /// <returns></returns>
        // 实施说明:
        // 参见顺序图“从对象快照重建对象”。
        public object Rebuild(ObjectSnapshot snapshot, AttachObject attachObj, bool asRoot,
            Dictionary<ObjectKey, ObjectSnapshot> references, Dictionary<ObjectKey, object> rebuiltObjs)
        {
            //创建基础对象
            var resultObj = _constructor.Construct();
            // 添加到rebuiltObjs
            if (this is ObjectType)
                rebuiltObjs[snapshot.GetKey()] = resultObj;
            //循环类型元素
            foreach (var element in Elements)
            {
                //获取元素的值
                var eleName = element.Name;
                object eleValue;
                try
                {
                    eleValue = snapshot.GetElement(eleName);
                }
                catch (ElementNotFoundException)
                {
                    continue;
                }

                //为属性
                if (element is Attribute attribute)
                {
                    //复杂属性 获取下一层
                    if (attribute is ComplexAttribute)
                        //重建复杂属性对象
                        eleValue = Rebuild((ObjectSnapshot)eleValue, attachObj, false, references, rebuiltObjs);
                    if (attribute.DataType.IsEnum)
                        eleValue = Enum.Parse(attribute.DataType, eleValue.ToString());
                    element.SetValue(resultObj, eleValue);
                }
                //为引用
                else if (element is ReferenceElement associationReference)
                {
                    //引用的类型
                    var subType = element.ValueType as StructuralType;
                    //引用值为集合类型并且设值模式为“赋值”
                    if (associationReference.IsMultiple &&
                        associationReference.ValueSetter.Mode == EValueSettingMode.Assignment)
                    {
                        var values = new List<object>();
                        if (!(eleValue is List<ObjectKey> eleValueList))
                            continue;
                        //循环引用字典
                        foreach (var eleKey in eleValueList)
                        {
                            //重建引用对象 有就从字典里取 没有再创建
                            var refObj = rebuiltObjs.TryGetValue(eleKey, out var rebuiltObj)
                                ? rebuiltObj
                                : subType.Rebuild(references[eleKey], attachObj, false, references, rebuiltObjs);
                            values.Add(refObj);
                        }

                        element.SetValue(resultObj, values);
                    }
                    else //引用值为非集合类型或引用值为集合类型并且设值模式为“追加”
                    {
                        if (!(eleValue is List<ObjectKey> eleValueList))
                            continue;
                        //循环引用字典
                        foreach (var eleKey in eleValueList)
                        {
                            //重建引用对象 有就从字典里取 没有再创建
                            var refObj = rebuiltObjs.TryGetValue(eleKey, out var rebuiltObj)
                                ? rebuiltObj
                                : subType.Rebuild(references[eleKey], attachObj, false, references, rebuiltObjs);
                            element.SetValue(resultObj, refObj);
                        }
                    }
                }
            }

            if (this is ObjectType) attachObj?.Invoke(ref resultObj, asRoot);
            return resultObj;
        }


        /// <summary>
        ///     为指定对象生成快照。
        /// </summary>
        /// <param name="targetObj">被快照的对象。</param>
        /// <returns></returns>
        // 实施说明
        // 首先，创建类型为Dictionary<ObjectKey, ObjectSnapshot> 的字典（references）作为被引用对象的容器；
        // 然后，调用Snapshot(targetObj, references)；
        // 最后，从references移除当前对象。
        public ObjectSnapshot Snapshot(object targetObj)
        {
            var references = new Dictionary<ObjectKey, ObjectSnapshot>();
            var snapshot = Snapshot(targetObj, references);
            if (this is ObjectType objectType)
                references.Remove(objectType.GetObjectKey(targetObj));
            snapshot.AllReferences = references;
            return snapshot;
        }

        /// <summary>
        ///     为指定对象生成快照。
        /// </summary>
        /// <param name="targetObj">被快照的对象。</param>
        /// <param name="references">在快照过程中存储被引用对象的容器，它将沿递归路径逐级传递。</param>
        /// <returns></returns>
        // 实施说明:
        // 参见顺序图“生成对象快照”。
        private ObjectSnapshot Snapshot(object targetObj, Dictionary<ObjectKey, ObjectSnapshot> references)
        {
            var snapshot = new ObjectSnapshot(this);
            if (this is ObjectType objectType)
            {
                var key = objectType.GetObjectKey(targetObj);
                references.Add(key, snapshot);
            }

            //循环类型元素
            foreach (var element in Elements)
            {
                //获取值
                var eleValue = element.GetValue(targetObj);
                //为属性
                if (element is Attribute attribute)
                {
                    //如果为复合属性 则继续向下一层快照
                    if (element is ComplexAttribute)
                        eleValue = Snapshot(eleValue, references);

                    if (attribute.DataType.IsEnum) //枚举作为整数处理
                        eleValue = (int)eleValue;
                    snapshot.SetAttribute(element.Name, eleValue);
                }

                //为引用
                else
                {
                    if (eleValue != null && element is ReferenceElement referenceElement)
                        if (element.ValueType is ObjectType refType)
                        {
                            if (referenceElement.IsMultiple && eleValue is IEnumerable valueEnumerable)
                            {
                                //循环引用对象
                                foreach (var refObj in valueEnumerable)
                                {
                                    var key = refType.GetObjectKey(refObj);
                                    //为引用建立快照
                                    if (!references.ContainsKey(key))
                                        refType.Snapshot(refObj, references);
                                    //添加到引用
                                    snapshot.AddReference(element.Name, key);
                                }
                            }
                            else
                            {
                                var key = refType.GetObjectKey(eleValue);
                                //为引用建立快照
                                if (!references.ContainsKey(key))
                                    refType.Snapshot(eleValue, references);
                                //添加到引用
                                snapshot.AddReference(element.Name, key);
                            }
                        }
                }
            }

            return snapshot;
        }

        /// <summary>
        ///     为新元素命名。
        ///     实施说明
        ///     默认使用建议名；如果该名称已被占用，在建议名后追加数字“1”，如果仍然被占用，则将追加数字值加1，直到得到一个未被占用的名称。
        ///     注意，实施同名校验时，不仅要检查类型的已有元素，还要检查预定义元素。
        /// </summary>
        /// <param name="proposedName">推荐使用的名称。</param>
        /// <param name="predefined">预定义的元素。</param>
        internal string NameNew(string proposedName, TypeElement[] predefined = null)
        {
            //是否已被占用
            var exits = Elements.Any(elementsValue => elementsValue.Name == proposedName);

            if (predefined != null)
                if (predefined.Any(preElement => preElement.Name == proposedName))
                    exits = true;

            //不重名
            if (!exits)
                return proposedName;
            //尾部附加
            var ext = 0;
            while (exits)
            {
                ext++;
                proposedName = $"proposedName{ext}";
                exits = Elements.Any(elementsValue => elementsValue.Name == proposedName);

                if (predefined != null)
                    if (predefined.Any(preElement => preElement.Name == proposedName))
                        exits = true;
            }

            return proposedName;
        }

        /// <summary>
        ///     注册派生类型
        /// </summary>
        /// <param name="derivedType">派生类型</param>
        private void RegisterDerivedType(StructuralType derivedType)
        {
            //如果配置相应的判别标志值
            if (derivedType.ConcreteTypeSign != null)
                foreach (var derived in _derivedTypes)
                    //检测判别字段是否冲突
                    if (derived.ConcreteTypeSign != null && derived.ConcreteTypeSign.Item2.GetType() !=
                        derivedType.ConcreteTypeSign.Item2.GetType())
                        throw new ArgumentException(
                            $"{derivedType.Name}与{derived.Name}均为{_derivingFrom.Name}的继承类,但判别字段类型不相符.");

            //加入派生类型集合
            if (!_derivedTypes.Contains(derivedType))
                _derivedTypes.Add(derivedType);
        }

        /// <summary>
        ///     设置具体类型判别器。
        ///     说明
        ///     本方法将自动生成一个抽象构造器作为当前类型的构造器。
        ///     如果在此之前已显式设置了构造器（通过Constructor属性），自动将该构造器作为基类实例构造器。
        ///     调用此方法前应将当前类型指定为基类（通过派生类型的DerivingFrom属性），否则无法生成抽象构造器。
        /// </summary>
        /// <param name="discriminator">判别器实例</param>
        /// <param name="typeAttributeName">类型的一个属性,用于指示具体类型</param>
        public void SetConcreteTypeDiscriminator(IConcreteTypeDiscriminator discriminator, string typeAttributeName)
        {
            if (_derivedTypes.Count == 0)
                throw new ArgumentException("只有基类类型可以设置具体类型判别器.");

            //当前的构造器保存至_baseTypeConstructor
            _baseTypeConstructor = _constructor;

            //_constructor改为AbstractConstructor
            var constructor = new AbstractConstructor(_constructor.Parameters, discriminator, typeAttributeName)
            {
                InstanceType = this
            };
            _constructor = constructor;
        }

        /// <summary>
        ///     基于属性取值委托的取参器。
        ///     包含一个简单属性取值委托，通过调用该委托为绑定到属性的构造参数取值。
        /// </summary>
        protected class AttributeValueGetterBasedArgumentGetter : StructuralType
        {
            /// <summary>
            ///     简单属性取值委托
            /// </summary>
            private readonly Func<SimpleAttributeNode, object> _attrValueGetter;

            /// <summary>
            ///     创建AttributeValueGetterBasedArgumentGetter实例。
            /// </summary>
            /// <param name="attrValueGetter">简单属性取值委托。</param>
            public AttributeValueGetterBasedArgumentGetter(Func<SimpleAttributeNode, object> attrValueGetter) : base(
                typeof(SimpleAttributeNode))
            {
                _attrValueGetter = attrValueGetter;
            }

            /// <summary>
            ///     获取指定构造参数的值。
            /// </summary>
            /// <param name="parameter">要取值的构造参数。只能是属性参数。</param>
            public object Get(Parameter parameter)
            {
                var attrName = parameter.ElementName;
                var element = parameter.GetElement();
                //没有对应的元素
                if (element == null)
                {
                    var result =
                        _attrValueGetter.Invoke(
                            new SimpleAttributeNode(new Attribute(parameter.Expression.Type, attrName)
                                { TargetField = attrName }));

                    return parameter.ValueConverter == null ? result : parameter.ValueConverter(result);
                }

                if (element is ReferenceElement) throw new Exception("类型的构造函数不能具有引用型参数。");
                var attributeTree = element.HostType.GetAttributeTree(attrName);
                var generator = new AttributeValueGenerator(_attrValueGetter);
                attributeTree.Accept(generator);
                var value = generator.Result;
                var converter = parameter.ValueConverter;
                return converter == null ? value : converter(value);
            }

            /// <summary>
            ///     完整性检查
            ///     并不需要
            /// </summary>
            public override void IntegrityCheck()
            {
                //Nothing To Do
            }
        }
    }
}