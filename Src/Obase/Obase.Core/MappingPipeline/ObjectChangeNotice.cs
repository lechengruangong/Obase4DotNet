/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象变更通知.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:20:05
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     对象变更通知
    /// </summary>
    [Serializable]
    public class ObjectChangeNotice : ChangeNotice
    {
        /// <summary>
        ///     对象的属性及其取值
        /// </summary>
        private readonly List<ObjectAttribute> _attributes;

        /// <summary>
        ///     对象的标识
        /// </summary>
        private readonly List<ObjectAttribute> _objectKeys;


        /// <summary>
        ///     初始化ChangeNotice类的新实例。
        /// </summary>
        /// <param name="changeAction">对象变更行为，可取值为Create、Update、Delete、Increase，分别对应“创建”、“修改”、“删除”、“就地累加”四种行为。</param>
        /// <param name="nameSpace">对象类型的命名空间</param>
        /// <param name="objectType">对象类型名称</param>
        /// <param name="attributes">对象的属性及其取值</param>
        /// <param name="objectKeys">对象的标识</param>
        public ObjectChangeNotice(string changeAction, string nameSpace, string objectType,
            List<ObjectAttribute> attributes, List<ObjectAttribute> objectKeys) : base(EChangeNoticeType.ObjectChange,
            changeAction, nameSpace, objectType)
        {
            _attributes = attributes;
            _objectKeys = objectKeys;
        }

        /// <summary>
        ///     对象的属性及其取值
        /// </summary>
        public List<ObjectAttribute> Attributes => _attributes;

        /// <summary>
        ///     对象的标识
        /// </summary>
        public List<ObjectAttribute> ObjectKeys => _objectKeys;
    }
}