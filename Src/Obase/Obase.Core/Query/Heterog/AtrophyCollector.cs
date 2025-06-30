/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：退化路径收集器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:58:21
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     针对查询链中的退化投影运算，记录其退化路径，按顺序形成一个退化序列。
    ///     实施说明
    ///     访问结果为退化路径构成的序列。
    ///     检查查询链节点，如果为投影运算，再检查其AssociationResult(selectOp.AtrophyPath?.AssociationPath)是否为空，如果不为空则追加到序列末尾。
    /// </summary>
    public class AtrophyCollector : QueryOpVisitor<AssociationTreeNode[]>
    {
        /// <summary>
        ///     暂存退化路径序列
        /// </summary>
        private readonly List<AssociationTreeNode> _atrophyPaths = new List<AssociationTreeNode>();

        /// <summary>
        ///     获取访问操作的结果。
        /// </summary>
        public override AssociationTreeNode[] Result => _atrophyPaths.ToArray();

        /// <summary>
        ///     执行通用后置访问逻辑。
        /// </summary>
        /// <param name="queryOp">要访问的查询运算。</param>
        /// <param name="previousState">访问前一运算时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        protected override bool PostvisitGenerally(QueryOp queryOp, object previousState, object previsitState)
        {
            //收集退化路径
            if (queryOp is SelectOp selectOp)
                if (selectOp.AtrophyPath?.AssociationPath != null)
                    _atrophyPaths.Add(selectOp.AtrophyPath?.AssociationPath);

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
            //Nothing to do
            outPreviousState = null;
            outPrevisitState = null;
            return false;
        }
    }
}