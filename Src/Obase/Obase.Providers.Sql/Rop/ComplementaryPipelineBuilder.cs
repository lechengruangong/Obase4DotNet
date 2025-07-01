/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：补充运算的对象运算管道构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:04:36
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Query;
using Obase.Core.Query.Oop;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     支持补充运算的对象运算管道构造器。
    /// </summary>
    /// 实施说明:
    /// 在构造函数中为所有补充运算添加特定的后置访问逻辑，针对每一补充运算创建对应的执行器。
    public class ComplementaryPipelineBuilder : OopPipelineBuilder
    {
        /// <summary>
        ///     支持补充运算的对象运算管道构造器
        /// </summary>
        public ComplementaryPipelineBuilder()
        {
            // All
            Specify(EQueryOpName.All, PostvisitFunc, predicate => ESpecialPredicate.PostExecute);
            // Any
            Specify(EQueryOpName.Any, PostvisitFunc, predicate => ESpecialPredicate.PostExecute);
            //Contains
            Specify(EQueryOpName.Contains, PostvisitFunc, predicate => ESpecialPredicate.PostExecute);
            //Single
            Specify(EQueryOpName.Single, PostvisitFunc, predicate => ESpecialPredicate.PostExecute);
            //First
            Specify(EQueryOpName.First, PostvisitFunc, predicate => ESpecialPredicate.PostExecute);
            //Last
            Specify(EQueryOpName.Last, PostvisitFunc, predicate => ESpecialPredicate.PostExecute);
            //ElementAt
            Specify(EQueryOpName.ElementAt, PostvisitFunc, predicate => ESpecialPredicate.PostExecute);
        }

        /// <summary>
        ///     为指定运算添加特定的后置访问逻辑。
        /// </summary>
        private void PostvisitFunc(QueryOp queryOp, object previousState, object previsitState)
        {
            switch (queryOp.Name)
            {
                /*测定类运算（AllOp, AnyOp, ContainsOp, SingleOp）的补充运算*/
                // All测定运算。
                case EQueryOpName.All:
                // Any测定运算。
                case EQueryOpName.Any:
                //ContainsOp
                case EQueryOpName.Contains:
                //SingleOp
                case EQueryOpName.Single:
                    _result = new DetectionComplementaryOpExecutor((DetectionComplementaryOp)queryOp, _result);
                    break;
                /*选择类运算（FirstOp, LastOp）的补充运算*/
                //First
                case EQueryOpName.First:
                //Last
                case EQueryOpName.Last:
                    _result = new FilteringComplementaryOpExecutor((FilteringComplementaryOp)queryOp, _result);
                    break;
                /*索引运算（ElementAtOp）的补充运算*/
                case EQueryOpName.ElementAt:
                    _result = new IndexComplementaryOpExecutor((IndexComplementaryOp)queryOp, _result);
                    break;
            }
        }
    }
}