/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：元素扩展,提供类型元素的扩展配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:17:58
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     元素扩展。
    /// </summary>
    public abstract class ElementExtension
    {
        /// <summary>
        ///     被扩展元素。
        /// </summary>
        private TypeElement _extendedElement;

        /// <summary>
        ///     获取被扩展的元素。
        /// </summary>
        public TypeElement ExtendedElement => _extendedElement;

        /// <summary>
        ///     设置被扩展的元素。
        /// </summary>
        /// <param name="extendedElement">被扩展的元素。</param>
        internal void SetExtendedElement(TypeElement extendedElement)
        {
            _extendedElement = extendedElement;
        }
    }
}