namespace Obase.Test.Domain.Association.ExplicitlyCompion;

/// <summary>
///     表示汽车的车轮
/// </summary>
public class CarWheel
{
    /// <summary>
    ///     汽车
    /// </summary>
    private Car _car;

    /// <summary>
    ///     汽车编号
    /// </summary>
    private string _carCode;

    /// <summary>
    ///     车轮
    /// </summary>
    private Wheel _wheel;

    /// <summary>
    ///     车轮编号
    /// </summary>
    private string _wheelCode;


    /// <summary>
    ///     车轮位置
    /// </summary>
    private WheelPosition _wheelPosition;

    /// <summary>
    ///     初始化汽车轮胎
    /// </summary>
    /// <param name="car">汽车</param>
    /// <param name="wheel">车轮</param>
    /// <param name="wheelPosition">位置</param>
    public CarWheel(Car car, Wheel wheel, WheelPosition wheelPosition)
    {
        _car = car;
        _carCode = car.CarCode;
        _wheel = wheel;
        _wheelCode = wheel.WheelCode;
        _wheelPosition = wheelPosition;
    }

    /// <summary>
    ///     反持久化构造函数
    /// </summary>
    protected CarWheel()
    {
    }

    /// <summary>
    ///     汽车
    /// </summary>
    public Car Car
    {
        get => _car;
        set => _car = value;
    }

    /// <summary>
    ///     车轮
    /// </summary>
    public Wheel Wheel
    {
        get => _wheel;
        set => _wheel = value;
    }

    /// <summary>
    ///     车轮位置
    /// </summary>
    public WheelPosition WheelPosition
    {
        get => _wheelPosition;
        set => _wheelPosition = value;
    }

    /// <summary>
    ///     汽车编号
    /// </summary>
    public string CarCode
    {
        get => _carCode;
        set => _carCode = value;
    }

    /// <summary>
    ///     车轮编号
    /// </summary>
    public string WheelCode
    {
        get => _wheelCode;
        set => _wheelCode = value;
    }
}