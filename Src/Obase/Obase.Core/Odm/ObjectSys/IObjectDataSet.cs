/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据集规范,该数据集中的数据项用于创建对象系统.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:33:27
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     对象数据集规范，该数据集中的数据项用于创建对象系统，该对象系统符合特定关联树定义的结构。
    /// </summary>
    public interface IObjectDataSet
    {
        /// <summary>
        ///     获取挂靠在指定关联树节点上的对象数据集合。
        /// </summary>
        /// <param name="assoNode">关联树节点。</param>
        IEnumerable<ObjectDataSetItem> Get(AssociationTreeNode assoNode);
    }
}