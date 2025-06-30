/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为重复创建冲突的处理策略定义规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:49:42
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     为重复创建冲突的处理策略定义规范。
    /// </summary>
    public interface IRepeatCreationHandler
    {
        /// <summary>
        ///     处理重复创建冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void ProcessConflict(MappingUnit mappingUnit);
    }
}