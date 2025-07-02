/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象运算管道构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:21:15
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     对象运算管道构造器。
    /// </summary>
    /// 实施说明:
    /// 在构造函数中为所有运算添加特定的后置访问逻辑。针对每一运算，调用OopExecutor类的相应静态方法创建执行器。
    public class OopPipelineBuilder : QueryOpVisitor<OopExecutor>
    {
        /// <summary>
        ///     初始化OopPipelineBuilder类的新实例。
        /// </summary>
        public OopPipelineBuilder()
        {
            // 累加运算。
            // Accumulate
            Specify(EQueryOpName.Accumulate, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 算术聚合运算。
            // ArithAggregate
            Specify(EQueryOpName.ArithAggregate, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // All测定运算。
            // All
            Specify(EQueryOpName.All, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // Any测定运算。
            // Any
            Specify(EQueryOpName.Any, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 类型转换运算。
            // Cast
            Specify(EQueryOpName.Cast, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // Contains测定运算。
            // Contains
            Specify(EQueryOpName.Contains, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 计数运算。
            // Count
            Specify(EQueryOpName.Count, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 取默认值运算。
            // DefaultIfEmpty
            Specify(EQueryOpName.DefaultIfEmpty, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 去重运算。
            // Distinct
            Specify(EQueryOpName.Distinct, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 索引运算。
            // ElementAt
            Specify(EQueryOpName.ElementAt, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // First索引运算。
            // First
            Specify(EQueryOpName.First, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 分组运算。
            // Group
            Specify(EQueryOpName.Group, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 包含运算。
            // Include
            Specify(EQueryOpName.Include, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 联接运算。
            // Join
            Specify(EQueryOpName.Join, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // Last索引运算。
            // Last
            Specify(EQueryOpName.Last, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 类型筛选运算。
            // OfType
            Specify(EQueryOpName.OfType, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 排序运算。
            // Order
            Specify(EQueryOpName.Order, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 反序运算。
            // Reverse
            Specify(EQueryOpName.Reverse, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 投影运算。
            // Select
            Specify(EQueryOpName.Select, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 顺序相等比较运算。
            // SequenceEqual
            Specify(EQueryOpName.SequenceEqual, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 集运算。
            // Set
            Specify(EQueryOpName.Set, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 单值索引运算。
            // Single
            Specify(EQueryOpName.Single, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 略过运算。
            // Skip
            Specify(EQueryOpName.Skip, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 条件略过运算。
            // SkipWhile
            Specify(EQueryOpName.SkipWhile, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 提取运算。
            // Take
            Specify(EQueryOpName.Take, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 条件提取运算。
            // TakeWhile
            Specify(EQueryOpName.TakeWhile, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 筛选运算。
            // Where
            //Where,
            Specify(EQueryOpName.Where, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
            // 合并运算。
            // Zip
            Specify(EQueryOpName.Zip, PostvisitFunc, predicate => ESpecialPredicate.PreExecute);
        }

        /// <summary>
        ///     获取生成的对象运算管道。
        /// </summary>
        public OopExecutor Pipeline => _result;

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
            return true;
        }


        /// <summary>
        ///     为指定运算添加特定的后置访问逻辑。
        /// </summary>
        private void PostvisitFunc(QueryOp queryOp, object previousState, object previsitState)
        {
            switch (queryOp.Name)
            {
                // 累加运算。
                case EQueryOpName.Accumulate:
                    _result = OopExecutor.Create((AccumulateOp)queryOp, _result);
                    break;
                // 算术聚合运算。
                case EQueryOpName.ArithAggregate:
                    _result = OopExecutor.Create((ArithAggregateOp)queryOp, _result);
                    break;
                // All测定运算。
                case EQueryOpName.All:
                    _result = OopExecutor.Create((AllOp)queryOp, _result);
                    break;
                // Any测定运算。
                case EQueryOpName.Any:
                    _result = OopExecutor.Create((AnyOp)queryOp, _result);
                    break;
                // 类型转换运算。
                case EQueryOpName.Cast:
                    _result = OopExecutor.Create((CastOp)queryOp, _result);
                    break;
                // Contains
                // 测定运算。
                case EQueryOpName.Contains:
                    _result = OopExecutor.Create((ContainsOp)queryOp, _result);
                    break;
                // 计数运算。
                case EQueryOpName.Count:
                    _result = OopExecutor.Create((CountOp)queryOp, _result);
                    break;
                // 取默认值运算。
                case EQueryOpName.DefaultIfEmpty:
                    _result = OopExecutor.Create((DefaultIfEmptyOp)queryOp, _result);
                    break;
                // 去重运算。
                case EQueryOpName.Distinct:
                    _result = OopExecutor.Create((DistinctOp)queryOp, _result);
                    break;
                // 索引运算。
                case EQueryOpName.ElementAt:
                    _result = OopExecutor.Create((ElementAtOp)queryOp, _result);
                    break;
                // First索引运算。
                case EQueryOpName.First:
                    _result = OopExecutor.Create((FirstOp)queryOp, _result);
                    break;
                // 分组运算。
                case EQueryOpName.Group:
                    if (queryOp is GroupAggregationOp groupAggregation)
                    {
                        _result = OopExecutor.Create(groupAggregation, _result);
                        break;
                    }

                    if (queryOp is GroupOp op)
                        _result = OopExecutor.Create(op, _result);
                    break;
                // 包含运算。
                case EQueryOpName.Include:
                    _result = OopExecutor.Create((IncludeOp)queryOp, _result);
                    break;
                // 联接运算。
                case EQueryOpName.Join:
                    _result = OopExecutor.Create((JoinOp)queryOp, _result);
                    break;
                // Last索引运算。
                case EQueryOpName.Last:
                    _result = OopExecutor.Create((LastOp)queryOp, _result);
                    break;
                // 类型筛选运算。
                case EQueryOpName.OfType:
                    _result = OopExecutor.Create((OfTypeOp)queryOp, _result);
                    break;
                // 排序运算。
                case EQueryOpName.Order:
                    _result = OopExecutor.Create((OrderOp)queryOp, _result);
                    break;
                // 反序运算。
                case EQueryOpName.Reverse:
                    _result = OopExecutor.Create((ReverseOp)queryOp, _result);
                    break;
                // 投影运算。
                case EQueryOpName.Select:
                    _result = OopExecutor.Create((SelectOp)queryOp, _result);
                    break;
                // 顺序相等比较运算。
                case EQueryOpName.SequenceEqual:
                    _result = OopExecutor.Create((SequenceEqualOp)queryOp, _result);
                    break;
                // 集运算。
                case EQueryOpName.Set:
                    _result = OopExecutor.Create((SetOp)queryOp, _result);
                    break;
                // 单值索引运算。
                case EQueryOpName.Single:
                    _result = OopExecutor.Create((SingleOp)queryOp, _result);
                    break;
                // 略过运算。
                case EQueryOpName.Skip:
                    _result = OopExecutor.Create((SkipOp)queryOp, _result);
                    break;
                // 条件略过运算。
                case EQueryOpName.SkipWhile:
                    _result = OopExecutor.Create((SkipWhileOp)queryOp, _result);
                    break;
                // 提取运算。
                case EQueryOpName.Take:
                    _result = OopExecutor.Create((TakeOp)queryOp, _result);
                    break;
                // 条件提取运算。
                case EQueryOpName.TakeWhile:
                    _result = OopExecutor.Create((TakeWhileOp)queryOp, _result);
                    break;
                // 筛选运算。
                case EQueryOpName.Where:
                    _result = OopExecutor.Create((WhereOp)queryOp, _result);
                    break;
                // 合并运算。
                case EQueryOpName.Zip:
                    _result = OopExecutor.Create((ZipOp)queryOp, _result);
                    break;
            }
        }
    }
}