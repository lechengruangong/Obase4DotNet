/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示数据库查询结果集中的一行.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:56:55
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using System;
using System.Collections.Generic;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     表示数据库查询结果集中的一行，简称数据行。结果集各列在该行中的值称为域。
    /// </summary>
    public class DataRow
    {
        /// <summary>
        ///     别名生成器。
        /// </summary>
        private readonly AliasGenerator _aliasGenerator;

        /// <summary>
        ///     存储域的字典，其值为域，键为该域对应的数据集列名。
        /// </summary>
        private readonly Dictionary<string, object> _dataDict = new Dictionary<string, object>();

        /// <summary>
        ///     行号字典
        /// </summary>
        private readonly Dictionary<int, string> _rowIndexDict = new Dictionary<int, string>();

        /// <summary>
        ///     映射字段生成器。
        /// </summary>
        private readonly TargetFieldGenerator _targetFieldGenerator;

        /// <summary>
        ///     创建DataRow实例。
        /// </summary>
        /// <param name="aliasGenerator">别名生成器。</param>
        /// <param name="targetFieldGenerator">映射字段生成器。</param>
        public DataRow(AliasGenerator aliasGenerator, TargetFieldGenerator targetFieldGenerator)
        {
            _aliasGenerator = aliasGenerator;
            _targetFieldGenerator = targetFieldGenerator;
        }

        /// <summary>
        ///     向数据行添加一个域。
        /// </summary>
        /// <param name="columnName">列名。</param>
        /// <param name="value">域。</param>
        /// <param name="rowIndex">行号</param>
        public void Add(string columnName, object value, int rowIndex)
        {
            columnName = columnName.ToLower();
            if (_dataDict.ContainsKey(columnName))
                _dataDict[columnName] = value;
            else
                _dataDict.Add(columnName, value);

            if (_rowIndexDict.ContainsKey(rowIndex))
                _rowIndexDict[rowIndex] = columnName;
            else
                _rowIndexDict.Add(rowIndex, columnName);
        }

        /// <summary>
        ///     获取基于数据行在指定关联树节点上创建的对象的标识。
        /// </summary>
        /// <returns>
        ///     返回对象标识。返回null表示数据行在指定节点上不创建任何对象。
        ///     备注：
        ///     只要有有一个标识成员的值为DBNull.Value即返回null。
        /// </returns>
        /// <param name="treeNode">关联树节点。</param>
        public ObjectKey GetObjectKey(AssociationTreeNode treeNode)
        {
            //获取节点类型
            var nodeType = treeNode.RepresentedType;
            //作为根节点的树
            var tree = treeNode.AsTree();


            //KeyMember和KeyField是对应的
            var count = ((IMappable)nodeType).KeyFields.Count;
            var keyField = ((IMappable)nodeType).KeyFields;
            var keyMember = ((IMappable)nodeType).KeyMemberNames;

            //key成员
            var members = new List<ObjectKeyMember>();

            for (var i = 0; i < count; i++)
            {
                //设置参数 等同于设置FiledName
                // _aliasGenerator.SetArgument(keyField[i]);
                //别名
                var alias = tree.Accept(_aliasGenerator, keyField[i]);

                //列名
                var columnName = (string.IsNullOrEmpty(alias) ? keyField[i] : alias).ToLower();

                var value = _dataDict[columnName];
                //是null 返回null
                if (value is DBNull) return null;

                members.Add(new ObjectKeyMember(keyMember[i], value));
            }

            return new ObjectKey(nodeType, members);
        }

        /// <summary>
        ///     从数据行中获取指定简单属性的值。
        /// </summary>
        /// <param name="attrNode">代表简单属性的属性树节点。</param>
        /// <param name="assoNode">代表属性所属类型的关联树节点。</param>
        public object GetValue(SimpleAttributeNode attrNode, AssociationTreeNode assoNode = null)
        {
            //根节点树
            var tree = attrNode.AsTree();
            //目标字段
            var targetField = tree.Accept(_targetFieldGenerator);
            //列名
            var columnName = "";

            //处理关联树
            if (assoNode != null)
            {
                var assTree = assoNode.AsTree();
                columnName = assTree.Accept(_aliasGenerator, targetField);
            }

            //没有别名
            if (string.IsNullOrEmpty(columnName)) columnName = targetField;
            columnName = columnName.ToLower();
            return _dataDict[columnName];
        }

        /// <summary>
        ///     按索引号从数据行中获取域，索引号为域加入数据行的顺序。
        /// </summary>
        /// <param name="columnIndex">索引号。</param>
        public object GetValue(int columnIndex)
        {
            var colName = _rowIndexDict[columnIndex];
            //按照索引号返回
            return _dataDict[colName];
        }

        /// <summary>
        ///     节点专门视图
        /// </summary>
        public class NodeSpecializedView : IObjectData
        {
            /// <summary>
            ///     作为视图依据的关联树节点。
            /// </summary>
            private readonly AssociationTreeNode _node;

            /// <summary>
            ///     作为视图源的数据行。
            /// </summary>
            private readonly DataRow _sourceRow;

            /// <summary>
            ///     创建NodeSpecializedView实例。
            /// </summary>
            /// <param name="sourceRow">作为源的数据行。</param>
            /// <param name="node">作为视图依据的关联树节点。</param>
            public NodeSpecializedView(DataRow sourceRow, AssociationTreeNode node)
            {
                _sourceRow = sourceRow;
                _node = node;
            }

            /// <summary>
            ///     获取对象标识。
            /// </summary>
            public ObjectKey GetObjectKey()
            {
                return _sourceRow.GetObjectKey(_node);
            }

            /// <summary>
            ///     获取指定属性树节点代表的简单属性的值。
            /// </summary>
            /// <param name="attrNode">属性树节点。</param>
            public object GetValue(SimpleAttributeNode attrNode)
            {
                return _sourceRow.GetValue(attrNode, _node);
            }
        }
    }
}
