/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性映射器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:42:11
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     属性映射器，封装特定于属性的映射方案。
    /// </summary>
    public class AttributeMapper : RealElementMapper
    {
        /// <summary>
        ///     确定是否应当选取指定的元素参与映射。
        /// </summary>
        /// <param name="element">要确定的元素。</param>
        /// <param name="objectType">元素所属对象的类型。</param>
        /// <param name="objectStatus">元素所属对象的状态。</param>
        /// <param name="attributeHasChanged">Predicate{String}委托，用于判定属性是否已修改。</param>
        public override bool Select(TypeElement element, ObjectType objectType, EObjectStatus objectStatus,
            Predicate<string> attributeHasChanged = null)
        {
            var attr = element as Attribute;
            var associationType = objectType as AssociationType;
            if (attr == null)
                throw new ArgumentException("要选取的参与映射的元素必须为属性,且对象必须为关联型.");

            //按照以下顺序判断是否参与映射：
            //1. 如果是自动生成的值，则不参与映射
            //2. 如果是新增的对象，则参与映射
            //3. 如果是主键字段，则不参与映射
            //4. 如果是删除的对象，且如果是独立的关联端，则参与映射，否则不参与映射
            //5. 如果属性未修改，则不参与映射
            if (attr.DbGenerateValue) return false;

            if (objectStatus == EObjectStatus.Added) return true;

            if (objectType.KeyFields.Contains(attr.TargetField)) return false;

            if (objectStatus == EObjectStatus.Deleted)
            {
                if (associationType != null && !associationType.Independent) return true;

                return false;
            }

            if (attributeHasChanged != null && !attributeHasChanged(element.Name)) return false;

            return true;
        }

        /// <summary>
        ///     将元素映射到字段，即生成字段设值器。
        /// </summary>
        /// <param name="element">要映射的元素。</param>
        /// <param name="obj">要映射的元素所属的对象。</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制。</param>
        public override void Map(TypeElement element, object obj, IMappingWorkflow mappingWorkflow)
        {
            if (element is Attribute attr)
                //如果是外键保证机制定义的 不进行处理
                if (!attr.IsForeignKeyDefineMissing)
                {
                    var value = attr.ValueGetter.GetValue(obj);
                    //复杂属性需要进一步的获取内部属性处理
                    if (attr.IsComplex)
                    {
                        var complex = attr as ComplexAttribute;
                        complex?.ComplexType.Attributes.ForEach(a =>
                            MappComplexAttribute(complex, a, value, mappingWorkflow));
                    }
                    else
                    {
                        var realValue = SetNull ? null : value;
                        mappingWorkflow.SetField(attr.TargetField, realValue);
                    }
                }
        }

        /// <summary>
        ///     映射复杂属性
        /// </summary>
        /// <param name="complex">所属的复杂属性</param>
        /// <param name="attribute">当前复杂属性的属性</param>
        /// <param name="obj">值</param>
        /// <param name="mappingWorkflow">工作流</param>
        private void MappComplexAttribute(ComplexAttribute complex, Attribute attribute, object obj,
            IMappingWorkflow mappingWorkflow)
        {
            //转换值
            var value = attribute.ValueGetter.GetValue(obj);
            var realValue = SetNull ? null : value;

            var connectionStr = "";
            //如果是minvalue 则表示未设置连接符 按照一般名称处理
            if (complex.MappingConnectionChar != char.MinValue)
                connectionStr = $"{complex.TargetField}{complex.MappingConnectionChar}";
            //字段全名
            var filedName = $"{connectionStr}{attribute.TargetField}";

            mappingWorkflow.SetField(filedName, realValue);
        }
    }
}