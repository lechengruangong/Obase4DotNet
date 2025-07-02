/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序依据的成员表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:46:02
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     描述作为排序依据的成员表达式。
    /// </summary>
    public struct OrderExpression
    {
        /// <summary>
        ///     作为排序依据的成员表达式。
        ///     如果该表达式指向一个关联端，则表示该关联端的所有标识属性都作为排序依据；如果指向某个关联端的某个标识属性，则表示该标识属性将作为排序依据；如果指向关联类的某个属
        ///     性，则该属性将作为排序依据。
        /// </summary>
        public MemberExpression Expression;

        /// <summary>
        ///     指示是否倒序（即降序）排列。
        /// </summary>
        public bool Inverted;
    }
}