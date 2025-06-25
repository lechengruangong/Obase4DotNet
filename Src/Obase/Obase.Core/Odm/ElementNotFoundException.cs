/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：元素不存在时引发的异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:53:27
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     元素（简单属性、复杂属性、引用元素）不存在时引发的异常。
    /// </summary>
    public class ElementNotFoundException : Exception
    {
        /// <summary>
        ///     元素名称
        /// </summary>
        private readonly string _elementName;

        /// <summary>
        ///     创建ElementNotFoundException实例。
        /// </summary>
        /// <param name="elementName">元素名称。</param>
        public ElementNotFoundException(string elementName)
        {
            _elementName = elementName;
        }

        /// <summary>
        ///     元素名称
        /// </summary>
        public string ElementName => _elementName;

        /// <summary>
        ///     异常信息
        /// </summary>
        public override string Message => $"无法找到元素{_elementName}";
    }
}