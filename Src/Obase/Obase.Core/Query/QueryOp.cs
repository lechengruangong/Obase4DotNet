/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:47:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Query.Oop;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示查询运算。
    ///     对对象集的一次操作称为一次查询运算，System.Linq.Queryable类定义的扩展方法定义了绝大多数查询运算。当前支持的所有查询运算请参见eQueryOpName枚举。
    ///     多个查询运算串联构成一个查询链，或称查询表达式。运算串联是指前一个运算的结果作为后一个的源。
    ///     QueryOp类是对查询运算的描述，记载查询的名称、查询源类型、参数等信息，同时引用查询链中的下一个运算。
    /// </summary>
    public abstract class QueryOp
    {
        /// <summary>
        ///     查询运算的名称。
        /// </summary>
        private readonly EQueryOpName _name;

        /// <summary>
        ///     查询源的类型
        /// </summary>
        private readonly Type _sourceType;

        /// <summary>
        ///     查询运算中隐含的包含运算，称为隐含包含。
        ///     如果一个查询运算虽未显示要求包含一个引用，但该运算的执行依赖于该引用，则称该查询运算隐含包含该引用。
        /// </summary>
        private AssociationTree _impliedIncludings;

        /// <summary>
        ///     适用于查询运算的对象数据模型。
        /// </summary>
        protected ObjectDataModel _model;

        /// <summary>
        ///     查询链中的下一个运算。
        /// </summary>
        private QueryOp _next;

        /// <summary>
        ///     寄存器（寄存查询链的尾部节点）。
        /// </summary>
        private QueryOp _tail;

        /// <summary>
        ///     创建QueryOp的新实例。
        /// </summary>
        /// <param name="name">运算名称。</param>
        /// <param name="sourceType">查询源模型类型。</param>
        protected QueryOp(EQueryOpName name, Type sourceType)
        {
            _name = name;
            _sourceType = sourceType;
        }

        /// <summary>
        ///     获取查询链中的下一个运算。
        /// </summary>
        public QueryOp Next => _next;

        /// <summary>
        ///     获取运算名称。
        /// </summary>
        public EQueryOpName Name => _name;

        /// <summary>
        ///     获取运算参数。
        /// </summary>
        [Obsolete("已过时")]
        private Expression[] Arguments => GetArguments();

        /// <summary>
        ///     获取结果类型
        /// </summary>
        public abstract Type ResultType { get; }

        /// <summary>
        ///     获取源对象的类型。
        /// </summary>
        public Type SourceType => _sourceType;

        /// <summary>
        ///     获取查询链的尾部节点。
        /// </summary>
        /// 实施说明:
        /// 遍历查询链直至最后一个节点。
        /// 寄存遍历结果，避免重复遍历。须在ReplaceTail方法中清空此寄存器。
        internal QueryOp Tail
        {
            get
            {
                if (_tail != null) return _tail;
                _tail = _next == null ? this : _next.Tail;
                return _tail;
            }
        }

        /// <summary>
        ///     获取查询运算的隐含包含。
        ///     如果一个查询运算虽未显示要求包含一个引用，但该运算的执行依赖于该引用，则称该查询运算隐含包含该引用。
        /// </summary>
        /// 实施说明:
        /// 使用TakeImpliedIncluding方法提取隐含包含。寄存提取结果，避免重复提取。
        public AssociationTree ImpliedIncluding => _impliedIncludings ?? (_impliedIncludings = TakeImpliedIncluding());

        /// <summary>
        ///     获取适用于查询运算的对象数据模型。
        ///     如果查询源为基元类型，返回null。
        /// </summary>
        /// 实施说明:
        /// 如果查询源为基元类型，返回null；
        /// 否则，返回查询源类型所属的模型。
        public ObjectDataModel Model => PrimitiveType.IsObasePrimitiveType(_sourceType) ? null : _model;

        /// <summary>
        ///     查询源的模型类型
        /// </summary>
        public TypeBase SourceModelType => _model?.GetType(_sourceType);

        /// <summary>
        ///     获取一个值，该值指示查询运算是否为异构的。
        ///     实施说明
        ///     使用IsHeterogeneous方法判定是否为异构。
        ///     为避免重复判定操作，应当寄存判定结果
        /// </summary>
        /// <param name="predicationProvider">异构断言提供程序，如果不指定将使用默认的存储异构断言提供程序。</param>
        public bool Heterogeneous(HeterogeneityPredicationProvider predicationProvider = null)
        {
            //没有指定异构断言提供程序，则使用默认的存储异构断言提供程序。
            if (predicationProvider == null)
                predicationProvider = new StorageHeterogeneityPredicationProvider();
            return IsHeterogeneous(predicationProvider);
        }

        /// <summary>
        ///     接受访问者对查询链各节点的访问。
        /// </summary>
        /// <param name="visitor">一个查询链访问者，它执行不接收参数且有返回值的操作。</param>
        public TResult Accept<TResult>(QueryOpVisitor<TResult> visitor)
        {
            Accept(visitor, null);
            return visitor.Result;
        }

        /// <summary>
        ///     接受访问者对查询链各节点的访问。
        /// </summary>
        /// <param name="visitor">一个查询链访问者，它执行不接收参数且有返回值和一个输出参数的操作。</param>
        /// <param name="outArg">输出参数的值。</param>
        public TResult Accept<TResult, TOut>(QueryOpVisitorWithOutArgs<TResult, TOut> visitor, out TOut outArg)
        {
            Accept(visitor, null);
            outArg = visitor.OutArgument;
            return visitor.Result;
        }


        /// <summary>
        ///     接受访问者对查询链各节点的访问。
        /// </summary>
        /// <param name="visitor">一个查询链访问者，它执行不接收参数且有返回值和一个访问参数的操作。</param>
        /// <param name="arg">输出参数的值。</param>
        public TResult Accept<TArg, TResult>(QueryOpVisitorWithArgs<TArg, TResult> visitor, TArg arg)
        {
            visitor.Argument = arg;
            Accept(visitor, null);
            return visitor.Result;
        }

        /// <summary>
        ///     接受访问者对查询链各节点的访问。
        /// </summary>
        /// <param name="visitor">一个查询链访问者，它执行不接收参数且无返回值的操作。</param>
        /// <param name="previousState">前一个操作的状态值</param>
        public void Accept(QueryOpVisitor visitor, object previousState)
        {
            //前置访问
            var re = visitor.Previsit(this, null, out var outPreviousState, out var outPrevisitState); //访问后续节点前 执行操作。
            //结果是true，表示继续访问后续节点
            if (re)
                //后续节点接受访问者
                Next?.Accept(visitor, outPreviousState);
            //后置访问
            visitor.Postvisit(this, previousState, outPrevisitState);
        }

        /// <summary>
        ///     生成查询运算的副本，并将该副本作为指定运算的前序运算。
        /// </summary>
        /// <param name="nextOp">副本的后续运算。</param>
        public QueryOp Clone(QueryOp nextOp)
        {
            var newOp = (QueryOp)MemberwiseClone();
            newOp._next = nextOp;
            return newOp;
        }

        /// <summary>
        ///     为查询链生成对象运算管道。
        /// </summary>
        /// 实施说明:
        /// 实例化OopPipelineBuilder，将其作为查询链访问者调用Accept方法。
        public OopExecutor GeneratePipeline()
        {
            var builder = new OopPipelineBuilder();
            Accept(builder);
            return builder.Result;
        }

        /// <summary>
        ///     使用指定的对象运算管道生成器，为查询链生成对象运算管道。
        /// </summary>
        /// <param name="builder">运算管道生成器。</param>
        public OopExecutor GeneratePipeline(OopPipelineBuilder builder)
        {
            Accept(builder);
            return builder.Result;
        }

        /// <summary>
        ///     获取查询链中所有包含运算（显式或隐含）的包含链构成的包含树，该树根节点代表查询链的基点源类型。
        /// </summary>
        /// 实施说明:
        /// 使用IncludingCollector收集包含运算，该收集器会将收到的包含链沿退化路径反向溯源到基点类型。
        public AssociationTree GetChainIncluding()
        {
            //没有隐含包含，直接返回null。
            if (ImpliedIncluding == null)
                return null;
            //使用包含收集器收集包含运算
            var collector = new IncludingCollector(ImpliedIncluding.RepresentedType);
            Accept(collector);
            return collector.Result[0];
        }

        /// <summary>
        ///     从查询链首跳过指定步骤后截取剩余部分。
        /// </summary>
        /// <param name="stepCount">从1开始的步骤数。</param>
        public QueryOp Jump(int stepCount)
        {
            var nextOp = _next;
            while (stepCount-- > 0)
            {
                if (nextOp == null) throw new ArgumentException("跳过步骤数过长");
                nextOp = nextOp.Next;
            }

            return Clone(nextOp);
        }

        /// <summary>
        ///     判定查询运算是否是异构的。
        ///     实施说明
        ///     遍历Arguments，对于每一个不是常量表达式的参数，从中提取成员表达式然后抽取关联树，如果关联树是异构的，则该参数是异构的。只要有一个参数是异构的，则判定该
        ///     运算是异构的。
        /// </summary>
        /// <param name="predicationProvider">异构断言提供程序，如果不指定将使用默认的存储异构断言提供程序。</param>
        protected virtual bool IsHeterogeneous(HeterogeneityPredicationProvider predicationProvider = null)
        {
            //获取查询运算的参数
            foreach (var argument in Arguments)
            {
                //只处理成员访问参数
                if (argument == null || argument.NodeType != ExpressionType.MemberAccess) continue;
                var memberExp = (MemberExpression)argument;
                if (_model == null)
                    return false;
                var assoTree = memberExp.ExtractAssociation(_model);
                //使用关联树异构断言判定关联树是否是异构的
                var heterogeneityPredicater = new AssociationTreeHeterogeneityPredicater(predicationProvider);
                assoTree.Accept(heterogeneityPredicater);
                if (heterogeneityPredicater.Result) return true;
            }

            return false;
        }

        /// <summary>
        ///     从查询运算中提取隐含包含。
        /// </summary>
        /// 实施说明:
        /// 遍历Arguments，对于每一个不是常量表达式的参数，从中提取成员表达式，然后以该表达式生长包含树。
        protected virtual AssociationTree TakeImpliedIncluding()
        {
            //获取查询运算的参数
            foreach (var argument in Arguments)
            {
                //只处理成员访问参数
                if (argument == null || argument.NodeType != ExpressionType.MemberAccess) continue;
                var member = (MemberExpression)argument;
                _impliedIncludings?.Grow(member.Member.Name);
            }

            return _impliedIncludings;
        }


        /// <summary>
        ///     将查询链的末节点替换为新的运算。
        /// </summary>
        /// <returns>返回替换末节点后的查询链。如果查询只有一个节点，返回的是新节点；否则返回的是当前节点。</returns>
        /// <param name="newTail">新的末节点，值为null表示移除当前末节点。</param>
        internal QueryOp ReplaceTail(QueryOp newTail)
        {
            if (_next == null) return newTail;
            var current = this;
            var currentNext = _next;
            _tail = newTail;
            //找到末节点
            while (currentNext?.Next != null)
            {
                current = currentNext;
                currentNext = currentNext?._next;
                current._tail = newTail;
            }

            //替换末节点
            current._next = newTail;
            current._tail = newTail;
            return this;
        }

        /// <summary>
        ///     在查询链中搜索指定子链，并将其替换为指定的新子链。
        /// </summary>
        /// <returns>返回替换后的查询链。如果查询只有一个节点，返回的是新子链；否则返回的是当前链。</returns>
        /// <param name="subChain">要替换的子链。</param>
        /// <param name="newSub">新的子链，值为null表示移除指定子链。</param>
        internal QueryOp Replace(QueryOp subChain, QueryOp newSub)
        {
            if (_next == null) return newSub;
            var current = this;
            var currentNext = _next;
            if (currentNext == subChain)
                current._next = newSub;
            //找到末节点
            while (currentNext?.Next != null)
            {
                if (currentNext == subChain)
                {
                    current._next = newSub;
                    break;
                }

                //替换
                current = currentNext;
                currentNext = currentNext.Next;
            }

            return this;
        }

        /// <summary>
        ///     由实现类重写 获取表达式参数
        /// </summary>
        /// <returns></returns>
        protected virtual Expression[] GetArguments()
        {
            return Array.Empty<Expression>();
        }


        /// <summary>
        ///     作为一个查询运算访问者，收集查询链中的包含运算（显式或隐含），并将收集到的包含链沿退化路径反向溯源到基点类型。
        /// </summary>
        private class IncludingCollector : QueryOpVisitor<AssociationTree[]>
        {
            /// <summary>
            ///     退化投影运算导致查询源类型沿关联关系退化的记录。
            /// </summary>
            private readonly List<AssociationTreeNode> _atrophies = new List<AssociationTreeNode>();

            /// <summary>
            ///     初始化IncludingCollector的新实例。
            /// </summary>
            /// <param name="initialType">查询基点类型。</param>
            /// 实施说明:
            /// 构造包含树根作为初始结果。
            public IncludingCollector(ReferringType initialType)
            {
                _result = new[] { new AssociationTree(initialType) };
            }

            /// <summary>
            ///     执行通用后置访问逻辑。
            /// </summary>
            /// <param name="queryOp">要访问的查询运算。</param>
            /// <param name="previousState">访问前一运算时产生的状态数据。</param>
            /// <param name="previsitState">前置访问产生的状态数据。</param>
            protected override bool PostvisitGenerally(QueryOp queryOp, object previousState, object previsitState)
            {
                //后置访问 没有具体的处理
                return true;
            }

            /// <summary>
            ///     执行通用前置访问逻辑。
            /// </summary>
            /// <param name="queryOp">要访问的查询运算。</param>
            /// <param name="previousState">访问前一运算时产生的状态数据。</param>
            /// <param name="outPreviousState">返回一个状态数据，在遍历到下一运算时该数据将被视为前序状态。</param>
            /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
            protected override bool PrevisitGenerally(QueryOp queryOp, object previousState,
                out object outPreviousState, out object outPrevisitState)
            {
                outPreviousState = outPrevisitState = null;
                //特定于投影运算的前置访问逻辑
                if (queryOp is SelectOp selectOp)
                {
                    //指定一个断言函数 始终返回PostExecute
                    Specify(EQueryOpName.Select, Previsit, op => ESpecialPredicate.PostExecute);
                    var atrophyPath = selectOp.AtrophyPath;
                    if (atrophyPath != null) _atrophies.Add(atrophyPath.AssociationPath);
                }
                //通用前置访问逻辑
                else
                {
                    var targetTree = queryOp.Name == EQueryOpName.Include
                        ? ((IncludeOp)queryOp).IncludingTree
                        : queryOp.ImpliedIncluding;

                    if (targetTree != null)
                        foreach (var tree in _result ?? Array.Empty<AssociationTree>())
                            tree.Grow(targetTree, _atrophies?.ToArray() ?? Array.Empty<AssociationTreeNode>());
                }

                return true;
            }
        }

        #region 静态方法

        /// <summary>
        ///     创建表示Accumulate运算的QueryOp实例。
        /// </summary>
        /// <param name="accumulator">累加函数。</param>
        /// <param name="seed">种子值。</param>
        /// <param name="resultSelector">结果函数，用于将累加器的最终值转换为结果值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Accumulate(LambdaExpression accumulator, object seed, LambdaExpression resultSelector,
            ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new AccumulateOp(accumulator, seed, resultSelector, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示All运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp All(LambdaExpression predicate, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new AllOp(predicate, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Any运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Any(LambdaExpression predicate, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new AnyOp(predicate, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Any运算的QueryOp实例
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算</param>
        /// <returns></returns>
        public static QueryOp Any(Type sourceType, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new AnyOp(sourceType) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示运算符为Average的算术聚合运算的QueryOp实例。
        /// </summary>
        /// <param name="selector">投影函数，应用于每个元素然后以投影结果参与聚合。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Average(LambdaExpression selector, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new ArithAggregateOp(EAggregationOperator.Average, model, selector) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Cast运算的QueryOp实例。
        /// </summary>
        /// <param name="scourceType">查询源类型。</param>
        /// <param name="resultType">转换目标类型。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Cast(Type scourceType, Type resultType, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new CastOp(resultType, scourceType) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Contains运算的QueryOp实例。
        /// </summary>
        /// <param name="item">要在序列中查找的对象。</param>
        /// <param name="comparer">相等比较器，用于测试两个元素是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Contains(object item, IEqualityComparer comparer, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new ContainsOp(item, item.GetType(), comparer) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Count运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于判定元素是否参与计数。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Counts(LambdaExpression predicate, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new CountOp(predicate, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Count运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Counts(Type sourceType, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new CountOp(sourceType) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示DefaultIfEmpty运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="defaultValue">序列为空时要返回的值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp DefaultIfEmpty(Type sourceType, object defaultValue, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new DefaultIfEmptyOp(sourceType, defaultValue) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Distinct运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="comparer">相等比较器，用于测试两个元素是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Distinct(Type sourceType, IEqualityComparer comparer, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new DistinctOp(sourceType, comparer) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示ElementAt运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型</param>
        /// <param name="index">要检索的从零开始的元素索引。</param>
        /// <param name="returnDefault">指示当指定索引处无元素时是否返回默认值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp ElementAt(Type sourceType, int index, bool returnDefault, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new ElementAtOp(sourceType, index, returnDefault) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示无参运算的QueryOp实例。
        /// </summary>
        public static QueryOp Every(Type sourceType, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new EveryOp(sourceType, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示First运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp First(LambdaExpression predicate, bool returnDefault, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new FirstOp(predicate, model, returnDefault) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示First运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp First(Type sourceType, bool returnDefault, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new FirstOp(sourceType, returnDefault) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Group运算的QueryOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵。</param>
        /// <param name="comparer">相等比较器，用于测试两个分组鍵是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp GroupBy(LambdaExpression keySelector, IEqualityComparer comparer, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new GroupOp(keySelector, comparer, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Group运算的QueryOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵。</param>
        /// <param name="elementSelector">组元素函数，用于从每个元素提取组元素。</param>
        /// <param name="comparer">相等比较器，用于测试两个分组鍵是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp GroupBy(LambdaExpression keySelector, LambdaExpression elementSelector,
            IEqualityComparer comparer, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new GroupOp(keySelector, comparer, model, elementSelector) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示分组（聚合）运算的QueryOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵。</param>
        /// <param name="comparer">相等比较器，用于测试两个分组鍵是否相等。</param>
        /// <param name="resultSelector">聚合投影函数，用于对每个组生成聚合值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp GroupBy(LambdaExpression keySelector, IEqualityComparer comparer,
            LambdaExpression resultSelector, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new GroupAggregationOp(resultSelector, keySelector, comparer, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示分组（聚合）运算的QueryOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素提取分组鍵。</param>
        /// <param name="elementSelector">组元素函数，用于从每个元素提取组元素。</param>
        /// <param name="resultSelector">聚合投影函数，用于对每个组生成聚合值。</param>
        /// <param name="comparer">相等比较器，用于测试两个分组鍵是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp GroupBy(LambdaExpression keySelector, LambdaExpression elementSelector,
            LambdaExpression resultSelector, IEqualityComparer comparer, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new GroupAggregationOp(resultSelector, keySelector, comparer, model, elementSelector)
                { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Include运算的QueryOp实例。
        /// </summary>
        /// <param name="selector">包含表达式，用于指示包含路径。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Include(LambdaExpression selector, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new IncludeOp(selector, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Include运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        /// <param name="includingPath">包含路径</param>
        public static QueryOp Include(string includingPath, Type sourceType, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new IncludeOp(includingPath, sourceType, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Join运算的QueryOp实例。
        /// </summary>
        /// <param name="innerSource">要与第一个序列联接的序列。</param>
        /// <param name="outerKeySelector">联接鍵函数，用于从第一个序列的每个元素提取联接鍵。</param>
        /// <param name="innerKeySelector">联接鍵函数，用于从第二个序列的每个元素提取联接鍵。</param>
        /// <param name="resultSelector">结果投影函数，用于从两个匹配元素创建结果元素。</param>
        /// <param name="comparer">相等比较器，用于测试来自两个元素的联接鍵是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Join(IEnumerable innerSource, LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector, LambdaExpression resultSelector, IEqualityComparer comparer,
            ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new JoinOp(innerSource, outerKeySelector, innerKeySelector, resultSelector, model, comparer)
                { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Last运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Last(LambdaExpression predicate, bool returnDefault, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new LastOp(predicate, model, returnDefault) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Last运算的QueryOp实例
        /// </summary>
        /// <param name="sourceType">查询源</param>
        /// <param name="returnDefault">指示未选中任何元素时是否返回默认值</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算</param>
        /// <returns></returns>
        public static QueryOp Last(Type sourceType, bool returnDefault, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new LastOp(sourceType, returnDefault) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示运算符为Max的算术聚合运算的QueryOp实例。
        /// </summary>
        /// <param name="selector">投影函数，应用于每个元素然后以投影结果参与聚合。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Max(LambdaExpression selector, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new ArithAggregateOp(EAggregationOperator.Max, model, selector) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示运算符为Min的算术聚合运算的QueryOp实例。
        /// </summary>
        /// <param name="selector">投影函数，应用于每个元素然后以投影结果参与聚合。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Min(LambdaExpression selector, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new ArithAggregateOp(EAggregationOperator.Min, model, selector) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示OfType运算的QueryOp实例。
        /// </summary>
        /// <param name="resultType">作为筛选依据的类型。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp OfType(Type resultType, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new OfTypeOp(resultType, resultType) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Order运算的QueryOp实例，该Order运算清除之前的排序结果。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素抽取排序鍵。</param>
        /// <param name="descending">指示是否反序排列。</param>
        /// <param name="comparer">比较器，用于比较排序鍵的大小。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp OrderBy(LambdaExpression keySelector, bool descending, IComparer comparer,
            ObjectDataModel model, QueryOp nextOp = null)
        {
            return new OrderOp(keySelector, model, descending, true, comparer) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Reverse运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Reverse(Type sourceType, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new ReverseOp(sourceType) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Select运算的QueryOp实例。
        /// </summary>
        /// <param name="resultSelector">应用于每个元素的投影函数。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Select(LambdaExpression resultSelector, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new SelectOp(resultSelector, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示对结果进行合并的多重投影运算的QueryOp实例。
        /// </summary>
        /// <param name="resultSelector">应用于每个元素的投影函数。</param>
        /// <param name="resultType">对每个元素投影的结果的类型。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Select(LambdaExpression resultSelector, Type resultType, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new CombiningSelectOp(resultSelector, resultType, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示“集合中介投影”运算的QueryOp实例。
        /// </summary>
        /// <param name="resultSelector">结果投影函数，应用于每个中间序列的每个元素。</param>
        /// <param name="collectionSelector">中介投影函数，应用于输入序列的每个元素。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Select(LambdaExpression resultSelector, LambdaExpression collectionSelector,
            ObjectDataModel model, QueryOp nextOp = null)
        {
            return new CollectionSelectOp(resultSelector, collectionSelector, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示一般投影运算的SelectOp实例。
        ///     实施说明
        ///     如果视图具有平展点，生成CollectionSelectOp实例。
        /// </summary>
        /// <param name="resultView">投影结果视图。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">查询链中的下一个节点。</param>
        public static SelectOp Select(TypeView resultView, ObjectDataModel model, QueryOp nextOp = null)
        {
            SelectOp selectOp;
            //如果视图具有平展点
            if (resultView.FlatteningPoints != null && resultView.FlatteningPoints.Length > 0)
                //将平展点生成的表达式传入
                selectOp = new CollectionSelectOp(resultView, model) { _next = nextOp };
            else
                selectOp = new SelectOp(resultView, model) { _next = nextOp };
            return selectOp;
        }


        /// <summary>
        ///     创建表示退化投影运算的SelectOp实例。
        ///     实施说明
        ///     如果退化路径具有平展点，生成CollectionSelectOp实例。
        /// </summary>
        /// <param name="atrophyPath">退化路径。</param>
        /// <param name="combining">多重投影时指示是否对结果实施合并。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">查询链中的下一个节点。</param>
        internal static SelectOp Select(AtrophyPath atrophyPath, bool combining, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            SelectOp op;
            //如果视图具有平展点 或 combining
            if ((atrophyPath.FlatteningPoints != null && atrophyPath.FlatteningPoints.Length > 0) || combining)
                op = new CollectionSelectOp(atrophyPath, model) { _next = nextOp };
            else
                op = new SelectOp(atrophyPath, model) { _next = nextOp };
            return op;
        }

        /// <summary>
        ///     创建表示SequenceEqual运算的QueryOp实例。
        /// </summary>
        /// <param name="other">参与比较的另一序列。</param>
        /// <param name="comparer">相等比较器，用于测试来自两个序列的元素是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp SequenceEqual(IEnumerable other, IEqualityComparer comparer, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new SequenceEqualOp(other, other.GetType().GetGenericArguments()[0], comparer)
                { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Set运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型</param>
        /// <param name="other">参与运算的另一集合。</param>
        /// <param name="operator">集运算符。</param>
        /// <param name="comparer">相等比较器，用于测试来自于两个集合的元素是否相等。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Set(Type sourceType, IEnumerable other, ESetOperator @operator,
            IEqualityComparer comparer, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new SetOp(sourceType, @operator, other, comparer) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Single运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试元素是否满足条件。</param>
        /// <param name="returnDefault">指示不满足条件时是否返回默认值。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Single(LambdaExpression predicate, bool returnDefault, ObjectDataModel model,
            QueryOp nextOp = null)
        {
            return new SingleOp(predicate, model, returnDefault) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Single运算的QueryOp实例
        /// </summary>
        /// <param name="sourceType">查询源类型</param>
        /// <param name="returnDefault">指示不满足条件时是否返回默认值</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算</param>
        /// <returns></returns>
        public static QueryOp Single(Type sourceType, bool returnDefault, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new SingleOp(sourceType, returnDefault) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示Skip运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="count">要略过的个数。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Skip(Type sourceType, int count, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new SkipOp(sourceType, count) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示SkipWhile运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试每个元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp SkipWhile(LambdaExpression predicate, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new SkipWhileOp(predicate, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示运算符为Sum的算术聚合运算的QueryOp实例。
        /// </summary>
        /// <param name="selector">投影函数，应用于每个元素然后以投影结果参与聚合。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Sum(LambdaExpression selector, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new ArithAggregateOp(EAggregationOperator.Sum, model, selector) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Take运算的QueryOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="count">要提取的个数。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Take(Type sourceType, int count, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new TakeOp(sourceType, count) { _next = nextOp, _model = model };
        }

        /// <summary>
        ///     创建表示TakeWhile运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试每个元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp TakeWhile(LambdaExpression predicate, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new TakeWhileOp(predicate, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Order运算的QueryOp实例，该Order运算不清除之前的排序结果。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素抽取排序鍵。</param>
        /// <param name="descending">指示是否反序排列。</param>
        /// <param name="comparer">比较器，用于比较排序鍵的大小。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp ThenOrderBy(LambdaExpression keySelector, bool descending, IComparer comparer,
            ObjectDataModel model, QueryOp nextOp = null)
        {
            return new OrderOp(keySelector, model, descending, false, comparer) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Where运算的QueryOp实例。
        /// </summary>
        /// <param name="predicate">断言函数，用于测试每个元素是否满足条件。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Where(LambdaExpression predicate, ObjectDataModel model, QueryOp nextOp = null)
        {
            return new WhereOp(predicate, model) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Zip运算的QueryOp实例。
        /// </summary>
        /// <param name="second">要合并的第二个序列。</param>
        /// <param name="firstType">第一个序列的元素类型 即源的类型</param>
        /// <param name="resultSelector">合并投影函数，用于指定如何合并这两个序列中的元素。</param>
        /// <param name="nextOp">后续运算。</param>
        public static QueryOp Zip(IEnumerable second, Type firstType, LambdaExpression resultSelector,
            QueryOp nextOp = null)
        {
            return new ZipOp(second, firstType, resultSelector) { _next = nextOp };
        }

        /// <summary>
        ///     创建表示Zip运算的QueryOp实例。
        /// </summary>
        /// <param name="second">要合并的第二个序列</param>
        /// <param name="firstType">第一个序列的元素类型 即源的类型</param>
        /// <param name="resultType">结果</param>
        /// <param name="nextOp">后续运算</param>
        /// <returns></returns>
        public static QueryOp Zip(IEnumerable second, Type firstType, Type resultType, QueryOp nextOp = null)
        {
            return new ZipOp(firstType, resultType, second, firstType) { _next = nextOp };
        }

        #endregion
    }
}