/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Include运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:57:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Common;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Include运算。
    /// </summary>
    public class IncludeOp : QueryOp
    {
        /// <summary>
        ///     包含树。
        ///     包含树指示随同对象加载的引用元素。在任一子树中，根节点表示要加载的对象，其子节点指示要加载的引用。
        /// </summary>
        private AssociationTree _includingTree;

        /// <summary>
        ///     包含表达式，用于指示包含路径。
        /// </summary>
        private LambdaExpression[] _selectors;

        /// <summary>
        ///     创建IncludeOp实例。
        /// </summary>
        /// <param name="selector">包含表达式。</param>
        /// <param name="model">对象数据模型</param>
        internal IncludeOp(LambdaExpression selector, ObjectDataModel model)
            : base(EQueryOpName.Include, selector.Parameters[0].Type)
        {
            if (selector == null)
                throw new ArgumentException("包含表达式不能为空", nameof(selector));

            _model = model;
            _selectors = new[] { selector };
            TargetType = _selectors[0].ReturnType;
        }


        /// <summary>
        ///     创建IncludeOp实例
        /// </summary>
        /// <param name="includingPath">包含路径</param>
        /// <param name="sourceType">原类型</param>
        /// <param name="model">对象数据模型</param>
        internal IncludeOp(string includingPath, Type sourceType, ObjectDataModel model)
            : base(EQueryOpName.Include, sourceType)
        {
            //推进类型 获取返回类型
            var subPaths = includingPath.Split('.');
            _model = model;
            GenerateIncludingTreeByPath(subPaths, sourceType);
        }

        /// <summary>
        ///     创建IncludeOp实例。
        /// </summary>
        /// <param name="includingTree">包含树。</param>
        /// <param name="model">对象数据模型</param>
        public IncludeOp(AssociationTree includingTree, ObjectDataModel model)
            : base(EQueryOpName.Include, includingTree.RepresentedType.ClrType)
        {
            _includingTree = includingTree;
            _model = model;
        }

        /// <summary>
        ///     获取包含树。
        ///     实施说明
        ///     如果初始化时未设置包含树，则根据包含表达式生成。
        ///     确保在对象整个生命周期中只执行一次生成操作。
        /// </summary>
        public AssociationTree IncludingTree
        {
            get
            {
                if (_includingTree == null && _selectors?.FirstOrDefault() != null)
                    //此处使用仅抽取方法 以验证关联树内部元素
                    _includingTree = _selectors[0].Body.OnlyExtractAssociation(_model);

                return _includingTree;
            }
        }

        /// <summary>
        ///     获取包含表达式。
        /// </summary>
        /// 实施说明:
        /// 如果初始化时未设置包含表达式，则根据包含树生成：
        /// 首先使用AssociationLeafNodeCollector收集包含树的所有叶子节点；
        /// 然后针对每一个叶子节点，使用AssociationExpressionGenerator生成表达式。
        /// 确保在对象整个生命周期中只执行一次生成操作。
        public LambdaExpression[] Selectors
        {
            get
            {
                if (_selectors != null) return _selectors;
                var exps = new List<LambdaExpression>();
                //首先使用AssociationLeafNodeCollector收集包含树的所有叶子节点
                var collector = new AssociationLeafNodeCollector();
                _includingTree.Accept(collector);
                //针对每一个叶子节点，使用AssociationExpressionGenerator生成表达式
                foreach (var item in collector.Result)
                {
                    var generator = new AssociationExpressionGenerator(Expression.Parameter(SourceType));
                    item.AsTree().Accept(generator);
                    exps.Add(generator.Result);
                }

                return _selectors = exps.ToArray();
            }
        }

        /// <summary>
        ///     获取包含目标类型。
        /// </summary>
        public Type TargetType { get; private set; }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;

        /// <summary>
        ///     判定查询运算是否是异构的。
        /// </summary>
        /// 实施说明:
        /// 如果初始化时已设置包含树，使用AssociationTreeHeterogeneousPredicator判定包含树是否为异构的；
        /// 如果未设置，调用基实现。
        protected sealed override bool IsHeterogeneous(HeterogeneityPredicationProvider predicationProvider = null)
        {
            if (_includingTree != null)
            {
                var predicater = new AssociationTreeHeterogeneityPredicater(predicationProvider);
                _includingTree.Accept(predicater);
                return predicater.Result;
            }

            return base.IsHeterogeneous(predicationProvider);
        }

        /// <summary>
        ///     从查询运算中提取隐含包含。
        /// </summary>
        /// 实施说明:
        /// 始终返回null。
        protected sealed override AssociationTree TakeImpliedIncluding()
        {
            return null;
        }

        /// <summary>
        ///     根据包含路径构造包含树
        /// </summary>
        /// <param name="subPaths">子包含路径</param>
        /// <param name="sourceType">源类型</param>
        private void GenerateIncludingTreeByPath(string[] subPaths, Type sourceType)
        {
            //获取引用类型
            var referringType = _model.GetReferringType(sourceType);
            //构造初始关联树
            _includingTree = new AssociationTree(referringType);

            var lastNode = _includingTree;

            //返回类型
            Type returnType = null;
            //当前类型 从sourceType开始
            var currentType = sourceType;
            //前一个类型
            var preType = sourceType;
            //前一个路径
            var prePath = string.Empty;

            foreach (var subPath in subPaths)
            {
                var property = currentType.GetProperty(subPath);
                if (property == null)
                    throw new ArgumentException($"无法从{currentType.FullName}中获取属性{subPath},请检查Include的参数.",
                        nameof(sourceType));

                //在模型内查找
                var structuralType = _model.GetTypeOrNull(currentType);
                //增加一个是否处理的
                var isProcessed = false;
                //找不到 可能是元组
                //如果是元组
                if (structuralType == null && Utils.IsTuple(property.DeclaringType))
                {
                    var preStructuralType = _model.GetTypeOrNull(preType) as StructuralType;
                    var refElement = preStructuralType?.Elements?.FirstOrDefault(p => p.Name == prePath);

                    //为关联引用
                    if (refElement is AssociationReference reference)
                    {
                        var currentEnd = reference.AssociationType.AssociationEnds.FirstOrDefault(p =>
                            p.EntityType.ClrType == property.PropertyType);

                        //获取关联树子树
                        var sub = lastNode?.GetSubTree(currentEnd.Name);
                        if (sub != null)
                        {
                            if (currentEnd.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                var navi = currentEnd.Navigation;
                                var refTree = sub.GetSubTree(navi.TargetEndName);
                                if (refTree != null)
                                    lastNode = refTree;
                            }
                            else
                            {
                                lastNode = sub;
                            }
                        }
                        else
                        {
                            //构造树
                            sub = new AssociationTree(currentEnd.ReferenceType, currentEnd.Name);
                            lastNode?.AddSubTree(sub);

                            if (currentEnd.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                lastNode = sub;
                                //根据导航构造树
                                var navi = currentEnd.Navigation;
                                sub = new AssociationTree(navi.TargetEnd.EntityType,
                                    navi.TargetEndName);
                                lastNode?.AddSubTree(sub);
                                lastNode = sub;
                            }
                            else
                            {
                                lastNode = sub;
                            }
                        }
                    }
                    //找不到引用元素
                    else
                    {
                        throw new ArgumentException($"包含路径错误,找不到为{prePath}的引用元素.");
                    }

                    //处理过了
                    isProcessed = true;
                }

                //找得到 直接处理
                if (structuralType is StructuralType structural)
                {
                    //查找Name
                    var element = structural.Elements?.FirstOrDefault(p => p.Name == subPath);
                    //为关联引用或关联端
                    if (element is ReferenceElement referenceElement)
                    {
                        //获取关联树子树
                        var sub = lastNode?.GetSubTree(subPath);
                        if (sub != null)
                        {
                            if (referenceElement.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                var navi = referenceElement.Navigation;
                                var refTree = sub.GetSubTree(navi.TargetEndName);
                                if (refTree != null)
                                    lastNode = refTree;
                            }
                            else
                            {
                                lastNode = sub;
                            }
                        }
                        else
                        {
                            //构造树
                            sub = new AssociationTree(referenceElement.ReferenceType, subPath);
                            lastNode?.AddSubTree(sub);

                            //如果当前为显式关联型 且表达式不完整 仅指向对端
                            if (referenceElement is AssociationReference associationReference &&
                                associationReference.AssociationType.Visible)
                            {
                                //是否集合属性
                                var propType = property.PropertyType.GetInterface("IEnumerable");
                                if (propType != null && property.PropertyType != typeof(string)) //集合属性
                                    propType = property.PropertyType.GetGenericArguments()[0];
                                else
                                    propType = property.PropertyType;

                                if (propType != associationReference.AssociationType.ClrType)
                                {
                                    var end = associationReference.AssociationType.AssociationEnds.FirstOrDefault(p =>
                                        p.EntityType.ClrType == propType);

                                    if (end != null)
                                    {
                                        //下移 补一层
                                        lastNode = sub;
                                        sub = new AssociationTree(end.ReferenceType, end.Name);
                                        lastNode?.AddSubTree(sub);
                                    }
                                }
                            }

                            if (referenceElement.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                lastNode = sub;
                                //根据导航构造树
                                var navi = referenceElement.Navigation;
                                sub = new AssociationTree(navi.TargetEnd.EntityType,
                                    navi.TargetEndName);
                                lastNode?.AddSubTree(sub);
                                lastNode = sub;
                            }
                            else
                            {
                                lastNode = sub;
                            }
                        }
                    }
                    //找不到引用元素
                    else
                    {
                        throw new ArgumentException($"包含路径错误,找不到为{subPath}的引用元素.");
                    }

                    //处理过了
                    isProcessed = true;
                }

                if (!isProcessed)
                    throw new ArgumentException($"包含路径错误,{currentType}不是已注册的Obase类型.");

                //记录前一个类型
                preType = currentType;
                //记录前一个类型
                prePath = subPath;

                //是否集合属性
                var type = property.PropertyType.GetInterface("IEnumerable");
                //推进下一个类型
                if (type != null && property.PropertyType != typeof(string)) //集合属性
                    currentType = property.PropertyType.GetGenericArguments()[0];
                else
                    currentType = property.PropertyType;

                returnType = currentType;
            }

            TargetType = returnType;
        }

        /// <summary>
        ///     创建IncludeOp实例。
        /// </summary>
        /// <param name="includingTree">包含树。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">查询链中的下一个运算。</param>
        public static IncludeOp Create(AssociationTree includingTree, ObjectDataModel model, QueryOp nextOp = null)
        {
            var sourcePar = Expression.Parameter(includingTree.Root.RepresentedType.ClrType);
            var generator = new AssociationExpressionGenerator(sourcePar);
            includingTree.Accept(generator);
            return (IncludeOp)Include(generator.Result, model, nextOp);
        }
    }
}