/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联树,关联关系生成的树形结构.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 14:44:36
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     表示根据关联关系生成的树形结构，根节点为当前对象类型，其它节点为关联引用或关联端的类型。
    /// </summary>
    public class AssociationTree
    {
        /// <summary>
        ///     关联树的节点层级结构（根节点为当前子树的根）。
        /// </summary>
        private readonly AssociationTreeNode _node;

        /// <summary>
        ///     构造AssociationTree的新实例，构造的树将作为子树添加到某一节点。
        /// </summary>
        /// <param name="objectType">当前节点的对象类型。</param>
        /// <param name="elementName">当前节点在父级类型中的元素名称。</param>
        public AssociationTree(ObjectType objectType, string elementName) : this(
            new ObjectTypeNode(objectType, elementName))
        {
        }

        /// <summary>
        ///     创建代表指定引用元素的AssociationTree实例，构造的树将作为子树添加到某一节点。
        /// </summary>
        /// <param name="reference">要创建的节点代表的引用元素。</param>
        public AssociationTree(ReferenceElement reference) : this(new ObjectTypeNode(reference.ReferenceType))
        {
        }

        /// <summary>
        ///     构造AssociationTree的新实例。
        /// </summary>
        /// <param name="representedTyp">当前节点的对象类型。</param>
        public AssociationTree(ReferringType representedTyp) : this(
            representedTyp is TypeView
                ? new TypeViewNode((TypeView)representedTyp)
                : (AssociationTreeNode)new ObjectTypeNode(representedTyp as ObjectType)
        )
        {
        }

        /// <summary>
        ///     使用指定的节点层级结构（根节点为树根）创建AssociationTree实例。
        /// </summary>
        /// <param name="node">节点层级结构。</param>
        internal AssociationTree(AssociationTreeNode node)
        {
            _node = node;
        }

        /// <summary>
        ///     获取关联树代表的类型。
        /// </summary>
        public ReferringType RepresentedType => Node.RepresentedType;

        /// <summary>
        ///     获取当前节点在父级类型中的元素名称（即关联引用名或关联端名）。
        /// </summary>
        public string ElementName => (Node as ObjectTypeNode)?.ElementName;

        /// <summary>
        ///     获取当前节点的所有子树。
        /// </summary>
        public AssociationTree[] SubTrees =>
            Node.Children?.Select(p => p.AsTree()).ToArray() ?? Array.Empty<AssociationTree>();

        /// <summary>
        ///     获取子树（直接子代）的数量。
        /// </summary>
        public int SubCount => Node.Children?.Count() ?? 0;

        /// <summary>
        ///     获取顶级树。
        /// </summary>
        public AssociationTree Root => _node.Root.AsTree();

        /// <summary>
        ///     获取一个值，该值指示当前子树是否为顶级树。
        /// </summary>
        [Obsolete]
        public bool IsRoot => _node is TypeViewNode || _node.Parent == null;

        /// <summary>
        ///     获取当前子树所属的关联树。
        /// </summary>
        public AssociationTree Parent => _node.Parent?.AsTree();

        /// <summary>
        ///     获取关联树节点代表的类型元素。
        /// </summary>
        public ReferenceElement Element
        {
            get
            {
                if (Node is ObjectTypeNode objectTypeNode) return objectTypeNode.Element;
                return null;
            }
        }

        /// <summary>
        ///     获取关联树的节点层级结构（根节点为当前子树的根）。
        /// </summary>
        public AssociationTreeNode Node => _node;

        /// <summary>
        ///     获取指定元素名称（关联引用名或关联端名）对应的子树。
        /// </summary>
        /// <param name="elementName">要获取其对应子树的元素名称。</param>
        public AssociationTree GetSubTree(string elementName)
        {
            var child = Node.GetChild(elementName);
            return child?.AsTree();
        }

        /// <summary>
        ///     为当前节点添加子树。
        ///     如果已存在同名子树，不执行添加操作。
        /// </summary>
        /// <returns>返回刚添加的子树；如果存在同名子树，返回已存在的子树。</returns>
        /// <param name="elementName">子树对应的元素名称。</param>
        /// <param name="subTree">要添加的子树。</param>
        public AssociationTree AddSubTree(AssociationTree subTree, string elementName = null)
        {
            var childNode = subTree.Node as ObjectTypeNode;
            //只能添加对象类型的节点
            if (childNode == null)
                throw new ArgumentException("非顶级节点只能是“对象类型的节点”");
            var node = Node.AddChild(childNode, elementName ?? subTree.ElementName);
            return node.AsTree();
        }

        /// <summary>
        ///     生长关联树以使其覆盖指定的元素。
        ///     如果已存在同名子树，不执行操作。
        /// </summary>
        /// <param name="elementName">子树对应的元素名称。</param>
        /// <returns>返回刚添加的子树；如果存在同名子树，返回已存在的子树。</returns>
        public AssociationTree Grow(string elementName)
        {
            var subTree = GetSubTree(elementName);
            var reference = _node.RepresentedType.GetReferenceElement(elementName);
            if (subTree == null)
            {
                //必须是引用元素
                if (reference == null)
                    throw new ArgumentException($"子树对应的元素名称{elementName}的引用类型不存在。");
                subTree = new AssociationTree(reference);
                AddSubTree(subTree, reference.Name);
                //如果是隐式关联 直接加入对端
                if (reference is AssociationReference ar && ar.AssociationType.Visible == false)
                {
                    var sub1 = new AssociationTree(ar.AssociationType.GetAssociationEnd(ar.RightEnd).EntityType,
                        ar.RightEnd);
                    subTree.AddSubTree(sub1, ar.RightEnd);
                }
                //隐式视图引用 加入对端
                else if (reference is ViewReference vr)
                {
                    if (vr.Binding is AssociationReference ar1 && ar1.AssociationType.Visible == false)
                    {
                        var sub1 = new AssociationTree(ar1.AssociationType.GetAssociationEnd(ar1.RightEnd).EntityType,
                            ar1.RightEnd);
                        subTree.AddSubTree(sub1, ar1.RightEnd);
                    }
                }
            }

            return subTree;
        }

        /// <summary>
        ///     生长关联树以使其覆盖指定的元素。
        ///     如果已存在同名子树，不执行操作。
        /// </summary>
        /// <param name="element">子树代表的元素。</param>
        /// <returns>返回刚添加的子树；如果存在同名子树，返回已存在的子树。</returns>
        /// 实施说明:
        /// 参照重载版本Grow(String)。
        public AssociationTree Grow(ReferenceElement element)
        {
            var subTree = GetSubTree(element.Name);
            if (subTree == null)
            {
                //只能加入当前子树的节点
                if (element == null)
                    throw new ArgumentException($"子树对应的元素名称{element.Name}的引用类型不存在。");
                subTree = new AssociationTree(element);
                AddSubTree(subTree, element.Name);
                //如果是隐式关联 直接加入对端
                if (element is AssociationReference ar && ar.AssociationType.Visible == false)
                {
                    var sub1 = new AssociationTree(ar.AssociationType.GetAssociationEnd(ar.RightEnd).EntityType,
                        ar.RightEnd);
                    subTree.AddSubTree(sub1, ar.RightEnd);
                }
                //隐式视图引用 加入对端
                else if (element is ViewReference vr)
                {
                    if (vr.Binding is AssociationReference ar1 && ar1.AssociationType.Visible == false)
                    {
                        var sub1 = new AssociationTree(ar1.AssociationType.GetAssociationEnd(ar1.RightEnd).EntityType,
                            ar1.RightEnd);
                        subTree.AddSubTree(sub1, ar1.RightEnd);
                    }
                }
            }

            return subTree;
        }

        /// <summary>
        ///     生长关联树从而覆盖另一棵树（目标树），但目标树代表的类型是由当前树代表的类型经过一次或多次退化投影得出的。
        ///     实施说明
        ///     参见活动图“关联树生长算法（二）”。
        /// </summary>
        /// <param name="targetTree">要覆盖的目标树。</param>
        /// <param name="atrophies">退化投影形成的退化路径序列。</param>
        internal void Grow(AssociationTree targetTree, AssociationTreeNode[] atrophies)
        {
            //期望类型
            var expectdType = atrophies.Length > 0
                ? atrophies[atrophies.Length - 1].RepresentedType
                : RepresentedType;
            //拆解关联树
            var trees = Unpackage(targetTree, expectdType);

            foreach (var tree in trees)
            {
                //根据路径加入子树
                var rootNode = tree.Node;
                foreach (var atrophy in atrophies)
                    rootNode.AddChild(atrophy.Children);

                Grow(rootNode.AsTree());
            }
        }

        /// <summary>
        ///     对以视图为根的关联树拆包，即将以视图为根的关联树映射为以该视图的源为根的关联树。如果存在视图嵌套，则层层拆包直到所得关联树代表的类型为指定的期望类型。
        /// </summary>
        /// <param name="targetTree">要拆包的关联树。</param>
        /// <param name="expectedType">期望类型。</param>
        private AssociationTree[] Unpackage(AssociationTree targetTree, StructuralType expectedType)
        {
            //如果当前树的根节点代表的类型与期望类型相同，则直接返回该树
            if (targetTree.Element?.ReferenceType == expectedType)
                return new[] { targetTree };
            var result = new List<AssociationTree>();
            //如果当前树的根节点代表的类型不是期望类型，则继续拆包
            foreach (var tree in targetTree.SubTrees)
                result.AddRange(Unpackage(tree, expectedType));

            return result.ToArray();
        }

        /// <summary>
        ///     移除代表指定元素的子树，然后返回该子树。
        /// </summary>
        /// <param name="elementName"></param>
        public AssociationTree RemoveSub(string elementName)
        {
            //移除
            var subTreeNode = Node.RemoveChild(elementName);
            return subTreeNode.AsTree();
        }

        /// <summary>
        ///     使关联树生长从而覆盖另一棵树（目标树）。
        ///     实施说明：两根树的根节点必须代表相同的类型。生长算法参数类图“关联树生长”及活动图“关联树生长算法（一）”。注意类图中的依赖关系。
        /// </summary>
        /// <param name="other">要覆盖的另一棵树。</param>
        public AssociationTree Grow(AssociationTree other)
        {
            var grower = new AssociationTreeGrower();
            Accept(grower, other);
            return other;
        }

        /// <summary>
        ///     对当前关联树圈定的对象系统实施退化投影，得到新的对象系统结构。
        ///     实施说明
        ///     与HeterogQueryExecutor类的CutIncluding(AssociationTree,
        ///     AssociationTreeNode)方法逻辑相同。
        ///     使用AssociationTree.Search方法搜索退化路径指向的子树，以该子树作为新包含树返回。
        /// </summary>
        /// <returns>表示新的对象系统结构的关联树。</returns>
        /// <param name="atrophyPath">退化路径</param>
        public AssociationTree Select(AssociationTreeNode atrophyPath)
        {
            var sub = SearchSub(atrophyPath);

            return sub;
        }

        /// <summary>
        ///     对当前关联树圈定的对象系统实施一般投影，得到新的对象系统结构。
        ///     实施说明
        ///     与HeterogQueryExecutor类的CutIncluding(AssociationTree, TypeView)方法逻辑相同。
        ///     参照活动图“Rop/关系运算上下文/设置投影结果类型（二）”。
        /// </summary>
        /// <returns>表示新的对象系统结构的关联树。</returns>
        /// <param name="typeView">投影结果视图。</param>
        public AssociationTree Select(TypeView typeView)
        {
            var elements = typeView.ReferenceElements;
            //裁剪包含树
            foreach (var referenceElement in elements)
                if (referenceElement is ViewReference viewReference)
                {
                    var anchorTree = SearchSub(viewReference.Anchor);
                    if (anchorTree != null)
                    {
                        //根据视图引用的绑定名称移除子树
                        var bindingTree = RemoveSub(viewReference.Binding.Name);
                        if (bindingTree != null) AddSubTree(bindingTree, viewReference.Name);
                    }

                    Grow(viewReference.Name);
                }

            return this;
        }

        /// <summary>
        ///     在关联树中搜索指定表达式对应的子树。
        /// </summary>
        /// <param name="expression">作为搜索依据的表达式。</param>
        /// <param name="model">对象数据模型。</param>
        public AssociationTree SearchSub(Expression expression, ObjectDataModel model)
        {
            //提取关联树
            expression.ExtractAssociation(model, out AssociationTreeNode assoTail);
            //子树搜索器
            var subSearcher = new SubTreeSeacher();
            //遍历 搜索
            var sub = assoTail.AsTree().Accept(subSearcher, this);

            return sub;
        }

        /// <summary>
        ///     在关联树中搜索以指定节点为根的子树
        /// </summary>
        /// <param name="targetNode"></param>
        /// <returns></returns>
        public AssociationTree SearchSub(AssociationTreeNode targetNode)
        {
            //子树搜索器
            var subSearcher = new SubTreeSeacher();
            //遍历 搜索
            var sub = targetNode.AsTree().Accept(subSearcher, this);
            return sub;
        }


        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向上访问者。</param>
        public void Accept(IAssociationTreeUpwardVisitor visitor)
        {
            visitor.Reset();
            Accept(visitor, null);
        }

        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向上访问者。</param>
        /// <param name="argument">遍历操作参数。</param>
        public void Accept<TArg>(IParameterizedAssociationTreeUpwardVisitor<TArg> visitor, TArg argument)
        {
            visitor.Reset();
            //设置参数
            visitor.SetArgument(argument);
            Accept(visitor, childState: argument);
        }

        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向上访问者。</param>
        public TResult Accept<TResult>(IAssociationTreeUpwardVisitor<TResult> visitor)
        {
            visitor.Reset();
            Accept(visitor, null);
            return visitor.Result;
        }

        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向上访问者。</param>
        /// <param name="argument">遍历操作参数。</param>
        public TResult Accept<TArg, TResult>(IParameterizedAssociationTreeUpwardVisitor<TArg, TResult> visitor,
            TArg argument)
        {
            visitor.Reset();
            //设置参数
            visitor.SetArgument(argument);
            Accept(visitor, null);
            return visitor.Result;
        }

        /// <summary>
        ///     在向上遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向上访问者。</param>
        /// <param name="childState">子级访问产生的状态数据。</param>
        private void Accept(IAssociationTreeUpwardVisitor visitor, object childState)
        {
            //执行前置访问
            var res = visitor.Previsit(this, childState, out var outChildState, out var outPrevisitState);
            if (res)
                //如果需要继续向上遍历
                if (Node is ObjectTypeNode objectTypeNode && objectTypeNode.Parent != null)
                {
                    var parent = objectTypeNode.Parent.AsTree();
                    //访问父节点
                    parent.Accept(visitor, outChildState);
                }

            //执行后置访问
            visitor.Postvisit(this, childState, outPrevisitState);
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向下访问者。</param>
        public void Accept(IAssociationTreeDownwardVisitor visitor)
        {
            visitor.Reset();
            Accept(visitor, null);
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向下访问者。</param>
        /// <param name="argument">遍历操作参数。</param>
        public void Accept<TArg>(IParameterizedAssociationTreeDownwardVisitor<TArg> visitor, TArg argument)
        {
            visitor.Reset();
            //设置参数
            visitor.SetArgument(argument);
            Accept(visitor, parentState: argument);
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向下访问者。</param>
        public TResult Accept<TResult>(IAssociationTreeDownwardVisitor<TResult> visitor)
        {
            visitor.Reset();
            Accept(visitor, null);
            return visitor.Result;
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向下访问者。</param>
        /// <param name="argument">遍历操作参数。</param>
        public TResult Accept<TArg, TResult>(IParameterizedAssociationTreeDownwardVisitor<TArg, TResult> visitor,
            TArg argument)
        {
            visitor.Reset();
            //设置参数
            visitor.SetArgument(argument);
            Accept(visitor, null);
            return visitor.Result;
        }


        /// <summary>
        ///     在向下遍历关联树过程中接受访问者。
        /// </summary>
        /// <param name="visitor">向下访问者。</param>
        /// <param name="parentState">父级访问产生的状态数据。</param>
        private void Accept(IAssociationTreeDownwardVisitor visitor, object parentState)
        {
            //前置访问
            var previsitResult = visitor.Previsit(this, parentState, out var outParentState, out var outPrevisitState);
            if (previsitResult)
                //子树遍历
                foreach (var subTree in SubTrees)
                    subTree.Accept(visitor, outParentState);
            //后置访问
            visitor.Postvisit(this, parentState, outPrevisitState);
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者
        /// </summary>
        /// <typeparam name="TResult">遍历操作返回结果的类型</typeparam>
        /// <typeparam name="TOut">输出参数的类型</typeparam>
        /// <param name="visitor">向下访问者</param>
        /// <param name="outArg">以输出参数的形式返回访问结果</param>
        /// <returns></returns>
        public TResult Accept<TResult, TOut>(IAssociationTreeDownwardVisitor<TResult, TOut> visitor, out TOut outArg)
        {
            visitor.Reset();
            Accept(visitor, null);
            outArg = visitor.OutArgument;
            return visitor.Result;
        }

        /// <summary>
        ///     在向下遍历关联树过程中接受访问者
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="argument"></param>
        /// <param name="outArg"></param>
        /// <returns></returns>
        public TResult Accept<TArg, TResult, TOut>(
            IParameterizedAssociationTreeDownwardVisitor<TArg, TResult, TOut> visitor, TArg argument, out TOut outArg)
        {
            visitor.Reset();
            //设置参数
            visitor.SetArgument(argument);
            //先接受无状态参数的
            Accept(visitor, null);
            //前置访问
            visitor.Previsit(this, null, out var outParentState, out var outPrevisitState);
            //接受有状态参数的
            Accept(visitor, outParentState);
            //后置访问
            visitor.Postvisit(this, null, outPrevisitState);
            outArg = visitor.OutArgument;
            return visitor.Result;
        }

        /// <summary>
        ///     将两个关联树合并。
        ///     该方法不会生成新关联树，而是生长其中一个以覆盖另一个。
        /// </summary>
        /// <returns>生长了的关联树。如果assoTree1为null则返回assoTree2；如果assoTree2为null则返回assoTree1。</returns>
        /// <param name="assoTree1">第一个关联树。</param>
        /// <param name="assoTree2">第二个关联树。</param>
        public static AssociationTree Combine(AssociationTree assoTree1, AssociationTree assoTree2)
        {
            //有空的情况下
            if (assoTree1 == null && assoTree2 == null) return null;
            if (assoTree1 == null) return assoTree2;
            if (assoTree2 == null) return assoTree1;
            //用一个覆盖另一个
            return assoTree1.Grow(assoTree2);
        }

        /// <summary>
        ///     子树搜索器。
        /// </summary>
        private class SubTreeSeacher : IParameterizedAssociationTreeUpwardVisitor<AssociationTree, AssociationTree>
        {
            /// <summary>
            ///     作为搜索源的关联树。
            /// </summary>
            private AssociationTree _sourceTree;

            /// <summary>
            ///     获取遍历关联树的结果。
            /// </summary>
            public AssociationTree Result => _sourceTree;

            /// <summary>
            ///     后置访问，即在访问父级后执行操作。
            /// </summary>
            /// <param name="subTree">被访问的子树。</param>
            /// <param name="childState">访问子级时产生的状态数据。</param>
            /// <param name="previsitState">前置访问产生的状态数据。</param>
            public void Postvisit(AssociationTree subTree, object childState, object previsitState)
            {
                if (subTree == null) return;

                //如果已经是根节点
                if (subTree.IsRoot)
                {
                    var sourceType = _sourceTree.RepresentedType;
                    var treeType = subTree.RepresentedType;
                    //相等 直接返回
                    if (sourceType == treeType) return;
                    //否则 结果赋空
                    _sourceTree = null;
                }
                else
                {
                    var sub = _sourceTree?.GetSubTree(subTree.ElementName);
                    //找到子树 赋值子树 否则赋空
                    _sourceTree = sub;
                }
            }

            /// <summary>
            ///     前置访问，即在访问父级前执行操作。
            /// </summary>
            /// <param name="subTree">被访问的子树。</param>
            /// <param name="childState">访问子级时产生的状态数据。</param>
            /// <param name="outChildState">返回一个状态数据，在遍历到父级时该数据将被视为子级状态。</param>
            /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
            public bool Previsit(AssociationTree subTree, object childState, out object outChildState,
                out object outPrevisitState)
            {
                //Nothing To Do
                outChildState = null;
                outPrevisitState = null;
                return true;
            }


            /// <summary>
            ///     为即将开始的遍历操作设置参数。
            /// </summary>
            /// <param name="argument">参数值。</param>
            public void SetArgument(AssociationTree argument)
            {
                _sourceTree = argument;
            }

            /// <summary>
            ///     重置
            /// </summary>
            public void Reset()
            {
                _sourceTree = null;
            }
        }
    }
}