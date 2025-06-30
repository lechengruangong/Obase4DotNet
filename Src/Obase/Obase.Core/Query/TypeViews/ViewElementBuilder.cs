/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图元素建造器基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:34:45
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     为视图元素建造器提供基础实现。
    /// </summary>
    public abstract class ViewElementBuilder
    {
        /// <summary>
        ///     作为建造产品的类型元素。
        /// </summary>
        protected TypeElement _element;

        /// <summary>
        ///     对象数据模型。
        /// </summary>
        protected ObjectDataModel _model;

        /// <summary>
        ///     创建ViewElementBuilder实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        protected ViewElementBuilder(ObjectDataModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     获取创建的类型元素。
        /// </summary>
        public TypeElement Element => _element;

        /// <summary>
        ///     实例化类型元素，同时根据需要扩展视图源。
        /// </summary>
        /// <param name="member">与元素对应的类成员。</param>
        /// <param name="expression">类成员绑定的表达式。</param>
        /// <param name="sourceExtension">视图源扩展树。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public abstract void Instantiate(MemberInfo member, Expression expression, AssociationTree sourceExtension,
            ParameterBinding[] paraBindings = null);

        /// <summary>
        ///     设置映射字段。
        /// </summary>
        /// <param name="member">与元素对应的类成员。</param>
        public virtual void SetTargetField(MemberInfo member)
        {
            if (_element is Attribute attribute) attribute.TargetField = member.Name;
        }

        /// <summary>
        ///     设置多重性。
        /// </summary>
        /// <param name="expression">类成员绑定的表达式。</param>
        public virtual void SetMultiple(Expression expression)
        {
            if (_element is Attribute attribute) attribute.IsMultiple = true;
        }

        /// <summary>
        ///     设置取值器。
        /// </summary>
        /// <param name="member">与元素对应的类成员。</param>
        /// <param name="expression">类成员绑定的表达式。</param>
        public virtual void SetValueGetter(MemberInfo member, Expression expression)
        {
            //设置字段取值器或者委托取值器
            if (member is FieldInfo fieldInfo)
            {
                _element.ValueGetter = new FieldValueGetter(fieldInfo);
            }
            else if (member is PropertyInfo propertyInfo)
            {
                var getMethod = propertyInfo.GetMethod;
                if (getMethod != null)
                {
                    var delegater =
                        getMethod.CreateDelegate(Expression.GetFuncType(propertyInfo.DeclaringType,
                            getMethod.ReturnType));
                    var type = typeof(DelegateValueGetter<,>).MakeGenericType(propertyInfo.DeclaringType,
                        propertyInfo.PropertyType);
                    _element.ValueGetter = Activator.CreateInstance(type, delegater) as IValueGetter;
                }
            }
        }

        /// <summary>
        ///     设置设值器。
        /// </summary>
        /// <param name="member">与元素对应的类成员。</param>
        /// <param name="expression">类成员绑定的表达式。</param>
        public virtual void SetValueSetter(MemberInfo member, Expression expression)
        {
            //设置字段设值器或者委托设值器
            if (member is FieldInfo fieldInfo)
            {
                _element.ValueSetter = new FieldValueSetter(fieldInfo);
            }
            else if (member is PropertyInfo propertyInfo)
            {
                var setMethod = propertyInfo.SetMethod;
                if (setMethod == null) return;
                var delegateType = typeof(Action<,>).MakeGenericType(propertyInfo.DeclaringType,
                    setMethod.GetParameters()[0].ParameterType);
                var @delegate = setMethod.CreateDelegate(delegateType);
                _element.ValueSetter = ValueSetter.Create(@delegate, EValueSettingMode.Assignment);
            }
        }
    }
}