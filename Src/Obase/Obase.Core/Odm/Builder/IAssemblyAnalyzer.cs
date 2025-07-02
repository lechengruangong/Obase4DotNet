/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义从程序集提取类型并注册至模型的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:04:40
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义从程序集提取类型并注册至模型的规范。
    /// </summary>
    public interface IAssemblyAnalyzer
    {
        /// <summary>
        ///     从指定的程序集提取类型并注册到模型。
        /// </summary>
        /// <param name="assembly">要解析的程序集。</param>
        /// <param name="modelBuilder">建模器。</param>
        void Analyze(Assembly assembly, ModelBuilder modelBuilder);

        /// <summary>
        ///     从指定的类型数组中提取类型并注册到模型
        /// </summary>
        /// <param name="types">指定的类型</param>
        /// <param name="modelBuilder">建模器</param>
        void Analyze(Type[] types, ModelBuilder modelBuilder);
    }
}