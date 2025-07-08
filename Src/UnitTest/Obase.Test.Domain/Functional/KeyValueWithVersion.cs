namespace Obase.Test.Domain.Functional;

/// <summary>
///     带有版本键 键值对
/// </summary>
public abstract class KeyValueWithVersion
{
    /// <summary>
    ///     唯一标识
    /// </summary>
    private int _id;

    /// <summary>
    ///     键
    /// </summary>
    private string _key;

    /// <summary>
    ///     值
    /// </summary>
    private int _value;

    /// <summary>
    ///     版本键
    /// </summary>
    private int _versionKey;

    /// <summary>
    ///     键
    /// </summary>
    public string Key
    {
        get => _key;
        set => _key = value;
    }

    /// <summary>
    ///     值
    /// </summary>
    public int Value
    {
        get => _value;
        set => _value = value;
    }

    /// <summary>
    ///     版本键
    /// </summary>
    public int VersionKey
    {
        get => _versionKey;
        set => _versionKey = value;
    }

    /// <summary>
    ///     唯一标识
    /// </summary>
    public int Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"KeyValueWithVersion:{{Id-{_id},Key-\"{_key}\",Value-{_value},VersionKey-{_versionKey}}}";
    }
}

/// <summary>
///     用于测试忽略并发策略的简单属性类
/// </summary>
public class IngoreKeyValue : KeyValueWithVersion
{
}

/// <summary>
///     用于测试抛出异常并发策略的简单属性类
/// </summary>
public class ThrowExceptionKeyValue : KeyValueWithVersion
{
}

/// <summary>
///     用于测试覆盖并发策略的简单属性类
/// </summary>
public class OverwriteKeyValue : KeyValueWithVersion
{
}

/// <summary>
///     累加合并策略的简单类
/// </summary>
public class AccumulateCombineKeyValue : KeyValueWithVersion
{
}

/// <summary>
///     忽略合并策略的简单类
/// </summary>
public class IgnoreCombineKeyValue : KeyValueWithVersion
{
}

/// <summary>
///     覆盖合并策略的简单类
/// </summary>
public class OverwriteCombineKeyValue : KeyValueWithVersion
{
}

/// <summary>
///     用于测试重建对象并发策略的简单属性类
/// </summary>
public class ReconstructKeyValue : KeyValueWithVersion
{
}

/// <summary>
///     带有版本键 将键值对视作复杂属性
/// </summary>
public abstract class ComplexKeyValueWithVersion
{
    /// <summary>
    ///     唯一标识
    /// </summary>
    private int _id;

    /// <summary>
    ///     版本键
    /// </summary>
    private int _versionKey;

    /// <summary>
    ///     唯一标识
    /// </summary>
    public int Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    ///     版本键
    /// </summary>
    public int VersionKey
    {
        get => _versionKey;
        set => _versionKey = value;
    }

    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"KeyValueWithVersion:{{Id-{_id},VersionKey-{_versionKey}}}";
    }
}

/// <summary>
///     作为复杂属性的键值对
/// </summary>
public struct ComplexKeyValue
{
    /// <summary>
    ///     键
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     值
    /// </summary>
    public int Value { get; set; }
}

/// <summary>
///     累加合并的复杂属性
/// </summary>
public struct AccumulateCombineComplexKeyValue
{
    /// <summary>
    ///     键
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     值
    /// </summary>
    public int Value { get; set; }
}

/// <summary>
///     忽略合并的复杂属性
/// </summary>
public struct IgnoreCombineComplexKeyValue
{
    /// <summary>
    ///     键
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     值
    /// </summary>
    public int Value { get; set; }
}

/// <summary>
///     覆盖合并的复杂属性
/// </summary>
public struct OverWriteCombineComplexKeyValue
{
    /// <summary>
    ///     键
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     值
    /// </summary>
    public int Value { get; set; }
}

/// <summary>
///     用于测试忽略并发策略的复杂属性类
/// </summary>
public class ComplexIngoreKeyValue : ComplexKeyValueWithVersion
{
    /// <summary>
    ///     键值对
    /// </summary>
    private ComplexKeyValue _keyValue;

