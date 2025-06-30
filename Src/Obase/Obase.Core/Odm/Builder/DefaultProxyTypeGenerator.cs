/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的代理类型生成器,判断是否需要生成代理类型和生成代理类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:36:45
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     默认的代理类型生成器。
    /// </summary>
    public class DefaultProxyTypeGenerator : IProxyTypeGenerator
    {
        /// <summary>
        ///     构造默认的代理类型生成器
        /// </summary>
        /// <param name="next">下一节</param>
        public DefaultProxyTypeGenerator(IProxyTypeGenerator next)
        {
            Next = next;
        }

        /// <summary>
        ///     获取代理类型生成管道中的下一个生成器。
        /// </summary>
        public IProxyTypeGenerator Next { get; }

        /// <summary>
        ///     为指定类型的代理类型定义成员。
        /// </summary>
        /// <param name="typeBuilder">一个类型建造器，用于定义代理类型。</param>
        /// <param name="objType">要为其定义代理类的类型，即代理类的基类。</param>
        /// <param name="configurator">上述类型的配置器。</param>
        public void DefineMembers(TypeBuilder typeBuilder, ObjectType objType, IObjectTypeConfigurator configurator)
        {
            //如果是ObjectType 处理外键相关逻辑
            if (objType is ObjectType objectType)
            {
                //生成外键
                var foreignKeyAdder = new StructuralTypeConfiguration.ForeignKeyAdder(objectType, typeBuilder);
                foreignKeyAdder.Guarantee(objectType, null);
                if (configurator is StructuralTypeConfiguration structuralTypeConfiguration)
                    structuralTypeConfiguration.Adder = foreignKeyAdder;
            }

            //定义代理类相关的接口实现和介入者接口注入
            //主要就是定义IIntervener类型的_intervener字段
            //定义RegisterIntervener EnableLazyLoading方法
            var dic = new Dictionary<string, FieldBuilder>();
            const FieldAttributes fieldAttributes = FieldAttributes.Private;
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual;
            //定义一个IIntervener类型的字段，用于存储介入者
            var intervenerField = typeBuilder.DefineField("_intervener", typeof(IIntervener), fieldAttributes);
            var methodBuilder = typeBuilder.DefineMethod("RegisterIntervener", methodAttributes, null,
                new[] { typeof(IIntervener) });
            var ilR = methodBuilder.GetILGenerator();
            ilR.Emit(OpCodes.Ldarg, 0);
            ilR.Emit(OpCodes.Ldarg, 1);
            ilR.Emit(OpCodes.Stfld, intervenerField);
            ilR.Emit(OpCodes.Ret);
            //增加用于开关延迟加载的字段和方法
            var forbidLazyLoading1 = typeBuilder.DefineField("_forbidLazyLoading", typeof(bool), fieldAttributes);
            var forbidLazyLoading2 =
                typeBuilder.DefineMethod("ForbidLazyLoading", methodAttributes, null, Type.EmptyTypes);
            var forbidLazyLoadingIl = forbidLazyLoading2.GetILGenerator();
            forbidLazyLoadingIl.Emit(OpCodes.Ldarg_0);
            forbidLazyLoadingIl.Emit(OpCodes.Ldc_I4_1);
            forbidLazyLoadingIl.Emit(OpCodes.Stfld, forbidLazyLoading1);
            forbidLazyLoadingIl.Emit(OpCodes.Ret);
            var enableLazyLoading =
                typeBuilder.DefineMethod("EnableLazyLoading", methodAttributes, null, Type.EmptyTypes);
            var enableLazyLoadingIl = enableLazyLoading.GetILGenerator();
            enableLazyLoadingIl.Emit(OpCodes.Ldarg_0);
            enableLazyLoadingIl.Emit(OpCodes.Ldc_I4_0);
            enableLazyLoadingIl.Emit(OpCodes.Stfld, forbidLazyLoading1);
            enableLazyLoadingIl.Emit(OpCodes.Ret);

            //遍历触发器重写属性或方法
            foreach (var tri in configurator.BehaviorTriggers)
            {
                var meb = tri.Override(typeBuilder);
                var il = meb.GetILGenerator();
                var elements = configurator.GetBehaviorElements(tri).Cast<TypeElementConfiguration>().ToList();
                foreach (var elem in elements)

                    if (elem is ILazyLoadingConfiguration re && re.EnableLazyLoading)
                    {
                        //增加延迟加载是否已被触发的字段
                        FieldBuilder hasCalled;
                        if (dic.ContainsKey("_" + elem + "HasCalled"))
                        {
                            hasCalled = dic["_" + elem.Name + "HasCalled"];
                        }
                        else
                        {
                            hasCalled = typeBuilder.DefineField("_" + elem.Name + "HasCalled", typeof(bool),
                                fieldAttributes);
                            dic["_" + elem.Name + "HasCalled"] = hasCalled;
                        }

                        //关联引用 先调用LoadAssociation再调用原方法
                        var label = il.DefineLabel();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, intervenerField);
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Cgt_Un);
                        il.Emit(OpCodes.Brfalse_S, label);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, forbidLazyLoading1);

                        il.Emit(OpCodes.Brtrue_S, label);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, hasCalled);
                        il.Emit(OpCodes.Brtrue_S, label);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, intervenerField);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldstr, elem.Name);
                        il.Emit(OpCodes.Callvirt,
                            typeof(IIntervener).GetMethod("LoadAssociation",
                                new[] { typeof(object), typeof(string) }) ??
                            throw new InvalidOperationException("介入者接口定义错误"));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Stfld, hasCalled);
                        il.MarkLabel(label);
                    }

                tri.CallBase(il);
                foreach (var elem in elements)
                    if (elem.ElementType == EElementType.Attribute)
                    {
                        //属性 则先调用AttributeChanged方法 再调用原方法
                        var label = il.DefineLabel();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, intervenerField);
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Cgt_Un);
                        il.Emit(OpCodes.Brfalse_S, label);

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, intervenerField);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldstr, elem.Name);
                        il.Emit(OpCodes.Callvirt,
                            typeof(IIntervener).GetMethod("AttributeChanged",
                                new[] { typeof(object), typeof(string) }) ??
                            throw new InvalidOperationException("介入者接口定义错误"));
                        il.MarkLabel(label);
                    }

                il.Emit(OpCodes.Ret);
            }

            //处理继承关系的代理
            var signAttr = Utils.GetDerivedConcreteTypeSign(objType);
            if (signAttr != null)
                //生成一个字段obase_gen_ct 作为补充管道属性的承载
                typeBuilder.DefineField("obase_gen_ct", signAttr.Item2.GetType(), FieldAttributes.Public);
        }

        /// <summary>
        ///     判定指定的类型是否需要生成代理类型。
        /// </summary>
        /// <param name="objType">要判定的类型。</param>
        /// <param name="configurator">上述类型的配置器。</param>
        public bool Should(ObjectType objType, IObjectTypeConfigurator configurator)
        {
            //获取定义的外键
            var attrs = Utils.GetDefinedForeignAttributes(objType, null, out _);
            //获取顶级父类的类型判别标记对应的属性
            var signAttr = Utils.GetDerivedConcreteTypeSign(objType);
            //默认的判断条件 1.有触发器 2.有定义的外键 3.没有标记属性
            return configurator.BehaviorTriggers.Count > 0 || attrs.Count > 0 || signAttr != null;
        }
    }
}