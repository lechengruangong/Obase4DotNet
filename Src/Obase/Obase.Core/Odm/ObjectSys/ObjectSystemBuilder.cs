/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象系统建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:07:26
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     用于将对象附加到对象上下文的委托。
    ///     如果要附加斩对象在对象上下文中不存在，则附加该对象，否则将该对象合并至已存在的对象，并将参数的引用修改为已存在的对象。
    /// </summary>
    /// <param name="obj">对要附加的对象的引用</param>
    /// <param name="asRoot">是否作为根对象</param>
    public delegate void AttachObject(ref object obj, bool asRoot = false);

    /// <summary>
    ///     对象系统建造器。
    /// </summary>
    public class ObjectSystemBuilder : IAssociationTreeDownwardVisitor<object>
    {
        /// <summary>
        ///     用于将创建的对象附加到对象上下文的委托。 不指定将不执行附加操作
        /// </summary>
        private readonly AttachObject _attachObject;

        /// <summary>
        ///     指示是否附加根对象
        /// </summary>
        private readonly bool _attachRoot;

        /// <summary>
        ///     已创建对象
        /// </summary>
        private readonly Dictionary<ObjectKey, object> _createdObjs = new Dictionary<ObjectKey, object>();

        /// <summary>
        ///     用于寄存新建对象的容器。
        /// </summary>
        private readonly ObjectCreationSet _objectCreationSet = new ObjectCreationSet();

        /// <summary>
        ///     作为对象数据源的对象数据集。
        /// </summary>
        private readonly IObjectDataSet _objectDataSet;

        /// <summary>
        ///     创建ObjectBuilder实例。
        /// </summary>
        /// <param name="objDataSet">用于创建对象系统的对象数据集。</param>
        /// <param name="attachObject">用于将创建的对象附加到对象上下文的委托。不指定将不执行附加操作</param>
        /// <param name="attachRoot">指示是否附加根对象</param>
        public ObjectSystemBuilder(IObjectDataSet objDataSet, AttachObject attachObject = null, bool attachRoot = true)
        {
            _objectDataSet = objDataSet;
            _attachObject = attachObject;
            _attachRoot = attachRoot;
        }

        /// <summary>
        ///     获取对新建对象系统根对象的引用。
        /// </summary>
        public object Result { get; private set; }

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
            //无前置访问
            outParentState = null;
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
            //获取节点的代表类型
            var nodeType = subTree.RepresentedType;
            var assoNode = subTree.Node;
            //获取对象数据
            var objectDataSetItems = _objectDataSet.Get(assoNode);

            foreach (var objectDataSetItem in objectDataSetItems)
            {
                var currentKey = objectDataSetItem.ObjectData.GetObjectKey();
                if (currentKey == null) continue;

                //创建属性取值委托(本地函数)
                object AttrValueGetter(SimpleAttributeNode attrTreeNode)
                {
                    return objectDataSetItem.ObjectData.GetValue(attrTreeNode);
                }

                //创建引用取值委托(本地函数)
                IEnumerable<object> RefValueGetter(ReferenceElement re)
                {
                    if (re.NavigationUse == ENavigationUse.EmittingReference && !(re.HostType is TypeView))
                        yield break;
                    //使用关联引用的名称取子树
                    var tempTree = subTree.GetSubTree(re.Name);
                    if (tempTree == null) yield break;
                    //用子树从新建对象的容器内获取
                    var objs = _objectCreationSet.GetObjects(tempTree.Node, currentKey);
                    foreach (var item in objs)
                        if (re.NavigationUse == ENavigationUse.DirectlyReference)
                            yield return re.Navigation.TargetEnd.GetValue(item);
                        else
                            yield return item;
                }

                //是否使用Include进行了加载 如果进行了 则可以获取到关联树
                bool HasInclude(ReferenceElement re)
                {
                    return subTree.GetSubTree(re.Name) != null;
                }

                //构造对象
                var obj = nodeType.Instantiate(AttrValueGetter, RefValueGetter, HasInclude);
                if (!CheckAssociation(obj, subTree.Node)) continue;

                // 1.如果根对象是TypeView，整棵树上的对象均不附加。
                // 2.关联对象由补充操作附加，此处不附加
                if (!(nodeType is TypeView) && !(nodeType is AssociationType && !subTree.IsRoot))
                    obj = AttachAndDeduplicate(obj, (ObjectType)nodeType, subTree.IsRoot);
                //创建并寄存ObjectCreation
                _objectCreationSet.Add(assoNode, obj, objectDataSetItem.ParentKey);
                //执行补偿操作
                if (nodeType is EntityType) Complement(assoNode, obj, currentKey);
                Result = obj;
            }
        }

        /// <summary>
        /// </summary>
        public void Reset()
        {
            //Nothing to Do
        }

        /// <summary>
        ///     对象附加与去重。
        /// </summary>
        /// <param name="obj">要附加并去重的对象。</param>
        /// <param name="objType">对象类型。</param>
        /// <param name="asRoot">是否作为根对象。</param>
        private object AttachAndDeduplicate(object obj, ObjectType objType, bool asRoot = false)
        {
            //如果有附加委托
            if (_attachObject != null)
            {
                //调用附加委托
                var attachedObj = obj;
                _attachObject.Invoke(ref attachedObj, asRoot);
                //引用不相同 替换
                if (!ReferenceEquals(obj, attachedObj)) obj = attachedObj;
            }
            else
            {
                //没有附加委托 用_createdObjs保存
                var objectKey = objType.GetObjectKey(obj);
                var created = _createdObjs.ContainsKey(objectKey);
                //检测是否创建过该对象
                if (created)
                    obj = _createdObjs[objectKey];
                else
                    _createdObjs.Add(objectKey, obj);
            }

            return obj;
        }

        /// <summary>
        ///     执行补充操作。
        /// </summary>
        /// <param name="currentNode">当前关联树节点。</param>
        /// <param name="currentObj">当前对象。</param>
        /// <param name="currentKey">当前对象的标识。</param>
        private void Complement(AssociationTreeNode currentNode, object currentObj, ObjectKey currentKey)
        {
            foreach (var child in currentNode.Children)
            {
                //获取子节点上的对象(实例)
                var assoObjs = _objectCreationSet.GetObjects(child, currentKey);
                //获取子节点代表的元素
                var childRepresentRef =
                    currentNode.RepresentedType.GetReferenceElement(child.ElementName) as AssociationReference;

                //获取左端名
                var leftEndName = childRepresentRef?.LeftEnd;
                AssociationEnd leftEnd = null;
                //获取关联型的左端
                if (leftEndName != null && child.RepresentedType is AssociationType at)
                    leftEnd = at.GetAssociationEnd(leftEndName);
                if (leftEnd == null) throw new Exception("关联型的左端不存在");

                if (child.RepresentedType is ObjectType objectType)
                {
                    //去重set
                    var set = new HashSet<ObjectKey>();
                    var list = new List<object>();
                    foreach (var obj in assoObjs)
                    {
                        var key = objectType.GetObjectKey(obj);
                        //不存在时加入List
                        if (set.Add(key)) list.Add(obj);
                    }

                    assoObjs = list.ToArray();
                }

                //遍历关联实例
                for (var i = 0; i < assoObjs.Length; i++)
                {
                    //设置关联实例的左端值
                    leftEnd.SetValue(assoObjs[i], currentObj);
                    assoObjs[i] = AttachAndDeduplicate(assoObjs[i], (ObjectType)currentNode.RepresentedType);
                }

                if (!childRepresentRef.AssociationType.Visible) continue;

                if (assoObjs.Length == 0) continue;
                var value = childRepresentRef.IsMultiple ? assoObjs : assoObjs[0];
                childRepresentRef.SetValue(currentObj, value);
            }
        }

        /// <summary>
        ///     审核创建的关联型实例是否符合规范。
        ///     <returns>符合规范返回true，否则返回false。</returns>
        ///     <param name="instance">要检查的关联型实例。</param>
        ///     <param name="assoNode">该关联实例挂靠的关联树节点。</param>
        /// </summary>
        /// 实施说明:
        /// 关联型实例的所有端都必须有值，除非该端（assoEnd）满足以下条件中的至少一个：
        /// （1）启用了延迟加载，忽略；
        /// （2）关联型实例的挂靠节点代表的引用元素为视图引用或反身引用；
        /// （3）上述引用元素是关联引用，且该端是该关联引用的左端（assoNode.Element.GetLeftEnd().Equals(assoEnd)）；
        /// （4）所有映射目标字段（assoEnd.Mappings[].TargetField）都能在关联型上找到映射到它的属性。
        private bool CheckAssociation(object instance, AssociationTreeNode assoNode)
        {
            if (!(assoNode.RepresentedType is AssociationType associationType)) return true; //节点代表类型不是关联型。
            if (!(assoNode is ObjectTypeNode objectTypeNode)) return false;
            if (objectTypeNode.Element is ViewReference || objectTypeNode.Element is SelfReference)
                return true; // 关联型实例的挂靠节点代表的引用元素为视图引用或反身引用；
            foreach (var assoEnd in associationType.AssociationEnds) //遍历关联端
            {
                if (assoEnd.EnableLazyLoading) continue; //启用了延迟加载，忽略；
                if (objectTypeNode.Element is AssociationReference ar && ar.GetLeftEnd().Equals(assoEnd))
                    continue; //是关联型实例的挂靠节点代表的关联引用的左端
                var allExist = true; //所有映射目标字段（assoEnd.Mappings[].TargetField）都能在关联型上找到映射到它的属性
                foreach (var mapping in assoEnd.Mappings)
                    if (associationType.GetAttribute(mapping.TargetField) == null &&
                        associationType.FindAttributeByTargetField(mapping.TargetField) == null)
                        allExist = false;
                if (allExist) continue;

                var endValue = assoEnd.GetValue(instance);
                if (endValue == null) return false;
            }

            return true;
        }
    }
}