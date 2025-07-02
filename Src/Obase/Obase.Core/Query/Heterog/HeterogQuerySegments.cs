/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构查询按一定规则进行分解得到的片段.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:10:41
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     一个数据结构，用于表示对异构查询按一定规则进行分解得到的片段。
    /// </summary>
    public struct HeterogQuerySegments
    {
        /// <summary>
        ///     补充链。
        /// </summary>
        public QueryOp Complement;

        /// <summary>
        ///     查询链中的包含运算（显式或隐式）生成的包含树（以主体链末节点的源类型为基点）。
        ///     给实施者的说明
        ///     IHeterogQuerySegmentallyExecutor的实施者应当明白，Including是相对于主体链末节点源类型的，执行主体链时应该对其实施裁剪，以得到相对于结果类型的包含树
        /// </summary>
        public AssociationTree Including;

        /// <summary>
        ///     主体链。对于同构查询，主体链是其自身剔除包含运算后形成的查询链。
        /// </summary>
        public QueryOp MainQuery;

        /// <summary>
        ///     主体链末尾的异构运算。
        /// </summary>
        public QueryOp MainTail;
    }
}