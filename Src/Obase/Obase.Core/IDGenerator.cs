﻿/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标识生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:56:08
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core
{
    /// <summary>
    ///     定义一个生成标识的方法
    /// </summary>
    /// <typeparam name="T">标识的类型</typeparam>
    public interface IDGenerator<out T>
    {
        /// <summary>
        ///     生成下一个标识。
        /// </summary>
        T Next();
    }
}