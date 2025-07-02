/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：代理类型生成管道建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:50:03
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     代理类型生成管道建造器
    /// </summary>
    public class ProxyTypeGenerationPipelineBuilder
    {
        /// <summary>
        ///     寄存每次USE的委托
        /// </summary>
        private readonly List<Func<IProxyTypeGenerator, IProxyTypeGenerator>> _components =
            new List<Func<IProxyTypeGenerator, IProxyTypeGenerator>>();

        /// <summary>
        ///     建造代理类型生成管道。
        /// </summary>
        /// <returns>
        ///     代理类型生成管道。
        ///     实施说明
        ///     递归到最后一个委托，调用该委托创建最后一个生成器，然后将其传入倒数第二个委托，创建倒数第二个生成器，依此类推，直到第一个。
        /// </returns>
        internal IProxyTypeGenerator Build()
        {
            IProxyTypeGenerator proxyTypeGenerator = null;
            //倒序调用每个委托，创建生成器
            for (var c = _components.Count - 1; c >= 0; c--)
                proxyTypeGenerator = _components[c](proxyTypeGenerator);

            return proxyTypeGenerator;
        }

        /// <summary>
        ///     向代理类型生成管道注册中间件，该管道用于为模型中注册的类型生成代理类。
        /// </summary>
        /// <returns>代理类型生成管道建造器。</returns>
        /// <param
        ///     name="middlewareDelegate">
        ///     中间件委托，代表创建管道中间件（即生成器）的方法，该方法的参数用于指定管道中的下一个生成器，返回值为生成的中
        ///     间件。
        /// </param>
        public ProxyTypeGenerationPipelineBuilder Use(Func<IProxyTypeGenerator, IProxyTypeGenerator> middlewareDelegate)
        {
            // 添加中间件委托到管道组件列表
            _components.Add(middlewareDelegate);
            return this;
        }
    }
}