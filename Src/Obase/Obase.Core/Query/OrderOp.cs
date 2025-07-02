/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Order运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 12:13:09
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Order运算。
    /// </summary>
    public class OrderOp : QueryOp
    {
        /// <summary>
        ///     指示是否清除之前的排序结果。
        /// </summary>
        private readonly bool _clearPrevious = true;

        /// <summary>
        ///     比较器，用于比较排序鍵的大小。
        /// </summary>
        private readonly IComparer _comparer;

        /// <summary>
        ///     指示是否反序排列。
        /// </summary>
        private readonly bool _descending;

        /// <summary>
        ///     鍵函数，用于从每个元素抽取排序鍵。
        /// </summary>
        private readonly LambdaExpression _keySelector;

        /// <summary>
        ///     创建OrderOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素抽取排序鍵。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="comparer">比较器，用于比较排序鍵的大小。</param>
        internal OrderOp(LambdaExpression keySelector, ObjectDataModel model, IComparer comparer = null)
            : base(EQueryOpName.Order, keySelector.Parameters[0].Type)
        {
            _keySelector = keySelector;
            _comparer = comparer;
            _model = model;
        }

        /// <summary>
        ///     创建OrderOp实例。
        /// </summary>
        /// <param name="keySelector">鍵函数，用于从每个元素抽取排序鍵。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="descending">指示是否反序排列。</param>
        /// <param name="clearPrevious">指示是否清除之前的排序结果。</param>
        /// <param name="comparer">比较器，用于比较排序鍵的大小。</param>
        internal OrderOp(LambdaExpression keySelector, ObjectDataModel model, bool descending = false,
            bool clearPrevious = true,
            IComparer comparer = null)
            : this(keySelector, model, comparer)
        {
            _descending = descending;
            _clearPrevious = clearPrevious;
        }

        /// <summary>
        ///     获取一个值，该值指示是否清除之前的排序结果。
        /// </summary>
        public bool ClearPrevious => _clearPrevious;

        /// <summary>
        ///     获取比较器，该比较器用于比较排序鍵的大小。
        /// </summary>
        public IComparer Comparer => _comparer;

        /// <summary>
        ///     获取一个值，该值指示是否反序排列。
        /// </summary>
        public bool Descending => _descending;

        /// <summary>
        ///     获取鍵函数，该函数用于从每个元素抽取排序鍵。
        /// </summary>
        public LambdaExpression KeySelector => _keySelector;

        /// <summary>
        ///     获取排序键类型。
        /// </summary>
        public Type KeyType => KeySelector?.ReturnType;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}