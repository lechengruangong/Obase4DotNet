/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：映射模块接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:44:21
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     映射模块接口。
    /// </summary>
    public interface IMappingModule
    {
        /// <summary>
        ///     初始化映射模块。
        /// </summary>
        /// <param name="savingPipeline">"保存"管道。</param>
        /// <param name="deletingPipeline">"删除"管道。</param>
        /// <param name="queryPipeline">"查询"管道。</param>
        /// <param name="directlyChangingPipeline">"就地修改"管道。</param>
        /// <param name="objectContext">对象上下文</param>
        void Init(ISavingPipeline savingPipeline, IDeletingPipeline deletingPipeline, IQueryPipeline queryPipeline,
            IDirectlyChangingPipeline directlyChangingPipeline, ObjectContext objectContext);
    }
}