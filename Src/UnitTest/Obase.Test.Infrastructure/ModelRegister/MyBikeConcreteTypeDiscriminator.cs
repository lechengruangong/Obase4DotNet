using System;
using Obase.Core;
using Obase.Core.Odm;
using Obase.Test.Domain.Association.Implement;

namespace Obase.Test.Infrastructure.ModelRegister;

/// <summary>
///     我的自行车类型选择器
/// </summary>
public class MyBikeConcreteTypeDiscriminator : IConcreteTypeDiscriminator
{
    /// <summary>
    ///     上下文类型
    /// </summary>
    private readonly Type _contextType;

    /// <summary>
    ///     自行车类型选择器
    /// </summary>
    /// <param name="contextType">上下文类型</param>
    public MyBikeConcreteTypeDiscriminator(Type contextType)
    {
        _contextType = contextType;
    }

    /// <summary>根据类型代码选择一个具体类型。</summary>
    /// <param name="typeCode">类型代码</param>
    public StructuralType Discriminate(object typeCode)
    {
        //这里的类型代码typeCode就是获取到的用于判别类型的值
        //这里我们规定 2是MyBikeA 4是myBikeC

        //从模型里取具体的类型 此处获取模型的参数是此配置属于的上下文类型
        var myBikeAType = GlobalModelCache.Current.GetModel(_contextType).GetStructuralType(typeof(MyBikeA));
        var myBikeCType = GlobalModelCache.Current.GetModel(_contextType).GetStructuralType(typeof(MyBikeC));

        //处理参数
        if (typeCode == null)
            throw new ArgumentException("未能获取类型判别参数.");

        if (typeCode.ToString() == "2")
            return myBikeAType;

        if (typeCode.ToString() == "4")
            return myBikeCType;

        throw new ArgumentException($"未知的MyBike类型判别参数值{typeCode}.");
    }
}