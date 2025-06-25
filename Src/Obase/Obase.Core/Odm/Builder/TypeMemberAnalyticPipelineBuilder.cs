/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型成员解析管道建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:52:55
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     类型成员解析管道建造器
    /// </summary>
    public class TypeMemberAnalyticPipelineBuilder
    {
        /// <summary>
        ///     寄存每次USE的委托
        /// </summary>
        private readonly List<Func<ITypeMemberAnalyzer, ITypeMemberAnalyzer>> _components =
            new List<Func<ITypeMemberAnalyzer, ITypeMemberAnalyzer>>();


        /// <summary>
        ///     建造类型成员解析管道。
        /// </summary>
        /// <returns>
        ///     类型成员解析管道。
        ///     实施说明
        ///     递归到最后一个委托，调用该委托创建最后一个解析器，然后将其传入倒数第二个委托，创建倒数第二个解析器，依此类推，直到第一个。
        /// </returns>
        internal ITypeMemberAnalyzer Build()
        {
            ITypeMemberAnalyzer analyzer = null;
            //倒序遍历所有注册的中间件委托 调用每个委托，传入上一个委托的结果
            for (var c = _components.Count - 1; c >= 0; c--)
                analyzer = _components[c](analyzer);

            return analyzer;
        }

        /// <summary>
        ///     向类型成员解析管道注册中间件，该管道用于在反射建模过程中解析类型成员。
        /// </summary>
        /// <returns>类型成员解析管道建造器。</returns>
        /// <param
        ///     name="middlewareDelegate">
        ///     中间件委托，代表创建管道中间件（即解析器）的方法，该方法的参数用于指定管道中的下一个解析器，返回值为生成的中
        ///     间件。
        /// </param>
        public TypeMemberAnalyticPipelineBuilder Use(Func<ITypeMemberAnalyzer, ITypeMemberAnalyzer> middlewareDelegate)
        {
            _components.Add(middlewareDelegate);
            return this;
        }
    }
}