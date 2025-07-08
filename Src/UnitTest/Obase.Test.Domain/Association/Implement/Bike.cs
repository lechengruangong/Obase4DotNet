using System.Collections.Generic;

namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     表示自行车
/// </summary>
public class Bike
{
    /// <summary>
    ///     自行车编码
    /// </summary>
    private string _code;

    /// <summary>
    ///     自行车灯
    /// </summary>
    private BikeLight _light;

    /// <summary>
    ///     车灯编码
    /// </summary>
    private string _lightCode;

    /// <summary>
    ///     自行车名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     1-普通车 2-MyBikeA 3-MyBikeB
    /// </summary>
    private int _type;

    /// <summary>
    ///     自行车轮
    /// </summary>
    private List<BikeWheel> _wheels;

    /// <summary>
    ///     自行车编码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     自行车名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     自行车灯
    /// </summary>
    public virtual BikeLight Light
    {
        get => _light;
        set => _light = value;
    }

    /// <summary>
    ///     自行车轮
    /// </summary>
    public virtual List<BikeWheel> Wheels
    {
        get => _wheels;
        set => _wheels = value;
    }

    /// <summary>
    ///     车灯编码
    /// </summary>
    public string LightCode
    {
        get => _lightCode;
        set => _lightCode = value;
    }

    /// <summary>
    ///     1-普通车 2-MyBikeA 3-MyBikeB
    /// </summary>
    public int Type
    {
        get => _type;
        protected internal set => _type = value;
    }
}