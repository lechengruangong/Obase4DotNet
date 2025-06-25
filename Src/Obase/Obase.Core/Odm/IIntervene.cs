/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：介入接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:06:00
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     介入接口。
    ///     对象实现此接口以允许第三方介入其行为。
    /// </summary>
    public interface IIntervene
    {
        /// <summary>
        ///     向指定的对象注册介入者以实施介入。
        /// </summary>
        /// <param name="intervener">介入者</param>
        void RegisterIntervener(IIntervener intervener);

        /// <summary>
        ///     禁用延迟加载。
        /// </summary>
        void ForbidLazyLoading();

        /// <summary>
        ///     启用延迟加载。
        /// </summary>
        void EnableLazyLoading();
    }
}
