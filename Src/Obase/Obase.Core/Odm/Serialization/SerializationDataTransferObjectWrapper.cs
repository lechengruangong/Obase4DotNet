/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化数据传输对象包装对象.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 15:00:04
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化数据传输对象包装对象
    ///     内部引用序列化数据传输对象 用于简化序列化过程中的类型检测
    /// </summary>
    public class SerializationDataTransferObjectWrapper
    {
        /// <summary>
        ///     初始化序列化数据传输对象包装对象
        /// </summary>
        /// <param name="dto">序列化数据传输对象</param>
        public SerializationDataTransferObjectWrapper(List<SerializationDataTransferObject> dto)
        {
            Dto = dto;
        }

        /// <summary>
        ///     获取或设置序列化数据传输对象
        /// </summary>
        public List<SerializationDataTransferObject> Dto { get; set; }

        /// <summary>
        ///     获取或设置创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        ///     获取或设置修改时间
        /// </summary>
        public DateTime ModifiedTime { get; set; }
    }
}