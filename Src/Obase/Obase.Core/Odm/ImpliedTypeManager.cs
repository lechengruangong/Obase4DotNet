/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：隐含类型管理器,管理代理类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:07:26
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     隐含类型管理器，负责创建、维护隐含类型，并提供对这些类型的访问入口。
    ///     隐含类型是指非由应用程序开发人员定义，而由Obase基于实现某些功能需要自行定义的类型。通常情况下，这些类型对开发人员是不可见的。
    /// </summary>
    public class ImpliedTypeManager
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     接受管理的隐含类型。
        /// </summary>
        private readonly Dictionary<IdentityArray, Type> _impliedTypes = new Dictionary<IdentityArray, Type>();

        /// <summary>
        ///     作为隐含类型宿主的模块，提供在其中定义类型的方法。
        /// </summary>
        private readonly ModuleBuilder _module = Utils.InitModuleBuilder();

        /// <summary>
        ///     命名计数器，用于在命名过程中累加计数，避免命名重复。
        /// </summary>
        private int _namingCounter;

        /// <summary>
        ///     创建ImpliedTypeManager实例。
        /// </summary>
        private ImpliedTypeManager()
        {
        }

        /// <summary>
        ///     获取当前应用程序域中唯一的隐含类型管理器实例。
        ///     实施说明
        ///     需要解决并发冲突，保证在并发场景下实现全局唯一性。
        /// </summary>
        public static ImpliedTypeManager Current { get; } = new ImpliedTypeManager();

        /// <summary>
        ///     获取隐含类型。
        /// </summary>
        /// <param name="identity">要获取类型的标识。</param>
        public Type GetType(IdentityArray identity)
        {
            //尝试从已定义的隐含类型中获取
            if (_impliedTypes.TryGetValue(identity, out var type))
                return type;

            return null;
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型定义且只定义了指定的字段。
        ///     实施说明
        ///     以各字段的文本化说明构成的序列作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(FieldDescriptor[] fields, Action<TypeBuilder> defineMembers = null)
        {
            //字段转化为文本
            var filedTexts = fields.Select(p => (object)p.ToString()).ToArray();
            //构造标识数组
            var identityArray = new IdentityArray(filedTexts);

            return SearchOrDefineType(identityArray, typeof(object), null, fields, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型定义且只定义了指定的字段，如果这样的类型有多个则以指定的子标识进一步识别。
        ///     实施说明
        ///     以各字段的文本化说明构成的序列，联合指定的子标识，作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="subIdentity">子标识。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(FieldDescriptor[] fields, IdentityArray subIdentity,
            Action<TypeBuilder> defineMembers = null)
        {
            //字段转化为文本
            var filedTexts = fields.Select(p => (object)p.ToString()).ToArray();
            //构造标识数组 并加入子标识
            var identityArray = new IdentityArray(filedTexts);
            identityArray.Append(subIdentity);

            return SearchOrDefineType(identityArray, typeof(object), null, fields, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型。
        ///     实施说明
        ///     以基类型的完全限定名作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="defineMembers">用于定义类型成员的委托。</param>
        public Type ApplyType(Type baseType, Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名作为标识
            var identityArray = new IdentityArray(baseType.FullName);

            return SearchOrDefineType(identityArray, baseType, null, null, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型，如果这样的类型有多个则以指定的子标识进一步识别。
        ///     实施说明
        ///     以基类型的完全限定名联合指定的子标识作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="subIdentity">子标识。</param>
        /// <param name="defineMembers">用于定义类型成员的委托。</param>
        public Type ApplyType(Type baseType, IdentityArray subIdentity, Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名联合子标识作为标识 并加入子标识
            var identityArray = new IdentityArray(baseType.FullName);
            identityArray.Append(subIdentity);

            return SearchOrDefineType(identityArray, baseType, null, null, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型，定义且只定义了指定的字段。
        ///     实施说明
        ///     以基类型的完全限定名联合各字段的文本化说明构成的序列，作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(Type baseType, FieldDescriptor[] fields, Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名联合子标识作为标识
            var identityArray = new IdentityArray(baseType.FullName);

            return SearchOrDefineType(identityArray, baseType, null, fields, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型，定义且只定义了指定的字段，如果这样的类型有多个则以指定的子标识进一步识别。
        ///     实施说明
        ///     以基类型的完全限定名联合各字段的文本化说明构成的序列，再联合子标识，作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="subIdentity">子标识。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(Type baseType, FieldDescriptor[] fields, IdentityArray subIdentity,
            Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名联合子标识作为标识 并加入子标识
            var identityArray = new IdentityArray(baseType.FullName);
            identityArray.Append(subIdentity);

            return SearchOrDefineType(identityArray, baseType, null, fields, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型实现指定的接口，定义且只定义了指定的字段。
        ///     实施说明
        ///     以各接口的完全限定名构成的序列，联合各字段的文本化说明构成的序列作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="interfaces">类型实现的接口。</param>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(Type[] interfaces, FieldDescriptor[] fields, Action<TypeBuilder> defineMembers = null)
        {
            //以各接口的完全限定名构成的序列
            var interfacesTexts = interfaces.Select(p => (object)p.FullName).ToArray();
            //完全限定名构成的序列
            var identityArray = new IdentityArray(interfacesTexts);

            return SearchOrDefineType(identityArray, typeof(object), interfaces, fields, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型实现指定的接口，定义且只定义了指定的字段，如果这样的类型有多个则以指定的子标识进一步识别。
        ///     实施说明
        ///     以各接口的完全限定名构成的序列，联合各字段的文本化说明构成的序列，再联合指定的子标识，作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="interfaces">类型实现的接口。</param>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="subIdentity">子标识。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(Type[] interfaces, FieldDescriptor[] fields, IdentityArray subIdentity,
            Action<TypeBuilder> defineMembers = null)
        {
            //以各接口的完全限定名构成的序列
            var interfacesTexts = interfaces.Select(p => (object)p.FullName).ToArray();
            //完全限定名构成的序列 并加入子标识
            var identityArray = new IdentityArray(interfacesTexts);
            identityArray.Append(subIdentity);

            return SearchOrDefineType(identityArray, typeof(object), interfaces, fields, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型并实现指定的接口。
        ///     实施说明
        ///     以基类型的完全限定名联合各接口的完全限定名构成的序列作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="interfaces">类型实现的接口。</param>
        /// <param name="defineMembers">用于定义类型成员的委托。</param>
        public Type ApplyType(Type baseType, Type[] interfaces, Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名联合各接口的完全限定名构成的序列
            var identityArray = new IdentityArray(baseType.FullName);
            //再加入接口完全限定名构成的序列
            var interfacesTexts = interfaces.Select(p => (object)p.FullName).ToArray();
            identityArray.Append(interfacesTexts);

            return SearchOrDefineType(identityArray, baseType, interfaces, null, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型并实现指定的接口，如果这样的类型有多个则以指定的子标识进一步识别。
        ///     实施说明
        ///     以基类型的完全限定名联合各接口的完全限定名构成的序列，再联合指定的子标识作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="interfaces">类型实现的接口。</param>
        /// <param name="subIdentity">子标识。</param>
        /// <param name="defineMembers">用于定义类型成员的委托。</param>
        public Type ApplyType(Type baseType, Type[] interfaces, IdentityArray subIdentity,
            Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名联合各接口的完全限定名构成的序列
            var identityArray = new IdentityArray(baseType.FullName);
            var interfacesTexts = interfaces.Select(p => (object)p.FullName).ToArray();
            identityArray.Append(interfacesTexts);
            identityArray.Append(subIdentity);

            return SearchOrDefineType(identityArray, baseType, interfaces, null, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型，实现指定的接口，定义且只定义了指定的字段。
        ///     实施说明
        ///     以基类型的完全限定名联合各接口的完全限定名构成的序列，再联合各字段的文本化说明构成的序列，作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="interfaces">类型实现的接口。</param>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(Type baseType, Type[] interfaces, FieldDescriptor[] fields,
            Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名联合各字段的文本化构成的序列
            var identityArray = new IdentityArray(baseType.FullName);
            var interfacesTexts = interfaces.Select(p => (object)p.FullName).ToArray();
            var filedTexts = fields.Select(p => (object)p.ToString()).ToArray();

            identityArray.Append(interfacesTexts);
            identityArray.Append(filedTexts);

            return SearchOrDefineType(identityArray, baseType, interfaces, fields, defineMembers);
        }

        /// <summary>
        ///     向隐含类型管理器申请一个类型，该类型派生自指定的基类型，实现指定的接口，定义且只定义了指定的字段，如果这样的类型有多个则以指定的子标识进一步识别。
        ///     实施说明
        ///     以基类型的完全限定名联合各接口的完全限定名构成的序列，再联合各字段的文本化说明构成的序列，再联合子标识，作为类型的标识。
        ///     如果满足条件的类型不存在，管理器将自动创建一个新类型。
        /// </summary>
        /// <returns>符合条件的隐含类型。</returns>
        /// <param name="baseType">类型的基类。</param>
        /// <param name="interfaces">类型实现的接口。</param>
        /// <param name="fields">类型应当且只能定义的字段。</param>
        /// <param name="subIdentity">子标识。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        public Type ApplyType(Type baseType, Type[] interfaces, FieldDescriptor[] fields, IdentityArray subIdentity,
            Action<TypeBuilder> defineMembers = null)
        {
            //以基类型的完全限定名联合接口完全限定名,各字段的文本化构成的序列 并加入子标识
            var identityArray = new IdentityArray(baseType.FullName);
            var interfacesTexts = interfaces.Select(p => (object)p.FullName).ToArray();
            var filedTexts = fields.Select(p => (object)p.ToString()).ToArray();

            identityArray.Append(interfacesTexts);
            identityArray.Append(filedTexts);
            identityArray.Append(subIdentity);

            return SearchOrDefineType(identityArray, baseType, interfaces, fields, defineMembers);
        }

        /// <summary>
        ///     以指定的标识搜索隐含类型，如果未找到则根据指定的内容创建类型。
        /// </summary>
        /// <param name="identity">要搜索的类型的标识。</param>
        /// <param name="baseType">要定义的类型的基类。</param>
        /// <param name="interfaces">实现的接口</param>
        /// <param name="fields">要定义的字段。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        private Type SearchOrDefineType(IdentityArray identity, Type baseType, Type[] interfaces,
            FieldDescriptor[] fields, Action<TypeBuilder> defineMembers = null)
        {
            //查找是否已定义
            var existType = GetType(identity);
            if (existType != null)
                return existType;

            //命名
            var name = $"{baseType.Name}<>Obase<>ImpliedType^{++_namingCounter}";

            ReaderWriterLock.EnterWriteLock();
            //再次查找
            existType = GetType(identity);
            if (existType != null)
                return existType;
            //定义一个新类型
            var type = DefineType(name, interfaces, baseType, fields, defineMembers);
            _impliedTypes.Add(identity, type);
            ReaderWriterLock.ExitWriteLock();
            return type;
        }

        /// <summary>
        ///     根据指定的内容定义隐含类型。
        /// </summary>
        /// <param name="name">类型名称。</param>
        /// <param name="baseType">基类型。</param>
        /// <param name="fields">类型实现的接口。</param>
        /// <param name="defineMembers">一个委托，用于定义类型的成员。</param>
        /// <param name="interfaces">类型实现的接口。</param>
        private Type DefineType(string name, Type[] interfaces, Type baseType = null, FieldDescriptor[] fields = null,
            Action<TypeBuilder> defineMembers = null)
        {
            //目标类型
            var targetType =
                _module.DefineType(name, TypeAttributes.Public | TypeAttributes.Sealed, baseType, interfaces);

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

                        //如果创建构造函数参数
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
                    //写一个方法体
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

            //额外的处理
            defineMembers?.Invoke(targetType);
            var typeInfo = targetType.CreateTypeInfo();

            if (typeInfo == null) throw new InvalidOperationException($"无法创建{baseType}的隐含类型");
            //构造创建类型方法
            return typeInfo.AsType();
        }
    }
}