/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型扩展,存储针对结构化类型的扩展.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:45:40
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     类型扩展。
    /// </summary>
    public abstract class TypeExtension
    {
        /// <summary>
        ///     被扩展的类型。
        /// </summary>
        private StructuralType _extendedType;

        /// <summary>
        ///     获取被扩展的类型。
        /// </summary>
        public StructuralType ExtendedType => _extendedType;

        /// <summary>
        ///     设置被扩展的类型。
        /// </summary>
        /// <param name="extendedType">被扩展的类型。</param>
        internal void SetExtendedType(StructuralType extendedType)
        {
            _extendedType = extendedType;
        }
    }
}