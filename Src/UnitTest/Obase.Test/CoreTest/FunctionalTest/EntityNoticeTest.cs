using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Obase.Core.MappingPipeline;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     实体通知测试
/// </summary>
[TestFixture]
public class EntityNoticeTest
{
    /// <summary>
    ///     构造实例 为上下文赋值
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<NoticeSutdentInfo>().Delete(p => p.StudentId > 0);
        }
    }

    /// <summary>
    ///     销毁对象
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<NoticeSutdentInfo>().Delete(p => p.StudentId > 0);
            File.Delete(_path);
        }
    }

    /// <summary>
    ///     用于模拟消息队列的Txt文件路径
    /// </summary>
    private readonly string _path = $"{Directory.GetCurrentDirectory()}\\TestQueue.txt";

    /// <summary>
    ///     测试对象变更通知
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void NoticeTest(EDataSource dataSource)
    {
        var studentInfo = new NoticeSutdentInfo
        {
            Background = "强大背景",
            Description = "不可详查",
            StudentId = 888
        };

        var context = ContextUtils.CreateContext(dataSource);
        //启用修改消息通知
        context.EnableChangeNotice();
        //创建对象
        context.Attach(studentInfo);
        context.SaveChanges();
        //读取变更通知
        var changeNotice = ReadChangeMessage();

        //读取到变更通知是创建通知
        Assert.That(changeNotice, Is.Not.Null);
        Assert.That(changeNotice.Attributes.Count, Is.EqualTo(3));
        Assert.That(changeNotice.ObjectKeys.Count, Is.EqualTo(1));
        Assert.That(changeNotice.ChangeAction, Is.EqualTo("Create"));
        Assert.That(changeNotice.Attributes.FirstOrDefault(p => p.Attribute == "Description").Value.ToString(),
            Is.EqualTo("不可详查"));
        Assert.That(changeNotice.Attributes.FirstOrDefault(p => p.Attribute == "Background").Value.ToString(),
            Is.EqualTo("强大背景"));

        //修改对象
        studentInfo.Background = "神秘背景";
        context.SaveChanges();

        //读取变更通知
        changeNotice = ReadChangeMessage();

        //读取到变更通知是修改通知
        Assert.That(changeNotice, Is.Not.Null);
        Assert.That(changeNotice.Attributes.Count, Is.EqualTo(3));
        Assert.That(changeNotice.ObjectKeys.Count, Is.EqualTo(1));
        Assert.That(changeNotice.ChangeAction, Is.EqualTo("Update"));
        Assert.That(changeNotice.Attributes.FirstOrDefault(p => p.Attribute == "Description").Value.ToString(),
            Is.EqualTo("不可详查"));
        Assert.That(changeNotice.Attributes.FirstOrDefault(p => p.Attribute == "Background").Value.ToString(),
            Is.EqualTo("神秘背景"));

        //标记删除
        var queryStudentInfo = context.CreateSet<NoticeSutdentInfo>().FirstOrDefault(p => p.StudentId == 888);
        context.CreateSet<NoticeSutdentInfo>().Remove(queryStudentInfo);
        context.SaveChanges();

        //读取变更通知
        changeNotice = ReadChangeMessage();

        //读取到变更通知是删除通知
        Assert.That(changeNotice, Is.Not.Null);
        Assert.That(changeNotice.Attributes.Count, Is.EqualTo(3));
        Assert.That(changeNotice.ObjectKeys.Count, Is.EqualTo(1));
        Assert.That(changeNotice.ChangeAction, Is.EqualTo("Delete"));
        Assert.That(changeNotice.Attributes.FirstOrDefault(p => p.Attribute == "Description").Value.ToString(),
            Is.EqualTo("不可详查"));
        Assert.That(changeNotice.Attributes.FirstOrDefault(p => p.Attribute == "Background").Value.ToString(),
            Is.EqualTo("神秘背景"));

        //直接修改对象
        context.CreateSet<NoticeSutdentInfo>().SetAttributes(
            new[]
            {
                new KeyValuePair<string, object>("Background", "极度强大"),
                new KeyValuePair<string, object>("Description", "无法估计")
            },
            p => p.StudentId == 888);

        //读取直接修改通知
        var directNotice = ReadDirectMessage();

        Assert.That(directNotice, Is.Not.Null);
        Assert.That(directNotice.DirectlyChangeType, Is.EqualTo(EDirectlyChangeType.Update));
        Assert.That(directNotice.NewValues.FirstOrDefault(p => p.Key == "Background").Value.ToString(),
            Is.EqualTo("极度强大"));
        Assert.That(directNotice.NewValues.FirstOrDefault(p => p.Key == "Description").Value.ToString(),
            Is.EqualTo("无法估计"));
    }

    /// <summary>
    ///     转换为变更通知对象
    /// </summary>
    /// <returns></returns>
    private ObjectChangeNotice ReadChangeMessage()
    {
        var notice = JsonConvert.DeserializeObject<ObjectChangeNotice>(ReadMessage());
        return notice;
    }

    /// <summary>
    ///     转换为直接通知对象
    /// </summary>
    /// <returns></returns>
    private DirectlyChangingNotice ReadDirectMessage()
    {
        var notice = JsonConvert.DeserializeObject<DirectlyChangingNotice>(ReadMessage());
        return notice;
    }

    /// <summary>
    ///     读取通知序列化结果
    /// </summary>
    /// <returns></returns>
    private string ReadMessage()
    {
        //读消息
        var fileStream = new FileStream(_path, FileMode.OpenOrCreate);
        var reader = new StreamReader(fileStream);
        var result = reader.ReadLine();
        reader.Close();
        //读完就删除文件
        File.Delete(_path);

        return result;
    }
}