    /// <summary>
    ///     键值对
    /// </summary>
    public ComplexKeyValue KeyValue
    {
        get => _keyValue;
        set => _keyValue = value;
    }


    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"KeyValueWithVersion:{{Id-{Id},Key-\"{_keyValue.Key}\",Value-{_keyValue.Value},VersionKey-{VersionKey}}}";
    }
}

/// <summary>
///     用于测试抛出异常并发策略的简单属性类
/// </summary>
public class ComplexThrowExceptionKeyValue : ComplexKeyValueWithVersion
{
    /// <summary>
    ///     键值对
    /// </summary>
    private ComplexKeyValue _keyValue;

    /// <summary>
    ///     键值对
    /// </summary>
    public ComplexKeyValue KeyValue
    {
        get => _keyValue;
        set => _keyValue = value;
    }


    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"KeyValueWithVersion:{{Id-{Id},Key-\"{_keyValue.Key}\",Value-{_keyValue.Value},VersionKey-{VersionKey}}}";
    }
}

/// <summary>
///     用于测试覆盖并发策略的简单属性类
/// </summary>
public class ComplexOverwriteKeyValue : ComplexKeyValueWithVersion
{
    /// <summary>
    ///     键值对
    /// </summary>
    private ComplexKeyValue _keyValue;

    /// <summary>
    ///     键值对
    /// </summary>
    public ComplexKeyValue KeyValue
    {
        get => _keyValue;
        set => _keyValue = value;
    }


    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"KeyValueWithVersion:{{Id-{Id},Key-\"{_keyValue.Key}\",Value-{_keyValue.Value},VersionKey-{VersionKey}}}";
    }
}

/// <summary>
///     累加合并策略的复杂类
/// </summary>
public class ComplexAccumulateCombineKeyValue : ComplexKeyValueWithVersion
{
    /// <summary>
    ///     键值对
    /// </summary>
    private AccumulateCombineComplexKeyValue _keyValue;

    /// <summary>
    ///     键值对
    /// </summary>
    public AccumulateCombineComplexKeyValue KeyValue
    {
        get => _keyValue;
        set => _keyValue = value;
    }


    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"KeyValueWithVersion:{{Id-{Id},Key-\"{_keyValue.Key}\",Value-{_keyValue.Value},VersionKey-{VersionKey}}}";
    }
}

/// <summary>
///     忽略合并策略的简单类
/// </summary>
public class ComplexIgnoreCombineKeyValue : ComplexKeyValueWithVersion
{
    /// <summary>
    ///     键值对
    /// </summary>
    private IgnoreCombineComplexKeyValue _keyValue;

    /// <summary>
    ///     键值对
    /// </summary>
    public IgnoreCombineComplexKeyValue KeyValue
    {
        get => _keyValue;
        set => _keyValue = value;
    }


    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"KeyValueWithVersion:{{Id-{Id},Key-\"{_keyValue.Key}\",Value-{_keyValue.Value},VersionKey-{VersionKey}}}";
    }
}

/// <summary>
///     覆盖合并策略的简单类
/// </summary>
public class ComplexOverwriteCombineKeyValue : ComplexKeyValueWithVersion
{
    /// <summary>
    ///     键值对
    /// </summary>
    private OverWriteCombineComplexKeyValue _keyValue;

    /// <summary>
    ///     键值对
    /// </summary>
    public OverWriteCombineComplexKeyValue KeyValue
    {
        get => _keyValue;
        set => _keyValue = value;
    }


    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"KeyValueWithVersion:{{Id-{Id},Key-\"{_keyValue.Key}\",Value-{_keyValue.Value},VersionKey-{VersionKey}}}";
    }
}

/// <summary>
///     用于测试重建对象并发策略的简单属性类
/// </summary>
public class ComplexReconstructKeyValue : ComplexKeyValueWithVersion
{
    /// <summary>
    ///     键值对
    /// </summary>
    private ComplexKeyValue _keyValue;

    /// <summary>
    ///     键值对
    /// </summary>
    public ComplexKeyValue KeyValue
    {
        get => _keyValue;
        set => _keyValue = value;
    }


    /// <summary>
    ///     返回字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"KeyValueWithVersion:{{Id-{Id},Key-\"{_keyValue.Key}\",Value-{_keyValue.Value},VersionKey-{VersionKey}}}";
    }
}