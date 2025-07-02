/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的程序集解析器,负责从程序集中注册结构化类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 14:47:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     默认的程序集解析器
    /// </summary>
    public class DefaultAssemblyAnalyzer : IAssemblyAnalyzer
    {
        /// <summary>
        ///     存储从程序集解析类型过程中应忽略的类型
        /// </summary>
        private readonly HashSet<Type> _ignoredTypes;

        /// <summary>
        ///     构造默认的程序集解析器
        /// </summary>
        /// <param name="ignoredTypes">忽略的类型</param>
        public DefaultAssemblyAnalyzer(HashSet<Type> ignoredTypes)
        {
            _ignoredTypes = ignoredTypes;
        }

        /// <summary>
        ///     从指定的程序集提取类型并注册到模型。
        /// </summary>
        /// <param name="assembly">要解析的程序集。</param>
        /// <param name="modelBuilder">建模器。</param>
        public void Analyze(Assembly assembly, ModelBuilder modelBuilder)
        {
            //1.显式关联推断 如果类（Class）上未定义符合“标识属性推断”约定的访问器，推断其为关联类型。
            //2.实体类型推断 如果类（Class）不能推断为显式关联，推断为实体类型。
            //3.复杂类型推断 结构体推断为复杂类型。
            var types = assembly.GetExportedTypes();
            Analyze(types, modelBuilder);
        }

        /// <summary>
        ///     从指定的类型数组中提取类型并注册到模型
        /// </summary>
        /// <param name="types">指定的类型</param>
        /// <param name="modelBuilder">建模器</param>
        public void Analyze(Type[] types, ModelBuilder modelBuilder)
        {
            //解析所有的类型
            foreach (var type in types)
            {
                //忽略的类型不参与推断
                if (_ignoredTypes.Contains(type))
                    continue;

                //已配置的不参与
                if (modelBuilder.FindConfiguration(type) != null)
                    continue;

                //枚举 接口 抽象类不参与推断
                if (type.IsEnum || type.IsInterface || type.IsAbstract)
                    continue;

                //如果是类
                if (type.IsClass)
                {
                    //定义了符合“标识属性推断”约定的访问器
                    if (Utils.ExistIdentity(type, out _))
                        //推断为实体类型
                        modelBuilder.Entity(type);
                    //没定义
                    else
                        //推断为显式关联
                        modelBuilder.Association(type);
                }
                //如果是结构体
                else if (type.IsValueType)
                {
                    //推断为复杂类型
                    modelBuilder.Complex(type);
                }
            }
        }
    }
}