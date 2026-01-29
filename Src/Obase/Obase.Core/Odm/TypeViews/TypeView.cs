/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型视图.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 16:24:53
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     类型视图。
    ///     类型视图是对一个类型及以其为中心的对象系统的局部观察。从形式上看，类型视图是由该类型的元素及其关联（直接或间接）的类型的元素组合而成的临时类型。
    ///     该类型称为视图的源，以源类型为中心的对象系统称为源扩展，可以用一个关联树表示，其根节点代表源类型。
    /// </summary>
    public class TypeView : ReferringType, IMappable
    {
        /// <summary>
        ///     别名生成器
        /// </summary>
        private readonly AssociationTreeNodeAliasGenerator
            _aliasGenerator = new AssociationTreeNodeAliasGenerator();

        /// <summary>
        ///     附加项。
        /// </summary>
        private readonly List<TypeViewAttachingItem> _attachingItems = new List<TypeViewAttachingItem>();

        /// <summary>
        ///     锁对象
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        ///     作为视图源的类型。
        /// </summary>
        private readonly StructuralType _source;

        /// <summary>
        ///     在表达式（如视图属性的绑定表达式）中代表视图源的形式参数。
        /// </summary>
        private readonly ParameterExpression _sourceParameter;

        /// <summary>
        ///     用于存储锚点与视图元素之间关联的字典，其中键为锚点，值为元素集合。
        /// </summary>
        private Dictionary<AssociationTreeNode, TypeElement[]> _anchorElements;

        /// <summary>
        ///     执行极限分解后的基础视图。
        /// </summary>
        private TypeView _baseView;

        /// <summary>
        ///     是否已分解
        /// </summary>
        private bool _decomposed;

        /// <summary>
        ///     平展鍵
        /// </summary>
        private ViewAttribute[] _flatteningKey;

        /// <summary>
        ///     平展点。
        ///     在源扩展树中，如果某个节点代表的元素具有多重性（IsMultiple == true），如果指定在此节点上平展，那么在最终生成的视图实例集中，该元素的属主对象将被复制多份，分别引用该元素值集合中的一个。该节点称为平展点。
        /// </summary>
        private List<ViewFlatteningPoint> _flatteningPoints;

        /// <summary>
        ///     视图的标识属性，各属性值的组合可以唯一标识一个视图实例。标识属性是顺序敏感的。
        /// </summary>
        private string[] _keyAttributes;


        /// <summary>
        ///     标识成员
        /// </summary>
        private List<string> _keyField;

        /// <summary>
        ///     标识成员的名称序列
        /// </summary>
        private List<string> _keyMemberNames;

        /// <summary>
        ///     形参绑定，即作为参数值来源的表达式。
        /// </summary>
        private List<ParameterBinding> _parameterBindings;

        /// <summary>
        ///     源扩展。
        /// </summary>
        private AssociationTree _sourceExtension;

        /// <summary>
        ///     目标名称
        /// </summary>
        private string _targetName;

        /// <summary>
        ///     创建TypeView实例。
        /// </summary>
        /// <param name="source">视图源。</param>
        /// <param name="clrType">视图的CLR类型。</param>
        /// <param name="sourcePara">在表达式中代表视图源的形式参数。</param>
        public TypeView(StructuralType source, Type clrType, ParameterExpression sourcePara)
            : base(clrType)
        {
            _source = source;
            _sourceParameter = sourcePara;
            if (source is ReferringType referringType) _sourceExtension = new AssociationTree(referringType);
        }

        /// <summary>
        ///     创建基于指定源扩展树的TypeView实例。
        /// </summary>
        /// <param name="sourceExtension">源扩展树</param>
        /// 实施说明:
        /// 以指定源扩展树的根节点所代表的类型作为视图源，创建一个ParameterExpression实例作为源形参。
        public TypeView(AssociationTree sourceExtension)
        {
            _source = sourceExtension.Root.RepresentedType;
            _sourceParameter = Expression.Parameter(sourceExtension.Root.RepresentedType.ClrType);
            _sourceExtension = sourceExtension;
        }

        /// <summary>
        ///     获取所有视图引用。
        /// </summary>
        public ViewReference[] ViewReferences =>
            Elements?.Where(p => p.GetType() == typeof(ViewReference)).Select(p => (ViewReference)p).ToArray() ??
            Array.Empty<ViewReference>();

        /// <summary>
        ///     获取视图源。
        /// </summary>
        public StructuralType Source => _source;

        /// <summary>
        ///     获取视图扩展。
        /// </summary>
        public AssociationTree Extension
        {
            get => _sourceExtension;
            internal set => _sourceExtension = value;
        }

        /// <summary>
        ///     获取视图的平展鍵。如果视图没有平展点或者未定义平展属性，均返回null。
        /// </summary>
        public ViewAttribute[] FlatteningKey => _flatteningKey;

        /// <summary>
        ///     获取所有平展点。
        /// </summary>
        public AssociationTreeNode[] FlatteningPoints => _flatteningPoints?.Select(p => p.ExtensionNode).ToArray() ??
                                                         Array.Empty<AssociationTreeNode>();

        /// <summary>
        ///     获取或设置形参绑定。
        /// </summary>
        public ParameterBinding[] ParameterBindings
        {
            get => _parameterBindings?.ToArray() ?? Array.Empty<ParameterBinding>();
            set => _parameterBindings = new List<ParameterBinding>(value);
        }

        /// <summary>
        ///     此视图是否为极限分解的结果
        /// </summary>
        public bool IsDecomposeExtremelyResult { get; internal set; }

        /// <summary>
        ///     获取或设置标识属性。
        /// </summary>
        public string[] KeyAttributes
        {
            get => _keyAttributes;
            set
            {
                //执行以下操作时清空寄存器：
                //（1）设置标识属性（KeyAttributes）。
                //（2）添加平展点（AddFlatteningPoint）。
                _keyField = null;
                _keyMemberNames = null;
                _keyAttributes = value;
            }
        }

        /// <summary>
        ///     获取在表达式（如视图属性的绑定表达式）中代表视图源的形式参数。
        /// </summary>
        public ParameterExpression SourceParameter => _sourceParameter;

        /// <summary>
        ///     获取标识成员的映射目标序列。
        /// </summary>
        public List<string> KeyFields
        {
            get
            {
                //已设置/已生成标识成员 则返回值
                if (_keyField != null && _keyField.Count > 0)
                    return _keyField;
                //锁后生成
                lock (_lockObject)
                {
                    GenerateKey(out _);
                    return _keyField;
                }
            }
            set => _keyField = value;
        }

        /// <summary>
        ///     获取映射目标名称。
        /// </summary>
        public string TargetName
        {
            get => _targetName ?? _source.Name;
            set => _targetName = value;
        }

        /// <summary>
        ///     获取标识成员的名称序列。
        /// </summary>
        public string[] KeyMemberNames
        {
            get
            {
                //是否已生成
                if (_keyMemberNames != null && _keyMemberNames.Count > 0)
                    return _keyMemberNames.ToArray();
                lock (_lockObject)
                {
                    //生成标识和标识成员
                    _keyMemberNames = GenerateKey(out _).ToList();
                    return _keyMemberNames?.ToArray();
                }
            }
        }

        /// <summary>
        ///     获取对视图实施极限分解得到的基础视图。
        ///     警告
        ///     不会检测视图是否为异构，对于同构视图，将生成其副本作为基础视图。强烈建议调用前实施异构性检测。
        /// </summary>
        public TypeView GetBaseView(HeterogeneityPredicationProvider predicationProvider = null)
        {
            if (_baseView == null)
                //锁后分解
                lock (_lockObject)
                {
                    DecomposeExtremely(predicationProvider);
                }

            return _baseView;
        }

        /// <summary>
        ///     获取一个值，该值指示视图是否是异构的。
        /// </summary>
        /// 实施说明:
        /// 使用AssociationTreeHeterogeneityPredicater判定视图的源扩展树是否为异构，如果是则视图为异构，反之亦反。
        /// 寄存判定结果，避免重复判定。
        public bool Heterogeneous(HeterogeneityPredicationProvider predicationProvider = null)
        {
            lock (_lockObject)
            {
                //如果当前为已分解的视图 不再分解
                if (IsDecomposeExtremelyResult)
                    return false;
                //没有异构提供器 使用默认的
                if (predicationProvider == null)
                    predicationProvider = new StorageHeterogeneityPredicationProvider();
                //进行分解
                var predicater = new AssociationTreeHeterogeneityPredicater(predicationProvider);
                _sourceExtension.Accept(predicater);
                return predicater.Result;
            }
        }

        /// <summary>
        ///     为视图添加元素。
        /// </summary>
        /// <param name="element">要添加的视图元素。</param>
        public override void AddElement(TypeElement element)
        {
            lock (_lockObject)
            {
                base.AddElement(element);
                if (_anchorElements == null)
                    _anchorElements = new Dictionary<AssociationTreeNode, TypeElement[]>();
                //视图属性
                if (element is ViewAttribute viewAttribute)
                {
                    foreach (var source in viewAttribute.Sources)
                    {
                        var anchor = source.ExtensionNode;
                        var elements = new List<TypeElement>();
                        //从源扩展节点根据锚点取
                        if (anchor != null && _anchorElements.TryGetValue(anchor, out var anchorElement))
                            elements = anchorElement.ToList();
                        elements.Add(element);
                        if (anchor != null)
                            _anchorElements[anchor] = elements.ToArray();
                    }
                }
                //视图复杂属性
                else if (element is ViewComplexAttribute viewComplexAttribute)
                {
                    var anchor = viewComplexAttribute.Anchor;
                    var elements = new List<TypeElement>();
                    //从复杂属性的锚点取
                    if (anchor != null && _anchorElements.TryGetValue(anchor, out var anchorElement))
                        elements = anchorElement.ToList();
                    elements.Add(element);
                    if (anchor != null)
                        _anchorElements[anchor] = elements.ToArray();
                }
                //视图引用
                else if (element is ViewReference viewReference)
                {
                    var anchor = viewReference.Anchor;
                    var elements = new List<TypeElement>();
                    //从视图引用的锚点取
                    if (anchor != null && _anchorElements.TryGetValue(anchor, out var anchorElement))
                        elements = anchorElement.ToList();
                    elements.Add(element);
                    if (anchor != null)
                        _anchorElements[anchor] = elements.ToArray();
                }
            }
        }

        /// <summary>
        ///     为视图添加元素。
        /// </summary>
        /// <param name="elements">要添加的视图元素。</param>
        public void AddElement(TypeElement[] elements)
        {
            foreach (var element in elements)
                AddElement(element);
        }

        /// <summary>
        ///     添加平展点。
        /// </summary>
        /// <param name="extensionNode">源扩展树上的节点，在此节点上实施扩展。</param>
        /// <param name="ensureKey">指示是否确保定义平展鍵。</param>
        public void AddFlatteningPoint(AssociationTreeNode extensionNode, bool ensureKey = false)
        {
            //构造一个p=>p.RepresentedType.RebuildingType的表达式
            var flatteningPara = Expression.Parameter(extensionNode.RepresentedType.RebuildingType);
            var generator = new AssociationExpressionGenerator(_sourceParameter);
            //根据扩展点生成Lambda表达式
            var lambda = extensionNode.AsTree().Accept(generator);
            //生成出来 加入
            if (lambda != null)
                AddParameterBinding(flatteningPara, lambda, EParameterReferring.Single);
            //加入扩展节点
            AddFlatteningPoint(extensionNode, flatteningPara, ensureKey);
        }

        /// <summary>
        ///     添加平展点。
        /// </summary>
        /// <param name="extensionNode">源扩展树上的节点，在此节点上实施扩展。</param>
        /// <param name="flatteningPara">平展形参。</param>
        /// <param name="ensureKey">指示是否确保定义平展鍵。</param>
        public void AddFlatteningPoint(AssociationTreeNode extensionNode, ParameterExpression flatteningPara,
            bool ensureKey = false)
        {
            lock (_lockObject)
            {
                //前置条件：
                //1.根节点不能作为平展点；
                //2.节点代表的引用不是多重引用的，不能作为平展点
                var tree = extensionNode.AsTree();
                //根节点不能作为平展点；
                if (tree.IsRoot) return;
                //节点代表的引用不是多重引用的，不能作为平展点
                if (!(extensionNode is ObjectTypeNode objectTypeNode) || !(objectTypeNode.Element?.IsMultiple ?? false))
                    return;
                if (_flatteningPoints == null) _flatteningPoints = new List<ViewFlatteningPoint>();
                //已存在。
                if (_flatteningPoints.Any(p => p.ExtensionNode.Equals(extensionNode))) return;
                var flatteningPoint = new ViewFlatteningPoint(extensionNode, flatteningPara);
                _flatteningPoints.Add(flatteningPoint);
                //不确保定义平展键，则直接结束。
                if (!ensureKey) return;
                var nodeType = extensionNode.RepresentedType;
                var keyAttrs = Array.Empty<Attribute>();
                //根据不同的结构化类型获取键
                if (nodeType is EntityType entityType)
                    keyAttrs = entityType.GetKey();
                else if (nodeType is AssociationType associationType)
                    keyAttrs = associationType.AssociationEnds.SelectMany(p => p.GetForeignKey()).ToArray();
                //确保直观属性
                var items = EnsureIntuitive(keyAttrs, extensionNode);
                //添加平展点
                var tp = _flatteningKey?.ToList() ?? new List<ViewAttribute>();
                tp.AddRange(items);
                _flatteningKey = tp.ToArray();

                //执行以下操作时清空寄存器：
                //（1）设置标识属性（KeyAttributes）。
                //（2）添加平展点（AddFlatteningPoint）。
                _keyField = null;
                _keyMemberNames = null;
            }
        }

        /// <summary>
        ///     添加形参绑定。
        /// </summary>
        /// <param name="parameter">形参。</param>
        /// <param name="expression">绑定目标。</param>
        /// <param name="referring">形参指代。</param>
        public void AddParameterBinding(ParameterExpression parameter, Expression expression,
            EParameterReferring referring)
        {
            lock (_lockObject)
            {
                if (_parameterBindings == null)
                    _parameterBindings = new List<ParameterBinding>();
                _parameterBindings.Add(new ParameterBinding(parameter, referring, expression));
            }
        }

        /// <summary>
        ///     添加形参绑定。
        /// </summary>
        /// <param name="paraBindings">待添加的形参绑定集。</param>
        public void AddParameterBinding(ParameterBinding[] paraBindings)
        {
            lock (_lockObject)
            {
                if (_parameterBindings == null)
                    _parameterBindings = new List<ParameterBinding>();
                _parameterBindings.AddRange(paraBindings);
            }
        }

        /// <summary>
        ///     添加形参绑定。
        /// </summary>
        /// <param name="paraBinding">待添加的形参绑定实例。</param>
        public void AddParameterBinding(ParameterBinding paraBinding)
        {
            lock (_lockObject)
            {
                if (_parameterBindings == null)
                    _parameterBindings = new List<ParameterBinding>();
                _parameterBindings.Add(paraBinding);
            }
        }

        /// <summary>
        ///     获取形参绑定。
        /// </summary>
        /// <param name="parameter">要获取其绑定的形式参数。</param>
        /// <param name="referring">形参指代。</param>
        public Expression GetParameterBinding(ParameterExpression parameter, out EParameterReferring referring)
        {
            lock (_lockObject)
            {
                if (_parameterBindings == null)
                    _parameterBindings = new List<ParameterBinding>();
                //查找值
                var binding = _parameterBindings.FirstOrDefault(p => p.Parameter == parameter);
                //有值赋值 否则返回single
                referring = binding?.Referring ?? EParameterReferring.Single;
                return binding?.Expression;
            }
        }

        /// <summary>
        ///     生成视图的标识成员，同时生成标识成员对应的映射目标。
        /// </summary>
        /// <param name="keyFields">返回标识成员的映射目标序列。</param>
        /// 实施说明
        /// 寄存生成结果避免重复执行生成逻辑。
        /// 执行以下操作时清空寄存器：
        /// （1）设置标识属性（KeyAttributes）。
        /// （2）添加平展点（AddFlatteningPoint）。
        private string[] GenerateKey(out string[] keyFields)
        {
            //已经寄存 就不再生成
            if (_keyField?.Count > 0 && _keyMemberNames?.Count > 0)
            {
                keyFields = _keyField.ToArray();
                return _keyMemberNames.ToArray();
            }

            if (_source is IMappable mappableSource)
            {
                //键成员名称序列
                var keyMemberNameList = mappableSource.KeyMemberNames.ToList();
                //映射目标序列
                var keyFieldList = mappableSource.KeyFields;

                //平展点转成关联树
                var trees = _flatteningPoints?.Select(p => p.ExtensionNode.AsTree()).ToArray();
                foreach (var tree in trees ?? Array.Empty<AssociationTree>())
                {
                    tree.Accept(_aliasGenerator);
                    //节点别名
                    var nodeAlias = _aliasGenerator.Result;
                    //分别加入
                    foreach (var member in mappableSource.KeyMemberNames)
                        keyMemberNameList.Add($"{nodeAlias}_{member}");
                    foreach (var field in mappableSource.KeyFields) keyFieldList.Add($"{nodeAlias}_{field}");
                }

                //寄存
                _keyField = keyFieldList;
                _keyMemberNames = keyMemberNameList;

                keyFields = keyFieldList.ToArray();
                return keyMemberNameList.ToArray();
            }

            _keyMemberNames = new List<string>();
            keyFields = Array.Empty<string>();
            _keyField = new List<string>();
            return Array.Empty<string>();
        }

        /// <summary>
        ///     完整性检查
        ///     继承类需要检查则重写此方法
        /// </summary>
        /// <param name="errDictionary">错误信息字典</param>
        public override void IntegrityCheck(Dictionary<string, List<string>> errDictionary)
        {
            //Nothing
        }

        /// <summary>
        ///     生成视图的嵌套堆栈，最外层视图位于堆栈底部，最内层视图位于顶部。
        /// </summary>
        public Stack<TypeView> GetNestingStack()
        {
            var currentView = this;
            var nestinView = new Stack<TypeView>();
            //把每个视图压入堆栈，直到没有源视图为止。
            while (currentView != null)
            {
                nestinView.Push(this);
                currentView = currentView.Source as TypeView;
            }

            return nestinView;
        }

        /// <summary>
        ///     生成视图的CLR类型，并为视图绑定实例构造器，为视图元素绑定取值器和设值器。
        /// </summary>
        /// 实施说明:
        /// 第一步，生成隐含类型。
        /// 调用方法ImpliedTypeManager.ApplyType(fields, subIdentity)，其中：
        /// （1）fields根据视图元素生成，不需要指定ValueExpression；
        /// （2）subIdentity由两个元素组成，第一个为视图源的FullName，第二个当前时间。
        /// 第二步，为视图绑定实例构造器。
        /// 生成一个lambda表达式，该表达式没有形参，主体为调用第一步生成类型的无参构造函数。然后将该lambda表达式编译成委托，创建委托构造器。
        /// 第三步，为视图元素绑定设值器和取值器。
        /// 对每一个视图元素，生成一个lambda表达式，其形参为第一步生成的类型，主体为一个表示字段访问的MemberExpression。然后将该lambda表达式编译成委托，创建委托取值器。按类似的方式创建委托设值器，其中lambda表达式的主体为表示赋值运算的Binary表达式，该表达式的左操作数为表示字段访问的MemberExpression。
        public void GenerateType()
        {
            //生成隐含类型
            var fields = new List<FieldDescriptor>();
            var elements = _anchorElements.SelectMany(p => p.Value);
            foreach (var element in elements)
            {
                //根据视图元素生成字段描述符
                Type type = null;
                if (element is ViewAttribute viewAttribute)
                    type = viewAttribute.DataType;
                else if (element is ViewComplexAttribute viewComplexAttribute)
                    type = viewComplexAttribute.DataType;
                else if (element is ViewReference viewReference)
                    type = viewReference.ValueType.ClrType;
                //其他的类型
                if (type == null)
                    throw new ArgumentException("获取视图元素类型失败。");
                fields.Add(new FieldDescriptor(type, element.Name));
            }

            var subIdentity = new IdentityArray(_source.FullName, DateTime.Now);

            if (_clrType == null)
                _clrType = ImpliedTypeManager.Current.ApplyType(fields.ToArray(), subIdentity);

            //为视图绑定实例构造器
            var methodinfo = typeof(Activator).GetMethod("CreateInstance", new[] { typeof(Type) });
            if (methodinfo != null)
            {
                var callMethodExpression = Expression.Call(methodinfo, Expression.Constant(_clrType));
                var constructorExpression = Expression.Lambda(Expression.Convert(callMethodExpression, _clrType));
                _constructor = (IInstanceConstructor)Activator.CreateInstance(
                    typeof(DelegateConstructor<>).MakeGenericType(_clrType), constructorExpression.Compile());
            }
            else
            {
                throw new ArgumentException("Activator无法获取到CreateInstance方法.");
            }

            //为视图元素绑定设值器和取值器
            var obj = Expression.Parameter(_clrType, "obj");
            foreach (var field in fields)
            {
                if (!_elements.TryGetValue(field.Name, out var element)) continue;
                //属性
                var fieldInfo = _clrType.GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo == null)
                    throw new ArgumentException($"视图元素{field.Name}在CLR类型中未找到对应的字段。");

                //ValueGetter
                var fieldGetLambda = Expression.Lambda(Expression.MakeMemberAccess(obj, fieldInfo), obj);
                var valueGetter = (IValueGetter)Activator.CreateInstance(
                    typeof(DelegateValueGetter<,>).MakeGenericType(_clrType, field.Type), fieldGetLambda.Compile());
                element.ValueGetter = valueGetter;

                //ValueSetter
                var val = Expression.Parameter(field.Type, "val");
                var body = Expression.MakeBinary(ExpressionType.Assign, Expression.MakeMemberAccess(obj, fieldInfo),
                    val);
                var fieldSetLambda =
                    Expression.Lambda(typeof(Action<,>).MakeGenericType(obj.Type, val.Type), body, obj, val);

                var valueSetter = (IValueSetter)Activator.CreateInstance(
                    typeof(DelegateValueSetter<,>).MakeGenericType(_clrType, field.Type), fieldSetLambda.Compile(),
                    EValueSettingMode.Assignment);
                element.ValueSetter = valueSetter;
            }
        }

        /// <summary>
        ///     获取对异构视图实施极限分解后得到的附加视图及相应的附加引用和附加点。
        /// </summary>
        /// <returns>
        ///     包含附加视图及其附加点、附加引用的附加项集合。
        ///     警告
        ///     本方法不会检测视图是否为异构，对于同构视图，将生成其副本作为基础视图，然后返回null。强烈建议调用前实施异构性检测。
        /// </returns>
        public TypeViewAttachingItem[] GetAttachedViews(
            HeterogeneityPredicationProvider heterogeneityPredicationProvider = null)
        {
            //没有异构断言提供器
            if (heterogeneityPredicationProvider == null)
                heterogeneityPredicationProvider = new StorageHeterogeneityPredicationProvider();
            //没有分解过 进行分解
            if (_decomposed == false)
                DecomposeExtremely(heterogeneityPredicationProvider);
            return _attachingItems?.ToArray();
        }

        /// <summary>
        ///     获取锚定于指定扩展节点的元素，返回的元素中不包含非直观属性。
        /// </summary>
        /// <param name="anchor">锚点。</param>
        /// 实施说明
        /// 从锚点元素字典中查询，不要遍历元素线性序列。
        public TypeElement[] GetElements(AssociationTreeNode anchor)
        {
            if (_anchorElements == null)
                _anchorElements = new Dictionary<AssociationTreeNode, TypeElement[]>();
            //如果锚点元素字典中没有锚点，则返回空数组。
            return _anchorElements.TryGetValue(anchor, out var element) ? element : Array.Empty<TypeElement>();
        }

        /// <summary>
        ///     统计锚定于指定扩展节点的元素个数，不计算非直观属性。
        /// </summary>
        /// <param name="anchor">锚点。</param>
        /// 实施说明:
        /// 从锚点元素字典中查询，不要遍历元素线性序列。
        /// <returns></returns>
        public int CountElements(AssociationTreeNode anchor)
        {
            if (_anchorElements == null)
                _anchorElements = new Dictionary<AssociationTreeNode, TypeElement[]>();
            //如果锚点元素字典中没有锚点，则返回0。
            return _anchorElements.TryGetValue(anchor, out var element) ? element.Length : 0;
        }

        /// <summary>
        ///     根据指定的属性源搜索直观属性。
        /// </summary>
        /// <param name="attribute">构成属性源的属性，须为顶级属性，不接受子属性。</param>
        /// <param name="extensionNode">构成属性源的扩展树节点，未指定表示根节点。</param>
        public ViewAttribute GetIntuitiveAttribute(Attribute attribute, AssociationTreeNode extensionNode = null)
        {
            var elements = extensionNode == null ? _elements.Values.ToArray() : GetElements(extensionNode);
            foreach (var element in elements)
                //视图属性
                if (element is ViewAttribute viewAttribute)
                {
                    if (!viewAttribute.IsIntuitive) continue;
                    var attrNode = viewAttribute.Sources[0].AttributeNode;
                    if (attrNode.Parent != null || !attribute.Equals(attrNode.Attribute)) continue;
                    return viewAttribute;
                }
                //视图引用
                else if (element is ViewReference)
                {
                    return (ViewAttribute)_elements?.Values.FirstOrDefault(p =>
                        p is ViewAttribute viewAttre && viewAttre.Name == attribute.Name);
                }

            return null;
        }

        /// <summary>
        ///     对视图实施极限分解。
        /// </summary>
        /// 实施说明:
        /// 如果已执行过极限分解（定义一个寄存器），则不再执行，直接返回。
        private void DecomposeExtremely(HeterogeneityPredicationProvider heterogeneityPredicationProvider = null)
        {
            if (_decomposed) return;
            if (heterogeneityPredicationProvider == null)
                heterogeneityPredicationProvider = new StorageHeterogeneityPredicationProvider();
            var visitor = new AssociationTreeDecomposer(heterogeneityPredicationProvider);
            var baseTree = _sourceExtension.Accept(visitor, false);
            TypeView baseView;
            if (visitor.OutArgument == null)
            {
                baseView = this;
            }
            else
            {
                baseView = new TypeView(baseTree) { IsDecomposeExtremelyResult = true };

                foreach (var item in visitor.OutArgument)
                {
                    //创建附加视图
                    var attachingView = new TypeView(item.AttachingTree) { IsDecomposeExtremelyResult = true };
                    //确保附加引用
                    var vr = baseView.EnsureReference(item.AttachingReference, item.AttachingNode);
                    //确保参考键
                    baseView.EnsureReferredKey(vr);
                    //获取附加引用的引用键
                    var refKeys = item.AttachingReference.GetReferringKey(true);
                    //确保绑定到引用属性的直观属性
                    attachingView.EnsureIntuitive(refKeys);
                    //添加到视图附加项
                    _attachingItems.Add(new TypeViewAttachingItem(attachingView, item.AttachingNode, vr));
                }

                var ea = new ElementAdder(this, baseView, _attachingItems.ToArray());
                //为基础视图和附加视图定义元素 
                _sourceExtension.Accept(ea);
                foreach (var typElementsValue in _elements.Values)
                    if (typElementsValue is ViewAttribute viewAttribute && viewAttribute.Shadow == null)
                        baseView.AddElement(viewAttribute);
                //生成基础视图的CLR类型并添加
                baseView.GenerateType();
                Model.AddType(baseView);
                //生成附加视图的CLR类型并添加
                _attachingItems.ForEach(p => p.AttachingView.GenerateType());
                _attachingItems.ForEach(p => Model.AddType(p.AttachingView));
            }

            _baseView = baseView;

            _decomposed = true;
        }

        /// <summary>
        ///     确保视图已在指定属性源上定义了直观属性。
        /// </summary>
        /// <returns>返回以该属性为源的直观属性，可能是新定义的，也可能是已存在的。</returns>
        /// <param name="attribute">构成属性源的属性，须为顶级属性，不接受子属性。</param>
        /// <param name="extensionNode">构成属性源的扩展树节点，未指定表示根节点。</param>
        /// <param name="proposedName">定义属性时推荐使用的名称。</param>
        /// 实施说明:
        /// 添加属性时要解决名称冲突。
        public ViewAttribute EnsureIntuitive(Attribute attribute, AssociationTreeNode extensionNode = null,
            string proposedName = null)
        {
            return EnsureIntuitive(new SimpleAttributeNode(attribute), extensionNode, proposedName);
        }

        /// <summary>
        ///     确保视图已在指定属性源上定义了直观属性。
        /// </summary>
        /// <returns>返回以该属性为源的直观属性，可能是新定义的，也可能是已存在的。</returns>
        /// <param name="attribute">构成属性源的属性，须为顶级属性（每个属性与扩展树节点构成一个源），不接受子属性。</param>
        /// <param name="extensionNode">构成属性源的扩展树节点，未指定表示根节点。</param>
        /// 实施说明:
        /// 定义属性时要解决名称冲突。
        public ViewAttribute[] EnsureIntuitive(Attribute[] attribute, AssociationTreeNode extensionNode = null)
        {
            var attributes = new List<ViewAttribute>();
            foreach (var item in attribute)
            {
                //每个属性都确保直观属性
                var attr = EnsureIntuitive(item, extensionNode);
                attributes.Add(attr);
            }

            return attributes.ToArray();
        }

        /// <summary>
        ///     确保视图已在指定属性源上定义了直观属性。
        /// </summary>
        /// <returns>返回以该属性为源的直观属性，可能是新定义的，也可能是已存在的。</returns>
        /// <param name="attributeNode">构成属性源的属性树节点。</param>
        /// <param name="extensionNode">构成属性源的扩展树节点，未指定表示根节点。</param>
        /// <param name="proposedName">定义属性时推荐使用的名称。</param>
        /// 实施说明:
        /// 添加属性时要解决名称冲突。
        public ViewAttribute EnsureIntuitive(AttributeTreeNode attributeNode, AssociationTreeNode extensionNode = null,
            string proposedName = null)
        {
            if (extensionNode != null)
            {
                var elements = GetElements(extensionNode);
                foreach (var element in elements)
                    if (element is ViewAttribute viewAttribute)
                    {
                        if (!viewAttribute.IsIntuitive) continue;
                        if (attributeNode.Equals(viewAttribute.Sources[0].AttributeNode)) return viewAttribute; //返回已存在的
                    }
            }

            //新定义
            if (proposedName == null) proposedName = attributeNode.AttributeName;
            //解决名称冲突
            var name = NameNew(proposedName);
            var newViewAttribute = new ViewAttribute(name, attributeNode, extensionNode);
            AddElement(newViewAttribute);
            return newViewAttribute;
        }

        /// <summary>
        ///     确保视图已在指定源上定义了视图引用。
        /// </summary>
        /// <returns>返回定义在指定源上的视图引用，可能是新定义的，也可能是已存在的。</returns>
        /// <param name="binding">构成视图引用源的引用元素，也称为视图引用的绑定。</param>
        /// <param name="anchor">构成视图引用源的扩展树节点，也称为视图引用的锚点，未指定表示根节点。</param>
        /// <param name="proposedName">定义视图引用时推荐使用的名称。</param>
        /// 实施说明:
        /// 添加引用时要解决名称冲突。
        public ViewReference EnsureReference(ReferenceElement binding, AssociationTreeNode anchor = null,
            string proposedName = null)
        {
            var elements = GetElements(anchor);
            foreach (var element in elements)
                if (element is ViewReference viewReference)
                    //返回已存在的
                    if (binding.Equals(viewReference.Binding))
                        return viewReference;
            //新定义
            if (proposedName == null) proposedName = binding.Name;
            //解决名称冲突
            var name = NameNew(proposedName);
            var newViewReference = new ViewReference(binding, name, anchor);
            //添加元素
            AddElement(newViewReference);
            return newViewReference;
        }

        /// <summary>
        ///     确保已在视图上定义了指定视图引用的参考鍵。
        /// </summary>
        /// <param name="viewRef">视图引用。</param>
        public ViewAttribute[] EnsureReferredKey(ViewReference viewRef)
        {
            var binding = viewRef.Binding;
            Attribute[] attrs;
            AssociationTreeNode treeNode = null;
            //如果是视图引用
            if (binding.ElementType == EElementType.ViewReference)
            {
                var anchor = viewRef.Anchor;
                //使用锚点的类型视图确保参考键
                attrs = ((TypeView)anchor.RepresentedType).EnsureReferredKey(viewRef);
                treeNode = anchor;
            }
            else
            {
                //获取参考键即可
                attrs = viewRef.GetReferringKey(true);
            }

            return EnsureIntuitive(attrs, treeNode);
        }

        /// <summary>
        ///     根据指定的基础视图实例和附加视图实例生成异构视图实例。
        /// </summary>
        /// <returns>生成的异构视图实例序列。</returns>
        /// <param name="baseInstances">基础视图实例的序列，每一个基础视图实例对应生成一个异构视图实例。</param>
        /// <param name="attachingInstanceSets">
        ///     附加视图实例集的集合，每一个实例集与一个附加视图对应；实例集内部包含一个或多个视图实例，具体取决于附加
        ///     引用的重数。
        /// </param>
        public object[] Instantiate(object[] baseInstances, params AttachingInstanceSet[] attachingInstanceSets)
        {
            var resultObjs = new List<object>();

            var units = baseInstances.Select(p => new InstantiationUnit(p, _baseView)).ToList();
            //遍历附加实例集
            foreach (var item in attachingInstanceSets ?? Array.Empty<AttachingInstanceSet>())
            {
                //根据平展键分组。
                var groupSets = item.GroupByFlatteningKey();

                var newUnits = new List<InstantiationUnit>(groupSets.Length * units.Count);
                //遍历实例单元
                foreach (var unit in units)
                {
                    var count = groupSets.Count();
                    //克隆实例化单元
                    var unitClones = unit.Clone(count);
                    //遍历分组
                    for (var i = 0; i < count; i++)
                    {
                        var unitClone = unitClones[i];
                        //向实例化单元添加附加实例。
                        unitClone.AddAttachingInstance(groupSets[i]);
                        newUnits.Add(unitClone);
                    }
                }

                units = newUnits;
            }

            //遍历实例化
            foreach (var unit in units)
            {
                //元素取值
                object GetElementValue(TypeElement element)
                {
                    object result;
                    //视图属性
                    if (element is ViewAttribute viewAttribute && !viewAttribute.IsIntuitive)
                    {
                        var sourceValues = new List<object>();
                        foreach (var item in viewAttribute.Sources)
                        {
                            //根据源从单元里取值
                            var agentValue = unit.GetValue(item.Agent); //获取代理属性值
                            sourceValues.Add(agentValue);
                        }

                        result = viewAttribute.Evaluator.Evaluate(sourceValues.ToArray()); //属性求值
                    }
                    else
                    {
                        //从影子元素获取值
                        var shadow = ((ITypeViewElement)element).Shadow as TypeElement;
                        result = unit.GetValue(shadow ?? element);
                    }

                    return result;
                }

                resultObjs.Add(Instantiate(GetElementValue));
            }

            return resultObjs.ToArray();
        }

        /// <summary>
        ///     生成视图表达式。
        /// </summary>
        /// <param name="flatteningExpressions">返回平展表达式，无平展点返回null。</param>
        public LambdaExpression GenerateExpression(out LambdaExpression[] flatteningExpressions)
        {
            var parameters = _constructor?.Parameters ?? new List<Parameter>(); //获取构造参数；

            //获取代表指定平展点的形参(本地函数);
            ParameterExpression Flatting(AssociationTreeNode node)
            {
                return _flatteningPoints?.FirstOrDefault(fp => fp.ExtensionNode == node)?.FlatteningParameter;
            }

            var arguments = new List<Expression>();
            //为构造参数生成实例参数
            foreach (var parameter in parameters)
            {
                var ele = parameter.GetElement();
                //生成元素的绑定表达式
                var exp = (ele as ITypeViewElement)?.GenerateExpression(_sourceParameter, Flatting);
                if (exp is LambdaExpression lambdaExpression)
                    arguments.Add(lambdaExpression.Body);
                else
                    arguments.Add(exp);
            }

            var constructor = RebuildingType.GetConstructor(parameters.Select(p => p.GetType()).ToArray());
            if (constructor == null)
                throw new ArgumentException($"视图{RebuildingType.FullName}没有可用的构造函数.");
            Expression expression = Expression.New(constructor, arguments.ToArray());

            if (Elements.Count > parameters.Count)
            {
                var bindings = new List<MemberAssignment>();
                //为元素赋值
                foreach (var element in Elements)
                {
                    //检测是否存在关联到此元素的参数；
                    if (_constructor?.GetParameterByElement(element.Name) != null) continue;
                    Expression typeViewExp = null;
                    if (element is ITypeViewElement typeViewElement)
                        typeViewExp = typeViewElement.GenerateExpression(_sourceParameter, Flatting);

                    if (typeViewExp != null)
                    {
                        //仅有ViewReference生成的表达式是MemberExpression
                        if (typeViewExp is MemberExpression memberExpression)
                        {
                            //生成绑定表达式
                            var member = RebuildingType.GetMember(element.Name).First();
                            bindings.Add(Expression.Bind(member, memberExpression));
                        }
                        //其他几种视图元素
                        else if (typeViewExp is LambdaExpression exp)
                        {
                            //生成绑定表达式
                            var member = RebuildingType.GetMember(element.Name).FirstOrDefault();
                            if (member == null)
                                member = RebuildingType
                                    .GetMember(element.Name, BindingFlags.NonPublic | BindingFlags.Instance).First();
                            bindings.Add(Expression.Bind(member, exp.Body));
                        }
                    }
                }

                expression = Expression.MemberInit((NewExpression)expression, bindings);
            }

            var flattenings = new List<LambdaExpression>();

            //生成平展点的表达式
            foreach (var item in _flatteningPoints ?? new List<ViewFlatteningPoint>())
            {
                var exp = _parameterBindings.FirstOrDefault(p => p.Parameter == item.FlatteningParameter);
                if (exp == null) throw new ArgumentException("平展点,没有对应的形参绑定。");
                flattenings.Add(Expression.Lambda(exp.Expression, exp.Parameter));
            }

            flatteningExpressions = flattenings.ToArray();
            var allParams = new List<ParameterExpression> { _sourceParameter };
            allParams.AddRange(_flatteningPoints?.Select(p => p.FlatteningParameter) ??
                               new List<ParameterExpression>());
            return Expression.Lambda(expression, allParams);
        }

        /// <summary>
        ///     获取过滤键
        /// </summary>
        /// <returns></returns>
        public override Attribute[] GetFilterKey()
        {
            return _flatteningKey.Where(p => p != null).Cast<Attribute>().ToArray();
        }


        /// <summary>
        ///     为基础视图和附加视图添加元素，这些元素将作为被分解视图的元素的影子元素或属性源代理。
        /// </summary>
        private class ElementAdder : IAssociationTreeDownwardVisitor
        {
            /// <summary>
            ///     实施极限分解得到的附加视图及其附加节点和附加引用。
            /// </summary>
            private readonly TypeViewAttachingItem[] _attachingItems;

            /// <summary>
            ///     实施极限分解得到的基础视图。
            /// </summary>
            private readonly TypeView _baseView;

            /// <summary>
            ///     被分解的视图。
            /// </summary>
            private readonly TypeView _decomposedView;

            /// <summary>
            ///     创建ElementAdder实例。
            /// </summary>
            /// <param name="decomposedView">被分解的视图。</param>
            /// <param name="baseView">实施极限分解得到的基础视图。</param>
            /// <param name="attachingItems">实施极限分解得到的附加视图及其附加节点、附加引用。</param>
            public ElementAdder(TypeView decomposedView, TypeView baseView, TypeViewAttachingItem[] attachingItems)
            {
                _decomposedView = decomposedView;
                _baseView = baseView;
                _attachingItems = attachingItems;
            }

            /// <summary>
            ///     后置访问，即在访问子级后执行操作。
            /// </summary>
            /// <param name="subTree">被访问的关联树子树。</param>
            /// <param name="parentState">访问父级时产生的状态数据。</param>
            /// <param name="previsitState">前置访问产生的状态数据。</param>
            public void Postvisit(AssociationTree subTree, object parentState, object previsitState)
            {
            }

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
                var parentStateUnboxing = parentState == null ? Array.Empty<object>() : (object[])parentState;
                var targetView =
                    parentStateUnboxing.Length > 0 ? (TypeView)parentStateUnboxing[1] : _baseView; //从状态参数中取出目标视图
                //在目标视图（基础视图或附加视图）中定位节点
                AssociationTreeNode anchor;
                if (parentState != null)
                {
                    var parentNode = (AssociationTreeNode)parentStateUnboxing[0];
                    //获取子节点。
                    var currentNode = parentNode.GetChild(subTree.ElementName);
                    if (currentNode != null)
                    {
                        anchor = currentNode;
                    }
                    else
                    {
                        //查找引用元素。
                        var @ref = parentNode.RepresentedType.GetReferenceElement(subTree.ElementName);
                        var attchingItem = _attachingItems
                            .First(item => item.AttachingNode == parentNode && item.AttachingReference.Binding == @ref);
                        targetView = attchingItem.AttachingView;
                        anchor = attchingItem.AttachingView.Extension.Node;
                    }
                }
                else
                {
                    anchor = _baseView.Extension.Node;
                }

                //将原视图中锚定于当前节点的元素克隆到目标视图
                var elements = _decomposedView.GetElements(anchor);
                //获取锚定于当前节点的元素。
                foreach (var element in elements)
                    //根据各个元素的类型进行处理
                    if (element is SelfReference selfReference)
                    {
                        var attr = new SelfReference(selfReference.Name);
                        targetView.AddElement(element);
                        selfReference.Shadow = attr;
                    }
                    else if (element is ViewComplexAttribute complexAttribute)
                    {
                        var attr = new ViewComplexAttribute(element.Name, anchor, complexAttribute.Binding);
                        targetView.AddElement(element);
                        complexAttribute.Shadow = attr;
                    }
                    else if (element is ViewReference viewReference)
                    {
                        var rf = targetView.EnsureReference(viewReference.Binding, anchor);
                        viewReference.Shadow = rf;
                    }
                    else if (element is ViewAttribute viewAttribute)
                    {
                        //直观属性.
                        if (viewAttribute.IsIntuitive)
                        {
                            //确保定义直观属性。
                            var attr = targetView.EnsureIntuitive(viewAttribute, anchor);
                            viewAttribute.Shadow = attr;
                        }
                        else //非直观属性
                        {
                            var sources = viewAttribute.Sources.Where(item => item.ExtensionNode == subTree.Node)
                                .ToList();
                            foreach (var item in sources)
                            {
                                //确保定义直观属性。
                                var attr = targetView.EnsureIntuitive(item.AttributeNode, anchor);
                                item.Agent = attr;
                            }
                        }
                    }

                outParentState = new object[] { anchor, targetView };
                outPrevisitState = null;

                //参照原视图，在目标视图中设置平展点
                var flatteningPoints = _decomposedView.FlatteningPoints; //获取平展点。
                if (flatteningPoints.Contains(subTree.Node))
                    targetView.AddFlatteningPoint(anchor, true); //添加平展点。

                return true;
            }

            /// <summary>
            ///     重置访问者。
            /// </summary>
            public void Reset()
            {
            }
        }
    }
}