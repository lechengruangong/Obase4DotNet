using System;

namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     某种身份
/// </summary>
public class Identity
{
    /// <summary>
    ///     创建时间
    /// </summary>
    private DateTime _createTime;

    /// <summary>
    ///     身份标识
    /// </summary>
    private Guid _id;

    /// <summary>
    ///     查询时间
    /// </summary>
    private DateTime _queryTime;

    /// <summary>
    ///     角色
    /// </summary>
    private string _role;

    /// <summary>
    ///     初始化某种身份
    /// </summary>
    /// <param name="id">身份标识</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="role">角色</param>
    public Identity(Guid id, DateTime createTime, string role)
    {
        _id = id;
        _createTime = createTime;
        _role = role;
        _queryTime = DateTime.Now;
    }

    /// <summary>
    ///     反序列化函数
    /// </summary>
    /// <param name="id">身份标识</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="role">角色</param>
    /// <param name="queryTime">查询时间</param>
    protected internal Identity(Guid id, DateTime createTime, string role, DateTime queryTime)
    {
        _id = id;
        _createTime = createTime;
        _role = role;
        _queryTime = queryTime;
    }

    /// <summary>
    ///     身份标识
    /// </summary>
    public Guid Id
    {
        get => _id;
        protected internal set => _id = value;
    }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime
    {
        get => _createTime;
        protected internal set => _createTime = value;
    }

    /// <summary>
    ///     角色
    /// </summary>
    public string Role
    {
        get => _role;
        protected internal set => _role = value;
    }

    /// <summary>
    ///     查询时间
    /// </summary>
    public DateTime QueryTime
    {
        get => _queryTime;
        protected internal set => _queryTime = value;
    }

    /// <summary>
    ///     版本
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    ///     次版本
    /// </summary>
    public int SubVersion { get; set; }

    /// <summary>
    ///     返回字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"{nameof(_createTime)}: {_createTime}, {nameof(_id)}: {_id}, {nameof(_queryTime)}: {_queryTime}, {nameof(_role)}: {_role}";
    }
}