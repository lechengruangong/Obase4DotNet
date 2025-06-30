/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：存储标记判定器规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:35:26
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     存储标记判定器规范，提供判定指定对象的存储标记的方法
    /// </summary>
    public interface IStorageSymbolJudge
    {
        /// <summary>
        ///     判定指定对象的存储标记。
        /// </summary>
        /// <returns>存储标记。</returns>
        /// <param name="obj">要判定其存储标记的对象。</param>
        /// <param name="objType">对象的类型。</param>
        StorageSymbol Judge(object obj, ObjectType objType);

        /// <summary>
        ///     判定指定类型的对象的存储标记。
        /// </summary>
        /// <returns>
        ///     存储标记集。
        ///     说明
        ///     在特定情形（如分区存储）下，同一类型的对象可能分散存储于多个存储服务，因而有多个存储标记。
        /// </returns>
        /// <param name="objType">对象类型。</param>
        StorageSymbol[] Judge(ObjectType objType);
    }
}