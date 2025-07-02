/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户标记标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:59:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Query;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户映射模块
    /// </summary>
    public class MultiTenantModule : IMappingModule
    {
        /// <summary>
        ///     宿主上下文类型
        /// </summary>
        private Type _hostContextType;

        /// <summary>
        ///     模型
        /// </summary>
        private ObjectDataModel _model;

        /// <summary>
        ///     构造多租户映射模块
        /// </summary>
        public MultiTenantModule()
        {
        }

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
            savingPipeline.BeginSavingUnit += SavingPipelineOnBeginSavingUnit;
            _model = objectContext.Model;
            _hostContextType = objectContext.GetType();
        }

        /// <summary>
        ///     订阅事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件数据</param>
        private void SavingPipelineOnBeginSavingUnit(object sender, BeginSavingUnitEventArgs e)
        {
            var unit = e.MappingUnit;
            if (unit.HostObject != null) SetMultiTenantValue(unit.HostObject);

            if (unit.MappingObjects != null)
                foreach (var obj in unit.MappingObjects)
                    if (obj != null)
                        SetMultiTenantValue(obj);
        }

        /// <summary>
        ///     设置多租户的值
        /// </summary>
        /// <param name="obj">对象</param>
        private void SetMultiTenantValue(object obj)
        {
            var structuralType = _model.GetStructuralType(obj.GetType());
            if (structuralType != null)
            {
                var ext = structuralType.GetExtension<MultiTenantExtension>();
                if (ext != null)
                {
                    var attrName = string.IsNullOrEmpty(ext.TenantIdMark) ? "obase_gen_tenantIdMark" : ext.TenantIdMark;
                    var attr = structuralType.GetAttribute(attrName);
                    var value = Extensions.GetTenantId(_hostContextType);

                    attr.ValueSetter.SetValue(obj, value);
                }
            }
        }

        /// <summary>
        ///     订阅事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件数据</param>
        private void QueryPipelineOnBeginQuery(object sender, QueryEventArgs e)
        {
            var queryOp = e.Context.Query;
            if (queryOp.SourceModelType is StructuralType structuralType)
            {
                var ext = structuralType.GetExtension<MultiTenantExtension>();
                if (ext != null)
                {
                    // 如果不是标记的 就是Obase生成的
                    var member = string.IsNullOrEmpty(ext.TenantIdMark)
                        ? structuralType.RebuildingType.GetMember("obase_gen_tenantIdMark").FirstOrDefault()
                        : structuralType.RebuildingType.GetMember(ext.TenantIdMark).FirstOrDefault();
                    if (member != null)
                    {
                        Expression segments = null;
                        //取出租户ID
                        var tenantId = Extensions.GetTenantId(_hostContextType);
                        //载入全局就组两个 否则一个
                        var values = ext.LoadingGlobal ? new[] { tenantId, ext.GlobalTenantId } : new[] { tenantId };
                        var parameterExp = Expression.Parameter(structuralType.RebuildingType, "o");
                        foreach (var value in values)
                        {
                            var left = Expression.MakeMemberAccess(parameterExp, member);
                            var type = ext.TenantIdType;
                            var right = Expression.Constant(value, type);
                            var segment = Expression.Equal(left, right);
                            segments = segments == null ? segment : Expression.OrElse(segment, segments);
                        }

                        if (segments != null)
                            e.Context.Query = QueryOp.Where(Expression.Lambda(segments, parameterExp), queryOp.Model,
                                queryOp);
                    }
                }
            }
        }
    }
}