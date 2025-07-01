/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：包含运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:41:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.Common;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     包含运算执行器。
    ///     算法：
    ///     includings.Grow(目标表达式);
    /// </summary>
    public class IncludeExecutor : RopExecutor
    {
        /// <summary>
        ///     指示包含目标的表达式。
        /// </summary>
        private readonly LambdaExpression _expression;

        /// <summary>
        ///     指示包含路径的起点
        /// </summary>
        private readonly Type _includeFromType;

        /// <summary>
        ///     指示包含目标的路径字符串
        /// </summary>
        private readonly string _includePath;

        /// <summary>
        ///     构造IncludeExecutor的新实例。
        /// </summary>
        /// <param name="queryOp"></param>
        /// <param name="expression">指示包含目标的表达式。</param>
        /// <param name="includePath">指示包含目标的路径字符串</param>
        /// <param name="includeFromType"></param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        public IncludeExecutor(QueryOp queryOp, LambdaExpression expression, string includePath, Type includeFromType,
            OpExecutor<RopContext> next = null) : base(queryOp, next)
        {
            _expression = expression;
            _includePath = includePath;
            _includeFromType = includeFromType;
        }


        /// <summary>
        ///     分析表示包含目标的表达式，执行必要的强制包含操作。
        ///     如果包含了一个指向显式关联的关联引用，检查该关联的各端（该关联引用的左端除外）是否启用了延迟加载，如果未启用则强制包含该端。
        /// </summary>
        /// <param name="expression">表示包含目标的表达式。</param>
        /// <param name="context">关系运算上下文。</param>
        /// <param name="lastNode">返回上述表达式指向的包含树节点。</param>
        private void ExecuteForciblyIncluding(MemberExpression expression, RopContext context,
            out AssociationTree lastNode)
        {
            //取包含对象
            var hostObj = expression.Expression;

            AssociationTree previousNode;
            //向下寻找包含
            if (hostObj is MemberExpression hostMember)
                ExecuteForciblyIncluding(hostMember, context, out previousNode);
            else
                previousNode = context.Includings;
            //取字段
            var member = expression.Member;

            //var previousType = previousNode.ObjectType;
            var previousType = previousNode.RepresentedType as ObjectType;
            var refrence = previousType?.GetReferenceElement(member.Name);

            //是否在子树内
            if (refrence != null)
            {
                lastNode = previousNode.GetSubTree(member.Name);
            }
            else
            {
                lastNode = null;
                return;
            }


            //隐式关联型 关联树生长至对端
            var associanRef = refrence as AssociationReference;
            if (associanRef != null && associanRef.AssociationType.Visible == false)
                lastNode = lastNode.GetSubTree(associanRef.RightEnd);

            var lastType = lastNode.RepresentedType as ObjectType;
            //显式关联型 针对每个关联端进行判断
            if (lastType is AssociationType associanTypeLast && associanTypeLast.Visible)
                foreach (var end in associanTypeLast.AssociationEnds)
                {
                    //关联树生长将不是本端 并且不进行延迟加载的端生长至树内
                    var endName = end.Name;
                    if (associanRef != null && end.EnableLazyLoading == false && endName != associanRef.LeftEnd)
                    {
                        var endTree = new AssociationTree(end.EntityType, endName);
                        lastNode.AddSubTree(endTree, endName);
                    }
                }
        }

        /// <summary>
        ///     分析表示包含目标的表达式，执行必要的强制包含操作。
        ///     如果包含了一个指向显式关联的关联引用，检查该关联的各端（该关联引用的左端除外）是否启用了延迟加载，如果未启用则强制包含该端。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="context"></param>
        private void ExecuteForciblyIncluding(string path, RopContext context)
        {
            //拆解包含路径
            var currentPath = path.Split('.');

            //从根节点找起
            var currentNode = context.Includings;

            foreach (var node in currentPath)
            {
                //var previousType = currentNode.ObjectType;
                var previousType = currentNode.RepresentedType as ObjectType;
                var refrence = previousType?.GetReferenceElement(node);
                //是否在子树内
                if (refrence != null)
                    currentNode = currentNode.GetSubTree(node);
                else
                    break;

                //隐式关联型 关联树生长至对端
                var associanRef = refrence as AssociationReference;
                if (associanRef != null && associanRef.AssociationType.Visible == false)
                    currentNode = currentNode.GetSubTree(associanRef.RightEnd);

                //var lastType = currentNode.ObjectType;
                var lastType = currentNode.RepresentedType as ObjectType;
                //显式关联型 针对每个关联端进行判断
                if (lastType is AssociationType associanTypeLast && associanTypeLast.Visible)
                    foreach (var end in associanTypeLast.AssociationEnds)
                    {
                        var endName = end.Name;
                        if (associanRef != null && end.EnableLazyLoading == false && endName != associanRef.LeftEnd)
                        {
                            var endTree = new AssociationTree(end.EntityType, endName);
                            currentNode.AddSubTree(endTree, endName);
                        }
                    }
            }
        }

        /// <summary>
        ///     仿照AssociationGrower根据字符串生成包含树
        /// </summary>
        /// <param name="assoTree"></param>
        /// <param name="model"></param>
        private void GrowIncludingByIncludingPath(AssociationTree assoTree,
            ObjectDataModel model)
        {
            //拆解包含路径
            var currentPaths = _includePath.Split('.');
            //关联树
            var lastAssociationNode = assoTree;
            //当前类型 从sourceType开始
            var currentType = _includeFromType;
            //前一个类型
            var preType = _includeFromType;
            //前一个路径
            var prePath = string.Empty;

            foreach (var subPath in currentPaths)
            {
                var property = currentType.GetProperty(subPath);
                if (property == null)
                    throw new ArgumentException($"无法从{currentType.FullName}中获取属性{subPath},请检查Include的参数.",
                        nameof(currentType));

                //在模型内查找
                var structuralType = model.GetTypeOrNull(currentType);
                //增加一个是否处理的
                var isProcessed = false;
                //找不到 可能是元组
                //如果是元组
                if (structuralType == null && Utils.IsTuple(property.DeclaringType))
                {
                    var preStructuralType = model.GetTypeOrNull(preType) as StructuralType;
                    var refElement = preStructuralType?.Elements?.FirstOrDefault(p => p.Name == prePath);

                    //为关联引用
                    if (refElement is AssociationReference reference)
                    {
                        var currentEnd = reference.AssociationType.AssociationEnds.FirstOrDefault(p =>
                            p.EntityType.ClrType == property.PropertyType);

                        //获取关联树子树
                        var sub = lastAssociationNode?.GetSubTree(currentEnd.Name);
                        if (sub != null)
                        {
                            if (currentEnd.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                var navi = currentEnd.Navigation;
                                var refTree = sub.GetSubTree(navi.TargetEndName);
                                if (refTree != null)
                                    lastAssociationNode = refTree;
                            }
                            else
                            {
                                lastAssociationNode = sub;
                            }
                        }
                        else
                        {
                            //构造树
                            sub = new AssociationTree(currentEnd.ReferenceType, currentEnd.Name);
                            lastAssociationNode?.AddSubTree(sub);

                            if (currentEnd.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                lastAssociationNode = sub;
                                //根据导航构造树
                                var navi = currentEnd.Navigation;
                                sub = new AssociationTree(navi.TargetEnd.EntityType,
                                    navi.TargetEndName);
                                lastAssociationNode?.AddSubTree(sub);
                                lastAssociationNode = sub;
                            }
                            else
                            {
                                lastAssociationNode = sub;
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
                        var sub = lastAssociationNode?.GetSubTree(subPath);
                        if (sub != null)
                        {
                            if (referenceElement.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                var navi = referenceElement.Navigation;
                                var refTree = sub.GetSubTree(navi.TargetEndName);
                                if (refTree != null)
                                    lastAssociationNode = refTree;
                            }
                            else
                            {
                                lastAssociationNode = sub;
                            }
                        }
                        else
                        {
                            //构造树
                            sub = new AssociationTree(referenceElement.ReferenceType, subPath);
                            lastAssociationNode?.AddSubTree(sub);

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
                                        lastAssociationNode = sub;
                                        sub = new AssociationTree(end.ReferenceType, end.Name);
                                        lastAssociationNode?.AddSubTree(sub);
                                    }
                                }
                            }

                            if (referenceElement.NavigationUse == ENavigationUse.DirectlyReference)
                            {
                                lastAssociationNode = sub;
                                //根据导航构造树
                                var navi = referenceElement.Navigation;
                                sub = new AssociationTree(navi.TargetEnd.EntityType,
                                    navi.TargetEndName);
                                lastAssociationNode?.AddSubTree(sub);
                                lastAssociationNode = sub;
                            }
                            else
                            {
                                lastAssociationNode = sub;
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
            }
        }

        /// <summary>
        ///     执行包含运算
        /// </summary>
        /// <param name="context">关系运算上下文</param>
        public override void Execute(RopContext context)
        {
            //抽取对应的关联树
            if (_expression != null)
            {
                var members =
                    new MemberExpressionExtractor(new SubTreeEvaluator(_expression)).ExtractMember(_expression);
                members?.ForEach(member =>
                    member.OnlyGrowAssociationTree(context.Includings, context.Model, out _, out _));

                ExecuteForciblyIncluding(_expression.Body as MemberExpression, context, out _);
            }
            else
            {
                GrowIncludingByIncludingPath(context.Includings, context.Model);
                ExecuteForciblyIncluding(_includePath, context);
            }

            (_next as OpExecutor<RopContext>)?.Execute(context);
        }
    }
}