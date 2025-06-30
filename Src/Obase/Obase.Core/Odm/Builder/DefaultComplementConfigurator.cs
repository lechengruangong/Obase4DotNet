/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的补充配置器,补充关联引用左右端和继承相关配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:36:33
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     默认的补充配置器
    /// </summary>
    public class DefaultComplementConfigurator : IComplementConfigurator
    {
        /// <summary>
        ///     构造默认的补充配置器
        /// </summary>
        /// <param name="next">下一节</param>
        public DefaultComplementConfigurator(IComplementConfigurator next)
        {
            Next = next;
        }

        /// <summary>
        ///     补充配置管道中的下一个配置器。
        /// </summary>
        public IComplementConfigurator Next { get; }

        /// <summary>
        ///     根据类型配置项中的元数据配置指定的类型。
        /// </summary>
        /// <param name="targetType">要配置的类型。</param>
        /// <param name="configuration">包含配置元数据的类型配置项。</param>
        public void Configurate(StructuralType targetType, StructuralTypeConfiguration configuration)
        {
            //配置关联引用的右端
            if (targetType is EntityType entity)
                foreach (var referenceElement in entity.ReferenceElements)
                    if (referenceElement is AssociationReference associationReference)
                    {
                        var associationType = associationReference.AssociationType;
                        //如果当前的关联引用只有两个端的才需要配置
                        if (associationType.AssociationEnds.Count == 2)
                            //左端一般都会侦测配置 尝试配置右端即可
                            //只配置有左端没右端的即可
                            if (!string.IsNullOrEmpty(associationReference.LeftEnd) &&
                                string.IsNullOrEmpty(associationReference.RightEnd))
                                associationReference.RightEnd = associationType.AssociationEnds
                                    .FirstOrDefault(p => p.Name != associationReference.LeftEnd)?.Name;
                    }

            //是个基类 补充自己的具体类型属性和继承类的虚拟属性
            if (targetType.DerivedTypes.Count > 0)
            {
                //增加自己的具体类型虚拟属性
                AddConcreteTypeAttr(targetType);

                //将子类中与自己不同的属性补进来
                var needAdded = new List<Attribute>();
                CreateDerivedTypeAttr(targetType, needAdded);

                //补充虚拟属性
                foreach (var add in needAdded)
                    //这些虚拟属性实际上不会用到 只是为了查询
                    targetType.AddAttribute(new Attribute(add.DataType, add.Name)
                    {
                        TargetField = add.TargetField,
                        Nullable = add.Nullable,
                        //所以这些设值器和取值器都是空值 不会真正的设置值或取出值
                        ValueSetter = new ConcreteTypeSignValueSetter(new Dictionary<Type, object>(),
                            new Dictionary<Type, object>(), null),
                        ValueGetter = new ConcreteTypeSignValueGetter(new Dictionary<Type, object>(),
                            new Dictionary<Type, object>())
                    });
            }

            //是继承类 补充自己的具体类型属
            if (targetType.DerivingFrom != null)
                AddConcreteTypeAttr(targetType, false);
        }

        /// <summary>
        ///     创建继承类在父类中的虚拟属性
        /// </summary>
        /// <param name="targetType">目标类型</param>
        /// <param name="needAdded">需要增加的虚拟属性</param>
        private void CreateDerivedTypeAttr(StructuralType targetType, List<Attribute> needAdded)
        {
            //循环所有继承类
            foreach (var derivedType in targetType.DerivedTypes)
            {
                //添加不是从父类继承而来的属性
                foreach (var attr in derivedType.Attributes)
                    if (targetType.Attributes.All(p => p.TargetField != attr.TargetField))
                        needAdded.Add(attr);
                //递归处理子类
                CreateDerivedTypeAttr(derivedType, needAdded);
            }
        }

        /// <summary>
        ///     增加自己的具体类型虚拟属性
        /// </summary>
        /// <param name="targetType">目标类型</param>
        /// <param name="isBase">是否只包装基类</param>
        private void AddConcreteTypeAttr(StructuralType targetType, bool isBase = true)
        {
            //取自己的类型判别标记
            var sign = targetType.ConcreteTypeSign;
            //没配置 就不处理 之后会检查出来
            if (sign != null)
            {
                //获取继承链 构造字典
                var dict1 = new Dictionary<Type, object>();
                var dict2 = new Dictionary<Type, object>();
                var chains = Utils.GetDerivingChain(targetType);
                //沿着继承链处理每一级
                foreach (var structural in chains)
                {
                    CreateConcreteTypeSignValueSetterDict(structural, dict1, dict2);
                    foreach (var derivedType1 in structural.DerivedTypes)
                    {
                        CreateConcreteTypeSignValueSetterDict(derivedType1, dict1, dict2);
                        foreach (var derivedType2 in derivedType1.DerivedTypes)
                            CreateConcreteTypeSignValueSetterDict(derivedType2, dict1, dict2);
                    }
                }

                var attribute = targetType.FindAttributeByTargetField(sign.Item1);
                //如果这个属性没有定义
                if (attribute == null)
                {
                    //补充一个类型判别的虚拟属性 固定为obase_gen_ct
                    targetType.AddAttribute(new Attribute(sign.Item2.GetType(), "obase_gen_ct")
                    {
                        TargetField = sign.Item1,
                        Nullable = false,
                        ValueSetter = new ConcreteTypeSignValueSetter(dict1, dict2,
                            new ConcreteTypeSignFiledSetter("obase_gen_ct")),
                        ValueGetter = new ConcreteTypeSignValueGetter(dict1, dict2)
                    });
                }
                else
                {
                    //定义了 只包装基类的
                    if (isBase)
                    {
                        //包装已有的属性即可
                        attribute.ValueSetter = new ConcreteTypeSignValueSetter(dict1, dict2, attribute.ValueSetter);
                        attribute.ValueGetter = new ConcreteTypeSignValueGetter(dict1, dict2);
                        attribute.Nullable = false;
                    }
                }
            }
        }

        /// <summary>
        ///     组合ConcreteTypeSignValueSetter的字典参数
        /// </summary>
        /// <param name="targetType">目标类型</param>
        /// <param name="dict1">代理类的类型和具体类型判别标识的映射</param>
        /// <param name="dict2">原类的类型和具体类型判别标识的映射</param>
        private void CreateConcreteTypeSignValueSetterDict(StructuralType targetType, Dictionary<Type, object> dict1,
            Dictionary<Type, object> dict2)
        {
            //保存代理类的类型和具体类型判别标识的映射
            if (!dict1.ContainsKey(targetType.RebuildingType) && targetType.ConcreteTypeSign != null)
                dict1.Add(targetType.RebuildingType, targetType.ConcreteTypeSign.Item2);
            //保存原类的类型和具体类型判别标识的映射
            if (!dict2.ContainsKey(targetType.ClrType) && targetType.ConcreteTypeSign != null)
                dict2.Add(targetType.ClrType, targetType.ConcreteTypeSign.Item2);
        }
    }
}