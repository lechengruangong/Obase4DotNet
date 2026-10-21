using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     测试构造函数参数绑定元素的合法性校验。
///     构造参数只能绑定到本类型的简单属性（含复杂类型属性），绑定到关联引用、关联端等引用元素时应抛出可读的异常，
///     而不是在建模阶段深处抛出不点名任何信息的NullReferenceException。
/// </summary>
[TestFixture]
public class ConstructorParameterBindingTest
{
    /// <summary>
    ///     国家（隐式关联的被引用端）
    /// </summary>
    public class Country
    {
        public long CountryId { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    ///     人员 含有隐式关联的关联引用 构造参数为两个标量
    /// </summary>
    public class Person
    {
        public Person(long personId, string name)
        {
            PersonId = personId;
            Name = name;
        }

        /// <summary>
        ///     可写的键属性 便于只做建模的用例通过完整性检查
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        ///     普通属性 配置变更触发器后建模时会为此实体生成代理类型（需为virtual）
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     关联引用（隐式关联，端是实体型Country）
        /// </summary>
        public virtual Country HomeCountry { get; set; }
    }

    /// <summary>
    ///     构造参数绑定到普通（只读）标量属性
    /// </summary>
    public class PersonWithScalarCtor
    {
        public PersonWithScalarCtor(long personId, string name)
        {
            PersonId = personId;
            Name = name;
        }

        public long PersonId { get; }

        public string Name { get; }

        public Country HomeCountry { get; set; }
    }

    /// <summary>
    ///     复杂类型
    /// </summary>
    public class Address
    {
        public string City { get; set; }
    }

    /// <summary>
    ///     构造参数绑定到复杂类型属性
    /// </summary>
    public class PersonWithComplexCtor
    {
        public PersonWithComplexCtor(long personId, Address address)
        {
            PersonId = personId;
            Address = address;
        }

        public long PersonId { get; }

        public Address Address { get; }
    }

    /// <summary>
    ///     显式关联型
    /// </summary>
    public class PersonCountry
    {
        public PersonCountry()
        {
        }

        public PersonCountry(PersonWithExplicitReference person, Country country)
        {
            Person = person;
            Country = country;
        }

        public PersonWithExplicitReference Person { get; set; }

        public Country Country { get; set; }
    }

    /// <summary>
    ///     含显式关联引用的实体
    /// </summary>
    public class PersonWithExplicitReference
    {
        public PersonWithExplicitReference()
        {
        }

        public PersonWithExplicitReference(long personId, string name)
        {
            PersonId = personId;
            Name = name;
        }

        public long PersonId { get; }

        public string Name { get; set; }

        /// <summary>
        ///     基于显式关联的关联引用
        /// </summary>
        public Country HomeCountry { get; set; }
    }

    /// <summary>
    ///     城市（多方关联的参与方）
    /// </summary>
    public class City
    {
        public long CityId { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    ///     参与多方（三端）隐式关联的人员 其关联引用指向多方关联本身而不是某个关联端
    /// </summary>
    public class PersonWithMultiAssociation
    {
        public PersonWithMultiAssociation(long personId, string name)
        {
            PersonId = personId;
            Name = name;
        }

        public long PersonId { get; set; }

        /// <summary>
        ///     普通属性 为其配置变更触发器后建模时会生成代理类型（需为virtual）
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     多方关联的关联引用 关联型的端数为3 不提供右端
        /// </summary>
        public List<Tuple<City, Country>> Cities { get; set; }
    }

    /// <summary>
    ///     三端关联型 用于构造“关联引用取不到右端”的情形
    /// </summary>
    public class PersonCityCountry
    {
        public PersonWithMultiAssociation Person { get; set; }

        public City City { get; set; }

        public Country Country { get; set; }
    }

    /// <summary>
    ///     构造参数名与导航属性名相同的实体（用于MapDefault）
    /// </summary>
    public class PersonWithDefaultMap
    {
        /// <summary>
        ///     无参构造 供仅需建模的用例使用
        /// </summary>
        public PersonWithDefaultMap()
        {
        }

        public PersonWithDefaultMap(long personId, Country homeCountry)
        {
            PersonId = personId;
            HomeCountry = homeCountry;
        }

        public long PersonId { get; }

        public Country HomeCountry { get; }
    }

    /// <summary>
    ///     绑定元素名称写错的实体
    /// </summary>
    public class PersonWithWrongElementName
    {
        public PersonWithWrongElementName(long personId, string name)
        {
            PersonId = personId;
            Name = name;
        }

        public long PersonId { get; }

        public string Name { get; }
    }

    /// <summary>
    ///     配置国家实体型
    /// </summary>
    /// <param name="modelBuilder">对象数据模型建造器</param>
    private static void ConfigCountry(ModelBuilder modelBuilder)
    {
        var c = modelBuilder.Entity<Country>();
        c.HasKeyAttribute(p => p.CountryId).HasKeyIsSelfIncreased(true);
        c.ToTable("Country");
    }

    /// <summary>
    ///     用例1/2：构造参数绑定到普通（只读）标量属性 建模应通过
    /// </summary>
    [Test]
    public void MapScalarTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);

        var p = modelBuilder.Entity<PersonWithScalarCtor>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("PersonWithScalarCtor");
        p.HasConstructor(typeof(PersonWithScalarCtor).GetConstructor(new[] { typeof(long), typeof(string) }))
            .Map(x => x.PersonId)
            .Map(x => x.Name)
            .End();

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     用例3：构造参数绑定到复杂类型属性 建模应通过
    /// </summary>
    [Test]
    public void MapComplexTypeTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);
        modelBuilder.Complex<Address>();

        var p = modelBuilder.Entity<PersonWithComplexCtor>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("PersonWithComplexCtor");
        p.HasConstructor(typeof(PersonWithComplexCtor).GetConstructor(new[] { typeof(long), typeof(Address) }))
            .Map(x => x.PersonId)
            .Map(x => x.Address)
            .End();

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     用例5：构造参数绑定到隐式关联的关联引用 Map阶段即拦截
    /// </summary>
    [Test]
    public void MapImplicitAssociationReferenceTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);

