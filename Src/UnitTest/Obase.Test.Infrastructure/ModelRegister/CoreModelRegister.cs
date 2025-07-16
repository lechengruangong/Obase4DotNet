using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Obase.Test.Domain.Association;
using Obase.Test.Domain.Association.DefaultAsNew;
using Obase.Test.Domain.Association.ExplicitlyCompion;
using Obase.Test.Domain.Association.ExplicitlySelf;
using Obase.Test.Domain.Association.Implement;
using Obase.Test.Domain.Association.MultiAssociationEnd;
using Obase.Test.Domain.Association.MultiImplicitAssociationSearch;
using Obase.Test.Domain.Association.MultiplexAssociation;
using Obase.Test.Domain.Association.NoAssocationExtAttr;
using Obase.Test.Domain.Association.Self;
using Obase.Test.Domain.Functional;
using Obase.Test.Domain.Functional.DataError;
using Obase.Test.Domain.Functional.DependencyInjection;
using Obase.Test.Domain.SimpleType;
using Product = Obase.Test.Domain.Association.MultiImplicitAssociationSearch.Product;

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

        //对应测试文件CoreTest/AssociationTest文件夹内ExplicitlyCompionTest

        #region 显式关联伴随存储

        //对于显式关联型 通常都会使用独立映射表进行存储
        //但也是可以进行伴随存储的

        //配置汽车的实体型 符合推断 无需配置
        //Car

        //配置车轮实体型 符合推断 无需配置
        //Wheel

        //配置汽车车轮 显式关联型 默认是存储在类名的表内 也可以指定为伴随存储
        var carWheelAssociation = modelBuilder.Association<CarWheel>();
        //配置一个反持久化构造函数
        carWheelAssociation.HasConstructor(typeof(CarWheel).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null)).End();
        //配置Car端
        carWheelAssociation.AssociationEnd(p => p.Car);
        //配置Wheel端
        carWheelAssociation.AssociationEnd(p => p.Wheel);
        //没有独立映射表 和 Wheel存储在一起
        carWheelAssociation.ToTable("Wheel");

        #endregion

        //对应测试文件CoreTest/AssociationTest文件夹内MultiplexAssociationTest

        #region 两个类之间有多种关系

        //当两个类间有多种关系时 只需要分别配置关系即可
        //配置员工实体型 符合推断
        //Employee

        //配置办公室 不符合推断 配置主键
        modelBuilder.Entity<OfficeRoom>()
            .HasKeyAttribute(p => p.RoomCode).HasKeyIsSelfIncreased(false);

        //开启一个新的隐式关联配置
        var manageAssociationTypeConfiguration = modelBuilder.Association();
        //配置第一个端
        manageAssociationTypeConfiguration.AssociationEnd<Employee>()
            //对于当两个类间有多种关系时 必须配置关联引用以保证不同的引用使用不同的隐式关联型
            .AssociationReference(p => p.ManageRooms);
        //另外一个端
        manageAssociationTypeConfiguration.AssociationEnd<OfficeRoom>();
        //映射表
        manageAssociationTypeConfiguration.ToTable("ManageRoom");

        //开启一个新的隐式关联配置
        var workAssociationTypeConfiguration = modelBuilder.Association();
        //配置第一个端
        workAssociationTypeConfiguration.AssociationEnd<Employee>()
            //对于当两个类间有多种关系时 必须配置关联引用以保证不同的引用使用不同的隐式关联型
            .AssociationReference(p => p.WorkRoom);
        //另外一个端
        workAssociationTypeConfiguration.AssociationEnd<OfficeRoom>()
            .HasMapping("RoomCode", "WorkRoomCode");
        //映射表
        workAssociationTypeConfiguration.ToTable("Employee");

        #endregion

        //对应测试文件CoreTest/AssociationTest文件夹内ImplicitMultiSearchTest

        #region 隐式关联的显式化(隐式多对多的搜索优化)

        //多对多的隐式关联 必须使用独立映射表
        //如果需要根据一端的某个值筛选另外一端 或者 需要根据两端的属性来筛选
        //常规的查询需要从一段的映射表连接关联的独立映射表再连接要查询的一端
        //此时可以将关联型配置为显式关联型 查询时就可以从关联的独立映射表进行查询
        //并且可以在此关联型上定义需要的筛选属性 直接进行筛选无需查询其他端


        //将产品配置为实体型 符合推断 无需配置
        //Product

        //将分类配置为实体型 符合推断 无需配置
        //Category

        //配置显式化的隐式多对多关联型
        var implicitMultiAssociation = modelBuilder.Association<ProductCategory>();
        //配置产品关联端 在关联表中映射为主键Code->字段ProductCode 映射符合推断
        var productEnd = implicitMultiAssociation.AssociationEnd(p => p.Product);
        //配置分类关联端 在关联表中映射为主键CategoryId->字段CategoryId 映射符合推断
        var categoryEnd = implicitMultiAssociation.AssociationEnd(p => p.Category);
        //多对多 独立关联表 默认的关联表名会被推断为ProductAssCategory
        implicitMultiAssociation.ToTable("ProductCategory");

        // 如果在概念建模阶段就注意到需要此种查询 域类已将此关联设置为显示关联时 类内会直接定义关联引用为List<ProductCategory> 则此处不需要做此转换
        //此下的配置为领域模型未将关联引用显式化时的配置方式
        //配置关联引用 注意此处使用的配置方法 使用的是手动配置方法
        productEnd.AssociationReference("Categories", true)
            //配置取值器 即从对象中取值的方法 此处即为从关联型转换为List<Category>
            .HasValueGetter(new DelegateValueGetter<Product, List<ProductCategory>>(p =>
            {
                //这个配置一般不会被配置为延迟加载
                //但为了测试显式化的隐式关联型是否可以进行延迟加载 仍将此关联引用配置为延迟加载的

                if (p.Categories == null || p.Categories.Count == 0)
                    return null;
                var result = p.Categories.Select(q => new ProductCategory
                {
                    Category = q,
                    CategoryId = q.CategoryId,
                    CategoryName = q.Name,
                    Product = p,
                    ProductCode = p.Code
                }).ToList();
                return result;
            }))
            //配置设值器 即为对象设置值 此处即为从关联型转换为List<Category>
            .HasValueSetter<ProductCategory>((p, impValue) =>
            {
                if (impValue != null)
                {
                    //这个配置一般不会被配置为延迟加载
                    //但为了测试显式化的隐式关联型是否可以进行延迟加载 仍将此关联引用配置为延迟加载的
                    //所以使用了此行表示禁用了延迟加载 
                    p.Categories ??= new List<Category>();
                    //检查是否为自己的关联 以及去重
                    if (p.Code == impValue.ProductCode
                        && p.Categories.All(q => q != null && q.CategoryId != impValue.CategoryId))
                        p.Categories.Add(impValue.Category);
                }
            }, EValueSettingMode.Appending)
            //设置为可以延迟加载
            .HasEnableLazyLoading(true);

        //配置关联引用 注意此处使用的配置方法 使用的是手动配置方法
        categoryEnd.AssociationReference("Products", true)
            //配置取值器 即从对象中取值的方法 此处即为从关联型转换为List<Product>
            .HasValueGetter(new DelegateValueGetter<Category, List<ProductCategory>>(p =>
            {
                if (p.Products == null || p.Products.Count == 0)
                    return null;
                return p.Products.Select(q => new ProductCategory
                {
                    Category = p,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Name,
                    Product = q,
                    ProductCode = q.Code
                }).ToList();
            }))
            //配置设值器 即为对象设置值 此处即为从关联型转换为List<Product>
            .HasValueSetter<ProductCategory>((p, impValue) =>
            {
                if (impValue != null)
                {
                    p.Products ??= new List<Product>();
                    //检查是否为自己的关联 以及去重
                    if (p.CategoryId == impValue.CategoryId
                        && p.Products.All(q => q != null && q.Code != impValue.ProductCode))
                        p.Products.Add(impValue.Product);
                }
            }, EValueSettingMode.Appending);

        #endregion

        //对应测试文件CoreTest/AssociationTest文件夹内MultiAssociationEndTest

        #region 多方关联(多个关联端的关联)

        //当参与关联的关联方多于2个时 关联称之为多方关联
        //多方关联与一般的两方关联并无特殊的区别 都可以配置为显式或隐式
        //Obase支持将元组解析为隐式多方关联

        //将产品配置为实体型 符合推断
        //Domain.Multi.Product

        //将属性配置为实体型 符合推断
        //Property

        //将属性取值配置为实体型 符合推断
        //PropertyValue


        //多方关联 即关联型上有数个关联端
        //配置显式的多方关联的关联型
        //PropertyTakingValue
        //关联端 映射 是否延迟加载 都符合推断
        //第一个关联端 Product 此关联端在关联表PropertyTakingValue中的映射为Product的主键Code映射为ProductCode
        //第二个关联端 Property 此关联端在关联表PropertyTakingValue中的映射为Property的主键Code映射为PropertyCode
        //第三个关联端 PropertyValue 此关联端在关联表PropertyTakingValue中的映射为PropertyValue的主键Code映射为PropertyValueCode

        //配置隐式的多方关联的关联型
        //使用元组作为关联引用的类型 即可被解析为隐式的多方关联的关联型
        //关联端 映射 是否延迟加载 都符合推断
        //第一个关联端 Product 此关联端在关联表PropertyTakingValue中的映射为Product的主键Code映射为ProductCode
        //第二个关联端 Property 此关联端在关联表PropertyTakingValue中的映射为Property的主键Code映射为PropertyCode
        //第三个关联端 PropertyValue 此关联端在关联表PropertyTakingValue中的映射为PropertyValue的主键Code映射为PropertyValueCode
        var propertyValue = modelBuilder.Association();
        //关联端定义出来即可
        propertyValue.AssociationEnd<Domain.Association.MultiAssociationEnd.Product>()
            //设置关联引用延迟加载为true
            .AssociationReference(p => p.PropertyValues).HasEnableLazyLoading(true);
        propertyValue.AssociationEnd<Property>();
        propertyValue.AssociationEnd<PropertyValue>();
        //只需要设置一下关联表
        propertyValue.ToTable("PropertyTakingValue");

        #endregion

        //对应测试文件CoreTest/AssociationTest文件夹内ImplementTest

        #region 继承关系

        //定义一个自行车实体配置
        var bikeEntity = modelBuilder.Entity<Bike>();
        bikeEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);
        //此处需要配置类型判别器和根据哪个数据源字段的值来判断 不再需要配置自定义的构造器
        //具体配置见下方的BikeConcreteTypeDiscriminator
        bikeEntity.HasConcreteTypeDiscriminator(new BikeConcreteTypeDiscriminator(modelBuilder.ContextType), "Type");
        //Bike的Type字段是1 这里的类型需要根据具体的类型进行调整 
        //如果此基础类型是抽象的 此处可以配置一个如-1一类的值抽象的类型不会被创建 所以配置一个特殊值即可
        //字段名需要与基类类型的HasConcreteTypeDiscriminator方法的第二个参数相同
        bikeEntity.HasConcreteTypeSign("Type", 1);

        //定义车灯实体配置
        var bikeLightEntity = modelBuilder.Entity<BikeLight>();
        bikeLightEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);

        //定义车轮实体配置
        var bikeWheelEntity = modelBuilder.Entity<BikeWheel>();
        bikeWheelEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);

        //定义车旗实体配置
        var bikeFlagEntity = modelBuilder.Entity<BikeFlag>();
        bikeFlagEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);

        //定义车筐实体配置
        var bikeBucketEntity = modelBuilder.Entity<BikeBucket>();
        bikeBucketEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);

        //定义一个特定的我的自行车A
        var myBikeAEntity = modelBuilder.Entity<MyBikeA>();
        myBikeAEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);
        //设置继承关系
        myBikeAEntity.DeriveFrom<Bike>();
        //MyBikeA的Type字段是2 这里的类型需要根据具体的类型进行调整
        //字段名需要与基类类型的HasConcreteTypeDiscriminator方法的第二个参数相同
        myBikeAEntity.HasConcreteTypeSign("Type", 2);
        //设置A和C的具体类型区分器
        myBikeAEntity.HasConcreteTypeDiscriminator(new MyBikeConcreteTypeDiscriminator(modelBuilder.ContextType),
            "Type");
        //此处与父类一起保存于Bike
        myBikeAEntity.ToTable("Bike");

        //定义一个特定的我的自行车B
        var myBikeBEntity = modelBuilder.Entity<MyBikeB>();
        myBikeBEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);
        //设置继承关系
        myBikeBEntity.DeriveFrom<Bike>();
        //MyBikeB的Type字段是3 这里的类型需要根据具体的类型进行调整
        //字段名需要与基类类型的HasConcreteTypeDiscriminator方法的第二个参数相同
        myBikeBEntity.HasConcreteTypeSign("Type", 3);
        //此处与父类一起保存于Bike
        myBikeBEntity.ToTable("Bike");

        //定义一个特定的我的自行车C
        var myBikeCEntity = modelBuilder.Entity<MyBikeC>();
        myBikeCEntity.HasKeyAttribute(p => p.Code).HasKeyIsSelfIncreased(false);
        //设置继承关系
        myBikeCEntity.DeriveFrom<MyBikeA>();
        //MyBikeB的Type字段是4 这里的类型需要根据具体的类型进行调整
        //字段名需要与基类类型的HasConcreteTypeDiscriminator方法的第二个参数相同
        myBikeCEntity.HasConcreteTypeSign("Type", 4);
        //此处与父类一起保存于Bike
        myBikeCEntity.ToTable("Bike");

        //定义车灯的关联
        var bikeAssLight = modelBuilder.Association();
        //关联端 关联映射
        var bikeEnd1 = bikeAssLight.AssociationEnd<Bike>();
        //启用延迟加载
        bikeEnd1.AssociationReference(p => p.Light).HasEnableLazyLoading(true);
        bikeEnd1.HasMapping("Code", "Code");
        bikeAssLight.AssociationEnd<BikeLight>().HasMapping("Code", "LightCode");
        bikeAssLight.ToTable("Bike");

        //定义车轮的关联
        var bikeAssWheel = modelBuilder.Association();
        //关联端 关联映射
        var bikeEnd2 = bikeAssWheel.AssociationEnd<Bike>();
        //启用延迟加载
        bikeEnd2.AssociationReference(p => p.Wheels).HasEnableLazyLoading(true);
        bikeEnd2.HasMapping("Code", "BikeCode");
        bikeAssWheel.AssociationEnd<BikeWheel>().HasMapping("Code", "Code");
        bikeAssWheel.ToTable("BikeWheel");

        //定义车旗的关联
        var mybikeAssFlag = modelBuilder.Association();
        //关联端 关联映射
        var myBikeEnd1 = mybikeAssFlag.AssociationEnd<MyBikeA>();
        myBikeEnd1.AssociationReference(p => p.Flag).HasEnableLazyLoading(true);
        //启用延迟加载
        myBikeEnd1.HasMapping("Code", "Code");
        mybikeAssFlag.AssociationEnd<BikeFlag>().HasMapping("Code", "FlagCode");
        mybikeAssFlag.ToTable("Bike");

        //定义车筐的关联
        var mybikeAssBucket = modelBuilder.Association();
        //关联端 关联映射
        var myBikeEnd2 = mybikeAssBucket.AssociationEnd<MyBikeB>();
        myBikeEnd2.AssociationReference(p => p.Bucket).HasEnableLazyLoading(true);
        //启用延迟加载
        myBikeEnd2.HasMapping("Code", "Code");
        mybikeAssBucket.AssociationEnd<BikeBucket>().HasMapping("Code", "BucketCode");
        mybikeAssBucket.ToTable("Bike");

        //配置活动实体型
        var activityEntity = modelBuilder.Entity<Activity>();
        //配置主键
        activityEntity.HasKeyAttribute(p => p.Id);

        //奖品是抽象的 判别值设置一个特殊值即可 因为不会被实际创建出来 同时实现类配置DeriveFrom即可
        //为奖品配置实体型
        var prizeEntity = modelBuilder.Entity<Prize>();
        //配置主键
        prizeEntity.HasKeyAttribute(p => p.Id);
        //配置一个具体类型判别器 在判别器中返回具体的类型
        //实现见PrizeConcreteTypeDiscriminator中 此处类内没有定义Type Obase会其补充
        prizeEntity.HasConcreteTypeDiscriminator(new PrizeConcreteTypeDiscriminator(modelBuilder.ContextType), "Type");
        //此类型是抽象的 不会被创建 用一个特殊值即可
        prizeEntity.HasConcreteTypeSign("Type", -1);

        //为实体奖品配置实体型
        var inKindPrizeEntity = modelBuilder.Entity<InKindPrize>();
        //配置主键
        inKindPrizeEntity.HasKeyAttribute(p => p.Id);
        //配置为从Prize派生而来
        inKindPrizeEntity.DeriveFrom(typeof(Prize));
        //配置一个类型判别属性和值
        inKindPrizeEntity.HasConcreteTypeSign("Type", 1);
        //都存储在Prize里
        inKindPrizeEntity.ToTable("Prize");

        //为红包配置实体型
        var redEnvelopEntity = modelBuilder.Entity<RedEnvelope>();
        //配置主键
        redEnvelopEntity.HasKeyAttribute(p => p.Id);
        //配置为从Prize派生而来
        redEnvelopEntity.DeriveFrom(typeof(Prize));
        //配置类型判别器
        redEnvelopEntity.HasConcreteTypeDiscriminator(
            new RedEnvelopeConcreteTypeDiscriminator(modelBuilder.ContextType), "Type");
        //配置一个判别属性和值
        redEnvelopEntity.HasConcreteTypeSign("Type", 2);
        //都存储在Prize里
        redEnvelopEntity.ToTable("Prize");

        //为幸运红包配置实体型
        var luckRedEnvelopeEntity = modelBuilder.Entity<LuckyRedEnvelope>();
        //配置主键
        luckRedEnvelopeEntity.HasKeyAttribute(p => p.Id);
        //配置为从RedEnvelope派生而来
        luckRedEnvelopeEntity.DeriveFrom(typeof(RedEnvelope));
        //配置一个判别属性和值
        luckRedEnvelopeEntity.HasConcreteTypeSign("Type", 3);
        //都存储在Prize里
        luckRedEnvelopeEntity.ToTable("Prize");

        //配置关联型
        var activityAssPrize = modelBuilder.Association();
        //配置关联端 Activity关联端为End1这个属性 在关联表中Activity的主键Id映射为ActivityId 符合推断 无需配置
        activityAssPrize.AssociationEnd<Activity>();
        //配置关联端 Prize关联端为End2这个属性 在关联表中Prize的主键Id映射为Id 符合推断 无需配置
        activityAssPrize.AssociationEnd<Prize>();
        //关联表是Prize
        activityAssPrize.ToTable("Prize");

        #endregion

        //对应测试文件FunctionalTest文件夹内DataErrorTest

        #region 数据错误(关联引用是一对一 但数据是一对多)

        //DataErrorStudent 实体型
        var dataErrorStudent = modelBuilder.Entity<DataErrorStudent>();
        //主键 不是类名+id 不自增
        dataErrorStudent.HasKeyAttribute(p => p.StudentId).HasKeyIsSelfIncreased(false);

        //DataErrorStudentInfo实体型
        var dataErrorStudentInfo = modelBuilder.Entity<DataErrorStudentInfo>();
        //主键 不是类名+id 不自增
        dataErrorStudentInfo.HasKeyAttribute(p => p.StudentInfoId).HasKeyIsSelfIncreased(false);

        //DataErrorStudent和DataErrorStudentInfo间的关系
        var dataErrorAssociation = modelBuilder.Association();
        //关联端和映射
        dataErrorAssociation.AssociationEnd<DataErrorStudent>().HasMapping("StudentId", "StudentId");
        dataErrorAssociation.AssociationEnd<DataErrorStudentInfo>().HasMapping("StudentInfoId", "StudentInfoId");
        //根据测试需要 配置成关联表是DataErrorStudentInfo
        dataErrorAssociation.ToTable("DataErrorStudentInfo");

        #endregion

        //对应测试文件FunctionalTest文件夹内ComplexAttributeTest

        #region 复杂类型

        //国内地址 配置为实体型
        var domesticAdressConfig = modelBuilder.Entity<DomesticAddress>();
        domesticAdressConfig.HasKeyAttribute(p => p.Key);

        //复杂类型 符合推断 无需注册
        //City
        //Province
        //Region

        //对应的属性
        domesticAdressConfig.Attribute(p => p.City).HasMappingConnectionChar('_');
        domesticAdressConfig.Attribute(p => p.Region).HasMappingConnectionChar('-');

        #endregion

        //对应测试文件FunctionalTest文件夹内SimpleAttributeConcurrentConflictTest

        #region 并发策略 简单属性

        //并发策略适用于对象创建和修改时出现并发的情况
        //Obase将并发冲突分为三种 重复创建 版本冲突 更新幻影
        //重复创建 即尝试创建主键相同的对象
        //版本冲突 在配置了版本键的情况下 修改对象时版本键已被其他线程/进程修改
        //更新幻影 修改对象时对象已被其他线程/进程删除
        //要配置并发策略 需要在实体型上配置

        //配置实体型
        var ingoreConflictConfig = modelBuilder.Entity<IgnoreKeyValue>();
        //配置键属性
        ingoreConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 忽略
        //忽略策略 当发生并发时 不做任何处理
        ingoreConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy.Ignore);
        //配置版本键 用于检测修改时的并发冲突 对于忽略策略 可以不配置版本键
        ingoreConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        ingoreConflictConfig.ToTable("KeyValues");

        //配置实体型
        var throwExceptionConflictConfig = modelBuilder.Entity<ThrowExceptionKeyValue>();
        //配置键属性
        throwExceptionConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 抛出异常
        //抛出异常策略 当发生并发异常 会抛出特定的异常
        //分别是
        //NothingUpdatedException 未更新任何记录
        //RepeatInsertionException 重复插入记录
        //默认的处理策略即为抛出异常 故使用此种策略时可以不配置
        throwExceptionConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy
            .ThrowException);
        //配置版本键 用于检测修改时的并发冲突 对于抛出异常策略 可以不配置版本键
        throwExceptionConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        throwExceptionConflictConfig.ToTable("KeyValues");

        //配置实体型
        var overWriteConflictConfig = modelBuilder.Entity<OverwriteKeyValue>();
        //配置键属性
        overWriteConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 强制覆盖
        //强制覆盖策略可以处理重复创建 和 版本冲突 两种并发情况
        //强制覆盖策略 当发生并发时 用当前对象覆盖原有对象
        overWriteConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy.Overwrite);
        //配置版本键 用于检测修改时的并发冲突 要想处理版本冲突并发 必须配置版本键
        //版本键可以配置多个
        //可以使用会发生并发冲突的属性 或者使用 时间戳标识最后的修改时间 来作为版本键
        //能区分对象最后被谁修改的属性都可以作为版本键
        overWriteConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        overWriteConflictConfig.ToTable("KeyValues");

        //配置实体型
        var reconstructConflictConfig = modelBuilder.Entity<ReconstructKeyValue>();
        //配置键属性
        reconstructConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 重建对象
        //重建对象策略可以处理更新幻影 这种并发情况
        //重建对象策略 当发生异常时 将当前对象做为新对象进行创建
        reconstructConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy
            .Reconstruct);
        //配置版本键 用于检测修改时的并发冲突 对于重建对象策略 可以不配置版本键
        reconstructConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        reconstructConflictConfig.ToTable("KeyValues");

        //配置实体型
        var accumulateCombineConfig = modelBuilder.Entity<AccumulateCombineKeyValue>();
        //配置键属性
        accumulateCombineConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 版本合并
        //版本合并策略可以处理重复创建和版本冲突 这两种并发情况
        //版本合并策略 当发生异常时 将当前对象与旧对象的属性进行合并
        accumulateCombineConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy.Combine);
        //配置版本键 用于检测修改时的并发冲突 要想处理版本冲突并发 必须配置版本键
        //版本键可以配置多个
        //可以使用会发生并发冲突的属性 或者使用 时间戳标识最后的修改时间 来作为版本键
        //能区分对象最后被谁修改的属性都可以作为版本键
        accumulateCombineConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        accumulateCombineConfig.ToTable("KeyValues");
        //配置要进行合并的属性的合并策略
        accumulateCombineConfig.Attribute(p => p.Value)
            //设置为累加 即将当前版本中属性值的增量累加到对方版本 只支持数值型的属性
            .HasCombinationHandler(EAttributeCombinationHandlingStrategy.Accumulate);

        //配置实体型
        var ignoreCombineConfig = modelBuilder.Entity<IgnoreCombineKeyValue>();
        //配置键属性
        ignoreCombineConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 版本合并
        //版本合并策略可以处理重复创建和版本冲突 这两种并发情况
        //版本合并策略 当发生异常时 将当前对象与旧对象的属性进行合并
        ignoreCombineConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy.Combine);
        //配置版本键 用于检测修改时的并发冲突 要想处理版本冲突并发 必须配置版本键
        //版本键可以配置多个
        //可以使用会发生并发冲突的属性 或者使用 时间戳标识最后的修改时间 来作为版本键
        //能区分对象最后被谁修改的属性都可以作为版本键
        ignoreCombineConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        ignoreCombineConfig.ToTable("KeyValues");
        //配置要进行合并的属性的合并策略
        ignoreCombineConfig.Attribute(p => p.Value)
            //设置为忽略 即使用旧对象(即原有对象)的值
            .HasCombinationHandler(EAttributeCombinationHandlingStrategy.Ignore);

        //配置实体型
        var overwriteCombineConfig = modelBuilder.Entity<OverwriteCombineKeyValue>();
        //配置键属性
        overwriteCombineConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 版本合并
        //版本合并策略可以处理重复创建和版本冲突 这两种并发情况
        //版本合并策略 当发生异常时 将当前对象与旧对象的属性进行合并
        overwriteCombineConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy.Combine);
        //配置版本键 用于检测修改时的并发冲突 要想处理版本冲突并发 必须配置版本键
        //版本键可以配置多个
        //可以使用会发生并发冲突的属性 或者使用 时间戳标识最后的修改时间 来作为版本键
        //能区分对象最后被谁修改的属性都可以作为版本键
        overwriteCombineConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        overwriteCombineConfig.ToTable("KeyValues");
        //配置要进行合并的属性的合并策略
        overwriteCombineConfig.Attribute(p => p.Value)
            //设置为覆盖 即使用新对象(即当前)的值 此种策略为默认的策略 可以不配置
            .HasCombinationHandler(EAttributeCombinationHandlingStrategy.Overwrite);

        #endregion

        //对应测试文件FunctionalTest文件夹内ComplexAttributeConcurrentConflictTest

        #region 并发策略 复杂类型属性

        //复杂类型属性的并发策略与简单属性的并发策略基本相同
        //唯一不同的是使用版本合并策略时 具体的属性合并策略需要配置在复杂类型的属性上

        //复杂类型ComplexKeyValue 符合推断 无需配置
        //ComplexKeyValue

        //配置实体型
        var complexIngoreConflictConfig = modelBuilder.Entity<ComplexIgnoreKeyValue>();
        //配置键属性
        complexIngoreConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 忽略
        //忽略策略 当发生并发时 不做任何处理
        //使用忽略策略时 不需要对复杂类型属性进行配置
        complexIngoreConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy.Ignore);
        //配置版本键 用于检测修改时的并发冲突 对于忽略策略 可以不配置版本键
        complexIngoreConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        complexIngoreConflictConfig.ToTable("KeyValues");

        //配置实体型
        var complexThrowExceptionConflictConfig = modelBuilder.Entity<ComplexThrowExceptionKeyValue>();
        //配置键属性
        complexThrowExceptionConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 抛出异常
        //抛出异常策略 当发生并发异常 会抛出特定的异常
        //分别是
        //NothingUpdatedException 未更新任何记录
        //RepeatInsertionException 重复插入记录
        //默认的处理策略即为抛出异常 故使用此种策略时可以不配置
        //使用抛出异常策略时 不需要对复杂类型属性进行配置
        complexThrowExceptionConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy
            .ThrowException);
        //配置版本键 用于检测修改时的并发冲突 对于抛出异常策略 可以不配置版本键
        complexThrowExceptionConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        complexThrowExceptionConflictConfig.ToTable("KeyValues");

        //配置实体型
        var complexOverWriteConflictConfig = modelBuilder.Entity<ComplexOverwriteKeyValue>();
        //配置键属性
        complexOverWriteConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 强制覆盖
        //强制覆盖策略可以处理重复创建 和 版本冲突 两种并发情况
        //强制覆盖策略 当发生并发时 用当前对象覆盖原有对象
        complexOverWriteConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy
            .Overwrite);
        //配置并发处理策略为 强制覆盖
        //强制覆盖策略可以处理重复创建 和 版本冲突 两种并发情况
        //强制覆盖策略 当发生并发时 用当前对象覆盖原有对象
        complexOverWriteConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        complexOverWriteConflictConfig.ToTable("KeyValues");

        //配置实体型
        var complexReconstructConflictConfig = modelBuilder.Entity<ComplexReconstructKeyValue>();
        //配置键属性
        complexReconstructConflictConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 重建对象
        //强制覆盖策略可以处理更新幻影 这种并发情况
        //强制覆盖策略 当发生异常时 将当前对象做为新对象进行创建
        complexReconstructConflictConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy
            .Reconstruct);
        //配置版本键 用于检测修改时的并发冲突 对于重建对象策略 可以不配置版本键
        complexReconstructConflictConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        complexReconstructConflictConfig.ToTable("KeyValues");

        //配置累加合并策略复杂类型
        var accumulateComplexCombieConfig = modelBuilder.Complex<AccumulateCombineComplexKeyValue>();
        //配置具体要合并的属性
        accumulateComplexCombieConfig
            .Attribute("Value", typeof(int))
            .HasValueGetter(p => p.Value)
            .HasValueSetter(p => p.Value)
            //设置为累加 即将当前版本中属性值的增量累加到对方版本 只支持数值型的属性
            .HasCombinationHandler(EAttributeCombinationHandlingStrategy.Accumulate);

        //配置实体型
        var complexAccumulateCombineConfig = modelBuilder.Entity<ComplexAccumulateCombineKeyValue>();
        //配置键属性
        complexAccumulateCombineConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 版本合并
        //版本合并策略可以处理重复创建和版本冲突 这两种并发情况
        //版本合并策略 当发生异常时 将当前对象与旧对象的属性进行合并
        complexAccumulateCombineConfig.HasConcurrentConflictHandlingStrategy(
            EConcurrentConflictHandlingStrategy.Combine);
        //配置版本键 用于检测修改时的并发冲突 要想处理版本冲突并发 必须配置版本键
        //版本键可以配置多个
        //可以使用会发生并发冲突的属性 或者使用 时间戳标识最后的修改时间 来作为版本键
        //能区分对象最后被谁修改的属性都可以作为版本键
        complexAccumulateCombineConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        complexAccumulateCombineConfig.ToTable("KeyValues");

        //配置忽略合并策略复杂类型
        var ignoreComplexCombieConfig = modelBuilder.Complex<IgnoreCombineComplexKeyValue>();
        //配置具体要合并的属性
        ignoreComplexCombieConfig
            .Attribute("Value", typeof(int))
            .HasValueGetter(p => p.Value)
            .HasValueSetter(p => p.Value)
            //设置为忽略 即使用旧对象(即原有对象)的值
            .HasCombinationHandler(EAttributeCombinationHandlingStrategy.Ignore);

        //配置实体型
        var complexIgnoreCombineConfig = modelBuilder.Entity<ComplexIgnoreCombineKeyValue>();
        //配置键属性
        complexIgnoreCombineConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 版本合并
        //版本合并策略可以处理重复创建和版本冲突 这两种并发情况
        //版本合并策略 当发生异常时 将当前对象与旧对象的属性进行合并
        complexIgnoreCombineConfig.HasConcurrentConflictHandlingStrategy(
            EConcurrentConflictHandlingStrategy.Combine);
        //配置版本键 用于检测修改时的并发冲突 要想处理版本冲突并发 必须配置版本键
        //版本键可以配置多个
        //可以使用会发生并发冲突的属性 或者使用 时间戳标识最后的修改时间 来作为版本键
        //能区分对象最后被谁修改的属性都可以作为版本键
        complexIgnoreCombineConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        complexIgnoreCombineConfig.ToTable("KeyValues");

        //配置覆盖合并策略复杂类型
        var overWriteComplexConbineConfiguration = modelBuilder.Complex<OverWriteCombineComplexKeyValue>();
        //配置具体要合并的属性
        overWriteComplexConbineConfiguration
            .Attribute("Value", typeof(int))
            .HasValueGetter(p => p.Value)
            .HasValueSetter(p => p.Value)
            //设置为覆盖 即使用新对象(即当前)的值 此种策略为默认的策略 可以不配置
            .HasCombinationHandler(EAttributeCombinationHandlingStrategy.Overwrite);

        //配置实体型
        var complexOverWriteCombineConfig = modelBuilder.Entity<ComplexOverwriteCombineKeyValue>();
        //配置键属性
        complexOverWriteCombineConfig.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(false);
        //配置并发处理策略为 版本合并
        //版本合并策略可以处理重复创建和版本冲突 这两种并发情况
        //版本合并策略 当发生异常时 将当前对象与旧对象的属性进行合并
        complexOverWriteCombineConfig.HasConcurrentConflictHandlingStrategy(EConcurrentConflictHandlingStrategy
            .Combine);
        //配置版本键 用于检测修改时的并发冲突 要想处理版本冲突并发 必须配置版本键
        //版本键可以配置多个
        //可以使用会发生并发冲突的属性 或者使用 时间戳标识最后的修改时间 来作为版本键
        //能区分对象最后被谁修改的属性都可以作为版本键
        complexOverWriteCombineConfig.HasVersionAttribute(p => p.VersionKey);
        //配置映射表
        complexOverWriteCombineConfig.ToTable("KeyValues");

        #endregion

        //对应测试文件FunctionalTest文件夹内EntityNoticeTest

        #region 实体通知

        //配置实体型NoticeSutdentInfo 不符合推断
        var noticeEntityConfig = modelBuilder.Entity<NoticeSutdentInfo>();
        //配置主键
        noticeEntityConfig.HasKeyAttribute(p => p.StudentId).HasKeyIsSelfIncreased(false);

        //配置要进行通知的属性 这些属性即此实体型的属性 当发生特定的行为时 这些属性的值会包含在通知消息内
        //noticeEntityConfig.HasNoticeAttributes(new List<string> { "Description", "Background" });
        //无参的方法则表示通知所有的属性
        noticeEntityConfig.HasNoticeAttributes();
        //指示是否在对象被创建时进行通知
        noticeEntityConfig.HasNotifyCreation(true);
        //指示是否在对象被删除时进行通知
        noticeEntityConfig.HasNotifyDeletion(true);
        //指示是否在对象被修改时进行通知
        noticeEntityConfig.HasNotifyUpdate(true);

        #endregion
    }
}