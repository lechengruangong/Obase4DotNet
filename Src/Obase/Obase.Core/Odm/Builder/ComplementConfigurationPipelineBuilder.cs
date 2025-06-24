/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：补充配置管道建造器,负责建造补充配置管道.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 14:42:55
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     补充配置管道建造器
    /// </summary>
    public class ComplementConfigurationPipelineBuilder
    {
        /// <summary>
        ///     寄存每次USE的委托
        /// </summary>
        private readonly List<Func<IComplementConfigurator, IComplementConfigurator>> _components =
            new List<Func<IComplementConfigurator, IComplementConfigurator>>();

        /// <summary>
        ///     建造补充配置管道。
        /// </summary>
        /// <returns>
        ///     补充配置管道。
        ///     实施说明
        ///     递归到最后一个委托，调用该委托创建最后一个配置器，然后将其传入倒数第二个委托，创建倒数第二个配置器，依此类推，直到第一个。
        /// </returns>
        internal IComplementConfigurator Build()
        {
            IComplementConfigurator complementConfigurator = null;
            //倒序查找 调用每个委托
            for (var c = _components.Count - 1; c >= 0; c--)
                complementConfigurator = _components[c](complementConfigurator);

            return complementConfigurator;
        }

        /// <summary>
        ///     向补充配置管道注册中间件，该管道用于在生成模型过程中执行补充配置。
        /// </summary>
        /// <returns>补充配置管道建造器。</returns>
        /// <param
        ///     name="middlewareDelegate">
        ///     中间件委托，代表创建管道中间件（即补充配置器）的方法，该方法的参数用于指定管道中的下一个配置器，返回值为生成
        ///     的中间件。
        /// </param>
        public ComplementConfigurationPipelineBuilder Use(
            Func<IComplementConfigurator, IComplementConfigurator> middlewareDelegate)
        {
            //存放委托
            _components.Add(middlewareDelegate);
            return this;
        }
    }
}
