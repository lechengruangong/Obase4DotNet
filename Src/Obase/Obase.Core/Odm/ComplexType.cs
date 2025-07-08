/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：复杂类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 09:58:19
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示复杂类型。
    /// </summary>
    public class ComplexType : StructuralType
    {
        /// <summary>
        ///     根据指定的CLR类型创建类型实例。
        /// </summary>
        /// <param name="clrType">CLR类型</param>
        /// <param name="derivingFrom">基类</param>
        public ComplexType(Type clrType, StructuralType derivingFrom = null)
            : base(clrType, derivingFrom)
        {
            _typeName.IsAssociation = false;
            _typeName.IsEntity = false;
        }

        /// <summary>
        ///     完整性检查
        ///     对于复杂类型 目前不进行完整性检查
        /// </summary>
        public override void IntegrityCheck()
        {
            //复杂类型 没有完整性检查
        }

        /// <summary>
        ///     字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"ComplexType:{{Name-\"{Name}\",ClrType-\"{ClrType}\"}}";
        }
    }
}