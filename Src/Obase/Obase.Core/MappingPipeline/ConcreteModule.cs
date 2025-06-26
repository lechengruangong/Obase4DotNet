/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型判别模块.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 09:36:29
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     类型判别模块
    /// </summary>
    public class ConcreteModule : IMappingModule
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
                var sign = structuralType.ConcreteTypeSign;
                // 配置了具体类型区分标记
                if (sign != null)
                {
                    var member = structuralType.RebuildingType.GetMember(sign.Item1).FirstOrDefault();
                    if (member == null)
                        //找不到由用户定义的 就找自己补充的
                        member = structuralType.RebuildingType.GetMember("obase_gen_ct").FirstOrDefault();

                    if (member != null)
                    {
                        // 获取所有继承类的区分标记值
                        var values = GetDerivingConcreteTypeValue(structuralType);
                        Expression segments = null;
                        //构造形如 o => o.区分标记 == 值1 || o.区分标记 == 值2 || ... 的表达式
                        var parameterExp = Expression.Parameter(structuralType.RebuildingType, "o");
                        foreach (var value in values)
                        {
                            var left = Expression.MakeMemberAccess(parameterExp, member);
                            var type = sign.Item2.GetType();
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

        /// <summary>
        ///     获取自己和继承类的区分标记值
        /// </summary>
        /// <param name="structuralType"></param>
        /// <returns></returns>
        private List<object> GetDerivingConcreteTypeValue(StructuralType structuralType)
        {
            //加入自己的区分标记
            var result = new List<object> { structuralType.ConcreteTypeSign.Item2 };
            foreach (var derivedType in structuralType.DerivedTypes)
                //加入自己继承类的区分标记
                result.AddRange(GetDerivingConcreteTypeValue(derivedType));

            return result;
        }
    }
}