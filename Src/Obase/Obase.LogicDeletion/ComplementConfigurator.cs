/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除的补充配置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:22:28
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除的补充配置器
    /// </summary>
    public class ComplementConfigurator : IComplementConfigurator
    {
        /// <summary>
        ///     构造补充配置器
        /// </summary>
        /// <param name="next">下一节</param>
        public ComplementConfigurator(IComplementConfigurator next)
        {
            Next = next;
        }

        /// <summary>
        ///     补充配置管道中的下一个配置器。
        /// </summary>
        public IComplementConfigurator Next { get; }

        /// <summary>
        ///     根据类型配置项中的元数据配置指定的类型。
        /// </summary>
        /// <param name="targetType">要配置的类型。</param>
        /// <param name="configuration">包含配置元数据的类型配置项。</param>
        public void Configurate(StructuralType targetType, StructuralTypeConfiguration configuration)
        {
            var ext = targetType.GetExtension<LogicDeletionExtension>();
            if (ext != null && string.IsNullOrEmpty(ext.DeletionMark))
            {
                var attribute = new Attribute(typeof(bool), "obase_gen_deletionMark")
                {
                    //目标字段 若果未设置DeletionField就和DeletionMark相同
                    TargetField = string.IsNullOrEmpty(ext.DeletionField) ? ext.DeletionMark : ext.DeletionField,
                    //默认不为空
                    Nullable = false
                };

                var field = targetType.RebuildingType.GetField($"{attribute.Name}");
                //构造FieldValueGetter
                var valueGetter = new LogicDeletionFieldValueGetter(field, targetType);
                attribute.ValueGetter = valueGetter;
                //构造FieldValueSetter
                var setter = new LogicDeletionFieldValueSetter(field, targetType);
                attribute.ValueSetter = setter;
                targetType.AddAttribute(attribute);
            }
        }
    }
}