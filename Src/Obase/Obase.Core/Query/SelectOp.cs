/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Select运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:31:44
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Query.TypeViews;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Select运算。
    /// </summary>
    public class SelectOp : QueryOp
    {
        /// <summary>
        ///     退化路径。
        /// </summary>
        private AtrophyPath _atrophyPath;

        /// <summary>
        ///     应用于每个元素的投影函数。
        /// </summary>
        private LambdaExpression _resultSelector;

        /// <summary>
        ///     投影结果视图。
        /// </summary>
        private TypeView _resultView;

        /// <summary>
        ///     创建SelectOp实例。
        /// </summary>
        /// <param name="resultSelector">应用于每个元素的投影函数。</param>
        /// <param name="model"></param>
        internal SelectOp(LambdaExpression resultSelector, ObjectDataModel model)
            : base(EQueryOpName.Select, resultSelector.Parameters[0].Type)
        {
            _resultSelector = resultSelector;
            _model = model;
        }

        /// <summary>
        ///     创建表示一般投影运算的SelectOp实例。
        /// </summary>
        /// <param name="resultView">投影结果视图。</param>
        /// <param name="model"></param>
        internal SelectOp(TypeView resultView, ObjectDataModel model)
            : this(resultView.GenerateExpression(out _), model)
        {
            _resultView = resultView;
        }

        /// <summary>
        ///     创建表示退化投影运算的SelectOp实例。
        /// </summary>
        /// <param name="atrophyPath">退化路径。</param>
        /// <param name="model"></param>
        internal SelectOp(AtrophyPath atrophyPath, ObjectDataModel model)
            : this(atrophyPath.GenerateExpression(out _), model)
        {
            _atrophyPath = atrophyPath;
        }

        /// <summary>
        ///     获取退化路径，（仅适用于退化投影运算）。
        /// </summary>
        /// 实施说明:
        /// 如果初始化期间未设置退化路径，则从投影函数解析，（使用AtrophyPath.FromSelector方法，如果是实例化投影则返回null）。
        /// 应当保证在对象生命周期内最多执行一次解析操作。
        public AtrophyPath AtrophyPath
        {
            get
            {
                if (IsNew)
                    return null;

                if (_atrophyPath != null) return _atrophyPath;
                _atrophyPath = AtrophyPath.FromExpression(Model, _resultSelector);
                return _atrophyPath;
            }
        }

        /// <summary>
        ///     获取一个值，该值指示投影运算是否将元素在序列中的索引作为（第二个）参数。
        ///     实施说明
        ///     对于普通投影运算，依据结果投影函数判定；
        ///     对于集合中介投影，依据中介投影函数判定。
        /// </summary>
        public virtual bool IndexReferred
        {
            get
            {
                var indexReferred = false;
                var parameters = _resultSelector?.Parameters;
                //获取索引 并且索引是Int
                if (parameters != null && parameters.Count == 2 && parameters[1]?.Type == typeof(int))
                    indexReferred = true;
                return indexReferred;
            }
        }

        /// <summary>
        ///     获取一个值，该值指示投影运算是否为多重投影。
        ///     多重投影是指投影到一个具有多重性的引用元素或其下级元素（下级元素不要求多重性）的运算。
        ///     下级元素是指关联树中代表当前元素的节点的后代所代表的元素，或者是当前节点或其后代所含属性树节点所代表的属性。
        /// </summary>
        public virtual bool IsMultiple => ResultType.GetInterface("IEnumerable") != null;

        /// <summary>
        ///     获取一个值，该值指示投影运算是否为实例化投影。
        ///     实例化投影运算是指投影函数的Body为New或MemberInit表达式的投影运算。
        /// </summary>
        public virtual bool IsNew
        {
            get
            {
                if (_resultSelector == null) return false;
                // 判定投影函数的Body是否为New或MemberInit表达式
                return _resultSelector.Body.NodeType == ExpressionType.New ||
                       _resultSelector.Body.NodeType == ExpressionType.MemberInit;
            }
        }

        /// <summary>
        ///     获取应用于每个元素的投影函数。
        /// </summary>
        /// 实施说明:
        /// 如果初始化期间未设置投影函数，则根据退化路径或视图生成：
        /// 如果是退化投影（IsNew==false），调用AtrophyPath.GenerateExpression方法生成；
        /// 如果是一般投影（IsNew==true），调用TypeView.GenerateExpression方法生成。
        public LambdaExpression ResultSelector
        {
            get
            {
                if (_resultSelector != null) return _resultSelector;
                // 如果是退化投影，使用AtrophyPath生成投影函数
                // 如果是一般投影，使用ResultView生成投影函数
                _resultSelector = IsNew ? ResultView?.GenerateExpression(out _) : AtrophyPath.GenerateExpression(out _);
                return _resultSelector;
            }
        }

        /// <summary>
        ///     获取投影结果视图，（仅适用于一般投影运算）。
        /// </summary>
        /// 实施说明:
        /// 如果初始化期间未设置结果视图，则从投影函数解析，（使用NewSelectionParser类，如果不是实例化投影则返回null）。
        /// 应当保证在对象生命周期内最多执行一次解析操作。
        public virtual TypeView ResultView
        {
            get
            {
                if (_resultView != null) return _resultView;
                if (!IsNew) return null;

                var modelType = _model.GetStructuralType(ResultType);
                TypeView modelTypeView = null;
                if (modelType != null && modelType is TypeView typeView)
                {
                    if (typeView.Source.ClrType == SourceType)
                    {
                        modelTypeView = typeView;
                    }
                    else
                    {
                        var dirivedType = ImpliedTypeManager.Current.ApplyType(ResultType,
                            new IdentityArray(typeView.Source.FullName));
                        //从模型中获取模型视图
                        modelTypeView = _model.GetTypeView(dirivedType);
                    }
                }

                //没有取到 使用NewSelectionParser解析投影函数
                if (modelTypeView == null)
                {
                    var parser = new NewSelectionParser();
                    _resultView = parser.Parse(this, Model);
                }
                else
                {
                    _resultView = modelTypeView;
                }

                return _resultView;
            }
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => _resultSelector?.ReturnType;

        /// <summary>
        ///     判定查询运算是否是异构的。
        /// </summary>
        /// 实施说明:
        /// 对于退化投影（IsNew==false），如果初始化时已设置退化路径，直接调用其属性访问器Heterogeneous，如果未设置，调用基实现。
        /// 对于一般投影（IsNew==true），不能调用基实现，须通过访问器ResultView获取结果视图，判定该视图是否是异构的。
        protected sealed override bool IsHeterogeneous(HeterogeneityPredicationProvider predicationProvider = null)
        {
            bool result;
            //退化投影 和 一般投影 使用各自的方式判定是否异构
            if (IsNew)
                result = ResultView.Heterogeneous(predicationProvider);
            else
                result = _atrophyPath?.Heterogeneous(predicationProvider) ?? base.IsHeterogeneous(predicationProvider);

            return result;
        }

        /// <summary>
        ///     从查询运算中提取隐含包含。
        /// </summary>
        /// 实施说明:
        /// 对于退化投影，如果初始化时设置了退化路径，退化路径所属的关联树即为隐含包含树；如果未设置，调用基实现。
        /// 对于一般投影，如果初始化时未设置结果视图，调用基实现；否则，执行以下步骤：
        /// （1）创建一个代表视图源的关联树节点，作为包含树根节点；
        /// （2）生长上诉含树，覆盖视图源扩展；
        /// （3）对每一视图引用，在包含树中搜索其锚点，为该锚点添加一个代表视图引用绑定目标的子节点。
        protected sealed override AssociationTree TakeImpliedIncluding()
        {
            AssociationTree includingTree;
            //一般投影
            if (IsNew)
            {
                if (_resultView == null)
                {
                    includingTree = base.TakeImpliedIncluding();
                }
                else
                {
                    //创建一个代表视图源的关联树节点，作为包含树根节点；
                    AssociationTreeNode
                        rootNode = new ObjectTypeNode((ObjectType)_resultView.Source);
                    //生成包含树
                    includingTree = rootNode.AsTree();
                    //生长上诉含树，覆盖视图源扩展；
                    includingTree.Grow(_resultView.Extension);
                    //对每一视图引用，在包含树中搜索其锚点，为该锚点添加一个代表视图引用绑定目标的子节点。
                    foreach (var refEle in _resultView.ReferenceElements)
                    {
                        var viewRef = (ViewReference)refEle;
                        //包含树中搜索锚点
                        var node = includingTree.SearchSub(viewRef.Anchor);
                        //为锚点添加一个代表视图引用绑定目标的子节点。
                        if (node?.Node is ObjectTypeNode objectTypeNode)
                            viewRef.Anchor.AddChild(objectTypeNode, node.ElementName);
                    }
                }
            }
            else
            {
                //退化投影
                includingTree = _atrophyPath != null
                    ? _atrophyPath.AssociationPath.AsTree()
                    : base.TakeImpliedIncluding();
            }

            return includingTree;
        }

        /// <summary>
        ///     获取参数
        /// </summary>
        /// <returns></returns>
        protected override Expression[] GetArguments()
        {
            if (ResultSelector == null)
                return Array.Empty<Expression>();
            var member = new MemberExpressionExtractor(new SubTreeEvaluator(ResultSelector))
                .ExtractMember(ResultSelector).Distinct().ToArray();
            var result = new List<Expression>(member);
            return result.ToArray();
        }
    }
}