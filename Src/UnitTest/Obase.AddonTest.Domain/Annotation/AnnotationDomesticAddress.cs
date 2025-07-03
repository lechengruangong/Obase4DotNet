using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.Annotation;

/// <summary>
///     标注建模测试用国内地址
/// </summary>
[Entity("", false, "Key")]
public class AnnotationDomesticAddress
{
    /// <summary>
    ///     市
    /// </summary>
    private AnnotationCity _city;

    /// <summary>
    ///     详细地址
    /// </summary>
    private string _detailAdress;

    /// <summary>
    ///     地址的键
    /// </summary>
    private string _key;

    /// <summary>
    ///     省/直辖市
    /// </summary>
    private AnnotationProvince _province;

    /// <summary>
    ///     区/县
    /// </summary>
    private AnnotationRegion _region;

    /// <summary>
    ///     地址的键
    /// </summary>
    public string Key
    {
        get => _key;
        set => _key = value;
    }

    /// <summary>
    ///     省/直辖市
    /// </summary>
    public AnnotationProvince Province
    {
        get => _province;
        set => _province = value;
    }

    /// <summary>
    ///     市
    /// </summary>
    public AnnotationCity City
    {
        get => _city;
        set => _city = value;
    }

    /// <summary>
    ///     区/县
    /// </summary>
    public AnnotationRegion Region
    {
        get => _region;
        set => _region = value;
    }

    /// <summary>
    ///     详细地址
    /// </summary>
    public string DetailAdress
    {
        get => _detailAdress;
        set => _detailAdress = value;
    }

    /// <summary>
    ///     转换为字符串表示
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"DomesticAdress:{{Key-\"{_key}\",Province-{{Name-\"{_province.Name}\",Code-{_province.Code}}},City-{{Name-\"{_city.Name}\",Code-{_city.Code}}},Region-{{Name-\"{_region.Name}\",Code-{_region.Code}}},DetailAdress-\"{_detailAdress}\"}}";
    }
}

/// <summary>
///     标注建模测试用省级行政区划
/// </summary>
[Complex]
public struct AnnotationProvince
{
    /// <summary>
    ///     省名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     省代码
    /// </summary>
    public int Code { get; set; }
}

/// <summary>
///     标注建模测试用市级行政区划
/// </summary>
[Complex]
public struct AnnotationCity
{
    /// <summary>
    ///     市名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     市代码
    /// </summary>
    public int Code { get; set; }
}

/// <summary>
///     标注建模测试用区/县级行政区划
/// </summary>
[Complex]
public struct AnnotationRegion
{
    /// <summary>
    ///     区/县名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     区/县代码
    /// </summary>
    public int Code { get; set; }
}