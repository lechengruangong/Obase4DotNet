/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关系运算管道构建器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:56:31
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Query;
using Obase.Core.Query.Oop;
using Obase.Core.Query.TypeViews;
using Obase.Providers.Sql.Common;
using Obase.Providers.Sql.SqlObject;
using Expression = System.Linq.Expressions.Expression;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     关系运算管道构建器。
    /// </summary>
    /// 实施说明
    ///  
    /// 算法框架参见活动图“生成关系运算管道”。
    /// 定义通用前置访问逻辑：执行“不支持Rop”分支。即所有运算默认不适用关系运算。
    /// 为适用关系运算的运算（如下）定义特定前置访问逻辑：执行“else”分支。
    /// 为适用关系运算的运算定义特定后置访问逻辑。
    ///  
    /// 附：适用关系运算的运算及其访问逻辑和断言准则
    ///  
    /// （说明：蓝色表明将旧版的多个运算合并；红色表明新版有修改）
    ///  
    /// 1.All
    /// 分解为：筛选运算(生成筛选条件(测定条件)) >> Count聚合运算 >> 补充运算，其中，生成筛选条件：criteria = 测定条件.Not()。
    ///  
    /// 2.Any
    /// 分解为：筛选运算(生成筛选条件(测定条件或测试对象)) >> Count聚合运算 >> 补充运算，其中，生成筛选条件：criteria = 测定条件。
    ///  
    /// 3.ArithAggregate
    /// <font color="#0000ff">分解为：投影运算(投影表达式) >> （无参）聚合运算。</font>
    /// 4.Contains
    /// 分解为：筛选运算(生成筛选条件(测定条件或测试对象)) >> Count聚合运算 >> 补充运算，其中，生成筛选条件：
    /// ObjectMapper om = new ObjectMapper();
    /// criteria = om.GenerateCriteria(测试对象, model[typeof(TSource)])。
    ///  
    /// 5.Count
    /// 分解为：筛选运算(条件表达式) >> （无参）聚合运算。
    ///  
    /// 6.Distinct
    ///  
    /// 7.ElementAt
    /// 分解为：提取运算(index + 1) >> 略过运算(index) >> 补充运算。
    ///  
    /// 8.Set
    /// <font color="#0000ff">判定该运算是否适用关系运算请参见设计图“判定集运算是否适用关系运算”。</font>
    /// 9.First
    /// 分解为：Where(条件) >> Take(1) >> 补充运算。
    ///  
    /// 10.Group
    /// 若满足以下任一条件，断言不适用关系运算：
    /// （1）使用了比较器；
    /// （2）为普通分组运算且未使用元素投影器；
    /// （3）为普通分组且元素投影器的Body为New或MemberInit表达式。
    /// 若满足以下任一条件，从运算参数中解析出视图（使用视图查询解析器）：
    /// （1）为普通分组运算；
    /// （2）为分组聚合运算且IsNew==true。
    /// 如果为普通分组运算，生成SelectExecutor并补充一个执行分组操作的对象运算；如果为分组聚合运算，首先生成GroupAggregationExecutor，然后再判断：如果IsNew==true生成SelectExecutor，否则生成AtrophySelectExecutor。
    /// 11.Include
    ///  
    /// 12.Last
    /// 分解为：反序运算 >> 提取运算(1) >> 补充运算。
    ///  
    /// 13.Order
    ///  
    /// 14.Reverse
    ///  
    /// 15.Select
    /// 若满足以下任一条件，断言不适用关系运算：（1）为CollectionSelectionOp且IndexRefferred==true且IsNew==true；（2）IsNew==true，且投影目标视图的某一成员绑定到New或MemberInit表达式。
    /// 若满足以下任一条件，从运算参数中解析出视图：（1）ResultType是IEnumerable，且从投影表达式中抽取的关联树有子节点，（使用MultipleSelectionParser）；（2）IsNew==true,（调用SelectOp.ResultView）
    /// 如果解析了视图，生成SelectExecutor，否则生成AtrophySelectExecutor。如果ResultType是IEnumerable，补充一个退化投影的对象运算。
    /// 16.Single
    /// 分解为：筛选运算(索引条件) >> 补充运算。
    ///  
    /// 17.Skip
    ///  
    /// 18.Take
    ///  
    /// 19.Where
    public class RopPipelineBuilder : QueryOpVisitorWithOutArgs<OpExecutor<RopContext>, QueryOp>
    {
        /// <summary>
        ///     缓存视图Sql表达式的字典
        /// </summary>
        private static readonly Dictionary<Expression, SqlObject.Expression>
            TypeViewExpressionDict =
                new Dictionary<Expression, SqlObject.Expression>();

        /// <summary>
        ///     对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     ROP管道所使用的数据源
        /// </summary>
        private readonly EDataSource _targetSource;

        /// <summary>
        ///     寄存器(寄存补充运算管道，避免重复生成。)
        /// </summary>
        private OopExecutor _complement;

        /// <summary>
        ///     构造RopPipelineBuilder的新实例。
        ///     实施建议：实例化的同时生成一个以RopTerminator作为唯一节点的管道，该管道将作为查询链中最后一个查询运算的执行器的后继。
        /// </summary>
        /// <param name="model">对象数据模型</param>
        /// <param name="targetSource">目标源类型</param>
        public RopPipelineBuilder(ObjectDataModel model, EDataSource targetSource)
        {
            _model = model;
            _targetSource = targetSource;
            SpecificPrev();
            SpecificPost();
        }

        /// <summary>
        ///     获取补充运算管道。
        /// </summary>
        /// 实施说明：
        /// 使用ComplementaryPipelineBuilder
        /// 
        /// 调用QueryOp.GeneratePipeline方法生成对象运算管道。
        /// 首次生成管道后寄存，避免重复生成。
        public OopExecutor Complement
        {
            get
            {
                if (_complement != null) return _complement;
                {
                    var complementaryPipelineBuilder = new ComplementaryPipelineBuilder();
                    _complement = complementaryPipelineBuilder.Pipeline;
                }
                return _complement;
            }
        }

        /// <summary>
        ///     获取构建出来的关系运算管道。
        /// </summary>
        public OpExecutor<RopContext> Pipeline => _result ?? new RopTerminator(_outArgument);

        /// <summary>
        ///     执行通用后置访问逻辑。
        /// </summary>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        protected override bool PostvisitGenerally(QueryOp queryOp, object previousState, object previsitState)
        {
            return true;
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
            outPreviousState = outPrevisitState = null;
            if (_outArgument == null)
                _outArgument = queryOp;
            _result = new RopTerminator(queryOp);
            return false;
        }

        /// <summary>
        ///     添加特定前置访问
        /// </summary>
        private void SpecificPrev()
        {
            Specify(EQueryOpName.All, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Any, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.ArithAggregate, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Contains, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Count, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Distinct, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.ElementAt, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Set, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.First, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Group, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Include, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Last, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Order, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Reverse, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Select, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Single, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Skip, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Take, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Where, SpecificPreVisitDelegate, PredicatePre);
            Specify(EQueryOpName.Non, SpecificPreVisitDelegate, PredicatePre);
        }

        /// <summary>
        ///     添加特定后置访问。
        /// </summary>
        private void SpecificPost()
        {
            Specify(EQueryOpName.All, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Any, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.ArithAggregate, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Contains, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Count, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Distinct, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.ElementAt, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Set, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.First, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Group, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Include, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Last, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Order, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Reverse, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Select, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Single, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Skip, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Take, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Where, SpecificPostvisitDelegate, PredicatePos);
            Specify(EQueryOpName.Non, SpecificPostvisitDelegate, PredicatePos);
        }

        /// <summary>
        ///     前置访问逻辑的委托
        /// </summary>
        /// <param name="queryOp">查询运算</param>
        /// <param name="previousState">前置状态数据</param>
        /// <param name="outPreviousState">前置状态数据输出</param>
        /// <param name="outPrevisitState">前置访问数据</param>
        /// <returns>是否继续访问</returns>
        private bool SpecificPreVisitDelegate(QueryOp queryOp, object previousState, out object outPreviousState,
            out object outPrevisitState)
        {
            outPreviousState = outPrevisitState = null;
            try
            {
                outPrevisitState = AnalyzeParameterExpression(queryOp);
                var next = queryOp.Next;
                Func<QueryOp, QueryOp> complement = null;

                bool needComplement;
                if (queryOp is GroupAggregationOp && outPrevisitState == null)
                {
                    needComplement = true;
                    complement = op1 => queryOp;
                }
                else if (queryOp is WhereOp && outPrevisitState == null)
                {
                    needComplement = true;
                    if (_outArgument != null)
                        complement = op1 => _outArgument;
                    else
                        complement = op1 => queryOp;
                }
                else
                {
                    needComplement = NeedComplement(queryOp, ref complement);
                }

                if (!needComplement)
                {
                    _outArgument = null;
                    if (next != null) return true;

                    _result = new RopTerminator(queryOp);
                    return false;
                }

                _outArgument = complement(next);
                _result = new RopTerminator(queryOp);

                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                _outArgument = queryOp;
                _result = new RopTerminator(queryOp);
                return false;
            }
        }

        /// <summary>
        ///     后置访问逻辑的委托
        /// </summary>
        /// <param name="queryOp">查询运算</param>
        /// <param name="previousState">前置数据</param>
        /// <param name="previsitState">前置访问数据</param>
        private void SpecificPostvisitDelegate(QueryOp queryOp, object previousState, object previsitState)
        {
            _result = CreateRopExecutorInstance(queryOp, previsitState, _result);
        }

        /// <summary>
        ///     启用特定前置访问逻辑
        /// </summary>
        /// <param name="queryOp">查询运算</param>
        /// <returns>特定前置访问逻辑</returns>
        private ESpecialPredicate PredicatePre(QueryOp queryOp)
        {
            return ESpecialPredicate.PostExecute;
        }

        /// <summary>
        ///     启用特定后置访问逻辑
        /// </summary>
        /// <param name="queryOp">查询运算</param>
        /// <returns>特定后置访问逻辑</returns>
        private ESpecialPredicate PredicatePos(QueryOp queryOp)
        {
            return ESpecialPredicate.PreExecute;
        }

        /// <summary>
        ///     是否需要补充运算
        /// </summary>
        /// <param name="queryOp">queryOp</param>
        /// <param name="complementFunc">返回添加补充运算委托</param>
        private bool NeedComplement(QueryOp queryOp, ref Func<QueryOp, QueryOp> complementFunc)
        {
            bool needComplement;
            switch (queryOp.Name)
            {
                case EQueryOpName.All:
                case EQueryOpName.Any:
                case EQueryOpName.Contains:
                case EQueryOpName.Single:
                    needComplement = true;
                    complementFunc = next => new DetectionComplementaryOp(queryOp, _model);
                    break;
                case EQueryOpName.ElementAt:
                    needComplement = true;
                    complementFunc = next => new IndexComplementaryOp(queryOp, _model);
                    break;
                case EQueryOpName.First:
                case EQueryOpName.Last:
                    needComplement = true;
                    complementFunc = next => new FilteringComplementaryOp(queryOp, _model);
                    break;
                case EQueryOpName.Select:
                    var selectOp = (SelectOp)queryOp;
                    if (selectOp.ResultType != typeof(string) &&
                        selectOp.ResultType.GetInterface("IEnumerable") != null)
                    {
                        needComplement = true;
                        complementFunc = next =>
                        {
                            var parser = new MultipleSelectionParser();
                            var typeView = parser.Parse(queryOp, _model);
                            var typeViewType = typeView.ClrType;
                            //构造 key/element属性名称，和视图生成是属性生成规则一致
                            var keyAttr = typeView.ViewReferences[0].Name;
                            //resultSelector表达式的参数
                            var sp = Expression.Parameter(typeViewType, "s");
                            //typeView _key属性MemberAccess 表达式
                            var keyAccess =
                                Expression.Lambda(
                                    Expression.MakeMemberAccess(
                                        sp,
                                        typeViewType.GetProperty(keyAttr,
                                            BindingFlags.NonPublic | BindingFlags.Instance) ??
                                        throw new ArgumentException($"构造投影操作失败,没有{keyAttr}属性.")),
                                    sp);

                            return QueryOp.Select(keyAccess, _model, next);
                        };
                    }
                    else
                    {
                        needComplement = false;
                    }

                    break;

                case EQueryOpName.Group:
                {
                    var groupOp = (GroupOp)queryOp;
                    if (!(groupOp is GroupAggregationOp))
                    {
                        needComplement = true;
                        complementFunc = next =>
                        {
                            if (groupOp.ElementSelector != null)
                            {
                                var parser = new GroupingParser();
                                var typeView = parser.Parse(groupOp, _model);
                                var typeViewType = typeView.ClrType;

                                string keyAttr; //视图绑定到KeySelector的属性 的字段名称
                                string refOrEleAttr; //视图绑定到ElementSeletor的属性或引用 的字段名称

                                #region keyAttr和refOrEleAttr赋值

                                var attrs = typeView.Attributes;
                                var refs = typeView.ViewReferences;

                                if (refs?.Length > 0) //表示typeView有一个属性和一个关联引用
                                {
                                    keyAttr = attrs[0].Name;
                                    refOrEleAttr = refs[0].Name;
                                }
                                else //表示typeView两个都是属性
                                {
                                    keyAttr = attrs
                                        .First(p => ((ViewAttribute)p).Binding.ToString() ==
                                                    groupOp.KeySelector.Body.ToString()).Name;
                                    refOrEleAttr = attrs
                                        .First(p => ((ViewAttribute)p).Binding.ToString() ==
                                                    groupOp.ElementSelector.Body.ToString()).Name;
                                }

                                #endregion

                                //keySelector和elementSelector 表达式的参数
                                var sp = Expression.Parameter(typeViewType, "s");

                                //typeView _key属性MemberAccess 表达式
                                var newKeySelector = Expression.Lambda(
                                    Expression.MakeMemberAccess(sp,
                                        typeViewType.GetProperty(keyAttr,
                                            BindingFlags.NonPublic | BindingFlags.Instance) ??
                                        throw new ArgumentException($"构造分组操作失败,没有{keyAttr}属性.")),
                                    sp);
                                //typeView _element属性MemberAccess 表达式
                                var newElementSelector = Expression.Lambda(
                                    Expression.MakeMemberAccess(sp,
                                        typeViewType.GetProperty(refOrEleAttr,
                                            BindingFlags.NonPublic | BindingFlags.Instance) ??
                                        throw new ArgumentException($"构造分组操作失败,没有{refOrEleAttr}属性.")),
                                    sp);
                                return QueryOp.GroupBy(newKeySelector, newElementSelector, groupOp.Comparer, _model,
                                    next);
                            }

                            return groupOp;
                        }; //补充对象运算。
                    }
                    else
                    {
                        needComplement = false;
                    }
                }
                    break;
                default:
                    needComplement = false;
                    break;
            }

            return needComplement;
        }


        /// <summary>
        ///     解析参数中的表达式
        /// </summary>
        /// <param name="op">查询运算</param>
        /// <returns>解析结果</returns>
        private object AnalyzeParameterExpression(QueryOp op)
        {
            switch (op.Name)
            {
                case EQueryOpName.All:
                    return Analyze((AllOp)op);
                case EQueryOpName.Any:
                    return Analyze((AnyOp)op);
                case EQueryOpName.ArithAggregate:
                    return Analyze((ArithAggregateOp)op);
                case EQueryOpName.Contains:
                    return Analyze((ContainsOp)op);
                case EQueryOpName.Count:
                    return Analyze((CountOp)op);
                case EQueryOpName.Distinct:
                    return Analyze((DistinctOp)op);
                case EQueryOpName.ElementAt:
                    return Analyze((ElementAtOp)op);
                case EQueryOpName.Set:
                    return Analyze((SetOp)op);
                case EQueryOpName.First:
                    return Analyze((FirstOp)op);
                case EQueryOpName.Group:
                    return Analyze((GroupOp)op);
                case EQueryOpName.Include:
                    return Analyze((IncludeOp)op);
                case EQueryOpName.Last:
                    return Analyze((LastOp)op);
                case EQueryOpName.Order:
                    return Analyze((OrderOp)op);
                case EQueryOpName.Reverse:
                    return Analyze((ReverseOp)op);
                case EQueryOpName.Select:
                    return Analyze((SelectOp)op);
                case EQueryOpName.Single:
                    return Analyze((SingleOp)op);
                case EQueryOpName.Skip:
                    return Analyze((SkipOp)op);
                case EQueryOpName.Take:
                    return Analyze((TakeOp)op);
                case EQueryOpName.Where:
                    return Analyze((WhereOp)op);
                case EQueryOpName.Non:
                    return Analyze((EveryOp)op);
                default:
                    throw new ArgumentException("指定查询运算无法解析参数中的表达式。");
            }
        }

        /// <summary>
        ///     创建关系运算执行器实例
        /// </summary>
        /// <param name="op">查询运算</param>
        /// <param name="previsitState">前置数据</param>
        /// <param name="next">运算器的下一节</param>
        /// <returns></returns>
        private OpExecutor<RopContext> CreateRopExecutorInstance(QueryOp op, object previsitState,
            OpExecutor<RopContext> next)
        {
            switch (op.Name)
            {
                case EQueryOpName.All:
                    return Create((AllOp)op, previsitState, next);
                case EQueryOpName.Any:
                    return Create((AnyOp)op, previsitState, next);
                case EQueryOpName.ArithAggregate:
                    return Create((ArithAggregateOp)op, previsitState, next);
                case EQueryOpName.Contains:
                    return Create((ContainsOp)op, previsitState, next);
                case EQueryOpName.Count:
                    return Create((CountOp)op, previsitState, next);
                case EQueryOpName.Distinct:
                    return Create((DistinctOp)op, previsitState, next);
                case EQueryOpName.ElementAt:
                    return Create((ElementAtOp)op, previsitState, next);
                case EQueryOpName.Set:
                    return Create((SetOp)op, previsitState, next);
                case EQueryOpName.First:
                    return Create((FirstOp)op, previsitState, next);
                case EQueryOpName.Group:
                    return Create((GroupOp)op, previsitState, next);
                case EQueryOpName.Include:
                    return Create((IncludeOp)op, previsitState, next);
                case EQueryOpName.Last:
                    return Create((LastOp)op, previsitState, next);
                case EQueryOpName.Order:
                    return Create((OrderOp)op, previsitState, next);
                case EQueryOpName.Reverse:
                    return Create((ReverseOp)op, previsitState, next);
                case EQueryOpName.Select:
                    return Create((SelectOp)op, previsitState, next);
                case EQueryOpName.Single:
                    return Create((SingleOp)op, previsitState, next);
                case EQueryOpName.Skip:
                    return Create((SkipOp)op, previsitState, next);
                case EQueryOpName.Take:
                    return Create((TakeOp)op, previsitState, next);
                case EQueryOpName.Where:
                    return Create((WhereOp)op, previsitState, next);
                case EQueryOpName.Non:
                    return Create((EveryOp)op, previsitState, next);
                default:
                    return next;
            }
        }

        #region 创建RopExecutor/解析参数中的表达式

        /// <summary>
        ///     解析NonQueryOp参数中的表达式
        /// </summary>
        private object Analyze(EveryOp op)
        {
            return null;
        }

        /// <summary>
        ///     创建NonQueryOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(EveryOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            return next;
        }

        /// <summary>
        ///     解析WhereOp参数中的表达式
        /// </summary>
        /// <param name="op">查询运算</param>
        /// <returns></returns>
        private object Analyze(WhereOp op)
        {
            var criteria = TranslateToICriteria(op.Predicate);
            return criteria;
        }

        /// <summary>
        ///     创建WhereOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(WhereOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            var executor = next;
            if (previsitState is ICriteria criteria)
                executor = new WhereExecutor(op, criteria, executor);
            return executor;
        }


        /// <summary>
        ///     解析TakeOp参数中的表达式
        /// </summary>
        private object Analyze(TakeOp op)
        {
            return null;
        }

        /// <summary>
        ///     创建TakeOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(TakeOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            OpExecutor<RopContext> executor = new TakeExecutor(op, op.Count, next);
            return executor;
        }


        /// <summary>
        ///     解析SkipOp参数中的表达式
        /// </summary>
        private object Analyze(SkipOp op)
        {
            return null;
        }

        /// <summary>
        ///     创建SkipOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(SkipOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            OpExecutor<RopContext> executor = new SkipExecutor(op, op.Count, next);
            return executor;
        }


        /// <summary>
        ///     解析SingleOp参数中的表达式
        /// </summary>
        private object Analyze(SingleOp op)
        {
            var criteria = TranslateToICriteria(op.Predicate);
            return criteria;
        }

        /// <summary>
        ///     创建SingleOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(SingleOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            var executor = next;
            if (previsitState is ICriteria criteria)
                executor = new WhereExecutor(op, criteria, executor);
            return executor;
        }


        /// <summary>
        ///     解析SelectOp参数中的表达式
        /// </summary>
        private object Analyze(SelectOp op)
        {
            //若满足以下任一条件，断言不适用关系运算：（1）为CollectionSelectionOp且IndexRefferred == true且IsNew == true；（2）IsNew == true，且投影目标视图的某一成员绑定到New或MemberInit表达式。
            //若满足以下任一条件，从运算参数中解析出视图：（1）ResultType是IEnumerable，且从投影表达式中抽取的关联树有子节点，（使用MultipleSelectionParser）；（2）IsNew == true,（调用SelectOp.ResultView）
            //如果解析了视图，生成SelectExecutor，否则生成AtrophySelectExecutor。如果ResultType是IEnumerable，补充一个退化投影的对象运算。


            TypeView tv = null;
            Dictionary<string, SqlObject.Expression> dic = null;
            ISelectionSet set = null;
            AssociationTreeNode associationTree = null;
            AttributeTreeNode attributeTree = null;
            LambdaExpression collectionSelector = null;

            if (op.IsNew)
                tv = op.ResultView;

            if (op.ResultType != typeof(string) && op.ResultType.GetInterface("IEnumerable") != null)
            {
                var assoTree = op.ResultSelector.Body.ExtractAssociation(_model);
                if (assoTree?.SubCount > 0)
                {
                    var parser = new MultipleSelectionParser();
                    tv = parser.Parse(op, _model);
                }
            }

            var bindings = new List<ParameterBinding>();
            if (op is CollectionSelectOp collectionSelectOp)
            {
                collectionSelector = collectionSelectOp.CollectionSelector;

                if (collectionSelector.Parameters.Count == 2)
                    bindings.Add(new ParameterBinding(collectionSelector.Parameters[1], EParameterReferring.Index));
                if (collectionSelectOp.ResultSelector.Parameters.Count() >= 2)
                {
                    var parameter = collectionSelectOp.ResultSelector.Parameters[1];
                    bindings.Add(new ParameterBinding(parameter, collectionSelector));
                }
            }

            if (tv != null)
            {
                dic = TranslateTypeView(tv);
            }
            else
            {
                var keyExp = op.ResultSelector;
                var tree = new SubTreeEvaluator(keyExp);
                var parser = new SelectionExpressionParser(_model, tree, false, bindings.ToArray());
                set = parser.Parse(keyExp, out associationTree, out attributeTree);
            }


            return new object[] { tv, dic, set, associationTree, attributeTree, collectionSelector };
        }

        /// <summary>
        ///     创建SelectOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(SelectOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            // 如果解析了视图，生成SelectExecutor，否则生成AtrophySelectExecutor。如果ResultType是IEnumerable，补充一个退化投影的对象运算。


            var executor = next;
            if (!(previsitState is object[] unboxed)) return executor;

            var typeView = unboxed[0] as TypeView;
            var dic = unboxed[1] as Dictionary<string, SqlObject.Expression>;
            var set = unboxed[2] as ISelectionSet;
            var associationTree = unboxed[3] as AssociationTreeNode;
            var attributeTree = unboxed[4] as AttributeTreeNode;
            var collectionSelector = unboxed[5] as LambdaExpression;
            if (typeView != null)
                executor = new SelectExecutor(op, typeView, dic, executor);
            else
                executor = new AtrophySelectExecutor(op, op.ResultSelector, collectionSelector, set, associationTree,
                    attributeTree, null, executor);
            return executor;
        }


        /// <summary>
        ///     解析ReverseOp参数中的表达式
        /// </summary>
        private object Analyze(ReverseOp op)
        {
            return null;
        }

        /// <summary>
        ///     创建ReverseOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(ReverseOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            OpExecutor<RopContext> executor = new ReverseExecutor(op, next);
            return executor;
        }


        /// <summary>
        ///     解析OrderOp参数中的表达式
        /// </summary>
        private object Analyze(OrderOp op)
        {
            var exp = Translate(op.KeySelector);
            return exp;
        }

        /// <summary>
        ///     创建OrderOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(OrderOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            var exp = previsitState as SqlObject.Expression;
            OpExecutor<RopContext> executor =
                new OrderExecutor(op, op.KeySelector, exp, op.Descending, op.ClearPrevious, next);
            return executor;
        }


        /// <summary>
        ///     解析LastOp参数中的表达式
        /// </summary>
        private object Analyze(LastOp op)
        {
            var criteria = TranslateToICriteria(op.Predicate);
            return criteria;
        }

        /// <summary>
        ///     创建LastOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(LastOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            //分解为：Where(条件) >>反序运算 >> 提取运算(1) >> 补充运算。
            var executor = next;
            executor = new TakeExecutor(op, 1, executor);
            executor = new ReverseExecutor(op, executor);
            if (previsitState is ICriteria criteria) executor = new WhereExecutor(op, criteria, executor);
            return executor;
        }


        /// <summary>
        ///     解析IncludeOp参数中的表达式
        /// </summary>
        private object Analyze(IncludeOp op)
        {
            return null;
        }

        /// <summary>
        ///     创建IncludeOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(IncludeOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            OpExecutor<RopContext> executor = new IncludeExecutor(op, op.Selectors[0], null, op.SourceType, next);

            return executor;
        }


        /// <summary>
        ///     解析GroupOp参数中的表达式
        /// </summary>
        private object Analyze(GroupOp op)
        {
            // 若满足以下任一条件，从运算参数中解析出视图（使用视图查询解析器）：
            // （1）为普通分组运算；
            // （2）为分组聚合运算且IsNew==true。
            TypeView typeView = null;
            Dictionary<string, SqlObject.Expression> dic = null;
            ISelectionSet set = null;
            AssociationTreeNode associationTree = null;
            AttributeTreeNode attributeTree = null;
            if (op is GroupAggregationOp groupAggregationOp && groupAggregationOp.IsNew)
            {
                try
                {
                    var parser = new GroupingAggregationParser();
                    typeView = parser.Parse(op, _model);
                    dic = TranslateTypeView(typeView);
                }
                catch
                {
                    //发生异常 一般为无法翻译的Sql函数
                    return null;
                }
            }
            else if (!(op is GroupAggregationOp))
            {
                var viewQueryParserFactory = new ViewQueryParserFactory();
                var parser = viewQueryParserFactory.Create(op);
                if (parser != null)
                {
                    typeView = parser.Parse(op, _model);
                    dic = TranslateTypeView(typeView);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var keyExp = op.KeySelector;
                var tree = new SubTreeEvaluator(keyExp);
                var parser = new SelectionExpressionParser(_model, tree, false);
                set = parser.Parse(keyExp, out associationTree, out attributeTree);
            }

            return new object[] { typeView, dic, set, associationTree, attributeTree };
        }

        /// <summary>
        ///     创建GroupOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(GroupOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            // 若满足以下任一条件，从运算参数中解析出视图：
            // （1）为普通分组运算；
            // （2）为分组聚合运算且IsNew==true。
            // 如果为普通分组运算，生成SelectExecutor并补充一个执行分组操作的对象运算；
            // 如果为分组聚合运算，首先生成GroupAggregationExecutor，然后再判断：如果IsNew==true生成SelectExecutor，否则生成AtrophyExecutor。
            var executor = next;
            //前置运算后为空 表示不适用关系运算
            if (previsitState == null) return next;

            var unboxed = (object[])previsitState;
            var typeView = unboxed[0] as TypeView;
            var dic = unboxed[1] as Dictionary<string, SqlObject.Expression>;
            var set = unboxed[2] as ISelectionSet;
            var associationTree = unboxed[3] as AssociationTreeNode;
            var attributeTree = unboxed[4] as AttributeTreeNode;

            if (op is GroupAggregationOp groupAggregationOp)
            {
                var keyExp = groupAggregationOp.KeySelector;
                if (groupAggregationOp.IsNew)
                    executor = new SelectExecutor(op, typeView, dic, executor);
                else
                    executor = new AtrophySelectExecutor(op, keyExp, groupAggregationOp.ElementSelector, set,
                        associationTree, attributeTree, null, executor);

                var tree = new SubTreeEvaluator(keyExp);
                var tr = new ExpressionTranslator(_model, tree);
                var grourBy = tr.Translate(keyExp);
                executor = new GroupAggregationExecutor(op, keyExp, grourBy, executor);
            }
            else
            {
                executor = new SelectExecutor(op, typeView, dic, executor);
            }

            return executor;
        }


        /// <summary>
        ///     解析FirstOp参数中的表达式
        /// </summary>
        private object Analyze(FirstOp op)
        {
            var criteria = TranslateToICriteria(op.Predicate);
            return criteria;
        }

        /// <summary>
        ///     创建FirstOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(FirstOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            //分解为：Where(条件) >> 提取运算(1) >> 补充运算。
            OpExecutor<RopContext> executor = new TakeExecutor(op, 1, next);
            if (previsitState is ICriteria criteria) executor = new WhereExecutor(op, criteria, executor);
            return executor;
        }


        /// <summary>
        ///     解析SetOp参数中的表达式
        /// </summary>
        private object Analyze(SetOp op)
        {
            if (!(op.Other is IQueryable queryable)) throw new ArgumentException("要进行Set操作的另一集合不是IQueryable的");
            var exp = queryable.Expression;
            var parser = new QueryExpressionParser(_model);
            exp.Accept(parser);
            var newOp = parser.QueryOp;
            var builder = new RopPipelineBuilder(_model, _targetSource);
            newOp.Accept(builder);
            var context = new RopContext(newOp.SourceType, _model, _targetSource);
            builder.Pipeline.Execute(context);
            return context.ResultSql;
        }

        /// <summary>
        ///     创建SetOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(SetOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            if (previsitState is ISetOperand operand)
            {
                OpExecutor<RopContext> executor = new SetOpExecutor(op, operand, op.Operator, next);
                return executor;
            }

            return next;
        }


        /// <summary>
        ///     解析ElementAtOp参数中的表达式
        /// </summary>
        private object Analyze(ElementAtOp op)
        {
            return null;
        }

        /// <summary>
        ///     创建ElementAtOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(ElementAtOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            //分解为：提取运算(index + 1) >> 略过运算(index) >> 补充运算。
            OpExecutor<RopContext> executor = new SkipExecutor(op, op.Index, next);
            executor = new TakeExecutor(op, op.Index + 1, executor);
            return executor;
        }


        /// <summary>
        ///     解析DistinctOp参数中的表达式
        /// </summary>
        private object Analyze(DistinctOp op)
        {
            return null;
        }

        /// <summary>
        ///     创建DistinctOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(DistinctOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            OpExecutor<RopContext> executor = new DistinctExecutor(op, next);
            return executor;
        }


        /// <summary>
        ///     解析CountOp参数中的表达式
        /// </summary>
        private object Analyze(CountOp op)
        {
            var criteria = TranslateToICriteria(op.Predicate);
            return criteria;
        }

        /// <summary>
        ///     创建CountOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(CountOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            // 分解为：筛选运算(条件表达式) >> （无参）聚合运算。
            OpExecutor<RopContext> executor =
                new AggregateExecutor(op, EAggregationFunction.Count, op.SourceType, next);
            if (previsitState is ICriteria criteria)
                executor = new WhereExecutor(op, criteria, executor);
            return executor;
        }


        /// <summary>
        ///     解析ContainsOp参数中的表达式
        /// </summary>
        private object Analyze(ContainsOp op)
        {
            var criteria = SqlUtils.GenerateCriteria(op.Item, _model.GetObjectType(op.SourceType));
            return criteria;
        }

        /// <summary>
        ///     创建ContainsOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(ContainsOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            // 分解为：筛选运算(生成筛选条件(测定条件或测试对象)) >> Count聚合运算 >> 补充运算，
            // 其中，生成筛选条件：
            // ObjectMapper om = new ObjectMapper();
            // criteria = om.GenerateCriteria(测试对象, model[typeof(TSource)])。
            OpExecutor<RopContext> executor =
                new AggregateExecutor(op, EAggregationFunction.Count, op.SourceType, next);
            if (previsitState is ICriteria criteria)
                executor = new WhereExecutor(op, criteria, executor);
            return executor;
        }


        /// <summary>
        ///     解析ArithAggregateOp参数中的表达式
        /// </summary>
        private object Analyze(ArithAggregateOp op)
        {
            var keyExp = op.Selector;
            var tree = new SubTreeEvaluator(keyExp);
            var parser = new SelectionExpressionParser(_model, tree, false);
            var selectionSet = parser.Parse(keyExp, out var assoResult, out var attrResult);

            foreach (var setItem in selectionSet.Columns)
                if (setItem is ExpressionColumn expression)
                    expression.Alias = "";

            object[] res = { op.Selector, null, selectionSet, assoResult, attrResult, null };
            return res;
        }

        /// <summary>
        ///     创建ArithAggregateOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(ArithAggregateOp op, object previsitState,
            OpExecutor<RopContext> next = null)
        {
            var unboxed = (object[])previsitState;

            //分解为：投影运算(投影表达式) >> （无参）聚合运算。
            OpExecutor<RopContext> executor;
            switch (op.Operator)
            {
                case EAggregationOperator.Average:
                    executor = new AggregateExecutor(op, EAggregationFunction.Average, op.ResultType, next);
                    break;
                case EAggregationOperator.Max:
                    executor = new AggregateExecutor(op, EAggregationFunction.Max, op.ResultType, next);
                    break;
                case EAggregationOperator.Min:
                    executor = new AggregateExecutor(op, EAggregationFunction.Min, op.ResultType, next);
                    break;
                case EAggregationOperator.Sum:
                    executor = new AggregateExecutor(op, EAggregationFunction.Sum, op.ResultType, next);
                    break;
                default:
                    executor = new AggregateExecutor(op, EAggregationFunction.None, op.ResultType, next);
                    break;
            }

            var expression = unboxed[0] as LambdaExpression;
            var collectionSelector = unboxed[1] as LambdaExpression;
            var selectionSet = unboxed[2] as ISelectionSet;
            var assoResult = unboxed[3] as AssociationTreeNode;
            var attrResult = unboxed[4] as AttributeTreeNode;
            var resultAlias = unboxed[5] as string;

            executor = new AtrophySelectExecutor(op, expression, collectionSelector, selectionSet, assoResult,
                attrResult,
                resultAlias, executor);
            return executor;
        }


        /// <summary>
        ///     解析AnyOp参数中的表达式
        /// </summary>
        private object Analyze(AnyOp op)
        {
            var criteria = TranslateToICriteria(op.Predicate);
            return criteria;
        }

        /// <summary>
        ///     创建AnyOp执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(AnyOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            //分解为：筛选运算(生成筛选条件(测定条件或测试对象)) >> Count聚合运算 >> 补充运算，其中，生成筛选条件：criteria = 测定条件。
            OpExecutor<RopContext> executor;
            executor = new AggregateExecutor(op, EAggregationFunction.Count, op.SourceType, next);
            if (previsitState is ICriteria criteria)
                executor = new WhereExecutor(op, criteria, executor);
            return executor;
        }


        /// <summary>
        ///     解析AllOp参数中的表达式
        /// </summary>
        private object Analyze(AllOp op)
        {
            var criteria = TranslateToICriteria(op.Predicate);
            return criteria;
        }

        /// <summary>
        ///     创建All执行器
        /// </summary>
        /// <returns></returns>
        private OpExecutor<RopContext> Create(AllOp op, object previsitState, OpExecutor<RopContext> next = null)
        {
            //分解为：筛选运算(生成筛选条件(测定条件)) >> Count聚合运算 >> 补充运算，其中，生成筛选条件：criteria = 测定条件.Not()。
            OpExecutor<RopContext> executor =
                new AggregateExecutor(op, EAggregationFunction.Count, op.SourceType, next);
            if (previsitState is ICriteria criteria)
                executor = new WhereExecutor(op, criteria.Not(), executor);
            return executor;
        }

        /// <summary>
        ///     翻译类型视图中的表达式
        /// </summary>
        /// <param name="typeView">类型视图</param>
        /// <returns>返回解析结果的字典。键为属性名称，值为Sql表达式，该字典将用于构造投影运算执行器。</returns>
        private Dictionary<string, SqlObject.Expression> TranslateTypeView(TypeView typeView)
        {
            var result = new Dictionary<string, SqlObject.Expression>();
            if (typeView == null) return result;
            foreach (var attr in typeView.Attributes)
            {
                if (attr.IsComplex) continue;
                //获取绑定表达式
                var attrExp = (attr as ViewAttribute)?.Binding;
                SqlObject.Expression sqlExp;
                if (attrExp != null && !TypeViewExpressionDict.ContainsKey(attrExp)) //缓存中不存在
                {
                    var subTree = new SubTreeEvaluator(attrExp);
                    //翻译表达式
                    var translator = new ExpressionTranslator(_model, subTree, typeView.ParameterBindings);
                    sqlExp = translator.Translate(attrExp);
                    //添加到缓存
                    TypeViewExpressionDict[attrExp] = sqlExp;
                }

                if (attrExp != null)
                {
                    sqlExp = TypeViewExpressionDict[attrExp];
                    result[attr.Name] = sqlExp ?? throw new ExpressionIllegalException(attrExp);
                }
            }

            //处理构造函数参数 构造函数参数内也有可能为需要投影的字段
            var parameters = typeView.Constructor.Parameters;
            if (parameters != null && parameters.Count > 0)
                foreach (var parameter in parameters)
                {
                    //获取绑定表达式
                    var parameterExp = parameter.Expression;
                    if (parameterExp == null) continue;
                    SqlObject.Expression sqlExp;
                    if (!TypeViewExpressionDict.ContainsKey(parameterExp)) //缓存中不存在
                    {
                        var subTree = new SubTreeEvaluator(parameterExp);
                        //翻译表达式
                        var translator = new ExpressionTranslator(_model, subTree, typeView.ParameterBindings);
                        sqlExp = translator.Translate(parameterExp);
                        //添加到缓存
                        TypeViewExpressionDict[parameterExp] = sqlExp;
                    }

                    sqlExp = TypeViewExpressionDict[parameterExp];
                    result[parameter.Name] = sqlExp ?? throw new ExpressionIllegalException(parameterExp);
                }

            return result;
        }

        /// <summary>
        ///     表达式翻译成ICriteria。
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <exception cref="ExpressionIllegalException">解析发生异常。</exception>
        /// <returns>
        ///     1.如果predicate==nul，返回null。
        ///     2.如果解析成功返回解析结果值，否则返回null。
        /// </returns>
        private ICriteria TranslateToICriteria(Expression predicate)
        {
            try
            {
                if (predicate == null) return null;
                var tree = new SubTreeEvaluator(predicate);
                var bindings = new List<ParameterBinding>();
                if (predicate is LambdaExpression lambdaExpression && lambdaExpression.Parameters.Count == 2)
                    bindings.Add(new ParameterBinding(lambdaExpression.Parameters[1], EParameterReferring.Index));

                var parser = new CriteriaExpressionParser(_model, tree, _targetSource, bindings.ToArray());
                return parser.Parse(predicate);
            }
            catch (ExpressionIllegalException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     翻译表达式
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <returns>翻译后的表达式</returns>
        private SqlObject.Expression Translate(Expression exp)
        {
            var tree = new SubTreeEvaluator(exp);
            var tr = new ExpressionTranslator(_model, tree);
            return tr.Translate(exp);
        }

        #endregion
    }
}