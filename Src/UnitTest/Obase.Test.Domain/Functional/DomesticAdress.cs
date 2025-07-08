namespace Obase.Test.Domain.Functional;

/// <summary>
///     国内地址
/// </summary>
public class DomesticAddress
{
    /// <summary>
    ///     市
    /// </summary>
    private City _city;

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
    private Province _province;

    /// <summary>
    ///     区/县
    /// </summary>
    private Region _region;

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
    public Province Province
    {
        get => _province;
        set => _province = value;
    }

    /// <summary>
    ///     市
    /// </summary>
    public City City
    {
        get => _city;
        set => _city = value;
    }

    /// <summary>
    ///     区/县
    /// </summary>
    public Region Region
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
///     省级行政区划
/// </summary>
public struct Province
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
///     市级行政区划
/// </summary>
public struct City
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
///     区/县级行政区划
/// </summary>
public struct Region
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