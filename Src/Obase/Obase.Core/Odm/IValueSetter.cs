/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象设置器接口,向对象中的类型元素设置值.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:28:50
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     对象设置器接口。
    ///     对象设置器用于为对象的属性、关联引用或关联端设置值。
    /// </summary>
    public interface IValueSetter
    {
        /// <summary>
        ///     获取设值模式。
        /// </summary>
        EValueSettingMode Mode { get; }

        /// <summary>
        ///     为对象设值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        void SetValue(object obj, object value);
    }
}