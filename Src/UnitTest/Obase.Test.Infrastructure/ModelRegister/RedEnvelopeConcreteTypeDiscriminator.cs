using System;
using Obase.Core;
using Obase.Core.Odm;
using Obase.Test.Domain.Association.Implement;

namespace Obase.Test.Infrastructure.ModelRegister;

/// <summary>
///     红包的具体类型选择器
/// </summary>
public class RedEnvelopeConcreteTypeDiscriminator : IConcreteTypeDiscriminator
{
    /// <summary>
    ///     上下文类型
    /// </summary>
    private readonly Type _contextType;

    /// <summary>
    ///     红包的具体类型选择器
    /// </summary>
    /// <param name="contextType">上下文类型</param>
    public RedEnvelopeConcreteTypeDiscriminator(Type contextType)
    {
        _contextType = contextType;
    }

    /// <summary>根据类型代码选择一个具体类型。</summary>
    /// <param name="typeCode">类型代码</param>
    public StructuralType Discriminate(object typeCode)
    {
        //这里的类型代码typeCode就是获取到的用于判别类型的值
        //这里我们规定1是InKindPrize 2是RedEnvelope 3是LuckyRedEnvelope

        //从模型里取具体的类型 此处获取模型的参数是此配置属于的上下文类型
        var redEnvelopeType = GlobalModelCache.Current.GetModel(_contextType).GetStructuralType(typeof(RedEnvelope));
        var luckyRedEnvelopeType =
            GlobalModelCache.Current.GetModel(_contextType).GetStructuralType(typeof(LuckyRedEnvelope));

        //处理参数
        if (typeCode == null)
            throw new ArgumentException("未能获取类型判别参数.");

        if (typeCode.ToString() == "2")
            return redEnvelopeType;

        if (typeCode.ToString() == "3")
            return luckyRedEnvelopeType;

        throw new ArgumentException($"未知的RedEnvelope类型判别参数值{typeCode}.");
    }
}