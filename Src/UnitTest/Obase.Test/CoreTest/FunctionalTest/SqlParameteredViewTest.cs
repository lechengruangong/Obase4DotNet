using System;
using System.Linq;
using Obase.Core;
using Obase.Core.MappingPipeline;
using Obase.Providers.Sql;
using Obase.Providers.Sql.SqlObject;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     QuerySqlParameteredView查看执行的SQL语句测试
/// </summary>
[TestFixture]
public class SqlParameteredViewTest
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
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i < 21; i++)
                context.CreateSet<JavaBean>().Attach(new JavaBean
                {
                    Bool = i % 2 == 0,
                    DateTime = DateTime.Now,
                    DecimalNumber = new decimal(Math.Pow(Math.PI, i)),
                    IntNumber = i,
                    LongNumber = i,
                    ByteNumber = (byte)i,
                    Guid = Guid.NewGuid(),
                    CharNumber = '\u006A',
                    FloatNumber = (float)Math.Pow(Math.PI, i),
                    DoubleNumber = (float)Math.Pow(Math.PI, i),
                    Time = new TimeSpan(0, 8, 40, 0),
                    Date = DateTime.Now,
                    String = $"{i}号字符串",
                    Strings = new[] { $"{i - 1}", $"{i}", $"{i + 1}" }
                });

            context.SaveChanges();
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
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     测试查看执行的SQL语句
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ViewTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //模块仅对当前上下文有效 如果需要都生效 把注册代码放置于构造函数中
        context.RegisterModule(new QueryTestModule(dataSource));

        //测试较为复杂条件
        var whereResult = context.CreateSet<JavaBean>()
            .Where(p => p.IntNumber > 0 && p.DecimalNumber > 987 && p.Strings.Contains("2") == false).ToList();

        Assert.That(whereResult.Count, Is.EqualTo(9));
        //测试几种布尔值的查询
        whereResult = context.CreateSet<JavaBean>().Where(p => true).ToList();

        Assert.That(whereResult.Count, Is.EqualTo(20));

        //测试常数表达式置于比较前方
        whereResult = context.CreateSet<JavaBean>()
            .Where(p => 0 < p.IntNumber && 987 < p.DecimalNumber && p.Strings.Contains("2") == false).ToList();

        Assert.That(whereResult.Count, Is.EqualTo(9));

        //测试几种取空
        whereResult = context.CreateSet<JavaBean>().Where(p => null != p.String).ToList();

        Assert.That(whereResult.Count, Is.EqualTo(20));

        whereResult = context.CreateSet<JavaBean>().Where(p => "" != p.String).ToList();

        Assert.That(whereResult.Count, Is.EqualTo(20));
    }
}

/// <summary>
///     用于显示Sql语句的模块
/// </summary>
public class QueryTestModule : IMappingModule
{
    /// <summary>
    ///     当前的数据源
    /// </summary>
    private readonly EDataSource _dataSource;

    /// <summary>
    ///     构造用于显示Sql语句的模块
    /// </summary>
    public QueryTestModule(EDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <summary>
    ///     初始化映射模块。
    /// </summary>
    /// <param name="savingPipeline">"保存"管道。</param>
    /// <param name="deletingPipeline">"删除"管道。</param>
    /// <param name="queryPipeline">"查询"管道。</param>
    /// <param name="directlyChangingPipeline">"就地修改"管道。</param>
    /// <param name="objectContext">对象上下文</param>
    public void Init(ISavingPipeline savingPipeline, IDeletingPipeline deletingPipeline,
        IQueryPipeline queryPipeline,
        IDirectlyChangingPipeline directlyChangingPipeline, ObjectContext objectContext)
    {
        savingPipeline.PreExecuteCommand += (sender, args) =>
        {
            Assert.That(sender, Is.Not.Null);
            //用QuerySqlParameteredView进行处理
            var view = SqlParameterizedView.GetSqlParameterizedView((ChangeSql)args.Command,
                _dataSource);
            Assert.That(view, Is.Not.Null);
            Assert.That(view.SqlString, Is.Not.Null);
            Assert.That(view.Parameters, Is.Not.Null);
            Assert.That(view.SimpleViewString, Is.Not.Null);
        };

        savingPipeline.PostExecuteCommand += (sender, args) =>
        {
            Assert.That(sender, Is.Not.Null);
            //用QuerySqlParameteredView进行处理
            var view = SqlParameterizedView.GetSqlParameterizedView((ChangeSql)args.Command,
                _dataSource);
            Assert.That(view, Is.Not.Null);
            Assert.That(view.SqlString, Is.Not.Null);
            Assert.That(view.Parameters, Is.Not.Null);
            Assert.That(view.SimpleViewString, Is.Not.Null);
        };

        queryPipeline.PreExecuteCommand += (sender, args) =>
        {
            Assert.That(sender, Is.Not.Null);
            //用QuerySqlParameteredView进行处理
            var view = SqlParameterizedView.GetSqlParameterizedView((QuerySql)args.Context.Command,
                _dataSource);
            Assert.That(view, Is.Not.Null);
            Assert.That(view.SqlString, Is.Not.Null);
            Assert.That(view.Parameters, Is.Not.Null);
            Assert.That(view.SimpleViewString, Is.Not.Null);
        };

        queryPipeline.PostExecuteCommand += (sender, args) =>
        {
            Assert.That(sender, Is.Not.Null);
            //用QuerySqlParameteredView进行处理
            var view = SqlParameterizedView.GetSqlParameterizedView((QuerySql)args.Context.Command,
                _dataSource);
            Assert.That(view, Is.Not.Null);
            Assert.That(view.SqlString, Is.Not.Null);
            Assert.That(view.Parameters, Is.Not.Null);
            Assert.That(view.SimpleViewString, Is.Not.Null);
        };
    }
}