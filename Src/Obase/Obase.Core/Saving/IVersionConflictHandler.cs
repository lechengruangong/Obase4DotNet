/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为版本冲突的处理策略定义规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:49:01
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     为版本冲突的处理策略定义规范。
    /// </summary>
    public interface IVersionConflictHandler
    {
        /// <summary>
        ///     处理版本冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void ProcessConflict(MappingUnit mappingUnit);
    }
}