/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象标识的成员.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:29:03
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     对象标识的成员。
    /// </summary>
    [Serializable]
    public class ObjectKeyMember : IEquatable<ObjectKeyMember>
    {
        /// <summary>
        ///     属性名
        /// </summary>
        private readonly string _attribute;

        /// <summary>
        ///     属性值
        /// </summary>
        private readonly object _value;

        /// <summary>
        ///     创建对象标识成员实例。
        /// </summary>
        /// <param name="attribute">属性名</param>
        /// <param name="value">属性值</param>
        public ObjectKeyMember(string attribute, object value)
        {
            _attribute = attribute;
            _value = value ?? throw new ArgumentNullException(nameof(value), $"对象标识成员{attribute}的属性值不能为空");
        }

        /// <summary>
        ///     获取属性名。
        /// </summary>
        public string Attribute => _attribute;

        /// <summary>
        ///     获取属性值。
        /// </summary>
        public object Value => _value;

        /// <summary>
        ///     判定两个标识成员是否相等，相等返回true，否则返回false。
        ///     算法：当且仅当属性名和属性值同时相等时，标识成员相等。
        /// </summary>
        /// <param name="other"></param>
        public bool Equals(ObjectKeyMember other)
        {
            return other != null && Attribute == other.Attribute && Value == other.Value;
        }

        /// <summary>
        ///     判定两个标识成员是否相等，（重写Object.Equals方法）。
        ///     相等返回true，否则返回false。
        ///     本方法是对Equals(ObjectKeyMember)的调用。
        /// </summary>
        /// <param name="other"></param>
        public override bool Equals(object other)
        {
            if (other is ObjectKeyMember key)
                return Equals(key);
            return false;
        }

        /// <summary>
        ///     获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return $"{_attribute}_{_value}".GetHashCode();
        }

        /// <summary>
        ///     /// 返回对象标识成员的字符串表示形式。重写Object.ToString()。
        ///     算法：标识成员字符串 = Attribute + ":" + Value。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Attribute + ":" + Value;
        }


        /// <summary>
        ///     根据按既定规则编码的字符串生成ObjectKeyMember实例。
        ///     编码规则与ToString方法一致。
        /// </summary>
        /// <returns></returns>
        public static ObjectKeyMember FromString(string memberString)
        {
            //切分字符串
            var splits = memberString.Split(':');
            return splits.Length == 2 ? new ObjectKeyMember(splits[0], splits[1]) : null;
        }
    }
}