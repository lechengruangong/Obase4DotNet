/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：方法触发器,调用方法作为触发条件.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:22:55
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     方法触发器。
    /// </summary>
    public class MethodTrigger : IBehaviorTrigger
    {
        /// <summary>
        ///     一个MethodInfo实例，该实例表示触发对象行为的方法。
        /// </summary>
        private readonly MethodInfo _methodInfo;

        /// <summary>
        ///     创建方法触发器实例。
        /// </summary>
        /// <param name="method">作为触发器的方法</param>
        public MethodTrigger(MethodInfo method)
        {
            _methodInfo = method;
        }

        /// <summary>
        ///     获取一个MethodInfo实例，该实例表示触发对象行为的方法。
        /// </summary>
        public MethodInfo MethodInfo => _methodInfo;

        /// <summary>
        ///     调用Emit方法
        /// </summary>
        /// <param name="ilGenerator"></param>
        public void CallBase(ILGenerator ilGenerator)
        {
            //复制原来的参数
            var parameters = _methodInfo.GetParameters();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            for (var i = 1; i <= parameters.Length; i++)
                ilGenerator.Emit(OpCodes.Ldarg, i);
            //调用原来的方法
            ilGenerator.Emit(OpCodes.Call, _methodInfo);
        }

        /// <summary>
        ///     生成一个方法，该方法用于在指定的类中重写当前触发器。
        /// </summary>
        /// <param name="type">要重写当前触发器的类</param>
        public MethodBuilder Override(TypeBuilder type)
        {
            if (_methodInfo == null) throw new NullReferenceException("方法触发器的方法为空.");
            var methodAttributes = _methodInfo.Attributes;
            //只有虚方法才能被重写
            if ((methodAttributes & MethodAttributes.Virtual) == 0)
                throw new InvalidOperationException($"方法触发器{_methodInfo.Name}必须是虚方法.");
            //用一个新的方法来重写当前触发器 新的方法内调用原来的方法
            var methodBuilder = type.DefineMethod(_methodInfo.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, _methodInfo.ReturnType,
                _methodInfo.GetParameters().Select(s => s.ParameterType).ToArray());
            return methodBuilder;
        }

        /// <summary>
        ///     获取触发器标识，该标识在当前类型的所有触发器中是唯一的。
        ///     算法：方法名称 + 参数1.DataType + 参数2.DataType + ……
        /// </summary>
        public string UniqueId
        {
            get { return _methodInfo.Name + string.Join("_", MethodInfo.GetGenericArguments().Select(s => s.Name)); }
        }

        /// <summary>
        ///     返回触发器实例的哈希代码。
        ///     算法：返回内部MethodInfo实例的哈希代码。
        /// </summary>
        public override int GetHashCode()
        {
            return _methodInfo.GetHashCode();
        }

        /// <summary>
        ///     返回一个值，该值指示此触发器实例是否与指定的对象相等。
        ///     算法：比较两个实例的内部MethodInfo是否相等。
        /// </summary>
        /// <param name="other">与此实例进行比较的触发器实例。</param>
        public override bool Equals(object other)
        {
            var trigger = other as MethodTrigger;
            return trigger != null && Equals(trigger);
        }

        /// <summary>
        ///     比较二者是否相等
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        private bool Equals(MethodTrigger trigger)
        {
            if (trigger == null) return false;
            return _methodInfo == trigger._methodInfo;
        }

        /// <summary>
        ///     相等运算符，测定两个触发器是否相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static bool operator ==(MethodTrigger left, MethodTrigger right)
        {
            if (Equals(left, null) && Equals(right, null)) return true;
            return !Equals(left, null) && left.Equals(right);
        }

        /// <summary>
        ///     不相等运算符，测定两个触发器是否不相等。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="right">右操作数。</param>
        public static bool operator !=(MethodTrigger left, MethodTrigger right)
        {
            return !(left == right);
        }
    }
}