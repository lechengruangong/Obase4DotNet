using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     测试配置继承关系时"必须先配父类"的校验与提示。
///     父类未注册、或父类未配置具体类型判别值时，应在建模阶段抛出具名且说明改法的异常，
///     而不是在建模深处抛出不点名任何信息的NullReferenceException。
/// </summary>
[TestFixture]
public class InheritanceConfigurationOrderTest
{
    /// <summary>
    ///     基类
    /// </summary>
    public class Animal
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    ///     派生类
    /// </summary>
    public class Dog : Animal
    {
        public string Breed { get; set; }
    }

    /// <summary>
    ///     三级继承链的最末一级
    /// </summary>
    public class Puppy : Dog
    {
        public int Age { get; set; }
    }

    /// <summary>
    ///     抽象基类
    /// </summary>
    public abstract class Shape
    {
        public long ShapeId { get; set; }
    }

    /// <summary>
    ///     抽象基类的派生类
    /// </summary>
    public class Circle : Shape
    {
        public double Radius { get; set; }
    }

    /// <summary>
    ///     用例1：父类先配完整、再配子类 建模应通过（原有行为不变）
    /// </summary>
    [Test]
    public void ParentConfiguredBeforeChildTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.HasConcreteTypeDiscriminator("Type");
        animal.HasConcreteTypeSign(1);
        animal.ToTable("Animal");

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();
        dog.HasConcreteTypeSign(2);
        dog.ToTable("Animal");

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     用例2：三级继承链 严格父先子后 建模应通过（原有行为不变）
    /// </summary>
    [Test]
    public void ThreeLevelInheritanceTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.HasConcreteTypeDiscriminator("Type");
        animal.HasConcreteTypeSign(1);
        animal.ToTable("Animal");

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();
        dog.HasConcreteTypeSign(2);
        dog.ToTable("Animal");

        var puppy = modelBuilder.Entity<Puppy>();
        puppy.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        puppy.DeriveFrom<Dog>();
        puppy.HasConcreteTypeSign(3);
        puppy.ToTable("Animal");

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     用例3：抽象基类参与继承 判别值用一个不会被实际使用的值 建模应通过（原有行为不变）
    /// </summary>
    [Test]
    public void AbstractBaseInheritanceTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var shape = modelBuilder.Entity<Shape>();
        shape.HasKeyAttribute(p => p.ShapeId).HasKeyIsSelfIncreased(true);
        shape.HasConcreteTypeDiscriminator("Type");
        //抽象类型不会被创建 配置一个特殊值即可
        shape.HasConcreteTypeSign(-1);
        shape.ToTable("Shape");

        var circle = modelBuilder.Entity<Circle>();
        circle.HasKeyAttribute(p => p.ShapeId).HasKeyIsSelfIncreased(true);
        circle.DeriveFrom<Shape>();
        circle.HasConcreteTypeSign(1);
        circle.ToTable("Shape");

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     用例4：CLR上有继承关系但未配置DeriveFrom 两者各自建表 建模应通过（原有行为不变）
    /// </summary>
    [Test]
    public void NoDeriveFromConfiguredTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.ToTable("Animal");

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.ToTable("Dog");

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     用例5：子类配置HasConcreteTypeDiscriminator而父类未注册 应抛具名异常而不是NullReferenceException
    /// </summary>
    [Test]
    public void ChildHasConcreteTypeDiscriminatorWithoutParentTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();

