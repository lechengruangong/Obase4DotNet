using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Obase.Providers.Sql;
using Obase.Providers.Sql.Common;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     Sql别名缩短器测试（集中式替换方案）。
///     验证在生成Sql字符串后，按"原始别名↔短别名"映射字典将规则生成的别名（下划线前缀）统一替换为
///     "_obase_gen_alias+哈希"的短名称（总长度40以下），以保证不被数据库因别名过长等原因截断。
/// </summary>
[TestFixture]
public class SqlAliasShortenerTest
{
    /// <summary>
    ///     规则生成的别名（下划线前缀）一律缩短为"前缀+哈希"，总长度40以下。
    /// </summary>
    [Test]
    public void Shorten_RuleAlias_IsShortenedWithinFortyChars()
    {
        var aliases = new[] { "_Order_Items_Price", "_Student", "_" + new string('a', 100) };
        foreach (var alias in aliases)
        {
            var shortAlias = SqlAliasShortener.Shorten(alias);
            Assert.That(shortAlias, Does.StartWith(SqlAliasShortener.Prefix), alias);
            Assert.That(shortAlias.Length, Is.LessThan(40), alias);
            Assert.That(shortAlias.Length, Is.EqualTo(SqlAliasShortener.MaxGeneratedLength), alias);
        }
    }

    /// <summary>
    ///     非规则名称（无下划线前缀）保持原样。
    /// </summary>
    [Test]
    public void Shorten_PlainName_KeepsOriginal()
    {
        var names = new[] { "price", "Name", "Student", "t1", "OTB_Col", "obaseOrderCol0" };
        foreach (var name in names)
            Assert.That(SqlAliasShortener.Shorten(name), Is.EqualTo(name), name);
    }

    /// <summary>
    ///     同一原始别名在多次调用中产生相同结果（确定性），且映射字典缓存生效。
    /// </summary>
    [Test]
    public void Shorten_IsDeterministic()
    {
        var alias = "_ThisIsAVeryLongAliasNameThatExceedsTheLimitForSure_Order_Items";
        var first = SqlAliasShortener.Shorten(alias);
        var second = SqlAliasShortener.Shorten(alias);
        Assert.That(second, Is.EqualTo(first));
        Assert.That(SqlAliasShortener.GetShort(alias), Is.EqualTo(first));
    }

    /// <summary>
    ///     缩短是幂等的：对已缩短的别名再次缩短结果不变。
    /// </summary>
    [Test]
    public void Shorten_IsIdempotent()
    {
        var alias = "_ThisIsAVeryLongAliasNameThatExceedsTheLimitForSure_Order_Items";
        var once = SqlAliasShortener.Shorten(alias);
        Assert.That(SqlAliasShortener.Shorten(once), Is.EqualTo(once));
    }

    /// <summary>
    ///     不同的原始别名产生不同的短别名。
    /// </summary>
    [Test]
    public void Shorten_DifferentAliases_ProduceDifferentResults()
    {
        var aliases = new List<string>();
        for (var i = 0; i < 200; i++)
        {
            var alias = $"_{i}_SomeVeryLongElementName_{i}_AnotherVeryLongElementName_{i}";
            aliases.Add(SqlAliasShortener.Shorten(alias));
        }

        Assert.That(aliases.Distinct().Count(), Is.EqualTo(aliases.Count), "不同原始别名产生了相同的短别名");
    }

    /// <summary>
    ///     空别名与null保持原样。
    /// </summary>
    [Test]
    public void Shorten_NullOrEmpty_ReturnsOriginal()
    {
        Assert.That(SqlAliasShortener.Shorten(null), Is.Null);
        Assert.That(SqlAliasShortener.Shorten(""), Is.EqualTo(""));
    }

