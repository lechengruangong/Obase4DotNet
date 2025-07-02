/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：注册模块后事件数据.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 16:10:03
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.MappingPipeline;

namespace Obase.Core
{
    /// <summary>
    ///     PostRegisterModule事件的事件参数。
    /// </summary>
    public class PostRegisterModuleEventArgs : EventArgs
    {
        /// <summary>
        ///     刚注册的映射模块。
        /// </summary>
        private readonly IMappingModule _module;

        /// <summary>
        ///     初始化PostRegisterModuleEventArgs的新实例。
        /// </summary>
        /// <param name="module">刚注册的映射模块。</param>
        public PostRegisterModuleEventArgs(IMappingModule module)
        {
            _module = module;
        }

        /// <summary>
        ///     获取刚注册的映射模块。
        /// </summary>
        public IMappingModule Module => _module;
    }
}