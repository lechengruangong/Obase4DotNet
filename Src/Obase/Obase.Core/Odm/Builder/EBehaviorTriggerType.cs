/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举对象行为触发器的类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:16:12
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     枚举对象行为触发器的类型。
    /// </summary>
    public enum EBehaviorTriggerType : byte
    {
        /// <summary>
        ///     方法型触发器。
        /// </summary>
        Method = 0,

        /// <summary>
        ///     Get访问器型触发器。
        /// </summary>
        PropertyGet = 1,

        /// <summary>
        ///     Set访问器型触发器。
        /// </summary>
        PropertySet = 2
    }
}