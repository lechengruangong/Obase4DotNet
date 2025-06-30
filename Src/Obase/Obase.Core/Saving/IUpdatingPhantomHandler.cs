/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为更新幻影冲突的处理策略定义规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:54:54
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     为更新幻影冲突的处理策略定义规范。
    /// </summary>
    public interface IUpdatingPhantomHandler
    {
        /// <summary>
        ///     处理更新幻影冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void ProcessConflict(MappingUnit mappingUnit);
    }
}