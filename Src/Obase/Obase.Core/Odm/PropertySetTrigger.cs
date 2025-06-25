/*
┌──────────────────────────────────────────────────────────────┐
│　描   述： Property-Set触发器,用于触发属性修改.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:47:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     Property-Set触发器。（代理类通知属性被修改）
    /// </summary>
    public class PropertySetTrigger<TObject, TValue> : IBehaviorTrigger
    {
        /// <summary>
        ///     属性访问器名称。
        /// </summary>
        private readonly string _propertyName;


        /// <summary>
        ///     重写方法
        /// </summary>
        private readonly MethodInfo _setMethod;


        /// <summary>
        ///     使用PropertyInfo创建Property-Set触发器实例。
        /// </summary>
        /// <param name="property">表示一个Property，该Property包含一个Set方法，该方法为触发器</param>
        public PropertySetTrigger(PropertyInfo property)
        {
            _setMethod = property.SetMethod;
            _propertyName = property.Name;
        }

        /// <summary>
        ///     使用Lamda表达式创建Property-Set触发器实例。
        /// </summary>
        /// <param name="expression">表示Property-Set的Lamda表达式</param>
        public PropertySetTrigger(Expression<Action<TObject, TValue>> expression)
        {
            var member = (MemberExpression)expression.Body;
            var setName = "set_" + member.Member.Name;
            var classType = expression.Parameters[0].Type;
            _setMethod = classType.GetMethod(setName);
            _propertyName = member.Member.Name;
        }

        /// <summary>
        ///     获取属性访问器名称。
        /// </summary>
        public string PropertyName => _propertyName;

        /// <summary>
        ///     生成一个方法，该方法用于在指定的类中重写当前触发器。
        /// </summary>
        /// <param name="type">要重写当前触发器的类</param>
        public MethodBuilder Override(TypeBuilder type)
        {
            if ((_setMethod.Attributes & MethodAttributes.Virtual) == 0)
                throw new InvalidOperationException($"属性设值触发器{_setMethod.Name}不为虚方法");
            //创建方法
            var methodBuilder = type.DefineMethod(_setMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, _setMethod.ReturnType,
                _setMethod.GetParameters().Select(s => s.ParameterType).ToArray());

            return methodBuilder;
        }

        /// <summary>
        ///     使用反射发出调用触发器的基础实现
        /// </summary>
        /// <param name="ilGenerator">MSIL指令生成器</param>
        public void CallBase(ILGenerator ilGenerator)
        {
            //压入参数

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);

            //调用基实现
            ilGenerator.Emit(OpCodes.Call, _setMethod);
        }

        /// <summary>
        ///     获取触发器标识，该标识在当前类型的所有触发器中是唯一的。
        ///     算法：直接返回属性访问器名称。
        /// </summary>
        public string UniqueId => _propertyName;

        /// <summary>
        ///     返回触发器实例的哈希代码。
        ///     算法：返回属性访问器名称的哈希代码。
        /// </summary>
        public override int GetHashCode()
        {
            if (_setMethod != null)
                return _setMethod.GetHashCode();
            if (_propertyName != null)
                return _propertyName.GetHashCode();
            return 0;
        }

        /// <summary>
        ///     返回一个值，该值指示此实例是否与指定的对象相等。
        ///     算法：比较两个实例的属性访问器名称是否相等。
        /// </summary>
        /// <param name="other">与此实例进行比较的触发器实例。</param>
        public override bool Equals(object other)
        {
            var trigger = other as PropertySetTrigger<TObject, TValue>;
            return trigger != null && Equals(trigger);
        }

        /// <summary>
        ///     比较二者是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool Equals(PropertySetTrigger<TObject, TValue> other)
        {
            if (other == null) return false;
            return _setMethod == other._setMethod;
        }

        /// <summary>
        ///     重写相等运算符，测定两个触发器实例是否相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static bool operator ==(PropertySetTrigger<TObject, TValue> left,
            PropertySetTrigger<TObject, TValue> right)
        {
            if (Equals(left, null) && Equals(right, null)) return true;
            return !Equals(left, null) && left.Equals(right);
        }

        /// <summary>
        ///     重写不相等运算符，测定两个触发器实例是否不相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static bool operator !=(PropertySetTrigger<TObject, TValue> left,
            PropertySetTrigger<TObject, TValue> right)
        {
            return !(left == right);
        }
    }
}