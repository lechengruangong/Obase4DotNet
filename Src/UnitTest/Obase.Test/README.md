## Obase.Test
本项目为Obase的单元测试项目,主要用于验证Obase的功能.
### 目录结构
按照测试所使用的域类进行分类为,分类为AddonTest和CoreTest两大类.
#### AddonTest
AddonTest主要用于测试Obase的插件功能,每个插件都有一个对应的测试文件夹.
- AddonTest/AnnotationTest为标注建模测试.
- AddonTest/LogicDeletionTest为逻辑删除测试.
- AddonTest/MultiTenantTest为多租户测试.
#### CoreTest
CoreTest主要用于测试Obase的核心功能,每一类功能都有一个对应的测试文件夹.
- CoreTest/AssociationTest为关联对象测试.
- CoreTest/FunctionalTest为功能性测试.
- CoreTest/SimpleTypeTest为单独对象测试.
#### Configuration
测试配置文件的管理类放置于此文件夹.
#### Service
测试所需的向Obase注入的服务类放置于此文件夹,包括:
- MessageSender,模拟的消息发送服务,用文件模拟了消息队列,用于测试对象变更通知功能.
- TenantIdReader,模拟的租户ID读取服务,用于测试多租户功能.
### 测试运行
测试会统一输出至./Src/UnitTest/Output目录下对应的运行时文件夹下,可以通过Visual Studio的测试资源管理器运行,也可以通过命令行运行.

运行时需要在./Src/UnitTest/Output目录下放置配置文件Obase.Test.Config.json,该文件的内容可以参考Obase.Test.Config.example.json文件.

配置通过Configuration/TestCaseSourceConfigurationManager类统一管理,具体的用处可以参考Obase.Test.Config.example.json文件的注释和TestCaseSourceConfigurationManager类代码.