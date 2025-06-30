/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构查询分解器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:08:31
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Query.TypeViews;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构查询分解器。
    /// </summary>
    public class HeterogQueryDecomposer : QueryOpVisitorWithArgs<AssociationTree, HeterogQuerySegments>
    {
        /// <summary>
        ///     断言器
        /// </summary>
        private readonly HeterogeneityPredicationProvider _heterogeneityPredicationProvider;

        /// <summary>
        ///     主查询链分离出异构链后剩余的部分，称为补充链。
        /// </summary>
        private QueryOp _complement;

        /// <summary>
        ///     包含树，以当前访问节点为基点
        /// </summary>
        private AssociationTree _including;

        /// <summary>
        ///     从主查询中分离出的异构查询，称为主体链。
        /// </summary>
        private QueryOp _mainQuery;

        /// <summary>
        ///     异构运算，主查询链从该运算节点处离断，前半段（含该节点）为异构查询，后半段为后续查询。
        /// </summary>
        private QueryOp _mainTail;

        /// <summary>
        ///     前一步的包含树
        /// </summary>
        private AssociationTree _previousIncluding;

        /// <summary>
        ///     是否支持
        /// </summary>
        private bool _supported = true;

        /// <summary>
        ///     初始化分解器。
        /// </summary>
        public HeterogQueryDecomposer(HeterogeneityPredicationProvider heterogeneityPredicationProvider = null)
        {
            _heterogeneityPredicationProvider = heterogeneityPredicationProvider;
            SpecifyQueryOp();
        }

        /// <summary>
        ///     获取访问操作的结果。
        /// </summary>
        public override HeterogQuerySegments Result
        {
            get
            {
                //创建分解得到的片段
                var segments = new HeterogQuerySegments
                {
                    MainQuery = _mainQuery,
                    Complement = _complement,
                    Including = _supported ? _including : _previousIncluding,
                    MainTail = _mainTail
                };
                _mainTail = null;
                _mainQuery = null;
                _complement = null;
                _including = null;
                _supported = true;
                return segments;
            }
        }

        /// <summary>
        ///     获取或设置访问操作参数。
        /// </summary>
        internal override AssociationTree Argument { get; set; }

        /// <summary>
        ///     针对特定操作进行额外的特殊处理
        /// </summary>
        private void SpecifyQueryOp()
        {
            //1.生成包含树，是指生成当前包含树（_including），让它覆盖Include运算指定的包含树（可通过IncludOp类获取）。
            //2. “裁剪包含树”的返回值替换当前包含树。
            //3.裁剪包含树是指裁剪当前包含树（_including）。
            //4.抽取关联链，是指获取所述成员表达式指向的关联树节点，即MemberExtension.ExtractAssciation方法；裁剪包含树是指裁剪当前包含树，即CutIncluding(_including, 关联链)。
            //5.置空包含树是指，_including = null。

            //构造一个针对以上特定几种操作的断言函数
            ESpecialPredicate Predicate(QueryOp op)
            {
                switch (op.Name)
                {
                    case EQueryOpName.Include:
                        return ESpecialPredicate.PostExecute;
                    case EQueryOpName.Select:
                    case EQueryOpName.Group:
                    case EQueryOpName.Count:
                    case EQueryOpName.ArithAggregate:
                    case EQueryOpName.Accumulate:
                    case EQueryOpName.Join:
                    case EQueryOpName.Zip:
                    {
                        //这些操作不是异构的
                        if (!op.Heterogeneous(_heterogeneityPredicationProvider)) return ESpecialPredicate.PostExecute;
                        return ESpecialPredicate.False;
                    }
                    default:
                        return ESpecialPredicate.False;
                }
            }

            //1.Include
            //生长包含树。
            //Inculde操作的具体委托
            var includePrevisit = new Previsit((QueryOp op, object state, out object previousState,
                out object previsitState) =>
            {
                if (op is IncludeOp includeOp)
                    //生长包含树
                    _including = _including == null
                        ? includeOp.IncludingTree
                        : _including.Grow(includeOp.IncludingTree);

                previousState = null;
                previsitState = null;
                return false;
            });
            //加入指定的操作
            Specify(EQueryOpName.Include, includePrevisit, Predicate);

            //2.Select
            //裁剪包含树，根据投影运算的不同状态选择CutIncluding方法的不同重载。
            //Select操作的具体委托
            var selectPrevisit = new Previsit((QueryOp op, object state, out object previousState,
                out object previsitState) =>
            {
                if (op is SelectOp selectOp)
                {
                    if (selectOp.ResultView != null)
                    {
                        //裁剪包含树
                        _including = CutIncluding(_including, selectOp.ResultView);
                    }
                    else
                    {
                        var objectType = selectOp.Model.GetObjectType(selectOp.ResultType);
                        if (objectType != null)
                        {
                            //裁剪包含树
                            var path = selectOp.AtrophyPath == null
                                ? new ObjectTypeNode(objectType)
                                : selectOp.AtrophyPath.AssociationPath;
                            _including = CutIncluding(_including, path);
                            if (_including != null)
                            {
                                //切下来的首个是关联端 取子树
                                if (_including.Element is AssociationEnd)
                                    _including = _including.SubTrees.FirstOrDefault();
                                //自己投自己 置空
                                if (selectOp.ResultType == selectOp.SourceType)
                                    _including = null;
                            }
                        }
                    }
                }

                previousState = null;
                previsitState = null;
                return false;
            });


            //加入指定的操作
            Specify(EQueryOpName.Select, selectPrevisit, Predicate);

            //3.Group
            //如果是普通分组且组元素函数不为空，且组元素函数为成员表达式且指向对象类型（ObjectType），将该成员表达式作为退化路径裁剪包含树。
            //否则，置空包含树。
            //Group操作的具体委托
            var groupPrevisit = new Previsit((QueryOp op, object state, out object previousState,
                out object previsitState) =>
            {
                if (op is GroupOp groupOp)
                {
                    if (groupOp.KeySelector.Body is MemberExpression memberExpression)
                    {
                        var objectType = op.Model.GetObjectType(memberExpression.Type);
                        if (objectType != null)
                        {
                            //裁剪包含树
                            var path = new ObjectTypeNode(objectType);
                            _including = CutIncluding(_including, path);
                        }
                        else
                        {
                            //置空包含树
                            _including = null;
                        }
                    }
                    else
                    {
                        //置空包含树
                        _including = null;
                    }
                }

                previousState = null;
                previsitState = null;
                return false;
            });


            //加入指定的操作
            Specify(EQueryOpName.Group, groupPrevisit, Predicate);

            //4.Count、ArithAggregate
            //如果Seed不为空且SeedType与查询源类型不相同，置空包含树；否则，当ResultSelector不为空时，检查其Body：
            // （1）如果Body为NewExpression或MemberInitExpression，将其作为视图表达式生成视图，根据此视图裁剪包含树；
            // （2）如果为成员表达式且返回对象类型（序列），抽取关联链，据此关联链裁剪包含树；
            // （3）其它情况下，置空包含树。
            //6.Join、Zip
            //置空包含树。
            //Count、ArithAggregate、Join、Zip操作的具体委托
            var countOrArithAggregatePrevisit = new Previsit((QueryOp op, object state, out object previousState,
                out object previsitState) =>
            {
                if (op is CountOp)
                    //置空包含树
                    _including = null;

                if (op is ArithAggregateOp)
                    //置空包含树
                    _including = null;

                if (op is JoinOp)
                    //置空包含树
                    _including = null;

                if (op is ZipOp)
                    //置空包含树
                    _including = null;

                previousState = null;
                previsitState = null;
                return false;
            });

            //加入指定的操作
            Specify(EQueryOpName.Count, countOrArithAggregatePrevisit, Predicate);
            Specify(EQueryOpName.ArithAggregate, countOrArithAggregatePrevisit, Predicate);
            Specify(EQueryOpName.Join, countOrArithAggregatePrevisit, Predicate);
            Specify(EQueryOpName.Zip, countOrArithAggregatePrevisit, Predicate);

            //5.Accumulate
            //如果Seed不为空且SeedType与查询源类型不相同，置空包含树；否则，当ResultSelector不为空时，检查其Body：
            //（1）如果Body为NewExpression或MemberInitExpression，将其作为视图表达式生成视图，根据此视图裁剪包含树；
            //（2）如果为成员表达式且返回对象类型（序列），抽取关联链，据此关联链裁剪包含树；
            //（3）其它情况下，置空包含树。
            //Accumulate的具体委托
            var accumulatePrevisit = new Previsit((QueryOp op, object state, out object previousState,
                out object previsitState) =>
            {
                if (op is AccumulateOp accumulateOp)
                {
                    if (accumulateOp.Seed != null && accumulateOp.SeedType != accumulateOp.SourceType)
                    {
                        //置空包含树
                        _including = null;
                    }
                    else if (accumulateOp.ResultSelector != null)
                    {
                        var body = accumulateOp.ResultSelector.Body;
                        if (body is NewExpression || body is MemberInitExpression)
                        {
                            ITypeViewBuilder builder;
                            if (body is NewExpression)
                                builder = new NewExpressionBasedBuilder();
                            else
                                builder = new MemberInitExpressionBasedBuilder();
                            var typeView = builder.Build(body, op.Model.GetStructuralType(body.Type), op.Model,
                                accumulateOp.ResultSelector.Parameters[0]);
                            //裁剪包含树
                            _including = CutIncluding(accumulateOp.ImpliedIncluding, typeView);
                        }
                        else if (body is MemberExpression memberExpression &&
                                 memberExpression.Type == accumulateOp.ResultType)
                        {
                            //裁剪包含树
                            var tree = memberExpression.ExtractAssociation(op.Model);
                            _including = CutIncluding(accumulateOp.ImpliedIncluding, tree.Node);
                        }
                        else
                        {
                            //置空包含树
                            _including = null;
                        }
                    }
                }

                previousState = null;
                previsitState = null;
                return false;
            });
            //加入指定的操作
            Specify(EQueryOpName.Accumulate, accumulatePrevisit, Predicate);
        }

        /// <summary>
        ///     根据退化投影路径裁剪包含树。
        /// </summary>
        /// <returns>
        ///     裁剪后得到的新包含树。
        ///     实施说明
        ///     使用AssociationTree.Search方法搜索退化路径指向的子树，以该子树作为新包含树返回。
        /// </returns>
        /// <param name="including">要裁剪的包含树。</param>
        /// <param name="assoResult">退化路径。</param>
        private AssociationTree CutIncluding(AssociationTree including, AssociationTreeNode assoResult)
        {
            return including?.SearchSub(assoResult);
        }

        /// <summary>
        ///     根据一般投影结果视图裁剪包含树。
        /// </summary>
        /// <returns>
        ///     裁剪后的新包含树。
        ///     实施说明
        ///     参照活动图“Rop/关系运算上下文/设置投影结果类型（二）”。
        /// </returns>
        /// <param name="including">要裁剪的包含树。</param>
        /// <param name="typeView">投影结果视图。</param>
        private AssociationTree CutIncluding(AssociationTree including, TypeView typeView)
        {
            var newIncludings = new AssociationTree(typeView);

            var elements = typeView.ReferenceElements;
            //裁剪包含树
            foreach (var referenceElement in elements)
                if (referenceElement is ViewReference viewReference)
                {
                    var anchorTree = including.SearchSub(viewReference.Anchor);
                    if (anchorTree != null)
                    {
                        var bindingTree = newIncludings.RemoveSub(viewReference.Binding.Name);
                        if (bindingTree != null) newIncludings.AddSubTree(bindingTree, viewReference.Name);
                    }

                    newIncludings.Grow(viewReference.Name);
                }

            return newIncludings;
        }

        /// <summary>
        ///     执行通用后置访问逻辑。
        /// </summary>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        protected override bool PostvisitGenerally(QueryOp queryOp, object previousState, object previsitState)
        {
            if (_supported == false)
                _supported = true;

            //包含运算 直接返回
            if (queryOp.Name == EQueryOpName.Include) return false;

            QueryOp currentNode;
            if (_mainQuery != null)
            {
                currentNode = queryOp.Clone(_mainQuery);
            }
            else
            {
                if (queryOp is SelectOp selectOp && selectOp.IsNew == false)
                    currentNode = queryOp.Clone(queryOp.Next);
                else
                    currentNode = queryOp.Clone(null);
            }


            //赋值
            _mainQuery = currentNode;
            if (_mainTail != null) return false;

            _mainTail = currentNode;
            return false;
        }

        /// <summary>
        ///     执行通用前置访问逻辑。
        /// </summary>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="outPreviousState">返回一个状态数据，在遍历到下一运算时该数据将被视为前序状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        protected override bool PrevisitGenerally(QueryOp queryOp, object previousState, out object outPreviousState,
            out object outPrevisitState)
        {
            outPreviousState = null;
            outPrevisitState = null;

            if (queryOp.Name == EQueryOpName.Include)
            {
                _previousIncluding = _including;
                return true;
            }

            if (!queryOp.Heterogeneous(_heterogeneityPredicationProvider))
            {
                _previousIncluding = _including;
                return true;
            }

            _supported = queryOp.Name == EQueryOpName.Select || queryOp.Name == EQueryOpName.Where ||
                         queryOp.Name == EQueryOpName.Group;

            if (queryOp is SelectOp selectOp && selectOp.IsNew == false) return false;

            if (_supported)
            {
                outPreviousState = queryOp;
                _complement = queryOp.Next;
            }
            else
            {
                _complement = ((QueryOp)previousState).Next;
            }

            //合并
            var chain = queryOp.GetChainIncluding();
            AssociationTree.Combine(chain, _supported ? _including : _previousIncluding);

            return false;
        }
    }
}