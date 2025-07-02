/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：隐式关联型的Clr类型管理器,存放所有创建的隐式关联型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 11:29:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     隐式关联型的Clr类型管理器
    /// </summary>
    public class ImplicitAssociationManager
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     接受管理的隐式关联型的Clr类型
        /// </summary>
        private readonly HashSet<Type> _impliedTypes = new HashSet<Type>();

        /// <summary>
        ///     作为隐含类型宿主的模块，提供在其中定义类型的方法。
        /// </summary>
        private readonly ModuleBuilder _module = Utils.InitModuleBuilder();

        /// <summary>
        ///     命名计数器，用于在命名过程中累加计数，避免命名重复。
        /// </summary>
        private int _namingCounter;

        /// <summary>
        ///     创建隐式关联型的Clr类型管理器。
        /// </summary>
        private ImplicitAssociationManager()
        {
        }

        /// <summary>
        ///     获取当前应用程序域中唯一的隐含类型管理器实例。
        ///     实施说明
        ///     需要解决并发冲突，保证在并发场景下实现全局唯一性。
        /// </summary>
        public static ImplicitAssociationManager Current { get; } = new ImplicitAssociationManager();

        /// <summary>
        ///     接受管理的隐式关联型的Clr类型
        /// </summary>
        public HashSet<Type> ImpliedTypes => _impliedTypes;

        /// <summary>
        ///     获取一个动态创建的隐式关联型Clr类型。
        /// </summary>
        /// <param name="fields">字段描述</param>
        /// <param name="fullName">全名</param>
        /// <returns></returns>
        public Type ApplyType(FieldDescriptor[] fields, string fullName)
        {
            return SearchOrDefineType(fullName, fields);
        }

        /// <summary>
        ///     以指定的标识搜索隐含类型，如果未找到则根据指定的内容创建隐式关联型Clr类型。
        /// </summary>
        /// <param name="fullName">全名</param>
        /// <param name="fields">要定义的字段。</param>
        private Type SearchOrDefineType(string fullName, FieldDescriptor[] fields)
        {
            //命名
            var name = $"{fullName}<>{++_namingCounter}";
            ReaderWriterLock.EnterWriteLock();
            //定义一个新类型
            var type = DefineType(name, fields);
            _impliedTypes.Add(type);
            ReaderWriterLock.ExitWriteLock();
            return type;
        }


        /// <summary>
        ///     根据指定的内容定义隐式关联型Clr类型。
        /// </summary>
        /// <param name="name">类型名称。</param>
        /// <param name="fields">类型实现的接口。</param>
        private Type DefineType(string name, FieldDescriptor[] fields = null)
        {
            //目标类型
            var targetType =
                _module.DefineType(name, TypeAttributes.Public, typeof(ImplicitAssociation));

            //有字段
            if (fields != null)
            {
                var construstorParameter = new Dictionary<string, FieldBuilder>();
                //命名计数器
                var i = 0;
                foreach (var field in fields)
                {
                    //构造一个委托 用于生成字段
                    var namingFunc = new Func<string>(() =>
                    {
                        //字段前半部分
                        var filedStart = field.HasGetter || field.HasSetter ? "_field_" : "Field_";
                        return $"{filedStart}{++i}";
                    });
                    //名称
                    var filedName = field.GetName(namingFunc);
                    //类型
                    var filedType = field.Type;
                    //设值取值方法
                    var typeAttr = field.HasGetter || field.HasSetter
                        ? FieldAttributes.Private
                        : FieldAttributes.Public;
                    var fieldBuilder = targetType.DefineField(filedName, filedType, typeAttr);
                    //有取值/设值器
                    if (field.HasSetter || field.HasGetter)
                    {
                        //定一个属性访问器
                        var propName = field.GetPropertyName();
                        var propBuilder = targetType.DefineProperty(propName, PropertyAttributes.None, filedType,
                            Type.EmptyTypes);
                        //取值方法
                        if (field.HasGetter)
                        {
                            var getterBuilder = targetType.DefineMethod($"get_{propName}",
                                field.PublicGetter
                                    ? MethodAttributes.Public | MethodAttributes.Virtual
                                    : MethodAttributes.Assembly,
                                filedType, Type.EmptyTypes);
                            var getterIl = getterBuilder.GetILGenerator();
                            getterIl.Emit(OpCodes.Ldarg_0);
                            getterIl.Emit(OpCodes.Ldfld, fieldBuilder);
                            getterIl.Emit(OpCodes.Ret);

                            propBuilder.SetGetMethod(getterBuilder);
                        }

                        //设值方法
                        if (field.HasSetter)
                        {
                            var setterBuilder = targetType.DefineMethod($"set_{propName}",
                                field.PublicSetter
                                    ? MethodAttributes.Public | MethodAttributes.Virtual
                                    : MethodAttributes.Assembly,
                                null,
                                new[] { filedType });
                            var setterIl = setterBuilder.GetILGenerator();
                            setterIl.Emit(OpCodes.Ldarg_0);
                            setterIl.Emit(OpCodes.Ldarg_1);
                            setterIl.Emit(OpCodes.Stfld, fieldBuilder);
                            setterIl.Emit(OpCodes.Ret);

                            propBuilder.SetSetMethod(setterBuilder);
                        }

                        if (field.CreateConstructorParameter)
                            construstorParameter.Add(field.GetPropertyName(), fieldBuilder);
                    }
                }

                if (fields.Any(p => p.CreateConstructorParameter))
                {
                    var createParameter = fields.Where(p => p.CreateConstructorParameter).ToArray();
                    var types = createParameter.Select(p => p.Type).ToArray();
                    //在构造函数内创建参数
                    var ctorBuilder = targetType.DefineConstructor(MethodAttributes.Public,
                        CallingConventions.Standard,
                        types);
                    for (var j = 0; j < createParameter.Length; j++)
                        ctorBuilder.DefineParameter(j + 1, ParameterAttributes.None,
                            createParameter[j].GetPropertyName());

                    var ctorIl = ctorBuilder.GetILGenerator();

                    for (var j = 0; j < createParameter.Length; j++)
                    {
                        ctorIl.Emit(OpCodes.Ldarg_0);
                        ctorIl.Emit(OpCodes.Ldarg, j + 1);
                        var viewAttrField = construstorParameter[createParameter[j].GetPropertyName()];
                        ctorIl.Emit(OpCodes.Stfld, viewAttrField);
                    }

                    ctorIl.Emit(OpCodes.Ret);
                }
            }

            var typeInfo = targetType.CreateTypeInfo();

            if (typeInfo == null) throw new InvalidOperationException($"无法创建隐式关联型{name}类型");
            //构造创建类型方法
            return typeInfo.AsType();
        }
    }
}