        var p = modelBuilder.Entity<Person>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("Person");
        p.Attribute(x => x.Name, typeof(string)).HasChangeTrigger();

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            p.HasConstructor(typeof(Person).GetConstructor(new[] { typeof(long), typeof(string) }))
                .Map(x => x.PersonId)
                .Map(x => x.HomeCountry)
                .End();
        });

        //异常信息应点名类型名、属性名和参数名 并给出正确做法
        Assert.That(exception.Message, Does.Contain(nameof(Person)));
        Assert.That(exception.Message, Does.Contain("HomeCountry"));
        Assert.That(exception.Message, Does.Contain("name"));
        Assert.That(exception.Message, Does.Contain("AssociationEnd + HasMapping + AssociationReference"));
    }

    /// <summary>
    ///     用例5（兜底）：关联引用的右端无法确定时（显式关联或端数大于2的多方关联），
    ///     建模阶段重建代理类型构造器（ModelBuilder.CreateConstructor → Parameter.GetType → AssociationReference.ValueType）
    ///     应给出点名问题的异常，而不是裸NullReferenceException
    /// </summary>
    [Test]
    public void BindImplicitAssociationReferenceTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);

        var p = modelBuilder.Entity<PersonWithMultiAssociation>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("PersonWithMultiAssociation");
        //配置变更触发器 建模时会为此实体生成代理类型 从而触发构造器在代理类型上的重建
        p.Attribute(x => x.Name, typeof(string)).HasChangeTrigger();
        p.HasConstructor(typeof(PersonWithMultiAssociation).GetConstructor(new[] { typeof(long), typeof(string) }))
            .Map(x => x.PersonId)
            .Map(x => x.Name)
            .End();

        modelBuilder.Entity<City>();
        //多方(三端)关联 关联引用不指向具体的关联端
        var association = modelBuilder.Association<PersonCityCountry>();
        association.AssociationEnd(x => x.Person).HasMapping("PersonId", "PersonId");
        association.AssociationEnd(x => x.City).HasMapping("CityId", "CityId");
        association.AssociationEnd(x => x.Country).HasMapping("CountryId", "CountryId");
        association.ToTable("PersonCityCountry");

        var model = modelBuilder.Build();
        var objectType = model.GetObjectType(typeof(PersonWithMultiAssociation));
        var associationReference = (AssociationReference)objectType.GetElement("Cities");
        //把关联型改为隐式关联 使AssociationReference.ValueType走“取右端”的分支
        //而三端关联的GetRightEnd()恒返回null 这正是原缺陷抛出裸NullReferenceException的路径
        associationReference.AssociationType.Visible = false;
        Assert.That(associationReference.GetRightEnd(), Is.Null);

        //绕过Map阶段的校验 把name参数绑定到这个关联引用上
        ((InstanceConstructor)objectType.Constructor).Parameters.First(x => x.Name == "name").ElementName = "Cities";

        var exception = Assert.Throws<ArgumentException>(() =>
            ModelBuilderHost.CreateProxyConstructor(typeof(PersonWithMultiAssociation), objectType, modelBuilder));
        //异常不能是裸空引用异常 且要点名关联引用、端数和正确做法
        Assert.That(exception.Message, Does.Contain("Cities"));
        Assert.That(exception.Message, Does.Contain("端数为3"));
        Assert.That(exception.Message, Does.Contain("AssociationEnd + HasMapping + AssociationReference"));
    }

    /// <summary>
    ///     用例6：构造参数绑定到显式关联型上的关联引用 Map阶段即拦截
    /// </summary>
    [Test]
    public void MapExplicitAssociationReferenceTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);

        var p = modelBuilder.Entity<PersonWithExplicitReference>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("PersonWithExplicitReference");

        var association = modelBuilder.Association<PersonCountry>();
        association.AssociationEnd(p => p.Person).HasMapping("PersonId", "PersonId");
        association.AssociationEnd(p => p.Country).HasMapping("CountryId", "CountryId");
        association.ToTable("PersonCountry");

        //关联端配置完成后关联引用配置项已建立
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            p.HasConstructor(typeof(PersonWithExplicitReference).GetConstructor(new[] { typeof(long), typeof(string) }))
                .Map(x => x.PersonId)
                .Map(x => x.HomeCountry)
                .End();
        });

        Assert.That(exception.Message, Does.Contain(nameof(PersonWithExplicitReference)));
        Assert.That(exception.Message, Does.Contain("HomeCountry"));
        Assert.That(exception.Message, Does.Contain("AssociationEnd + HasMapping + AssociationReference"));
    }

    /// <summary>
    ///     用例7：构造参数绑定到关联端 Map阶段即拦截
    /// </summary>
    [Test]
    public void MapAssociationEndTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);

        var association = modelBuilder.Association<PersonCountry>();
        association.AssociationEnd(p => p.Person).HasMapping("PersonId", "PersonId");
        association.AssociationEnd(p => p.Country).HasMapping("CountryId", "CountryId");
        association.ToTable("PersonCountry");

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            association.HasConstructor(typeof(PersonCountry)
                    .GetConstructor(new[] { typeof(PersonWithExplicitReference), typeof(Country) }))
                .Map(nameof(PersonCountry.Person))
                .Map(nameof(PersonCountry.Country))
                .End();
        });

        Assert.That(exception.ParamName, Is.EqualTo("elementName"));
        Assert.That(exception.Message, Does.Contain(nameof(PersonCountry)));
        Assert.That(exception.Message, Does.Contain(nameof(PersonCountry.Person)));
        Assert.That(exception.Message, Does.Contain("引用元素"));
    }

    /// <summary>
    ///     用例8：MapDefault 参数名撞上导航属性名 Map阶段即拦截
    /// </summary>
    [Test]
    public void MapDefaultAssociationReferenceTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);

        var p = modelBuilder.Entity<PersonWithDefaultMap>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("PersonWithDefaultMap");

        //参数名homeCountry与导航属性HomeCountry仅大小写不同 MapDefault按参数名绑定会撞上导航属性
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            p.HasConstructor(typeof(PersonWithDefaultMap)
                    .GetConstructor(new[] { typeof(long), typeof(Country) }))
                .Map(x => x.PersonId)
                .MapDefault()
                .End();
        });

        Assert.That(exception.ParamName, Is.EqualTo("parameterName"));
        Assert.That(exception.Message, Does.Contain("homeCountry"));
        Assert.That(exception.Message, Does.Contain("HomeCountry"));
    }


    /// <summary>
    ///     用例9：绑定元素名称写错（元素不存在） 建模阶段应给出点名元素名与类型名的异常
    /// </summary>
    [Test]
    public void MapNotExistElementTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var p = modelBuilder.Entity<PersonWithWrongElementName>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("PersonWithWrongElementName");
        //先把构造器配成合法的 完成一次建模让元素配置项就绪
        p.HasConstructor(typeof(PersonWithWrongElementName)
                .GetConstructor(new[] { typeof(long), typeof(string) }))
            .Map(x => x.PersonId)
            .Map(x => x.Name)
            .End();
        var model = modelBuilder.Build();
        var objectType = model.GetObjectType(typeof(PersonWithWrongElementName));

        //再把name参数改绑到不存在的元素上（绕过Map阶段的校验）
        ((InstanceConstructor)objectType.Constructor).Parameters.First(x => x.Name == "name").ElementName =
            "NotExistElement";

        var exception = Assert.Throws<ArgumentException>(() =>
            ModelBuilderHost.CreateProxyConstructor(typeof(PersonWithWrongElementName), objectType, modelBuilder));
        //异常应点名元素名与类型名
        Assert.That(exception.Message, Does.Contain("NotExistElement"));
        Assert.That(exception.Message, Does.Contain(nameof(PersonWithWrongElementName)));
    }

    /// <summary>
    ///     用例10：不配置构造器 走无参构造推断 建模应通过
    /// </summary>
    [Test]
    public void ImplicitConstructorTest()
    {
        var modelBuilder = ModelBuilderHost.Create();
        //Country有无参构造
        var c = modelBuilder.Entity<Country>();
        c.HasKeyAttribute(p => p.CountryId).HasKeyIsSelfIncreased(true);
        c.ToTable("Country");

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     用例11：显式关联型 + 两端HasMapping + 构造参数只绑标量 建模应通过
    /// </summary>
    [Test]
    public void ExplicitAssociationWithScalarCtorTest()
    {
        var modelBuilder = ModelBuilderHost.Create(ConfigCountry);

        var p = modelBuilder.Entity<PersonWithExplicitReference>();
        p.HasKeyAttribute(x => x.PersonId).HasKeyIsSelfIncreased(false);
        p.ToTable("PersonWithExplicitReference");
        p.HasConstructor(typeof(PersonWithExplicitReference).GetConstructor(new[] { typeof(long), typeof(string) }))
            .Map(x => x.PersonId)
            .Map(x => x.Name)
            .End();

        var association = modelBuilder.Association<PersonCountry>();
        association.AssociationEnd(p => p.Person).HasMapping("PersonId", "PersonId");
        association.AssociationEnd(p => p.Country).HasMapping("CountryId", "CountryId");
        association.ToTable("PersonCountry");

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     建模器容器 用于在测试中取得建模器。
    ///     ModelBuilder的构造函数为internal 此处借助反射创建，取得后可继续配置并调用Build。
    /// </summary>
    private static class ModelBuilderHost
    {
        /// <summary>
        ///     创建一个建模器
        /// </summary>
        /// <param name="configure">建模器配置</param>
        /// <returns>建模器</returns>
        public static ModelBuilder Create(Action<ModelBuilder> configure = null)
        {
            var modelBuilder = (ModelBuilder)Activator.CreateInstance(typeof(ModelBuilder),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,
                new object[] { null }, null);
            configure?.Invoke(modelBuilder);
            return modelBuilder;
        }

        /// <summary>
        ///     直接调用建模阶段重建代理类型构造器的私有方法ModelBuilder.CreateConstructor
        ///     proxyType传入原类型即可 本方法只用到其GetConstructor
        /// </summary>
        /// <param name="targetType">对象类型对应的CLR类型</param>
        /// <param name="objectType">对象类型模型</param>
        /// <param name="modelBuilder">建模器</param>
        /// <returns>重建的构造器</returns>
        public static IInstanceConstructor CreateProxyConstructor(Type targetType, ObjectType objectType,
            ModelBuilder modelBuilder)
        {
            var method = typeof(ModelBuilder).GetMethod("CreateConstructor",
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(Type), typeof(IInstanceConstructor) }, null);
            try
            {
                return (IInstanceConstructor)method.Invoke(modelBuilder,
                    new object[] { targetType, objectType.Constructor });
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                //把反射调用包装的异常还原出来 便于断言真实异常类型
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw;
            }
        }
    }
}
