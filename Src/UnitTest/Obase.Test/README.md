## Obase.Test
本项目为Obase的单元测试项目,主要用于验证Obase的功能和稳定性.
### 目录结构
按照测试所使用的域类进行分类为,分类为AddonTest和CoreTest两大类.
### AddonTest
AddonTest主要用于测试Obase的插件功能,每个插件都有一个对应的测试文件夹.
- AddonTest/LogicDeletionTest为逻辑删除测试.
- AddonTest/MultiTenantTest为多租户测试.
- AddonTest/AnnotationTest为标注建模测试.
### CoreTest
CoreTest主要用于测试Obase的核心功能,每一类功能都有一个对应的测试文件夹.
- CoreTest/FunctionalTest为功能性测试.
- CoreTest/AssociationTest为关联对象测试.
- CoreTest/SimpleTypeTest为单独对象测试.
### 测试运行
测试会统一输出至./Src/UnitTest/Output目录下对应的运行时文件夹下,可以通过Visual Studio的测试资源管理器运行,也可以通过命令行运行.

首次运行时需要在./Src/UnitTest/Output目录下放置配置文件Obase.Test.Config.json,该文件应当包含测试所需的数据库连接字符串.配置通过RelationshipDataBaseConfigurationManager类统一管理,如果需要调整文件位置或名称,可以在代码中修改Obase.Test.Config.json的路径和名称.

配置具体的用处可以参考Obase.Test.Config.example.json文件的注释和测试项目的TestCaseSourceConfigurationManager类代码.