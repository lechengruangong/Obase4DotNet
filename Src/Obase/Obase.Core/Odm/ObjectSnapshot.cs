/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象快照,用于记录对象的当前状态，包含属性值和引用元素值.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:36:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     对象快照，用于记录对象的当前状态，包含属性值和引用元素值。
    /// </summary>
    [Serializable]
    public class ObjectSnapshot
    {
        /// <summary>
        ///     存储复杂属性值的字典，键为属性名，值为属性值的快照。
        /// </summary>
        private readonly Dictionary<string, ObjectSnapshot> _complexAttributes =
            new Dictionary<string, ObjectSnapshot>();

        /// <summary>
        ///     存储引用元素值的字典，键为元素名，值为被引用对象的标识的集合。
        /// </summary>
        private readonly Dictionary<string, List<ObjectKey>> _referenceKeys = new Dictionary<string, List<ObjectKey>>();

        /// <summary>
        ///     存储简单属性值的字典，键为属性名，值为属性值。
        /// </summary>
        private readonly Dictionary<string, object> _simpleAttributes = new Dictionary<string, object>();

        /// <summary>
        ///     全局引用字典，存储被当前对象直接和间接引用的对象，键为对象标识，值为对象的快照。
        /// </summary>
        private Dictionary<ObjectKey, ObjectSnapshot> _allReferences = new Dictionary<ObjectKey, ObjectSnapshot>();

        /// <summary>
        ///     被快照的对象的类型。
        /// </summary>
        private StructuralType _structuralType;


        /// <summary>
        ///     创建ObjectSnapshot实例。
        /// </summary>
        /// <param name="structuralType">对象的类型。</param>
        public ObjectSnapshot(StructuralType structuralType)
        {
            _structuralType = structuralType;
        }

        /// <summary>
        ///     获取或设置全局引用字典。
        /// </summary>
        public Dictionary<ObjectKey, ObjectSnapshot> AllReferences
        {
            get => _allReferences;
            set => _allReferences = value;
        }

        /// <summary>
        ///     获取对象的所有属性，包括简单属性和复杂属性。
        /// </summary>
        public string[] Attributes
        {
            get
            {
                var list = new List<string>();
                // 添加简单属性和复杂属性的名称
                foreach (var simple in _simpleAttributes.Keys) list.Add(simple);
                foreach (var complex in _complexAttributes.Keys) list.Add(complex);

                return list.ToArray();
            }
        }

        /// <summary>
        ///     获取对象的所有引用元素。
        /// </summary>
        public string[] References
        {
            get
            {
                var list = new List<string>();
                // 添加引用元素的名称
                foreach (var reference in _referenceKeys.Keys) list.Add(reference);

                return list.ToArray();
            }
        }

        /// <summary>
        ///     设置简单属性的值。
        /// </summary>
        /// <param name="attrName">属性名称。</param>
        /// <param name="value">属性值。</param>
        public void SetAttribute(string attrName, object value)
        {
            _simpleAttributes[attrName] = value;
        }

        /// <summary>
        ///     设置复杂属性的值。
        /// </summary>
        /// <param name="attrName">属性名称。</param>
        /// <param name="value">属性值的快照。</param>
        public void SetAttribute(string attrName, ObjectSnapshot value)
        {
            _complexAttributes[attrName] = value;
        }

        /// <summary>
        ///     设置子属性（即复杂属性的值的属性）的值。
        /// </summary>
        /// <param name="attribute">属性名称。</param>
        /// <param name="subAttribute">子属性的名称。</param>
        /// <param name="value">子属性的值。</param>
        public void SetAttribute(string attribute, string subAttribute, object value)
        {
            //存在子属性的值 才添加子属性
            if (_complexAttributes.TryGetValue(attribute, out var complexAttribute))
                complexAttribute.SetAttribute(subAttribute, value);
        }

        /// <summary>
        ///     为指定的引用元素添加一个目标对象。
        /// </summary>
        /// <param name="elementName">元素名称。</param>
        /// <param name="objectKey">被引用对象的标识。</param>
        public void AddReference(string elementName, ObjectKey objectKey)
        {
            //如果存在
            if (_referenceKeys.ContainsKey(elementName))
            {
                //不是空 添加 否则 新建
                if (_referenceKeys[elementName] != null)
                    _referenceKeys[elementName].Add(objectKey);
                else
                    _referenceKeys[elementName] = new List<ObjectKey> { objectKey };
            }
            else
            {
                //不存在则新建
                _referenceKeys.Add(elementName, new List<ObjectKey> { objectKey });
            }
        }

        /// <summary>
        ///     获取指定元素（简单属性、复杂属性、引用元素）的值。
        /// </summary>
        /// <returns>元素的值，对于简单属性、复杂属性和引用元素，值类型分别为基元类型、对象快照和对象标识集合。</returns>
        /// <exception cref="ElementNotFoundException">指定的元素不存在。</exception>
        /// <param name="attributeName">属性名。</param>
        public object GetElement(string attributeName)
        {
            //查找简单属性
            _simpleAttributes.TryGetValue(attributeName, out var simpleValue);
            if (simpleValue != null) return simpleValue;
            //查找复合属性
            _complexAttributes.TryGetValue(attributeName, out var complexValue);
            if (complexValue != null) return complexValue;
            //查找引用
            _referenceKeys.TryGetValue(attributeName, out var referenceValue);
            if (referenceValue != null) return referenceValue;

            throw new ElementNotFoundException(attributeName);
        }

        /// <summary>
        ///     获取指定子属性的值。
        /// </summary>
        /// <returns>子属性的值。</returns>
        /// <exception cref="ElementNotFoundException">指定的属性不存在。</exception>
        /// <param name="attribute">属性名。</param>
        /// <param name="subAttribute">子属性名。</param>
        public object GetAttribute(string attribute, string subAttribute)
        {
            //查找复合属性 获取子属性
            if (_complexAttributes.TryGetValue(attribute, out var complexAttribute))
                return complexAttribute.GetElement(subAttribute);
            throw new ElementNotFoundException(attribute);
        }


        /// <summary>
        ///     获取对象标识。
        /// </summary>
        /// <returns>对象标识</returns>
        // 实施说明:
        // 如果不是ObjectType，引发异常“当前对象不是ObjectType，没有对象标识”。
        // 如果是EntityType，取出标识属性的快照值，生成标识实例。
        // 如果是AssociationType，遍历各端，根据该端的快照值（ObjectKey集合）生成标识实例。如果该端未快照，用其外键属性的快照值替代。
        // 注意，使用ObjectType.KeyMemberNames属性获取标识成员的名称。
        public ObjectKey GetKey()
        {
            //实体型
            if (_structuralType is EntityType entityType)
            {
                var keyMembers = new List<ObjectKeyMember>();
                foreach (var keyMenberName in entityType.KeyFields)
                {
                    var keyMemberValue = GetElement(keyMenberName);
                    keyMembers.Add(
                        new ObjectKeyMember($"{entityType.ClrType.FullName}-{keyMenberName}", keyMemberValue));
                }

                //获取实体型的标识
                return new ObjectKey(entityType, keyMembers);
            }

            //关联型
            if (_structuralType is AssociationType associationType)
            {
                var keyMembers = new List<ObjectKeyMember>();
                foreach (var end in associationType.AssociationEnds)
                {
                    //获取端的快照
                    ObjectSnapshot endObj;
                    try
                    {
                        endObj = GetElement(end.Name) as ObjectSnapshot;
                    }
                    catch
                    {
                        endObj = null;
                    }

                    //关联端有快照
                    if (endObj != null)
                    {
                        var endKey = endObj.GetKey();
                        keyMembers.AddRange(endKey.Members);
                    }
                    //关联端未快照
                    else
                    {
                        foreach (var mapping in end.Mappings)
                        {
                            var val = GetElement(mapping.TargetField);
                            var member = new ObjectKeyMember(mapping.TargetField, val);
                            keyMembers.Add(member);
                        }
                    }
                }

                //获取关联型的标识
                return new ObjectKey(associationType, keyMembers);
            }

            throw new ArgumentException("当前对象不是ObjectType,没有对象标识。");
        }

        /// <summary>
        ///     获取指定的引用元素的值。
        /// </summary>
        /// <returns>对象标识序列。</returns>
        /// <exception cref="ElementNotFoundException">指定的元素不存在。</exception>
        /// <param name="element">引用元素名。</param>
        public IEnumerable<ObjectKey> GetReference(string element)
        {
            //查找引用元素
            if (_referenceKeys.TryGetValue(element, out var reference))
                return reference;

            throw new ElementNotFoundException(element);
        }

        /// <summary>
        ///     检测指定的元素（简单属性、复杂属性、引用元素）是否存在。
        /// </summary>
        /// <param name="attributeName">属性名</param>
        /// <returns></returns>
        public bool ContainsElement(string attributeName)
        {
            //查找简单属性
            _simpleAttributes.TryGetValue(attributeName, out var simpleValue);
            if (simpleValue != null) return true;
            //查找复合属性
            _complexAttributes.TryGetValue(attributeName, out var complexValue);
            if (complexValue != null) return true;
            //查找引用
            _referenceKeys.TryGetValue(attributeName, out var referenceValue);
            if (referenceValue != null) return true;
            //都没有
            return false;
        }

        /// <summary>
        ///     依据全局引用字典生成关联树，并返回用于重建对象系统的数据集。
        /// </summary>
        /// <param name="dataSet">对象数据集合</param>
        /// <returns></returns>
        public AssociationTree GenerateTree(out IObjectDataSet dataSet)
        {
            // 实施说明
            //在为对象建立快照时，对于引用元素，只记录被引对象的标识，
            //而将对象本身放入全局引用字典（AllReferences）——仍然是以快照形式——
            //对象沿关联关系直接或间接引用的对象都被放入了全局引用字典，因此我们通过分析字典中的对象及其引用关系即可重建对象的关联树。

            if (_structuralType is ObjectType objectType)
            {
                var grower = new AssociationTreeGrower(this);
                var associanTree = new AssociationTree(objectType);
                return associanTree.Accept(grower, out dataSet);
            }

            throw new ArgumentException("当前对象不是ObjectType,无法生成关联树");
        }

        /// <summary>
        ///     作为关联树向下访问者，依据对象快照的全局引用字典生成关联树。
        /// </summary>
        private class AssociationTreeGrower : IAssociationTreeDownwardVisitor<AssociationTree, ObjectAssignmentSet>
        {
            /// <summary>
            ///     对象快照
            /// </summary>
            private readonly ObjectSnapshot _obj;

            /// <summary>
            ///     一并返回值
            /// </summary>
            private ObjectAssignmentSet _outSet;

            /// <summary>
            ///     结果
            /// </summary>
            private AssociationTree _result;

            /// <summary>
            ///     构造关联树访问者
            /// </summary>
            /// <param name="obj">对象快照</param>
            public AssociationTreeGrower(ObjectSnapshot obj)
            {
                _obj = obj;
            }

            /// <summary>
            ///     结果
            /// </summary>
            public AssociationTree Result => _result;

            /// <summary>
            ///     需要一并返回的值
            /// </summary>
            public ObjectAssignmentSet OutArgument => _outSet;

            /// <summary>
            ///     前置访问，即在访问子级前执行操作。
            /// </summary>
            /// <param name="subTree">被访问的关联树子树。</param>
            /// <param name="parentState">访问父级时产生的状态数据。</param>
            /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
            /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
            public bool Previsit(AssociationTree subTree, object parentState, out object outParentState,
                out object outPrevisitState)
            {
                if (parentState == null)
                {
                    //创建并存储分派关系
                    var node = subTree.Node;
                    _outSet = new ObjectAssignmentSet();
                    _outSet.Add(new ObjectAssignment(_obj, node, null));
                    //生长关联树
                    var refElements = subTree.RepresentedType.ReferenceElements;
                    foreach (var reference in refElements)
                    {
                        if (node.HasChild(reference.Name))
                            continue;
                        var subKey = _obj.GetReference(reference.Name).ToArray();
                        if (subKey.Length > 0)
                            subTree.AddSubTree(new AssociationTree(reference), reference.Name);
                    }

                    outParentState = new[] { _obj };
                    _result = subTree;
                }

                else
                {
                    if (parentState is ObjectSnapshot[] snapshots)
                    {
                        var objList = new List<ObjectSnapshot>();
                        foreach (var snapshot in snapshots)
                        {
                            var keys = snapshot.GetReference(subTree.ElementName).ToArray();

                            foreach (var key in keys)
                            {
                                //创建并存储分派关系
                                var node = subTree.Node;
                                if (_obj.AllReferences.TryGetValue(key, out var obj))
                                {
                                    _outSet.Add(new ObjectAssignment(obj, node, key));
                                    objList.Add(obj);
                                }

                                //生长关联树
                                var refElements = subTree.RepresentedType.ReferenceElements;
                                foreach (var reference in refElements)
                                {
                                    if (node.HasChild(reference.Name))
                                        continue;
                                    var subKey = _obj.GetReference(reference.Name).ToArray();
                                    if (subKey.Length > 0)
                                        subTree.AddSubTree(new AssociationTree(reference), reference.Name);
                                }
                            }
                        }

                        outParentState = objList.ToArray();
                    }
                    else
                    {
                        outParentState = null;
                    }
                }

                outPrevisitState = null;

                return true;
            }

            /// <summary>
            ///     后置访问，即在访问子级后执行操作。
            /// </summary>
            /// <param name="subTree">被访问的关联树子树。</param>
            /// <param name="parentState">访问父级时产生的状态数据。</param>
            /// <param name="previsitState">前置访问产生的状态数据。</param>
            public void Postvisit(AssociationTree subTree, object parentState, object previsitState)
            {
                //Nothing to do
            }

            /// <summary>
            ///     重置
            /// </summary>
            public void Reset()
            {
                //Nothing to do
            }
        }

        /// <summary>
        ///     存储对象分派关系的实例。
        /// </summary>
        private class ObjectAssignmentSet : IObjectDataSet
        {
            /// <summary>
            ///     添加对象分派关系集合
            /// </summary>
            private readonly List<ObjectAssignment> _assignments = new List<ObjectAssignment>();

            /// <summary>
            ///     获取数据实例
            /// </summary>
            /// <param name="assoNode">关联树节点</param>
            /// <returns></returns>
            public IEnumerable<ObjectDataSetItem> Get(AssociationTreeNode assoNode)
            {
                var resutList = new List<ObjectDataSetItem>();
                //查找符合锚点的分派实例
                var assignments = _assignments.Where(p => p.AnchorNode == assoNode).ToArray();
                if (assignments.Length > 0)
                    foreach (var assignment in assignments)
                        resutList.Add(new ObjectDataSetItem
                        {
                            ObjectData = new ObjectData(assignment.Obj),
                            ParentKey = assignment.ParentKey
                        });

                return resutList;
            }

            /// <summary>
            ///     添加对象分派关系实例。
            /// </summary>
            /// <param name="assignment">分派关系</param>
            public void Add(ObjectAssignment assignment)
            {
                _assignments.Add(assignment);
            }
        }

        /// <summary>
        ///     对象快照面向IObjectData的适配器。
        /// </summary>
        private class ObjectData : IObjectData
        {
            /// <summary>
            ///     对象快照
            /// </summary>
            private readonly ObjectSnapshot _obj;

            /// <summary>
            ///     初始化ObjectData
            /// </summary>
            /// <param name="obj">对象快照</param>
            public ObjectData(ObjectSnapshot obj)
            {
                _obj = obj;
            }

            /// <summary>
            ///     获取属性值
            /// </summary>
            /// <param name="attrNode">属性节点</param>
            /// <returns></returns>
            public object GetValue(SimpleAttributeNode attrNode)
            {
                return _obj.GetElement(attrNode.AttributeName);
            }

            /// <summary>
            ///     获取对象标识
            /// </summary>
            /// <returns></returns>
            public ObjectKey GetObjectKey()
            {
                return _obj.GetKey();
            }
        }

        /// <summary>
        ///     描述对象分派关系。对象分派关系是指执行对象分派过程中，在对象快照与关联树节点间建立的对应关系。
        ///     在为对象建立快照时，对于引用元素，只记录被引对象的标识，而将对象本身放入全局引用字典（AllReferences）。
        ///     对象沿关联关系直接或间接引用的对象都被放入了全局引用字典。
        ///     既然全局引用字典中的对象都是被根对象沿着关联关系引用的，它们与关联树节点之间就存在着对应关系
        ///     ，建立这种对应关系的过程称为“对象分派”。
        ///     在使用对象系统构造器（ObjectSystemBuilder）重建对象时，首先要依据全局引用字典重建关联树并执行对象分派。
        /// </summary>
        private class ObjectAssignment
        {
            /// <summary>
            ///     锚点节点
            /// </summary>
            private readonly AssociationTreeNode _anchorNode;

            /// <summary>
            ///     快照
            /// </summary>
            private readonly ObjectSnapshot _obj;

            /// <summary>
            ///     父级对象键
            /// </summary>
            private readonly ObjectKey _parentKey;

            /// <summary>
            ///     初始化ObjectAssignment的新实例。
            /// </summary>
            /// <param name="obj">快照</param>
            /// <param name="anchorNode">锚点节点</param>
            /// <param name="parentKey">父级对象键</param>
            public ObjectAssignment(ObjectSnapshot obj, AssociationTreeNode anchorNode, ObjectKey parentKey)
            {
                _obj = obj;
                _anchorNode = anchorNode;
                _parentKey = parentKey;
            }

            /// <summary>
            ///     快照
            /// </summary>
            public ObjectSnapshot Obj => _obj;

            /// <summary>
            ///     锚点节点
            /// </summary>
            public AssociationTreeNode AnchorNode => _anchorNode;

            /// <summary>
            ///     父级对象键
            /// </summary>
            public ObjectKey ParentKey => _parentKey;
        }
    }
}