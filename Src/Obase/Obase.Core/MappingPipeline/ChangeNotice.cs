/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：更改通知基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:58:00
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     为更改通知提供基础实现。
    /// </summary>
    [Serializable]
    public abstract class ChangeNotice
    {
        /// <summary>
        ///     对象变更行为，可取值为Create、Update、Delete、Increase，分别对应“创建”、“修改”、“删除”、“就地累加”四种行为。
        /// </summary>
        private readonly string _changeAction;

        /// <summary>
        ///     对象类型的命名空间。
        /// </summary>
        private readonly string _namespace;

        /// <summary>
        ///     对象类型名称。
        /// </summary>
        private readonly string _objectType;

        /// <summary>
        ///     变更通知的类型。
        /// </summary>
        private readonly EChangeNoticeType _type;

        /// <summary>
        ///     初始化ChangeNotice类的新实例。
        /// </summary>
        /// <param name="type">通知类型。</param>
        /// <param name="changeAction">对象变更行为，可取值为Create、Update、Delete、Increase，分别对应“创建”、“修改”、“删除”、“就地累加”四种行为。</param>
        /// <param name="nameSpace">对象类型的命名空间</param>
        /// <param name="objectType">对象类型名称</param>
        protected ChangeNotice(EChangeNoticeType type, string changeAction, string nameSpace, string objectType)
        {
            _type = type;
            _changeAction = changeAction;
            _namespace = nameSpace;
            _objectType = objectType;
        }

        /// <summary>
        ///     获取对象变更行为，可取值为Create、Update、Delete、Increase，分别对应“创建”、“修改”、“删除”、“就地累加”四种行为。
        ///     实施说明
        ///     附带一个可访问性为internal的Set访问器。
        /// </summary>
        public string ChangeAction => _changeAction;

        /// <summary>
        ///     获取对象类型的命名空间。
        ///     实施说明
        ///     附带一个可访问性为internal的Set访问器。
        /// </summary>
        public string Namespace => _namespace;

        /// <summary>
        ///     获取对象类型的名称。
        ///     实施说明
        ///     附带一个可访问性为internal的Set访问器。
        /// </summary>
        public string ObjectType => _objectType;

        /// <summary>
        ///     获取变更通知的类型。
        /// </summary>
        public EChangeNoticeType Type => _type;
    }
}