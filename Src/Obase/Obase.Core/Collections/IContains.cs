/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：包含检测接口,提供是否包含方法.                                                    
│　作   者：Obase开发团队                                              
│　版权所有：武汉乐程软工科技有限公司                                                 
│　创建时间：2025-6-20 11:37:02                            
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Collections
{
    /// <summary>
    ///     包含检测接口
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public interface IContains<in T>
    {
        /// <summary>
        ///     是否包含元素
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns></returns>
        bool Contains(T item);
    }
}