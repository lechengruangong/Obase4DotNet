using System.Collections.Generic;
using System.Linq;

namespace Obase.Test.Domain.Association.ExplicitlyCompion;

/// <summary>
///     汽车
/// </summary>
public class Car
{
    /// <summary>
    ///     编号
    /// </summary>
    private string _carCode;

    /// <summary>
    ///     名称
    /// </summary>
    private string _carName;

    /// <summary>
    ///     汽车的车轮
    /// </summary>
    private List<CarWheel> _carWheels;

    /// <summary>
    ///     编号
    /// </summary>
    public string CarCode
    {
        get => _carCode;
        set => _carCode = value;
    }

    /// <summary>
    ///     名称
    /// </summary>
    public string CarName
    {
        get => _carName;
        set => _carName = value;
    }

    /// <summary>
    ///     汽车的车轮
    /// </summary>
    public List<CarWheel> CarWheels
    {
        get => _carWheels;
        set => _carWheels = value;
    }

    /// <summary>
    ///     根据车的车轮位置获取车轮
    /// </summary>
    /// <param name="wheelPosition">车轮位置</param>
    /// <returns>车轮</returns>
    public Wheel GetWheel(WheelPosition wheelPosition)
    {
        return _carWheels.First(p => p.WheelPosition == wheelPosition).Wheel;
    }
}