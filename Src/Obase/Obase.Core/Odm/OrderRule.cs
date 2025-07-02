/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序规则.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 17:39:56
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     描述排序规则。
    /// </summary>
    public class OrderRule
    {
        /// <summary>
        ///     是否倒序排列。注：默认为正序（即升序）排序。
        /// </summary>
        private bool _inverted;

        /// <summary>
        ///     排序依据。
        /// </summary>
        private IOrderBy _orderBy;

        /// <summary>
        ///     获取或设置排序依据。
        /// </summary>
        public IOrderBy OrderBy
        {
            get => _orderBy;
            set => _orderBy = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示是否倒序（即降序）排列。默认为升序排列。
        /// </summary>
        public bool Inverted
        {
            get => _inverted;
            set => _inverted = value;
        }
    }
}