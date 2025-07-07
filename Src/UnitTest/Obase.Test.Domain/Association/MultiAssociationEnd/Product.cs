using System;
using System.Collections.Generic;

namespace Obase.Test.Domain.Association.MultiAssociationEnd;

/// <summary>
///     产品
/// </summary>
public class Product
{
    /// <summary>
    ///     产品编号
    /// </summary>
    private string _code;

    /// <summary>
    ///     产品名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     属性取值
    /// </summary>
    private List<PropertyTakingValue> _propertyTakingValues;

    /// <summary>
    ///     隐式的属性取值
    /// </summary>
    private List<Tuple<Property, PropertyValue>> _propertyValues;

    /// <summary>
    ///     产品编号
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     产品名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     属性取值
    /// </summary>
    public List<PropertyTakingValue> PropertyTakingValues
    {
        get => _propertyTakingValues;
        set => _propertyTakingValues = value;
    }

    /// <summary>
    ///     隐式的属性取值
    /// </summary>
    public virtual List<Tuple<Property, PropertyValue>> PropertyValues
    {
        get => _propertyValues;
        set => _propertyValues = value;
    }
}