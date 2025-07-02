/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象变更通知编写器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:20:55
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     对象变更通知编写器
    /// </summary>
    public class ObjectChangeNoticeWriter : IChangeNoticeWriter
    {
        /// <summary>
        ///     对象的属性及其取值
        /// </summary>
        private readonly List<ObjectAttribute> _attributes;

        /// <summary>
        ///     对象变更行为，可取值为Create、Update、Delete、Increase，分别对应“创建”、“修改”、“删除”、“就地累加”四种行为。
        /// </summary>
        private readonly string _changeAction;

        /// <summary>
        ///     对象类型的命名空间。
        /// </summary>
        private readonly string _namespace;

        /// <summary>
        ///     对象的标识
        /// </summary>
        private readonly List<ObjectAttribute> _objectKeys;

        /// <summary>
        ///     对象类型名称。
        /// </summary>
        private readonly string _objectType;

        /// <summary>
        ///     初始化对象变更通知编写器
        /// </summary>
        /// <param name="changeAction">对象变更行为</param>
        /// <param name="ns">对象类型的命名空间</param>
        /// <param name="objectType">对象类型名称</param>
        /// <param name="attributes">对象的属性及其取值</param>
        /// <param name="objectKeys">对象的标识</param>
        public ObjectChangeNoticeWriter(string changeAction, string ns, string objectType,
            List<ObjectAttribute> attributes, List<ObjectAttribute> objectKeys)
        {
            _changeAction = changeAction;
            _namespace = ns;
            _objectType = objectType;
            _attributes = attributes;
            _objectKeys = objectKeys;
        }

        /// <summary>
        ///     编写通知的字符串形式
        /// </summary>
        /// <param name="serializeFunction">序列化方法</param>
        /// <returns></returns>
        public string Write(Func<object, string> serializeFunction)
        {
            return serializeFunction.Invoke(Write());
        }

        /// <summary>
        ///     编写通知
        /// </summary>
        /// <returns></returns>
        public ChangeNotice Write()
        {
            return new ObjectChangeNotice(_changeAction, _namespace, _objectType,
                _attributes, _objectKeys);
        }
    }
}