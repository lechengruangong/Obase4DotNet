/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据集中的数据项.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:36:29
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     表示对象数据集中的数据项。
    /// </summary>
    public struct ObjectDataSetItem
    {
        /// <summary>
        ///     对象数据。
        /// </summary>
        public IObjectData ObjectData;

        /// <summary>
        ///     数据项创建的对象的父标识。
        /// </summary>
        public ObjectKey ParentKey;
    }
}