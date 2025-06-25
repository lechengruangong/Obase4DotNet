/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：开始删除组事件数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:44:47
└──────────────────────────────────────────────────────────────┘
*/


using Obase.Core.Odm;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     开始删除组事件数据类
    /// </summary>
    public class BeginDeletingGroupEventArgs : DeletingGroupEventArgs
    {
        /// <summary>
        ///     创建BeginDeletingGroupEventArgs实例，并指定要删除的对象及其类型。
        /// </summary>
        /// <param name="objType">对象组中对象的类型。</param>
        /// <param name="objects">对象组中对象的集合。</param>
        public BeginDeletingGroupEventArgs(ObjectType objType, object[] objects) : base(objType, objects)
        {
        }
    }
}