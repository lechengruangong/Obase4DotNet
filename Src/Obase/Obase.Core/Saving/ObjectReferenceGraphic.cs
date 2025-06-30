/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象参照图.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:24:15
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     对象参照图.
    ///     对象系统映射到关系模型后，关系（表）间存在着参照关系，对象参照图即为按照这种关系建立的图型数据结构,。
    /// </summary>
    public class ObjectReferenceGraphic
    {
        /// <summary>
        ///     对象图
        /// </summary>
        private readonly Dictionary<object, MappingUnit> _dic = new Dictionary<object, MappingUnit>();

        /// <summary>
        ///     对象集合
        /// </summary>
        private List<MappingUnit> _units;

        /// <summary>
        ///     对象图中指定位置的映射单元
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public MappingUnit this[int index]
        {
            get
            {
                var unitsEnumerator = Units.GetEnumerator();
                for (var i = 0; i <= index && i < Count; i++) unitsEnumerator.MoveNext();
                unitsEnumerator.Dispose();
                return unitsEnumerator.Current;
            }
        }


        /// <summary>
        ///     获取对象图中所有的映射单元。
        /// </summary>
        public List<MappingUnit> Units => _units ?? (_units = new List<MappingUnit>());

        /// <summary>
        ///     获取对象图中映射单元的个数。
        /// </summary>
        public int Count => Units.Count;


        /// <summary>
        ///     检查指定对象在当前对象参照图是否存在。
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        public bool Exists(object obj)
        {
            if (obj != null && _dic.Keys.Contains(obj))
            {
                var unit = _dic[obj];
                if (Equals(obj, unit.HostObject) || unit.Companions.Select(s => s.AssociationObject).Contains(obj))
                    return false;
            }

            return false;
        }

        /// <summary>
        ///     向对象参照图添加伴随关联对象，同时指定其参照的对象。
        /// </summary>
        /// <param name="companion">要添加的伴随关联对象</param>
        /// <param name="referredObjs">关联对象参照的对象</param>
        /// <param name="hostObj">关联对象所伴随的主体对象，值为null表示主体对象不参与映射</param>
        public void AddCompanion(object companion, object[] referredObjs, object hostObj = null)
        {
            MappingUnit unit;
            //主体存在
            if (hostObj != null)
            {
                //主体不在字典
                if (!_dic.TryGetValue(hostObj, out var value))
                {
                    unit = new MappingUnit();
                    //添加到字典
                    _dic.Add(hostObj, unit);
                    //添加映射单元集合
                    Units.Add(unit);
                }
                else
                {
                    //取出映射单元
                    unit = value;
                }
            }
            else
            {
                //创建映射单元
                unit = new MappingUnit();
                Units.Add(unit);
            }

            //加入关联
            unit.AddCompanion(companion, EObjectStatus.Added, referredObjs);
            _dic.Add(companion, unit);
        }

        /// <summary>
        ///     向对象参照图添加对象，该对象将作为映射单元的主体对象。
        ///     注：只有实体对象和独立关联对象才可作为主体对象。
        /// </summary>
        /// <param name="hostObj">要添加的对象</param>
        public void AddHost(object hostObj)
        {
            MappingUnit unit;
            //集合不存在对应的映射单元
            if (!_dic.TryGetValue(hostObj, out var value))
            {
                //不存在者创建映射单元
                unit = new MappingUnit();
                //将映射单元放入映射单元集合
                Units.Add(unit);
                //放入映射单元字典
                _dic.Add(hostObj, unit);
            }
            else
            {
                //从字典中取出映射单元
                unit = value;
            }

            //将主体对象放入映射单元
            unit.AddHost(hostObj);
        }

        /// <summary>
        ///     向对象参照图添加关联对象，同时指定该关联对象参照的对象。该关联对象将作为映射单元的主体对象。
        ///     注：只有当关联对象为独立映射时才可作为主体对象。
        /// </summary>
        /// <param name="associationObj">要添加的关联对象</param>
        /// <param name="referredObj">关联对象参照的对象的集合</param>
        public void AddHost(object associationObj, object[] referredObj)
        {
            //创建映射单元
            var unit = new MappingUnit();
            //放入关联对象与参照对象（独立映射的所有端对象）
            unit.AddHost(associationObj, referredObj);
            //放入映射单元集合
            Units.Add(unit);
            //放入字典
            _dic.Add(associationObj, unit);
        }

        /// <summary>
        ///     移除指定位置的映射单元。
        /// </summary>
        /// <param name="index">要移除的映射单元的位置</param>
        public void Remove(int index)
        {
            var unitsEnumerator = Units.GetEnumerator();
            for (var i = 0; i <= index && i < Count; i++)
                if (i < index)
                {
                    unitsEnumerator.MoveNext();
                }
                else if (i == index)
                {
                    unitsEnumerator.MoveNext();
                    var unit = unitsEnumerator.Current;
                    //删除集合
                    Units.Remove(unit);
                    //删除字典
                    var keys = new List<object>();
                    foreach (var key in _dic.Keys)
                        if (Equals(_dic[key], unit))
                            keys.Add(key);
                    keys.ForEach(key => _dic.Remove(key));
                }

            unitsEnumerator.Dispose();
        }
    }
}