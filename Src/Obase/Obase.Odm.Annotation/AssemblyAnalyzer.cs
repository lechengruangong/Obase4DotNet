/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标注建模的程序集分析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:13:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     标注建模的程序集分析器
    /// </summary>
    public class AssemblyAnalyzer : IAssemblyAnalyzer
    {
        /// <summary>
        ///     从指定的程序集提取类型并注册到模型。
        /// </summary>
        /// <param name="assembly">要解析的程序集。</param>
        /// <param name="modelBuilder">建模器。</param>
        public void Analyze(Assembly assembly, ModelBuilder modelBuilder)
        {
            //查找所有类型
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
            foreach (var type in types)
            {
                //类型的标记
                var attrs = type.GetCustomAttributes().ToArray();
                if (attrs.Length > 0)
                {
                    if (attrs.Count(p => p is EntityAttribute || p is AssociationAttribute || p is ComplexAttribute) >
                        1)
                        throw new InvalidOperationException($"不支持将{type.FullName}同时标注为多个模型类型");
                    var attribute = attrs.LastOrDefault(p =>
                        p is EntityAttribute || p is AssociationAttribute || p is ComplexAttribute);
                    if (attribute == null)
                        continue;
                    //配置实体型
                    if (attribute is EntityAttribute) modelBuilder.Entity(type);

                    //配置显式关联型
                    if (attribute is AssociationAttribute) modelBuilder.Association(type);

                    //配置复杂类型
                    if (attribute is ComplexAttribute) modelBuilder.Complex(type);
                }
            }

            //都注册完了 再配置隐式关联
            foreach (var type in types)
            {
                //配置隐式关联型
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    var implicitAttr = prop.GetCustomAttributes<ImplicitAssociationAttribute>().LastOrDefault();
                    if (implicitAttr != null)
                    {
                        //解析并创建隐式关联型
                        var endType = prop.PropertyType.GetInterface("IEnumerable");
                        if (endType != null && prop.PropertyType != typeof(string)) //集合属性
                            endType = prop.PropertyType.GetGenericArguments()[0];
                        else
                            endType = prop.PropertyType;

                        var endTypes = new[] { type, endType };
                        var builder = modelBuilder.Association();
                        foreach (var end in endTypes) builder.AssociationEnd(end);
                        builder.ToTable(implicitAttr.Target);
                    }
                }
            }
        }
    }
}