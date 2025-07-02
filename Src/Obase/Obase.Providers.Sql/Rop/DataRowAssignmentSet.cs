/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：存储DataRowAssignment关联实例.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:17:27
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     存储DataRowAssignment关联实例。
    ///     从数据库查询关联树所代表的对象系统时，结果数据集的结构是由关联树平展而来，即将树型结构平展为线性结构。如果关联树中某一节点代表的类型有一个具多重性的引用元素，那
    ///     么结果数据集在该节点上将表现出重复性。也就是说，结果集中至少存在两个数据行，它们在该节点上将创建出同一对象的两个相同副本，我们把这样的数据行称为等效数据行，或者
    ///     说它们在该节点上是等效的。
    ///     为了过滤等效数据行，在正式创建对象系统前需要先对数据行实施分派操作，保证分派至同一关联树节点的数据行两两不等效。分派算法见活动图“分派数据行”。
    ///     在创建对象系统过程中，需要在挂靠于父、子节点的两组对象间建立配属关系，因而分派至关联树节点的数据行还需要明确其在该节点上的父标识。
    ///     DataRowAssignment就是用于描述上述分派及配属关系的关联。
    /// </summary>
    public class DataRowAssignmentSet : IObjectDataSet
    {
        /// <summary>
        ///     用于存储DataRowAssignment实例的字典，其键为数据行的属主节点，值为存储所属数据行的字典，该字典的键为配属关系父标识，值为数据行序列。
        /// </summary>
        private readonly Dictionary<AssociationTreeNode, Dictionary<ObjectKey, List<DataRow>>> _dict =
            new Dictionary<AssociationTreeNode, Dictionary<ObjectKey, List<DataRow>>>();

        /// <summary>
        ///     寄存分派至根节点的数据行的变量。
        /// </summary>
        private DataRow _rootRow;

        /// <summary>
        ///     获取一个值，该值指示是否不存在任何DataRowAssignment实例，包括分派至根节点的数据行。
        /// </summary>
        public bool IsEmpty => _dict.Count <= 0 && _rootRow == null;

        /// <summary>
        ///     获取挂靠在指定关联树节点上的对象数据集合。
        /// </summary>
        /// <param name="assoNode">关联树节点。</param>
        public IEnumerable<ObjectDataSetItem> Get(AssociationTreeNode assoNode)
        {
            if (assoNode == null || !_dict.ContainsKey(assoNode))
                return new List<ObjectDataSetItem>
                    { new ObjectDataSetItem { ObjectData = new DataRow.NodeSpecializedView(_rootRow, assoNode) } };
            var rowDatas = _dict[assoNode];

            //获取ParentKey的委托
            ObjectKey GetParentKey(DataRow dataRow)
            {
                ObjectKey parentKey = null;
                AssociationTreeNode parentNode = null;
                if (assoNode is ObjectTypeNode objectTypeNode) parentNode = objectTypeNode.Parent;
                if (parentNode != null) parentKey = dataRow.GetObjectKey(parentNode);
                return parentKey;
            }

            return rowDatas.SelectMany(kv => kv.Value.Select(v => new ObjectDataSetItem
            {
                ObjectData = new DataRow.NodeSpecializedView(v, assoNode),
                ParentKey = GetParentKey(v)
            }));
        }

        /// <summary>
        ///     添加一个DataRowAssignment实例。
        /// </summary>
        /// <param name="treeNode">属主节点。</param>
        /// <param name="dataRow">数据行。</param>
        public void Add(AssociationTreeNode treeNode, DataRow dataRow)
        {
            //父节点
            AssociationTreeNode parent = null;
            if (treeNode is ObjectTypeNode objectTypeNode) parent = objectTypeNode.Parent;
            //当前即为根 暂存DataRow
            if (parent == null) _rootRow = dataRow;

            var parentKey = parent == null ? null : dataRow.GetObjectKey(parent);
            if (parentKey != null)
            {
                if (!_dict.ContainsKey(treeNode)) _dict[treeNode] = new Dictionary<ObjectKey, List<DataRow>>();
                var parentRow = _dict[treeNode];
                if (parentRow.ContainsKey(parentKey))
                {
                    var currentObjectTypeNode = (ObjectTypeNode)treeNode;
                    //如果是多重的关联引用 或者 是单个的关联引用但是没有添加过
                    if (currentObjectTypeNode.Element.IsMultiple || parentRow[parentKey].Count < 1)
                        parentRow[parentKey].Add(dataRow);
                }
                else
                {
                    parentRow[parentKey] = new List<DataRow> { dataRow };
                }
            }
        }

        /// <summary>
        ///     清除所有DataRowAssignment实例，包括分派至根节点的数据行。
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
            _rootRow = null;
        }

        /// <summary>
        ///     等效检查，即检查指定关联树节点是否已分派了一个与指定数据行等效的数据行。
        /// </summary>
        /// <param name="treeNode">属主节点。</param>
        /// <param name="dataRow">待检测的数据行。</param>
        public bool ContainEquivalent(AssociationTreeNode treeNode, DataRow dataRow)
        {
            var objectTypeNode = treeNode as ObjectTypeNode;
            if ((objectTypeNode != null && objectTypeNode.Parent == null) || treeNode is TypeViewNode) //根节点
            {
                if (_rootRow == null) return false;
                //比较根
                var rootId = _rootRow.GetObjectKey(treeNode);
                var currId = dataRow.GetObjectKey(treeNode);
                return rootId == currId;
            }

            if (!_dict.ContainsKey(treeNode)) return false; //不存在此节点 肯定不等效
            //取父级
            var parent = objectTypeNode?.Parent;
            //有父级
            var parentKey = parent == null ? null : dataRow.GetObjectKey(parent);
            if (parentKey == null) return false;
            if (_dict[treeNode].ContainsKey(parentKey))
            {
                var currentRowKey = dataRow.GetObjectKey(treeNode);
                var rows = _dict[treeNode][parentKey];
                foreach (var row in rows)
                {
                    if (dataRow.Equals(row))
                        return true;
                    var currentKey = row.GetObjectKey(treeNode);
                    if (currentRowKey == currentKey)
                        return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }
    }
}