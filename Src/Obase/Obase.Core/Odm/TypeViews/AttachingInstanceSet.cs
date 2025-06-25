/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：附加视图实例集.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 16:07:02
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     附加视图实例集，存储某一附加视图的一个或多个实例。
    /// </summary>
    public class AttachingInstanceSet
    {
        /// <summary>
        ///     附加视图的视图引用
        /// </summary>
        private readonly ViewReference _attachingRef;

        /// <summary>
        ///     作为集中视图实例的类型的附加视图。
        /// </summary>
        private readonly TypeView _attachingView;

        /// <summary>
        ///     附加视图的实例。
        /// </summary>
        private readonly object[] _instances;

        /// <summary>
        ///     创建AttachingInstanceSet实例。
        /// </summary>
        /// <param name="attachingView">作为集中视图实例的类型的附加视图。</param>
        /// <param name="attachingRef">附加视图的视图引用</param>
        /// <param name="instances">一个或多个视图实例。</param>
        internal AttachingInstanceSet(TypeView attachingView, ViewReference attachingRef, object[] instances)
        {
            _attachingView = attachingView;
            _attachingRef = attachingRef;
            _instances = instances;
        }

        /// <summary>
        ///     获取视图实例集包含的视图实例。
        /// </summary>
        internal object[] Instances => _instances;

        /// <summary>
        ///     获取作为集中视图实例的类型的附加视图。
        /// </summary>
        public TypeView AttachingView => _attachingView;

        /// <summary>
        ///     根据平展鍵对视图实例分组，每一组构成一个新的实例集。
        /// </summary>
        /// <returns>每组实例构成的实例集。</returns>
        internal AttachingInstanceSet[] GroupByFlatteningKey()
        {
            var flatteningAttrs = _attachingView.FlatteningKey;
            if (flatteningAttrs == null) return new[] { this };

            //平展方法
            object[] Flattening(object o)
            {
                var len = flatteningAttrs.Length;
                var key = new object[len];
                for (var i = 0; i < len; i++)
                    //获取平展属性的值
                    key[i] = flatteningAttrs[i].GetValue(o);
                return key;
            }

            //根据平展后的对象分组 使用自定义比较器
            var instances = _instances.GroupBy(Flattening, new Comparer());
            //将每组转换为AttachingInstanceSet
            return instances.Select(p => new AttachingInstanceSet(_attachingView, _attachingRef, p.ToArray()))
                .ToArray();
        }


        /// <summary>
        ///     从附加实例中筛选出一个子集，该子集由指定的基础实例引用。
        ///     说明
        ///     附加实例集中的实例可能是对应于一组基础实例的，可以使用本方法从中筛选出对应于特定基础实例的子集。
        /// </summary>
        /// <param name="baseInstance">基础实例。</param>
        /// <param name="removing">指示是否移除筛选出的子集。默认不移除。</param>
        internal AttachingInstanceSet FilterByBaseInstance(object baseInstance, bool removing = false)
        {
            var objects = _instances;
            var subSet = _attachingRef.FilterTarget(ref objects, baseInstance, _attachingView, removing);

            return new AttachingInstanceSet(_attachingView, _attachingRef, subSet);
        }

        /// <summary>
        ///     私有比较器，用于比较对象数组。
        /// </summary>
        private class Comparer : IEqualityComparer<object[]>
        {
            /// <summary>
            ///     比较方法
            /// </summary>
            /// <param name="x">第一个数组</param>
            /// <param name="y">第二个数组</param>
            /// <returns></returns>
            public bool Equals(object[] x, object[] y)
            {
                //顺序比较
                if (y != null && x != null && x.Length != y.Length) return false;
                if (x != null)
                {
                    var len = x.Length;
                    for (var i = 0; i < len; i++)
                        if (y != null && x[i] != y[i])
                            return false;
                }

                return true;
            }

            /// <summary>
            ///     获取哈希码
            /// </summary>
            /// <param name="obj">对象</param>
            /// <returns></returns>
            public int GetHashCode(object[] obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}