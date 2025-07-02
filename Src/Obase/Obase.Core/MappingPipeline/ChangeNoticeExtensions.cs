/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：修改通知扩展.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:44:48
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     修改通知扩展
    /// </summary>
    public static class ChangeNoticeExtensions
    {
        /// <summary>
        ///     启用修改通知
        /// </summary>
        /// <param name="context">对象上下文</param>
        public static void EnableChangeNotice(this ObjectContext context)
        {
            context.RegisterModule(new ChangeNoticeModule(context));
        }
    }
}