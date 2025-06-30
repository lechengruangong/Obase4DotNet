/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：版本合并上下文.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:00:45
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Saving;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     属性取值委托。用于从指定对象取出指定属性的值。
    /// </summary>
    /// <param name="obj">目标对象。</param>
    /// <param name="attribute">要取其值的属性。</param>
    /// <param name="parent">属性的父属性。</param>
    public delegate object GetAttributeValue(object obj, Attribute attribute, AttributePath parent = null);

    /// <summary>
    ///     版本合并上下文。
    /// </summary>
    public class VersionCombinationContext
    {
        /// <summary>
        ///     获取属性原值的委托。
        /// </summary>
        private readonly GetAttributeValue _attributeOriginalValueGetter;

        /// <summary>
        ///     并发冲突类型。
        /// </summary>
        private readonly EConcurrentConflictType _conflictType;

        /// <summary>
        ///     执行版本合并的对象。
        /// </summary>
        private readonly object _object;

        /// <summary>
        ///     执行版本合并的对象的模型类型。
        /// </summary>
        private readonly ObjectType _objectType;

        /// <summary>
        ///     当前正在执行合并处理的复杂属性
        /// </summary>
        private ComplexAttribute _complexAttribute;

        /// <summary>
        ///     当前正在执行合并处理的复杂属性的对象
        /// </summary>
        private object _complexObject;

        /// <summary>
        ///     当前正在执行合并处理的属性的父属性。
        /// </summary>
        private AttributePath _parentAttribute;

        /// <summary>
        ///     创建VersionCombinationContext实例。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="objType">对象类型</param>
        /// <param name="conflictType">并发冲突类型</param>
        /// <param name="attrOriginalValueGetter">原始值获取器</param>
        public VersionCombinationContext(object obj, ObjectType objType, EConcurrentConflictType conflictType,
            GetAttributeValue attrOriginalValueGetter = null)
        {
            _object = obj;
            _objectType = objType;
            _conflictType = conflictType;
            _attributeOriginalValueGetter = attrOriginalValueGetter;
        }

        /// <summary>
        ///     获取执行版本合并的对象。
        /// </summary>
        public object Object => _object;

        /// <summary>
        ///     获取执行版本合并的对象的模型类型。
        /// </summary>
        public ObjectType ObjectType => _objectType;

        /// <summary>
        ///     获取或设置当前正在执行合并处理的属性的父属性。
        /// </summary>
        public AttributePath ParentAttribute
        {
            get => _parentAttribute;
            set => _parentAttribute = value;
        }

        /// <summary>
        ///     获取获取属性原值的委托。
        /// </summary>
        public GetAttributeValue AttributeOriginalValueGetter => _attributeOriginalValueGetter;

        /// <summary>
        ///     获取并发冲突类型。
        /// </summary>
        public EConcurrentConflictType ConflictType => _conflictType;

        /// <summary>
        ///     当前正在执行合并处理的复杂属性的对象
        /// </summary>
        public object ComplexObject
        {
            get => _complexObject;
            set => _complexObject = value;
        }

        /// <summary>
        ///     当前正在执行合并处理的复杂属性
        /// </summary>
        public ComplexAttribute ComplexAttribute
        {
            get => _complexAttribute;
            set => _complexAttribute = value;
        }
    }
}