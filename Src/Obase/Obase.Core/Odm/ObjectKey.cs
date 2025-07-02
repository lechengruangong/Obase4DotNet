/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象标识.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:30:40
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     对象标识。
    /// </summary>
    [Serializable]
    public class ObjectKey : IEquatable<ObjectKey>, ISerializable
    {
        /// <summary>
        ///     键为ObjectKeyMember 的Attribute，值为键为ObjectKeyMember对象
        /// </summary>
        private readonly Dictionary<string, ObjectKeyMember> _members;

        /// <summary>
        ///     对象的模型类型
        /// </summary>
        private readonly StructuralType _objectType;

        /// <summary>
        ///     原始对象集合
        /// </summary>
        private readonly List<ObjectKeyMember> _orginMembers;

        /// <summary>
        ///     对象类型（模型类型）的名称
        /// </summary>
        private readonly string _typeName;

        /// <summary>
        ///     对象类型（模型类型）的命名空间
        /// </summary>
        private readonly string _typeNamespace;

        /// <summary>
        ///     创建对象标识实例。
        /// </summary>
        /// <param name="modelType">对象的类型（模型类型）</param>
        /// <param name="members">成员集合</param>
        public ObjectKey(StructuralType modelType, List<ObjectKeyMember> members)
        {
            if (members == null || members.Count <= 0)
                throw new ArgumentException($"构造对象标识失败,{modelType.FullName}的标识序列不能为空");
            _members = members.ToDictionary(p => p.Attribute);
            _typeNamespace = modelType.ClrType.Namespace;
            _typeName = modelType.ClrType.Name;
            _objectType = modelType;
            _orginMembers = members;
        }

        /// <summary>
        ///     为反序列化构造对象标识实例
        /// </summary>
        /// <param name="info">序列化信息</param>
        /// <param name="context">流式上下文</param>
        public ObjectKey(SerializationInfo info, StreamingContext context)
        {
            _typeName = info.GetString("typeName");
            _typeNamespace = info.GetString("typeNamespace");
            _orginMembers = (List<ObjectKeyMember>)info.GetValue("orginMembers", typeof(List<ObjectKeyMember>));
            _members = _orginMembers.ToDictionary(p => p.Attribute);
        }

        /// <summary>
        ///     构造对象标识实例
        /// </summary>
        /// <param name="typeNameStr">类型名称字符串</param>
        /// <param name="members">对象键成员集合</param>
        private ObjectKey(string typeNameStr, List<ObjectKeyMember> members)
        {
            var typeNameSplits = typeNameStr.Split('.');
            _members = members.ToDictionary(p => p.Attribute);
            _typeNamespace = typeNameSplits[0];
            _typeName = typeNameSplits[1];
            _orginMembers = members;
        }

        /// <summary>
        ///     获取对象类型（模型类型）的命名空间
        /// </summary>
        public string TypeNamespace => _typeNamespace;

        /// <summary>
        ///     获取类型（模型类型）的名称
        /// </summary>
        public string TypeName => _typeName;

        /// <summary>
        ///     获取对象的模型类型
        /// </summary>
        public StructuralType ObjectType => _objectType;

        /// <summary>
        ///     获取对象标识的成员。
        /// </summary>
        public List<ObjectKeyMember> Members => _members.Values.ToList();

        /// <summary>
        ///     根据属性名获取属性值。
        /// </summary>
        /// <param name="attrName">属性名</param>
        public object this[string attrName]
        {
            get => _members.Keys.Contains(attrName) ? _members[attrName].Value : null;
            set
            {
                if (!_members.Keys.Contains(attrName)) return;
                _members.Remove(attrName);
                _members.Add(attrName, new ObjectKeyMember(attrName, value));
            }
        }

        /// <summary>
        ///     判定对象标识与另外一个对象标识是否相等。相等返回true，否则返回false。
        ///     算法：
        ///     （1）如果成员数不相等，判定为不相等；
        ///     （2）如果成员数相等，将两者的成员根据Attribute排序，顺次调用各成员的Equals方法，当且仅当全部成员的Equals方法返回true时判为相等。
        /// </summary>
        /// <param name="other">另外一个对象键</param>
        public bool Equals(ObjectKey other)
        {
            //判断TypeName
            if (other == null || _typeNamespace != other.TypeNamespace || _typeName != other.TypeName) return false;
            //比较Members
            var isEquals = true;
            if (other._members.Count != _members.Count) return false;
            //排序后比较值
            var otherMembers = other._members.Values.ToList().OrderBy(p => p.Attribute).ToArray();
            var members = _members.Values.ToList().OrderBy(p => p.Attribute).ToArray();
            for (var i = 0; i < otherMembers.Length; i++)
                if (otherMembers[i].Attribute != members[i].Attribute ||
                    !otherMembers[i].Value.Equals(members[i].Value))
                    isEquals = false;
            return isEquals;
        }

        /// <summary>
        ///     指定序列化策略
        /// </summary>
        /// <param name="info">序列化信息</param>
        /// <param name="context">上下文</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("typeName", _typeName);
            info.AddValue("typeNamespace", _typeNamespace);
            info.AddValue("orginMembers", _orginMembers);
        }

        /// <summary>
        ///     判定对象标识与另外一个对象标识是否相等，（重写Object.Equals方法）。
        ///     相等返回true，否则返回false。
        ///     本方法调用Equals(ObjectKey)方法。
        /// </summary>
        /// <param name="other">另外一个对象键</param>
        public override bool Equals(object other)
        {
            if (other is ObjectKey key)
                return Equals(key);
            return false;
        }

        /// <summary>
        ///     返回对象的Hash码，（重写Object.GetHashCode方法）。
        ///     算法：
        ///     （1）将成员按Attribute排序；
        ///     （2）顺次串联各成员的Attribute和Value属性；
        ///     （3）取上述结果字符串的Hash码；
        ///     （4）返回上述Hash码。
        /// </summary>
        public override int GetHashCode()
        {
            if (_members == null) throw new NullReferenceException("Key属性不可为空.");
            //Member排序后 加入字符串
            var objectKeyMember = _members.Values.ToList();
            var temp = objectKeyMember.OrderBy(p => p.Attribute).ToList();
            var contentBuilder = new StringBuilder();
            foreach (var tempKeyMember in temp)
                contentBuilder.Append($"{tempKeyMember.Attribute}{tempKeyMember.Value}");
            var resultBuilder = new StringBuilder();
            //TypeName加入字符串
            if (!string.IsNullOrEmpty(_typeNamespace)) resultBuilder.Append(_typeNamespace);
            if (!string.IsNullOrEmpty(_typeName)) resultBuilder.Append(_typeName);
            resultBuilder.Append(contentBuilder);
            //字符串的HashCode
            return resultBuilder.ToString().GetHashCode();
        }

        /// <summary>
        ///     重载等于运算符（==），算法同Equals方法。
        /// </summary>
        /// <param name="key1">第一个对象键</param>
        /// <param name="key2">第二个对象键</param>
        public static bool operator ==(ObjectKey key1, ObjectKey key2)
        {
            if (Equals(key1, null) && Equals(key2, null)) return true;
            return !Equals(key1, null) && key1.Equals(key2);
        }

        /// <summary>
        ///     重载不等于运算符（!=），算法为Equals方法结果取反。
        /// </summary>
        /// <param name="key1">第一个对象键</param>
        /// <param name="key2">第二个对象键</param>
        public static bool operator !=(ObjectKey key1, ObjectKey key2)
        {
            return !(key1 == key2);
        }

        /// <summary>
        ///     返回对象标识的字符串表示形式。重写Object.ToString()。
        ///     算法：
        ///     首先将标识成员按Attribute排序，然后按以下方法生成字符串：对象标识字符串=TypeNamespace + TypeName +
        ///     各标识成员的字符串表示形式。
        ///     即typeName[keyMember1-...-KeyMember999]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (null == _members) throw new NullReferenceException("Key属性不可为空.");
            //将成员按Attribute排序
            var temp = Members.OrderBy(p => p.Attribute).ToList();
            var contentBuilder = new StringBuilder();
            foreach (var tempKeyMember in temp) contentBuilder.Append($"{tempKeyMember}-");
            var content = contentBuilder.ToString();
            content = content.TrimEnd('-');
            //加入TypeName
            var resultBuildr = new StringBuilder();
            if (!string.IsNullOrEmpty(_typeNamespace)) resultBuildr.Append($"{_typeNamespace}.");
            if (!string.IsNullOrEmpty(_typeName)) resultBuildr.Append($"{_typeName}[");
            resultBuildr.Append(content);
            resultBuildr.Append("]");
            //拼接后返回
            return resultBuildr.ToString();
        }

        /// <summary>
        ///     根据按既定规则编码的字符串生成ObjectKey实例。
        ///     编码规则与ToString方法一致。
        /// </summary>
        /// <returns></returns>
        public static ObjectKey FromString(string keyString)
        {
            //用正则切分字符串
            var reg = new Regex("(\\[[^\\]]*\\])");
            var keySplits = reg.Split(keyString);
            //构造标识成员
            var keyMemberList = new List<ObjectKeyMember>();

            foreach (var keySplit in keySplits)
            {
                //空的和不含冒号的不是要处理的
                if (string.IsNullOrEmpty(keySplit) || !keySplit.Contains(":"))
                    continue;
                //此keySplit即为[a:1-b:2...z:26]
                var memberSplit = keySplit.Replace("[", "").Replace("]", "").Split('-');
                foreach (var member in memberSplit)
                {
                    var filedSplits = member.Split(':');
                    if (filedSplits.Length == 2) keyMemberList.Add(new ObjectKeyMember(filedSplits[0], filedSplits[1]));
                }
            }

            return new ObjectKey(keySplits[0], keyMemberList);
        }
    }
}