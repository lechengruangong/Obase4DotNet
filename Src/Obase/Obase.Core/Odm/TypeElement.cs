/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型元素,为各种类型元素（属性、关联引用、关联端）提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:17:50
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     为各种类型元素（属性、关联引用、关联端）提供基础实现。
    /// </summary>
    public abstract class TypeElement
    {
        /// <summary>
        ///     元素的类型。
        /// </summary>
        private readonly EElementType _elementType;

        /// <summary>
        ///     元素扩展。
        /// </summary>
        private readonly List<ElementExtension> _extensions = new List<ElementExtension>();

        /// <summary>
        ///     名称
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     元素宿主对象的类型。
        /// </summary>
        private StructuralType _hostType;

        /// <summary>
        ///     指示元素是否具有多重性，即其值是否为集合类型。
        /// </summary>
        private bool _isMultiple;

        /// <summary>
        ///     取值器
        /// </summary>
        private IValueGetter _valueGetter;

        /// <summary>
        ///     设置器
        /// </summary>
        private IValueSetter _valueSetter;

        /// <summary>
        ///     创建TypeElement实例。
        /// </summary>
        /// <param name="name">元素的名称</param>
        /// <param name="elementType">元素的类型</param>
        protected TypeElement(string name, EElementType elementType)
        {
            _name = name;
            _elementType = elementType;
        }


        /// <summary>
        ///     获取或设置取值器。
        /// </summary>
        public IValueGetter ValueGetter
        {
            get => _valueGetter;
            set => _valueGetter = value;
        }

        /// <summary>
        ///     获取或设置设值器。
        /// </summary>
        public IValueSetter ValueSetter
        {
            get => _valueSetter;
            set => _valueSetter = value;
        }

        /// <summary>
        ///     获取元素（属性、关联引用、关联端）的名称。
        /// </summary>
        public string Name => _name;


        /// <summary>
        ///     获取或设置元素宿主对象的类型。
        /// </summary>
        public StructuralType HostType
        {
            get => _hostType;
            internal set => _hostType = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示元素是否具有多重性，即其值是否为集合类型。
        /// </summary>
        public bool IsMultiple
        {
            get => _isMultiple;
            set => _isMultiple = value;
        }

        /// <summary>
        ///     获取元素值的类型。
        /// </summary>
        public abstract TypeBase ValueType { get; }

        /// <summary>
        ///     元素的类型。
        /// </summary>
        public EElementType ElementType => _elementType;

        /// <summary>
        ///     为当前元素添加扩展。
        /// </summary>
        /// <param name="extension">要添加的元素扩展。</param>
        public void AddExtension(ElementExtension extension)
        {
            _extensions.Add(extension);
        }

        /// <summary>
        ///     为当前元素添加扩展。
        /// </summary>
        /// <returns>新创建的元素扩展实例。</returns>
        /// <param name="extensionType">扩展类型，它是一个继承自ElementExtension的类型。</param>
        public ElementExtension AddExtension(Type extensionType)
        {
            if (!typeof(TypeExtension).IsAssignableFrom(extensionType))
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}不是ElementExtension类型", nameof(extensionType));
            try
            {
                //使用反射创建扩展实例
                var extension = (ElementExtension)Activator.CreateInstance(extensionType);
                _extensions.Add(extension);
                return extension;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}没有适合的无参构造函数", nameof(extensionType), e);
            }
        }

        /// <summary>
        ///     为当前元素添加扩展。
        /// </summary>
        /// <returns>新创建的元素扩展实例。</returns>
        public TExtension AddExtension<TExtension>() where TExtension : ElementExtension
        {
            var extensionType = typeof(TExtension);
            if (!typeof(TypeExtension).IsAssignableFrom(extensionType))
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}不是ElementExtension类型", nameof(extensionType));
            try
            {
                //使用反射创建扩展实例
                var extension = (ElementExtension)Activator.CreateInstance(extensionType);
                _extensions.Add(extension);
                return (TExtension)extension;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展失败,{extensionType.Name}没有适合的无参构造函数", nameof(extensionType), e);
            }
        }

        /// <summary>
        ///     为指定对象的当前元素设置值，适用于具有多重性的元素。
        /// </summary>
        /// <param name="targetObj">要为其元素设值的对象。</param>
        /// <param name="value">元素的值。</param>
        public void SetValue(object targetObj, IEnumerable value)
        {
            if (targetObj is IIntervene inter1)
                //禁用延迟加载（防止延迟加载期间内部访问属性又开始加载，造成死循环）
                inter1.ForbidLazyLoading();

            var settinMode = _valueSetter.Mode;
            switch (settinMode)
            {
                case EValueSettingMode.Assignment:
                    _valueSetter.SetValue(targetObj, value);
                    break;
                case EValueSettingMode.Appending:
                    if (value == null) return;
                    foreach (var valueItem in value) _valueSetter.SetValue(targetObj, valueItem);
                    break;
            }

            if (targetObj is IIntervene inter2)
                //启用延迟加载
                inter2.EnableLazyLoading();
        }

        /// <summary>
        ///     为指定对象的当前元素设置值，适用于不具多重性的元素。
        /// </summary>
        /// <param name="targetObj">要为其元素设值的对象。</param>
        /// <param name="value">元素的值。</param>
        public void SetValue(object targetObj, object value)
        {
            if (targetObj is IIntervene inter1)
                //禁用延迟加载（防止延迟加载期间内部访问属性又开始加载，造成死循环）
                inter1.ForbidLazyLoading();

            //前置过滤，如果value实现了IEnumerable或IEnumerable<>，调用另一重载。
            var valueType = value.GetType();
            if (valueType != typeof(string) && valueType.GetInterface("IEnumerable") != null)
            {
                var iEnumerableValue = (IEnumerable)value;
                SetValue(targetObj, iEnumerableValue);
            }
            else
            {
                _valueSetter.SetValue(targetObj, value);
            }

            if (targetObj is IIntervene inter2)
                //启用延迟加载
                inter2.EnableLazyLoading();
        }

        /// <summary>
        ///     从指定对象取出当前元素的值。
        /// </summary>
        /// <returns>如果元素具有多重性，返回IEnumerable{T}，否则返回object</returns>
        /// <param name="targetObj">要取其元素值的对象。</param>
        public object GetValue(object targetObj)
        {
            //实施说明  如果是引用元素，调用取值器前要记下是否已启用延迟加载，然后禁用延迟加载，调用后恢复到原始状态。
            object result;
            if (this is ReferenceElement re && targetObj is IIntervene inter)
            {
                //禁用延迟加载（防止延迟加载期间内部访问属性又开始加载，造成死循环）
                inter.ForbidLazyLoading();
                //获取值
                result = re.ValueGetter.GetValue(targetObj);
                //启用延迟加载
                inter.EnableLazyLoading();
            }
            else
            {
                //获取值
                result = ValueGetter.GetValue(targetObj);
            }

            return result;
        }

        /// <summary>
        ///     获取元素扩展。
        /// </summary>
        /// <returns>返回元素扩展实例；如果指定的扩展类型不存在，返回null。</returns>
        /// <param name="extensionType">扩展类型，即派生自ElementExtension的具体类型。</param>
        public ElementExtension GetExtension(Type extensionType)
        {
            return _extensions.FirstOrDefault(p => p.GetType() == extensionType);
        }

        /// <summary>
        ///     获取元素扩展。
        /// </summary>
        /// <returns>返回元素扩展实例；如果指定的扩展类型不存在，返回null。</returns>
        public TExtension GetExtension<TExtension>() where TExtension : ElementExtension
        {
            var extensionType = typeof(TypeExtension);
            return (TExtension)_extensions.FirstOrDefault(p => p.GetType() == extensionType);
        }
    }
}