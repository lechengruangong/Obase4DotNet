/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：退化投影运算形成的退化路径.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:21:39
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示执行退化投影运算形成的退化路径。
    ///     退化投影操作可以形象地理解为在关联树中寻找一个节点，需要时继续在锚定于此节点的某一属性树上寻找一个节点。后续运算将以此节点为根构建一棵新包含树。
    /// </summary>
    public class AtrophyPath
    {
        /// <summary>
        ///     在关联树上的退化路径。
        /// </summary>
        private readonly AssociationTreeNode _associationPath;

        /// <summary>
        ///     在属性树上的退化路径。
        /// </summary>
        private readonly AttributeTreeNode _attributePath;

        /// <summary>
        ///     分解结果暂存
        /// </summary>
        private readonly Dictionary<HeterogeneityPredicationProvider, DecomposeResult> _resultDict =
            new Dictionary<HeterogeneityPredicationProvider, DecomposeResult>();

        /// <summary>
        ///     平展点。
        /// </summary>
        private List<AssociationTreeNode> _flatteningPoints;

        /// <summary>
        ///     创建AtrophyPath实例。
        /// </summary>
        /// <param name="assoPath">关联退化路径。</param>
        /// <param name="attrPath">属性退化路径。</param>
        public AtrophyPath(AssociationTreeNode assoPath, AttributeTreeNode attrPath = null)
        {
            _associationPath = assoPath;
            _attributePath = attrPath;
        }

        /// <summary>
        ///     创建AtrophyPath实例，同时指定平展点。
        /// </summary>
        /// <param name="assoPath">关联退化路径。</param>
        /// <param name="flatteningPoint">平展点。</param>
        /// <param name="attrPath">属性退化路径。</param>
        public AtrophyPath(AssociationTreeNode assoPath, AssociationTreeNode flatteningPoint,
            AttributeTreeNode attrPath = null)
            : this(assoPath, attrPath)
        {
            _flatteningPoints = new List<AssociationTreeNode> { flatteningPoint };
        }

        /// <summary>
        ///     创建表示退化路径的AtrophyPath实例，该退化路径无关联退化。
        /// </summary>
        /// <param name="attrPath">属性退化路径。</param>
        public AtrophyPath(AttributeTreeNode attrPath)
        {
            _attributePath = attrPath;
        }

        /// <summary>
        ///     获取关联退化路径。
        /// </summary>
        public AssociationTreeNode AssociationPath => _associationPath;

        /// <summary>
        ///     获取属性退化路径。
        /// </summary>
        public AttributeTreeNode AttributePath => _attributePath;

        /// <summary>
        ///     获取平展点。
        /// </summary>
        public AssociationTreeNode[] FlatteningPoints => _flatteningPoints?.ToArray();

        /// <summary>
        ///     获取一个值，该值指示退化路径是否为异构的。
        /// </summary>
        /// 实施说明:
        /// 使用AssociationTreeHeterogeneityPredicater断言关联退化路径是否为异构。
        /// 寄存断言结果，避免重复操作。
        public bool Heterogeneous(HeterogeneityPredicationProvider predicationProvider = null)
        {
            //没有指定异构断言提供程序则使用默认的StorageHeterogeneityPredicationProvider
            if (predicationProvider == null)
                predicationProvider = new StorageHeterogeneityPredicationProvider();
            var predicater = new AssociationTreeHeterogeneityPredicater(predicationProvider);
            return _associationPath.Root.AsTree().Accept(predicater);
        }

        /// <summary>
        ///     添加平展点。
        /// </summary>
        /// <param name="point">一个关联树节点，退化路径在此节点处实施平展。</param>
        public void AddFlatteningPoint(AssociationTreeNode point)
        {
            if (_flatteningPoints == null)
                _flatteningPoints = new List<AssociationTreeNode>();
            _flatteningPoints.Add(point);
        }

        /// <summary>
        ///     对退化路径实施极限分解。
        /// </summary>
        /// <returns>
        ///     基础路径。
        ///     警告
        ///     本方法不会检测退化路径是否为异构，如果不是，将生成其副本作为基础路径。强烈建议调用前确保退化路径是异构的。
        /// </returns>
        /// <param name="predicationProvider">异构断言提供程序</param>
        /// <param name="attachingPath">返回附加路径。</param>
        /// <param name="attachingNode">返回附加节点。</param>
        /// <param name="attachingRef">返回附加引用。</param>
        public AtrophyPath DecomposeExtremely(out AtrophyPath attachingPath, out AssociationTreeNode attachingNode,
            out ReferenceElement attachingRef, HeterogeneityPredicationProvider predicationProvider = null)
        {
            if (predicationProvider == null)
                predicationProvider = new StorageHeterogeneityPredicationProvider();
            //如果已经分解过了，则直接返回结果。
            if (_resultDict.TryGetValue(predicationProvider, out var result))
            {
                attachingPath = result.AttachingPath;
                attachingNode = result.AttachingNode;
                attachingRef = result.AttachingRef;
                return result.BasePath;
            }

            var decomposer = new AssociationTreeDecomposer(predicationProvider);
            decomposer.SetArgument(true);
            //极限分解退化路径所代表的关联树
            var tree = _associationPath.Root.AsTree();
            tree.Accept(decomposer);
            var outArg = decomposer.OutArgument?.Length > 0 ? decomposer.OutArgument[0] : null;

            attachingNode = outArg?.AttachingNode;
            attachingRef = outArg?.AttachingReference;
            attachingPath = new AtrophyPath(GetLastAssociationTreeNode(outArg?.AttachingTree), _attributePath);

            var basePath = new AtrophyPath(outArg?.AttachingNode);
            if (_flatteningPoints != null)
            {
                //使用平展点添加器添加
                var pointAdder = new FlatteningPointAdder(this, basePath, attachingPath);
                tree.Accept(pointAdder);
            }

            //暂存结果
            var temp = new DecomposeResult
            {
                BasePath = basePath,
                AttachingPath = attachingPath,
                AttachingNode = attachingNode,
                AttachingRef = attachingRef
            };
            _resultDict.Add(predicationProvider, temp);

            return basePath;
        }

        /// <summary>
        ///     找到某关联树的末节点
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private AssociationTreeNode GetLastAssociationTreeNode(AssociationTree tree)
        {
            var current = tree;
            var result = current.Node;

            while (current.SubCount > 0)
            {
                current = current.SubTrees[0];
                result = current.Node;
            }

            return result;
        }

        /// <summary>
        ///     根据表达式生成退化路径。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="selectionExp">投影表达式。</param>
        /// <param name="flatteningExp">平展表达式。</param>
        public static AtrophyPath FromExpression(ObjectDataModel model, LambdaExpression selectionExp,
            LambdaExpression flatteningExp = null)
        {
            var paraBindings = new List<ParameterBinding>();
            AssociationTree subTree = null;
            if (flatteningExp != null)
                paraBindings.Add(new ParameterBinding(selectionExp.Parameters[1], flatteningExp.Body));
            //抽取关联树和属性树。
            var assoTree = selectionExp.Body.ExtractAssociation(model, out AssociationTreeNode assoTail,
                out AttributeTreeNode attrTail, paraBindings.ToArray());
            //有平展表达式则在关联树中寻找子树
            if (flatteningExp != null)
                subTree = assoTree.SearchSub(flatteningExp.Body, model);
            var atrophy = subTree != null
                ? new AtrophyPath(assoTail, subTree.Node, attrTail)
                : new AtrophyPath(assoTail, attrTail);
            return atrophy;
        }

        /// <summary>
        ///     生成表示退化路径的表达式。
        /// </summary>
        /// <returns>一个Lambda表达式，形如o=>o.Prop或(o, c) = c.Prop，其中形参c绑定于平展表达式。</returns>
        /// <param name="flatteningExps">返回平展表达式（形如o=>o.Prop），无平展点返回null。</param>
        public LambdaExpression GenerateExpression(out LambdaExpression[] flatteningExps)
        {
            var associationTree = _associationPath.Root.AsTree();
            var attributeTree = _attributePath?.AsTree();

            var sourceType = _associationPath.Root.RepresentedType.ClrType;
            var sourcePara = Expression.Parameter(sourceType);

            var collectionParas = new Dictionary<AssociationTreeNode, ParameterExpression>();

            //构造平展形参获取委托（本地函数）
            ParameterExpression GetParameterExpression(AssociationTreeNode p)
            {
                return collectionParas[p];
            }

            var associationExpressionGenerator = new AssociationExpressionGenerator(sourcePara, GetParameterExpression);
            //生成关联部分
            var hostExp = associationTree.Accept(associationExpressionGenerator);
            LambdaExpression resultExp;
            if (attributeTree != null)
            {
                //生成属性部分并合并关联部分。
                var attributeExpressionGenerator = new AttributeExpressionGenerator(hostExp);
                resultExp = attributeTree.Accept(attributeExpressionGenerator);
            }
            else
            {
                resultExp = hostExp;
            }

            var parameters = new List<ParameterExpression> { sourcePara };

            var collectionSelectors = new List<LambdaExpression>();
            //生成中介投影函数
            foreach (var item in _flatteningPoints ?? new List<AssociationTreeNode>())
            {
                var par = Expression.Parameter(item.RepresentedType.ClrType);
                parameters.Add(par);
                collectionParas[item] = par;
                var collectionExp = item.AsTree().Accept(associationExpressionGenerator);
                collectionSelectors.Add(Expression.Lambda(collectionExp, par));
            }

            flatteningExps = collectionSelectors.Count > 0 ? collectionSelectors.ToArray() : null;
            return Expression.Lambda(resultExp.Body, parameters);
        }

        /// <summary>
        ///     分解结果
        /// </summary>
        private class DecomposeResult
        {
            /// <summary>
            ///     附加节点
            /// </summary>
            public AssociationTreeNode AttachingNode;

            /// <summary>
            ///     附加陆军
            /// </summary>
            public AtrophyPath AttachingPath;

            /// <summary>
            ///     附加引用
            /// </summary>
            public ReferenceElement AttachingRef;

            /// <summary>
            ///     基础路径
            /// </summary>
            public AtrophyPath BasePath;
        }
    }
}