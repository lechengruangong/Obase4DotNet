using Obase.AddonTest.Domain.Annotation;
using Obase.AddonTest.Domain.LogicDeletion;
using Obase.Core.Odm.Builder;
using Obase.LogicDeletion;
using Obase.Odm.Annotation;

namespace Obase.Test.Infrastructure.ModelRegister;

/// <summary>
///     插件的模型注册器
/// </summary>
public static class AddonModelRegister
{
    /// <summary>
    ///     注册方法
    /// </summary>
    /// <param name="modelBuilder">建模器</param>
    public static void Regist(ModelBuilder modelBuilder)
    {
        //首先使用Obase.Odm.Annotation的AssemblyAnalyzer分析器来分析做过标注的程序集
        //之前尝试使用过程序集的依赖关系来自动注册 但发现程序集是动态加载的 不能保证每次都可以注册 反而造成调用者困惑 故只保留了此处的方法用于标注解析
        modelBuilder.RegisterType(typeof(AnnotationJavaBeanWithCustomAttribute).Assembly, new AssemblyAnalyzer());

        //AnnotationJavaBean已标注 且无特殊属性 自定义表名 仅需要在标记上指明主键和主键是否自增 和 指定反序列化构造函数
        //此处不需要配置

        //AnnotationJavaBeanWithCustomAttribute类已标注 但有特殊的属性Strings需要手动的设值器和取值器
        var entity = modelBuilder.Entity<AnnotationJavaBeanWithCustomAttribute>();
        entity.Attribute(p => p.Strings)
            .HasValueGetter(
                model => model.Strings.Length > 0 ? string.Join(",", model.Strings) : "")
            .HasValueSetter<string>(
                (model, s) =>
                {
                    if (!string.IsNullOrEmpty(s)) model.Strings = s.Split(',');
                });
        //此处设置的表名 主键 是否自增 反序列化构造函数会被标注属性覆盖
        entity.ToTable("123");
        entity.HasKeyAttribute(p => p.Bool);
        entity.HasKeyIsSelfIncreased(true);

        //复杂类型AnnotationProvince AnnotationCity AnnotationRegion 和 实体型AnnotationDomesticAddress已标注 此处AnnotationCity AnnotationRegion因为使用了特定的连接符 故需要配置
        //如与AnnotationProvince一样使用默认连接符 此下三行配置不需要
        var domesticAdressConfig = modelBuilder.Entity<AnnotationDomesticAddress>();
        //对应的属性
        domesticAdressConfig.Attribute(p => p.City).HasMappingConnectionChar('_');
        domesticAdressConfig.Attribute(p => p.Region).HasMappingConnectionChar('-');

        //AnnotationSchool已标注 此处无需配置
        //AnnotationClass已标注 此处无需配置
        //AnnotationStudent已标注 此处无需配置
        //AnnotationTeacher已标注 此处无需配置
        //AnnotationClassTeacher已标注 此处无需配置

        //代码配置逻辑删除
        var logicDeletion = modelBuilder.Entity<AddonTest.Domain.LogicDeletion.LogicDeletion>();
        logicDeletion.HasKeyAttribute(p => p.IntNumber).HasKeyIsSelfIncreased(false);
        //创建逻辑删除扩展
        var logicDeletionExt =
            logicDeletion
                .HasExtension<LogicDeletionExtensionConfiguration<AddonTest.Domain.LogicDeletion.LogicDeletion>>();
        //当类中有定义逻辑删除字段时 指定为逻辑删除标记
        logicDeletionExt.HasDeletionMark(p => p.Bool);
        //映射字段与标记名相同 则不需要下一行HasDeletionField设置字段 当前逻辑删除标记Bool与字段Bool相同 故此行可以注释掉
        //logicDeletionExt.HasDeletionField("Bool");

        //LogicDeletionAnnotation已标注 此处无需配置

        //代码配置未定义字段的逻辑删除
        var logicDeletionNodef = modelBuilder.Entity<LogicDeletionNoDef>();
        logicDeletionNodef.HasKeyAttribute(p => p.IntNumber).HasKeyIsSelfIncreased(false);
        //创建逻辑删除扩展
        var logicDeletionNodefExt =
            logicDeletionNodef.HasExtension<LogicDeletionExtensionConfiguration<LogicDeletionNoDef>>();
        //当类中未定义逻辑删除字段时 仅需要指定为逻辑删除映射字段
        logicDeletionNodefExt.HasDeletionField("Bool");

        //LogicDeletionNoDefAnnonation已标注 此处无需配置
    }
}