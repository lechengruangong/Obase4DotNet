/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关系运算上下文.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:55:13
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using Obase.Core;
using Obase.Core.Common;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Providers.Sql.SqlObject;
using Expression = System.Linq.Expressions.Expression;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     关系运算上下文。
    /// </summary>
    public class RopContext
    {
        /// <summary>
        ///     生成Sql语句时使用的对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     相对于查询链结果类型的包含树。
        /// </summary>
        private readonly AssociationTree _resultIncluding;

        /// <summary>
        ///     数据源类型
        /// </summary>
        private readonly EDataSource _targetSource;

        /// <summary>
        ///     别名生成器，用于生成退化路径和包含树各节点的别名。需要时创建。
        /// </summary>
        private AliasGenerator _aliasGenerator;

        /// <summary>
        ///     别名根
        /// </summary>
        private string _aliasRoot;

        /// <summary>
        ///     自查询运算开始或上次AcceptResult以来的退化路径。
        ///     退化投影操作可以形象地理解为在关联树中寻找一个节点，当attrTreeResult != null时则需要继续在锚定于此节点的某一属性树上寻找一个节点。后续运算将以此节点为根构建一棵新包含树。
        ///     作为后续运算参数的表达式是与新包含树相对应的，如o.PropA中的o即对应于新树的根。而投影操作完成时，结果SQL语句的查询源（From
        ///     子句）仍然对应于原树，因此在对表达式进行解析时，需要借助退化路径将新树的节点回退到原树，具体而言主要是两个问题：
        ///     （1）从表达式解析出的查询源，需要在其别名前附加一个前缀，该前缀根据关联退化路径生成；
        ///     （2）从表达式解析出的映射字段，需要在其名称前附加一个前缀，该前缀根据属性退化路径生成。
        ///     如果投影结果为基元类型，它可能是由多个简单属性经数学运算而成的，这时无法运用上述回退机制。如果投影到复杂属性，可以结合运用（1）和（2）所述的回退机制；但当前版本不支持（2)。
        /// </summary>
        private AssociationTreeNode _atrophyPath;

        /// <summary>
        ///     是否已排序
        /// </summary>
        private bool _hasOrdered;

        /// <summary>
        ///     包含树，包含所有挂起的包含运算，是根据关联关系生成的树形结构，根节点为当前查询结果类型（ResultType），节点表示包含运算的目标。
        /// </summary>
        private AssociationTree _includings;

        /// <summary>
        ///     基点源。
        ///     查询运算开始前和每次确认结果时会生成一个SimpleSource或SelectSource，后续运算可以看成是对该源的逐步修改。该源称为基点源。
        /// </summary>
        private MonomerSource _initialSource;

        /// <summary>
        ///     查询基点类型，即查询源中的对象的类型。注：AcceptResult方法会将当前查询结果类型切换为基点类型。
        /// </summary>
        private Type _initialType;

        /// <summary>
        ///     源联接备忘录。
        /// </summary>
        private JoinMemo _joinMemo;

        /// <summary>
        ///     指示查询结果是否为枚举数。
        /// </summary>
        private bool _resultIsEnum = true;

        /// <summary>
        ///     查询结果类型，如果查询结果为枚举数，为枚举元素的类型，如果为查询结果为单个值，则为该值的类型。
        ///     除以下三种运算外，结果类型等于基点类型：
        ///     （1）聚合运算，结果类型为int、long等值类型；
        ///     （2）测定运算，结果类型为bool；
        ///     （3）投影运算，结果类型为投影表达式的静态类型。
        /// </summary>
        private TypeBase _resultModelType;

        /// <summary>
        ///     作为查询结果的Sql语句。
        /// </summary>
        private QuerySql _resultSql;

        /// <summary>
        ///     构造RopContext的新实例。
        /// </summary>
        /// <param name="initialType">查询基点类型。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="targetSource">数据源类型。</param>
        /// <param name="resultIncluding">相对于查询链结果类型的包含树。</param>
        /// <param name="includingTree">初始包含树。</param>
        public RopContext(Type initialType, ObjectDataModel model, EDataSource targetSource,
            AssociationTree resultIncluding = null, AssociationTree includingTree = null)
        {
            _initialType = initialType;
            _model = model;
            _resultModelType = _model.GetObjectType(initialType);
            IncludingConstructorParameter();


            var objType = Model.GetObjectType(InitialType);
            var sourceName = objType.TargetTable;
            var orderRules = objType.StoringOrder;

            var alias = Utils.GetDerivedTargetTable(objType);
            _initialSource = alias == sourceName ? new SimpleSource(sourceName) : new SimpleSource(sourceName, alias);

            var orders = new List<Order>();
            foreach (var r in orderRules ?? new List<OrderRule>())
            {
                var orderBy = r.OrderBy;
                var inverted = r.Inverted;
                var order = new Order(_initialSource, orderBy.TargetField,
                    inverted ? EOrderDirection.Desc : EOrderDirection.Asc);
                orders.Add(order);
            }

            ((SimpleSource)_initialSource).StoringOrder = orders;
            _resultSql = new QuerySql(_initialSource);

            _resultIncluding = resultIncluding;

            _includings = includingTree ?? new AssociationTree(Model.GetObjectType(ResultType));
            _resultSql.SelectionSet.Add(new WildcardColumn { Source = _initialSource });

            JoinMemo.Append(null, _initialSource);

            _targetSource = targetSource;
        }

        /// <summary>
        ///     别名生成器，用于生成退化路径和包含树各节点的别名。需要时创建。
        /// </summary>
        private AliasGenerator AliasGenerator => _aliasGenerator ?? (_aliasGenerator = new AliasGenerator());

        /// <summary>
        ///     获取查询基点类型，即查询源中的对象的类型。注：AcceptResult方法会将当前查询结果类型切换为基点类型。
        /// </summary>
        public Type InitialType => _initialType;

        /// <summary>
        ///     获取查询结果类型，如果查询结果为枚举数，为枚举元素的类型，如果为查询结果为单个值，则为该值的类型。
        ///     除以下三种运算外，结果类型等于基点类型：
        ///     （1）聚合运算，结果类型为int、long等值类型；
        ///     （2）测定运算，结果类型为bool；
        ///     （3）投影运算，结果类型为投影表达式的静态类型。
        /// </summary>
        public Type ResultType => _resultModelType.ClrType;

        /// <summary>
        ///     获取查询结果类型的模型类型。
        ///     如果查询结果为枚举数，为枚举元素的类型，如果为查询结果为单个值，则为该值的类型。
        ///     除以下三种运算外，结果类型等于基点类型：
        ///     （1）聚合运算，结果类型为int、long等值类型；
        ///     （2）测定运算，结果类型为bool；
        ///     （3）投影运算，结果类型为投影表达式的静态类型。
        /// </summary>
        public TypeBase ResultModelType => _resultModelType;

        /// <summary>
        ///     获取一个值，指示查询结果是否为枚举数。
        /// </summary>
        public bool ResultIsEnum => _resultIsEnum;

        /// <summary>
        ///     获取源联接备忘录。
        /// </summary>
        public JoinMemo JoinMemo => _joinMemo ?? (_joinMemo = new JoinMemo());

        /// <summary>
        ///     获取别名根，在基点类型与投影结果类型之间沿关联关系生成的别名字符串。在联表查询中生成别名时，将以此字符串作为前缀，故称为别名根。
        ///     如果结果类型不为ObjectType，别名根为空。
        /// </summary>
        [Obsolete]
        public string AliasRoot
        {
            get
            {
                if (_aliasRoot != null) return _aliasRoot;
                //退化路径不存在或只有一个根节点
                if (_atrophyPath == null) return null;
                if (_atrophyPath is ObjectTypeNode objectTypeNode && objectTypeNode.Parent == null) return null;
                var nodeAlias = _atrophyPath?.AsTree().Accept(AliasGenerator);
                var source = JoinMemo.GetSource(nodeAlias);
                _aliasRoot = source?.Symbol;
                return _aliasRoot;
            }
        }

        /// <summary>
        ///     获取作为查询结果的Sql语句。
        /// </summary>
        public QuerySql ResultSql
        {
            get => _resultSql;
            internal set => _resultSql = value;
        }

        /// <summary>
        ///     获取生成Sql语句时使用的对象数据模型。
        /// </summary>
        public ObjectDataModel Model => _model;

        /// <summary>
        ///     获取包含树，包含所有挂起的包含运算，是根据关联关系生成的树形结构，根节点为当前查询结果类型（ResultType），节点表示包含运算的目标。
        /// </summary>
        public AssociationTree Includings =>
            _includings ?? (_includings = new AssociationTree(Model.GetObjectType(ResultType)));

        /// <summary>
        ///     获取自查询运算开始或上次AcceptResult以来的退化路径
        /// </summary>
        public AssociationTreeNode AtrophyPath => _atrophyPath;

        /// <summary>
        ///     数据源类型。
        /// </summary>
        public EDataSource SourceType => _targetSource;

        /// <summary>
        ///     相对于查询链结果类型的包含树。
        /// </summary>
        public AssociationTree ResultIncluding => _resultIncluding;

        /// <summary>
        ///     是否已排序
        /// </summary>
        public bool HasOrdered
        {
            get => _hasOrdered;
            set => _hasOrdered = value;
        }

        /// <summary>
        ///     接收当前查询结果并将其作为后续查询的基点，执行以下操作：
        ///     （1）将查询结果类型作为基点类型；
        ///     （2）置空退化路径；
        ///     （3）清空联接备忘录；
        ///     （4）将查询结果更换为以当前结果为源的QuerySql新实例。
        /// </summary>
        public void AcceptResult()
        {
            _initialType = ResultType; //将查询结果类型作为基点类型；
            _atrophyPath = null; //置空退化路径；
            _aliasRoot = null; //清空根别名；
            JoinMemo.Reset(); //清空联接备忘录；

            //将查询结果更换为以当前结果为源的QuerySql新实例。
            _initialSource = _resultModelType is IMappable mappable
                ? mappable is ObjectType objectType
                    ? new SelectSource(_resultSql, Utils.GetDerivedTargetTable(objectType))
                    : new SelectSource(_resultSql, mappable.TargetName)
                : new SelectSource(_resultSql, "OTB");
            //构造一通配符列
            var wildcardColumn = new WildcardColumn { Source = _initialSource };

            _resultSql = new QuerySql(_initialSource); //将查询结果类型切换为基点类型
            //加入至结果Sql的投影集
            _resultSql.SelectionSet.Add(wildcardColumn);
            if (_resultSql.TakeNumber == 0 && _resultSql.Source.CanBubbleOrder) _resultSql.BubbleOrder();
            JoinMemo.Append(null, _initialSource);
            //设置为 未排序
            _hasOrdered = false;
        }

        /// <summary>
        ///     依据关联关系拓展源以使其覆盖包含树，同时填写源联接备忘录。
        /// </summary>
        /// <param name="assoTree">要覆盖的关联树。</param>
        /// <param name="autoDistinct">是否自动去重。</param>
        public void ExpandSource(AssociationTree assoTree, bool autoDistinct = true)
        {
            if (assoTree?.SubTrees.Length > 0)
            {
                //无排序 则进行排序冒泡
                if (_resultSql.Orders.Count == 0 && _resultSql.Source.CanBubbleOrder) _resultSql.BubbleOrder();
                var baseSource = JoinMemo.GetSource(AliasRoot);
                var source = _resultSql.Source;
                if (assoTree.RepresentedType != null)
                    _resultSql.Source = JoinByAssociationTree(assoTree, baseSource, AliasRoot, source, autoDistinct);
            }
        }

        /// <summary>
        ///     依据关联关系拓展源以使其覆盖指定的表达式，同时填写源联接备忘录。
        /// </summary>
        /// <param name="expression">要覆盖的表达式。</param>
        /// <param name="joinType">Join运算类型</param>
        /// <param name="autoDistinct">是否自动去重</param>
        public void ExpandSource(Expression expression, ESourceJoinType joinType = ESourceJoinType.Left,
            bool autoDistinct = true)
        {
            //构造表达式提取器
            var memberExpressionExtractor = new MemberExpressionExtractor(new SubTreeEvaluator(expression));
            var members = memberExpressionExtractor.ExtractMember(expression);
            //冒泡排序
            if (members.Count > 0 && _resultSql.Orders.Count == 0 && _resultSql.Source.CanBubbleOrder)
                _resultSql.BubbleOrder();

            var baseSource = JoinMemo.GetSource(AliasRoot);

            //所有连接源
            var source = _resultSql.Source;
            foreach (var member in members)
            {
                var assoTree = member.ExtractAssociation(_model);
                if (assoTree == null) continue;
                source = JoinByAssociationTree(assoTree, baseSource, AliasRoot, source, autoDistinct, joinType);
            }

            _resultSql.Source = source;
        }

        /// <summary>
        ///     依据关联关系拓展源以使其覆盖指定的关联树，同时填写源联接备忘录。
        /// </summary>
        /// <param name="autoDistinct">是否自动去重</param>
        public void ExpandSource(bool autoDistinct = true)
        {
            ExpandSource(_includings, autoDistinct);
        }

        /// <summary>
        ///     强制包含引用型构造参数（即绑定到引用元素的参数）。
        /// </summary>
        private void IncludingConstructorParameter()
        {
            if (ResultModelType is ReferringType referringType)
            {
                var paras = referringType.Constructor.Parameters;
                _includings = new AssociationTree(referringType);
                foreach (var para in paras ?? new List<Parameter>())
                {
                    if (para.ElementType == EElementType.Attribute) continue;
                    _includings.Grow(para.ElementName);
                }
            }
        }

        /// <summary>
        ///     根据关联树联接源
        /// </summary>
        /// <param name="assoTree">关联树</param>
        /// <param name="baseSource">基础源</param>
        /// <param name="baseAlias">基础别名</param>
        /// <param name="leftSource">左端源</param>
        /// <param name="autoDistinct">是否自动去重</param>
        /// <param name="joinType">联接类型。</param>
        private ISource JoinByAssociationTree(AssociationTree assoTree, MonomerSource baseSource, string baseAlias,
            ISource leftSource, bool autoDistinct = true, ESourceJoinType joinType = ESourceJoinType.Left)
        {
            //构造连接器
            var objType = assoTree.RepresentedType;
            var sourcejoin = new SourceJoiner(objType, baseSource, baseAlias, leftSource);

            var subTrees = assoTree.SubTrees;
            //返回值
            var resultSource = leftSource;
            foreach (var sub in subTrees)
            {
                var elementName = sub.ElementName;
                var joinedSource = sourcejoin.Join(elementName, out var targetSource, out var targetAlias, joinType);

                if (JoinMemo.Exists(targetAlias) == false)
                {
                    JoinMemo.Append(targetAlias, targetSource);
                    var element = objType.GetElement(elementName);

                    //如果_resultModelType为主引类型 且为一对多 并自动去重 则对查询结果去重
                    if (_resultModelType is ReferringType && element != null && element.IsMultiple && autoDistinct)
                        _resultSql.Distinct = true;
                    resultSource = joinedSource;
                }

                resultSource =
                    JoinByAssociationTree(sub, targetSource, targetAlias, resultSource, autoDistinct, joinType);
                sourcejoin.LeftSource = resultSource;
            }

            return resultSource;
        }

        /// <summary>
        ///     在退化投影运算完成时设置运算结果类型，根据该运算在关联树上的投影结果和在属性树上的投影结果。
        ///     实施说明：
        ///     （1）如果attrTreeResult == null，延长退化路径；否则，置空退化路径；
        ///     （2）如果attrTreeResult != null且pipelineEnded == false，强制执行AcceptResult。
        ///     详见活动图“设置投影结果类型（一）”。
        ///     理论基础：
        ///     投影操作可以形象地理解为在关联树中寻找一个节点，当attrTreeResult != null时则需要继续在锚定于此节点的某一属性树上寻找一个节点。
        ///     后续运算将以此节点为根构建一棵新包含树。
        ///     作为后续运算参数的表达式是与新包含树相对应的，如o.PropA中的o即对应于新树的根。而投影操作完成时，结果SQL语句的查询源（From
        ///     子句）仍然对应于原树，因此在对表达式进行解析时，需要借助退化路径将新树的节点回退到原树，具体而言主要是两个问题：
        ///     （1）从表达式解析出的查询源，需要在其别名前附加一个前缀，该前缀根据关联退化路径生成；
        ///     （2）从表达式解析出的映射字段，需要在其名称前附加一个前缀，该前缀根据属性退化路径生成。
        ///     如果投影结果为基元类型，它可能是由多个简单属性经数学运算而成的，这时无法运用上述回退机制，因此必须强制AcceptResult。但从优化性能考虑，如果运算管道已
        ///     结束，则可以不再执行AcceptResult。
        ///     如果投影到复杂属性，可以结合运用（1）和（2）所述的回退机制；但当前版本不支持（2），故参照投影到基元类型的方案。
        /// </summary>
        /// <param name="assoResult">在关联树上的投影结果。</param>
        /// <param name="attrResult">在属性树上的投影结果。</param>
        /// <param name="pipelineEnded">指示运算管道是否已终结。</param>
        public void SetResultType(AssociationTreeNode assoResult, AttributeTreeNode attrResult, bool pipelineEnded)
        {
            _resultIsEnum = true;

            if (attrResult == null)
            {
                _resultModelType = assoResult.RepresentedType;
                var include = _includings.SearchSub(assoResult);
                _includings = include;
                IncludingConstructorParameter();

                var sub = _atrophyPath?.AsTree().Root.SubTrees[0];
                if (sub != null) assoResult.AddChild((ObjectTypeNode)sub.Node);
                _aliasRoot = null;
                _atrophyPath = assoResult;
            }
            else
            {
                _resultModelType = attrResult.AttributeType;
                _includings = null;
                _aliasRoot = null;
                _atrophyPath = null;

                if (!pipelineEnded)
                    AcceptResult();
            }
        }

        /// <summary>
        ///     设置查询结果类型为一个基元类型，同时清空退化路径和包含树。
        ///     理论说明：
        ///     当投影结果为基元类型时，它可能是由多个简单属性经数学运算而成的，这时无法运用回退机制，因此必须强制执行AcceptResult。但从优化性能考虑，如果运算管道已
        ///     结束，则可以不再执行AcceptResult。
        /// </summary>
        /// <param name="primitiveType">运算结果类型。</param>
        /// <param name="isEnumerable">指示运算结果是否为可枚举的。</param>
        /// <param name="pipelineEnded">指示运算管道是否已终结。</param>
        public void SetResultType(PrimitiveType primitiveType, bool isEnumerable, bool pipelineEnded)
        {
            var typeBase = _model.GetTypeOrNull(primitiveType.ClrType);
            _resultModelType = typeBase ?? primitiveType;
            _resultIsEnum = isEnumerable;
            _includings = null;
            if (!pipelineEnded)
                AcceptResult();
        }

        /// <summary>
        ///     在一般投影运算完成时将查询结果类型设置为类型视图，（强制为可枚举类型），同时根据视图结构裁剪关联树。
        /// </summary>
        /// <param name="typeView">类型视图。</param>
        /// <param name="pipelineEnd">指示运算管道是否已终结。</param>
        public void SetResultType(TypeView typeView, bool pipelineEnd)
        {
            _resultModelType = typeView;
            var newIncludings = new AssociationTree(typeView);

            var elements = typeView.ReferenceElements;
            //裁剪包含树
            foreach (var referenceElement in elements)
                if (referenceElement is ViewReference viewReference)
                {
                    var anchorTree = newIncludings.SearchSub(viewReference.Anchor);
                    if (anchorTree != null)
                    {
                        var bindingTree = newIncludings.RemoveSub(viewReference.Binding.Name);
                        if (bindingTree != null) newIncludings.AddSubTree(bindingTree, viewReference.Name);
                    }

                    //分解异构视图生成的不进行生长
                    if (!typeView.IsDecomposeExtremelyResult)
                        newIncludings.Grow(viewReference.Name);
                }

            _includings = newIncludings;
            if (!pipelineEnd)
                AcceptResult();
        }

        /// <summary>
        ///     在当前查询结果中追加一个索引列。
        /// </summary>
        public void AddIndexColumn()
        {
            switch (SourceType)
            {
                case EDataSource.SqlServer:
                {
                    if (_resultSql.Orders.Count == 0) _resultSql.BubbleOrder();
                    var index = SqlObject.Expression.Function("row_number");
                    var over = new OverClause(_resultSql.Orders.ToArray());
                    index.Over = over;
                    var alias = "obase$index";
                    _resultSql.SelectionSet?.Add(index, alias);
                    AcceptResult();
                    break;
                }
                case EDataSource.Oracle:
                case EDataSource.Oledb:
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}