using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     关联查询测试
///     测试Select GroupBy Include等方法
/// </summary>
[TestFixture]
public class AssociationQueryTest
{
    /// <summary>
    ///     构造实例 装载初始对象
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<Student>().Delete(p => p.StudentId > 0);
            context.CreateSet<School>().Delete(p => p.SchoolId > 0);
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);
            context.CreateSet<ClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);
            context.CreateSet<StudentInfo>().Delete(p => p.StudentInfoId > 0);
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);

            //加入测试学校
            var newschool = new School
            {
                Createtime = DateTime.Now,
                EstablishmentTime = DateTime.Parse("1999-12-31 23:59:59"),
                IsPrime = false,
                Name = "第X某某学校",
                SchoolType = (ESchoolType)new Random((int)DateTime.Now.Ticks).Next(3)
            };
            //学校的班级
            var newclass = new Class
            {
                Name = "某某班",
                School = newschool
            };

            context.Attach(newschool);
            context.Attach(newclass);

            //加入学生
            for (var i = 1; i < 6; i++)
            {
                var student = new Student
                {
                    Class = newclass,
                    Name = $"小{i}",
                    School = newschool
                };
                context.Attach(student);
            }

            //加入教师
            //一对多
            var teacher = new Teacher { Name = "某老师", School = newschool };
            var clasTeacher = new ClassTeacher(newclass, teacher)
            {
                IsManage = true,
                IsSubstitute = false,
                Subject = ["语文", "数学", "化学"]
            };

            newclass.SetTeacher(clasTeacher);
            context.Attach(teacher);
            context.Attach(clasTeacher);

            //保存
            context.SaveChanges();

            //为学生加入学生信息 学生信息没有引用学生 只能靠StudentId关联 所以此处需要先保存学生获取ID
            var studentList = context.CreateSet<Student>().ToList();
            foreach (var student in studentList)
                context.Attach(new StudentInfo
                {
                    StudentId = student.StudentId,
                    Background = "普通",
                    Description = "普普通通"
                });

            context.SaveChanges();
        }
    }

    /// <summary>
    ///     销毁
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<Student>().Delete(p => p.StudentId > 0);
            context.CreateSet<School>().Delete(p => p.SchoolId > 0);
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);
            context.CreateSet<ClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);
            context.CreateSet<StudentInfo>().Delete(p => p.StudentId > 0);
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     测试关系分组
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void GroupByTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //按照属性分组 并投影成List<匿名类>
        var result1 = context.CreateSet<Student>()
            .GroupBy(s => s.ClassId, s => s.StudentId,
                (cid, sids) => new { Id = cid, count = sids.Count() }).ToList();
        //可以投影到匿名类
        Assert.That(result1, Is.Not.Null);

        //按照属性分组为IGroupingBy<>对象
        var cla = context.CreateSet<Class>().FirstOrDefault();
        var result2 = context.CreateSet<Student>().GroupBy(s => s.ClassId, s => s.StudentId).ToList();

        //有一个班级
        Assert.That(result2.Count, Is.EqualTo(1));
        Assert.That(cla, Is.Not.Null);
        //分组的Key是班级ID
        Assert.That(result2[0].Key, Is.EqualTo(cla.ClassId));
        //分组的元素个数是5
        Assert.That(result2[0].ToList().Count, Is.EqualTo(5));
        //按照属性分组 并投影成某个类
        var result3 = context.CreateSet<Student>()
            .GroupBy(s => s.ClassId, s => s.StudentId,
                (cid, sids) => new SimpleGroup { Id = cid, Count = sids.Max() }).ToList();
        //可以投影
        Assert.That(result3, Is.Not.Null);

        //关联属性投影
        var result4 = context.CreateSet<Student>().GroupBy(p => p.School.Name, p => p.Name).ToList();
        //投影后是5个
        Assert.That(result4.Count, Is.EqualTo(5));

        //此处投影到匿名对象和非注册对象 PGSQL暂时有问题 以后修改
        if (dataSource != EDataSource.PostgreSql)
        {
            //投影到一个不属于模型的类型
            var result5 = context.CreateSet<Student>()
                .Select(s => new SimpleStu { StudentName = s.Name, Id = s.StudentId })
                .GroupBy(p => p.Id, p => p.StudentName).ToList();
            //投影后是5个
            Assert.That(result5.Count, Is.EqualTo(5));
        }
    }

    /// <summary>
    ///     测试强制包含方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void InculdeTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //测试字符串的Include 隐式关联型
        var cla09 = context.CreateSet<Class>().Where(p => p.Name != "").Include("Students.School").ToList();
        var te1 = cla09[0].Students[0].School;
        //可以获取到学校
        Assert.That(te1, Is.Not.Null);
        //显式关联型
        var cla9 = context.CreateSet<Class>().Where(p => p.Name != "").Include("ClassTeachers.Teacher").ToList();
        var te9 = cla9[0].ClassTeachers[0].Teacher;
        //可以获取到教师
        Assert.That(te9, Is.Not.Null);

        //测试没有条件直接包含关联引用
        context = ContextUtils.CreateContext(dataSource);

        var cla1 = context.CreateSet<Class>().Include(p => p.School).FirstOrDefault();
        //可以获取到学校
        Assert.That(cla1, Is.Not.Null);
        Assert.That(cla1.School, Is.Not.Null);

        //测试有条件包含关联引用
        cla1 = context.CreateSet<Class>().Include(p => p.School).FirstOrDefault(p => p.Name != "");
        //可以获取到学校
        Assert.That(cla1, Is.Not.Null);
        Assert.That(cla1.School, Is.Not.Null);

        context = ContextUtils.CreateContext(dataSource);
        //多重性元素阻断关联路径表达的问题 使用Select
        var cla0 = context.CreateSet<Class>().Where(p => p.Name != "").Include(p => p.Students.Select(q => q.School))
            .ToList();
        var te = cla0[0].Students[0].School;
        //可以获取到学校
        Assert.That(te, Is.Not.Null);

        //多重性元素阻断关联路径表达的问题 使用Select
        var cla01 = context.CreateSet<Class>().Where(p => p.Name != "")
            .Include(p => p.ClassTeachers.Select(q => q.Teacher.School)).ToList();
        //可以获取到学校
        Assert.That(cla01[0].Teachers[0].School, Is.Not.Null);

        context = ContextUtils.CreateContext(dataSource);
        //分别包含任课教师和学生
        var cla = context.CreateSet<Class>().Where(p => p.Name != "").Include(p => p.Students)
            .Include(p => p.ClassTeachers)
            .FirstOrDefault();
        //可以获取到任课教师和学生
        Assert.That(cla, Is.Not.Null);
        Assert.That(cla.Teachers, Is.Not.Null);
        Assert.That(cla.Teachers.Count, Is.EqualTo(1));
        Assert.That(cla.Students, Is.Not.Null);
        Assert.That(cla.Students.Count, Is.EqualTo(5));

        context = ContextUtils.CreateContext(dataSource);
        //执行强制包含 将非延迟加载的关联引用放入对象中
        var stu = context.CreateSet<Student>().Where(p => p.Name != "").Include(p => p.Class.School)
            .Include(p => p.StudentInfo)
            .FirstOrDefault();
        //可以获取到班级 学校 学生信息
        Assert.That(stu, Is.Not.Null);
        Assert.That(stu.Class, Is.Not.Null);
        Assert.That(stu.Class.School, Is.Not.Null);
        //延迟加载的学校
        Assert.That(stu.School, Is.Not.Null);

        context = ContextUtils.CreateContext(dataSource);
        //测试空查询 只有一个包含操作
        var classes = context.CreateSet<Class>().Include(p => p.ClassTeachers).ToList();
        //可以获取到任课教师
        Assert.That(classes.FirstOrDefault(), Is.Not.Null);
        Assert.That(classes[0].ClassTeachers, Is.Not.Null);

        context = ContextUtils.CreateContext(dataSource);
        //测试连续Include后无其他Op
        classes = context.CreateSet<Class>().Include(p => p.ClassTeachers).Include(p => p.Students)
            .Include(p => p.School)
            .ToList();
        //可以获取到任课教师 学生 学校
        Assert.That(classes.FirstOrDefault(), Is.Not.Null);
        Assert.That(classes[0].Students, Is.Not.Null);
        Assert.That(classes[0].ClassTeachers, Is.Not.Null);
        Assert.That(classes[0].School, Is.Not.Null);

        //测试错误的Include
        //Name不是引用元素
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            //测试Name不是引用元素
            var list = context.CreateSet<Class>().Include(p => p.Name).ToList();
            Assert.That(list, Is.Not.Null);
        });
        //校验
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo("包含路径错误,找不到为Name的引用元素."));

        //测试School是引用元素但Createtime不是
        ex = Assert.Throws<ArgumentException>(() =>
        {
            var list = context.CreateSet<Class>().Include(p => p.School.Createtime).ToList();
            Assert.That(list, Is.Not.Null);
        });
        //校验
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo("包含路径错误,找不到为Createtime的引用元素."));

        //Name不是引用元素
        ex = Assert.Throws<ArgumentException>(() =>
        {
            var list = context.CreateSet<Class>().Include("Name").ToList();
            Assert.That(list, Is.Not.Null);
        });
        //校验
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo("包含路径错误,找不到为Name的引用元素."));

        //测试根本没有的元素
        ex = Assert.Throws<ArgumentException>(() =>
        {
            var list = context.CreateSet<Class>().Include("123").ToList();
            Assert.That(list, Is.Not.Null);
        });
        //校验
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message,
            Is.EqualTo("无法从Obase.Test.Domain.Association.Class中获取属性123,请检查Include的参数. (Parameter 'sourceType')"));

        //测试School是引用元素但根本没有456元素
        ex = Assert.Throws<ArgumentException>(() =>
        {
            var list = context.CreateSet<Class>().Include("School.456").ToList();
            Assert.That(list, Is.Not.Null);
        });
        //校验
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message,
            Is.EqualTo("无法从Obase.Test.Domain.Association.School中获取属性456,请检查Include的参数. (Parameter 'sourceType')"));
    }

    /// <summary>
    ///     测试排序
    /// </summary>
    /// <param name="dataSource"></param>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void OrderTest(EDataSource dataSource)
    {
        //测试用关联属性排序
        var context = ContextUtils.CreateContext(dataSource);
        //使用班级名称 和 班级关联的学校创建时间排序
        var classes = context.CreateSet<Class>()
            .OrderBy(p => p.Name).ThenBy(p => p.School.Createtime).ToList();

        Assert.That(classes.Count, Is.EqualTo(1));

        //投影之后 使用学生名称 和 学生关联的班级关联的学校创建时间排序
        var oStud = context.CreateSet<Class>().SelectMany(p => p.Students)
            .OrderBy(p => p.Name).ThenBy(p => p.Class.School.Createtime).ToList();

        Assert.That(oStud.Count, Is.EqualTo(5));
    }

    /// <summary>
    ///     测试投影
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SelectTest(EDataSource dataSource)
    {
        //测试Select
        var context = ContextUtils.CreateContext(dataSource);

        //自己投影自己
        var clazz = context.CreateSet<Class>().Include(p => p.School).Select(p => p).ToList();
        //可以获取到1个
        Assert.That(clazz.Count, Is.EqualTo(1));

        //一对多 Select投影到关联引用
        //结果是List<List<关联引用对象>>
        var stu1 = context.CreateSet<Class>().Select(p => p.Students).ToList();
        //外层获取到1个 里层是5个
        Assert.That(stu1.Count, Is.EqualTo(1));
        Assert.That(stu1[0].Count, Is.EqualTo(5));

        //此处投影到匿名对象和非注册对象 PGSQL暂时有问题 以后修改
        if (dataSource != EDataSource.PostgreSql)
        {
            //将当前对象投影成匿名对象
            var stu2 = context.CreateSet<Student>().Include(p => p.School)
                .Select(s => new { name = s.Name, id = s.StudentId }).Where(p => p.name != "").ToList();
            //5个
            Assert.That(stu2.Count, Is.EqualTo(5));

            //投影成其他对象
            var stu3 = context.CreateSet<Student>().Include(p => p.School)
                .Select(s => new SimpleStu { StudentName = s.Name, Id = s.StudentId }).Where(p => p.StudentName != "")
                .ToList();
            //5个
            Assert.That(stu3.Count, Is.EqualTo(5));
        }

        //测试SelectMany
        context = ContextUtils.CreateContext(dataSource);
        //一对多 SelectMany至关联引用 后 投影成匿名对象
        var stu4 = context.CreateSet<Class>().Include(p => p.School).SelectMany(p => p.Students)
            .Select(s => new { name = s.Name, id = s.StudentId })
            .ToList();
        //5个
        Assert.That(stu4.Count, Is.EqualTo(5));

        //一对多 SelectMany至关联引用
        //结果是合并后的List<关联引用对象>
        var stu5 = context.CreateSet<Class>().Include(p => p.School).SelectMany(c => c.Students).ToList();
        //5个
        Assert.That(stu5.Count, Is.EqualTo(5));

        var allStu = stu5.All(p => p != null);
        //每一个都不是空
        Assert.That(allStu, Is.True);

        //一对多 SelectMany至关联引用
        //结果是合并后的List<关联引用对象>
        var stu51 = context.CreateSet<Class>().Include(p => p.School).SelectMany(c => c.Students, (_, s) => s.Name)
            .ToList();

        //5个
        Assert.That(stu51.Count, Is.EqualTo(1));
        var allStu1 = stu51.All(p => p != null);
        //每一个都不是空
        Assert.That(allStu1, Is.True);

        //一对多 SelectMany至关联引用
        //结果是合并后的List<关联引用对象>
        var stu52 = context.CreateSet<Class>().Include(p => p.School).SelectMany(c => c.Students, (_, s) => s.School)
            .ToList();
        //5个
        Assert.That(stu52.Count, Is.EqualTo(1));

        var allStu2 = stu52.All(p => p != null);
        //每一个都不是空
        Assert.That(allStu2, Is.True);

        //一对多 SelectMany至关联引用 并使用本集合和投影后集合构造匿名对象
        var stu6 = context.CreateSet<Class>().Include(p => p.School)
            .SelectMany(p => p.Students, (_, s) => new { name = s.Name, Id = s.StudentId })
            .ToList();
        //投影后是5个
        Assert.That(stu6.Count, Is.EqualTo(5));

        //一对多 SelectMany至关联引用 并使用本集合和投影后集合构造匿名对象
        var stu61 = context.CreateSet<Class>()
            .SelectMany(p => p.ClassTeachers, (_, s) => new { s.ClassId, s.TeacherId })
            .ToList();
        //投影后只有一个
        Assert.That(stu61.Count, Is.EqualTo(1));

        //一对多 SelectMany至关联引用 ,并取投影后集合中某元素的属性
        var stu62 = context.CreateSet<Class>().Include(p => p.School)
            .SelectMany(p => p.ClassTeachers, (_, s) => s.ClassId)
            .ToList();
        //投影后只有一个
        Assert.That(stu62.Count, Is.EqualTo(1));

        //一对多 SelectMany至关联引用 并使用本集合和投影后集合构造其他对象
        var stu7 = context.CreateSet<Class>().Include(p => p.School)
            .SelectMany(p => p.Students, (p, s) => new SimpleStudent { Id = s.StudentId, StudentName = s.Name })
            .ToList();
        //投影后是5个
        Assert.That(stu7.Count, Is.EqualTo(5));

        //投影到单个属性
        var stuNameList = context.CreateSet<Student>().Include(p => p.School).Select(p => p.Name).Distinct().ToList();
        //投影后是5个
        Assert.That(stuNameList.Count, Is.EqualTo(5));

        //投影到一对一关联的属性
        var studentBackgroundList = context.CreateSet<Student>().Include(p => p.School)
            .Select(p => p.StudentInfo.Background).Distinct().ToList();
        //投影后是5个
        Assert.That(studentBackgroundList.Count, Is.EqualTo(5));

        //投影到枚举
        var schoolType = context.CreateSet<School>().Where(p => p.Name != "").Select(p => p.SchoolType).ToList();
        //一个枚举
        Assert.That(schoolType.Count, Is.EqualTo(1));

        //投影到显式关联
        var classTeachers = context.CreateSet<Class>().SelectMany(p => p.ClassTeachers).ToList();
        //一个任课教师
        Assert.That(classTeachers.Count, Is.EqualTo(1));

        //从显式关联投影到某端的简单属性
        var teacherNames = context.CreateSet<ClassTeacher>().Include(p => p.Class).Include(p => p.Teacher)
            .Select(p => p.Teacher.Name).ToList();
        //一个教师名字
        Assert.That(teacherNames.Count, Is.EqualTo(1));

        //连续的一对一 如A.B.C A.Include(A.B.C).Select(B) <=> B.Include(C)
        var classIncludeSelect = context.CreateSet<Student>().Where(p => p.StudentId > 0).Skip(0).Take(1)
            .Include(p => p.Class.School).Select(p => p.Class).ToList();
        //投影后是1个班级 且加载了学校
        Assert.That(classIncludeSelect.Count, Is.EqualTo(1));
        Assert.That(classIncludeSelect[0].School, Is.Not.Null);

        //classIncludeSelect也可以改写为A.Select(B).Include(B.C)
        classIncludeSelect = context.CreateSet<Student>().Where(p => p.StudentId > 0).Skip(0).Take(1)
            .Select(p => p.Class).Include(p => p.School).ToList();
        //投影后是1个学生 且加载了学校
        Assert.That(classIncludeSelect.Count, Is.EqualTo(1));
        Assert.That(classIncludeSelect[0].School, Is.Not.Null);

        //一对多也可以投影
        var studentsIncludeSelect = context.CreateSet<Class>().Where(p => p.ClassId > 0).Skip(0).Take(1)
            .Include(p => p.Students.Select(q => q.School)).SelectMany(p => p.Students).ToList();
        //投影后是5个学生 且加载了学校
        Assert.That(studentsIncludeSelect.Count, Is.EqualTo(5));
        Assert.That(studentsIncludeSelect.All(p => p.School != null), Is.True);

        //从显式关联型投影
        var explicitlyClass = context.CreateSet<ClassTeacher>().Where(p => p.ClassId > 0).Skip(0).Take(1)
            .Include(p => p.Class.School).Include(p => p.Teacher).Select(p => p.Class).ToList();
        //投影后是1个班级 且加载了学校
        Assert.That(explicitlyClass.Count, Is.EqualTo(1));
        Assert.That(explicitlyClass[0].School, Is.Not.Null);

        //测试投影到显式关联型且Include显式关联型
        var explicitlyClassTeacher1 = context.CreateSet<Class>().Include(p => p.ClassTeachers)
            .Select(p => p.ClassTeachers).ToList();
        //投影后是1个任课教师
        Assert.That(explicitlyClassTeacher1.Count, Is.EqualTo(1));

        var explicitlyClassTeacher2 = context.CreateSet<Class>().Include(p => p.ClassTeachers)
            .SelectMany(p => p.ClassTeachers).ToList();
        //投影后是1个任课教师
        Assert.That(explicitlyClassTeacher2.Count, Is.EqualTo(1));

        //测试投影到显式关联型且Include显式关联型.对端
        var explicitlyClassTeacher3 = context.CreateSet<Class>().Include(p => p.ClassTeachers.Select(q => q.Teacher))
            .Select(p => p.ClassTeachers).ToList();
        //投影后是1个任课教师列表 且列表内的对象加载了教师
        Assert.That(explicitlyClassTeacher3.Count, Is.EqualTo(1));
        Assert.That(explicitlyClassTeacher3[0][0].Teacher, Is.Not.Null);

        //测试平展投影到显式关联型且Include显式关联型.对端
        var explicitlyClassTeacher4 = context.CreateSet<Class>().Include(p => p.ClassTeachers.Select(q => q.Teacher))
            .SelectMany(p => p.ClassTeachers).ToList();
        //投影后是1个任课教师 且加载了教师
        Assert.That(explicitlyClassTeacher4.Count, Is.EqualTo(1));
        Assert.That(explicitlyClassTeacher4[0].Teacher, Is.Not.Null);

        //测试投影到多端之后再次筛选
        var mStu = context.CreateSet<Class>().Where(p => p.Name == "某某班").SelectMany(p => p.Students)
            .Where(p => p.Name == "小2").ToList();
        //投影后是1个学生 名字是小2
        Assert.That(mStu, Is.Not.Null);
        Assert.That(mStu[0].Name, Is.EqualTo("小2"));

        var sStuInfo = context.CreateSet<Student>().Where(p => p.Name == "小3").Select(p => p.StudentInfo)
            .Where(p => p.Background == "普通").ToList();
        //投影后是1个学生信息 描述是普普通通
        Assert.That(sStuInfo, Is.Not.Null);
        Assert.That(sStuInfo[0].Description, Is.EqualTo("普普通通"));

        //从班级投影到学生再投影回来
        var cla = context.CreateSet<Class>().SelectMany(p => p.Students).Where(p => p.Name != "123").ToList()
            .Select(p => p.Class)
            .Where(p => p.Name != "123").ToList();
        //有5个满足条件的学生 再投影后是5个班级 且这5个班级都是一样的
        Assert.That(cla, Is.Not.Null);
        Assert.That(cla.Count == 5, Is.True);
        Assert.That(cla.All(p => p.ClassId == cla.First().ClassId), Is.True);
    }

    /// <summary>
    ///     测试根据关联对象属性筛选
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void WhereTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查询学生信息
        var queryStudent = context.CreateSet<StudentInfo>().FirstOrDefault();
        Assert.That(queryStudent, Is.Not.Null);
        //修改成不普通
        queryStudent.Background = "不普通";
        context.SaveChanges();

        //用学生信息的背景筛选学生
        var students = context.CreateSet<Student>().Where(p => p.StudentInfo.Background == "不普通").ToList();
        //有一个学生
        Assert.That(students, Is.Not.Null);
        Assert.That(students.Count, Is.EqualTo(1));

        //用学生信息的背景包含学生
        students = context.CreateSet<Student>().Where(p => p.StudentInfo.Background.Contains("不普通")).ToList();
        //有一个学生
        Assert.That(students, Is.Not.Null);
        Assert.That(students.Count, Is.EqualTo(1));

        //用之前查询的ID查询学生
        students = context.CreateSet<Student>().Where(p => p.StudentInfo.StudentId == queryStudent.StudentId).ToList();
        //有一个学生
        Assert.That(students, Is.Not.Null);
        Assert.That(students.Count, Is.EqualTo(1));

        //使用之前查询的ID查询学生
        var student = context.CreateSet<Student>()
            .FirstOrDefault(p => p.StudentInfo.StudentId == queryStudent.StudentId);
        //与之前查询的ID相同
        Assert.That(student, Is.Not.Null);
        Assert.That(student.StudentId, Is.EqualTo(queryStudent.StudentId));

        //使用之前查询的ID查询学生
        student = context.CreateSet<Student>().LastOrDefault(p => p.StudentInfo.StudentId == queryStudent.StudentId);
        //与之前查询的ID相同
        Assert.That(student, Is.Not.Null);
        Assert.That(student.StudentId, Is.EqualTo(queryStudent.StudentId));

        //使用列表包含
        var list = new List<long> { -1, -3, queryStudent.StudentId };
        //使用列表包含查询学生
        student = context.CreateSet<Student>().LastOrDefault(p => list.Contains(p.StudentInfo.StudentId));
        //与之前查询的ID相同
        Assert.That(student, Is.Not.Null);
        Assert.That(student.StudentId, Is.EqualTo(queryStudent.StudentId));

        //使用枚举类型查询
        var clazz = context.CreateSet<Class>().Where(p =>
            p.School.SchoolType == ESchoolType.Junior || p.School.SchoolType == ESchoolType.High ||
            p.School.SchoolType == ESchoolType.Primary).ToList();
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.Count, Is.EqualTo(1));

        //查询计数
        var count = context.CreateSet<Student>().Count(p => p.StudentInfo.StudentId == queryStudent.StudentId);
        //1个
        Assert.That(count, Is.EqualTo(1));

        //使用显式关联型的关联属性查询
        var classTeacher = context.CreateSet<ClassTeacher>().Include(p => p.Class).Where(p => p.Teacher.Name == "某老师1")
            .ToList();
        //没有符合条件的任课教师
        Assert.That(classTeacher, Is.Not.Null);
        Assert.That(classTeacher.Count, Is.EqualTo(0));
    }
}

/// <summary>
///     简单Stu类 符合Json命名标准
/// </summary>
public class SimpleStu
{
    /// <summary>
    ///     ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     名字
    /// </summary>
    public string StudentName { get; set; }
}

/// <summary>
///     简单Stu类 符合Json命名标准
/// </summary>
public class SimpleStudent
{
    /// <summary>
    ///     ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     名字
    /// </summary>
    public string StudentName { get; set; }
}

/// <summary>
///     简单Stu类 符合Json命名标准
/// </summary>
public class SimpleGroup
{
    /// <summary>
    ///     ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     名字
    /// </summary>
    public long Count { get; set; }
}