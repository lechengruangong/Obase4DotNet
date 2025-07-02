/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Zip运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:59:19
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Query;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     表示Zip运算。
    /// </summary>
    public class ZipOp : QueryOp
    {
        /// <summary>
        ///     第一个序列的元素类型
        /// </summary>
        private readonly Type _firstType;

        /// <summary>
        ///     合并投影函数，用于指定如何合并这两个序列中的元素。不指定则返回两个序列中的元素一一对应构成的元组序列。
        /// </summary>
        private readonly LambdaExpression _resultSelector;

        /// <summary>
        ///     返回值类型
        /// </summary>
        private readonly Type _resultType;

        /// <summary>
        ///     要合并的第二个序列。
        /// </summary>
        private readonly object _second;

        /// <summary>
        ///     第二个序列的元素类型
        /// </summary>
        private readonly Type _secondType;

        /// <summary>
        ///     创建ZipOp实例。
        /// </summary>
        /// <param name="second">要合并的第二个序列。</param>
        /// <param name="sourceType">源类型</param>
        /// <param name="resultSelector">合并投影函数，用于指定如何合并这两个序列中的元素。</param>
        internal ZipOp(IEnumerable second, Type sourceType, LambdaExpression resultSelector = null)
            : base(EQueryOpName.Zip, sourceType)
        {
            _second = second;
            _resultSelector = resultSelector;
        }

        /// <summary>
        ///     创建ZipOp实例。
        /// </summary>
        /// <param name="firstType">第一个序列的元素类型</param>
        /// <param name="resultType">返回值类型</param>
        /// <param name="second">要合并的第二个序列</param>
        /// <param name="sourceType">源类型</param>
        internal ZipOp(Type firstType, Type resultType, IEnumerable second, Type sourceType)
            : base(EQueryOpName.Zip, sourceType)
        {
            _firstType = firstType;
            _resultType = resultType;
            _second = second;
            _secondType = second.GetType().GetGenericArguments().FirstOrDefault() ?? second.GetType().GetElementType();
        }

        /// <summary>
        ///     获取第一个序列元素的类型。
        /// </summary>
        public Type FirstType => _resultSelector?.Parameters[0].Type ?? _firstType;

        /// <summary>
        ///     获取合并投影函数，该函数用于指定如何合并这两个序列中的元素。不指定则返回两个序列中的元素一一对应构成的元组序列。
        /// </summary>
        public LambdaExpression ResultSelector => _resultSelector;

        /// <summary>
        ///     获取合并结果序列元素的类型。
        /// </summary>
        public override Type ResultType => _resultSelector?.ReturnType ?? _resultType;

        /// <summary>
        ///     获取要合并的第二个序列。
        /// </summary>
        public object Second => _second;

        /// <summary>
        ///     获取第二个序列元素的类型。
        /// </summary>
        public Type SecondType => _resultSelector?.Parameters[1].Type ?? _secondType;
    }
}