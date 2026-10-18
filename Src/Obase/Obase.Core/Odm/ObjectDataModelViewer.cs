/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据模型查看器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:25:21
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;
using System.Text;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     对象数据模型查看器
    /// </summary>
    public static class ObjectDataModelViewer
    {
        /// <summary>
        ///     获取对象数据模型映射的简单视图
        ///     仅包含实体型的映射表和关联引用的映射关系
        /// </summary>
        /// <param name="context">要查看的上下文</param>
        /// <returns></returns>
        public static StringBuilder GetSimpleObjectDataModelMappingView(ObjectContext context)
        {
            //获取模型
            var model = context.Model;
            
            return GetSimpleObjectDataModelMappingView(model);
        }

        /// <summary>
        ///     获取对象数据模型映射的简单视图
        ///     仅包含实体型的映射表和关联引用的映射关系
        /// </summary>
        /// <param name="model">要查看的模型</param>
        /// <returns></returns>
        public static StringBuilder GetSimpleObjectDataModelMappingView(ObjectDataModel model)
        {
            //结果
            var result = new StringBuilder();
            //检查实体型即可
            var entities = model.Types.OfType<EntityType>().ToList();
            result.Append($"本模型共包含{entities.Count}个实体型.").AppendLine();
            //简略版 只处理本身和关联引用的
            foreach (var entity in entities)
            {
                result.AppendLine();
                ProcessEntity(entity, result);
                ProcessAssociationReference(entity, result, false);
                result.AppendLine();
            }

            return result;
        }

        /// <summary>
        ///     获取对象数据模型映射的简单视图
        ///     包含完整的映射关系
        /// </summary>
        /// <param name="context">要查看的上下文</param>
        /// <returns></returns>
        public static StringBuilder GetFullObjectDataModelMappingView(ObjectContext context)
        {
            //获取模型
            var model = context.Model;
            
            return GetFullObjectDataModelMappingView(model);
        }

        /// <summary>
        ///     获取对象数据模型映射的简单视图
        ///     包含完整的映射关系
        /// </summary>
        /// <param name="model">要查看的模型</param>
        /// <returns></returns>
        public static StringBuilder GetFullObjectDataModelMappingView(ObjectDataModel model)
        {
            //结果
            var result = new StringBuilder();
            //检查实体型即可
            var entities = model.Types.OfType<EntityType>().ToList();
            result.Append($"本模型共包含{entities.Count}个实体型.").AppendLine();
            //完整版 处理本身,属性和关联引用的
            foreach (var entity in entities)
            {
                result.AppendLine();
                ProcessEntity(entity, result);
                ProcessAttribute(entity, result);
                ProcessAssociationReference(entity, result, true);
                result.AppendLine();
            }

            return result;
        }

        /// <summary>
        ///     处理实体型本身的映射
        /// </summary>
        /// <param name="entityType">实体型</param>
        /// <param name="stringBuilder">结果</param>
        private static void ProcessEntity(EntityType entityType, StringBuilder stringBuilder)
        {
            //实体型的继承关系
            if (entityType.DerivingFrom != null)
                stringBuilder
                    .Append(
                        $"实体型{entityType.ClrType}继承自{entityType.DerivingFrom.ClrType},映射表为{entityType.TargetTable}.")
                    .AppendLine();
            else
                stringBuilder.Append($"实体型{entityType.ClrType}的映射表为{entityType.TargetTable}.").AppendLine();
            //实体型的主键
            stringBuilder.Append($"实体型{entityType.ClrType}共有{entityType.KeyAttributes.Count}个主键.").AppendLine();
            var seq = 1;
            foreach (var keyAttribute in entityType.KeyAttributes.Select(key =>
                         entityType.Attributes.FirstOrDefault(p => p.Name == key)))
            {
                if (keyAttribute != null)
                    stringBuilder
                        .Append(
                            $"{seq}. {(entityType.KeyIsSelfIncreased ? "" : "非")}自增主键{keyAttribute.Name},映射类型{keyAttribute.DataType},映射字段{keyAttribute.TargetField}.")
                        .AppendLine();
                seq++;
            }
        }

        /// <summary>
        ///     处理属性的映射
        /// </summary>
        /// <param name="structuralType">结构化类型</param>
        /// <param name="stringBuilder">结果</param>
        private static void ProcessAttribute(StructuralType structuralType, StringBuilder stringBuilder)
        {
            string typeName;
            switch (structuralType)
            {
                case AssociationType _:
                    typeName = "关联型";
                    break;
                case EntityType _:
                    typeName = "实体型";
                    break;
                default:
                    typeName = string.Empty; 
                    break;
            }

            //属性
            stringBuilder.Append($"{typeName}{structuralType.ClrType}共有{structuralType.Attributes.Count}个属性.").AppendLine();
            var seq = 1;
            foreach (var attribute in structuralType.Attributes)
            {
                if (attribute is ComplexAttribute complex)
                    stringBuilder
                        .Append(
                            $"{seq}. 复杂属性{complex.Name},使用的复杂类型为{complex.ComplexType.ClrType},映射类型{complex.DataType},映射字段{complex.TargetField}.")
                        .AppendLine();
                else
                    stringBuilder
                        .Append($"{seq}. 简单属性{attribute.Name},映射类型{attribute.DataType},映射字段{attribute.TargetField}.")
                        .AppendLine();

                seq++;
            }
        }

        /// <summary>
        ///     处理关联引用的映射
        /// </summary>
        /// <param name="entityType">实体型</param>
        /// <param name="stringBuilder">结果</param>
        /// <param name="needAttr">是否需要输出属性信息</param>
        private static void ProcessAssociationReference(EntityType entityType, StringBuilder stringBuilder, bool needAttr)
        {
            //实体型的关联引用
            stringBuilder.Append($"实体型{entityType.ClrType}共有{entityType.AssociationReferences.Count}个关联引用.")
                .AppendLine();
            var seq = 1;
            foreach (var reference in entityType.AssociationReferences)
            {
                stringBuilder
                    .Append(
                        $"{seq}. 关联引用{reference.Name},对应关联型为{reference.AssociationType.ClrType},映射表为{reference.AssociationType.TargetTable}.")
                    .AppendLine();
                //如果是显式关联型 且 需要输出属性信息 则增加关联型上的属性输出
                if (reference.AssociationType.Visible && needAttr)
                {
                    ProcessAttribute(reference.AssociationType, stringBuilder);
                }

                stringBuilder
                    .Append(
                        $"在映射表{reference.AssociationType.TargetTable}中,共有关联端{reference.AssociationType.AssociationEnds.Count}个.")
                    .AppendLine();
                foreach (var end in reference.AssociationType.AssociationEnds)
                {
                    stringBuilder.Append($"关联端{end.EntityType.ClrType}的映射为:").AppendLine();
                    foreach (var mapping in end.Mappings)
                        stringBuilder.Append($"主键{mapping.KeyAttribute}映射为{mapping.TargetField}").AppendLine();
                }

                seq++;
            }
        }
    }
}