/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：映射筛选器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:09:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core
{
    /// <summary>
    ///     表示映射筛选器。
    ///     映射筛选器用于从存储源选择对象，工作流仅作用于被选中的对象。
    /// </summary>
    public class MappingFilter
    {
        /// <summary>
        ///     一个委托，代表映射筛选器制作完成时回调的方法。
        /// </summary>
        private readonly Action<ELogicalOperator> _filterReady;

        /// <summary>
        ///     指示当前筛选器与已存在的筛选器执行逻辑“与”还是“或”运算。
        /// </summary>
        private readonly ELogicalOperator _logicOperator;

        /// <summary>
        ///     一个委托，代表映射筛选器片段制作完成时回调的方法。
        ///     该方法的第一个参数表示筛选器的依据域，第二个字段表示参考值。
        /// </summary>
        private readonly Action<string, object> _segmentReady;

        /// <summary>
        ///     适用筛选器的工作流
        /// </summary>
        private readonly IMappingWorkflow _workflow;

        /// <summary>
        ///     映射筛选器片段
        /// </summary>
        private List<FilterSegment> _filterSegments;

        /// <summary>
        ///     创建MappingFilter实例。
        /// </summary>
        /// <param name="workflow">适用筛选器的工作流。</param>
        /// <param name="logicOperator">指示当前筛选器与已存在的筛选器执行逻辑“与”还是“或”运算。</param>
        /// <param name="filterReady">一个委托，代表映射筛选器制作完成时回调的方法。</param>
        /// <param name="segmentReady">一个委托，代表映射筛选器片段制作完成时回调的方法。</param>
        public MappingFilter(IMappingWorkflow workflow, ELogicalOperator logicOperator,
            Action<ELogicalOperator> filterReady, Action<string, object> segmentReady)
        {
            _workflow = workflow;
            _logicOperator = logicOperator;
            _filterReady = filterReady;
            _segmentReady = segmentReady;
        }

        /// <summary>
        ///     在映射筛选器中追加一个片段。
        /// </summary>
        /// <returns>新增的筛选器片段。</returns>
        internal FilterSegment AddSegment()
        {
            if (_filterSegments == null)
                _filterSegments = new List<FilterSegment>();

            //构造一个新的片段
            var filter = new FilterSegment(this, _segmentReady);
            _filterSegments.Add(filter);

            return filter;
        }

        /// <summary>
        ///     通知映射筛选器制作过程已完成。
        /// </summary>
        /// <returns>适用当前筛选器的映射工作流。</returns>
        internal IMappingWorkflow End()
        {
            //调用结束回调
            _filterReady(_logicOperator);
            return _workflow;
        }
    }
}