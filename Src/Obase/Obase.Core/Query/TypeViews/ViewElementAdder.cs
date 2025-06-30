/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图元素添加器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:40:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     视图元素添加器，根据类的成员（字段或属性访问器）及其绑定的表达式创建视图元素，并添加到目标视图。
    ///     实施说明
    ///     本类为建造器模式指挥者，通过调度具体建造者完成建造。需要时才创建建造者；一旦创建即寄存以供下次使用。
    /// </summary>
    public class ViewElementAdder
    {
        /// <summary>
        ///     对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     为其创建并添加元素的类型视图，简称目标视图。
        /// </summary>
        private readonly TypeView _typeView;

        /// <summary>
        ///     视图属性建造器。
        /// </summary>
        private ViewAttributeBuilder _viewAttributeBuilder;

        /// <summary>
        ///     视图复杂属性建造器。
        /// </summary>
        private ViewComplexAttributeBuilder _viewComplexAttributeBuilder;

        /// <summary>
        ///     视图引用建造器。
        /// </summary>
        private ViewReferenceBuilder _viewReferenceBuilder;

        /// <summary>
        ///     创建ViewElementAdder实例。
        /// </summary>
        /// <param name="typeView">目标视图。</param>
        /// <param name="model">对象数据模型。</param>
        public ViewElementAdder(TypeView typeView, ObjectDataModel model)
        {
            _typeView = typeView;
            _model = model;
        }

        /// <summary>
        ///     创建视图元素并将其添加到目标视图。
        /// </summary>
        /// <param name="member">依据其创建元素的类成员。</param>
        /// <param name="expression">类成员的绑定表达式。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public TypeElement AddElement(MemberInfo member, Expression expression, ParameterBinding[] paraBindings = null)
        {
            Type memberType = null;
            //剥开集合
            if (member is FieldInfo fieldInfo)
                memberType = fieldInfo.FieldType;
            else if (member is PropertyInfo propertyInfo)
                memberType = propertyInfo.PropertyType;
            if (memberType != typeof(string) && memberType?.GetInterface("IEnumerable") != null)
                memberType = memberType.IsArray ? memberType.GetElementType() : memberType.GetGenericArguments()[0];
            var structType = _model.GetStructuralType(memberType);
            ViewElementBuilder builder;
            if (structType is ReferringType)
            {
                //视图引用
                if (_viewReferenceBuilder == null) _viewReferenceBuilder = new ViewReferenceBuilder(_model);
                builder = _viewReferenceBuilder;
            }
            else if (structType is ComplexType)
            {
                //复杂属性
                if (_viewComplexAttributeBuilder == null)
                    _viewComplexAttributeBuilder = new ViewComplexAttributeBuilder(_model);
                builder = _viewComplexAttributeBuilder;
            }
            else
            {
                //视图属性
                if (_viewAttributeBuilder == null) _viewAttributeBuilder = new ViewAttributeBuilder(_model);
                builder = _viewAttributeBuilder;
            }

            //依次调用建造器方法创建视图元素 设置目标字段、是否多值、值获取器和值设置器
            builder.Instantiate(member, expression, _typeView.Extension, paraBindings);
            builder.SetTargetField(member);
            builder.SetMultiple(expression);
            builder.SetValueGetter(member, expression);
            builder.SetValueSetter(member, expression);
            var element = builder.Element;
            _typeView.AddElement(element);
            return element;
        }
    }
}