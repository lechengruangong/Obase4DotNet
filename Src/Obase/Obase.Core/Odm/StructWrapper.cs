/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构体包装器,将值类型包装为引用类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:18:37
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     结构体包装器，用于将结构体（值类型）包装成引用类型对象。
    /// </summary>
    public class StructWrapper
    {
        /// <summary>
        ///     真实的结构体
        /// </summary>
        public object Struct;

        /// <summary>
        ///     构造结构体包装器
        /// </summary>
        /// <param name="structObj">要包装的结构体</param>
        public StructWrapper(object structObj)
        {
            Struct = structObj;
        }
    }
}