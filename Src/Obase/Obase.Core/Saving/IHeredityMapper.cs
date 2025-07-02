/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义遗传映射机制.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:18:01
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     定义遗传映射机制。
    /// </summary>
    public interface IHeredityMapper
    {
        /// <summary>
        ///     根据字段在母源中名称推断其在衍生源中的名称。
        /// </summary>
        /// <param name="fieldName">字段在母源中的名称。</param>
        string Map(string fieldName);
    }
}