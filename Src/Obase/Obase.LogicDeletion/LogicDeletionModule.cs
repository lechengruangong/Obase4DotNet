/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除映射模块.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:31:31
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Query;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除映射模块
    /// </summary>
    public class LogicDeletionModule : IMappingModule
    {
        /// <summary>
        ///     初始化映射模块。
        /// </summary>
        /// <param name="savingPipeline">"保存"管道。</param>
        /// <param name="deletingPipeline">"删除"管道。</param>
        /// <param name="queryPipeline">"查询"管道。</param>
        /// <param name="directlyChangingPipeline">"就地修改"管道。</param>
        /// <param name="objectContext">对象上下文</param>
        public void Init(ISavingPipeline savingPipeline, IDeletingPipeline deletingPipeline,
            IQueryPipeline queryPipeline,
            IDirectlyChangingPipeline directlyChangingPipeline, ObjectContext objectContext)
        {
            queryPipeline.BeginQuery += QueryPipelineOnBeginQuery;
        }

        /// <summary>
        ///     订阅事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件数据</param>
        private void QueryPipelineOnBeginQuery(object sender, QueryEventArgs e)
        {
            var queryOp = e.Context.Query;
            if (queryOp.SourceModelType is StructuralType structuralType)
            {
                var ext = structuralType.GetExtension<LogicDeletionExtension>();
                if (ext != null)
                {
                    //如果不是标记的 就是Obase生成的
                    var member = string.IsNullOrEmpty(ext.DeletionMark)
                        ? structuralType.RebuildingType.GetMember("obase_gen_deletionMark").FirstOrDefault()
                        : structuralType.RebuildingType.GetMember(ext.DeletionMark).FirstOrDefault();

                    if (member != null)
                    {
                        var parameterExp = Expression.Parameter(structuralType.RebuildingType, "o");
                        //构造一个形如 逻辑删除字段==false 的表达式
                        var left = Expression.MakeMemberAccess(parameterExp, member);
                        var right = Expression.Constant(false, typeof(bool));
                        var segment = Expression.Equal(left, right);
                        var logicDelete = QueryOp.Where(Expression.Lambda(segment, parameterExp), queryOp.Model,
                            queryOp);
                        e.Context.Query = logicDelete;
                    }
                }
            }
        }
    }
}