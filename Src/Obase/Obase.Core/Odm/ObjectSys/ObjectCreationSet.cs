/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：存储ObjectCreation实例的容器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:33:27
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     存储ObjectCreation实例的容器。
    ///     ObjectCreation是一个关联。在创建对象系统过程中我们将新建的对象临时“挂靠”在关联树各节点上，并在挂靠于父子节点的两组对象间建立配属关系（挂靠子节点
    ///     的一个或多个对象配属于一个挂靠父节点的对象），根据挂靠及配属关系可以在对象间建立引用关系，从而最终生成对象系统。ObjectCreation关联即用于描述上述挂
    ///     靠及配属关系。
    ///     基于对象标识建立上述配属关系。如果对象A配属于对象B,则称对象B的标识为A的父标识。
    /// </summary>
    public class ObjectCreationSet
    {
        /// <summary>
        ///     存储ObjectCreation实例的集合，其键为挂靠节点，值为存储新建对象的字典，该字典的键为对象父标识，值为对象自身。
        /// </summary>
        private readonly Dictionary<AssociationTreeNode, Dictionary<ObjectKey, List<object>>> _dict =
            new Dictionary<AssociationTreeNode, Dictionary<ObjectKey, List<object>>>();

        /// <summary>
        ///     用于寄存根对象的寄存器。
        /// </summary>
        private object _rootObject;

        /// <summary>
        ///     获取根对象。
        /// </summary>
        public object RootObject => _rootObject;

        /// <summary>
        ///     添加一个ObjectCreation实例。
        ///     实施说明
        ///     若为parentKey为null，寄存至_rootObj；否则存储到字典。
        /// </summary>
        /// <param name="treeNode">挂靠节点。</param>
        /// <param name="obj">挂靠的对象。</param>
        /// <param name="parentKey">父标识。</param>
        public void Add(AssociationTreeNode treeNode, object obj, ObjectKey parentKey = null)
        {
            //没有父标识时 自己就是根对象
            if (parentKey == null)
            {
                _rootObject = obj;
            }
            else
            {
                //从当前节点的字典中获取对象集
                if (_dict.TryGetValue(treeNode, out var nodeVals))
                {
                    //有 追加到对应的父标识下
                    if (nodeVals.ContainsKey(parentKey)) nodeVals[parentKey].Add(obj);
                    else nodeVals[parentKey] = new List<object> { obj };
                }
                else
                {
                    //没有 添加
                    _dict[treeNode] = new Dictionary<ObjectKey, List<object>>
                    {
                        { parentKey, new List<object> { obj } }
                    };
                }
            }
        }

        /// <summary>
        ///     获取挂靠在指定节点并配属于指定对象的对象集。
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="parentKey"></param>
        public object[] GetObjects(AssociationTreeNode treeNode, ObjectKey parentKey)
        {
            //用字典获取挂靠的对象集
            if (_dict.ContainsKey(treeNode) && _dict[treeNode].ContainsKey(parentKey))
                return _dict[treeNode][parentKey].ToArray();
            return Array.Empty<object>();
        }

        /// <summary>
        ///     清空ObjectCreation集合。
        /// </summary>
        public void Clear()
        {
            _dict?.Clear();
        }
    }
}