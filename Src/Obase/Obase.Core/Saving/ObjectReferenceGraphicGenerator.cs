/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象参照图生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:27:24
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Common;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     对象参照图生成器。
    /// </summary>
    public class ObjectReferenceGraphicGenerator
    {
        /// <summary>
        ///     分析关联对象。将关联对象加入对象参照图，并对各关联端进行分析。
        /// </summary>
        /// <param name="associationObj">要分析的关联对象</param>
        /// <param name="associationType">关联对象的类型</param>
        /// <param name="isSaving">是否存在添加对象集合中的委托</param>
        /// <param name="graphic">要生成的对象参照图</param>
        public void AnalyzeAssociation(object associationObj, AssociationType associationType,
            Predicate<object> isSaving, ObjectReferenceGraphic graphic)
        {
            var ends = associationType.AssociationEnds;
            var endObjs = new List<object>();
            if (ends == null || ends.Count <= 0) return;
            object hostObj = null;
            //遍历关联端
            foreach (var item in ends)
            {
                //获取端对象
                var endObj = ObjectSystemVisitor.GetValue(associationObj, item);
                //委托判断是否存在添加对象集合中
                if (endObj != null && isSaving(endObj))
                {
                    //是否为伴随端 含基类（伴随映射才有伴随端）
                    if (item.EntityType.TargetTable == associationType.TargetTable ||
                        Utils.GetDerivedTargetTable(item.EntityType) == associationType.TargetTable)
                        hostObj = endObj;
                    //排除伴随对象 含基类 (如果是伴随对象则不添加到关联参照对象集合)
                    if (item.EntityType.TargetTable != associationType.TargetTable &&
                        Utils.GetDerivedTargetTable(item.EntityType) != associationType.TargetTable)
                        endObjs.Add(endObj);
                }
            }

            //如果每一端的目标表均与关联表相同 则此时为自关联
            if (ends.All(item =>
                    item.EntityType.TargetTable == associationType.TargetTable ||
                    Utils.GetDerivedTargetTable(item.EntityType) == associationType.TargetTable))
                //自关联中 伴随端存在 则肯定是自己
                if (associationType.CompanionEnd != null)
                    hostObj = ObjectSystemVisitor.GetValue(associationObj, associationType.CompanionEnd);

            //图中不存在
            if (!graphic.Exists(associationObj))
            {
                //独立映射
                if (associationType.Independent)
                    graphic.AddHost(associationObj, endObjs.ToArray());
                //伴随映射
                else
                    graphic.AddCompanion(associationObj, endObjs.ToArray(), hostObj);
            }
        }

        /// <summary>
        ///     分析实体对象。将实体对象加入对象参照图，并导航到各关联对象、分析这些关联对象。
        /// </summary>
        /// <param name="entityObj">要分析的实体对象</param>
        /// <param name="isSaving">一个委托，用于检查传入的对象是否为正在执行保存操作的对象，如果是返回true。第一个参数为传入的对象，第二个参数为返回值。</param>
        /// <param name="graphic">要生成的对象参照图</param>
        public void AnalyzeObject(object entityObj, Predicate<object> isSaving,
            ObjectReferenceGraphic graphic)
        {
            //对象不存在图中并且是要保存的对象
            if (!graphic.Exists(entityObj) && isSaving(entityObj))
                graphic.AddHost(entityObj);
        }
    }
}