/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：就地修改变更通知编写器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 09:48:12
└──────────────────────────────────────────────────────────────┘
*/


using System;
using System.Collections.Generic;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     就地修改变更通知编写器
    /// </summary>
    public class DirectlyChangingNoticeWriter : IChangeNoticeWriter
    {
        /// <summary>
        ///     对象变更行为，可取值为Create、Update、Delete、Increase，分别对应“创建”、“修改”、“删除”、“就地累加”四种行为。
        /// </summary>
        private readonly string _changeAction;


        /// <summary>
        ///     筛选条件表达式
        /// </summary>
        private readonly string _criteria;

        /// <summary>
        ///     就地修改类型
        /// </summary>
        private readonly EDirectlyChangeType _directlyChangeType;

        /// <summary>
        ///     对象类型的命名空间。
        /// </summary>
        private readonly string _namespace;

        /// <summary>
        ///     修改的字段和值键值对
        /// </summary>
        private readonly Dictionary<string, object> _newValues;

        /// <summary>
        ///     对象类型名称。
        /// </summary>
        private readonly string _objectType;

        /// <summary>
        ///     构造就地修改变更通知编写器
        /// </summary>
        /// <param name="changeAction">对象变更行为</param>
        /// <param name="ns">对象类型的命名空间</param>
        /// <param name="objectType">对象类型名称</param>
        /// <param name="criteria">筛选条件表达式</param>
        /// <param name="directlyChangeType">就地修改类型</param>
        /// <param name="newValues">修改的字段和值键值对</param>
        public DirectlyChangingNoticeWriter(string changeAction, string ns, string objectType, string criteria,
            EDirectlyChangeType directlyChangeType, Dictionary<string, object> newValues)
        {
            _changeAction = changeAction;
            _namespace = ns;
            _objectType = objectType;
            _criteria = criteria;
            _directlyChangeType = directlyChangeType;
            _newValues = newValues;
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
            return new DirectlyChangingNotice(_changeAction, _namespace, _objectType, _criteria, _directlyChangeType,
                _newValues);
        }
    }
}