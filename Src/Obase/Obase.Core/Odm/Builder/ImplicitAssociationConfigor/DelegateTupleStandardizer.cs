/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：委托构建的元组标准化器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:33:56
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     基于两个分别充当标准化函数和反函数的委托构建的元组标准化器。
    /// </summary>
    public class DelegateTupleStandardizer<TReferred> : ITupleStandardizer
        where TReferred : class
    {
        /// <summary>
        ///     标准化函数的反函数
        /// </summary>
        private readonly Func<object, TReferred> _revertingFunc;

        /// <summary>
        ///     元组标准化函数
        /// </summary>
        private readonly Func<TReferred, object> _standardingFunc;

        /// <summary>
        ///     初始化DelegateTupleStandardizer类的新实例。
        ///     类型参数
        ///     TReferred
        ///     被引对象元组的类型。
        /// </summary>
        /// <param name="standardingFunc">元组标准化函数。</param>
        /// <param name="revertingFunc">标准化函数的反函数。</param>
        public DelegateTupleStandardizer(Func<TReferred, object> standardingFunc, Func<object, TReferred> revertingFunc)
        {
            _standardingFunc = standardingFunc;
            _revertingFunc = revertingFunc;
        }

        /// <summary>
        ///     元组标准化函数的反函数，将标准元组转换成被引对象元组。
        /// </summary>
        /// <returns>被引对象组成的元组（不限定元组的数据类型，只要逻辑上为元组即可）。</returns>
        /// <param name="tupleItems">标准化元组的项序列。</param>
        public object Revert(object[] tupleItems)
        {
            return _revertingFunc.Invoke(tupleItems);
        }

        /// <summary>
        ///     元组标准化函数，将被引对象元组转换成标准元组。
        /// </summary>
        /// <returns>表示标准化元组的对象数组。</returns>
        /// <param
        ///     name="referredTuple">
        ///     被引对象组成的元组（不限定元组的数据类型，只要逻辑上为元组即可）。被引对象是指关联引用指向的对象，如果关联引用是多重性
        ///     的，它是指其中的一个。
        /// </param>
        public object[] Standardize(object referredTuple)
        {
            return (object[])_standardingFunc((TReferred)referredTuple);
        }
    }
}