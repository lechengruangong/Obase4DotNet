# Obase4dotNet

本仓库为Obase在dotNet平台上的实现.

# 如何安装

本项目发布于Nuget,共有以下软件包.

| 包名                      | 地址                                                                                                                                    | 简介                                            |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| Obase.Core                | [![latest version](https://img.shields.io/nuget/v/Obase.Core)](https://www.nuget.org/packages/Obase.Core)                               | Obase存储抽象层框架中间件（.NET版).             |
| Obase.LogicDeletion       | [![latest version](https://img.shields.io/nuget/v/Obase.LogicDeletion)](https://www.nuget.org/packages/Obase.LogicDeletion)             | Obase存储抽象层框架中间件逻辑删除扩展.          |
| Obase.MultiTenant         | [![latest version](https://img.shields.io/nuget/v/Obase.MultiTenant)](https://www.nuget.org/packages/Obase.MultiTenant)                 | Obase存储抽象层框架中间件多租户扩展.            |
| Obase.Odm.Annotation      | [![latest version](https://img.shields.io/nuget/v/Obase.Odm.Annotation)](https://www.nuget.org/packages/Obase.Odm.Annotation)           | Obase存储抽象层框架中间件标注建模扩展.          |
| Obase.Providers.Sql       | [![latest version](https://img.shields.io/nuget/v/Obase.Providers.Sql)](https://www.nuget.org/packages/Obase.Providers.Sql)             | 适用于SQL数据库的Obase存储提供程序中间件.       |
| Obase.Providers.MySql     | [![latest version](https://img.shields.io/nuget/v/Obase.Providers.MySql)](https://www.nuget.org/packages/Obase.Providers.MySql)         | 适用于MySql数据库的Obase存储提供程序中间件.     |
| Obase.Providers.Oracle    | [![latest version](https://img.shields.io/nuget/v/Obase.Providers.Oracle)](https://www.nuget.org/packages/Obase.Providers.Oracle)       | 适用于Oracle数据库的Obase存储提供程序中间件.    |
| Obase.Providers.Sqlite    | [![latest version](https://img.shields.io/nuget/v/Obase.Providers.Sqlite)](https://www.nuget.org/packages/Obase.Providers.Sqlite)       | 适用于Sqlite数据库的Obase存储提供程序中间件.    |
| Obase.Providers.SqlServer | [![latest version](https://img.shields.io/nuget/v/Obase.Providers.SqlServer)](https://www.nuget.org/packages/Obase.Providers.SqlServer) | 适用于SqlServer数据库的Obase存储提供程序中间件. |
| Obase.Providers.PostgreSql | [![latest version](https://img.shields.io/nuget/v/Obase.Providers.PostgreSql)](https://www.nuget.org/packages/Obase.Providers.PostgreSql) | 适用于PostgreSql数据库的Obase存储提供程序中间件. |

# 如何使用

请参考[ObaseDoc](https://github.com/lechengruangong/ObaseDoc)项目.

# 引用的第三方软件包

本项目使用了以下第三方软件包:

### Obase.Core

- System.Reflection.Emit-4.7.0
- System.Reflection.Emit.ILGeneration-4.7.0
- System.Reflection.Emit.Lightweight-4.7.0

### Obase.Providers.Sql

- Microsoft.Extensions.Logging-8.0.0
- SafeObjectPool-3.0.0

### Obase.Providers.MySql

- MySql.Data-8.4.0

### Obase.Providers.Oracle

- Oracle.ManagedDataAccess.Core-2.19.22

### Obase.Providers.Sqlite

- Microsoft.Data.Sqlite-8.0.3

### Obase.Providers.SqlServer

- Microsoft.Data.SqlClient-5.2.2

### Obase.Providers.SqlServer

- npgsql-8.0.7

# 如何提出问题和需求

欢迎向我们提出Issue来协助我们改进,对应语言版本发现的问题请提交到对应的仓库.