        var exception = Assert.Throws<ArgumentException>(() => dog.HasConcreteTypeDiscriminator("Type"));
        //异常应点名子类与父类
        Assert.That(exception.Message, Does.Contain(nameof(Dog)));
        Assert.That(exception.Message, Does.Contain(nameof(Animal)));
    }

    /// <summary>
    ///     用例6：子类配置HasConcreteTypeSign而父类未注册 应抛具名异常而不是NullReferenceException
    /// </summary>
    [Test]
    public void ChildHasConcreteTypeSignWithoutParentTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();

        var exception = Assert.Throws<ArgumentException>(() => dog.HasConcreteTypeSign(1));
        //异常应点名子类与父类
        Assert.That(exception.Message, Does.Contain(nameof(Dog)));
        Assert.That(exception.Message, Does.Contain(nameof(Animal)));
    }

    /// <summary>
    ///     用例7：子类只配置DeriveFrom而父类未注册 建模阶段应抛具名异常
    /// </summary>
    [Test]
    public void ChildOnlyDeriveFromWithoutParentTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();
        dog.ToTable("Animal");

        var exception = Assert.Throws<ArgumentException>(() => modelBuilder.Build());
        //异常应点名子类与父类 并指出需要先注册父类
        Assert.That(exception.Message, Does.Contain(nameof(Dog)));
        Assert.That(exception.Message, Does.Contain(nameof(Animal)));
        Assert.That(exception.Message, Does.Contain("注册"));
    }

    /// <summary>
    ///     用例8：父类已注册但没有配置判别器与判别值 建模阶段应抛具名异常
    /// </summary>
    [Test]
    public void ParentWithoutConcreteTypeSignTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.ToTable("Animal");

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();
        dog.ToTable("Animal");

        var exception = Assert.Throws<ArgumentException>(() => modelBuilder.Build());
        //异常应点名父类 并给出判别器与判别值的配置方法
        Assert.That(exception.Message, Does.Contain(nameof(Animal)));
        Assert.That(exception.Message, Does.Contain("HasConcreteTypeDiscriminator"));
        Assert.That(exception.Message, Does.Contain("HasConcreteTypeSign"));
    }

    /// <summary>
    ///     用例9：父类配置了判别器与判别值 但子类没有配置判别值 建模阶段应抛具名异常
    ///     （父类使用内置判别器时 建模需要收集整条继承链的判别值）
    /// </summary>
    [Test]
    public void ChildWithoutConcreteTypeSignTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.HasConcreteTypeDiscriminator("Type");
        animal.HasConcreteTypeSign(1);
        animal.ToTable("Animal");

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();
        dog.ToTable("Animal");

        var exception = Assert.Throws<ArgumentException>(() => modelBuilder.Build());
        //异常应点名未配置判别值的子类 并给出判别器与判别值的配置方法
        Assert.That(exception.Message, Does.Contain(nameof(Dog)));
        Assert.That(exception.Message, Does.Contain("具体类型判别值"));
        Assert.That(exception.Message, Does.Contain("HasConcreteTypeSign"));
    }

    /// <summary>
    ///     用例9-1：父类配置了自定义判别器与判别值 但子类没有配置判别值
    ///     建模应给出完整性检查的说明性错误 而不是NullReferenceException
    /// </summary>
    [Test]
    public void ChildWithoutConcreteTypeSignWithCustomDiscriminatorTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.HasConcreteTypeDiscriminator("Type", new NullConcreteTypeDiscriminator());
        animal.HasConcreteTypeSign(1);
        animal.ToTable("Animal");

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();
        dog.ToTable("Animal");

        var exception = Assert.Throws<IntegrityCheckFailException>(() => modelBuilder.Build());
        //完整性检查应点名子类 并指出没有配置具体类型判别标志
        var messages = string.Join(";", exception.ErrorMessageDictionary.SelectMany(p => p.Value));
        Assert.That(messages, Does.Contain(nameof(Dog)));
        Assert.That(messages, Does.Contain("具体类型判别"));
    }

    /// <summary>
    ///     用例10：父类已注册但没有配置判别器 子类配置判别值时应给出既有的说明性提示
    /// </summary>
    [Test]
    public void ParentWithoutDiscriminatorChildHasSignTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.ToTable("Animal");

        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();

        var exception = Assert.Throws<ArgumentException>(() => dog.HasConcreteTypeSign(2));
        //既有的说明性提示 指出需要先在基类上配置判别字段
        Assert.That(exception.Message, Does.Contain("没有找到类型判别字段"));
        Assert.That(exception.Message, Does.Contain("HasConcreteTypeDiscriminator"));
    }

    /// <summary>
    ///     用例11：子类先DeriveFrom、父类随后注册并配置、最后补子类判别值 建模应通过。
    ///     DeriveFrom本身不校验父类是否已注册 注册顺序只要保证建模时父类已在模型中即可。
    /// </summary>
    [Test]
    public void ParentRegisteredAfterChildDeriveFromTest()
    {
        var modelBuilder = ModelBuilderHost.Create();

        //先注册子类并声明继承 此时父类还未注册
        var dog = modelBuilder.Entity<Dog>();
        dog.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        dog.DeriveFrom<Animal>();

        //再注册并配置父类
        var animal = modelBuilder.Entity<Animal>();
        animal.HasKeyAttribute(p => p.Id).HasKeyIsSelfIncreased(true);
        animal.HasConcreteTypeDiscriminator("Type");
        animal.HasConcreteTypeSign(1);
        animal.ToTable("Animal");

        //最后补上子类的判别值
        dog.HasConcreteTypeSign(2);
        dog.ToTable("Animal");

        Assert.DoesNotThrow(() => modelBuilder.Build());
    }

    /// <summary>
    ///     不做实际判别的自定义判别器 用于验证配置了自定义判别器时的建模行为
    /// </summary>
    private class NullConcreteTypeDiscriminator : IConcreteTypeDiscriminator
    {
        /// <summary>
        ///     根据类型代码选择一个具体类型
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <returns>本用例中不使用</returns>
        public StructuralType Discriminate(object typeCode)
        {
            return null;
        }
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
        /// <returns>建模器</returns>
        public static ModelBuilder Create()
        {
            return (ModelBuilder)Activator.CreateInstance(typeof(ModelBuilder),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,
                new object[] { null }, null);
        }
    }
}
