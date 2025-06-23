/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象行为触发器接口,提供代理类的触发器重写规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:54:20
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection.Emit;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     对象行为触发器接口。（代理类才有触发器）
    /// </summary>
    public interface IBehaviorTrigger
    {
        /// <summary>
        ///     获取触发器标识，该标识在当前类型的所有触发器中是唯一的。
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        ///     生成一个方法，该方法用于在指定的类中重写当前触发器。
        /// </summary>
        /// <param name="type">要重写当前触发器的类</param>
        MethodBuilder Override(TypeBuilder type);

        /// <summary>
        ///     使用反射发出调用触发器的基础实现
        /// </summary>
        /// <param name="ilGenerator">MSIL指令生成器</param>
        void CallBase(ILGenerator ilGenerator);

        /// <summary>
        ///     返回触发器实例的哈希代码。
        /// </summary>
        int GetHashCode();

        /// <summary>
        ///     返回一个值，该值指示此实例是否与指定的对象相等。
        /// </summary>
        /// <param name="other">与此实例进行比较的触发器实例。</param>
        bool Equals(object other);
    }
}