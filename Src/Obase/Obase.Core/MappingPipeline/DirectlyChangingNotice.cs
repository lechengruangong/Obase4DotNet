/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：就地修改通知.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 09:45:47
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     就地修改通知
    /// </summary>
    [Serializable]
    public class DirectlyChangingNotice : ChangeNotice
    {
        /// <summary>
        ///     筛选条件表达式
        /// </summary>
        private readonly string _criteria;

        /// <summary>
        ///     就地修改类型
        /// </summary>
        private readonly EDirectlyChangeType _directlyChangeType;

        /// <summary>
        ///     修改的字段和值键值对
        /// </summary>
        private readonly Dictionary<string, object> _newValues;

        /// <summary>
        ///     初始化ChangeNotice类的新实例。
        /// </summary>
        /// <param name="changeAction">对象变更行为，可取值为Create、Update、Delete、Increase，分别对应“创建”、“修改”、“删除”、“就地累加”四种行为。</param>
        /// <param name="nameSpace">对象类型的命名空间</param>
        /// <param name="objectType">对象类型名称</param>
        /// <param name="criteria">筛选条件表达式</param>
        /// <param name="directlyChangeType">就地修改类型</param>
        /// <param name="newValues">修改的字段和值键值对</param>
        public DirectlyChangingNotice(string changeAction, string nameSpace, string objectType,
            string criteria, EDirectlyChangeType directlyChangeType, Dictionary<string, object> newValues) : base(
            EChangeNoticeType.DirectlyChanging,
            changeAction, nameSpace, objectType)
        {
            _criteria = criteria;
            _directlyChangeType = directlyChangeType;
            _newValues = newValues;
        }

        /// <summary>
        ///     筛选条件表达式
        /// </summary>
        public string Criteria => _criteria;

        /// <summary>
        ///     就地修改类型
        /// </summary>
        public EDirectlyChangeType DirectlyChangeType => _directlyChangeType;

        /// <summary>
        ///     修改的字段和值键值对
        /// </summary>
        public Dictionary<string, object> NewValues => _newValues;
    }
}