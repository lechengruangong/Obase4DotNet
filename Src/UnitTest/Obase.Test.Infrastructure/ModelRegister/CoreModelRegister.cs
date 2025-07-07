using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Obase.Test.Domain.Association;
using Obase.Test.Domain.Association.DefaultAsNew;
using Obase.Test.Domain.Association.ExplicitlySelf;
using Obase.Test.Domain.Association.NoAssocationExtAttr;
using Obase.Test.Domain.Association.Self;
using Obase.Test.Domain.Functional.DependencyInjection;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.Infrastructure.ModelRegister;

/// <summary>
///     核心的模型注册器
/// </summary>
public static class CoreModelRegister
{
    /// <summary>
    ///     注册方法
    /// </summary>
    /// <param name="modelBuilder">建模器</param>
    public static void Regist(ModelBuilder modelBuilder)
    {
        //忽略项
        modelBuilder.Ignore<SmallJavaBeanLikeModel>();
        modelBuilder.Ignore<ServiceSa>();
        modelBuilder.Ignore<ServiceTa>();
        modelBuilder.Ignore<ServiceSb>();
        modelBuilder.Ignore<ServiceTb>();
        modelBuilder.Ignore<ServiceSc>();
        modelBuilder.Ignore<ServiceTc>();
        modelBuilder.Ignore<ServiceSd>();
        modelBuilder.Ignore<ServiceTd>();
        modelBuilder.Ignore<ServiceSe>();
        modelBuilder.Ignore<ServiceTe>();
        modelBuilder.Ignore<ServiceSf>();
        modelBuilder.Ignore<ServiceTf>();
        modelBuilder.Ignore<ServiceSg>();
        modelBuilder.Ignore<ServiceTg>();
        modelBuilder.Ignore<ServiceSh>();
        modelBuilder.Ignore<ServiceTh>();

        //符合推断的用程序集都注册方法注册
        modelBuilder.RegisterType(typeof(JavaBean).Assembly);

        //单独注册几个类型
        modelBuilder.RegisterType(typeof(School), typeof(Student));

        //对应测试CoreTest/SimpleTypeTest文件夹内NullableSimpleTypeEnumerableTest/SimpleTypeEnumerableTest/SimpleTypeWithConstructorArgsEnumerableTest

        #region 基础失血模型

        //失血模型 主键不符合推断 需要自定义属性
        var javaBeanLikeModelConfiguration = modelBuilder.Entity<JavaBean>();
        //主键 和 主键是否自增
        javaBeanLikeModelConfiguration.HasKeyAttribute(p => p.IntNumber).HasKeyIsSelfIncreased(false);
        //自定义的属性
        javaBeanLikeModelConfiguration.Attribute(p => p.Strings)
            .HasValueGetter(
                model => model.Strings.Length > 0 ? string.Join(",", model.Strings) : "")
            .HasValueSetter<string>(
                (model, s) =>
                {
                    if (!string.IsNullOrEmpty(s)) model.Strings = s.Split(',');
                })
            //设置为255长 超过255会令数据库建表类型变为Text
            .HasMaxcharNumber(255)
            //设置为不可空
            .HasNullable(false);
        //自定义精度 精度固定为(M,N) M即数据库decimal字段的最大值 MySql为65 SqlServer为38 Sqlite没有此概念 N即为HasPrecision设置的值 不能超过28
        javaBeanLikeModelConfiguration.Attribute(p => p.DecimalNumber).HasPrecision(5);

        //使用多个构造函数参数的失血模型 主键不符合推断 需要自定义属性 需要自定义构造函数
        var javaBeanWithConstructorArgsConfiguration = modelBuilder.Entity<JavaBeanWithConstructorArgs>();
        //主键 和 主键是否自增
        javaBeanWithConstructorArgsConfiguration.HasKeyAttribute(p => p.IntNumber).HasKeyIsSelfIncreased(false);
        //自定义的构造函数
        javaBeanWithConstructorArgsConfiguration
            //指定构造函数
            .HasConstructor(typeof(JavaBeanWithConstructorArgs).GetConstructor(
                new[]
                {
                    typeof(int),
                    typeof(long),
                    typeof(byte),
                    typeof(char),
                    typeof(float),
                    typeof(double),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(TimeSpan),
                    typeof(DateTime),
                    typeof(string),
                    typeof(bool),
                    typeof(string[])
                }))
            //指定构造函数参数 配置哪个参数为哪个字段
            .Map(p => p.IntNumber).Map(p => p.LongNumber).Map(p => p.ByteNumber)
            .Map(p => p.CharNumber).Map(p => p.FloatNumber).Map(p => p.DoubleNumber)
            .Map(p => p.DecimalNumber).Map(p => p.DateTime).Map(p => p.Time).Map(p => p.Date).Map(p => p.String)
            .Map(p => p.Bool)
            //第二个参数指定了转换函数
            .Map(p => p.Strings, o => ((string)o).Split(','))
            .End();
        //自定义的属性
        javaBeanWithConstructorArgsConfiguration.Attribute(p => p.Strings)
            .HasValueGetter(
                model => model.Strings.Length > 0 ? string.Join(",", model.Strings) : "");

        //可空值类型 主键不符合推断 
        var nullableJavaBeanConfiguration = modelBuilder.Entity<NullableJavaBean>();
        //配置主键
        nullableJavaBeanConfiguration.HasKeyAttribute(p => p.IntNumber).HasKeyIsSelfIncreased(false);

        #endregion

        //对应测试CoreTest/AssociationTest文件夹内AssociationQueryTest/AssociationUpdateAndDeleteTest/CompositePrimaryKeyTest/SelfAssociationTest/
        //AggregatedEndTest

        #region 基础关系模型

        //配置实体型
        //学校无需配置 符合推断
        //School

        //班级基本符合推断 只需要忽略Teachers
        modelBuilder.Entity<Class>().Ignore(p => p.Teachers);

        //学生 符合推断 无需配置
        //Student

        //学生信息 符合推断
        //StudentInfo

        //老师 符合推断 无需配置
        //Teacher

        //通行证 不符合推断 是个联合主键 且 没有无参构造函数
        var passPaperCfg = modelBuilder.Entity<PassPaper>();
        //联合主键
        passPaperCfg.HasKeyAttribute(p => p.TeacherId).HasKeyAttribute(p => p.Type).HasKeyIsSelfIncreased(false);
        //构造函数
        passPaperCfg
            .HasConstructor(typeof(PassPaper).GetConstructor(new[] { typeof(long), typeof(EPassPaperType) }))
            .Map(p => p.TeacherId).Map(p => p.Type).End();


        //区域 符合推断 无需配置
        //Area

        //配置关联型
        //班级->学校 关联 基本符合推断 无需配置

        //班级->学生 关联 基本符合推断 需要配置关联引用
        var classStudent = modelBuilder.Association();
        //配置Class端 映射符合推断 不需要配置映射
        var classStudentEnd1 = classStudent.AssociationEnd<Class>();
        //Class端的关联引用 配置特殊的取值器和设值器 延迟加载
        classStudentEnd1.AssociationReference(p => p.Students)
            .HasValueGetter(typeof(Class).GetField("_students", BindingFlags.NonPublic | BindingFlags.Instance))
            .HasValueSetter(typeof(Class).GetMethod("SetStudent", BindingFlags.Instance | BindingFlags.Public),
                EValueSettingMode.Appending);
        //配置Student端 符合推断 只需要配置延迟加载
        classStudent.AssociationEnd<Student>().AssociationReference(p => p.Class).HasEnableLazyLoading(true);

        //班级->老师 关联 不符合推断 没有无参构造函数 需要自定义属性
        var classAssTeacher = modelBuilder.Association<ClassTeacher>();
        //配置Class端 映射符合推断 不需要配置映射
        var classAssTeacherEnd1 = classAssTeacher.AssociationEnd(p => p.Class);
        //设置关联端延迟加载
        classAssTeacherEnd1.HasEnableLazyLoading(true);
        //设置关联引用延迟加载
        classAssTeacherEnd1.AssociationReference(p => p.ClassTeachers);
        //配置Teacher端 设置关联端延迟加
        classAssTeacher.AssociationEnd(p => p.Teacher).HasEnableLazyLoading(true);
        //特殊配置属性
        classAssTeacher.Attribute(p => p.Subject, typeof(string))
            .HasValueGetter(obj => string.Join(",", obj.Subject ?? new List<string>()))
            .HasValueSetter<string>((obj, v) => obj.Subject = v?.Split(',').ToList());
        //构造函数
        classAssTeacher
            .HasConstructor(typeof(ClassTeacher).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(long), typeof(long) }, null))
            .Map(p => p.ClassId).Map(p => p.TeacherId).End();
        //新实例构造函数
        classAssTeacher
            .HasNewInstanceConstructor(typeof(ClassTeacher).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(long), typeof(long), typeof(bool), typeof(bool), typeof(List<string>) }, null));

        //学生->学校 关联 符合推断 但需要配置延迟加载
        var studentSchool = modelBuilder.Association();
        //配置Student端 映射符合推断 不需要配置映射
        var studentSchoolEnd1 = studentSchool.AssociationEnd<Student>();
        //Student端的关联引用 配置延迟加载
        studentSchoolEnd1.AssociationReference(p => p.School).HasEnableLazyLoading(true);
        //配置School端 符合推断 无需配置
        studentSchool.AssociationEnd<School>();

        //学生->班级 关联 符合推断 在上文班级->学生中已配置

        //学生->学生信息 关联 无法推断关联表 需要设置关联表 StudentInfo端需要设置默认为新对象附加
        var studentStudentInfo = modelBuilder.Association();
        //Student端 符合推断 无需配置
        studentStudentInfo.AssociationEnd<Student>();
        //配置StudentInfo端 设置为伴随端 关联表即为StudentInfo的映射表
        var studentinfoEnd2 = studentStudentInfo.AssociationEnd<StudentInfo>().AsCompanion(true);
        //默认附加为新对象
        //具体的作用可以查看下面关联端是否作为新对象创建部分的说明
        studentinfoEnd2.HasDefaultAsNew(true);
        //并且设置此端为聚合的 默认为false
        //设置为ture 表示聚合的端对象在被聚合的关系解除或者关系的另外一端被删除是会被一并删除
        //当前关系中 即删除学生时和解除学生和学生信息关系时(如把学生中的学生信息置空或者替换,如果是一对多的从集合中移除) 学生信息也会被删除 
        studentinfoEnd2.IsAggregated(true);

        //老师->学校关联 需要配置关联引用
        var teacherSchool = modelBuilder.Association();
        //Teacher端 符合推断 无需配置 只需要配置关联引用的延迟加载
        teacherSchool.AssociationEnd<Teacher>().AssociationReference(p => p.School).HasEnableLazyLoading(true);
        //School端 符合推断 无需配置
        teacherSchool.AssociationEnd<School>();

        //教师->通行证关联
        var teacherPassPaper = modelBuilder.Association();
        //teacher端 符合推断 无需配置 只需要配置关联引用的延迟加载
        teacherPassPaper.AssociationEnd<Teacher>().AssociationReference(p => p.PassPaperList)
            .HasEnableLazyLoading(true);
        //PassPaper端 符合推断 无需配置 只需要配置关联引用的延迟加载
        teacherPassPaper.AssociationEnd<PassPaper>().AssociationReference(p => p.Teacher)
            .HasEnableLazyLoading(true);

        //学生->学生一卡通账户关联 符合推断 无需配置

        //区域的自关联
        var areaArea = modelBuilder.Association();
        //配置第一个Area端
        var areaEnd1 = areaArea.AssociationEnd<Area>();
        //配置映射
        areaEnd1.HasMapping("Code", "Code");
        //配置是否启用延迟加载
        areaEnd1.HasEnableLazyLoading(true);
        //配置关联引用和关联引用的延迟加载
        var subRef = areaEnd1.AssociationReference(p => p.ParentArea);
        subRef.HasEnableLazyLoading(true);
        //配置第二个Area端
        var areaEnd2 = areaArea.AssociationEnd<Area>();
        //配置映射
        areaEnd2.HasMapping("Code", "ParentCode");
        //配置是否启用延迟加载
        areaEnd2.HasEnableLazyLoading(true);
        //配置关联引用和关联引用的延迟加载
        var parentRef = areaEnd2.AssociationReference(p => p.SubAreas);
        parentRef.HasEnableLazyLoading(true);
        //映射表
        areaArea.ToTable("Area");

        //区域的显式自关联 友好区域
        var friendlyArea = modelBuilder.Association<FriendlyArea>();
        //配置第一个Area端
        var friendlyAreaEnd1 = friendlyArea.AssociationEnd(p => p.Area);
        //配置延迟加载 映射
        friendlyAreaEnd1.HasMapping("Code", "AreaCode").HasEnableLazyLoading(true);
        //配置关联引用 是否启用延迟加载 映射
        friendlyAreaEnd1.AssociationReference(p => p.FriendlyAreas).HasEnableLazyLoading(true);
        //配置第二个Area端 配置映射 是否启用延迟加载
        friendlyArea.AssociationEnd(p => p.Friend)
            .HasMapping("Code", "FriendlyAreaCode").HasEnableLazyLoading(true);
        //映射表
        friendlyArea.ToTable("FriendlyArea");

        //宾客的显式自关联
        //将宾客配置为实体型
        var guestEntity = modelBuilder.Entity<Guest>();
        //配置主键
        guestEntity.HasKeyAttribute(p => p.GuestId);

        //为宾客和宾客的朋友关系配置显式关联型
        var guestAssGuestAssociation = modelBuilder.Association<Friend>();
        //配置关联端 此处为自关联 两端都是Guest 所以只要配置每个端即可 无需根据类型进行判定 MySelf这一端在关联表中Friend的主键GuestIde映射为MySelfId
        var guestEnd1 = guestAssGuestAssociation.AssociationEnd(p => p.MySelf);
        guestEnd1.HasMapping("GuestId", "MySelfId");
        guestEnd1.AssociationReference(p => p.MyFriends);
        //配置关联端 此处为自关联 两端都是Guest 所以只要配置每个端即可 无需根据类型进行判定 FriendGuest这一端在关联表中Friend的主键GuestId映射为FriendFriendId
        var guestEnd2 = guestAssGuestAssociation.AssociationEnd(p => p.FriendGuest);
        guestEnd2.HasMapping("GuestId", "FriendId");
        guestEnd2.AssociationReference(p => p.FriendOfmes);

        #endregion

        //对应测试文件CoreTest/AssociationTest文件夹内DefaultAsNewTest

        #region 关联端是否作为新对象创建

        //关联端是否默认创建新对象配置控制如果某一个对象被创建出来后 未附加至上下文 但作为其他已附加对象的引用对象时 是否作为新对象附加至上下文
        //默认为不作为 因为对象往往是由应用层创建的 是否需要附加由应用层决定即可
        //但 如果某个对象的关联是无法通过此对象外部进行创建 如只能在构造函数内一起创建时 外部无法获取这个被一起创建的对象进行附加操作
        //就需要将关联端是否作为新对象创建设为true

        //默认作为不新对象创建的学校
        var defaultSchoolConfig = modelBuilder.Entity<DefaultSchool>();
        //配置主键
        defaultSchoolConfig.HasKeyAttribute(p => p.SchoolId).HasKeyIsSelfIncreased(true);
        //配置映射表
        defaultSchoolConfig.ToTable("School");

        //默认不作为新对象创建的学生
        var defaultStudentCfgConfiguration = modelBuilder.Entity<DefaultStudent>();
        //配置主键
        defaultStudentCfgConfiguration.HasKeyAttribute(p => p.StudentId).HasKeyIsSelfIncreased(true);
        //配置映射表
        defaultStudentCfgConfiguration.ToTable("Student");

        //默认不作为新对象创建的教师
        var defaultTeacherConfig = modelBuilder.Entity<DefaultTeacher>();
        //配置主键
        defaultTeacherConfig.HasKeyAttribute(p => p.TeacherId).HasKeyIsSelfIncreased(true);
        //配置映射表
        defaultTeacherConfig.ToTable("Teacher");

        //默认作为新对象创建的班级
        var defaultNewClassCfg = modelBuilder.Entity<DefaultNewClass>();
        //配置主键
        defaultNewClassCfg.HasKeyAttribute(p => p.ClassId).HasKeyIsSelfIncreased(true);
        //配置映射表
        defaultNewClassCfg.ToTable("Class");

        //默认作为新对象创建的班级->默认不作为新对象创建的学生关系
        var defaultnewClassAssociationStudent = modelBuilder.Association();
        //设置关联端 此端有引用 映射符合推断 
        defaultnewClassAssociationStudent.AssociationEnd<DefaultNewClass>()
            //配置引用
            .AssociationReference(p => p.Students);
        //设置关联端 此端没有引用 映射符合推断 配置默认作为新对象创建
        defaultnewClassAssociationStudent.AssociationEnd<DefaultStudent>().HasDefaultAsNew(true);

        //默认作为新对象创建的班级->默认不作为新对象创建的学校的关联
        var defaultnewClassAssociationSchool = modelBuilder.Association();
        //设置关联端 此端有引用 映射符合推断 
        defaultnewClassAssociationSchool.AssociationEnd<DefaultNewClass>()
            //配置引用
            .AssociationReference(p => p.School);
        //设置关联端 此端没有引用 映射符合推断 配置默认作为新对象创建
        defaultnewClassAssociationSchool.AssociationEnd<DefaultSchool>().HasDefaultAsNew(true);

        //默认作为新对象创建的任课教师关联
        var defaultnewClassAssociationClassTeacher = modelBuilder.Association<DefaultNewClassTeacher>();
        //配置关联端 此端有引用 映射符合推断 
        defaultnewClassAssociationClassTeacher.AssociationEnd(p => p.Class)
            //配置引用
            .AssociationReference(p => p.ClassTeachers);
        //设置关联端 此端没有引用 映射符合推断 配置默认作为新对象创建
        defaultnewClassAssociationClassTeacher.AssociationEnd(p => p.Teacher).HasDefaultAsNew(true);
        //设置关联表
        defaultnewClassAssociationClassTeacher.ToTable("ClassTeacher");

        //默认不作为新对象创建的班级
        var defaultClassCfg = modelBuilder.Entity<DefaultClass>();
        //配置主键
        defaultClassCfg.HasKeyAttribute(p => p.ClassId).HasKeyIsSelfIncreased(true);
        //配置映射表
        defaultClassCfg.ToTable("Class");

        //默认不作为新对象创建的班级->默认不作为新对象创建的学生关系
        var defaultClassAssociationStudent = modelBuilder.Association();
        //设置关联端 此端有引用 映射符合推断 
        defaultClassAssociationStudent.AssociationEnd<DefaultClass>()
            //配置引用
            .AssociationReference(p => p.Students);
        //设置关联端 此端没有引用 映射符合推断
        defaultClassAssociationStudent.AssociationEnd<DefaultStudent>();

        //默认不作为新对象创建的班级->默认不作为新对象创建的学校的关联
        var defaultClassAssociationSchool = modelBuilder.Association();
        //设置关联端 此端有引用 映射符合推断 
        defaultClassAssociationSchool.AssociationEnd<DefaultClass>()
            //配置引用
            .AssociationReference(p => p.School);
        //设置关联端 此端没有引用 映射符合推断 配置默认作为新对象创建
        defaultClassAssociationSchool.AssociationEnd<DefaultSchool>();

        //默认不作为新对象创建的任课教师关联型
        var defaultClassAssociationClassTeacher = modelBuilder.Association<DefaultClassTeacher>();
        //配置关联端 此端有引用 映射符合推断 
        defaultClassAssociationClassTeacher.AssociationEnd(p => p.Class)
            //配置引用
            .AssociationReference(p => p.ClassTeachers);
        //配置关联端 此端无引用 映射符合推断 
        defaultClassAssociationClassTeacher.AssociationEnd(p => p.Teacher);
        defaultClassAssociationClassTeacher.ToTable("ClassTeacher");

        #endregion

        //对应测试文件CoreTest/AssociationTest文件夹内NoAssocationAttrTest

        #region 无关联冗余属性的关联

        //关联冗余属性即对象上为关联定义的外键等属性
        //对于Obase 不定义这些属性也是可以支持的 只需要映射表内存在即可
        //当然 一般都会保留这些属性 用于查询优化 如A和B为一对多关联 在B上定义A的ID可以简单的检索所有与A有关联的B

        //无关联冗余属性的学校 不符合推断
        var noAttrShcoolCfg = modelBuilder.Entity<NoAssocationExtAttrSchool>();
        //配置主键
        noAttrShcoolCfg.HasKeyAttribute(p => p.SchoolId);
        //配置映射表
        noAttrShcoolCfg.ToTable("School");

        //无关联冗余属性的班级 不符合推断
        var noAttrClassCfg = modelBuilder.Entity<NoAssocationExtAttrClass>();
        //配置主键
        noAttrClassCfg.HasKeyAttribute(p => p.ClassId);
        //配置映射表
        noAttrClassCfg.ToTable("Class");

        //无关联冗余属性的学生 不符合推断
        var noAttrStudentCfg = modelBuilder.Entity<NoAssocationExtAttrStudent>();
        //配置主键
        noAttrStudentCfg.HasKeyAttribute(p => p.StudentId).HasKeyIsSelfIncreased(true);
        //配置映射表
        noAttrStudentCfg.ToTable("Student");

        //无关联冗余属性的老师
        var noAttrTeacherCfg = modelBuilder.Entity<NoAssocationExtAttrTeacher>();
        //配置主键
        noAttrTeacherCfg.HasKeyAttribute(p => p.TeacherId).HasKeyIsSelfIncreased(true);
        //配置映射表
        noAttrTeacherCfg.ToTable("Teacher");


        //无关联冗余属性的班级->学校 关联
        var noAttrSchoolSchoolClassAss = modelBuilder.Association();
        //配置无关联冗余属性的班级端
        noAttrSchoolSchoolClassAss.AssociationEnd<NoAssocationExtAttrClass>()
            //配置相应的关联引用和延迟加载
            .AssociationReference(p => p.School).HasEnableLazyLoading(true);
        //配置无关联冗余属性的学校端
        noAttrSchoolSchoolClassAss.AssociationEnd<NoAssocationExtAttrSchool>();

        //无关联冗余属性的班级->老师关联型
        var noAttrSchoolClassTeacherAss = modelBuilder.Association<NoAssocationExtAttrClassTeacher>();
        //配置无关联冗余属性的班级端
        noAttrSchoolClassTeacherAss.AssociationEnd(p => p.Class)
            //配置相应的关联引用和延迟加载
            .AssociationReference(p => p.ClassTeachers)
            .HasEnableLazyLoading(true);
        //配置无关联冗余属性的老师
        noAttrSchoolClassTeacherAss.AssociationEnd(p => p.Teacher);
        //配置特殊的属性
        noAttrSchoolClassTeacherAss.Attribute(p => p.Subject, typeof(string))
            .HasValueGetter(obj => string.Join(",", obj.Subject ?? new List<string>()))
            .HasValueSetter<string>((obj, v) => obj.Subject = v?.Split(',').ToList());
        //配置映射表
        noAttrSchoolClassTeacherAss.ToTable("ClassTeacher");

        //无关联冗余属性的学生->班级关联
        var noAttrStudentClassAss = modelBuilder.Association();
        noAttrStudentClassAss.AssociationEnd<NoAssocationExtAttrStudent>()
            .AssociationReference(p => p.Class);
        noAttrStudentClassAss.AssociationEnd<NoAssocationExtAttrClass>()
            .AssociationReference(p => p.Students).HasEnableLazyLoading(true);

        #endregion
    }
}