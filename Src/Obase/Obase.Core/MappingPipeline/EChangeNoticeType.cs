/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举修改通知类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:58:22
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     修改通知类型
    /// </summary>
    public enum EChangeNoticeType
    {
        /// <summary>
        ///     对象变更通知
        /// </summary>
        ObjectChange,

        /// <summary>
        ///     就地修改通知
        /// </summary>
        DirectlyChanging
    }
}