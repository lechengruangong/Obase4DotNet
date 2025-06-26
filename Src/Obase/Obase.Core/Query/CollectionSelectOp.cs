/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示集合中介投影运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:37:17
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示集合中介投影运算。
    ///     集合中介投影是指，首先投影到一个多重性元素，然后对每个投影结果中的每个元素再次投影，得到最终结果。
    /// </summary>
    public class CollectionSelectOp : SelectOp
    {
        /// <summary>
        ///     中介投影函数。
        /// </summary>
        private readonly LambdaExpression _collectionSelector;

        /// <summary>
        ///     创建CollectionSelectOp实例。
        /// </summary>
        /// <param name="resultSelector">结果投影函数。</param>
        /// <param name="collectionSelector">中介投影函数。</param>
        /// <param name="model"></param>
        internal CollectionSelectOp(LambdaExpression resultSelector, LambdaExpression collectionSelector,
            ObjectDataModel model)
            : base(resultSelector, model)
        {
            _collectionSelector = collectionSelector;
        }

        /// <summary>
        ///     创建表示退化投影运算的CollectionSelectOp实例。
        /// </summary>
        /// <param name="atrophyPath">退化路径。</param>
        /// 实施说明:
        /// 退化路径必须有平展点。
        internal CollectionSelectOp(AtrophyPath atrophyPath, ObjectDataModel model)
            : base(atrophyPath, model)
        {
        }

        /// <summary>
        ///     创建表示一般投影运算的CollectionSelectOp实例。
        /// </summary>
        /// <param name="resultView">投影结果视图。</param>
        /// 实施说明:
        /// 结果视图必须有平展点
        internal CollectionSelectOp(TypeView resultView, ObjectDataModel model)
            : base(resultView, model)
        {
        }

        /// <summary>
        ///     获取中介投影函数。
        /// </summary>
        public LambdaExpression CollectionSelector => _collectionSelector;

        /// <summary>
        ///     获取中介投影结果类型。
        /// </summary>
        public Type ElementType => _collectionSelector?.ReturnType;

        /// <summary>
        ///     获取一个值，该值指示投影运算是否将元素在序列中的索引作为（第二个）参数。
        /// </summary>
        /// 实施说明
        /// 对于普通投影运算，依据结果投影函数判定；
        /// 对于集合中介投影，依据中介投影函数判定。
        public override bool IndexReferred
        {
            get
            {
                var indexReferred = false;
                var parameters = _collectionSelector?.Parameters;
                //获取索引 并且索引是Int
                if (parameters != null && parameters.Count == 2 && parameters[1].Type == typeof(int))
                    indexReferred = true;
                return indexReferred;
            }
        }
    }
}