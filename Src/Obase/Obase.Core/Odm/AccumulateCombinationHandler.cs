/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：累加合并处理策略处理器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:54:35
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Saving;
using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     执行“累加”合并处理策略。
    /// </summary>
    public class AccumulateCombinationHandler : IAttributeCombinationHandler
    {
        /// <summary>
        ///     对指定属性执行合并处理。
        /// </summary>
        /// <param name="attribute">要合并其值的属性。</param>
        /// <param name="workflow">对象修改并实施持久化的工作流机制。</param>
        /// <param name="context">合并上下文。</param>
        public void Process(Attribute attribute, IMappingWorkflow workflow, VersionCombinationContext context)
        {
            var getter = attribute.ValueGetter;
            var obj = context.ComplexObject ?? context.Object;
            var newValue = getter.GetValue(obj);
            object originalValue = null;
            var conflictType = context.ConflictType;
            //取出原始值
            if (conflictType == EConcurrentConflictType.VersionConflict)
            {
                var attributeGetter = context.AttributeOriginalValueGetter;
                var tempValue = attributeGetter(context.Object, context.ComplexAttribute ?? attribute);
                originalValue = context.ComplexObject != null ? getter.GetValue(tempValue) : tempValue;
            }

            //判断累加值
            long increment = 0;
            if (originalValue != null && long.TryParse(newValue.ToString(), out var val1) &&
                long.TryParse(originalValue.ToString(), out var val2)) increment = val1 - val2;
            if (long.TryParse(newValue.ToString(), out var valnew) && increment == 0)
                increment += valnew;
            //复杂属性 根据复杂属性的映射连接符号来拼接字段全名
            var connectionStr = "";
            if (context.ParentAttribute?.FirstOrDefault() is ComplexAttribute complexAttr &&
                complexAttr.MappingConnectionChar != char.MinValue)
                connectionStr = $"{complexAttr.TargetField}{complexAttr.MappingConnectionChar}";

            //字段全名
            var filedName = $"{connectionStr}{attribute.TargetField}";

            workflow.IncreaseField(filedName, increment);
        }
    }
}
