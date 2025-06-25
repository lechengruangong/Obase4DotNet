/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Property-Get触发器,触发延迟加载.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:45:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     Property-Get触发器。（用以延长加载）
    /// </summary>
    public class PropertyGetTrigger<TObject, TValue> : IBehaviorTrigger
    {
        /// <summary>
        ///     重写方法
        /// </summary>
        private readonly MethodInfo _getMethod;


        /// <summary>
        ///     属性访问器的名称。
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        ///     使用PropertyInfo创建Property-Get触发器实例。
        /// </summary>
        /// <param name="property">表示一个Property，该Property包含一个Get方法，该方法为触发器</param>
        public PropertyGetTrigger(PropertyInfo property)
        {
            _getMethod = property.GetMethod;
            _propertyName = property.Name;
        }

        /// <summary>
        ///     使用Lamda表达式创建Property-Get触发器实例。
        /// </summary>
        /// <param name="expression">表示Property-Get的Lamda表达式</param>
        public PropertyGetTrigger(Expression<Func<TObject, TValue>> expression)
        {
            var member = (MemberExpression)expression.Body;
            var getName = "get_" + member.Member.Name;
            var classType = expression.Parameters[0].Type;
            _getMethod = classType.GetMethod(getName);
        }

        /// <summary>
        ///     获取属性访问器的名称。
        /// </summary>
        public string PropertyName => _propertyName;

        /// <summary>
        ///     生成一个方法，该方法用于在指定的类中重写当前触发器。
        /// </summary>
        /// <param name="type">要重写当前触发器的类</param>
        public MethodBuilder Override(TypeBuilder type)
        {
            if ((_getMethod.Attributes & MethodAttributes.Virtual) == 0)
                throw new InvalidOperationException($"属性取值触发器{_getMethod.Name}不为虚方法");
            //创建方法
            var methodBuilder = type.DefineMethod(_getMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, _getMethod.ReturnType,
                Type.EmptyTypes);

            return methodBuilder;
        }

        /// <summary>
        ///     方法体
        /// </summary>
        /// <param name="ilGenerator"></param>
        public void CallBase(ILGenerator ilGenerator)
        {
            //压入参数
            ilGenerator.Emit(OpCodes.Ldarg_0);

            //调用基实现
            ilGenerator.Emit(OpCodes.Call, _getMethod);
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
            return _getMethod.GetHashCode();
        }

        /// <summary>
        ///     返回一个值，该值指示此实例是否与指定的对象相等。
        ///     算法：比较两个实例的PropertyName是否相等。
        /// </summary>
        /// <param name="other">与此实例进行比较的触发器实例。</param>
        public override bool Equals(object other)
        {
            var trigger = other as PropertyGetTrigger<TObject, TValue>;
            return trigger != null && Equals(trigger);
        }

        /// <summary>
        ///     比较二者是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool Equals(PropertyGetTrigger<TObject, TValue> other)
        {
            if (other == null) return false;
            return _getMethod == other._getMethod;
        }

        /// <summary>
        ///     重写相等运算符，测定两个触发器实例是否相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static bool operator ==(PropertyGetTrigger<TObject, TValue> left,
            PropertyGetTrigger<TObject, TValue> right)
        {
            if (Equals(left, null) && Equals(right, null)) return true;
            return !Equals(left, null) && left.Equals(right);
        }

        /// <summary>
        ///     重写不相等运算符，测定两个触发器实例是否不相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static bool operator !=(PropertyGetTrigger<TObject, TValue> left,
            PropertyGetTrigger<TObject, TValue> right)
        {
            return !(left == right);
        }
    }
}