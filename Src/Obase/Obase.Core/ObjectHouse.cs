/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示对象仓.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 16:01:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Common;
using Obase.Core.Odm;
using Obase.Core.Query;
using Obase.Core.Saving;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core
{
    /// <summary>
    ///     表示对象仓。
    ///     对象仓是对象的生存环境，主要负责四个方面的职责：（1）记录对象类型、标识等信息；（2）维护对象状态；（3）更改跟踪；（4）延迟加载。
    ///     对象仓提供两种方式来跟踪对象的更改。如果对象实现了IIntervene接口，对象仓将作为介入者（实现IIntervener接口）介入到对象属性的修改流程从而监视
    ///     对象的属性修改行为。如果对象未实现IIntervene接口，对象仓将以属性快照方式跟踪更改。
    ///     当对象类型包含至少一个设置了修改触发器的属性时，Obase将自动为对象类生成实现IIntervene接口的代理类型。只有当应用程序使用ObjectSet{T}.Create方法创建对象实例时才会应用此代理类型。
    ///     对象仓始终使用快照方式跟踪关联引用变更。
    /// </summary>
    public class ObjectHouse : IIntervener
    {
        /// <summary>
        ///     属性变更集合
        /// </summary>
        private readonly HashSet<string> _changedAtteibutes = new HashSet<string>();

        /// <summary>
        ///     对象上下文
        /// </summary>
        private readonly ObjectContext _objectContext;

        /// <summary>
        ///     属性快照字典
        /// </summary>
        private readonly Dictionary<string, object> _prodic = new Dictionary<string, object>();

        /// <summary>
        ///     指示对象仓中的对象是否作为上下文的根对象。
        /// </summary>
        private bool _asRoot;

        /// <summary>
        ///     在对象变更探测过程中，指示对象是否被标记为“保留”。对于新对象，默认值为true，对于旧对象默认值为false
        /// </summary>
        private bool _isRetained;

        /// <summary>
        ///     对象仓中放置的对象。
        /// </summary>
        private object _object;

        /// <summary>
        ///     对象仓中放置的对象的标识。
        /// </summary>
        private ObjectKey _objectKey;

        /// <summary>
        ///     对象仓中放置的对象的类型。
        /// </summary>
        private StructuralType _objectType;

        /// <summary>
        ///     对象仓中放置的对象的状态。
        /// </summary>
        private EObjectStatus _status;

        /// <summary>
        ///     创建对象仓实例。
        /// </summary>
        /// <param name="hostContext">对象仓所属的对象上下文。</param>
        internal ObjectHouse(ObjectContext hostContext)
        {
            _objectContext = hostContext;
        }

        /// <summary>
        ///     指示对象仓中的对象是否作为上下文的根对象。
        /// </summary>
        public bool AsRoot => _asRoot;


        /// <summary>
        ///     获取对象仓中放置的对象的状态。
        /// </summary>
        internal EObjectStatus Status => _status;

        /// <summary>
        ///     获取或设置一个值，该值在对象变更探测过程中，指示对象是否被标记为“保留”。对于新对象，默认值为true，对于旧对象默认值为false。
        /// </summary>
        public bool IsRetained
        {
            get => _isRetained;
            set
            {
                if (value && _status == EObjectStatus.Deleted)
                    return;
                _isRetained = value;
            }
        }

        /// <summary>
        ///     获取对象仓所属的的上下文。
        /// </summary>
        internal ObjectContext HostContext => _objectContext;

        /// <summary>
        ///     获取一个值，该值指示对象仓中放置的对象是否为新创建的。
        /// </summary>
        internal bool IsNew => _status == EObjectStatus.Added;

        /// <summary>
        ///     获取一个值，该值指示对象仓中放置的对象是否被标记为已删除。
        /// </summary>
        internal bool IsRemoved => _status == EObjectStatus.Deleted;

        /// <summary>
        ///     获取对象仓中放置的对象。
        /// </summary>
        internal object Object => _object;

        /// <summary>
        ///     获取对象仓中放置的对象的标识。
        /// </summary>
        internal ObjectKey ObjectKey => _objectKey ?? ObjectSystemVisitor.GetObjectKey(_object, _objectType);

        /// <summary>
        ///     获取对象仓中放置的对象的类型。
        /// </summary>
        internal ObjectType ObjectType => _objectType as ObjectType;

        /// <summary>
        ///     通知介入者属性已更改。实现IIntervener.AttributeChanged方法。（有代理类从写的Set访问器）
        /// </summary>
        /// <param name="obj">发生属性更改的对象</param>
        /// <param name="attrName">发生更改的属性</param>
        public void AttributeChanged(object obj, string attrName)
        {
            //无需实现
        }

        /// <summary>
        ///     请求介入者加载关联。实现IIntervener.LoadAssociation方法。
        ///     对于实体对象，本方法将加载关联引用；对于关联对象则加载关联端。
        /// </summary>
        /// <param name="obj">要加载关联的对象</param>
        /// <param name="referenceName">要加载的关联引用或关联端的名称</param>
        public void LoadAssociation(object obj, string referenceName)
        {
            /////////
            //延迟加载关联引用（代理类重写属性的Get访问器实现，第一次访问关联引用属性或关联端的Get方法时执行本方法）
            /////////

            //取出引用元素和值
            var refElement = ObjectType.GetReferenceElement(referenceName);
            var refValue = refElement.GetValue(obj);

            //是否需要加载
            var needLoad = false;
            //如果值是空 需要进行加载
            if (refValue == null)
            {
                needLoad = true;
            }
            else
            {
                //值不是空 检查是否为可枚举类型
                if (refValue is IEnumerable iEnumerable)
                {
                    var enumerator = iEnumerable.GetEnumerator();
                    //如果可枚举类型没有值是个空集合 需要进行加载
                    if (!enumerator.MoveNext()) needLoad = true;
                    //释放掉资源
                    if (enumerator is IDisposable disposable) disposable.Dispose();
                }
            }

            //需要加载时才加载
            if (needLoad)
            {
                var context = HostContext;

                QueryOp query;
                if (refElement is AssociationReference associationReference)
                {
                    query = associationReference.GenerateLoadingQuery(new[] { obj }, true);
                    //拼接包含操作
                    query = CombineInclude(query, associationReference, obj);
                }

                else
                {
                    //直接获取包含操作
                    query = refElement.GenerateLoadingQuery(new[] { obj });
                }

                //使用查询提供程序进行查询
                var queryProvider = context.ConfigProvider.QueryProvider;
                //获得查询结果
                var refobjs = queryProvider.Execute(query);
                //设置值
                if (refElement.IsMultiple)
                {
                    refElement.SetValue(obj, refobjs);
                }
                else
                {
                    var enumerator = ((IEnumerable)refobjs).GetEnumerator();
                    while (enumerator.MoveNext()) refElement.SetValue(obj, enumerator.Current);
                    if (enumerator is IDisposable disposable) disposable.Dispose();
                }
            }
        }

        /// <summary>
        ///     如果对象不是根对象，将其标记为根对象
        /// </summary>
        internal void OverwriteRootTag()
        {
            _asRoot = true;
        }

        /// <summary>
        ///     拼接包含操作
        ///     如果是使用了显式化的隐式关联型 或者 隐式多方关联 这种用取值器和设值器包装的 需要拼接一个包含操作
        /// </summary>
        /// <param name="queryOp">之前拼接的查询</param>
        /// <param name="associationReference">当前要加载的关联引用</param>
        /// <param name="host">宿主对象</param>
        private QueryOp CombineInclude(QueryOp queryOp, AssociationReference associationReference, object host)
        {
            //是否需要增加包含操作
            var prop = host.GetType().GetProperty(associationReference.Name);
            Utils.GetIsMultiple(prop, out var type);
            //如果定义的类型不是关联引用属性的类型 且 是一个显式关联型
            if (type != associationReference.AssociationType.ClrType && associationReference.AssociationType.Visible)
            {
                //取出不是自己的关联端
                var ends = associationReference.AssociationType.AssociationEnds
                    .Where(p => p.Name != associationReference.LeftEnd).ToArray();

                QueryOp includeOp = null;
                for (var i = 0; i < ends.Length; i++)
                    includeOp = QueryOp.Include(ends[i].Name, queryOp.SourceType, queryOp.Model,
                        i == 0 ? queryOp : includeOp);
                //拼接过 返回
                if (includeOp != null)
                    return includeOp;
                return queryOp;
            }

            //相同 直接处理即可
            return queryOp;
        }

        /// <summary>
        ///     接受针对对象所做的所有更改，将对象状态置为“未修改”，并重新对属性和关联引用建立快照。
        /// </summary>
        internal void AcceptChanges()
        {
            //清空属性变更集合
            _changedAtteibutes.Clear();
            //清空属性快照
            _prodic.Clear();
            //代理对象会实现该接口
            var inter = _object as IIntervene;
            if (_status.Equals(EObjectStatus.Added)) inter?.RegisterIntervener(this);
            //将是否保留设置为False
            _isRetained = false;
            //状态设置为未改变
            _status = EObjectStatus.Unchanged;
            //去掉不是代理对象
            //if (inter == null)
            SnapshotAttribute();
            //重新获取ObjectKey
            _objectKey = ObjectSystemVisitor.GetObjectKey(_object, _objectType);
        }

        /// <summary>
        ///     属性变更探测，将属性的当前值与快照副本进行比对以确定该属性是否已修改。本方法适用于未实现IIntervene的接口。（不是代理类）
        /// </summary>
        public void DetectAttributesChange()
        {
            //不是代理对象并是未修改状态
            if (_status == EObjectStatus.Unchanged)
            {
                //遍历属性和快照值对比
                foreach (var attr in _objectType.Attributes)
                {
                    if (attr.IsForeignKeyDefineMissing)
                        continue;
                    //对象值
                    var obj = ObjectSystemVisitor.GetValue(_object, _objectType, attr.Name);
                    //和快照值对比
                    if (!_prodic.ContainsKey(attr.Name) || !Equals(_prodic[attr.Name], obj))
                        _changedAtteibutes.Add(attr.Name);
                }

                //有修改的属性
                if (_changedAtteibutes.Count > 0) _status = EObjectStatus.Modified;
            }
        }


        /// <summary>
        ///     判定指定的属性是否已修改。
        /// </summary>
        /// <param name="attrName">要判定的属性的名称。</param>
        internal bool JudgeAttributeChange(string attrName)
        {
            //属性是否在修改属性集合中
            return _changedAtteibutes.Contains(attrName);
        }

        /// <summary>
        ///     将对象放入对象仓。
        /// </summary>
        /// <param name="obj">要放入对象仓的对象。</param>
        /// <param name="objectType">对象的类型。</param>
        /// <param name="isNew">指示对象是否为新创建的对象。</param>
        /// <param name="asRoot">是否是根对象</param>
        internal void PutIn(object obj, ObjectType objectType, bool isNew, bool asRoot)
        {
            _asRoot = asRoot;
            _object = obj;
            _objectType = objectType;
            _objectKey = ObjectSystemVisitor.GetObjectKey(obj, objectType);
            if (isNew)
            {
                _status = EObjectStatus.Added;
                _isRetained = true;
                //判断是否为代理对象
                if (obj is IIntervene inter) inter.RegisterIntervener(this);

                //新实体对象，标识属性是自增的 对象键置空
                if (objectType is EntityType entityType && entityType.KeyIsSelfIncreased) _objectKey = null;

                //新关联对象，任意一端的标识属性是自增的 对象键置空
                if (objectType is AssociationType associationType &&
                    associationType.AssociationEnds.Any(p => p.EntityType.KeyIsSelfIncreased))
                    _objectKey = null;
            }
            else
            {
                _isRetained = false;
                _status = EObjectStatus.Unchanged;
                //判断是否为代理对象
                if (obj is IIntervene inter) inter.RegisterIntervener(this);
                //建立属性快照
                SnapshotAttribute();
            }
        }

        /// <summary>
        ///     将对象仓中的对象标记为已删除。
        /// </summary>
        internal void Remove()
        {
            _isRetained = false;
            _status = EObjectStatus.Deleted;

            if (_objectType is AssociationType associationType)
                //对于关联型 级联删除聚合的关联端
                CascadeDeleteAggregatedEnds(associationType);
        }

        /// <summary>
        ///     级联删除聚合的关联端
        /// </summary>
        /// <param name="associationType">关联型</param>
        private void CascadeDeleteAggregatedEnds(AssociationType associationType)
        {
            var aggregatedEnds = associationType.AggregatedEnds;
            //循环聚合端
            foreach (var end in aggregatedEnds)
            {
                //获取端对象
                var endObj = end.GetValue(_object);
                if (endObj == null)
                    continue;
                //是否已附加
                var attached = _objectContext.Attached(endObj);
                if (!attached)
                    //没附加 就附加
                    _objectContext.Attach(endObj);
                //一律移除
                _objectContext.Remove(endObj);
            }
        }

        /// <summary>
        ///     对对象的所有属性建立快照副本。
        /// </summary>
        private void SnapshotAttribute()
        {
            if (_status == EObjectStatus.Unchanged)
                foreach (var attr in _objectType.Attributes)
                {
                    if (attr.IsForeignKeyDefineMissing)
                        continue;
                    //获取值
                    var value = ObjectSystemVisitor.GetValue(_object, attr);
                    //放入快照
                    _prodic.Add(attr.Name, value);
                }
        }


        /// <summary>
        ///     使用指定的对象替换对象仓中的对象。
        ///     实施替换操作须满足：
        ///     （1）原对象的状态为UnChanged；
        ///     （2）两个对象的键相等；
        ///     （3）两个对象均未实现IIntervene接口。
        /// </summary>
        /// <param name="newObj">用于替换对象仓中的对象的新对象。</param>
        internal void ReplaceObject(object newObj)
        {
            //如果是代理对象则不替换（代理对象重写了属性的Set方法，重写的Set方法会调用AttributeChanged修改状态为eObjectStatus.Modified）
            //AttributeChanged其实没有实现 此处都应检查
            //如果是同一个对象 不进行操作
            if (ReferenceEquals(_object, newObj)) return;
            if (_status == EObjectStatus.Unchanged)
            {
                //获取对象的唯一标识
                var newKey = ObjectSystemVisitor.GetObjectKey(newObj, _objectType);
                if (ObjectKey == newKey)
                    _object = newObj;
                else
                    throw new ArgumentException("新对象的键必须与被替换的对象相等");
            }
            else
            {
                throw new ArgumentException("对象已修改,不能被替换");
            }
        }

        /// <summary>
        ///     获取对象指定属性的原值。
        /// </summary>
        /// <param name="attribute">属性。</param>
        /// <param name="parent">父属性。</param>
        internal object GetAttributeOriginalValue(Attribute attribute, AttributePath parent)
        {
            object parentObj = null;

            //是否为首节点
            var isFirst = true;

            if (parent != null)
                foreach (var pathNode in parent)
                    if (isFirst)
                    {
                        var name = pathNode.Name;
                        parentObj = _prodic[name];

                        //操作完成后翻转
                        isFirst = false;
                    }
                    else
                    {
                        return GetAttributeOriginalValue((Attribute)parentObj, parent);
                    }


            if (parentObj == null)
            {
                var name = attribute.Name;
                return _prodic[name];
            }

            return GetAttributeOriginalValue((Attribute)parentObj, parent);
        }
    }
}