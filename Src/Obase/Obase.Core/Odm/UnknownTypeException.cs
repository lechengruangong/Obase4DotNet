/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：未知类型异常,无法识别数据类型时抛出.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:56:45
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示未知类型异常。
    /// </summary>
    public class UnknownTypeException : Exception
    {
        /// <summary>
        ///     未识别的数据类型。
        /// </summary>
        private readonly Type _type;


        /// <summary>
        ///     创建UnknownTypeException实例。
        /// </summary>
        /// <param name="type">未识别的数据类型。</param>
        public UnknownTypeException(Type type)
        {
            _type = type;
        }

        /// <summary>
        ///     获取异常信息。
        ///     格式：“无法识别数据类型” + 类型名称。
        /// </summary>
        public override string Message => $"无法识别数据类型{_type.FullName}";

        /// <summary>
        ///     获取未识别的数据类型。
        /// </summary>
        public Type Type => _type;
    }
}