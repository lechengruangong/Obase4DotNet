/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构查询分段执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:14:51
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构查询分段执行器，提供执行异构查询分解所得片段的方案，执行该方案所得结果即为异构查询的结果
    /// </summary>
    public class HeterogQuerySegmentallyExecutor : IHeterogQuerySegmentallyExecutor
    {
        /// <summary>
        ///     执行异构查询分解所得的片段。
        /// </summary>
        /// <param name="segments">对异构查询实施分解产生的片段。</param>
        /// <param name="heterogQueryProvider">异构查询提供程序，用于执行从异构运算中分解出的附加查询。</param>
        /// <param name="attachObject">用于将对象附加到对象上下文的委托。</param>
        /// <param name="attachRoot">指示是否附加根对象。</param>
        public object Execute(HeterogQuerySegments segments, HeterogQueryProvider heterogQueryProvider,
            AttachObject attachObject,
            bool attachRoot = true)
        {
            //创建对应的运算执行器
            var executor = HeterogOpExecutor.Create(segments.MainQuery, heterogQueryProvider.StorageProviderCreator,
                heterogQueryProvider.Model,
                heterogQueryProvider.OnPreExecuteSql, heterogQueryProvider.OnPostExecuteSql, heterogQueryProvider,
                heterogQueryProvider.BaseProvider);

            var asRoot = false;
            if (attachRoot)
            {
                if (segments.Complement == null)
                    asRoot = true;
                else
                    asRoot = segments.MainTail.ResultType == segments.Complement.Tail.ResultType;
            }

            //执行主查询
            var result = executor.Execute(segments.MainTail, segments.MainQuery, segments.Including,
                heterogQueryProvider.AttachObject, asRoot);

            if (segments.Complement != null)
            {
                var oopExecutor = segments.Complement.GeneratePipeline();
                //执行补充运算
                if (result is IEnumerable instances) return oopExecutor.Execute(instances);

                return oopExecutor.Execute(result);
            }

            return result;
        }
    }
}