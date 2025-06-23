/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基元类型,指示一个类型是否属于Obase的基元类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:38:03
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示基元类型。
    /// </summary>
    public class PrimitiveType : TypeBase
    {
        /// <summary>
        ///     构造一个基元类型
        /// </summary>
        /// <param name="clrType">CLR类型</param>
        private PrimitiveType(Type clrType) : base(clrType)
        {
            _typeName.IsAssociation = false;
            _typeName.IsEntity = false;
        }

        /// <summary>
        ///     根据指定的CLR类型返回PrimitiveType类的实例。
        /// </summary>
        /// <param name="clrType">CLR类型。</param>
        public static PrimitiveType FromType(Type clrType)
        {
            return new PrimitiveType(clrType);
        }

        /// <summary>
        ///     判断一个类型是否为Obase的基元类型
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public static bool IsObasePrimitiveType(Type clrType)
        {
            //检测是否为可空类型 如果是 则从中拆出来
            clrType = Nullable.GetUnderlyingType(clrType) != null
                ? clrType.GenericTypeArguments[0]
                : clrType;

            //Obase基元类型为系统基元类型 + string(字符串)类型 + decimal(精确十进制数) + datetime(日期时间) + timespan(时间) + Guid(6.1新增) + 枚举
            return clrType.IsPrimitive || clrType == typeof(string) || clrType == typeof(DateTime) ||
                   clrType == typeof(TimeSpan) || clrType == typeof(Guid)
                   || clrType == typeof(decimal) || clrType.IsEnum;
        }
    }
}