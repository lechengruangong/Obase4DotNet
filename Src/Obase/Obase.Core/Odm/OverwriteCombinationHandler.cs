/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：覆盖合并处理策略.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:42:30
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     执行“覆盖”合并处理策略。
    /// </summary>
    public class OverwriteCombinationHandler : IAttributeCombinationHandler
    {
        /// <summary>
        ///     对指定属性执行合并处理。
        /// </summary>
        /// <param name="attribute">要合并其值的属性。</param>
        /// <param name="workflow">对象修改并实施持久化的工作流机制。</param>
        /// <param name="context">合并上下文。</param>
        public void Process(Attribute attribute, IMappingWorkflow workflow, VersionCombinationContext context)
        {
            var valueGetter = attribute.ValueGetter;
            var obj = context.ComplexObject ?? context.Object;
            var value = valueGetter.GetValue(obj);

            var connectionStr = "";
            //如果是复杂属性，获取连接字符 拼接字段名的前半部分
            if (context.ParentAttribute?.FirstOrDefault() is ComplexAttribute complexAttr &&
                complexAttr.MappingConnectionChar != char.MinValue)
                connectionStr = $"{complexAttr.TargetField}{complexAttr.MappingConnectionChar}";

            //字段全名
            var filedName = $"{connectionStr}{attribute.TargetField}";

            workflow.SetField(filedName, value);
        }
    }
}