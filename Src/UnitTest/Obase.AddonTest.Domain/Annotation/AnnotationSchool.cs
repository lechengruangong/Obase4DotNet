using System;
using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.Annotation;

/// <summary>
///     标注建模测试用学校
/// </summary>
[Entity(keyAttributes: "SchoolId")]
public class AnnotationSchool
{
    /// <summary>
    ///     录入时间
    /// </summary>
    private DateTime _createTime;

    /// <summary>
    ///     办学时间
    /// </summary>
    private DateTime _establishmentTime;

    /// <summary>
    ///     是否为重点中学
    /// </summary>
    private bool _isPrime;

    /// <summary>
    ///     学校名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     学校ID
    /// </summary>
    private long _schoolId;

    /// <summary>
    ///     学校类型
    /// </summary>
    private ESchoolType _schoolType;

    /// <summary>
    ///     学校id
    /// </summary>
    public long SchoolId
    {
        get => _schoolId;
        protected internal set => _schoolId = value;
    }


    /// <summary>
    ///     学校名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     是否为重点中学
    /// </summary>
    public bool IsPrime
    {
        get => _isPrime;
        set => _isPrime = value;
    }

    /// <summary>
    ///     办学时间
    /// </summary>
    public DateTime EstablishmentTime
    {
        get => _establishmentTime;
        set => _establishmentTime = value;
    }

    /// <summary>
    ///     录入时间
    /// </summary>
    public DateTime Createtime
    {
        get => _createTime;
        set => _createTime = value;
    }

    /// <summary>
    ///     学校类型
    /// </summary>
    public ESchoolType SchoolType
    {
        get => _schoolType;
        set => _schoolType = value;
    }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"AnnotationSchool:{{SchoolId-{_schoolId},Name-\"{_name}\",IsPrime-\"{_isPrime}\",EstablishmentTime-\"{_establishmentTime:yyyy-MM-dd}\",Createtime-\"{_createTime:yyyy-MM-dd}\",SchoolType-\"{_schoolType}\"}}";
    }
}