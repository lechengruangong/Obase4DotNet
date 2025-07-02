/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构视图实例化单元.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 16:21:25
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     异构视图实例化单元，包含一个基础视图实例及一个与之配对的附加视图实例集。附加视图实例集中的实例与极限分解得到的附加视图一一对应。
    /// </summary>
    internal class InstantiationUnit
    {
        /// <summary>
        ///     基础视图实例。
        /// </summary>
        private readonly object _baseInstance;

        /// <summary>
        ///     基础视图。
        /// </summary>
        private readonly TypeView _baseView;

        /// <summary>
        ///     存储附加视图实例的字典，其中鍵为附加视图，值为该视图的实例（一个或多个）。
        /// </summary>
        private Dictionary<TypeView, AttachingInstanceSet> _attachingInstances;


        /// <summary>
        ///     创建InstantiationUnit实例。
        /// </summary>
        /// <param name="baseInstance">基础视图实例。</param>
        /// <param name="baseView">基础视图。</param>
        internal InstantiationUnit(object baseInstance, TypeView baseView)
        {
            _baseInstance = baseInstance;
            _baseView = baseView;
        }

        /// <summary>
        ///     向实例化单元添加附加视图实例集。
        /// </summary>
        /// <param name="instanceSet">附加视图实例集。</param>
        internal void AddAttachingInstance(AttachingInstanceSet instanceSet)
        {
            if (_attachingInstances == null) _attachingInstances = new Dictionary<TypeView, AttachingInstanceSet>();
            _attachingInstances[instanceSet.AttachingView] = instanceSet.FilterByBaseInstance(_baseInstance, true);
        }

        /// <summary>
        ///     为实例化单元生成指定个数的复本。
        /// </summary>
        /// <returns>生成的实例化单元复本。</returns>
        /// <param name="count">要复制的副本数。</param>
        internal InstantiationUnit[] Clone(int count)
        {
            var clones = new InstantiationUnit[count];
            //为每个副本创建一个新的InstantiationUnit实例，使用相同的基础实例和基础视图。
            for (var i = 0; i < count; i++)
                clones[i] = new InstantiationUnit(_baseInstance, _baseView);
            return clones;
        }

        /// <summary>
        ///     从实例化单元获取指定元素的值。
        /// </summary>
        /// <returns>元素的值。</returns>
        /// <param name="element">要获取其值的元素。</param>
        internal object GetValue(TypeElement element)
        {
            //取元素宿主
            var typeView = (TypeView)element.HostType;
            if (typeView == _baseView) return element.GetValue(_baseInstance);

            var result = new List<object>();
            //获取实例集中的实例
            var instances = _attachingInstances[typeView].Instances;

            if (instances == null || instances.Length == 0)
                return null;

            foreach (var instance in instances)
                //从附加实例取元素值，并添加到结果集。
                result.Add(instance == null ? null : element.GetValue(instance));

            return result.Count == 1 ? result[0] : result.ToArray();
        }
    }
}