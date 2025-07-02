/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：更新映射集.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:39:29
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     更新映射集，负责将待执行更新映射的对象划分为一组映射单元，划分依据为：实体对象和独立关联取其键值、伴随关联取其伴随端的键值，键值相等者为一个单元。
    /// </summary>
    public class UpdateMappingSet
    {
        /// <summary>
        ///     映射单元组
        /// </summary>
        private readonly List<MappingUnit> _mappingUnits = new List<MappingUnit>();

        /// <summary>
        ///     映射单元检索字典
        /// </summary>
        private readonly Dictionary<ObjectKey, MappingUnit> _selectDic = new Dictionary<ObjectKey, MappingUnit>();

        /// <summary>
        ///     获取更新映射集中映射单元的数量。
        /// </summary>
        public int Count => _mappingUnits.Count;


        /// <summary>
        ///     获取指定索引处的映射单元。
        /// </summary>
        /// <param name="index">从零开始的索引</param>
        public MappingUnit this[int index]
        {
            get
            {
                if (index >= _mappingUnits.Count || index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "映射单元索引超出数组界限");
                return _mappingUnits[index];
            }

            set
            {
                if (index >= _mappingUnits.Count || index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "映射单元索引超出数组界限");
                _mappingUnits[index] = value;
            }
        }


        /// <summary>
        ///     向映射集添加伴随关联。
        /// </summary>
        /// <param name="companion">要添加的伴随关联</param>
        /// <param name="associationType">伴随关联的类型</param>
        /// <param name="status">伴随关联对象的状态。</param>
        public void AddCompanion(object companion, AssociationType associationType, EObjectStatus status)
        {
            var key = ObjectSystemVisitor.GetObjectKey(companion, associationType, associationType.CompanionEnd);
            var unit = _selectDic.TryGetValue(key, out var value) ? value : null;
            //向映射单元集中添加映射单元
            if (unit == null)
            {
                unit = new MappingUnit();
                _mappingUnits.Add(unit);
                _selectDic.Add(key, unit);
            }

            //向映射单元中添加伴随关联
            unit.AddCompanion(companion, status);
        }

        /// <summary>
        ///     向映射集中添加一个对象，该对象将作为映射单元的主体对象。注：只有实体对象和独立关联可作为主体对象。
        /// </summary>
        /// <param name="hostObj">要添加的对象</param>
        /// <param name="objectType">对象的类型</param>
        public void AddHost(object hostObj, ObjectType objectType)
        {
            var assoc = objectType as AssociationType;
            if (assoc == null || assoc.Independent)
            {
                var unit = new MappingUnit();
                unit.AddHost(hostObj);
                _mappingUnits.Add(unit);
                var key = ObjectSystemVisitor.GetObjectKey(hostObj, objectType);
                _selectDic[key] = unit;
            }
        }
    }
}