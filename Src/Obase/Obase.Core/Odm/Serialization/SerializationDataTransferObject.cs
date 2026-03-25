/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化数据传输对象.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 12:36:42
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化数据传输对象
    ///     用于在序列化过程中传输数据
    /// </summary>
    public class SerializationDataTransferObject
    {
        /// <summary>
        ///     初始化序列化数据传输对象
        /// </summary>
        public SerializationDataTransferObject()
        {
            Attributes = new Dictionary<string, object>();
            References = new Dictionary<string, List<string>>();
            ConstructorParameters = new Dictionary<string, object>();
        }

        /// <summary>
        ///     获取或设置序列化过程中分配的唯一ID
        ///     从$0开始递增
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     获取或设置存储要序列化的对象的类型名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        ///     获取或设置此Dto存储的对象是否为根对象
        ///     根对象指的是宿主对象直接引用的对象
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        ///     获取或设置存储属性的字典
        ///     存储需要存储的属性的名称和对应的值
        ///     默认将需要序列化的对象中的Obase基元类型的属性访问器视为属性
        /// </summary>
        public Dictionary<string, object> Attributes { get; set; }

        /// <summary>
        ///     获取或设置存储构造函数参数的字典
        ///     存储需要存储的构造函数参数的索引和对应的值
        ///     索引值从0开始 与构造函数配置的构造器参数参数索引一一对应
        /// </summary>
        public Dictionary<string, object> ConstructorParameters { get; set; }

        /// <summary>
        ///     获取或设置存储引用的其他序列化对象的字典
        ///     存储引用属性的名称和序列号ID的集合
        ///     默认将需要序列化的对象中的其他已配置为序列化模型的属性访问器视为引用
        /// </summary>
        public Dictionary<string, List<string>> References { get; set; }
    }
}