    /// <summary>
    ///     替换器：识别双引号、反引号、方括号及裸标识符并替换白名单中的别名。
    /// </summary>
    [Test]
    public void Replace_QuotedAndBareIdentifiers_AreReplaced()
    {
        var alias = "_Order_Items";
        var shortAlias = SqlAliasShortener.Shorten(alias);
        var aliases = new HashSet<string> { alias };

        //PostgreSql双引号
        Assert.That(SqlAliasReplacer.Replace($"\"t_order\" \"{alias}\"", aliases),
            Is.EqualTo($"\"t_order\" \"{shortAlias}\""));
        //MySql/Sqlite反引号
        Assert.That(SqlAliasReplacer.Replace($"`t_order` `{alias}`", aliases),
            Is.EqualTo($"`t_order` `{shortAlias}`"));
        //SqlServer方括号
        Assert.That(SqlAliasReplacer.Replace($"[t_order] [{alias}]", aliases),
            Is.EqualTo($"[t_order] [{shortAlias}]"));
        //Oracle裸标识符
        Assert.That(SqlAliasReplacer.Replace($"t_order {alias}", aliases),
            Is.EqualTo($"t_order {shortAlias}"));
        //限定引用
        Assert.That(SqlAliasReplacer.Replace($"\"{alias}\".\"price\"", aliases),
            Is.EqualTo($"\"{shortAlias}\".\"price\""));
    }

    /// <summary>
    ///     替换器：跳过单引号字符串字面量。
    /// </summary>
    [Test]
    public void Replace_StringLiteral_IsNotReplaced()
    {
        var alias = "_Order_Items";
        var aliases = new HashSet<string> { alias };
        var sql = $"select '{alias}' from t";
        Assert.That(SqlAliasReplacer.Replace(sql, aliases), Is.EqualTo(sql));
    }

    /// <summary>
    ///     替换器：不做子串替换，且白名单外的标识符（如真实表名）不替换。
    /// </summary>
    [Test]
    public void Replace_NoSubstringOrNonAliasReplacement()
    {
        var alias = "_Order_Items";
        var shortAlias = SqlAliasShortener.Shorten(alias);
        var aliases = new HashSet<string> { alias };

        //子串不误伤
        Assert.That(SqlAliasReplacer.Replace($"\"{alias}_Price\"", aliases), Is.EqualTo($"\"{alias}_Price\""));
        //白名单外的下划线名称（真实表名等）不替换
        var tableAlias = "_user";
        var sql = $"\"{tableAlias}\" \"{tableAlias}\"";
        Assert.That(SqlAliasReplacer.Replace(sql, aliases), Is.EqualTo(sql));
    }

    /// <summary>
    ///     Sql生成侧集成：QuerySql渲染Sql后，规则别名被统一替换为短别名。
    /// </summary>
    [Test]
    public void QuerySqlRender_RuleAliases_AreReplaced()
    {
        var source = new SimpleSource("t_order", "_Order_Items");
        var query = new QuerySql(source);
        query.SelectionSet.Add(new Field(source, "price"), "_Order_Items_Price");

        var sql = query.ToSql(EDataSource.PostgreSql);
        var expectedSymbol = SqlAliasShortener.Shorten("_Order_Items");
        var expectedColumn = SqlAliasShortener.Shorten("_Order_Items_Price");

        Assert.That(sql, Does.Contain($"\"t_order\" \"{expectedSymbol}\""));
        Assert.That(sql, Does.Contain($"\"{expectedSymbol}\".\"price\" \"{expectedColumn}\""));
        //原始别名不再出现
        Assert.That(sql, Does.Not.Contain("_Order_Items"));
    }

    /// <summary>
    ///     Sql生成侧集成：未设置别名的表名保持原样。
    /// </summary>
    [Test]
    public void QuerySqlRender_PlainTableName_IsKept()
    {
        var query = new QuerySql("t_order");
        var sql = query.ToSql(EDataSource.PostgreSql);
        Assert.That(sql, Does.Contain("\"t_order\" \"t_order\""));
        Assert.That(sql, Does.Not.Contain("_obase_gen_alias"));
    }

    /// <summary>
    ///     Sql生成侧集成：修改Sql（插入）的目标表名保持原样。
    /// </summary>
    [Test]
    public void ChangeSqlRender_InsertTarget_IsKept()
    {
        var change = new ChangeSql("t_order", EChangeType.Insert);
        change.OverwriteField("price", 100);
        var sql = change.ToSql(EDataSource.SqlServer);
        Assert.That(sql, Does.Contain("[t_order]"));
        Assert.That(sql, Does.Not.Contain("_obase_gen_alias"));
    }
}
