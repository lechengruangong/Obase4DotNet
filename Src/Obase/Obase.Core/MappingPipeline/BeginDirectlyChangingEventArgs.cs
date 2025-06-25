/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：开始就地修改事件数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:52:08
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     BeginDirectlyChanging事件数据类。
    /// </summary>
    public class BeginDirectlyChangingEventArgs : DirectlyChangingEventArgs
    {
        /// <summary>
        ///     创建BeginDirectlyChangingEventArgs实例，并指定条件表达式和属性新值字典。
        /// </summary>
        /// <param name="expression">条件表达式。</param>
        /// <param name="changeType">更改类型</param>
        /// <param name="objectType">修改的对象类型</param>
        /// <param name="attributes">属性新值字典。</param>
        public BeginDirectlyChangingEventArgs(Expression expression, EDirectlyChangeType changeType, Type objectType,
            KeyValuePair<string, object>[] attributes = null) : base(expression, changeType, objectType, attributes)
        {
        }
    }
}