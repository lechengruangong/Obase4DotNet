using System;

namespace Obase.Test.Domain.Functional.DependencyInjection;

/// <summary>
///     服务A
/// </summary>
public class ServiceSa : IServiceS
{
    /// <summary>
    ///     初始化服务A
    /// </summary>
    public ServiceSa()
    {
        CreateTime = DateTime.Now;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceA";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务B
/// </summary>
public class ServiceSb : IServiceS
{
    /// <summary>
    ///     初始化服务B
    /// </summary>
    public ServiceSb()
    {
        CreateTime = DateTime.Now;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceB";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务C
/// </summary>
public class ServiceSc : IServiceS
{
    /// <summary>
    ///     初始化服务C
    /// </summary>
    public ServiceSc()
    {
        CreateTime = new DateTime(1999, 12, 31);
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceC";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务D
/// </summary>
public class ServiceSd : IServiceS
{
    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceD";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime => new(1999, 12, 31);
}

/// <summary>
///     服务E
/// </summary>
public class ServiceSe : IServiceS
{
    /// <summary>
    ///     初始化服务E
    /// </summary>
    /// <param name="service">服务D</param>
    public ServiceSe(ServiceSd service)
    {
        CreateTime = service.CreateTime;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceE";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务F
/// </summary>
public class ServiceSf : IServiceS
{
    /// <summary>
    ///     初始化服务F
    /// </summary>
    /// <param name="dateTime">创建时间</param>
    public ServiceSf(DateTime dateTime)
    {
        CreateTime = dateTime;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceF";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务G
/// </summary>
public class ServiceSg : IServiceS
{
    /// <summary>
    ///     初始化服务G
    /// </summary>
    /// <param name="dateTime">创建时间</param>
    public ServiceSg(DateTime dateTime)
    {
        CreateTime = dateTime;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceG";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务H
/// </summary>
public class ServiceSh : IServiceSo
{
    /// <summary>
    ///     初始化服务H
    /// </summary>
    /// <param name="dateTime">创建时间</param>
    public ServiceSh(DateTime dateTime)
    {
        CreateTime = dateTime;
    }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务A
/// </summary>
public class ServiceTa : IServiceT
{
    /// <summary>
    ///     初始化服务A
    /// </summary>
    public ServiceTa()
    {
        CreateTime = DateTime.Now;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceA";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务B
/// </summary>
public class ServiceTb : IServiceT
{
    /// <summary>
    ///     初始化服务B
    /// </summary>
    public ServiceTb()
    {
        CreateTime = DateTime.Now;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceB";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务C
/// </summary>
public class ServiceTc : IServiceT
{
    /// <summary>
    ///     初始化服务C
    /// </summary>
    public ServiceTc()
    {
        CreateTime = new DateTime(1999, 12, 31);
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceC";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务D
/// </summary>
public class ServiceTd : IServiceT
{
    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceD";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime => new(1999, 12, 31);
}

/// <summary>
///     服务E
/// </summary>
public class ServiceTe : IServiceT
{
    /// <summary>
    ///     初始化服务E
    /// </summary>
    /// <param name="service">服务D</param>
    public ServiceTe(ServiceTd service)
    {
        CreateTime = service.CreateTime;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceE";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务F
/// </summary>
public class ServiceTf : IServiceT
{
    /// <summary>
    ///     初始化服务F
    /// </summary>
    /// <param name="dateTime">创建时间</param>
    public ServiceTf(DateTime dateTime)
    {
        CreateTime = dateTime;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceF";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务G
/// </summary>
public class ServiceTg : IServiceT
{
    /// <summary>
    ///     初始化服务G
    /// </summary>
    /// <param name="dateTime">创建时间</param>
    public ServiceTg(DateTime dateTime)
    {
        CreateTime = dateTime;
    }

    /// <summary>
    ///     服务的Code
    /// </summary>
    public string Code => "ServiceG";

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}

/// <summary>
///     服务H
/// </summary>
public class ServiceTh : IServiceTo
{
    /// <summary>
    ///     初始化服务H
    /// </summary>
    /// <param name="dateTime">创建时间</param>
    public ServiceTh(DateTime dateTime)
    {
        CreateTime = dateTime;
    }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; }
}