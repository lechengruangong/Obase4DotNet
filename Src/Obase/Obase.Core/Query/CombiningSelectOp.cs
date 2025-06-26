/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示对投影结果实施合并的多重投影运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:39:18
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示对投影结果实施合并的多重投影运算。
    /// </summary>
    public class CombiningSelectOp : SelectOp
    {
        /// <summary>
        ///     最终结果类型，即合并后的序列元素的类型。
        /// </summary>
        private readonly Type _resultType;

        /// <summary>
        ///     根据指定的投影函数创建CombiningSelectOp实例。
        /// </summary>
        /// <param name="resultSelector">应用于每个元素的投影函数。</param>
        /// <param name="resultType">最终结果类型，即合并后的序列元素的类型。</param>
        /// <param name="model"></param>
        internal CombiningSelectOp(LambdaExpression resultSelector, Type resultType, ObjectDataModel model)
            : base(resultSelector, model)
        {
            _resultType = resultType;
        }

        /// <summary>
        ///     根据指定的退化路径创建CombiningSelectOp实例。
        /// </summary>
        /// <param name="atrophyPath">退化路径。</param>
        /// <param name="model"></param>
        /// 实施说明:
        /// 退化路径不能有平展点。
        internal CombiningSelectOp(AtrophyPath atrophyPath, ObjectDataModel model)
            : base(atrophyPath.FlatteningPoints.Length == 0 ? atrophyPath : throw new ArgumentException("退化路径不能有平展点"),
                model)
        {
        }

        /// <summary>
        ///     始终返回true，表示是多重投影。
        /// </summary>
        public override bool IsMultiple => true;

        /// <summary>
        ///     始终返回false，表示不是实例化投影。。
        /// </summary>
        public override bool IsNew => false;

        /// <summary>
        ///     获取最终结果类型，即合并后的序列元素的类型。
        /// </summary>
        public override Type ResultType => _resultType;
    }
}