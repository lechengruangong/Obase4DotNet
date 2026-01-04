/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的类型解析器,设置类型的映射表,主键,构造器等配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:09:28
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.Common;
using Obase.Core.Odm.Builder.ImplicitAssociationConfigor;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     默认的类型解析器
    /// </summary>
    public class DefaultTypeAnalyzer : ITypeAnalyzer
    {
        /// <summary>
        ///     存储从程序集解析类型过程中应忽略的类型
        /// </summary>
        private readonly HashSet<Type> _ignoredTypes;

        /// <summary>
        ///     建模器
        /// </summary>
        private readonly ModelBuilder _modelBuilder;

        /// <summary>
        ///     构造默认的类型解析器
        /// </summary>
        /// <param name="ignoredTypes">要忽略的类型</param>
        /// <param name="modelBuilder">对象数据模型建模器</param>
        /// <param name="next">管道中的下一节</param>
        public DefaultTypeAnalyzer(HashSet<Type> ignoredTypes, ModelBuilder modelBuilder, ITypeAnalyzer next)
        {
            Next = next;
            _ignoredTypes = ignoredTypes;
            _modelBuilder = modelBuilder;
        }

        /// <summary>
        ///     获取类型解析管道中的下一个解析器。
        /// </summary>
        public ITypeAnalyzer Next { get; }

        /// <summary>
        ///     配置指定的类型。
        /// </summary>
        /// <param name="type">要配置的类型。</param>
        /// <param name="configurator">该类型的配置器。</param>
        public void Configurate(Type type, IStructuralTypeConfigurator configurator)
        {
            //忽略的类型不参与推断
            if (_ignoredTypes.Contains(type))
                return;

            //对于实体和显式关联，如果存在protected internal的构造方法（java版参照处理），推断为构造器；
            //如果不存在，但存在无参构造方法，推断为构造器；如果也不存在，就用第一个。
            var constructors =
                type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            ConstructorInfo constructor = null;

            if (constructors.Length > 0)
            {
                var isFirst = true;

                foreach (var constructorInfo in constructors)
                {
                    //有参数的 不进行推断
                    if (constructorInfo.GetParameters().Length > 0)
                        continue;
                    //首个 暂存
                    if (isFirst)
                    {
                        constructor = constructorInfo;
                        isFirst = false;
                    }

                    //或者是public/internal/protect/protect internal
                    if ((constructorInfo.Attributes & MethodAttributes.Public) == MethodAttributes.Public ||
                        constructorInfo.IsAssembly || constructorInfo.IsFamilyAndAssembly ||
                        constructorInfo.IsFamilyOrAssembly)
                        constructor = constructorInfo;
                }
            }

            //能找到符合推断的
            if (constructor != null)
                //配置 应当是无参的 直接End
                configurator.HasConstructor(constructor, false).End();


            //继续按照具体类型配置
            //对象类型
            if (configurator is IObjectTypeConfigurator objectTypeConfigurator)
                Configurate(type, objectTypeConfigurator);
        }

        /// <summary>
        ///     配置指定的对象类型。
        /// </summary>
        /// <param name="type">要配置的对象类型。</param>
        /// <param name="configurator">该对象类型的配置器。</param>
        public void Configurate(Type type, IObjectTypeConfigurator configurator)
        {
            //忽略的类型不参与推断
            if (_ignoredTypes.Contains(type))
                return;

            //继续按照具体类型配置
            //关联型或者实体型
            switch (configurator)
            {
                case IEntityTypeConfigurator entityTypeConfigurator:
                    //将简单名称推断为表名
                    configurator.ToTable(type.Name, false);
                    //继续配置
                    Configurate(type, entityTypeConfigurator);
                    break;
                case IAssociationTypeConfigurator associationTypeConfigurator:
                {
                    //显式关联型
                    if (!typeof(ImplicitAssociation).IsAssignableFrom(associationTypeConfigurator.AssociationType))
                    {
                        //将简单名称推断为表名
                        configurator.ToTable(type.Name, false);
                    }
                    else
                    {
                        var ends = associationTypeConfigurator.AssociationEnds;
                        //先根据伴随端进行配置
                        var companionEndTargetTable = string.Empty;
                        //查找伴随端
                        var endConfig = ends.FirstOrDefault(p =>
                            p is AssociationEndConfiguration associationEndConfiguration &&
                            associationEndConfiguration.IsCompanionEnd);
                        if (endConfig != null) companionEndTargetTable = GetEntityTargetTable(endConfig.EntityType);
                        if (!string.IsNullOrEmpty(companionEndTargetTable))
                            configurator.ToTable(companionEndTargetTable, false);

                        //如果是两方关联 进行如下推断
                        //A和B有关联
                        //1. A 上 B 一对一 B上A 一对一 无法推断
                        //2. A 上 B 一对一 B上A 一对多 关联表设为A
                        //3. A 上 B 一对一 B上A 没有 关联表设为A
                        //4. A 上 B 一对多 B上A 一对多 关联表设为 A + Ass + B
                        //5. A 上 B 一对多 B上A 没有 关联表设为B
                        configurator.ToTable(
                            ends.Length == 2
                                ? GetTargetTable(ends)
                                //多方关联 直接推断为独立表
                                //MultiAss + 各个端Clr类型的名称
                                : $"MultiAss{string.Join("", ends.Select(p => p.EntityType.Name).ToArray())}",
                            false);
                    }

                    //继续配置
                    Configurate(type, associationTypeConfigurator);
                }
                    break;
                default:
                    throw new ArgumentException("未知的配置器类型", nameof(configurator));
            }
        }

        /// <summary>
        ///     配置指定的实体型。
        /// </summary>
        /// <param name="type">要配置的实体类。</param>
        /// <param name="configurator">该实体型的配置器。</param>
        public void Configurate(Type type, IEntityTypeConfigurator configurator)
        {
            //忽略的类型不参与推断
            if (_ignoredTypes.Contains(type))
                return;
            //推断标识属性
            if (Utils.ExistIdentity(type, out var ids))
                foreach (var id in ids)
                    configurator.HasKeyAttribute(id.Name, false);
        }

        /// <summary>
        ///     配置指定的关联型。
        /// </summary>
        /// <param name="type">要配置的关联型。</param>
        /// <param name="configurator">该关联型的配置器。</param>
        public void Configurate(Type type, IAssociationTypeConfigurator configurator)
        {
            //忽略的类型不参与推断
            if (_ignoredTypes.Contains(type))
                return;
            //配置为显式关联
            configurator.IsVisible(true, false);
        }

        /// <summary>
        ///     根据关联端上的引用推断表名
        /// </summary>
        /// <param name="ends">关联端集合</param>
        /// <returns></returns>
        private string GetTargetTable(IAssociationEndConfigurator[] ends)
        {
            //此刻 一定是两个端
            var end1 = ends[0];
            var end2 = ends[1];
            //分别获取当前端上另外一端的引用情况
            var end1Multi = GetEndMulti(end1, end2);
            var end2Multi = GetEndMulti(end2, end1);

            //End1上End2 的引用情况
            if (end1Multi == EEndMulti.None)
                //end1上没有End2的引用
                switch (end2Multi)
                {
                    case EEndMulti.None:
                        //都没有 可能是继承来的等原因 不处理即可
                        return null;
                    case EEndMulti.Single:
                        //end1上没有 end2上有一对一 设为end2的表
                        return GetEntityTargetTable(end2.EntityType);
                    case EEndMulti.Multi:
                        //end1上没有 end2上有一对多 设为end1的表
                        return GetEntityTargetTable(end1.EntityType);
                    default:
                        throw new ArgumentException($"未知的关联端引用类型{end2Multi}");
                }

            if (end1Multi == EEndMulti.Single)
                //end1上有End2的引用 一对一
                switch (end2Multi)
                {
                    case EEndMulti.None:
                        //end1上一对一 end2上没有 设为end1的表
                        return GetEntityTargetTable(end1.EntityType);
                    case EEndMulti.Single:
                        //end1上一对一 end2上有一对一 无法推断
                        return null;
                    case EEndMulti.Multi:
                        //end1上一对一 end2上有一对多 设为end1的表
                        return GetEntityTargetTable(end1.EntityType);
                    default:
                        throw new ArgumentException($"未知的关联端引用类型{end2Multi}");
                }

            if (end1Multi == EEndMulti.Multi)
                //end1上有End2的引用 一对多
                switch (end2Multi)
                {
                    case EEndMulti.None:
                        //end1上一对多 end2上没有 设为end2的表
                        return GetEntityTargetTable(end2.EntityType);
                    case EEndMulti.Single:
                        //end1上一对多 end2上有一对一 设为end2的表
                        return GetEntityTargetTable(end2.EntityType);
                    case EEndMulti.Multi:
                        //end1上一对多 end2上有一对多 设为独立关联表
                        return $"{end1.EntityType.Name}Ass{end2.EntityType.Name}";
                    default:
                        throw new ArgumentException($"未知的关联端引用类型{end2Multi}");
                }

            return string.Empty;
        }

        /// <summary>
        ///     获取某端上另外一端引用的多重性
        /// </summary>
        /// <param name="end1">当前端</param>
        /// <param name="end2">另外一端</param>
        /// <returns></returns>
        private EEndMulti GetEndMulti(IAssociationEndConfigurator end1, IAssociationEndConfigurator end2)
        {
            var properties = end1.EntityType.GetProperties();
            var ignoreList = _modelBuilder.FindConfiguration(end1.EntityType)?.IgnoreList;
            //检查每个属性
            foreach (var property in properties)
            {
                //过滤属性不参与关联多重性推断
                if (ignoreList != null && ignoreList.Contains(property.Name))
                    continue;

                var isMulti = Utils.GetIsMultiple(property, out var type);
                //找到另外一端的实体型类型
                if (type == end2.EntityType)
                    return isMulti ? EEndMulti.Multi : EEndMulti.Single;
            }

            return EEndMulti.None;
        }

        /// <summary>
        ///     获取实体型的映射表
        /// </summary>
        /// <param name="entityType">实体型</param>
        /// <returns></returns>
        private string GetEntityTargetTable(Type entityType)
        {
            //如果是实体型 返回其配置的表名
            var structuralTypeConfiguration = _modelBuilder.FindConfiguration(entityType);
            if (structuralTypeConfiguration is IEntityTypeConfigurator configurator)
                return configurator.TargetTable;
            return string.Empty;
        }
    }
}