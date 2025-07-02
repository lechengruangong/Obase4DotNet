/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：数据行分派器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:17:03
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.ObjectSys;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     数据行分派器。
    /// </summary>
    public class DataRowAssigner : IAssociationTreeDownwardVisitor
    {
        /// <summary>
        ///     DataRowAssignment存储
        /// </summary>
        private readonly DataRowAssignmentSet _dataRowAssignmentSet;

        /// <summary>
        ///     被分派的数据行。
        /// </summary>
        private DataRow _dataRow;

        /// <summary>
        ///     创建DataRowAssigner实例。
        /// </summary>
        /// <param name="dataRowAssignmentSet">用于存储DataRowAssignment实例的容器。</param>
        public DataRowAssigner(DataRowAssignmentSet dataRowAssignmentSet)
        {
            _dataRowAssignmentSet = dataRowAssignmentSet;
        }

        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(AssociationTree subTree, object parentState, out object outParentState,
            out object outPrevisitState)
        {
            //获取键
            var objectKey = _dataRow?.GetObjectKey(subTree.Node);

            if (objectKey != null)
                //不存在等效行 则加入
                if (!_dataRowAssignmentSet.ContainEquivalent(subTree.Node, _dataRow))
                    _dataRowAssignmentSet.Add(subTree.Node, _dataRow);

            outParentState = null;
            outPrevisitState = null;

            return true;
        }

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的关联树子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AssociationTree subTree, object parentState, object previsitState)
        {
            //Nothing to do
        }

        /// <summary>
        ///     重置访问者
        /// </summary>
        public void Reset()
        {
            //Nothing to do
        }

        /// <summary>
        ///     设置待分派的数据行。
        /// </summary>
        /// <param name="dataRow">待分派数据行。</param>
        public void SetDataRow(DataRow dataRow)
        {
            _dataRow = dataRow;
        }
    }
}