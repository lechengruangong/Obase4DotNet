/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标识的数组.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:08:39
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core
{
    /// <summary>
    ///     可以作为标识的数组。
    ///     标识是一个值序列，该序列可以在同类事物中唯一标识一个事物。该序列中的项称为标识成员。
    /// </summary>
    public class IdentityArray : List<object>
    {
        /// <summary>
        ///     哈希生成器
        /// </summary>
        private readonly IArrayHashGenerator _hashGenerator;

        /// <summary>
        ///     用指定的标识成员创建IdentityArray实例。
        /// </summary>
        /// <param name="identity">标识成员序列。</param>
        public IdentityArray(params object[] identity) : base(identity)
        {
            _hashGenerator = new DefaultArrayHashGenerator();
        }

        /// <summary>
        ///     用指定的标识成员创建IdentityArray实例，并指定用于为标识生成哈希代码的方法。
        /// </summary>
        /// <param name="identity">标识成员序列。</param>
        /// <param name="hashGenerator">用于为标识数组生成哈希代码的方法。</param>
        public IdentityArray(IArrayHashGenerator hashGenerator, params object[] identity) : base(identity)
        {
            _hashGenerator = hashGenerator;
        }

        /// <summary>
        ///     向标识数组追加子标识。
        ///     在主标识不能唯一标识事物的情况下可以使用子标识进一步标识。
        /// </summary>
        /// <param name="subIdentity">子标识的成员序列。</param>
        public void Append(params object[] subIdentity)
        {
            AddRange(subIdentity);
        }

        /// <summary>
        ///     比价两个标识数组是否相等
        /// </summary>
        /// <param name="obj">另一个对象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as IdentityArray);
        }

        /// <summary>
        ///     比较二者是否相等
        /// </summary>
        /// <param name="obj">另一个IdentityArray</param>
        /// <returns></returns>
        private bool Equals(IdentityArray obj)
        {
            //类型不一样
            if (obj == null)
                return false;

            //比较序列
            var selfArray = ToArray();
            var otherArray = obj.ToArray();

            if (!selfArray.SequenceEqual(otherArray))
                return false;

            //比较生成器
            //因为生成器没有属性访问器 故比较生成的哈希码
            return obj.GetHashCode() == GetHashCode();
        }

        /// <summary>
        ///     获取hash码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _hashGenerator.Generator(ToArray());
        }

        /// <summary>
        ///     重写相等比较运算符。
        /// </summary>
        /// <param name="identity1">第一个标识。</param>
        /// <param name="identity2">第二个标识。</param>
        public static bool operator ==(IdentityArray identity1, IdentityArray identity2)
        {
            if (Equals(identity1, null) && Equals(identity2, null)) return true;
            return !Equals(identity1, null) && identity1.Equals(identity2);
        }

        /// <summary>
        ///     重写不相等比较运算符。
        /// </summary>
        /// <param name="identity1">第一个标识。</param>
        /// <param name="identity2">第二个标识。</param>
        public static bool operator !=(IdentityArray identity1, IdentityArray identity2)
        {
            return !(identity1 == identity2);
        }
    }
}