using System;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Obase.Test.Domain.Functional.Serialization;
using Obase.Test.Domain.SimpleType;
using Obase.Test.Infrastructure.ModelRegister;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     简单的ODM验证器,用于验证ODM的合法性
/// </summary>
public class SimpleOdmValidator : OdmValidator
{
    /// <summary>
    ///     使用指定的建模器创建对象数据模型
    /// </summary>
    /// <param name="modelBuilder">对象数据模型建造器</param>
    protected override void CreateModel(ModelBuilder modelBuilder)
    {
        CoreModelRegister.Regist(modelBuilder);
    }
}

/// <summary>
///     一个无法通过ODM完整性检查的验证器
///     只配置实体型 不配置主键
/// </summary>
public class InvalidOdmValidator : OdmValidator
{
    /// <summary>
    ///     使用指定的建模器创建对象数据模型
    /// </summary>
    /// <param name="modelBuilder">对象数据模型建造器</param>
    protected override void CreateModel(ModelBuilder modelBuilder)
    {
        //只配置实体型 不配置主键 完整性检查时会产生ODM的错误信息
        modelBuilder.Entity<NullableJavaBean>();
    }
}

/// <summary>
///     一个无法通过SODM完整性检查的验证器
///     配置的序列化实体型的构造器参数不全
/// </summary>
public class InvalidSodmValidator : OdmValidator
{
    /// <summary>
    ///     使用指定的建模器创建对象数据模型
    /// </summary>
    /// <param name="modelBuilder">对象数据模型建造器</param>
    protected override void CreateModel(ModelBuilder modelBuilder)
    {
        //配置一个序列化实体型
        var identityEntity = modelBuilder.SerializationEntity<Identity>();
        //配置序列化构造器 但只配置其中一个参数 完整性检查时会产生SODM的错误信息
        identityEntity.HasConstructor(typeof(Identity).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                [typeof(Guid), typeof(DateTime), typeof(string), typeof(DateTime), typeof(long)],
                null))
            .HasParameter(p => p.Id, typeof(Guid), true);
    }
}
