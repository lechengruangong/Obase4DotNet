/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：一种外键保证机制的具体实现,使用派生类型定义缺失的外键属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:51:05
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     提供一种外键保证机制的具体实现，该机制使用派生类型定义缺失的外键属性。
    ///     说明
    ///     定义属性的方式是定义一个派生类型，将所需属性定义在该派生类型上，并将该类型作为代理类型（ProxyType）。
    ///     警告
    ///     如果某一类型缺少外键属性，但其代理类型已存在，将引发ForeignKeyGuarantingException异常（代理类型已存在，无法通过定义派生类的方式追加
    ///     定义外键属性）。
    ///     实施说明
    ///     为每一属性（Attribute）定义一个公有字段，字段名称为属性名。调用ImpliedTypeManager.ApplyType(baseType,
    ///     fields)，其中：
    ///     （1）baseType的实参为objType.ClrType；
    ///     （2）fields的实参依据要定义的外鍵属性生成。
    ///     为每一属性（Attribute）设置取值器和设置器，使用委托取/设值器。委托可基于访问上述字段的MemberExpression生成。
    ///     将生成的代理类型赋予ObjectType的ProxyType属性。
    ///     更改类型的构造器，以确保反持久化过程中创建派生类型的实例。更改构造器的方法请参见顺序图“Odm.Builder/生成模型”。
    /// </summary>
    public class DerivingBasedForeignKeyGuarantor : ForeignKeyGuarantor
    {
        /// <summary>
        ///     在外键属性缺失的情况下定义所缺的属性。
        /// </summary>
        /// <param name="attrs">要定义的外键属性。</param>
        /// <param name="objType">要定义属性的类型。</param>
        protected override void DefineMissing(Attribute[] attrs, ObjectType objType)
        {
            //字段们
            var fileds = attrs.Select(attribute => new FieldDescriptor(attribute.DataType, attribute.Name)
                { HasGetter = true, HasSetter = true }).ToArray();

            //定义隐含类型
            var proxyType = ImpliedTypeManager.Current.ApplyType(objType.ClrType, fileds.ToArray());

            //为每个字段弄一个设值器一个取值器
            foreach (var attribute in attrs)
            {
                //获取定义的属性
                var property = proxyType.GetProperty(attribute.Name, BindingFlags.NonPublic | BindingFlags.Instance) ??
                               throw new ForeignKeyGuarantingException($"构造外键Key失败:无法取得字段{attribute.Name}");
                //用表达式编译
                var pe = Expression.Parameter(property.ReflectedType ??
                                              throw new InvalidOperationException("外键属性缺失:无所属类."));
                var funcType =
                    typeof(Func<,>).MakeGenericType(property.ReflectedType, property.PropertyType);
                var member = Expression.Property(pe, property);
                //构造取值表达式
                var exp = Expression.Lambda(funcType, member, pe);
                //用表达式编译结果构造委托设值器
                var getter = typeof(DelegateValueGetter<,>).MakeGenericType(proxyType, property.PropertyType);
                var getterObj = Activator.CreateInstance(getter, exp.Compile()) as IValueGetter;
                //取值器
                attribute.ValueGetter = getterObj;

                //获取属性的设值方法
                var parType = property.SetMethod.GetParameters()[0].ParameterType;
                //构造委托
                var actionType = typeof(Action<,>).MakeGenericType(property.DeclaringType, parType);
                var @delegate = property.SetMethod.CreateDelegate(actionType);
                //设值器类型
                var model = EValueSettingMode.Assignment;
                if (parType != typeof(string) && parType.GetInterface("IEnumerable") != null)
                    model = EValueSettingMode.Appending;

                //构造FieldValueSetter
                var setter = ValueSetter.Create(@delegate, model);
                attribute.ValueSetter = setter;
            }

            //参数
            var paraObjs = objType.Constructor.Parameters;
            //构造信息
            var ctorInfo = paraObjs == null
                ? proxyType.GetConstructor(Type.EmptyTypes)
                : proxyType.GetConstructor(paraObjs?.Select(p => p.GetType()).ToArray());
            //构造函数参数表达式
            var paraExps = paraObjs
                ?.Select(paraObj => Expression.Parameter(paraObj.GetType(), paraObj.Name))
                .ToArray();

            //构造表达式
            var body = Expression.New(
                ctorInfo ?? throw new InvalidOperationException($"建模错误:{objType.FullName}没有可用的构造函数"));
            var lambda = Expression.Lambda(body, paraExps);
            //创建委托
            var ctorDelegate = lambda.Compile();
            //构造器
            var ctorObj = InstanceConstructor.Create(ctorDelegate);

            //设置代理类型
            objType.ProxyType = proxyType;
            //设置构造函数
            objType.Constructor = ctorObj;
        }
    }
}