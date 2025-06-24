/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联端映射,存储关联端的实体主键和映射字段.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:31:23
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     关联端映射
    /// </summary>
    public class AssociationEndMapping : IOrderBy
    {
        /// <summary>
        ///     关联端的标识属性
        /// </summary>
        private string _keyAttribute;

        /// <summary>
        ///     关联表的映射字段
        /// </summary>
        private string _targetField;

        /// <summary>
        ///     获取或设置关联端的标识属性。
        /// </summary>
        public string KeyAttribute
        {
            get => _keyAttribute;
            set => _keyAttribute = value;
        }


        /// <summary>
        ///     获取或设置关联表的映射字段。
        /// </summary>
        public string TargetField
        {
            get => _targetField;
            set => _targetField = value;
        }


        /// <summary>
        ///     返回对象的哈希代码。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var key1 = 0;
            if (_keyAttribute != null)
                key1 = _keyAttribute.GetHashCode();
            return key1;
        }

        /// <summary>
        ///     重写比较方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var endMapping = obj as AssociationEndMapping;
            return endMapping != null && Equals(endMapping);
        }

        /// <summary>
        ///     比较二者是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Equals(AssociationEndMapping obj)
        {
            if (obj == null) return false;
            return _keyAttribute == obj._keyAttribute;
        }

        /// <summary>
        ///     算符重载 判断二者是否相等
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public static bool operator ==(AssociationEndMapping key1, AssociationEndMapping key2)
        {
            if (Equals(key1, null) && Equals(key2, null)) return true;
            return !Equals(key1, null) && key1.Equals(key2);
        }

        /// <summary>
        ///     算符重载 判断二者是否不相等
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public static bool operator !=(AssociationEndMapping key1, AssociationEndMapping key2)
        {
            return !(key1 == key2);
        }
    }
}