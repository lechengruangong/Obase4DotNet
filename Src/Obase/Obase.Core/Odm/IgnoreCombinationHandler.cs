/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：“忽略”合并处理策略.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:04:14
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     执行“忽略”合并处理策略。
    /// </summary>
    public class IgnoreCombinationHandler : IAttributeCombinationHandler
    {
        /// <summary>
        ///     对指定属性执行合并处理。
        /// </summary>
        /// <param name="attribute">要合并其值的属性。</param>
        /// <param name="workflow">对象修改并实施持久化的工作流机制。</param>
        /// <param name="context">合并上下文。</param>
        public void Process(Attribute attribute, IMappingWorkflow workflow, VersionCombinationContext context)
        {
            var connectionStr = "";
            //如果是复杂属性，并且存在映射连接字符，则使用目标字段和映射连接字符构建实际的字段名前半部分
            if (context.ParentAttribute?.FirstOrDefault() is ComplexAttribute complexAttr &&
                complexAttr.MappingConnectionChar != char.MinValue)
                connectionStr = $"{complexAttr.TargetField}{complexAttr.MappingConnectionChar}";

            //字段全名
            var filedName = $"{connectionStr}{attribute.TargetField}";
            workflow.IgnoreField(filedName);
        }
    }
}