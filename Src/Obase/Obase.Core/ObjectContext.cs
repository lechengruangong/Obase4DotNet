/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象上下文.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 16:00:49
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Query.Heterog;
using Obase.Core.Saving;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core
{
    /// <summary>
    ///     对象上下文，负责为应用程序提供接口。
    /// </summary>
    public abstract class ObjectContext
    {
        /// <summary>
        ///     配置提供程序
        /// </summary>
        private readonly ContextConfigProvider _configProvider;

        /// <summary>
        ///     对象数据模型
        /// </summary>
        protected readonly ObjectDataModel _model;

        /// <summary>
        ///     本地事务是否开始
        /// </summary>
        private bool _transactionBegun;

        /// <summary>
        ///     新对象集合
        /// </summary>
        protected ConcurrentDictionary<object, ObjectHouse> NewObjects;

        /// <summary>
        ///     对象仓集合
        ///     //Obase查询所涉及到的所有对象以及新对象（）（包含代理对象）
        /// </summary>
        protected List<ObjectHouse> ObjectHouses;

        /// <summary>
        ///     旧对象集合
        ///     //
        /// </summary>
        protected ConcurrentDictionary<ObjectKey, ObjectHouse> OldObjects;

        /// <summary>
        ///     构造ObjectContext对象
        /// </summary>
        /// <param name="provider">对象上下文配置提供者</param>
        protected ObjectContext(ContextConfigProvider provider)
        {
            OnInitializing();

            _configProvider = provider;
            _configProvider.ObjectContext = this;

            OnPreCreateModel();

            //获取模型键
            var cacheKey = GetType();
            //获取模型
            _model = GlobalModelCache.Current.GetModel(cacheKey);

            if (_model == null)
            {
                GlobalModelCache.Current.SetModel(cacheKey, _configProvider);
                _model = GlobalModelCache.Current.GetModel(cacheKey);
            }

            _configProvider.Model = _model;

            OnPostCreatedModel(new PostCreateModelEventArgs(_model));

            //自动创建对象集合对象
            if (_configProvider.WhetherCreateSet)
            {
                //获取所有的属性
                var properties = GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var propertie in properties)
                    //找到ObjectSet
                    if (propertie.PropertyType.GetGenericTypeDefinition() == typeof(ObjectSet<>) &&
                        propertie.GetValue(this) == null)
                    {
                        //如果不可写
                        if (!propertie.CanWrite)
                            throw new InvalidOperationException($"当前上下文配置了自动创建对象集合,但{propertie.Name}属性无可用的Set方法.");
                        //设值
                        propertie.SetValue(this, Activator.CreateInstance(propertie.PropertyType, this));
                    }
            }

            //处理继承类的判别标识 如果有 注册模块
            if (_model.Types.Any(p => p.ConcreteTypeSign != null))
                RegisterModule(new ConcreteModule());

            OnInitialized();
        }


        /// <summary>
        ///     获取当前上下文使用的对象数据模型。
        /// </summary>
        public ObjectDataModel Model => _model;

        /// <summary>
        ///     配置提供程序
        /// </summary>
        public ContextConfigProvider ConfigProvider => _configProvider;

        /// <summary>
        ///     本地事务是否开始
        /// </summary>
        public bool TransactionBegun => _transactionBegun;

        /// <summary>
        ///     Initializing（开始初始化）事件，在执行第一项初始化任务前引发
        /// </summary>
        public event EventHandler Initializing;

        /// <summary>
        ///     PreCreatedModel（预建模）事件，在即将开始建模前引发
        /// </summary>
        public event EventHandler PreCreateModel;

        /// <summary>
        ///     PostCreatedModel（建模完成）事件，在建模刚完成时引发；
        /// </summary>
        public event EventHandler<PostCreateModelEventArgs> PostCreatedModel;

        /// <summary>
        ///     PostRegisterModule（模块注册）事件，在每注册完一个映射模块时引发
        /// </summary>
        public event EventHandler<PostRegisterModuleEventArgs> PostRegisterModule;

        /// <summary>
        ///     Initialized（初始化完成）事件，在执行完最后一项初始化任务后引发
        /// </summary>
        public event EventHandler Initialized;

        #region 标记移除对象方法

        /// <summary>
        ///     将对象标记为已删除。
        /// </summary>
        /// <param name="obj">要标记为删除的对象。</param>
        public void Remove<T>(T obj)
        {
            //获取对象的模型
            var ot = Model.GetObjectType(obj.GetType());
            //获取删除对象的标识（标识:通过模型的主键标识）
            var key = ObjectSystemVisitor.GetObjectKey(obj, ot);
            //如果上下文中不存在这个旧对象则不执行任何操作
            if (!OldObjects.TryGetValue(key, out var house))
                return;
            if (house != null)
            {
                //标记为删除状态（SaveChanges时删除）
                house.Remove();
            }
            else
            {
                //新对象不存在者返回
                if (!NewObjects.TryGetValue(obj, out var h))
                    return;
                if (h != null)
                {
                    //从新对象字典移除
                    NewObjects.TryRemove(obj, out _);
                    //从对象仓集合移除
                    ObjectHouses.Remove(h);
                }
            }
        }

        #endregion

        /// <summary>
        ///     在对象上下文中创建一个对象集
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns></returns>
        public ObjectSet<T> CreateSet<T>() where T : class
        {
            return new ObjectSet<T>(this);
        }

        /// <summary>
        ///     使用无参构造函数创建对象的新实例并附加到上下文
        ///     默认使用HasNewInstanceConstructor配置的新实例构造函数 未配置时使用HasConstructor配置的构造函数
        /// </summary>
        /// <returns></returns>
        public T Create<T>() where T : class
        {
            return CreateSet<T>().Create();
        }

        /// <summary>
        ///     使用参数创建对象的新实例并附加到上下文
        ///     默认使用HasNewInstanceConstructor配置的新实例构造函数 未配置时使用HasConstructor配置的构造函数
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public T Create<T>(params object[] parameters) where T : class
        {
            return CreateSet<T>().Create(parameters);
        }


        /// <summary>
        ///     向当前对象上下文注册映射模块。
        ///     说明
        ///     建议具体的对象上下文在自己的构造函数中调用本方法。
        ///     实施说明
        ///     参见顺序图“初始化对象上下文”中注册映射模块的步骤。
        /// </summary>
        /// <param name="module">要注册的模块。</param>
        public void RegisterModule(IMappingModule module)
        {
            //提供程序
            var saveProvider = _configProvider.SavingProvider;
            var queryProvider = _configProvider.QueryProvider;

            module.Init(saveProvider, saveProvider, queryProvider, saveProvider, this);
            OnPostRegisterModule(new PostRegisterModuleEventArgs(module));
        }

        /// <summary>
        ///     Initializing（开始初始化）事件，在执行第一项初始化任务前引发
        /// </summary>
        private void OnInitializing()
        {
            Initializing?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     PreCreatedModel（预建模）事件，在即将开始建模前引发
        /// </summary>
        private void OnPreCreateModel()
        {
            PreCreateModel?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     PostCreatedModel（建模完成）事件，在建模刚完成时引发；
        /// </summary>
        /// <param name="e">建模完成事件数据</param>
        private void OnPostCreatedModel(PostCreateModelEventArgs e)
        {
            PostCreatedModel?.Invoke(this, e);
        }

        /// <summary>
        ///     PostRegisterModule（模块注册）事件，在每注册完一个映射模块时引发
        /// </summary>
        /// <param name="e">模块注册完成事件数据</param>
        private void OnPostRegisterModule(PostRegisterModuleEventArgs e)
        {
            PostRegisterModule?.Invoke(this, e);
        }

        /// <summary>
        ///     Initialized（初始化完成）事件，在执行完最后一项初始化任务后引发
        /// </summary>
        private void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        #region 附加对象方法

        /// <summary>
        ///     将指定的对象作为新对象附加到对象上下文。
        /// </summary>
        /// <param name="obj">要附加的对象。</param>
        public void Attach<T>(T obj)
        {
            if (!Attached(obj))
                Attach(ref obj, true, true);
        }

        /// <summary>
        ///     附加对象到对象上下文。
        /// </summary>
        /// <param name="obj">要附加的对象。</param>
        /// <param name="isNew">指示要附加的对象是否为新创建的。</param>
        /// <param name="asRoot">是否作为根对象</param>
        public void Attach<T>(ref T obj, bool isNew, bool asRoot = false)
        {
            //根据对象类型获取模型对象
            var objectType = _model.GetObjectType(obj.GetType());
            if (ObjectHouses == null)
                ObjectHouses = new List<ObjectHouse>();
            //新对象（表示要添加到数据库的对象）
            if (isNew)
            {
                if (NewObjects == null)
                    NewObjects = new ConcurrentDictionary<object, ObjectHouse>();
                if (NewObjects.ContainsKey(obj)) return;
                //为新对象创建一个对象仓
                var house = new ObjectHouse(this);
                //将新对象放入对象仓
                house.PutIn(obj, objectType, true, asRoot);
                //将新的对象仓放入当前上下文的对象仓集合
                ObjectHouses.Add(house);
                //放入新对象集合
                NewObjects.TryAdd(obj, house);
            }
            //老（旧）对象（不是新对象）
            else
            {
                //获取老对象(老对象：查出来的都是老对象)的标识（标识:通过模型的主键标识）
                var objkey = ObjectSystemVisitor.GetObjectKey(obj, objectType);
                if (OldObjects == null)
                    OldObjects = new ConcurrentDictionary<ObjectKey, ObjectHouse>();
                if (OldObjects.ContainsKey(objkey))
                {
                    //旧对象
                    var oldObj = OldObjects[objkey].Object;

                    //检测当前模型对象中所有引用对象和属性
                    //检查_oldObj的引用元素（记为R），如果值为空，进一步检查obj中该元素的值是否为空，如果不为空，将其值赋给R。
                    //覆盖属性的算法类同。
                    Assimilate(obj, objectType, oldObj);

                    obj = (T)oldObj;
                    //如果这个对象最终按照根对象附加 则此处覆盖对象仓的asRoot
                    if (asRoot != OldObjects[objkey].AsRoot && asRoot)
                        OldObjects[objkey].OverwriteRootTag();
                }
                else
                {
                    //为老对象建立对象仓
                    var house = new ObjectHouse(this);
                    //将老对象装入仓里
                    house.PutIn(obj, objectType, false, asRoot);
                    //加入对象仓集合
                    ObjectHouses.Add(house);
                    //加入旧对象字典
                    OldObjects.TryAdd(objkey, house);
                }
            }
        }

        /// <summary>
        ///     检查指定的对象是否已附加到对象上下文。
        /// </summary>
        /// <param name="obj">要检查的对象。</param>
        internal bool Attached(object obj)
        {
            //检查对象是否存在上下文件（当做新对象和老对象检查）
            return Attached(obj, true) || Attached(obj, false);
        }

        /// <summary>
        ///     检查指定的对象是否已附加到对象上下文。
        /// </summary>
        /// <param name="obj">要检查的对象。</param>
        /// <param name="isNew">指示要检查的对象是否为新创建的对象。</param>
        internal bool Attached(object obj, bool isNew)
        {
            if (isNew) //新对象
            {
                //检查这个新对象是否存在上下（是否已被附加）
                if (NewObjects == null || obj == null)
                    return false;
                return NewObjects.ContainsKey(obj);
            }

            if (OldObjects == null || obj == null)
                return false;
            //获取老对象的标识（标识:通过模型的主键标识）
            var key = ObjectSystemVisitor.GetObjectKey(obj, _model.GetStructuralType(obj.GetType()));
            //检查老对象是否存在
            return OldObjects.ContainsKey(key);
        }

        /// <summary>
        ///     检查指定的对象是否已附加到对象上下文。
        /// </summary>
        /// <param name="obj">要检查的对象。</param>
        /// <param name="house">当对象已附加时返回对象所在的对象仓。</param>
        private bool Attached(object obj, out ObjectHouse house)
        {
            //检查对象是否存在上下文中（当做新对象和老对象检查），如果存在返回存在的对象仓
            return Attached(obj, false, out house) || Attached(obj, true, out house);
        }

        /// <summary>
        ///     检查指定的对象是否已附加到对象上下文。
        /// </summary>
        /// <param name="obj">要检查的对象。</param>
        /// <param name="isNew">指示要检查的对象是否为新创建的对象。</param>
        /// <param name="house">对象仓</param>
        private bool Attached(object obj, bool isNew, out ObjectHouse house)
        {
            house = null;
            var result = false;
            if (isNew)
            {
                //检查新对象是否存在
                if (NewObjects == null || obj == null)
                    return false;
                if (NewObjects.TryGetValue(obj, out var o))
                {
                    //返回存在的对象仓
                    result = true;
                    house = o;
                }
            }
            else
            {
                //检查老对象是否存在
                if (OldObjects == null || obj == null)
                    return false;
                //获取老对象的标识（标识:通过模型的主键标识）
                var key = ObjectSystemVisitor.GetObjectKey(obj, _model.GetStructuralType(obj.GetType()));
                if (OldObjects.TryGetValue(key, out var o))
                {
                    //返回存在的对象仓
                    result = true;
                    house = o;
                }
            }

            return result;
        }

        /// <summary>
        ///     检查指定的对象是否已附加到对象上下文。
        /// </summary>
        /// <param name="obj">要检查的对象。</param>
        /// <param name="house">当对象已附加时返回对象所在的对象仓。</param>
        /// <param name="assimilate">对象已附加时，指示是否使用传入的对象覆盖其属性和引用元素。</param>
        internal bool Attached(object obj, out ObjectHouse house, bool assimilate)
        {
            // 实施说明
            // 
            // 首先作为旧对象检查，然后作为新对象检查。
            // 
            // 如果assimilate==true，首先检查已附加的对象与传入的对象是否为同一对象，如果不是则用传入对象的属性和引用元素值覆盖已附加的对象。覆盖引用算法如下：
            // 
            // 检查_oldObj的引用元素（记为R），如果值为空，进一步检查obj中该元素的值是否为空，如果不为空，将其值赋给R。
            // 覆盖属性的算法类同。
            //检查对象是否存在上下文中（当做新对象和老对象检查），如果存在返回存在的对象仓
            return Attached(obj, false, out house, assimilate) || Attached(obj, true, out house, assimilate);
        }

        /// <summary>
        ///     检查指定的对象是否已附加到对象上下文
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <param name="isNew">作为新对象还是旧对象检查</param>
        /// <param name="house">当对象已附加时返回对象所在的对象仓</param>
        /// <param name="assimilate">对象已附加时，指示是否使用传入的对象覆盖其属性和引用元素。</param>
        /// <returns></returns>
        private bool Attached(object obj, bool isNew, out ObjectHouse house, bool assimilate)
        {
            house = null;
            var result = false;
            if (isNew)
            {
                //检查新对象是否存在
                if (NewObjects == null || obj == null)
                    return false;
                //在新对象集合里找
                if (NewObjects.TryGetValue(obj, out var newObj))
                {
                    //返回存在的对象仓
                    result = true;
                    house = newObj;
                }
            }
            else
            {
                //检查老对象是否存在
                if (OldObjects == null || obj == null)
                    return false;
                //获取老对象的标识（标识:通过模型的主键标识）
                var key = ObjectSystemVisitor.GetObjectKey(obj, _model.GetStructuralType(obj.GetType()));
                if (OldObjects.ContainsKey(key))
                {
                    //返回存在的对象仓
                    result = true;
                    //是否施加覆盖
                    if (assimilate)
                    {
                        var oldObj = OldObjects[key].Object;
                        Assimilate(obj, _model.GetObjectType(obj.GetType()), oldObj);
                    }

                    house = OldObjects[key];
                }
            }

            return result;
        }

        /// <summary>
        ///     覆盖引用和属性
        /// </summary>
        /// <typeparam name="T">被覆盖的对象类型</typeparam>
        /// <param name="obj">要检查的对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="oldObj">存于对象仓中的旧对象</param>
        private void Assimilate<T>(T obj, ObjectType objectType, object oldObj)
        {
            //覆盖引用元素
            foreach (var referenceElement in objectType.ReferenceElements)
                if (referenceElement is AssociationReference associationReference)
                {
                    //旧对象中引用值为空
                    var oldRefValue = associationReference.GetValue(oldObj);

                    if (oldRefValue == null)
                    {
                        //检测新对象中对应的值
                        var objRefValue = associationReference.GetValue(obj);
                        if (objRefValue != null)
                            //新对象不是空则赋给旧对象
                            associationReference.SetValue(oldObj, objRefValue);
                    }
                    else
                    {
                        if (oldRefValue is IEnumerable iEnumerable)
                        {
                            var enumerator = iEnumerable.GetEnumerator();
                            //如果旧对象中引用值没有值
                            if (!enumerator.MoveNext())
                            {
                                //检测新对象中对应的值
                                var objRefValue = associationReference.GetValue(obj);
                                if (objRefValue != null)
                                    //新对象不是空则赋给旧对象
                                    associationReference.SetValue(oldObj, objRefValue);
                            }

                            if (enumerator is IDisposable disposable) disposable.Dispose();
                        }
                    }
                }
                else if (referenceElement is AssociationEnd associationEnd)
                {
                    //旧对象中端值为空
                    var oldEndValue = associationEnd.GetValue(oldObj);
                    if (oldEndValue == null)
                    {
                        //检测新对象中对应的值
                        var objEndValue = associationEnd.GetValue(obj);
                        if (objEndValue != null)
                            //新对象不是空则赋给旧对象
                            associationEnd.SetValue(oldObj, objEndValue);
                    }
                }

            //覆盖属性
            foreach (var attribute in objectType.Attributes)
            {
                //排除生成的属性
                if (attribute.IsForeignKeyDefineMissing)
                    continue;
                var oldAttrValue = attribute.GetValue(oldObj);
                //检测新对象中对应的值
                var objAttrValue = attribute.GetValue(obj);
                if (oldAttrValue == null)
                {
                    if (objAttrValue != null)
                        //新对象不是空则赋给旧对象
                        attribute.SetValue(oldObj, objAttrValue);
                }
                else
                {
                    if (objAttrValue != null)
                        if (!objAttrValue.ToString().Equals(oldAttrValue.ToString()))
                            attribute.SetValue(oldObj, objAttrValue);
                }
            }
        }

        #endregion

        #region 保存更改方法

        /// <summary>
        ///     将对象上下文中发生更改的对象保存到数据源。
        /// </summary>
        public void SaveChanges()
        {
            ObjectHouses = ObjectHouses ?? new List<ObjectHouse>();

            //对象变更探测（如：对象修改过的对象状态设为修改、对象删除的对象状态设为删除等等）
            DetectObjectChange();

            //对象分类（如：分析出那些对象是新增，那些对象是修改，那些对象是删除等等）
            ObjectClassify(out var added, out var changed, out var deleted, out var addedCompanions,
                out var deletedCompanions);

            //调用保存提供程序保存
            _configProvider.SavingProvider.Save(added, changed, deleted, addedCompanions, deletedCompanions,
                JudgeAttributeChange, GetAttributeOriginalValue);

            if (ObjectHouses == null)
                ObjectHouses = new List<ObjectHouse>();
            //移除标记
            var removedList = new List<int>();
            //处理每一个对象
            for (var i = 0; i < ObjectHouses.Count; i++)
                //如果被移除了 就添加至要移除的名单
                if (ObjectHouses[i].IsRemoved)
                {
                    OldObjects.TryRemove(ObjectHouses[i].ObjectKey, out _);
                    removedList.Add(i);
                }
                else
                {
                    if (ObjectHouses[i].IsNew)
                    {
                        //新对象 转移至旧对象中
                        NewObjects.TryRemove(ObjectHouses[i].Object, out var obj);
                        var key = ObjectSystemVisitor.GetObjectKey(obj.Object, obj.ObjectType);
                        if (OldObjects == null)
                            OldObjects = new ConcurrentDictionary<ObjectKey, ObjectHouse>();
                        //尝试添加
                        if (!OldObjects.TryAdd(key, ObjectHouses[i]))
                            //如果没有添加成功 那就是已存在
                            //如已存在的是关联型 是重复的关联型对象
                            //是由于新的隐式关联型的equal函数未通过Emit重写导致的
                            if (ObjectHouses[i].ObjectType is AssociationType)
                                removedList.Add(i);
                    }

                    //接收变更
                    ObjectHouses[i].AcceptChanges();
                }

            //具体移除
            var removedHouses = removedList.Select(removed => ObjectHouses[removed]).ToList();

            foreach (var removeHouse in removedHouses) ObjectHouses.Remove(removeHouse);
        }

        /// <summary>
        ///     探测对象变更
        /// </summary>
        private void DetectObjectChange()
        {
            //获取上下文中的根对象（因为所有老对象都可以通过根对象找出来：比如查询分类（根对象）会查出关联的文章对象，通过分类能找到分类和文章的关联进而找到文章）
            var houses = ObjectHouses.Where(h => h.AsRoot).ToList();
            foreach (var h in houses)
            {
                //所有根对象都默认保留
                h.IsRetained = true;
                if (h.ObjectType is EntityType type)
                    DetectAssociation(h.Object, type);
                else
                    DetectAssociationEnd(h.Object, h.ObjectType as AssociationType, "");
            }

            foreach (var h in ObjectHouses)
            {
                //属性变更探测
                h.DetectAttributesChange();
                //探测游离端 关联型自身没有保留 还不是新加入的
                if (h.ObjectType is AssociationType associationType && !h.IsNew && !h.IsRetained)
                    foreach (var end in associationType.AssociationEnds)
                    {
                        //取出关联端的对象仓
                        var endObj = ObjectSystemVisitor.GetValue(h.Object, end);
                        if (endObj != null)
                        {
                            var objKey = ObjectSystemVisitor.GetObjectKey(endObj, end.EntityType);
                            //找出旧的端对象
                            if (OldObjects != null)
                            {
                                var flag = false;
                                //直接检测 如果包含 标记可以检测
                                if (OldObjects.ContainsKey(objKey))
                                {
                                    flag = true;
                                }
                                else
                                {
                                    //不包含 可能是继承 按照实际类型查询
                                    objKey = ObjectSystemVisitor.GetObjectKey(endObj,
                                        _model.GetEntityType(endObj.GetType()));
                                    //如果包含 标记可以检测
                                    if (OldObjects.ContainsKey(objKey)) flag = true;
                                }

                                //可以检测 进行检测
                                if (flag)
                                {
                                    OldObjects.TryGetValue(objKey, out var endObjHouse);

                                    //此处肯定不为空 因为objKey在之前检测过了 自身没有保留 端对象是保留的
                                    if (endObjHouse != null && endObjHouse.IsRetained)
                                        //这个关联型对象就是被替换了 要移除掉
                                        Remove(h.Object);
                                }
                            }
                        }
                    }
            }
        }

        /// <summary>
        ///     探测指定实体对象的关联对象。
        /// </summary>
        /// <param name="entityObj">要探测其关联对象的实体对象。</param>
        /// <param name="entityType">实体对象的类型。</param>
        private void DetectAssociation(object entityObj, EntityType entityType)
        {
            //遍历实体模型的关联（探测这些关联）
            foreach (var re in entityType.AssociationReferences)
            {
                //从对象取出关联对象（实际的对象，不是模型对象）
                var assoObjs = ObjectSystemVisitor.AssociationNavigate(entityObj, re);
                if (assoObjs != null)
                    //遍历关联对象集合（关联对象可能是集合（如：类目对象包含文章集合），这里做统一处理（不是集合关联也当做集合处理））
                    foreach (var assobj in assoObjs)
                    {
                        //检查对象是否存在上下文（检查的老对象）
                        var attached = Attached(assobj, out var house, true);
                        if (attached)
                        {
                            if (house.IsNew == false && house.IsRetained == false)
                            {
                                //用这个对象替换老对象（供后面和快照对比探测是否被修改过）
                                house.ReplaceObject(assobj);
                                //标记为保留（这个对象不会被删除）
                                house.IsRetained = true;
                                //是否是根对象
                                var asroot = house.AsRoot;
                                if (!asroot) DetectAssociationEnd(assobj, re.AssociationType, re.LeftEnd);
                            }
                        }
                        else
                        {
                            //当做新对象附加到上下文（内部相同的对象只附加一次）
                            var obj = assobj;
                            Attach(ref obj, true);
                            DetectAssociationEnd(assobj, re.AssociationType, re.LeftEnd);
                        }
                    }
            }
        }

        /// <summary>
        ///     探测指定关联对象的关联端对象。
        /// </summary>
        /// <param name="assoObj">要探测其端对象的关联对象。</param>
        /// <param name="associationType">关联对象的类型。</param>
        /// <param name="excludedEnd">指定要排除的关联端。</param>
        private void DetectAssociationEnd(object assoObj, AssociationType associationType, string
            excludedEnd)
        {
            //遍历关联端
            foreach (var end in associationType.AssociationEnds)
                //排除指定端
                if (end.Name != excludedEnd)
                {
                    //从关联对象中获取端对象
                    var endObj = ObjectSystemVisitor.GetValue(assoObj, end);
                    if (endObj == null) continue;
                    //检查端是否在上下文存在
                    var attached = Attached(endObj, out var house);
                    if (attached)
                    {
                        if (house.IsNew == false && house.IsRetained == false)
                        {
                            //替换端对象（供后面进行属性变更探测,以确定是否修改）
                            house.ReplaceObject(endObj);
                            //标记为保留（不会被删除）
                            house.IsRetained = true;
                            //是否为根对象
                            var asRoot = house.AsRoot;
                            if (!asRoot) DetectAssociation(endObj, end.EntityType);
                        }
                    }
                    else
                    {
                        //附加新对象
                        var obj = endObj;
                        Attach(ref obj, end.DefaultAsNew);
                        //视为新对象（默认为false,需用户配置）
                        if (end.DefaultAsNew)
                            //对关联端进行关联引用探测（每个关联对象都是一个实体，如： ）
                            DetectAssociation(endObj, end.EntityType);
                    }
                }
        }

        /// <summary>
        ///     判定指定的属性是否已更改。
        /// </summary>
        /// <param name="obj">要检查的属性所属的对象。</param>
        /// <param name="attrName">要检查的属性。</param>
        private bool JudgeAttributeChange(object obj, string attrName)
        {
            //获取对象的标识（标识:通过模型的主键标识）
            var key = ObjectSystemVisitor.GetObjectKey(obj, _model.GetStructuralType(obj.GetType()));
            //上下文存在对象
            if (OldObjects.TryGetValue(key, out var house))
                //取出旧对象的对象仓
                //检查制度属性是否更改
                return house.JudgeAttributeChange(attrName);

            throw new ArgumentNullException(nameof(key), "旧对象不存在");
        }

        /// <summary>
        ///     获取指定对象指定属性的原值。
        /// </summary>
        /// <param name="obj">要获取其属性原值的对象。</param>
        /// <param name="attribute">属性。</param>
        /// <param name="parent">父属性。</param>
        private object GetAttributeOriginalValue(object obj, Attribute attribute, AttributePath parent)
        {
            //获取此对象的旧对象字典
            var objectType = _model.GetObjectType(obj.GetType());
            var objectKey = ObjectSystemVisitor.GetObjectKey(obj, objectType);
            var house = OldObjects[objectKey];

            return house.GetAttributeOriginalValue(attribute, parent);
        }

        /// <summary>
        ///     按对象状态对对象上下文中的对象进行分类，挑选出新增的、修改过的、已更改的对象。
        /// </summary>
        /// <param name="added">返回新增的对象。该对象既可能为新创建的对象也可能为数据源中已存在的对象。</param>
        /// <param name="changed">返回已修改的对象。</param>
        /// <param name="deleted">返回已删除的对象。</param>
        /// <param name="addedCompanions">增加的伴随关联</param>
        /// <param name="deletedCompanions">删除的伴随关联</param>
        private void ObjectClassify(out List<object> added, out List<object> changed, out List<object> deleted,
            out List<object> addedCompanions, out List<object> deletedCompanions)
        {
            ////////////////////////////////////////////////////
            //此方法在保存上下文时执行SaveChanges，确定新增对象、删除对象等等
            //////////////////////////////////////////////////


            //新增对象集合
            added = new List<object>();
            //修改对象集合
            changed = new List<object>();
            //删除对象集合
            deleted = new List<object>();
            //新增关联集合
            addedCompanions = new List<object>();
            //删除关联集合
            deletedCompanions = new List<object>();

            //遍历上下文中对象仓集合，确定对象所属于哪个集合
            foreach (var house in ObjectHouses)
            {
                //分析隐式关联对象
                if (house.ObjectType is AssociationType asstype)
                    if (asstype.CompanionEnd != null)
                    {
                        //获取关联对象的一端（这里是伴随端）
                        var endObj = ObjectSystemVisitor.GetValue(house.Object, asstype.CompanionEnd);
                        //判断伴随端是否存在上下文中
                        if (!(endObj == null || Attached(endObj, out var endHouse) == false))
                        {
                            switch (endHouse.Status)
                            {
                                //新增关联端（比如分类下添加文章：在文章分类这个关系中文章是新加的分类是原本就有的）
                                case EObjectStatus.Added:
                                    added.Add(house.Object);
                                    break;
                                //删除关联端（如：删除了文章，这个文章和分类的关联也要删除）
                                case EObjectStatus.Deleted:
                                    deleted.Add(house.Object);
                                    break;
                                default:
                                    switch (house.Status)
                                    {
                                        //新增关联（如在一个已有的文章放到已有的分类）
                                        case EObjectStatus.Added:
                                            addedCompanions.Add(house.Object);
                                            break;
                                        //修改关联对象
                                        case EObjectStatus.Modified:
                                            changed.Add(house.Object);
                                            break;
                                        //删除关联（如上：移除一个关系，文章移走）
                                        case EObjectStatus.Deleted:
                                            deletedCompanions.Add(house.Object);
                                            break;
                                    }

                                    break;
                            }

                            continue;
                        }
                    }


                //根据对象仓的状态确认对象要执行的操作（删除或新增等等）
                switch (house.Status)
                {
                    case EObjectStatus.Added:
                        added.Add(house.Object);
                        break;
                    case EObjectStatus.Deleted:
                        deleted.Add(house.Object);
                        break;
                    //对象的属性被修改
                    case EObjectStatus.Modified:
                        changed.Add(house.Object);
                        break;
                }
            }
        }

        #endregion

        #region 显式事务

        /// <summary>
        ///     开始事务。
        /// </summary>
        public void BeginTransaction()
        {
            if (_configProvider.StorageProviders.Count <= 0)
                if (_configProvider.QueryProvider is HeterogQueryProvider heterogQueryProvider)
                    heterogQueryProvider.StorageProviderCreator.Invoke(_model.StorageSymbol ??
                                                                       StorageSymbols.Current.Default);


            foreach (var provider in _configProvider.StorageProviders.Values) provider.BeginTransaction();

            _transactionBegun = true;
        }

        /// <summary>
        ///     提交当前事务。
        /// </summary>
        public void Commit()
        {
            if (_configProvider.StorageProviders.Count <= 0)
                if (_configProvider.QueryProvider is HeterogQueryProvider heterogQueryProvider)
                    heterogQueryProvider.StorageProviderCreator.Invoke(_model.StorageSymbol ??
                                                                       StorageSymbols.Current.Default);

            foreach (var provider in _configProvider.StorageProviders.Values) provider.CommitTransaction();
            _transactionBegun = false;
        }

        /// <summary>
        ///     回滚当前事务。
        /// </summary>
        public void RollbackTransaction()
        {
            if (_configProvider.StorageProviders.Count <= 0)
                if (_configProvider.QueryProvider is HeterogQueryProvider heterogQueryProvider)
                    heterogQueryProvider.StorageProviderCreator.Invoke(_model.StorageSymbol ??
                                                                       StorageSymbols.Current.Default);

            foreach (var provider in _configProvider.StorageProviders.Values) provider.RollbackTransaction();
            _transactionBegun = false;
        }

        /// <summary>
        ///     显式的声明释放资源
        /// </summary>
        public void Release()
        {
            foreach (var provider in _configProvider.StorageProviders.Values) provider.ReleaseResource();
        }

        #endregion
    }

    /// <summary>
    ///     特定于指定配置提供程序类型的对象上下文。
    /// </summary>
    public abstract class ObjectContext<TConfig> : ObjectContext
        where TConfig : ContextConfigProvider, new()
    {
        /// <summary>
        ///     构造ObjectContext对象
        /// </summary>
        protected ObjectContext() : base(new TConfig())
        {
        }
    }
}