/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：创建模型后事件数据.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 16:11:07
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core
{
    /// <summary>
    ///     PostCreatedModel事件的事件参数。
    /// </summary>
    public class PostCreateModelEventArgs : EventArgs
    {
        /// <summary>
        ///     刚创建的对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     初始化PostCreateModelEventArgs的新实例。
        /// </summary>
        /// <param name="model">刚创建的对象数据模型。</param>
        public PostCreateModelEventArgs(ObjectDataModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     获取刚创建的对象数据模型。
        /// </summary>
        public ObjectDataModel Model => _model;
    }
}